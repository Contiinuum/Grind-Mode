using MelonLoader;
using System.Reflection;

namespace AudicaModding
{
    public static class Config
    {
        public const string Category = "GrindMode";

        public static bool autoSkip;
        public static bool includeChainSustainBreak;
        public static int allowedMissCount;
        public static bool highscoreMode;
        public static bool showStats;
        
        public static void RegisterConfig()
        {
            MelonPrefs.RegisterBool(Category, nameof(highscoreMode), false, "Highscore mode restarts a song once you can't beat your current highscore anymore. Ignores allowed miss count.");
            MelonPrefs.RegisterBool(Category, nameof(showStats), true, "Shows stats screen after failing a song.");
            MelonPrefs.RegisterBool(Category, nameof(includeChainSustainBreak), false, "Counts chain and sustain breaks as misses.");
            MelonPrefs.RegisterInt(Category, nameof(allowedMissCount), 0, "How many misses you are allowed to have before restarting a song.[0, 10, 1, 0]");          
            MelonPrefs.RegisterBool(Category, nameof(autoSkip), false, "Enables automatic skipping of song intros.");
           

            OnModSettingsApplied();
        }

        public static void OnModSettingsApplied()
        {
            foreach (var fieldInfo in typeof(Config).GetFields(BindingFlags.Static | BindingFlags.Public))
            {
                if (fieldInfo.Name == "Category") continue;
                if (fieldInfo.FieldType == typeof(bool)) fieldInfo.SetValue(null, MelonPrefs.GetBool(Category, fieldInfo.Name));
                else if (fieldInfo.FieldType == typeof(int)) fieldInfo.SetValue(null, MelonPrefs.GetInt(Category, fieldInfo.Name));
            }
            GrindMode.UpdateQuickButtons();
        }

        public static void Save()
        {
            //MelonPrefs.SetBool("IntroSkip", "enabled", introSkip);
            MelonPrefs.SetBool("GrindMode", nameof(autoSkip), autoSkip);
            MelonPrefs.SetBool("GrindMode", nameof(includeChainSustainBreak), includeChainSustainBreak);
            MelonPrefs.SetInt("GrindMode", nameof(allowedMissCount), allowedMissCount);
            //MelonPrefs.SetBool("GrindMode", "quickButtons", quickButtons);
            MelonPrefs.SetBool("GrindMode", nameof(highscoreMode), highscoreMode);
            MelonPrefs.SetBool("GrindMode", nameof(showStats), showStats);
            OnModSettingsApplied();
           
        }
    }
}
