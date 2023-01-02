﻿using MelonLoader;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NeonWhiteQoL
{
    public class NeonLite : MelonMod
    {
        public static new HarmonyLib.Harmony Harmony { get; private set; }

        [Obsolete]
        public override void OnApplicationLateStart()
        {
            Game game = Singleton<Game>.Instance;
            GameObject modObject = new GameObject();
            UnityEngine.Object.DontDestroyOnLoad(modObject);

            game.OnLevelLoadComplete += OnLevelLoadComplete;

            Harmony = new HarmonyLib.Harmony("NeonLite");

            int i = 0;

            PBtracker.Initialize();
            Debug.Log("Initialization " + ++i);
            GreenHP.Initialize();
            Debug.Log("Initialization " + ++i);
            SkipIntro.Initialize();
            Debug.Log("Initialization " + ++i);
            RemoveMission.Initialize();
            Debug.Log("Initialization " + ++i);
            LeaderboardFix.Initialize();
            Debug.Log("Initialization " + ++i);
            CommunityMedals.Initialize();
            Debug.Log("Initialization " + ++i);
            ShowcaseBypass.Initialize();
            Debug.Log("Initialization " + ++i);
            IGTimer.Initialize();
            Debug.Log("Initialization " + ++i);
            BegoneApocalypse.Initialize();
            Debug.Log("Initialization " + ++i);
            BossfightGhost.Initialize();
            //GameObject text = new GameObject("Text", typeof(Text));
            GameObject timer = new GameObject("SessionTimer", typeof(SessionTimer));
            modObject.AddComponent<CheaterBanlist>();
            Debug.Log("Initialization " + ++i);

            Debug.Log("Initialization complete.");
        }

        private void OnLevelLoadComplete()
        {
            if (SceneManager.GetActiveScene().name.Equals("Heaven_Environment"))
                return;

            GameObject.Find("HUD").AddComponent<HUDManager>();
        }

        //public override void OnUpdate()
        //{
        //    if (!Keyboard.current.hKey.wasPressedThisFrame) return;
        //    Texture2D Tex2D;
        //    byte[] FileData;
        //    string FilePath = "C:\\Users\\faust\\Desktop\\medal testing\\medal.png";

        //    if (File.Exists(FilePath))
        //    {
        //        FileData = File.ReadAllBytes(FilePath);
        //        Tex2D = new Texture2D(2, 2);
        //        Tex2D.LoadImage(FileData);
        //        Texture2D SpriteTexture = Tex2D;
        //        CommunityMedals.emeraldMedal = Sprite.Create(SpriteTexture, new Rect(0, 0, SpriteTexture.width, SpriteTexture.height), new Vector2(0, 0), 100f);
        //    }
        //}

        public static MelonPreferences_Category neonLite_config;
        public static MelonPreferences_Entry<bool> CommunityMedals_enable;
        public static MelonPreferences_Entry<bool> PBtracker_display;
        public static MelonPreferences_Entry<bool> SessionTimer_display;
        //public static MelonPreferences_Entry<int> SessionTimer_fontSize;
       // public static MelonPreferences_Entry<Color> SessionTimer_color;
        public static MelonPreferences_Entry<bool> LevelTimer_display;
        //public static MelonPreferences_Entry<Color> LevelTimer_color;
        //public static MelonPreferences_Entry<int> LevelTimer_fontSize;
        //public static MelonPreferences_Entry<Vector3> LevelTimer_coords;
        public static MelonPreferences_Entry<bool> IGTimer_display;
        public static MelonPreferences_Entry<Color> IGTimer_color;
        public static MelonPreferences_Entry<bool> RemoveMission_display;
        public static MelonPreferences_Entry<bool> GreenHP_display;
        public static MelonPreferences_Entry<bool> Apocalypse_display;
        public static MelonPreferences_Entry<bool> InsightScreen_enable;
        public static MelonPreferences_Entry<bool> BossGhost_recorder;

        public static MelonPreferences_Category neonLite_visuals;
        public static MelonPreferences_Entry<bool> playerUIportrait_display;
        public static MelonPreferences_Entry<bool> backstory_display;
        public static MelonPreferences_Entry<bool> bottombar_display;
        public static MelonPreferences_Entry<bool> damageOverlay_display;
        public static MelonPreferences_Entry<bool> boostOverlay_display;
        public static MelonPreferences_Entry<bool> shockerOverlay_display;
        public static MelonPreferences_Entry<bool> telefragOverlay_display;
        public static MelonPreferences_Entry<bool> uiScreenFader_display;

        //add customization options for session timer/level timer (this bugs with the whole mod so i'll figure this out later)
        //allow people to customize PB tracker color(?), ext fov slider custom values(?) and/or toggle

        [Obsolete]
        public override void OnApplicationStart()
        {
            neonLite_config = MelonPreferences.CreateCategory("NeonLite Settings");
            CommunityMedals_enable = neonLite_config.CreateEntry("Enable Community Medals", true, description: "Enables Custom Community Medals that change sprites in the game.");
            PBtracker_display = neonLite_config.CreateEntry("Enable PB Tracker", true, description: "Displays a time based on whether or not you got a new personal best.");
            SessionTimer_display = neonLite_config.CreateEntry("Display Session Timer", true, description: "Tracks your current play session time.");
            //SessionTimer_color = neonLite_config.CreateEntry("Session Timer Color", Color.white, description: "Customization settings for the Session Timer.");
            //SessionTimer_fontSize = neonLite_config.CreateEntry("Session Timer Font Size", 20, description: "Enter a value to set your font size (Must not include decimals).");
            LevelTimer_display = neonLite_config.CreateEntry("Display Level Timer", true, description: "Tracks the time you've spent on the current level you're playing.");
            //LevelTimer_color = neonLite_config.CreateEntry("Session Timer Color", Color.white, description: "Customization settings for the Level Timer.");
            //LevelTimer_fontSize = neonLite_config.CreateEntry("Level Timer Font Size", 20, description: "Enter a value to set your font size (Must not include decimals).");
            //LevelTimer_coords = neonLite_config.CreateEntry("Level Timer Position", new Vector3(-5, -30, -0), description: "Enter a value to move the Level Timer.");
            IGTimer_display = neonLite_config.CreateEntry("Display in-depth in-game timer", true, description: "Allows the modification of the timer and lets you display milliseconds.");
            IGTimer_color = neonLite_config.CreateEntry("In-game Timer Color", Color.white, description: "Customization settings for the in-game timer, does not apply to result screen time.");
            GreenHP_display = neonLite_config.CreateEntry("Enable Neon Green HP", true, description: "Displays the HP of Neon Green in Text Form.");
            RemoveMission_display = neonLite_config.CreateEntry("Remove Start Mission button in Job Archive", true, description: "Sick and tired of the big, bulky \"Start Mission\" button that appears? Now you can get rid of it, forever!");
            InsightScreen_enable = neonLite_config.CreateEntry("Insight Screen Remover", true, description: "No longer displays the \"Insight Crystal Dust (Empty)\" screen after finishing a sidequest level.");
            Apocalypse_display = neonLite_config.CreateEntry("Begone Apocalypse", true, description: "Get rid of the Apocalyptic view and replace it with the blue skies.");
            BossGhost_recorder = neonLite_config.CreateEntry("Boss Recorder", true, description: "Allows you to record and playback a ghost for the boss levels.");

            neonLite_visuals = MelonPreferences.CreateCategory("NeonLite Visual Settings");
            playerUIportrait_display = neonLite_visuals.CreateEntry("Disable Player's portrait", false);
            backstory_display = neonLite_visuals.CreateEntry("Disable backstory", false);
            bottombar_display = neonLite_visuals.CreateEntry("Disable bottom bar", false, description: "Removes the bottom black bar that appears.");
            damageOverlay_display = neonLite_visuals.CreateEntry("Disable low HP overlay", false, description: "Removes the overlay around your screen when you're at 1 hp.");
            boostOverlay_display = neonLite_visuals.CreateEntry("Disable boost overlay", false, description: "Removes the overlay around your screen when you are getting a speed boost.");
            shockerOverlay_display = neonLite_visuals.CreateEntry("Disable shocker overlay", false, description: "Removes the small white flash around your screen when using a shocker.");
            telefragOverlay_display = neonLite_visuals.CreateEntry("Disable book of life overlay", false, description: "Removes the overlay around your screen when using the book of life.");
            uiScreenFader_display = neonLite_visuals.CreateEntry("Disable white screen fader", false, description: "Use in combination with shocker/book of life overlay!");
        }
    }
}
