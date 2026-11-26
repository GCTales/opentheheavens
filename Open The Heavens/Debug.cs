using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Open_The_Heavens
{
    internal static class Debug
    {
        internal static void Init()
        {
            On.RoR2.Run.RecalculateDifficultyCoefficent += Run_RecalculateDifficultyCoefficent;
            Run.onRunStartGlobal += Run_onRunStartGlobal;
            Run.onRunAmbientLevelUp += Run_onRunAmbientLevelUp;
        }


        private static void Run_onRunStartGlobal(Run obj)
        {
            Log.Info("Called onRunStartGlobal with name \"" + Run.instance.name + "\"");
            Log.Info("Getting Run.instance.selectedDifficulty: \"" + Run.instance.selectedDifficulty + "\"");
            RunTracker.Init();
        }

        private static void Run_onRunAmbientLevelUp(Run obj)
        {
            Log.Info("Ambient Level set to " + Run.instance.ambientLevelFloor);
            Log.LogChat(Log.LogTypes.Info, "Ambient Level Up: " + Run.instance.ambientLevelFloor);
        }

        private static void Run_RecalculateDifficultyCoefficent(On.RoR2.Run.orig_RecalculateDifficultyCoefficent orig, Run self)
        {
            orig(self);
            float diffCoeffTruncOneDP = Mathf.Floor(Run.instance.difficultyCoefficient * 10) / 10;
            if (RunTracker.debugDiffCoeff != diffCoeffTruncOneDP)
            {
                Log.Info("DiffCoeff now at " + diffCoeffTruncOneDP);
                Log.LogChat(Log.LogTypes.Debug, "DiffCoeff now at " + diffCoeffTruncOneDP);
                RunTracker.debugDiffCoeff = diffCoeffTruncOneDP;
            }
        }

    }
}
