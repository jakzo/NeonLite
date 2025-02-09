﻿#if !XBOX
using System.Reflection;
using HarmonyLib;
using MelonLoader;

namespace NeonLite.Modules.Optimization
{
    internal class UpdateGlobal : IModule
    {
#pragma warning disable CS0414
        const bool priority = false;
        static bool active = false;

        static bool ready;
        static bool popupPrepped;
        static bool titleDone;

        static MelonPreferences_Entry<bool> setting;
        static MelonPreferences_Entry<bool> popup;

        static void Setup()
        {
            setting = Settings.Add("NeonLite", "Misc", "updateGlobal", "Auto-update Global", "Updates your Global Neon Rank the instant you PB a stage.", true);
            popup = Settings.Add("NeonLite", "Misc", "updateGlobalP", "Auto-update Global Popup", null, true);
            popup.IsHidden = true;
            setting.OnEntryValueChanged.Subscribe((_, after) => Activate(after));
            active = setting.Value;
        }

        static readonly MethodInfo oglbupld = AccessTools.Method(typeof(Leaderboards), "OnLeaderboardUploaded");
        static readonly MethodInfo original = AccessTools.Method(typeof(LeaderboardIntegrationSteam), "SetupLeaderboardForLevel");
        static readonly MethodInfo ogtlvis = AccessTools.Method(typeof(MenuScreenTitle), "OnSetVisible");
        static readonly MethodInfo oggdgn = AccessTools.Method(typeof(GameData), "GetGlobalNeonScore");

        static void Activate(bool activate)
        {
            if (activate)
            {
                if (titleDone)
                    DoPopup();
                else
                    popupPrepped = true;
                NeonLite.Harmony.Patch(oglbupld, Helpers.HM(PreLBUploaded));
                NeonLite.Harmony.Patch(original, Helpers.HM(ChangeCallback));
                NeonLite.Harmony.Patch(ogtlvis, Helpers.HM(OnTitleShow));
                NeonLite.Harmony.Patch(oggdgn, Helpers.HM(NeonScoreDebug));
            }
            else
            {
                NeonLite.Harmony.Unpatch(oglbupld, Helpers.MI(PreLBUploaded));
                NeonLite.Harmony.Unpatch(original, Helpers.MI(ChangeCallback));
                NeonLite.Harmony.Unpatch(ogtlvis, Helpers.MI(OnTitleShow));
                NeonLite.Harmony.Unpatch(oggdgn, Helpers.MI(NeonScoreDebug));
            }

            active = activate;
        }

        static void PreLBUploaded() => ready = true;

        static void OnTitleShow()
        {
            if (!titleDone && popupPrepped)
                DoPopup();
            titleDone = true;
        }

        static void DoPopup()
        {
            if (!popup.Value)
            {
                MainMenu.Instance()._popup.SetPopup("NeonLite/AUTOGLOBAL_NOTICE", () =>
                {
                    setting.Value = false;
                    MelonPreferences.Save();
                }, () => { });
                popup.Value = true;
                MelonPreferences.Save();
            }
        }

        static void NeonScoreDebug(int __result) => NeonLite.Logger.Msg($"Calculated global microseconds: {__result}");

        static bool ChangeCallback(int ___previousUserRanking, LevelData newData, Leaderboards newRef, LeaderboardIntegrationSteam.LeaderboardLoadedCallback newCallback)
        {
            if (!ready)
                return true;

            ready = false;
            LeaderboardIntegrationSteam.UploadScore_GlobalNeonRank(null, (result, _) =>
            {
                if (result)
                    NeonLite.Logger.Msg("Updated global!");
                else
                    NeonLite.Logger.Warning("Failed to update global.");

                AccessTools.Field(typeof(LeaderboardIntegrationSteam), "previousUserRanking").SetValue(null, ___previousUserRanking);
                LeaderboardIntegrationSteam.SetupLeaderboardForLevel(newData, newRef, newCallback);
            });
            return false;
        }
    }
}
#endif