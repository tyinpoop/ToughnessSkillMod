using RimWorld;
using Verse;

namespace ToughnessSkillMod
{
    public class StatPart_ToughnessDamage : StatPart
    {
        public override void TransformValue(StatRequest req, ref float val)
        {
            if (!req.HasThing) return;
            Pawn pawn = req.Thing as Pawn;
            if (pawn?.skills == null) return;

            var skill = pawn.skills.GetSkill(DefDatabase<SkillDef>.GetNamed("Toughness", false));
            if (skill == null) return;

            // New formula: Increase damage at lower skill levels, reduce at higher levels
            float factor;
            if (skill.Level <= 5)
            {
                factor = 1f + (0.075f * (5 - skill.Level)); // Takes up to 2x damage at level 0
            }
            else
            {
                factor = 1f - (0.5f * (skill.Level - 5) / 15f); // Reduces damage from level 6-20
            }

            val *= factor;
        }

        public override string ExplanationPart(StatRequest req)
        {
            if (!req.HasThing) return null;
            Pawn pawn = req.Thing as Pawn;
            if (pawn?.skills == null) return null;

            var skill = pawn.skills.GetSkill(DefDatabase<SkillDef>.GetNamed("Toughness", false));
            if (skill == null) return null;

            float factor;
            if (skill.Level <= 5)
            {
                factor = 1f + (0.075f * (5 - skill.Level)); // Takes up to 2x damage at level 0
            }
            else
            {
                factor = 1f - (0.5f * (skill.Level - 5) / 15f); // Reduces damage from level 6-20
            }

            return $"Toughness skill: x{factor.ToStringPercent()} damage taken";
        }
    }
}