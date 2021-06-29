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
        static private bool isPaused = false;

        static public int cachedFirstTick = 0;
        #endregion
        #region Grind Mode
        static public bool Enabled = false;
        //static public bool includeChainSustainBreak = false;
        static public bool reportLastChainNode = false;
        //static public bool quickButtons = false;
        //static public bool highscoreMode = false;
        static public bool highscoreIsSetup = false;
        static public bool waitForRestart = false;
        static public bool skipSetScoreMiss = false;
        static public bool skipSetScoreSuccess = false;
        static public bool cuesSet = false;

        //static private bool showStats = false;
        static public bool recordRestarted = false;

        static public int missCount = 0;     
        //static public int allowedMissCount = 10;
 
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
        //public static OptionsMenuButton toggleButtonIncludeBreaks = null;
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

        static public bool grindButtonCreated => grindModeButton != null;
        static public bool autoSkipButtonCreated => autoSkipButton != null;
        static public bool allowedMissCountButtonCreated => allowedMissCountButton != null;
        static public bool behaviorButtonCreated => behaviorButton != null;
        static public bool instantRestartButtonCreated => instantRestartButton != null;
        static public bool ShowPanel = false;
        #endregion
        #region InGame Button
        static public GameObject menuButton = null;
        static private GameObject skipIntroButton = null;

        static private Vector3 skipIntroButtonPos = new Vector3(.5f, 1.3f, 1.5f);
        static private Vector3 skipintroButtonRot = new Vector3(35f, 0, 0);
        static private Vector3 skipIntroButtonScale = new Vector3(.16f, .16f, .16f);

        static public bool introSkipButtonCreated => skipIntroButton != null;
        #endregion

        public static class BuildInfo
        {
            public const string Name = "GrindMode";  // Name of the Mod.  (MUST BE SET)
            public const string Author = "Continuum"; // Author of the Mod.  (Set as null if none)
            public const string Company = null; // Company that made the Mod.  (Set as null if none)
            public const string Version = "3.0.0"; // Version of the Mod.  (MUST BE SET)
            public const string DownloadLink = null; // Download Link for the Mod.  (Set as null if none)
        }
        
        public override void OnApplicationStart()
        {
            Config.RegisterConfig();
        }

        public override void OnPreferencesSaved()
        {
            Config.OnPreferencesSaved();
        }

        public static void ReportMiss(SongCues.Cue cue)
        {
            if (Config.highscoreMode) return;
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
            if (missCount > Config.allowedMissCount)
            {
                RestartSong();
            }
               
        }

        public static void RestartSong(bool failed = false)
        {
            waitForRestart = true;
            if (!Config.showStats && !failed) 
            {
                PlaySound();
                InGameUI.I.Restart();
            }
            else if(Config.showStats)
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
            
            
            ResetVariables();
        }

        public static void RecordRestart()
        {
            recordRestarted = false;
            ScoreKeeper.I.OnRestart();
        }

        static public void GetAudioComponent()
        {
            if (audiocomponent is null) 
            {
                audiocomponent = GameObject.Find("HmxAudioEmitter (1)").GetComponent<HmxAudioEmitter>();
            }
        }

        static private void PlaySound()
        {
            KataUtil.PlayFMODEvent("event:/gameplay/overdrive_complete", audiocomponent.Get());
        }         

        public static void SetIntroSkipButtonActive(bool active)
        {
            if (!introSkipButtonCreated) return;
            if (Config.autoSkip) skipIntroButton.SetActive(false);
            else skipIntroButton.SetActive(active);
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

        public static IEnumerator CreateIntroSkipButton(bool reinstantiate = false)
        {
            if (introSkipButtonCreated) yield break;
            while (EnvironmentLoader.I.IsSwitching()) yield return new WaitForSeconds(.5f);
            //menuButton = GameObject.FindObjectOfType<LaunchPanel>().BackButton.gameObject;
            string name = "menu/ShellPage_Launch/page/backParent/back";
            menuButton = GameObject.Find(name);
            if (menuButton is null)
            {
                MelonLogger.Msg("menu button not found");
                yield break;
            }
            skipIntroButton = CreateButton(menuButton, "Skip Intro", OnSkipButtonShot, skipIntroButtonPos, skipintroButtonRot, skipIntroButtonScale);
            //skipIntroButton.hideFlags |= HideFlags.DontUnloadUnusedAsset;
            GameObject.DontDestroyOnLoad(skipIntroButton);
            SetIntroSkipButtonActive(reinstantiate);
        }
     
        //track paused state so we can disable intro button while paused
        public static void SetPaused(bool _isPaused)
        {
            isPaused = _isPaused;
            if (isPaused) SetIntroSkipButtonActive(false);
            else if (canSkip && !Config.autoSkip) SetIntroSkipButtonActive(true);
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
                MelonLogger.Msg("Max calculated score: " + sc.ToString());
                MelonLogger.Msg("Real max score: " + StarThresholds.I.GetMaxRawScore(SongDataHolder.I.songData.songID, KataConfig.Difficulty.Expert).ToString());
            }

            if (currentScore + theoreticalMaxScore < highscore)
            {
                RestartSong();
            }    
        }

        public override void OnUpdate()
        {
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
            if (!introSkipButtonCreated) return;
            if (!skipQueued && !introSkipped && GetCurrentTick() < GetFirstTick() - 5760)
            {
                if (!canSkip) canSkip = true;
                if(!isPaused && !skipIntroButton.activeSelf)
                {
                    if (!Config.autoSkip)
                    {
                        SetIntroSkipButtonActive(true);
                    }
                }
            }
            else
            {
                if (canSkip) canSkip = false;
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



