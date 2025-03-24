using RimWorld;
using Verse;
using System.Text;

namespace ToughnessSkillMod
{
    public class StatPart_ToughnessDamage : StatPart
    {
        public override void TransformValue(StatRequest req, ref float val)
        {
            if (!req.HasThing || !(req.Thing is Pawn pawn)) return;

            var comp = pawn.TryGetComp<CompToughnessCache>();
            if (comp == null) return;

            comp.UpdateCache(pawn);
            val *= comp.cachedFactor;
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

//************************************************************************************************************************************************************
    public class StatPart_FinalBleedRate : StatPart
    {
        public override void TransformValue(StatRequest req, ref float val)
        {
            if (!req.HasThing || !(req.Thing is Pawn pawn)) return;

            float finalFactor = 1f;

            // 1) Multiply in totalBleedFactor from *all* Hediffs that have it
            var hediffs = pawn.health?.hediffSet?.hediffs;
            if (hediffs != null)
            {
                foreach (var hediff in hediffs)
                {
                    var stage = hediff?.CurStage;
                    if (stage != null && stage.totalBleedFactor != 1f)
                    {
                        finalFactor *= stage.totalBleedFactor;
                    }
                }
            }

            // 2) Apply your Toughness skill factor using the correct scaling
            if (pawn.skills != null)
            {
                var toughnessSkill = pawn.skills.GetSkill(DefDatabase<SkillDef>.GetNamed("Toughness", false));
                if (toughnessSkill != null)
                {
                    int s = toughnessSkill.Level;
                    float toughnessFactor;

                    if (s <= 5)
                    {
                        toughnessFactor = 1f + (0.075f * (5 - s));  // Increases bleed rate when skill is low
                    }
                    else
                    {
                        toughnessFactor = 1f - (0.5f * (s - 5) / 15f); // Decreases bleed rate when skill is high
                    }

                    finalFactor *= toughnessFactor;
                }
            }

            // 3) Multiply the stat’s base value (1.0) by the final factor
            val *= finalFactor;
        }

        public override string ExplanationPart(StatRequest req)
        {
            if (!req.HasThing || !(req.Thing is Pawn pawn)) return null;

            StringBuilder sb = new StringBuilder();
            float total = 1f;

            // -- HEDIFF FACTORS --
            var hediffs = pawn.health?.hediffSet?.hediffs;
            if (hediffs != null)
            {
                foreach (var hediff in hediffs)
                {
                    var stage = hediff?.CurStage;
                    if (stage != null && stage.totalBleedFactor != 1f)
                    {
                        total *= stage.totalBleedFactor;
                        sb.AppendLine($"{hediff.def?.label ?? hediff.Label}: x{stage.totalBleedFactor.ToStringPercent()}");
                    }
                }
            }

            // -- TOUGHNESS FACTOR --
            float skillFactor = 1f;
            if (pawn.skills != null)
            {
                var toughnessSkill = pawn.skills.GetSkill(DefDatabase<SkillDef>.GetNamed("Toughness", false));
                if (toughnessSkill != null)
                {
                    int s = toughnessSkill.Level;
                    
                    if (s <= 5)
                    {
                        skillFactor = 1f + (0.075f * (5 - s));
                    }
                    else
                    {
                        skillFactor = 1f - (0.5f * (s - 5) / 15f);
                    }

                    if (skillFactor != 1f)
                    {
                        sb.AppendLine($"Toughness skill: x{skillFactor.ToStringPercent()}");
                        total *= skillFactor;
                    }
                }
            }

            if (total != 1f)
            {
                sb.AppendLine($"Final: x{total.ToStringPercent()}");
            }

            return (total != 1f) ? sb.ToString().Trim() : null;
        }
    }
}