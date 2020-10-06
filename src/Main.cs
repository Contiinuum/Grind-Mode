using System;
using System.Collections.Generic;
using MelonLoader;
using UnityEngine;
using Harmony;
using TMPro;
using UnityEngine.Events;
using System.Collections;

namespace AudicaModding
{
    public class GrindMode : MelonMod
    {

        #region Intro Skipping
        //static public bool introSkip = false;
        static public bool introSkipped = false;
        static public bool isPlaying = false;
        static public bool skipQueued = false;
        static public bool canSkip = false;
        static public bool autoSkip = false;
        static private bool isPaused = false;

        static public int cachedFirstTick = 0;
        #endregion
        #region Grind Mode
        static public bool grindMode = false;
        static public bool includeChainSustainBreak = false;
        static public bool reportLastChainNode = false;
        //static public bool quickButtons = false;
        static public bool highscoreMode = false;
        static public bool highscoreIsSetup = false;
        static public bool waitForRestart = false;
        static public bool skipSetScoreMiss = false;
        static public bool skipSetScoreSuccess = false;
        static public bool cuesSet = false;

        static private bool showStats = false;
        static public bool recordRestarted = false;

        static public int missCount = 0;     
        static public int allowedMissCount = 10;
 
        static private List<SongCues.Cue> reportedCues = new List<SongCues.Cue>();
        static private SongCues.Cue lastTarget = new SongCues.Cue(0, 0, 0, 0, Target.TargetHandType.None, Target.TargetBehavior.Standard, Vector2.zero);
        static private int sustainTickLH = 0;
        static private int sustainTickRH = 0;

        static public bool chainLH = false;
        static public bool chainRH = false;

        static public HmxAudioEmitter audiocomponent = null;

        static private List<SongCues.Cue> songCues = new List<SongCues.Cue>();

        static public int highscore = 0;
        static public int currentScore = 0;
        static public int currentStreak = 0;
        static public int currentMultiplier = 0;
        #endregion
        #region Settings Menu
        //public static OptionsMenu optionMenu;     
        //public static OptionsMenuButton toggleButtonGrind = null;
        //public static OptionsMenuButton toggleButtonIntro = null;
        //public static OptionsMenuButton toggleButtonAutoSkip = null;
        public static OptionsMenuButton toggleButtonIncludeBreaks = null;
        //public static OptionsMenuButton toggleButtonAllowedMissCount = null;
        //public static OptionsMenuButton toggleButtonQuickButtons = null;
        //public static OptionsMenuButton toggleButtonBehavior = null;

        public static bool menuSpawned = false;


        #endregion
        #region Launch Panel
        static public OptionsMenuButton launchButton = null;
        static public OptionsMenuButton grindModeButton = null;
        static public OptionsMenuButton autoSkipButton = null;
        static public OptionsMenuButton allowedMissCountButton = null;
        static public OptionsMenuButton behaviorButton = null;
        static public OptionsMenuButton instantRestartButton = null;

        static private Vector3 launchPanelButtonScale = new Vector3(1f, 1f, 1f);

        static public bool grindButtonCreated = false;
        static public bool autoSkipButtonCreated = false;
        static public bool allowedMissCountButtonCreated = false;
        static public bool behaviorButtonCreated = false;
        static public bool instantRestartButtonCreated = false;
        #endregion
        #region InGame Button
        static public GameObject menuButton = null;
        static private GameObject skipIntroButton = null;

        static private Vector3 skipIntroButtonPos = new Vector3(.5f, 1.3f, 1.5f);
        static private Vector3 skipintroButtonRot = new Vector3(35f, 0, 0);
        static private Vector3 skipIntroButtonScale = new Vector3(.16f, .16f, .16f);

        static public bool introSkipButtonCreated = false;
        #endregion

        public static class BuildInfo
        {
            public const string Name = "GrindMode";  // Name of the Mod.  (MUST BE SET)
            public const string Author = "Continuum"; // Author of the Mod.  (Set as null if none)
            public const string Company = null; // Company that made the Mod.  (Set as null if none)
            public const string Version = "0.1.0"; // Version of the Mod.  (MUST BE SET)
            public const string DownloadLink = null; // Download Link for the Mod.  (Set as null if none)
        }

        private void CreateConfig()
        {
            if(!ModPrefs.HasKey("IntroSkip", "enabled")) ModPrefs.RegisterPrefBool("IntroSkip", "enabled", false);
            if (!ModPrefs.HasKey("AutoSkip", "enabled")) ModPrefs.RegisterPrefBool("AutoSkip", "enabled", false);
            if (!ModPrefs.HasKey("GrindMode", "breaks")) ModPrefs.RegisterPrefBool("GrindMode", "breaks", false);
            if (!ModPrefs.HasKey("GrindMode", "missCount")) ModPrefs.RegisterPrefInt("GrindMode", "missCount", 0);
            //ModPrefs.RegisterPrefBool("GrindMode", "quickButtons", false);
            if (!ModPrefs.HasKey("GrindMode", "highscoreMode")) ModPrefs.RegisterPrefBool("GrindMode", "highscoreMode", false);
            if (!ModPrefs.HasKey("GrindMode", "showStats")) ModPrefs.RegisterPrefBool("GrindMode", "showStats", false);
            
        }

        private void LoadConfig()
        {
            //introSkip = ModPrefs.GetBool("IntroSkip", "enabled");
            autoSkip = ModPrefs.GetBool("AutoSkip", "enabled");
            includeChainSustainBreak = ModPrefs.GetBool("GrindMode", "breaks");
            allowedMissCount = ModPrefs.GetInt("GrindMode", "missCount");
            //quickButtons = ModPrefs.GetBool("GrindMode", "quickButtons");
            highscoreMode = ModPrefs.GetBool("GrindMode", "highscoreMode");
            showStats = ModPrefs.GetBool("GrindMode", "showStats");
        }

        public static void SaveConfig()
        {
            //ModPrefs.SetBool("IntroSkip", "enabled", introSkip);
            ModPrefs.SetBool("AutoSkip", "enabled", autoSkip);
            ModPrefs.SetBool("GrindMode", "breaks", includeChainSustainBreak);
            ModPrefs.SetInt("GrindMode", "missCount", allowedMissCount);
            //ModPrefs.SetBool("GrindMode", "quickButtons", quickButtons);
            ModPrefs.SetBool("GrindMode", "highscoreMode", highscoreMode);
            ModPrefs.SetBool("GrindMode", "showStats", showStats);
        }

        public override void OnApplicationStart()
        {
            HarmonyInstance instance = HarmonyInstance.Create("AudicaMod");
            //CreateModMenu();
        }

        public override void OnLevelWasLoaded(int level)
        {
  
            if (!ModPrefs.HasKey("IntroSkip", "enabled") || !ModPrefs.HasKey("GrindMode", "breaks") || !ModPrefs.HasKey("GrindMode", "missCount") || !ModPrefs.HasKey("AutoSkip", "enabled") || !ModPrefs.HasKey("GrindMode", "showStats"))
            {
                CreateConfig();
            }
            else
            {
                LoadConfig();
                
            }         
        }

        public static void ReportMiss(SongCues.Cue cue)
        {
            if (highscoreMode) return;
            //return here, else every single chain node would count as an individual miss
            if (lastTarget.behavior == Target.TargetBehavior.Chain && cue.behavior == Target.TargetBehavior.Chain && lastTarget.handType == cue.handType) return;
           
            if (cue.behavior == Target.TargetBehavior.Chain || cue.behavior == Target.TargetBehavior.ChainStart)
            {
                if (cue.handType == Target.TargetHandType.Left) chainLH = true;
                else chainRH = true;
            }
            else if (chainLH) chainLH = false;
            else if (chainRH) chainRH = false;
            lastTarget = cue;
            reportedCues.Add(cue);
            
            missCount++;
            CheckFail();
        }

        private static void CheckFail()
        {
            if (missCount > allowedMissCount)
            {
                RestartSong();
            }
               
        }

        public static void RestartSong(bool failed = false)
        {
            waitForRestart = true;
            if (!showStats && !failed) InGameUI.I.Restart();
            else if(showStats)
            {
                //SongEnd.I.ShowEndSeqence();
                TargetSpawner spawner = GameObject.FindObjectOfType<TargetSpawner>();
                Target[] activeTargets = GameObject.FindObjectsOfType<Target>();
                
                foreach(Target t in activeTargets)
                {
                    t.transform.root.gameObject.SetActive(false);
                }
                SongEnd.I.ShowResults();
                recordRestarted = true;
                AudioDriver.I.Pause();
            }
            PlaySound();
            ResetVariables();
        }

        public static void RecordRestart()
        {
            recordRestarted = false;
            ScoreKeeper.I.OnRestart();
        }

        static public void GetAudioComponent()
        {
            audiocomponent = GameObject.Find("HmxAudioEmitter (1)").GetComponent<HmxAudioEmitter>();
        }

        static private void PlaySound()
        {
            KataUtil.PlayFMODEvent("event:/gameplay/overdrive_complete", audiocomponent.Get());
        }

      

        public static IEnumerator SetLaunchPanelButtonsActive(bool active, bool immediate = false)
        {

            if (immediate) yield return null;
            else if (active) yield return new WaitForSeconds(.65f);
            else yield return new WaitForSeconds(.3f);

            if (autoSkipButtonCreated) autoSkipButton.gameObject.SetActive(active);
            if (grindButtonCreated) grindModeButton.gameObject.SetActive(active);
            if (instantRestartButtonCreated) instantRestartButton.gameObject.SetActive(active);
            if (behaviorButtonCreated) behaviorButton.gameObject.SetActive(active);
            if (grindMode && !highscoreMode && allowedMissCountButtonCreated) allowedMissCountButton.gameObject.SetActive(active);
            else if (allowedMissCountButtonCreated) allowedMissCountButton.gameObject.SetActive(false);                             
        }

        public static void SetIntroSkipButtonActive(bool active)
        {
            try
            {
                if (autoSkip) skipIntroButton.SetActive(false);
                else skipIntroButton.SetActive(active);
            }
            catch
            {
                CreateIntroSkipButton(true);
                SetIntroSkipButtonActive(active);
            }
        }

        public static int GetFirstTick()
        {
            if(cachedFirstTick == 0)
            {
                SongList.SongData songData = SongDataHolder.I.songData;
                KataConfig.Difficulty diff = KataConfig.I.GetDifficulty();
                cachedFirstTick = SongCues.GetCues(songData, diff)[0].tick;
            }
            return cachedFirstTick;
        }

        public static float GetCurrentTick()
        {
            return AudioDriver.I.mCachedTick;
        }

        //Queues an intro skip for when AudioDriver is not instantiated yet
        public static void QueueSkip()
        {
            skipQueued = true;
        }

        public static void SkipIntro()
        {
            if (GetCurrentTick() <= GetFirstTick() - 5760)
            {             
                AudioDriver.I.JumpToTick(GetFirstTick() - 2880);
                introSkipped = true;
                skipQueued = false;
                //Call this so the score gets validated
                //MelonModLogger.Log("Is practice mode: " + KataConfig.I.practiceMode.ToString());
                //MelonModLogger.Log("Is NofailKataConfig: " + KataConfig.I.NoFail()
                if(!KataConfig.I.practiceMode)
                    ScoreKeeper.I.GetScoreValidity();
            }
        }

        //Event listener for introSkipButton
        private static void OnSkipButtonShot()
        {          
            if (AudioDriver.I is null) QueueSkip();
            else SkipIntro();
        }

        public static void CreateIntroSkipButton(bool reinstantiate = false)
        {
            menuButton = GameObject.FindObjectOfType<MainMenuPanel>().buttons[1];
            skipIntroButton = CreateButton(menuButton, "Skip Intro", OnSkipButtonShot, skipIntroButtonPos, skipintroButtonRot, skipIntroButtonScale);
            introSkipButtonCreated = true;
            SetIntroSkipButtonActive(reinstantiate);
        }

        //wait a bit, else LaunchPanel is null
        public static IEnumerator AddLaunchPanelButtons()
        {
            yield return new WaitForSeconds(.6f);
            LaunchPanel lp = GameObject.FindObjectOfType<LaunchPanel>();
            launchButton = lp.songPreviewButton;
            CreateLaunchPanelButtons();      
        }
       

        public static void CreateLaunchPanelButtons()
        {
            #region Grind Button
            if (!grindButtonCreated)
            {
                grindModeButton = UnityEngine.Object.Instantiate(launchButton);
                grindModeButton.transform.localScale = launchPanelButtonScale;
                UnityEngine.Object.Destroy(grindModeButton.transform.root.GetComponentInChildren<Localizer>());

                TextMeshPro grindButtontext = grindModeButton.transform.root.GetComponentInChildren<TextMeshPro>();
                grindButtontext.text = grindMode ? "GrindMode ON" : "GrindMode OFF";

                grindModeButton.SelectedAction = null;
                grindModeButton.IsChecked = null;
                grindModeButton.SelectedAction = new Action(() =>
                {
                    grindMode = !grindMode;
                    string txt = grindMode ? "ON" : "OFF";
                    allowedMissCountButton.gameObject.SetActive(grindMode && !highscoreMode);
                    grindModeButton.label.text = "Grind Mode " + txt;
                    //if (toggleButtonGrind is OptionsMenuButton) toggleButtonGrind.label.text = txt;
                });
                grindModeButton.transform.position = new Vector3(0, 13.2f, 24.19168f);
                grindButtonCreated = true;
            }
            #endregion
            #region Auto Skip Button
            if (!autoSkipButtonCreated)
            {
                autoSkipButton = UnityEngine.Object.Instantiate(launchButton);
                autoSkipButton.transform.localScale = launchPanelButtonScale;
                UnityEngine.Object.Destroy(autoSkipButton.transform.root.GetComponentInChildren<Localizer>());


                TextMeshPro autoSkipButtonText = autoSkipButton.transform.root.GetComponentInChildren<TextMeshPro>();
                autoSkipButtonText.text = autoSkip ? "AutoSkip ON" : "AutoSkip OFF";

                autoSkipButton.SelectedAction = null;
                autoSkipButton.IsChecked = null;
                autoSkipButton.SelectedAction = new Action(() =>
                {
                    autoSkip = !autoSkip;
                    string txt = "Auto Skip " + (autoSkip ? "ON" : "OFF");
                    autoSkipButton.label.text = txt;
                    //if (toggleButtonAutoSkip is OptionsMenuButton) toggleButtonAutoSkip.label.text = "Auto Skip: " + txt;
                    SaveConfig();
                });
                autoSkipButton.transform.position = new Vector3(-7.317519f, 13.2f, 24.19168f);
                autoSkipButtonCreated = true;
            }
            #endregion
            #region Miss Count Button
            if (!allowedMissCountButtonCreated)
            {
                allowedMissCountButton = UnityEngine.Object.Instantiate(launchButton);
                allowedMissCountButton.transform.localScale = launchPanelButtonScale;
                UnityEngine.Object.Destroy(allowedMissCountButton.transform.root.GetComponentInChildren<Localizer>());


                TextMeshPro missCountButtonText = allowedMissCountButton.transform.root.GetComponentInChildren<TextMeshPro>();
                missCountButtonText.text = "Allowed misses: " + allowedMissCount.ToString();

                allowedMissCountButton.SelectedAction = null;
                allowedMissCountButton.IsChecked = null;
                allowedMissCountButton.SelectedAction = new Action(() =>
                {
                    allowedMissCount += 1;
                    if (allowedMissCount > 10) allowedMissCount = 0;
                    string txt = "Allowed Misses: " + allowedMissCount.ToString();
                    allowedMissCountButton.label.text = txt;
                    SaveConfig();
                    
                });
                allowedMissCountButton.transform.position = new Vector3(7.317519f, 13.2f, 24.19168f);
                allowedMissCountButton.gameObject.SetActive(false);
                allowedMissCountButtonCreated = true;
            }
            #endregion
            #region Behavior Button
            if (!behaviorButtonCreated)
            {
                behaviorButton = UnityEngine.Object.Instantiate(launchButton);
                behaviorButton.transform.localScale = launchPanelButtonScale;
                UnityEngine.Object.Destroy(behaviorButton.transform.root.GetComponentInChildren<Localizer>());

                TextMeshPro behaviorButtonText = behaviorButton.transform.root.GetComponentInChildren<TextMeshPro>();
                behaviorButtonText.text = highscoreMode ? "Mode: Highscore" : "Mode: Standard";

                behaviorButton.SelectedAction = null;
                behaviorButton.IsChecked = null;
                behaviorButton.SelectedAction = new Action(() =>
                {
                    highscoreMode = !highscoreMode;
                    string txt = highscoreMode ? "Mode: Highscore" : "Mode: Standard";
                    allowedMissCountButton.gameObject.SetActive(!highscoreMode && grindMode);
                    behaviorButton.label.text = txt;
                    SaveConfig();
                });
                behaviorButton.transform.position = new Vector3(0, 15.2f, 24.19168f);
                behaviorButtonCreated = true;
            }
            #endregion
            #region Instant Restart Button
            if (!instantRestartButtonCreated)
            {
                instantRestartButton = UnityEngine.Object.Instantiate(launchButton);
                instantRestartButton.transform.localScale = launchPanelButtonScale;
                UnityEngine.Object.Destroy(instantRestartButton.transform.root.GetComponentInChildren<Localizer>());


                TextMeshPro instantSkipButtonText = instantRestartButton.transform.root.GetComponentInChildren<TextMeshPro>();
                instantSkipButtonText.text = showStats ? "Show Stats ON" : "Show Stats OFF";

                instantRestartButton.SelectedAction = null;
                instantRestartButton.IsChecked = null;
                instantRestartButton.SelectedAction = new Action(() =>
                {
                    showStats = !showStats;
                    string txt = "Show Stats " + (showStats ? "ON" : "OFF");
                    instantRestartButton.label.text = txt;
                    //if (toggleButtonAutoSkip is OptionsMenuButton) toggleButtonAutoSkip.label.text = "Auto Skip: " + txt;
                    SaveConfig();
                });
                instantRestartButton.transform.position = new Vector3(-7.317519f, 15.2f, 24.19168f);
                instantRestartButtonCreated = true;
            }
            #endregion
            SetLaunchPanelButtonsActive(true);
        }

     
        //track paused state so we can disable intro button while paused
        public static void SetPaused(bool _isPaused)
        {
            isPaused = _isPaused;
            if (isPaused) SetIntroSkipButtonActive(false);
            else if (canSkip && !autoSkip) SetIntroSkipButtonActive(true);
        }

        //creates the button used ingame for intro skipping
        private static GameObject CreateButton(GameObject buttonPrefab, string label, Action onHit, Vector3 position, Vector3 eulerRotation, Vector3 scale)
        {
            GameObject buttonObject = UnityEngine.Object.Instantiate(buttonPrefab);
            buttonObject.transform.rotation = Quaternion.Euler(eulerRotation);
            buttonObject.transform.position = position;
            buttonObject.transform.localScale = scale;

            UnityEngine.Object.Destroy(buttonObject.GetComponentInChildren<Localizer>());

            TextMeshPro buttonText = buttonObject.GetComponentInChildren<TextMeshPro>();
            buttonText.text = label;

            //turn off destroy event so we don't lose the reference, disable particles so they don't hit the player's face
            GunButton button = buttonObject.GetComponentInChildren<GunButton>();
            //don't comment this out, else you'll lose your reference to the button
            button.destroyOnShot = false;
            //comment out from here...
            button.doMeshExplosion = false;
            button.doParticles = false;
            //..to here if you want the explosion effect to play
            button.onHitEvent = new UnityEvent();
            button.onHitEvent.AddListener(onHit);
            
            return buttonObject.gameObject;
        }

        public static void AddSettingsButtons(OptionsMenu optionMenu)
        {
            /*
            #region Intro Skip
            optionMenu.AddHeader(0, "Intro Skip");
            string toggleTextIntro = introSkip ? "ON" : "OFF";
            toggleButtonIntro = optionMenu.AddButton
                (0,
                toggleTextIntro,
                new Action(() =>
                {
                    introSkip = !introSkip;
                    toggleButtonIntro.label.text = introSkip ? "ON" : "OFF";
                    SaveConfig();
                }),
                null,
                "Allows you to skip song intros");
            #endregion
            #region Auto Skip
            string toggleTextAutoSkip = autoSkip ? "Auto Skip ON" : "Auto Skip OFF";
            toggleButtonAutoSkip = optionMenu.AddButton
                (1,
                toggleTextAutoSkip,
                new Action(() =>
                {
                    autoSkip = !autoSkip;
                    string txt = "AutoSkip " + (autoSkip ? "ON" : "OFF");
                    SaveConfig();
                    
                    toggleButtonAutoSkip.label.text = txt;
                    if (autoSkipButtonCreated) autoSkipButton.label.text = txt;
                }),
                null,
                "Automatically skips song intros");
            #endregion
            #region Grind Mode
            optionMenu.AddHeader(0, "Grind Mode");
            string toggleTextGrind = grindMode ? "ON" : "OFF";
            toggleButtonGrind = optionMenu.AddButton
                (0,
                toggleTextGrind,
                new Action(() =>
                {
                    grindMode = !grindMode;
                    string txt = grindMode ? "ON" : "OFF";
                    SaveConfig();
                    
                    toggleButtonGrind.label.text = txt;
                    if (grindButtonCreated) grindModeButton.label.text = "Grind Mode " + txt;
                }),
                null,
                "Automatially restarts a song after a set amount of misses (Allowed Misses)");
            #endregion
            */
            #region Breaks
            optionMenu.AddHeader(0, "Grind Mode");
            string toggleTextBreaks = includeChainSustainBreak ? "Include Breaks ON" : "Include Breaks OFF";
            toggleButtonIncludeBreaks = optionMenu.AddButton
                (0,
                toggleTextBreaks,
                new Action(() =>
                {
                    includeChainSustainBreak = !includeChainSustainBreak;
                    toggleButtonIncludeBreaks.label.text = includeChainSustainBreak ? "Include Breaks ON" : "Include Breaks OFF";
                    SaveConfig();                   
                }),
                null,
                "Counts chain and sustain breaks as misses");
            #endregion
            /*
            #region Allowed Misses
            string toggleTextMiss = "Allowed misses: " + allowedMissCount.ToString();
            toggleButtonAllowedMissCount = optionMenu.AddButton
                (0,
                toggleTextMiss,
                new Action(() =>
                {                  
                    allowedMissCount += 1;
                    if (allowedMissCount > 10) allowedMissCount = 0;
                    string txt = "Allowed Misses: " + allowedMissCount.ToString();
                    toggleButtonAllowedMissCount.label.text = txt;
                    if(allowedMissCountButtonCreated) allowedMissCountButton.label.text = txt;
                    SaveConfig();
                }),
                null,
                "Sets the allowed amount of misses before restarting");
            #endregion
            #region Behavior
            string toggleTextBehavior = highscoreMode ? "Mode: Highscore" : "Mode: Standard";
            toggleButtonBehavior = optionMenu.AddButton
                (1,
                toggleTextBehavior,
                new Action(() =>
                {
                    highscoreMode = !highscoreMode;
                    string txt = highscoreMode ? "Mode: Highscore" : "Mode: Standard";
                    SaveConfig();

                    toggleButtonBehavior.label.text = txt;
                    if (behaviorButtonCreated) behaviorButton.label.text = txt;
                }),
                null,
                "Highscore fails the song after not being able to beat your current highscore. Standard will fail after a set amount of misses");
            #endregion
            #region Quick Buttons
            string toggleTextQuickButtons = quickButtons ? "Quick Buttons ON" : "Quick Buttons OFF";
            toggleButtonQuickButtons = optionMenu.AddButton
                (0,
                toggleTextQuickButtons,
                new Action(() =>
                {
                    quickButtons = !quickButtons;
                    toggleButtonQuickButtons.label.text = quickButtons ? "Quick Buttons ON" : "Quick Buttons OFF";
                    SaveConfig();                   
                }),
                null,
                "Enables Quick Buttons for Auto Skip, Grind Mode, Allowed Misses and ModeSwitch before starting a song");
            #endregion
            */
            menuSpawned = true;
        }

        /*
        public static void CreateModMenu()
        {
            MelonModLogger.Log("Hello");
            ModMenu.ModPage page = new ModMenu.ModPage("Grind Mode", "Helps you grind scores by automatically restarting a song according to your settings.");
            page.AddHeader("Intro Skip");
            ModMenu.ModButton isb = new ModMenu.ModButton(introSkip ? "ON" : "OFF", introSkip ? "ON" : "OFF", "Allows you to skip song intros", OnIntroButtonShotEventHandler);
            page.AddButton(isb);
            ModMenu.RegisterModPage(page);
        }
        */
        public static void OnIntroButtonShotEventHandler()
        {
            //GrindMode.introSkip = !GrindMode.introSkip;
            //GrindMode.toggleButtonIntro.label.text = GrindMode.introSkip ? "ON" : "OFF";
            //GrindMode.SaveConfig();
        }

        public static void SetHighscore(int _highscore)
        {
            highscoreIsSetup = true;
            highscore = _highscore;
        }

        public static void SetCues(SongCues.Cue[] cues)
        {
            if (cuesSet) return;
            cuesSet = true;
            songCues = new List<SongCues.Cue>(cues);
        }

        public static void SetCurrentScore(int score, int streak, int multiplier, SongCues.Cue cue, bool miss = false)
        {
            if (waitForRestart) return;
            if (cue.behavior == Target.TargetBehavior.Chain)
            {
                lastTarget = cue;
                if(cue.nextCue.behavior == Target.TargetBehavior.Chain)
                {
                    return;
                }
                else
                {
                    if (chainLH && cue.handType == Target.TargetHandType.Left) chainLH = false;
                    else if (chainRH && cue.handType == Target.TargetHandType.Right) chainRH = false;
                    RemoveChainCues(cue.handType);
                }
            }

            if (cue.behavior == Target.TargetBehavior.ChainStart)
            {

                if (cue.handType == Target.TargetHandType.Left)
                {
                    chainLH = true;
                }
                else
                {
                    chainRH = true;
                }
                    
            }

           
            lastTarget = cue;
            
            int length = songCues.Count - 1;
            if (chainLH)
            {
                int index = 0;
                for (int i = 0; i < length; i++)
                {
                    SongCues.Cue c = songCues[i];
                    if (c.behavior == Target.TargetBehavior.Chain && c.handType == Target.TargetHandType.Left) index++;
                    else
                    {                      
                        songCues.RemoveAt(index);
                        break;
                    }
                }
            }
            else if (chainRH)
            {
                int index = 0;
                for (int i = 0; i < length; i++)
                {
                    SongCues.Cue c = songCues[i];
                    if (c.behavior == Target.TargetBehavior.Chain && c.handType == Target.TargetHandType.Right) index++;
                    else
                    {
                        songCues.RemoveAt(index);
                        break;
                    }
                }
            }
            else
            {
                if(cue.behavior != Target.TargetBehavior.Chain) songCues.RemoveAt(0);
            }


            currentScore = score;
            currentMultiplier = multiplier;
            currentStreak = streak;
            
            CalculateMaxPossibleScore();
        }

        private static void RemoveChainCues(Target.TargetHandType handType)
        {      
            int length = songCues.Count - 1;
            int index = 0;
            for(int i = 0; i < length; i++)
            {
                SongCues.Cue cue = songCues[index];
                if(cue.handType == handType)
                {
                    if (cue.behavior == Target.TargetBehavior.Chain)
                    {
                        songCues.RemoveAt(index);
                    }
                    else
                    {
                        return;
                    }
                        
                }
                else
                {
                    index++;
                }
            }
           
        }

        private static void CalculateMaxPossibleScore(bool debug = false)
        {
            int theoreticalStreak = currentStreak;
            int theoreticalMaxScore = 0;
            int theoreticalMultiplier = currentMultiplier;
            foreach(SongCues.Cue cue in songCues)
            {
                int score = 0;
                Target.TargetBehavior behavior = cue.behavior;

                if (sustainTickRH > 0)
                {
                    if (cue.tick >= sustainTickRH)
                    {
                        sustainTickRH = 0;
                        theoreticalMaxScore += (3000 * theoreticalMultiplier);
                    }

                }
                if (sustainTickLH > 0)
                {
                    if (cue.tick >= sustainTickLH)
                    {
                        sustainTickLH = 0;
                        theoreticalMaxScore += (3000 * theoreticalMultiplier);
                    }

                }

                if (behavior != Target.TargetBehavior.Chain)
                {
                    theoreticalStreak += 1;
                    if (theoreticalMultiplier < 4)
                    {
                        float mult = (theoreticalStreak / 10f);
                        if ((mult % 1) == 0) theoreticalMultiplier += 1;
                    }
                }
                
                switch (behavior)
                {
                    case Target.TargetBehavior.Hold:
                        if (cue.handType == Target.TargetHandType.Right) sustainTickRH = cue.tick + cue.tickLength;
                        else sustainTickLH = cue.tick + cue.tickLength;
                        break;
                    case Target.TargetBehavior.Chain:
                        score = 125;
                        break;
                    default:
                        score = 2000;
                        break;
                }

                score *= theoreticalMultiplier;
                theoreticalMaxScore += score;

            }
            if (debug)
            {
                int sc = currentScore + theoreticalMaxScore;
                MelonModLogger.Log("Max calculated score: " + sc.ToString());
                MelonModLogger.Log("Real max score: " + StarThresholds.I.GetMaxRawScore(SongDataHolder.I.songData.songID, KataConfig.Difficulty.Expert).ToString());
            }

            if (currentScore + theoreticalMaxScore < highscore)
            {
                RestartSong();
            }    
        }

        public override void OnUpdate()
        {

            //if (MenuState.sState != MenuState.State.Launched) return;
            //decide if we are currently playing a song
            if (!isPlaying && MenuState.sState == MenuState.State.Launched && AudioDriver.I is AudioDriver)
            {
                isPlaying = true;
            }

            else if (isPlaying && MenuState.sState != MenuState.State.Launched)
            {
                isPlaying = false;
            }

            if (isPlaying)
            {
                IntroHandling();                        
            }
            
        }
        //decides if we are able to skip
        private static void IntroHandling()
        {

            if (!skipQueued && !introSkipped && GetCurrentTick() < GetFirstTick() - 5760)
            {
                if(!canSkip) canSkip = true;
                if (!autoSkip && !isPaused && !skipIntroButton.activeSelf) SetIntroSkipButtonActive(true);
            }
            else
            {
                if(canSkip) canSkip = false;
                if (skipIntroButton.activeSelf) SetIntroSkipButtonActive(false);
            }
        }

        public static void DontRecordRestart()
        {
            recordRestarted = false;
        }

        public static void ResetVariables()
        {
            introSkipped = false;
            skipQueued = false;
            canSkip = true;
            missCount = 0;
            cachedFirstTick = 0;
            chainLH = false;
            chainRH = false;
            isPaused = false;

            highscore = 0;
            highscoreIsSetup = false;
            currentScore = 0;
            currentStreak = 0;
            currentMultiplier = 0;
            cuesSet = false;

            reportedCues.Clear();
        }

       
    }
}



