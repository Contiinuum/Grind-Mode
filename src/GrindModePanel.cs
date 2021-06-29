using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

namespace AudicaModding
{
	internal static class GrindModePanel
	{
		private static OptionsMenu primaryMenu;

		private static OptionsMenuButton enabledButton;
		private static OptionsMenuButton autoSkipButton;
		private static OptionsMenuButton missCountButton;
		private static OptionsMenuButton behaviorButton;
		private static OptionsMenuButton instantRestartButton;


		static public void SetMenu(OptionsMenu optionsMenu)
		{
			primaryMenu = optionsMenu;
		}

		static public void GoToPanel()
		{
			primaryMenu.ShowPage(OptionsMenu.Page.Customization);
			CleanUpPage(primaryMenu);
			AddButtons(primaryMenu);
			primaryMenu.screenTitle.text = "Grind Mode";
		}

		public static void Return()
		{
			GrindMode.ShowPanel = false;
			Config.Save();
			MenuState.I.GoToLaunchPage();
		}

		private static void AddButtons(OptionsMenu optionsMenu)
		{
			Il2CppSystem.Collections.Generic.List<GameObject> row = new Il2CppSystem.Collections.Generic.List<GameObject>();

			string enabledText = "Grind Mode " + (GrindMode.Enabled ? "ENABLED" : "DISABLED");
			var enabled = optionsMenu.AddButton(0, enabledText, new Action(() =>
			{
				GrindMode.Enabled = !GrindMode.Enabled;
				ToggleEnabled();
			}), null, "Enables Grind Mode", optionsMenu.buttonPrefab);
			row.Add(enabled.gameObject);
			enabledButton = enabled;

			string autoSkipText = "Auto Intro-Skip " + (Config.autoSkip ? "ENABLED" : "DISABLED");
			var autoSkip = optionsMenu.AddButton(1, autoSkipText, new Action(() =>
			{
				Config.autoSkip = !Config.autoSkip;
				ToggleAutoSkip();
			}), null, "Enables automatic intro skipping", optionsMenu.buttonPrefab);
			row.Add(autoSkip.gameObject);
			autoSkipButton = autoSkip;
						
			optionsMenu.scrollable.AddRow(row);
			row = new Il2CppSystem.Collections.Generic.List<GameObject>();

            if (GrindMode.Enabled)
            {
				string missText = "Allowed Misses: " + Config.allowedMissCount;
				var missCount = optionsMenu.AddButton(0, missText, new Action(() =>
				{
					if (Config.allowedMissCount == 10) Config.allowedMissCount = 0;
					else Config.allowedMissCount += 1;
					ToggleMissCount();
				}), null, "Set how many misses you're allowed to have before restarting", optionsMenu.buttonPrefab);
				row.Add(missCount.gameObject);
				missCountButton = missCount;
			}


            if (GrindMode.Enabled)
            {
				string behaviorText = "Mode: " + (Config.highscoreMode ? "HIGHSCORE" : "STANDARD");
				var behavior = optionsMenu.AddButton(1, behaviorText, new Action(() =>
				{
					Config.highscoreMode = !Config.highscoreMode;
					ToggleBehavior();
				}), null, "Changes Mode. HIGHSCORE restarts after you can't reach a new highscore anymore. STANDARD restarts after you missed a certain amount of times.", optionsMenu.buttonPrefab);
				row.Add(behavior.gameObject);
				behaviorButton = behavior;
			}		
			optionsMenu.scrollable.AddRow(row);
			row = new Il2CppSystem.Collections.Generic.List<GameObject>();

            if (GrindMode.Enabled)
            {
				string restartText = "Instant Restart: " + (Config.showStats ? "OFF" : "ON");
				var restart = optionsMenu.AddButton(0, restartText, new Action(() =>
				{
					Config.showStats = !Config.showStats;
					ToggleInstantRestart();
				}), null, "Enables instant restarting after failing.", optionsMenu.buttonPrefab);
				optionsMenu.scrollable.AddRow(restart.gameObject);
				instantRestartButton = restart;
			}			
		}

		private static void ToggleEnabled()
        {
			enabledButton.label.text = "Grind Mode " + (GrindMode.Enabled ? "ENABLED" : "DISABLED");
			RefreshPage();
		}

		private static void ToggleAutoSkip()
        {
			autoSkipButton.label.text = "Auto Intro-Skip " + (Config.autoSkip ? "ENABLED" : "DISABLED");
		}

		private static void ToggleMissCount()
        {
			missCountButton.label.text = "Allowed Misses: " + Config.allowedMissCount;
		}

		private static void ToggleBehavior()
        {
			behaviorButton.label.text = "Mode: " + (Config.highscoreMode ? "HIGHSCORE" : "STANDARD");
		}

		private static void ToggleInstantRestart()
        {
			instantRestartButton.label.text = "Instant Restart: " + (Config.showStats ? "OFF" : "ON");
		}

		private static void CleanUpPage(OptionsMenu optionsMenu)
		{
			Transform optionsTransform = optionsMenu.transform;
			for (int i = 0; i < optionsTransform.childCount; i++)
			{
				Transform child = optionsTransform.GetChild(i);
				if (child.gameObject.name.Contains("(Clone)"))
				{
					GameObject.Destroy(child.gameObject);
				}
			}
			optionsMenu.mRows.Clear();
			optionsMenu.scrollable.ClearRows();
			optionsMenu.scrollable.mRows.Clear();
		}

		private static void RefreshPage()
        {
			CleanUpPage(primaryMenu);
			AddButtons(primaryMenu);
        }


	}
}