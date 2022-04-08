using Modding;
using System.Collections.Generic;
using UnityEngine;
using Satchel;


namespace PureZote
{
    public class PureZote : Mod
    {
        private readonly Common common;
        private readonly Settings settings = new();
        private readonly Minions minions;
        private readonly Projectiles projectiles;
        private readonly Palette palette;
        private readonly System.Random random = new();
        public PureZote() : base("PureZote")
        {
            common = new Common(this);
            minions = new Minions(this);
            projectiles = new Projectiles(this);
            palette = new Palette(this);
        }
        public override string GetVersion() => "1.0";
        public override List<(string, string)> GetPreloadNames()
        {
            List<(string, string)> preloadNames = new();
            foreach (var name in minions.GetPreloadNames())
                preloadNames.Add(name);
            foreach (var name in projectiles.GetPreloadNames())
                preloadNames.Add(name);
            return preloadNames;
        }
        public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
        {
            Log("Initializing.");
            if (settings.enable)
            {
                Log("Enabled.");
                On.PlayMakerFSM.OnEnable += PlayMakerFSMOnEnable;
                minions.LoadPrefabs(preloadedObjects);
                projectiles.LoadPrefabs(preloadedObjects);
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
                Log("Upgrading FSM: " + fsm.gameObject.name + " - " + fsm.FsmName + ".");
                FsmUtil.AddCustomAction(fsm, "Level 3", () =>
                {
                    fsm.FsmVariables.FindFsmInt("ActiveZotelings Max").Value = settings.maxEasyMinionCount;
                    fsm.gameObject.GetComponent<HealthManager>().hp = 2800;
                });
                FsmUtil.InsertCustomAction(fsm, "Idle Start", () =>
                {
                    if (fsm.gameObject.GetComponent<HealthManager>().hp <= 2000 && !minions.variables.touchedMinionCheckpoint1)
                    {
                        Log("Checkpoint touched. Summoning hard minions.");
                        minions.variables.touchedMinionCheckpoint1 = true;
                        minions.variables.hardMinionQueue.Add("Salubra Zoteling");
                        minions.variables.hardMinionQueue.Add("Fat Zoteling");
                    }
                    if (fsm.gameObject.GetComponent<HealthManager>().hp <= 1200 && !minions.variables.touchedMinionCheckpoint2)
                    {
                        Log("Checkpoint touched. Summoning hard minions.");
                        minions.variables.touchedMinionCheckpoint2 = true;
                        minions.variables.hardMinionQueue.Add("Fat Zoteling");
                        minions.variables.hardMinionQueue.Add("Fat Zoteling");
                    }
                    if (minions.variables.hardMinionQueue.Count > 0)
                    {
                        Log("Hard minion queue is not empty. Going to spit.");
                        minions.variables.isSpittingHardMinions = true;
                        fsm.SetState("Spit Set");
                    }
                }, 0);
                FsmUtil.InsertCustomAction(fsm, "Ft Waves", () =>
                {
                    var prefab = projectiles.prefabs["traitorLordWave"];
                    var wave = Object.Instantiate(prefab);
                    wave.transform.position = new Vector3(fsm.gameObject.transform.position.x, 0, 0.061f);
                    wave.GetComponent<Rigidbody2D>().velocity = new Vector2(12, 0);
                    wave = Object.Instantiate(prefab);
                    wave.transform.position = new Vector3(fsm.gameObject.transform.position.x, 0, 0.061f);
                    wave.transform.localScale = new Vector3(-1, 1, 1);
                    wave.GetComponent<Rigidbody2D>().velocity = new Vector2(-12, 0);
                    fsm.SendEvent("FINISHED");
                }, 1);
                FsmUtil.InsertCustomAction(fsm, "Spit Antic", () =>
                {
                    if (minions.variables.isSpittingHardMinions)
                    {
                        Log("Allowing to go off limit for hard minions.");
                        fsm.FsmVariables.FindFsmInt("ActiveZotelings").Value = 0;
                    }
                    else
                    {
                        fsm.FsmVariables.FindFsmInt("ActiveZotelings").Value = minions.variables.minionCount;
                    }
                }, 2);
                void Spit(PlayMakerFSM fsm)
                {
                    Log("Spitting.");
                    var zoteling = fsm.FsmVariables.FindFsmGameObject("Zoteling").Value;
                    zoteling.GetComponent<Renderer>().enabled = false;
                    GameObject minion;
                    Minions.Settings settings;
                    if (minions.variables.isSpittingHardMinions)
                    {
                        Log("Retrieving hard minions.");
                        var name = minions.variables.hardMinionQueue[0];
                        minions.variables.hardMinionQueue.RemoveAt(0);
                        minion = minions.hardMinionPrefabs[name];
                        settings = minions.hardMinionSettings[name];
                    }
                    else
                    {
                        var index = random.Next(minions.easyMinionPrefabs.Count);
                        minion = minions.easyMinionPrefabs[index];
                        settings = minions.easyMinionSettings[index];
                    }
                    minion = Object.Instantiate(minion);
                    minion.SetActive(true);
                    minion.SetActiveChildren(true);
                    minion.transform.position = zoteling.transform.position;
                    minion.GetComponent<Rigidbody2D>().velocity = settings.velocityScale_ * zoteling.GetComponent<Rigidbody2D>().velocity;
                    ++minions.variables.minionCount;
                    Log("Spat.");
                }
                FsmUtil.RemoveAction(fsm, "Spit L", 7);
                FsmUtil.AddCustomAction(fsm, "Spit L", () => Spit(fsm));
                FsmUtil.RemoveAction(fsm, "Spit R", 7);
                FsmUtil.AddCustomAction(fsm, "Spit R", () => Spit(fsm));
                FsmUtil.InsertCustomAction(fsm, "Respit?", () =>
                {
                    if (minions.variables.isSpittingHardMinions)
                    {
                        Log("Spitting hard minions. Respit if and only the queue is not empty");
                        if (minions.variables.hardMinionQueue.Count > 0)
                        {
                            fsm.SendEvent("REPEAT");
                        }
                        else
                        {
                            minions.variables.isSpittingHardMinions = false;
                            fsm.SendEvent("FINISHED");
                        }
                    }
                }, 0);
                minions.variables = new();
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
            if (settings.enableTeleportation && Input.GetKeyDown(KeyCode.F2))
            {
                Log("Teleporting to scene GG_Grey_Prince_Zote.");
                UnityEngine.SceneManagement.SceneManager.LoadScene("GG_Grey_Prince_Zote");
                Log("Teleported to scene GG_Grey_Prince_Zote.");
            }
        }
    }
}
