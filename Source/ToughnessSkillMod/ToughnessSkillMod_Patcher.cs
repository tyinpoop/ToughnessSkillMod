using HarmonyLib;
using Verse;

namespace ToughnessSkillMod
{
    [StaticConstructorOnStartup]
    public static class ToughnessSkillMod_Patcher
    {
        static ToughnessSkillMod_Patcher()
        {
            // Create and run Harmony instance, applying all patches in this assembly
            var harmony = new Harmony("com.yourname.ToughnessSkillMod");
            harmony.PatchAll();
        }
    }
}
