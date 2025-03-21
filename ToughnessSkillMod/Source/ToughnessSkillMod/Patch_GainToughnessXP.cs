using HarmonyLib;
using RimWorld;
using Verse;
using System;
using System.Linq;

namespace ToughnessSkillMod
{
    [HarmonyPatch(typeof(DamageWorker_AddInjury))]
    [HarmonyPatch("ApplyDamageToPart")]
    [HarmonyPatch(new Type[] { typeof(DamageInfo), typeof(Pawn), typeof(DamageWorker.DamageResult) })]
    public static class Patch_GainToughnessXP
    {
        [HarmonyPrefix]
        public static void Prefix(DamageInfo dinfo, Pawn pawn, DamageWorker.DamageResult result)
        {
            if (pawn?.skills == null || dinfo.Amount <= 0) return;

            var skill = pawn.skills.GetSkill(DefDatabase<SkillDef>.GetNamed("Toughness", false));
            if (skill == null || skill.TotallyDisabled) return;

            float baseXP = dinfo.Amount * 50f; // Base XP per damage point
            float xpMultiplier = 1f;

            // Count only **active, negative hediffs**
            int negativeHediffCount = 0;
            float severityFactor = 0f;

            if (pawn.health?.hediffSet?.hediffs != null)
            {
                foreach (var hediff in pawn.health.hediffSet.hediffs)
                {
                    if (hediff is Hediff_Injury injury)
                    {
                        if (injury.IsTended() || injury.Severity <= 0.01f) continue; // Skip healed/tended injuries
                    }
                    else if (hediff is Hediff_MissingPart missingPart)
                    {
                        if (pawn.health.hediffSet.PartIsMissing(missingPart.Part))
                        {
                            // Only count missing parts if not replaced by an implant
                            if (pawn.health.hediffSet.hediffs.Any(h => h.Part == missingPart.Part && h.def.spawnThingOnRemoved != null))
                                continue;
                        }
                    }
                    else if (hediff.IsPermanent()) continue; // Ignore old scars

                    // Count valid hediffs
                    negativeHediffCount++;
                    severityFactor += hediff.Severity;
                }
            }

            // Scale XP based on number of **current** negative hediffs
            if (negativeHediffCount > 0)
            {
                xpMultiplier += 0.05f * negativeHediffCount; // Each negative hediff increases XP by 5%
            }

            // Apply severity-based XP scaling
            if (severityFactor > 5) xpMultiplier *= 1.2f; // Major injuries grant 20% extra XP
            if (severityFactor > 10) xpMultiplier *= 1.5f; // Extremely severe conditions give 50% extra XP

            // Apply type-based XP scaling (more XP for life-threatening hediffs)
            if (dinfo.Def == DamageDefOf.Burn || dinfo.Def == DamageDefOf.Frostbite)
            {
                xpMultiplier *= 1.25f; // Burns & frostbite grant 25% more XP
            }
            else if (dinfo.Def == DamageDefOf.Bullet || dinfo.Def == DamageDefOf.Cut)
            {
                xpMultiplier *= 1.1f; // Bullet wounds & cuts give 10% extra XP
            }

            // Calculate final XP
            float finalXP = baseXP * xpMultiplier;
            skill.Learn(finalXP);
        }
    }
}
