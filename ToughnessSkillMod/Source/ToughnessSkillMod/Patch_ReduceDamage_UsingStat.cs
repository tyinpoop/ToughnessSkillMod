using HarmonyLib;
using RimWorld;
using Verse;
using System;

namespace ToughnessSkillMod
{
    [HarmonyPatch(typeof(DamageWorker_AddInjury))]
    [HarmonyPatch("ApplySpecialEffectsToPart")]
    [HarmonyPatch(new Type[] { typeof(Pawn), typeof(float), typeof(DamageInfo), typeof(DamageWorker.DamageResult) })]
    public static class Patch_ReduceDamage_UsingStat
    {
        [HarmonyPrefix]
        public static void Prefix(Pawn pawn, ref float totalDamage, DamageInfo dinfo, DamageWorker.DamageResult result)
        {
            if (pawn == null) return;

            float factor = pawn.GetStatValue(StatDefOf.IncomingDamageFactor, true);

            // Modify damage factor based on skill level
            var skill = pawn.skills.GetSkill(DefDatabase<SkillDef>.GetNamed("Toughness", false));
            if (skill != null)
            {
                if (skill.Level <= 5)
                {
                    factor *= 1f + (0.075f * (5 - skill.Level)); // Increase damage for low levels
                }
                else
                {
                    factor *= 1f - (0.5f * (skill.Level - 5) / 15f); // Reduce damage at higher levels
                }
            }

            totalDamage *= factor;
        }
    }
}
