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
            MelonPreferences.CreateEntry(Category, nameof(highscoreMode), false, "Highscore mode restarts a song once you can't beat your current highscore anymore. Ignores allowed miss count.");
            MelonPreferences.CreateEntry(Category, nameof(showStats), true, "Shows stats screen after failing a song.");
            MelonPreferences.CreateEntry(Category, nameof(includeChainSustainBreak), false, "Counts chain and sustain breaks as misses.");
            MelonPreferences.CreateEntry(Category, nameof(allowedMissCount), 0, "How many misses you are allowed to have before restarting a song.[0, 10, 1, 0]");
            MelonPreferences.CreateEntry(Category, nameof(autoSkip), false, "Enables automatic skipping of song intros.");
           

            OnPreferencesSaved();
        }

        public static void OnPreferencesSaved()
        {
            foreach (var fieldInfo in typeof(Config).GetFields(BindingFlags.Static | BindingFlags.Public))
            {
                if (fieldInfo.Name == "Category") continue;
                if (fieldInfo.FieldType == typeof(bool)) fieldInfo.SetValue(null, MelonPreferences.GetEntryValue<bool>(Category, fieldInfo.Name));
                else if (fieldInfo.FieldType == typeof(int)) fieldInfo.SetValue(null, MelonPreferences.GetEntryValue<int>(Category, fieldInfo.Name));
            }
        }

        public static void Save()
        {
            MelonPreferences.SetEntryValue(Category, nameof(autoSkip), autoSkip);
            MelonPreferences.SetEntryValue(Category, nameof(includeChainSustainBreak), includeChainSustainBreak);
            MelonPreferences.SetEntryValue(Category, nameof(allowedMissCount), allowedMissCount);
            MelonPreferences.SetEntryValue(Category, nameof(highscoreMode), highscoreMode);
            MelonPreferences.SetEntryValue(Category, nameof(showStats), showStats);
            MelonPreferences.Save();
            //OnPreferencesSaved();
           
        }
    }
}
