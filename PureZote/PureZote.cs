using Modding;
using System.Collections.Generic;
using UnityEngine;


namespace PureZote
{
    public class PureZote : Mod, IGlobalSettings<Settings>
    {
        private Settings settings_ = new();
        private Minions minions;
        public PureZote() : base("PureZote"){ minions = new Minions(this); }
        public void OnLoadGlobal(Settings settings) => settings_ = settings;
        public Settings OnSaveGlobal() => settings_;
        public override string GetVersion() => "1.0";
        public override List<(string, string)> GetPreloadNames()
        {
            return minions.GetPreloadNames();
        }
        public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
        {
            Log("Initializing.");
            if (settings_.enabled)
            {
                Log("Enabled.");
                On.PlayMakerFSM.OnEnable += PlayMakerFSMOnEnable;
                minions.LoadPrefabs(preloadedObjects);
                UnityEngine.SceneManagement.SceneManager.activeSceneChanged += ActiveSceneChanged;
                ModHooks.HeroUpdateHook += HeroUpdateHook;
            }
            else
            {
                Log("Disabled.");
            }
            Log("Initialized.");
        }
        private void PlayMakerFSMOnEnable(On.PlayMakerFSM.orig_OnEnable original, PlayMakerFSM fsm)
        {
            original(fsm);
            minions.UpgradeFSM(fsm);
        }
        private void ActiveSceneChanged(UnityEngine.SceneManagement.Scene from, UnityEngine.SceneManagement.Scene to)
        {
            Log("Scene loaded: " + to.name + ".");
        }
        private void HeroUpdateHook()
        {
            if (Input.GetKeyDown(KeyCode.F2))
            {
                Log("Force loading scene GG_Grey_Prince_Zote.");
                UnityEngine.SceneManagement.SceneManager.LoadScene("GG_Grey_Prince_Zote");
                Log("Force loaded scene GG_Grey_Prince_Zote.");
            }
        }
    }
}