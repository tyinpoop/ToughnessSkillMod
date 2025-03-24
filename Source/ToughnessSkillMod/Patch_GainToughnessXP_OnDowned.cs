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
            if (pawn == null) return;

            var comp = pawn.TryGetComp<CompToughnessCache>();
            comp?.NotifyDowned(dinfo, hediff, pawn);
        }
    }
}
