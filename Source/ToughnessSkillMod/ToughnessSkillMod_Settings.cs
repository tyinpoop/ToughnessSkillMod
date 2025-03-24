using HugsLib;
using HugsLib.Settings;
using Verse;

namespace ToughnessSkillMod
{
    public class ToughnessSkillMod : ModBase
    {
        public static SettingHandle<float> baseXp;

        public override string ModIdentifier => "ToughnessSkillMod";

        public override void DefsLoaded()
        {
            baseXp = Settings.GetHandle<float>(
                "baseXp",
                "Toughness: Base XP per damage",
                "How much XP is awarded per point of final damage dealt",
                50f,
                Validators.FloatRangeValidator(0f, 500f)
            );
        }
    }
}
