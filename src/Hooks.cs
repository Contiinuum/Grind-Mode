using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using MelonLoader;
using UnityEngine;

namespace AudicaModding
{
    internal static class Hooks
    {
        private static int buttonCount = 0;

        //Hook to initiate intro skipping
        [HarmonyPatch(typeof(AudioDriver), "StartPlaying")]
        private static class PatchStartPlaying
        {
            private static void Prefix(AudioDriver __instance)
            {
                if (KataConfig.I.practiceMode) return;

                if (GrindMode.waitForRestart) GrindMode.waitForRestart = false;
                if (GrindMode.skipQueued || Config.autoSkip)
                {
                    GrindMode.SkipIntro();
                }
            }

        }

        [HarmonyPatch(typeof(SongCues), "LoadCues")]
        private static class PatchLoadCues
        {
            private static void Postfix(SongCues __instance)
            {
                if (KataConfig.I.practiceMode) return;

                if(GrindMode.Enabled && Config.highscoreMode)
                    GrindMode.SetCues(__instance.mCues.cues);
            }
        }

        [HarmonyPatch(typeof(OptionsMenu), "AddButton", new Type[] { typeof(int), typeof(string), typeof(OptionsMenuButton.SelectedActionDelegate), typeof(OptionsMenuButton.IsCheckedDelegate), typeof(string), typeof(OptionsMenuButton), })]
        private static class AddButtonButton
        {
            private static void Postfix(OptionsMenu __instance, int col, string label, OptionsMenuButton.SelectedActionDelegate onSelected, OptionsMenuButton.IsCheckedDelegate isChecked)
            {
                if (__instance.mPage == OptionsMenu.Page.Main)
                {
                    buttonCount++;
                    if (buttonCount == 9)
                    {
                        GrindModePanel.SetMenu(__instance);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(MenuState), "SetState", new Type[] { typeof(MenuState.State) })]
        private static class PatchSetMenuState
        {
            private static void Prefix(MenuState __instance, ref MenuState.State state)
            {
                if (KataConfig.I.practiceMode) return;

                if (GrindMode.recordRestarted)
                {
                    if (state == MenuState.State.SongPage) GrindMode.RecordRestart();                       
                }
            }

            private static void Postfix(MenuState __instance, ref MenuState.State state)
            {
                if (state == MenuState.State.LaunchPage)
                {
                    GrindModeButton.CreateGrindModeButton();
                    MelonCoroutines.Start(GrindMode.CreateIntroSkipButton());
                }
                else if(state == MenuState.State.SongPage)
                {
                    GrindMode.GetAudioComponent();
                }
            }
        }

        [HarmonyPatch(typeof(OptionsMenu), "ShowPage", new Type[] { typeof(OptionsMenu.Page) })]
        private static class PatchShowOptionsPage
        {
            private static void Prefix(OptionsMenu __instance, OptionsMenu.Page page)
            {
                buttonCount = 0;
            }

            private static void Postfix(InGameUI __instance, OptionsMenu.Page page)
            {
                if (page == OptionsMenu.Page.Main)
                {
                    if (GrindMode.ShowPanel) GrindModePanel.GoToPanel();
                }
            }
        }

        [HarmonyPatch(typeof(OptionsMenu), "BackOut", new Type[0])]
        private static class Backout
        {
            private static bool Prefix(OptionsMenu __instance)
            {
                if (GrindMode.ShowPanel)
                {
                    GrindModePanel.Return();
                    return false;
                }
                return true;
            }
        }



        [HarmonyPatch(typeof(PauseScreen), "Pause", new Type[] { typeof(bool) })]
        private static class PatchPause
        {
            private static void Postfix(PauseScreen __instance)
            {
                if (KataConfig.I.practiceMode) return;

                GrindMode.SetPaused(true);
            }
        }

        [HarmonyPatch(typeof(PauseScreen), "Resume")]
        private static class PathResume
        {
            private static void Prefix(PauseScreen __instance)
            {
                if (KataConfig.I.practiceMode) return;

                GrindMode.SetPaused(false);
            }
        }

        [HarmonyPatch(typeof(InGameUI), "Restart")]
        private static class PatchRestart
        {
            private static void Prefix(InGameUI __instance)
            {
                if (KataConfig.I.practiceMode) return;

                GrindMode.ResetVariables();
                GrindMode.DontRecordRestart();
            }
        }

        [HarmonyPatch(typeof(FailedScreen), "OnEnable")]
        private static class PatchGoToFailedPage
        {
            private static bool Prefix(InGameUI __instance)
            {
                if (KataConfig.I.practiceMode) return true;

                GrindMode.RestartSong(true);
                return true;
            }
        }

       /* [HarmonyPatch(typeof(OptionsMenu), "ShowPage", new Type[] { typeof(OptionsMenu.Page) })]
        private static class PatchOptionsMenuUpdate
        {
            private static void Postfix(OptionsMenu __instance, ref OptionsMenu.Page page)
            {
                if (GrindMode.menuSpawned && page != OptionsMenu.Page.Misc) GrindMode.menuSpawned = false;
                else if (!GrindMode.menuSpawned && page == OptionsMenu.Page.Misc) GrindMode.AddSettingsButtons(__instance);
            }

        }
        */

        //Hook for reporting misses
        [HarmonyPatch(typeof(ScoreKeeper), "OnFailure", new Type[] { typeof(SongCues.Cue), typeof(bool), typeof(bool) })]
        private static class PatchScoreKeeperOnFailure
        {
            private static void Postfix(ScoreKeeper __instance, ref SongCues.Cue cue)
            {
                if (KataConfig.I.practiceMode) return;

                if (GrindMode.waitForRestart) return;
                if(GrindMode.Enabled && Config.highscoreMode && !GrindMode.highscoreIsSetup)
                    GrindMode.SetHighscore(ScoreKeeper.I.GetHighScore());

                if (cue is null)
                {
                    return;
                }             
                   
                if (!GrindMode.Enabled || KataConfig.I.NoFail()) return;

                if (Config.highscoreMode)
                {
                    //if (!GrindMode.skipSetScoreMiss)
                        GrindMode.SetCurrentScore(__instance.mScore, __instance.mStreak, __instance.mMultiplier, cue, true);

                    //GrindMode.skipSetScoreMiss = !GrindMode.skipSetScoreMiss;
                    return;
                }

                if (!Config.includeChainSustainBreak)
                {
                    if (cue.behavior == Target.TargetBehavior.Chain)
                    {
                        //MelonModLogger.Log("Chain break! Ignoring.");
                        return;
                    }
                    else if (cue.behavior == Target.TargetBehavior.Hold && cue.target.mSustainFailed)
                    {
                        //MelonModLogger.Log("Sustain break! Ignoring.");
                        return;
                    }

                }
                GrindMode.ReportMiss(cue);

            }
        }

        //Hook for reporting completed chains
        [HarmonyPatch(typeof(ScoreKeeper), "OnSuccess", new Type[] { typeof(SongCues.Cue) })]
        private static class PatchScoreKeeperOnSuccess
        {
            private static void Postfix(ScoreKeeper __instance, ref SongCues.Cue cue)
            {
                if (KataConfig.I.practiceMode) return;
              

                if (!GrindMode.Enabled || KataConfig.I.NoFail()) return;

                if (Config.highscoreMode)
                {
                    if (!GrindMode.highscoreIsSetup)
                        GrindMode.SetHighscore(ScoreKeeper.I.GetHighScore());

                   // if (!GrindMode.skipSetScoreSuccess)
                        GrindMode.SetCurrentScore(__instance.mScore, __instance.mStreak, __instance.mMultiplier, cue);

                    //GrindMode.skipSetScoreSuccess = !GrindMode.skipSetScoreSuccess;
                    return;
                }

                if (GrindMode.chainLH)
                {
                    if (cue.handType == Target.TargetHandType.Left) GrindMode.chainLH = false;
                }
                else if (GrindMode.chainRH)
                {
                    if (cue.handType == Target.TargetHandType.Right) GrindMode.chainRH = false;
                }
            }
        }

        //Hook to validate scores after skipping the intro which we have to manually call after skipping an intro
        [HarmonyPatch(typeof(ScoreKeeper), "GetScoreValidity")]
        private static class PatchGetScoreValidity
        {
            private static bool Prefix(ScoreKeeper __instance, ref ScoreKeeper.ScoreValidity __result)
            {
                if (KataConfig.I.practiceMode) return true;

                if (GrindMode.introSkipped)
                {

                    //if (__result == ScoreKeeper.ScoreValidity.Valid || __result == ScoreKeeper.ScoreValidity.NoFail) return false;
                    __result = ScoreKeeper.ScoreValidity.Valid;
                    __instance.mHasInvalidatedScore = false;
                    return true;
                }
                return false;
            }
        }

        [HarmonyPatch(typeof(ScoreKeeper), "OnJump", new Type[] { typeof(float), typeof(float) })]
        private static class PatchOnJump
        {
            private static bool Prefix()
            {
                if (KataConfig.I.practiceMode) return true;

                if (GrindMode.introSkipped)
                {
                    return true;
                }
                else return false;
                
            }
        }
    }
}
