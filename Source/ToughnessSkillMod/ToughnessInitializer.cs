using RimWorld;
using Verse;
using System.Linq;

namespace ToughnessSkillMod
{
    public class ToughnessInitializer : GameComponent
    {
        private bool initialized;

        public ToughnessInitializer(Game game) { }

        public override void LoadedGame()
        {
            if (initialized) return;
            initialized = true;

            foreach (var pawn in PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive
                                           .Where(p => p.RaceProps.Humanlike && p.Faction == Faction.OfPlayer))
            {
                var skill = pawn.skills?.GetSkill(DefDatabase<SkillDef>.GetNamed("Toughness", false));
                if (skill == null || skill.Level > 0) continue;

                // Use your existing “adjust starting toughness” logic
                int baseToughness = Rand.RangeInclusive(2, 6);
                if (pawn.Faction.HostileTo(Faction.OfPlayer)) baseToughness = Rand.RangeInclusive(5, 10);
                baseToughness = Patch_AdjustStartingToughness.AdjustToughnessByBackstory(pawn, baseToughness);

                skill.Level = baseToughness;
                skill.xpSinceLastLevel = 0;
                if (Prefs.DevMode)
                    Log.Message($"[Toughness] Initialized {pawn.NameShortColored} → Toughness {baseToughness}");
            }
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref initialized, "initialized");
        }
    }
}
