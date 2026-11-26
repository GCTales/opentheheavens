using BepInEx;
using BepInEx.Configuration;
using RoR2;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace Open_The_Heavens
{
    public static class ConfigManager
    {
        private const string reloadText = "\nChanging this setting necessitates a game restart";
        private static ConfigFile GameplayConfig;
        public static ConfigEntry<bool> EnableHexAmbientLevel;
        public static ConfigEntry<float> HexLevelFactor;
        public static ConfigEntry<bool> EnableForceHexPickupGlobal;
        public static ConfigEntry<bool> EnableForceHexPickupCauldron;
        public static ConfigEntry<bool> EnableForceHexPickupReliquary;
        public static ConfigEntry<bool> DisableSaleStarReliquary;
        public static ConfigEntry<bool> EnableTweakedCauldronPickups;

        //private static ConfigFile ItemConfig;


        //private static ConfigFile EnemyConfig;


        private const string folderName = "OpenTheHeavensConfig";
        private static string configPath { get => System.IO.Path.Combine(Paths.ConfigPath, folderName); }
        internal static void Init()
        {
            if (!Directory.Exists(configPath))
            {
                Directory.CreateDirectory(configPath);
            }
            WriteGameplayConfig();
            WriteItemConfig();
            WriteEnemyConfig();
            GameplayConfig.SettingChanged += GameplayConfig_SettingChanged;
        }

        private static void GameplayConfig_SettingChanged(object sender, SettingChangedEventArgs e)
        {
            HexTweaks.ReadConfig_Gameplay();
        }

        private static void WriteGameplayConfig()
        {
            GameplayConfig = new ConfigFile(System.IO.Path.Combine(configPath, "othgameplay.cfg"), true);
            EnableHexAmbientLevel = GameplayConfig.Bind<bool>("Hexes", "Enable Hex Ambient Level Increase", true, "Whether cleansing Hexes increases ambient (enemy) level." + reloadText);
            HexLevelFactor = GameplayConfig.Bind<float>("Hexes", "Cleansed Hex Ambient Level Increase", 1.5f, "How many Ambient Levels should be gained per Hex cleansed. (Final level rounds up, so by default the first Hex gives 2 levels, the next gives 1 etc).");
            EnableForceHexPickupGlobal = GameplayConfig.Bind<bool>("Hexes", "Enable Forcing Hex Pickup (Global)", true, "Whether to enable forcing hexes to be picked up as a mechanic. Overrides options for specific sources." + reloadText);
            EnableForceHexPickupCauldron = GameplayConfig.Bind<bool>("Hexes", "Enable Forcing Hex Pickup (Shrine)", true, "Whether hexes gained from a Hallowed Shrine should be forced into the interactor's inventory. Also prevents the Hand of Death from closing to allow each player to interact with it.\nOveridden by the global setting." + reloadText);
            EnableForceHexPickupReliquary = GameplayConfig.Bind<bool>("Hexes", "Enable Forcing Hex Pickup (Reliquary)", true, "Whether hexes gained from a Concept Reliquary should be forced into the interactor's inventory. Overriden by the global setting.");
            DisableSaleStarReliquary = GameplayConfig.Bind<bool>("Hexes", "Disallow Sale Star On Concept Reliquary", true, "Prevents Sale Star from triggering on Concept Reliquaries");
            EnableTweakedCauldronPickups = GameplayConfig.Bind<bool>("Hexes", "Enable Hallowed Shrine Reward Changes", true, "Whether to change the reward pool of the Hand of Life to only take 5 Hexes from players' inventories, and adjust the rewards to account for less total hexes availiable.");
        }
        private static void WriteItemConfig()
        {

        }
        private static void WriteEnemyConfig()
        {

        }
        //[ConCommand(commandName = "oth_reload_config",flags = ConVarFlags.None, helpText = "Reloads all config for Open The Heavens")]
        //internal static void ConfigReload(ConCommandArgs args)
        //{
        //    GameplayConfig.Reload();
        //    //ItemConfig.Reload();
        //    //EnemyConfig.Reload();
        //}
    }
}
