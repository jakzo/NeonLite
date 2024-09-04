﻿using HarmonyLib;
using MelonLoader;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using UnityEngine.UI;

namespace NeonLite.Modules.Misc
{
    internal class GhostsEverywhere : IModule
    {
#pragma warning disable CS0414
        const bool priority = true;
        static bool active = false;

        static MelonPreferences_Entry<bool> setting;

        static void Setup()
        {
            setting = Settings.Add(Settings.h, "Misc", "bossGhosts", "Ghosts Everywhere", "Allows you to record, playback, and toggle ghosts for every level.", true);
            setting.OnEntryValueChanged.Subscribe((_, after) => Activate(after));
            active = setting.Value;
        }

        static readonly MethodInfo record = AccessTools.Method(typeof(GhostRecorder), "Start");
        static readonly MethodInfo playback = AccessTools.Method(typeof(GhostPlayback), "Start");
        static readonly MethodInfo oglvli = AccessTools.Method(typeof(LevelInfo), "SetLevel");
        static readonly MethodInfo oginsl = AccessTools.Method(typeof(InsightInfo), "SetLevel");
        static readonly MethodInfo oginsu = AccessTools.Method(typeof(InsightInfo), "Update");


        static void Activate(bool activate)
        {
            active = activate;

            if (activate)
            {
                NeonLite.Harmony.Patch(record, postfix: Helpers.HM(PostStart));
                NeonLite.Harmony.Patch(playback, prefix: Helpers.HM(PreStart));
                NeonLite.Harmony.Patch(oglvli, postfix: Helpers.HM(PostSetLevel));
                NeonLite.Harmony.Patch(oginsl, postfix: Helpers.HM(PostSetLevelInsight));
                NeonLite.Harmony.Patch(oginsu, transpiler: Helpers.HM(MidUpdateInsight));
            }
            else
            {
                foreach (var li in UnityEngine.Object.FindObjectsOfType<LevelInfo>())
                    PostSetLevel(li, null);

                NeonLite.Harmony.Unpatch(record, Helpers.MI(PostStart));
                NeonLite.Harmony.Unpatch(playback, Helpers.MI(PreStart));
                NeonLite.Harmony.Unpatch(oglvli, Helpers.MI(PostSetLevel));
                NeonLite.Harmony.Unpatch(oginsl, Helpers.MI(PostSetLevelInsight));
                NeonLite.Harmony.Unpatch(oginsu, Helpers.MI(MidUpdateInsight));
            }
        }

        static void PostStart(ref bool ___m_dontRecord) => ___m_dontRecord = false;
        static bool PreStart()
        {
            // i tried to simplify it but    this doesn't do anything??
            if (NeonLite.Game.GetCurrentLevel().isBossFight || LevelRush.IsHellRush())
                return setting.Value;
            return true;
        }

        static void PostSetLevel(LevelInfo __instance, LevelData level)
        {
            var stats = GameDataManager.GetLevelStats(level?.levelID ?? "");

            if (level && level.isSidequest && stats.GetInsightLevel() > 0)
            {
                __instance._emptyFrameFiller.SetActive(false);
                __instance._insightAniamtor.gameObject.SetActive(true);
            }

            bool showing = level && CommunityMedals.Ready && CommunityMedals.medalTimes.ContainsKey(level.levelID) &&
                (!CommunityMedals.oldStyle.Value ||
                (CommunityMedals.GetMedalIndex(level.levelID, stats._timeBestMicroseconds) >= (int)CommunityMedals.MedalEnum.Emerald));

            Image[] dotteds = __instance._insightAniamtor.GetComponentsInChildren<Image>();
            dotteds[0].enabled = !active || !level.isSidequest || (stats.GetCompleted() && CommunityMedals.setting.Value && showing);
            dotteds[1].enabled = !active || !level.isSidequest || (stats.GetCompleted() && CommunityMedals.setting.Value && showing);
        }
        static void PostSetLevelInsight(InsightInfo __instance, LevelData levelData_Input)
        {
            if (levelData_Input.isBossFight)
            {
                var stats = GameDataManager.GetLevelStats(levelData_Input.levelID);
                int num = Mathf.FloorToInt(stats._insightXp / (float)LevelStats.TRYS_PERLEVEL);
                __instance.personalGhostToggle.interactable = num >= 2;
                __instance.personalGhostToggle.SetIsOnWithoutNotify(GameDataManager.saveData._seeGhosts && num >= 2);
                if (num >= 2)
                    __instance.ghostText_Localized.SetKey("Interface/INSIGHT_GHOST");
                else
                    __instance.ghostText_Localized.SetKey("Interface/INSIGHT_INSIGHTWO");
            }
        }

        static IEnumerable<CodeInstruction> MidUpdateInsight(IEnumerable<CodeInstruction> instructions)
        {
            // i could just override update but this is faster
            int hit = 0;
            foreach (var code in instructions)
            {
                if (code.labels.Count > 0 && ++hit == 2)
                {
                    yield return new CodeInstruction(OpCodes.Ret).MoveLabelsFrom(code);
                    yield break;
                }

                yield return code;
            }
        }
    }
}
