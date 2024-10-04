using HarmonyLib;
using KinematicCharacterController;
using MelonLoader;
using NeonLite.Modules.Optimization;
using System.Reflection;
using TMPro;
using UnityEngine;

namespace NeonLite.Modules.UI
{
    internal class DNFTime : IModule
    {
#pragma warning disable CS0414
        const bool priority = true;
        static bool active = false;

        static bool hit = false;
        static MelonPreferences_Entry<string> sfxSetting;
        static GameObject frozenTime;

        static void Setup()
        {
            var setting = Settings.Add(Settings.h, "UI/In-game", "dnf", "Show DNFs", "Shows the time you would have got if you had killed all the demons in a level.", true);
            sfxSetting = Settings.Add(Settings.h, "UI/In-game", "dnfSound", "DNF Sound FX", "The sound to play when you DNF.\nBlank to disable.", "UI_CUTIN_IN");

            setting.OnEntryValueChanged.Subscribe((_, after) => Activate(after));
            active = setting.Value;
        }

        static readonly MethodInfo ogonsty = AccessTools.Method(typeof(LevelGate), "OnTriggerStay");
        static readonly MethodInfo ogstart = AccessTools.Method(typeof(LevelGate), "Start");
        static readonly MethodInfo ogedie = AccessTools.Method(typeof(Enemy), "Die");
        static readonly MethodInfo ogefdie = AccessTools.Method(typeof(Enemy), "ForceDie");

        static void Activate(bool activate)
        {
            if (activate)
            {
                NeonLite.Harmony.Patch(ogonsty, postfix: Helpers.HM(OnTrigger));
                NeonLite.Harmony.Patch(ogstart, postfix: Helpers.HM(PostStart));
                NeonLite.Harmony.Patch(ogedie, postfix: Helpers.HM(OnEnemyDie));
                NeonLite.Harmony.Patch(ogefdie, postfix: Helpers.HM(OnEnemyDie));
            }
            else
            {
                NeonLite.Harmony.Unpatch(ogonsty, Helpers.MI(OnTrigger));
                NeonLite.Harmony.Unpatch(ogstart, Helpers.MI(PostStart));
                NeonLite.Harmony.Unpatch(ogedie, Helpers.MI(OnEnemyDie));
                NeonLite.Harmony.Unpatch(ogefdie, Helpers.MI(OnEnemyDie));
            }

            active = activate;
        }

        static void OnLevelLoad(LevelData _) => hit = false;

        static void OnTrigger(LevelGate __instance, Collider other)
        {
            if (__instance.Unlocked || hit || (LevelRush.IsLevelRush() && LevelRush.GetCurrentLevelRush().randomizedIndex.Length - 1 != LevelRush.GetCurrentLevelRush().currentLevelIndex))
                return;

            if (NeonLite.DEBUG)
                NeonLite.Logger.Msg(other);

            hit = true;
            frozenTime = UnityEngine.Object.Instantiate(RM.ui.timerText.gameObject, RM.ui.timerText.transform);
            frozenTime.transform.localPosition += new Vector3(0, 35, 0);
            Game game = NeonLite.Game;
            long best = GameDataManager.levelStats[game.GetCurrentLevel().levelID].GetTimeBestMicroseconds();
            TextMeshPro frozenText = frozenTime.GetComponent<TextMeshPro>();
            var time = EnsureTimer.CalculateOffset(EnsureTimer.cOverride ?? __instance.GetComponentInChildren<MeshCollider>());
            frozenText.color = best < time ? Color.red : Color.green;
            var local = Localization.Setup(frozenText);
            local.SetKey("NeonLite/DNF", [new("{0}", Helpers.FormatTime(time / 1000, null), false)]);
            if (!string.IsNullOrEmpty(sfxSetting.Value))
                AudioController.Play(sfxSetting.Value);
        }

        static void OnEnemyDie()
        {
            UnityEngine.Object.Destroy(frozenTime);
            hit = false;
        }

        static void PostStart(LevelGate __instance)
        {
            // dnfs were actually using the entire rigidbody which is a bit taller and a tiny bit wider
            // this made dnfs fake, **ESPECIALLY** vertical DNFs 
            // shoutout mario/snowy/wolfu/floyd for pointing it out and letting me test
            // according to unity docs this doesn't even seem like it should happen,
            // so we have to add a rigidbody to redirect it
            // testing was done to make absolutely sure that this doesn't affect anything
            //__instance._collider.gameObject.SetActive(false);
            var rb = __instance._collider.GetOrAddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.detectCollisions = true;
            rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
            rb.useGravity = false;
            rb.freezeRotation = true;
            rb.constraints = RigidbodyConstraints.FreezeAll;
        }
    }
}
