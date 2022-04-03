using Modding;
using System.Collections.Generic;
using UnityEngine;
using Core.FsmUtil;


namespace Pure_Zote
{
    public class Pure_Zote : Mod
    {
        private GameObject minionTemplate;
        public Pure_Zote() : base("Pure Zote") { }
        public override string GetVersion() => "1.0";
        public override List<(string, string)> GetPreloadNames()
        {
            return new List<(string, string)>
            {
                ("Deepnest_East_07", "Super Spitter")
            };
        }
        public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
        {
            Log("Initializing.");
            On.PlayMakerFSM.OnEnable += PlayMakerFSMOnEnable;
            minionTemplate = preloadedObjects["Deepnest_East_07"]["Super Spitter"];
            UnityEngine.Object.Destroy(minionTemplate.GetComponent<PersistentBoolItem>());
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += ActiveSceneChanged;
            ModHooks.HeroUpdateHook += HeroUpdateHook;
            Log("Initialized.");
        }
        private void PlayMakerFSMOnEnable(On.PlayMakerFSM.orig_OnEnable original, PlayMakerFSM fsm)
        {
            original(fsm);
            if (fsm.gameObject.scene.name == "GG_Grey_Prince_Zote" && fsm.gameObject.name == "Grey Prince" && fsm.FsmName == "Control")
            {
                Log("Upgrading FSM.");
                FsmUtil.RemoveAction(fsm, "Spit L", 7);
                FsmUtil.AddMethod(fsm, "Spit L", Spit, fsm);
                FsmUtil.RemoveAction(fsm, "Spit R", 7);
                FsmUtil.AddMethod(fsm, "Spit R", Spit, fsm);
                Log("Upgraded FSM.");
            }
        }
        private void Spit(PlayMakerFSM fsm)
        {
            Log("Spitting.");
            var zoteling = FsmUtil.FindFsmGameObjectVariable(fsm, "Zoteling").Value;
            GameObject minion = GameObject.Instantiate(minionTemplate);
            minion.SetActive(true);
            minion.SetActiveChildren(true);
            minion.GetComponent<HealthManager>().hp = 52;
            minion.transform.position = zoteling.transform.position;
            Log("Spat.");
        }
        private void ActiveSceneChanged(UnityEngine.SceneManagement.Scene from, UnityEngine.SceneManagement.Scene to)
        {
        }
        private void HeroUpdateHook()
        {
        }
    }
}