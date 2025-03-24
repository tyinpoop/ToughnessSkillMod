using HarmonyLib;
using RimWorld;
using Verse;
using System.Linq;
using System;

namespace ToughnessSkillMod
{
    [HarmonyPatch(typeof(DamageWorker_AddInjury))]
    [HarmonyPatch("ApplyDamageToPart")]
    [HarmonyPatch(new Type[] { typeof(DamageInfo), typeof(Pawn), typeof(DamageWorker.DamageResult) })]
    public static class Patch_GainToughnessXP
    {
        [HarmonyPostfix]
        public static void Postfix(DamageInfo dinfo, Pawn pawn, DamageWorker.DamageResult result)
        {
            if (pawn?.skills == null || result.totalDamageDealt <= 0f) return;

            var comp = pawn.TryGetComp<CompToughnessCache>();
            comp?.NotifyDamageApplied(dinfo, result.totalDamageDealt, pawn);
        }
    }
}
