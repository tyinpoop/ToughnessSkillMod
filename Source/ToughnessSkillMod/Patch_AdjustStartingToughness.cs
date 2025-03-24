using HarmonyLib;
using RimWorld;
using Verse;
using System;
using System.Linq;

namespace ToughnessSkillMod
{
    [HarmonyPatch(typeof(PawnGenerator))]
    [HarmonyPatch("GenerateSkills")]
    public static class Patch_AdjustStartingToughness
    {
        // **Keywords for toughness scaling**
        private static readonly string[] PositiveToughnessKeywords = 
        {
            "soldier", "mercenary", "gladiator", "brawler", "miner", "scout", "gang",
            "marine", "tribal", "warrior", "hunter", "bodyguard", "wastelander", "leader", "war"
        };

        private static readonly string[] NegativeToughnessKeywords = 
        {
            "noble", "artist", "scholar", "scientist", "clerk", "engineer", "priest",
            "aristocrat", "diplomat", "actor", "academic", "researcher"
        };

        [HarmonyPostfix]
        public static void Postfix(Pawn pawn)
        {
            if (pawn == null || pawn.skills == null || !pawn.RaceProps.Humanlike) return; // Only for humanlike pawns

            var toughnessSkill = pawn.skills.GetSkill(DefDatabase<SkillDef>.GetNamed("Toughness", false));
            if (toughnessSkill == null || toughnessSkill.TotallyDisabled) return;

            // **Base Toughness Level**
            int baseToughness = Rand.RangeInclusive(1, 6); 

            // **Adjustments Based on Backstory**
            baseToughness = AdjustToughnessByBackstory(pawn, baseToughness);

            // **Apply the new level**
            toughnessSkill.Level = baseToughness;
            toughnessSkill.xpSinceLastLevel = 0; 

            // **Debug Info**
            if (Prefs.DevMode)
            {
                Log.Message($"[Toughness] {pawn.Name} generated with Toughness {baseToughness}.");
            }
        }

        public static int AdjustToughnessByBackstory(Pawn pawn, int baseToughness)
        {
            if (pawn.story != null)
            {
                string childhood = pawn.story.Childhood?.defName.ToLower() ?? "";
                string adulthood = pawn.story.Adulthood?.defName.ToLower() ?? "";
                string childhoodTitle = pawn.story.Childhood?.title.ToLower() ?? "";
                string adulthoodTitle = pawn.story.Adulthood?.title.ToLower() ?? "";

                // **Check for Positive Toughness keywords**
                if (PositiveToughnessKeywords.Any(keyword => childhood.Contains(keyword) || adulthood.Contains(keyword) ||
                                                            childhoodTitle.Contains(keyword) || adulthoodTitle.Contains(keyword)))
                {
                    baseToughness += 2;
                }

                // **Check for Negative Toughness keywords**
                if (NegativeToughnessKeywords.Any(keyword => childhood.Contains(keyword) || adulthood.Contains(keyword) ||
                                                            childhoodTitle.Contains(keyword) || adulthoodTitle.Contains(keyword)))
                {
                    baseToughness -= 2;
                }
            }
            return Math.Max(0, baseToughness); // Prevent negative toughness
        }

    }
}
