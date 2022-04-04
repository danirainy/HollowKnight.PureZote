﻿using Modding;
using System.Collections.Generic;
using UnityEngine;
using Core.FsmUtil;


namespace PureZote
{
    public class Settings
    {
        public bool enabled = true;
    }
    public class PureZote : Mod, IGlobalSettings<Settings>
    {
        private Settings settings_ = new();
        private readonly List<GameObject> minionPrefabs = new();
        private readonly System.Random random = new();
        public PureZote() : base("PureZote") { }
        public void OnLoadGlobal(Settings settings) => settings_ = settings;
        public Settings OnSaveGlobal() => settings_;
        public override string GetVersion() => "1.0";
        public override List<(string, string)> GetPreloadNames()
        {
            return new List<(string, string)>
            {
                 ("GG_Mighty_Zote","Battle Control"),
            };
        }
        private void LoadMinionPrefabs(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
        {
            var battleControl = preloadedObjects["GG_Mighty_Zote"]["Battle Control"];
            var names = new List<(string, string)>
            {
                ("Tall Zotes","Zote Crew Tall"),
            };
            foreach ((string group, string instance) in names)
            {
                var minion = battleControl.transform.Find(group).gameObject.transform.Find(instance).gameObject;
                Object.Destroy(minion.GetComponent<PersistentBoolItem>());
                minionPrefabs.Add(minion);
                Log("Minion added: " + minion.name + ".");
            }
        }
        public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
        {
            Log("Initializing.");
            if (settings_.enabled)
            {
                Log("Enabled.");
            }
            else
            {
                Log("Disabled.");
            }
            On.PlayMakerFSM.OnEnable += PlayMakerFSMOnEnable;
            LoadMinionPrefabs(preloadedObjects);
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += ActiveSceneChanged;
            ModHooks.HeroUpdateHook += HeroUpdateHook;
            Log("Initialized.");
        }
        private void PlayMakerFSMOnEnable(On.PlayMakerFSM.orig_OnEnable original, PlayMakerFSM fsm)
        {
            original(fsm);
            if (!settings_.enabled)
            {
                return;
            }
            if (fsm.gameObject.scene.name == "GG_Grey_Prince_Zote" && fsm.gameObject.name == "Grey Prince" && fsm.FsmName == "Control")
            {
                Log("Upgrading FSM: " + fsm.gameObject.name + " - " + fsm.FsmName + ".");
                FsmUtil.RemoveAction(fsm, "Spit L", 7);
                FsmUtil.AddMethod(fsm, "Spit L", Spit, fsm);
                FsmUtil.RemoveAction(fsm, "Spit R", 7);
                FsmUtil.AddMethod(fsm, "Spit R", Spit, fsm);
                Log("Upgraded FSM: " + fsm.gameObject.name + " - " + fsm.FsmName + ".");
            }
            else if (fsm.gameObject.scene.name == "GG_Grey_Prince_Zote" && fsm.gameObject.name == "Zote Crew Tall(Clone)" && fsm.FsmName == "Control")
            {
                Log("Upgrading FSM: " + fsm.gameObject.name + " - " + fsm.FsmName + ".");
                FsmUtil.AddFsmTransition(fsm, "Dormant", "FINISHED", "Multiply");
                for (int i = 0; i < 2; ++i)
                    FsmUtil.RemoveAction(fsm, "Spawn Antic", 0);
                Log("Upgraded FSM: " + fsm.gameObject.name + " - " + fsm.FsmName + ".");
            }
        }
        private void Spit(PlayMakerFSM fsm)
        {
            Log("Spitting.");
            var zoteling = FsmUtil.FindFsmGameObjectVariable(fsm, "Zoteling").Value;
            var index = random.Next(minionPrefabs.Count);
            var minion = Object.Instantiate(minionPrefabs[index]);
            minion.SetActive(true);
            minion.SetActiveChildren(true);
            minion.GetComponent<HealthManager>().hp = 52;
            minion.transform.position = zoteling.transform.position;
            Log("Spat.");
        }
        private void ActiveSceneChanged(UnityEngine.SceneManagement.Scene from, UnityEngine.SceneManagement.Scene to)
        {
            Log("Scene loaded: " + to.name + ".");
        }
        private void HeroUpdateHook()
        {
        }
    }
}