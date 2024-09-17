﻿using HarmonyLib;
using MelonLoader;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace NeonLite.Modules
{
    internal class Anticheat : IModule
    {
#pragma warning disable CS0414
        const bool priority = true;
        public static bool active = true;
        
        static bool hasSetup = false;
        static readonly HashSet<MelonAssembly> assemblies = [];

        static void Setup() => active = assemblies.Count > 0;

        static readonly MethodInfo ogutms = AccessTools.Method(typeof(LevelStats), "UpdateTimeMicroseconds");

#if XBOX
        static readonly MethodInfo oglbui = AccessTools.Method(typeof(LeaderboardIntegrationBitcode), "LoadLeaderboardAndConditionallySubmitScore");
#else
        static readonly MethodInfo oglbui = AccessTools.Method(typeof(LeaderboardIntegrationSteam), "OnFindLeaderboardForUpload");
#endif

        static void Activate(bool activate)
        {
            if (activate && (!active || !hasSetup))
            {
                NeonLite.Harmony.Patch(ogutms, prefix: Helpers.HM(DontUpdateTime));
                NeonLite.Harmony.Patch(oglbui, prefix: Helpers.HM(UploadScoreStopper));
            }
            else if (!activate && active)
            {
                NeonLite.Harmony.Unpatch(ogutms, Helpers.MI(DontUpdateTime));
                NeonLite.Harmony.Unpatch(oglbui, Helpers.MI(UploadScoreStopper));
            }

            hasSetup = true;
            active = activate;
        }

        static bool DontUpdateTime(long newTime, ref bool ____newBest, long ____timeBestMicroseconds) {
            ____newBest = newTime < ____timeBestMicroseconds;
            return false;
        }
#if XBOX
        static void UploadScoreStopper(ref bool uploadScore) => uploadScore = false;
#else
        static bool UploadScoreStopper(LeaderboardFindResult_t pCallback, bool bIOFailure, ref SteamLeaderboard_t ___currentLeaderboard)
        {
            if (pCallback.m_bLeaderboardFound != 1 || bIOFailure)
                return false;

            ___currentLeaderboard = pCallback.m_hSteamLeaderboard;
            LeaderboardScoreUploaded_t fakeUp = new()
            {
                m_bScoreChanged = 0
            };
            AccessTools.Method(typeof(LeaderboardIntegrationSteam), "OnLeaderboardScoreUploaded2").Invoke(null, [fakeUp, false]);
            return false;
        }

#endif

        public static void Register(MelonAssembly assembly)
        {
            assemblies.Add(assembly);
            if (!active || hasSetup)
                Activate(assemblies.Count > 0);
        }

        public static void Unregister(MelonAssembly assembly)
        {
            assemblies.Remove(assembly);
            if (!active || hasSetup)
                Activate(assemblies.Count > 0);
        }

    }
}
