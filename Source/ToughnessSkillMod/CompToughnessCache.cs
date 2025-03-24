using RimWorld; 
using Verse; 
using System.Linq;

namespace ToughnessSkillMod 
{ 
    public class CompToughnessCache : ThingComp 
    { 
        private int lastLevel = -1; public float cachedFactor = 1f;

        public void UpdateCache(Pawn pawn)
        {
            var skill = pawn.skills?.GetSkill(DefDatabase<SkillDef>.GetNamed("Toughness", false));
            if (skill == null) return;

            if (skill.Level != lastLevel)
            {
                lastLevel = skill.Level;
                if (lastLevel <= 5)
                    cachedFactor = 1f + 0.075f * (5 - lastLevel);
                else
                    cachedFactor = 1f - 0.5f * (lastLevel - 5) / 15f;
            }
        }

        
        // Handles XP gain based on the damage applied.
        public void NotifyDamageApplied(DamageInfo dinfo, float totalDamageDealt, Pawn pawn)
        {
            if (pawn?.skills == null || totalDamageDealt <= 0f) return;
            var skill = pawn.skills.GetSkill(DefDatabase<SkillDef>.GetNamed("Toughness", false));
            if (skill == null || skill.TotallyDisabled) return;

            float rawDamage = dinfo.Amount;
            float baseXP = totalDamageDealt * ToughnessSkillMod.baseXp.Value;
            float xpMultiplier = 1f;
            int negativeHediffCount = 0;
            float severityFactor = 0f;

            foreach (var injury in pawn.health?.hediffSet?.hediffs.OfType<Hediff_Injury>() ?? Enumerable.Empty<Hediff_Injury>())
            {
                if (injury.IsTended() || injury.Severity <= 0.01f)
                    continue;
                negativeHediffCount++;
                severityFactor += injury.Severity;
            }

            if (negativeHediffCount > 0)
                xpMultiplier += 0.05f * negativeHediffCount;
            if (severityFactor > 5) xpMultiplier *= 1.2f;
            if (severityFactor > 10) xpMultiplier *= 1.5f;

            if (dinfo.Def == DamageDefOf.Burn || dinfo.Def == DamageDefOf.Frostbite)
                xpMultiplier *= 1.25f;
            else if (dinfo.Def == DamageDefOf.Bullet || dinfo.Def == DamageDefOf.Cut)
                xpMultiplier *= 1.1f;

            float xpGained = baseXP * xpMultiplier;
            skill.Learn(xpGained);

            if (Prefs.DevMode)
            {
                float reduction = rawDamage - totalDamageDealt;
                Log.Message($"[ToughnessSkillMod] {pawn.NameShortColored} took {rawDamage:0.##} raw damage, reduced to {totalDamageDealt:0.##}. XP granted: {xpGained:0.##} (x{xpMultiplier:0.##}). Base XP: {ToughnessSkillMod.baseXp.Value:0.##}");
            }
        }


        public void NotifyDowned(DamageInfo? dinfo, Hediff hediff, Pawn pawn)
        {
            if (pawn?.skills == null || pawn.Dead) return;
            var skill = pawn.skills.GetSkill(DefDatabase<SkillDef>.GetNamed("Toughness", false));
            if (skill == null || skill.TotallyDisabled) return;

            // Check if the downing was combat-related.
            bool wasCombatRelated = false;
            if (dinfo.HasValue)
            {
                DamageDef damageType = dinfo.Value.Def;
                if (damageType == DamageDefOf.Cut || damageType == DamageDefOf.Blunt || damageType == DamageDefOf.Stab ||
                    damageType == DamageDefOf.Bullet || damageType == DamageDefOf.Bomb)
                {
                    wasCombatRelated = true;
                }
            }
            if (!wasCombatRelated && pawn.health?.hediffSet?.hediffs != null)
            {
                foreach (var injury in pawn.health.hediffSet.hediffs.OfType<Hediff_Injury>())
                {
                    if (injury.IsTended() || injury.Severity <= 0.01f) continue;
                    wasCombatRelated = true;
                    break;
                }
            }
            if (!wasCombatRelated ||
                hediff.def == HediffDefOf.FoodPoisoning ||
                hediff.def == HediffDefOf.WoundInfection ||
                hediff.def == HediffDefOf.Plague)
            {
                return;
            }

            // XP calculation specific for downed events.
            float downedXP = 600f;
            float xpMultiplier = 1f;
            int injuryCount = 0;
            float totalSeverity = 0f;

            if (pawn.health?.hediffSet?.hediffs != null)
            {
                foreach (var injury in pawn.health.hediffSet.hediffs.OfType<Hediff_Injury>())
                {
                    if (injury.IsTended() || injury.Severity <= 0.01f) continue;
                    injuryCount++;
                    totalSeverity += injury.Severity;
                }
            }
            if (injuryCount > 0)
                xpMultiplier += 0.05f * injuryCount;
            if (totalSeverity > 5) xpMultiplier *= 1.2f;
            if (totalSeverity > 10) xpMultiplier *= 1.5f;

            float finalXP = downedXP * xpMultiplier;
            skill.Learn(finalXP);

            if (Prefs.DevMode)
            {
                Log.Message($"[Toughness] {pawn.Name} downed in combat! Awarding {finalXP} XP.");
            }
        }
    }

    public class CompProperties_ToughnessCache : CompProperties
    {
        public CompProperties_ToughnessCache() => compClass = typeof(CompToughnessCache);
    }
}