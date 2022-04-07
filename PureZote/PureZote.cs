using Modding;
using System.Collections.Generic;
using UnityEngine;
using Satchel;


namespace PureZote
{
    public class PureZote : Mod, IGlobalSettings<Settings>
    {
        private Settings settings_ = new();
        private readonly Minions minions;
        private readonly Palette palette;
        private readonly Common common;
        private readonly System.Random random = new();
        public PureZote() : base("PureZote")
        {
            minions = new Minions(this);
            palette = new Palette(this);
            common = new Common(this);
        }
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
            if (settings_.enable)
            {
                Log("Enabled.");
                On.PlayMakerFSM.OnEnable += PlayMakerFSMOnEnable;
                minions.LoadPrefabs(preloadedObjects);
                UnityEngine.SceneManagement.SceneManager.activeSceneChanged += ActiveSceneChanged;
                ModHooks.HeroUpdateHook += HeroUpdateHook;
                On.SceneManager.SetLighting += palette.SetLighting;
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
            if (fsm.gameObject.scene.name == "GG_Grey_Prince_Zote" && fsm.gameObject.name == "Grey Prince" && fsm.FsmName == "Control")
            {
                void Spit(PlayMakerFSM fsm)
                {
                    Log("Spitting.");
                    var zoteling = fsm.FsmVariables.FindFsmGameObject("Zoteling").Value;
                    zoteling.GetComponent<Renderer>().enabled = false;
                    var index = random.Next(minions.spittedPrefabs.Count);
                    var minion = Object.Instantiate(minions.spittedPrefabs[index]);
                    var settings_ = minions.settings[index];
                    minion.SetActive(true);
                    minion.SetActiveChildren(true);
                    minion.transform.position = zoteling.transform.position;
                    if (settings_.enableSpittingVelocity_)
                    {
                        minion.GetComponent<Rigidbody2D>().velocity = zoteling.GetComponent<Rigidbody2D>().velocity;
                    }
                    ++minions.easyMinionsCount;
                    Log("Spat.");
                }
                Log("Upgrading FSM: " + fsm.gameObject.name + " - " + fsm.FsmName + ".");
                FsmUtil.RemoveAction(fsm, "Level 3", 15);
                FsmUtil.InsertCustomAction(fsm, "Level 3", () => fsm.FsmVariables.FindFsmInt("ActiveZotelings Max").Value = settings_.maxEasyMinionCount, 15);
                FsmUtil.RemoveAction(fsm, "Spit Antic", 1);
                FsmUtil.InsertCustomAction(fsm, "Spit Antic", () => fsm.FsmVariables.FindFsmInt("ActiveZotelings").Value = minions.easyMinionsCount, 1);
                FsmUtil.RemoveAction(fsm, "Spit L", 7);
                FsmUtil.AddCustomAction(fsm, "Spit L", () => Spit(fsm));
                FsmUtil.RemoveAction(fsm, "Spit R", 7);
                FsmUtil.AddCustomAction(fsm, "Spit R", () => Spit(fsm));
                minions.easyMinionsCount = 0;
                Log("Upgraded FSM: " + fsm.gameObject.name + " - " + fsm.FsmName + ".");
            }
            minions.UpgradeFSM(fsm);
        }
        private void ActiveSceneChanged(UnityEngine.SceneManagement.Scene from, UnityEngine.SceneManagement.Scene to)
        {
            Log("Scene loaded: " + to.name + ".");
        }
        private void HeroUpdateHook()
        {
            if (settings_.enableTeleportation && Input.GetKeyDown(KeyCode.F2))
            {
                Log("Teleporting to scene GG_Grey_Prince_Zote.");
                UnityEngine.SceneManagement.SceneManager.LoadScene("GG_Grey_Prince_Zote");
                Log("Teleported to scene GG_Grey_Prince_Zote.");
            }
        }
    }
}
