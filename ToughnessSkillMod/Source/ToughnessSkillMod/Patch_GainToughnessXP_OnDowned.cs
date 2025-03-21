using HarmonyLib;
using RimWorld;
using Verse;
using System;
using System.Linq;

namespace ToughnessSkillMod
{
    [HarmonyPatch(typeof(Pawn_HealthTracker))]
    [HarmonyPatch("MakeDowned")]
    public static class Patch_GainToughnessXP_OnDowned
    {
        [HarmonyPostfix]
        public static void Postfix(Pawn_HealthTracker __instance, DamageInfo? dinfo, Hediff hediff)
        {
            Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>(); // Access private pawn field safely
            if (pawn == null || pawn.skills == null || pawn.Dead) return;

            var skill = pawn.skills.GetSkill(DefDatabase<SkillDef>.GetNamed("Toughness", false));
            if (skill == null || skill.TotallyDisabled) return;

            // **Check if the downing was combat-related**
            bool wasCombatRelated = false;

            if (dinfo.HasValue)
            {
                DamageDef damageType = dinfo.Value.Def;
                if (damageType == DamageDefOf.Cut || damageType == DamageDefOf.Blunt || damageType == DamageDefOf.Stab ||
                    damageType == DamageDefOf.Bullet || damageType == DamageDefOf.Bomb)
                {
                    wasCombatRelated = true;
                }
            }

            // **Check if the pawn has combat-related injuries**
            if (!wasCombatRelated)
            {
                if (pawn.health?.hediffSet?.hediffs != null)
                {
                    foreach (var injury in pawn.health.hediffSet.hediffs.OfType<Hediff_Injury>())
                    {
                        if (injury.IsTended() || injury.Severity <= 0.01f) continue;
                        wasCombatRelated = true;
                        break;
                    }
                }
            }

            // **Prevent abuse - Don't give XP for disease, infections, or food poisoning**
            if (!wasCombatRelated || hediff.def == HediffDefOf.FoodPoisoning || hediff.def == HediffDefOf.WoundInfection || hediff.def == HediffDefOf.Plague)
            {
                return;
            }

            // **XP Scaling Based on Injuries**
            float downedXP = 750f; // Base XP for downing in combat
            float xpMultiplier = 1f;
            int injuryCount = 0;
            float totalSeverity = 0f;

            if (pawn.health?.hediffSet?.hediffs != null)
            {
                foreach (var injury in pawn.health.hediffSet.hediffs.OfType<Hediff_Injury>())
                {
                    if (injury.IsTended() || injury.Severity <= 0.01f) continue;
                    injuryCount++;
                    totalSeverity += injury.Severity;
                }
            }

            if (injuryCount > 0)
            {
                xpMultiplier += 0.05f * injuryCount;
            }
            if (totalSeverity > 5) xpMultiplier *= 1.2f;
            if (totalSeverity > 10) xpMultiplier *= 1.5f;

            // **Apply XP**
            float finalXP = downedXP * xpMultiplier;
            skill.Learn(finalXP);

            // **Debug Info**
            if (Prefs.DevMode)
            {
                Log.Message($"[Toughness] {pawn.Name} downed in combat! Awarding {finalXP} XP.");
            }
        }
    }
}
