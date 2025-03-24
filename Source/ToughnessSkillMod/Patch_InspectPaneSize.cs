using HarmonyLib;
using UnityEngine;
using RimWorld;
using Verse;
namespace ToughnessSkillMod
{
    [StaticConstructorOnStartup]
    public static class PatchPawnCardSize
    {
        static PatchPawnCardSize()
        {
            new Harmony("tyinplop.ToughnessSkillMod")
                .Patch(
                    AccessTools.Method(typeof(CharacterCardUtility), nameof(CharacterCardUtility.PawnCardSize)),
                    postfix: new HarmonyMethod(typeof(PatchPawnCardSize), nameof(AddOneRow))
                );
        }

        public static void AddOneRow(ref Vector2 __result)
        {
            if (__result.y >= 392f) __result.y += 24f;
        }
    }
}
