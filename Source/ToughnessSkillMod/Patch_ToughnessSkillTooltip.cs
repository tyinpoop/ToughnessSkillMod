using HarmonyLib;
using RimWorld;
using Verse;

namespace ToughnessSkillMod
{
    [HarmonyPatch(typeof(SkillUI), "GetSkillDescription")]
    public static class Patch_ToughnessSkillTooltip
    {
        [HarmonyPostfix]
        public static void Postfix(SkillRecord sk, ref string __result)
        {
            if (sk.def.defName != "Toughness") return; // Only modify the Toughness skill tooltip

            Pawn pawn = sk.Pawn;
            if (pawn == null) return;

            int skillLevel = sk.Level;

            // Calculate Damage Factor
            float damageFactor;
            if (skillLevel <= 5)
            {
                damageFactor = 1f + (0.075f * (5 - skillLevel));
            }
            else
            {
                damageFactor = 1f - (0.5f * (skillLevel - 5) / 15f);
            }

            // Calculate Bleed Factor
            float bleedFactor;
            if (skillLevel <= 5)
            {
                bleedFactor = 1f + (0.075f * (5 - skillLevel));
            }
            else
            {
                bleedFactor = 1f - (0.5f * (skillLevel - 5) / 15f);
            }

            // Append dynamic values to the skill description
            __result += $"\n\n<b>Current Effects:</b>\n" +
                        $"• Incoming Damage: x{damageFactor:0.00}\n" +
                        $"• Bleed Rate: x{bleedFactor:0.00}";
        }
    }
}
