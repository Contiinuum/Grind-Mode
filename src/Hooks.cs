using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Harmony;
using MelonLoader;
using UnityEngine;

namespace AudicaModding
{
    internal static class Hooks
    {
        public static void ApplyHooks(HarmonyInstance instance)
        {
            instance.PatchAll(Assembly.GetExecutingAssembly());
        }

        //Hook to initiate intro skipping
        [HarmonyPatch(typeof(AudioDriver), "StartPlaying")]
        private static class PatchStartPlaying
        {
            private static void Prefix(AudioDriver __instance)
            {
                if (!KataConfig.I.practiceMode && (AudicaMod.skipQueued || AudicaMod.autoSkip))
                {
                    AudicaMod.SkipIntro();
                }
            }

        }

        //Used mainly for creating and enabling/disabling buttons
        [HarmonyPatch(typeof(MenuState), "SetState", new Type[] { typeof(MenuState.State) })]
        private static class PatchSetMenuState
        {
            private static void Postfix(MenuState __instance, ref MenuState.State state)
            {
                if (AudicaMod.quickButtons)
                {
                    if (state == MenuState.State.LaunchPage && !AudicaMod.grindButtonCreated && !AudicaMod.autoSkipButtonCreated && !AudicaMod.allowedMissCountButtonCreated)
                    {
                        MelonCoroutines.Start(AudicaMod.AddLaunchPanelButtons());

                    }
                    else if (AudicaMod.grindButtonCreated || AudicaMod.autoSkipButtonCreated)
                    {
                        if (state == MenuState.State.LaunchPage) MelonCoroutines.Start(AudicaMod.SetLaunchPanelButtonsActive(true));
                        else if (state == MenuState.State.Launching) MelonCoroutines.Start(AudicaMod.SetLaunchPanelButtonsActive(false, true));
                        else if (state != MenuState.State.Launched) MelonCoroutines.Start(AudicaMod.SetLaunchPanelButtonsActive(false));
                    }
                }
                else if (AudicaMod.grindButtonCreated || AudicaMod.autoSkipButtonCreated) MelonCoroutines.Start(AudicaMod.SetLaunchPanelButtonsActive(false, true));
               
                           
                if (AudicaMod.introSkip && state == MenuState.State.SongPage && AudicaMod.menuButton is null) AudicaMod.CreateIntroSkipButton();

                if (AudicaMod.introSkipButtonCreated)
                {
                    if (state != MenuState.State.Launched || state != MenuState.State.Launching) AudicaMod.SetIntroSkipButtonActive(false);
                    else if (state == MenuState.State.Launched && (AudicaMod.autoSkip || KataConfig.I.practiceMode)) AudicaMod.SetIntroSkipButtonActive(false);
                }

                if(state == MenuState.State.Launched)
                {
                    AudicaMod.ResetVariables();
                }

                if (AudicaMod.audiocomponent is null && state == MenuState.State.SongPage) AudicaMod.GetAudioComponent();

            }

        }

        [HarmonyPatch(typeof(PauseScreen), "Pause", new Type[] { typeof(bool) })]
        private static class PatchPause
        {
            private static void Postfix(PauseScreen __instance)
            {
                if(AudicaMod.introSkip)
                    AudicaMod.SetPaused(true);
            }
        }

        [HarmonyPatch(typeof(PauseScreen), "Resume")]
        private static class PathResume
        {
            private static void Prefix(PauseScreen __instance)
            {
                if (AudicaMod.introSkip)
                    AudicaMod.SetPaused(false);
            }
        }

        [HarmonyPatch(typeof(InGameUI), "Restart")]
        private static class PatchRestart
        {
            private static void Prefix(InGameUI __instance)
            {
                AudicaMod.ResetVariables();
            }
        }

        [HarmonyPatch(typeof(OptionsMenu), "ShowPage", new Type[] { typeof(OptionsMenu.Page) })]
        private static class PatchOptionsMenuUpdate
        {
            private static void Postfix(OptionsMenu __instance, ref OptionsMenu.Page page)
            {
                if (AudicaMod.menuSpawned && page != OptionsMenu.Page.Misc) AudicaMod.menuSpawned = false;
                else if (!AudicaMod.menuSpawned && page == OptionsMenu.Page.Misc) AudicaMod.AddSettingsButtons(__instance);
            }

        }

        //Hook for reporting misses
        [HarmonyPatch(typeof(ScoreKeeper), "OnFailure", new Type[] { typeof(SongCues.Cue), typeof(bool), typeof(bool) })]
        private static class PatchScoreKeeperOnFailure
        {
            private static void Postfix(ref SongCues.Cue cue)
            {
                if (!AudicaMod.grindMode || KataConfig.I.NoFail()) return;

                if(cue is null)
                {
                    return;
                }
                if (!AudicaMod.includeChainSustainBreak)
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
                AudicaMod.ReportMiss(cue);
                
            }
        }

        //Hook for reporting completed chains
        [HarmonyPatch(typeof(ScoreKeeper), "OnSuccess", new Type[] { typeof(SongCues.Cue)})]
        private static class PatchScoreKeeperOnSuccess
        {
            private static void Postfix(ref SongCues.Cue cue)
            {
                if (AudicaMod.chainLH)
                {
                    if (cue.handType == Target.TargetHandType.Left) AudicaMod.chainLH = false;
                }
                else if(AudicaMod.chainRH)
                {
                    if (cue.handType == Target.TargetHandType.Right) AudicaMod.chainRH = false;
                }
            }
        }

        //Hook to validate scores after skipping the intro which we have to manually call after skipping an intro
        [HarmonyPatch(typeof(ScoreKeeper), "GetScoreValidity")]
        private static class PatchGetScoreValidity
        {
            private static bool Prefix(ScoreKeeper __instance, ref ScoreKeeper.ScoreValidity __result)
            {
                if (AudicaMod.introSkipped)
                {
                    if (__result == ScoreKeeper.ScoreValidity.Valid || __result == ScoreKeeper.ScoreValidity.NoFail) return true;
                    __result = ScoreKeeper.ScoreValidity.Valid;
                }
                return true;
            }
        }
    }
}
