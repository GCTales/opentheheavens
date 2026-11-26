using BepInEx;
using BepInEx.Configuration;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using R2API;
using R2API.Utils;
using RoR2;
using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Diagnostics;
using UnityEngine.Networking;
using System.Security;
using System.Security.Permissions;

[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]

//Reminder: Don't call debug/info log methods outside of Debug
//Reminder: Try not to add methods to OpenTheHeavens. Most everything in this class should exclusively call methods of other classes
/*
Next: Figure out the life hand pickuppicker to grant 2/2/1 choices instead of 3/3/3
Next: Make Life hand give proper rewards: 0 -> 1/2w -> 1/2w + 1/2w ->1/2w + 1/2g -> 1/2w + 1/2g + 1h
Next: Risk Of Options support
Next: ProperSave support
Next: Cleanup on OpenTheHeavens.cs

Sometime: should have figured out SB-T music by this point, probs a separate mod
Sometime???: fix the concommand to see if the event in ConfigManager works

Later: Need to decide if Imperishables should be standalone or to develop it as part of this mod. prob standalone but i need that damn recycler rework here


Notetaking: [Redacted: Boss Stage Reworks]

Notetaking: Could remove newt altars and lock behind Artifact of Discontinuity
Notetaking: Newt would be a pain in the ass to balance, esp. when trying to enforce red scarcity.
Notetaking: Would lock out Vields tho. Not a huge loss but it should probably go somewhere else too, like appending to void seeds.
Remark: don't run with that idea yet.
Remark: maybe do actually its not that deep
Remark: I KNEW THERE WAS A REASON I SAID NOT TO
*/

namespace Open_The_Heavens
{

    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    public class OpenTheHeavens : BaseUnityPlugin
    {
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "GreatCookieTales";
        public const string PluginName = "opentheheavens";
        public const string PluginVersion = "0.0.0";


        public void Awake()
        {
            Log.Init(Logger);
            ConfigManager.Init();
            HexTweaks.Init();
        }


        public void Start()
        {
            HexTweaks.LoadAssets();
        }


        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F3)|| Input.GetKeyDown(KeyCode.F4))
            {
                if (Input.GetKeyDown(KeyCode.F4))
                {
                    RunTracker.teamCleansedHexes = 0;
                    Log.Info("Reset Cleansed Hexes: " + RunTracker.teamCleansedHexes);
                } else
                {
                    RunTracker.teamCleansedHexes++;
                    Log.Info("Increased Cleansed Hexes " + RunTracker.teamCleansedHexes);
                }
                Log.LogChat(Log.LogTypes.Debug, "Hexes Cleansed set to" + RunTracker.teamCleansedHexes);
            }

        }

    }
}
