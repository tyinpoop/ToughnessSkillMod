using HarmonyLib;
using Verse;
using RimWorld;

namespace ToughnessSkillMod
{
    [HarmonyPatch(typeof(Hediff_Injury))]
    [HarmonyPatch("get_BleedRate")]
    public static class Patch_BleedRateByToughness
    {
        [HarmonyPostfix]
        public static void Postfix(Hediff_Injury __instance, ref float __result)
        {
            Pawn pawn = __instance?.pawn;
            if (pawn?.skills == null) return;

            var toughnessSkill = pawn.skills.GetSkill(DefDatabase<SkillDef>.GetNamed("Toughness", false));
            if (toughnessSkill != null)
            {
                float toughnessFactor;

                if (toughnessSkill.Level <= 5)
                {
                    toughnessFactor = 1f + (0.075f * (5 - toughnessSkill.Level)); // Up to 2.5x bleed rate at level 0
                }
                else
                {
                    toughnessFactor = 1f - (0.5f * (toughnessSkill.Level - 5) / 15f); // Reduce bleeding at higher levels
                }

                __result *= toughnessFactor;
            }
        }
    }
}