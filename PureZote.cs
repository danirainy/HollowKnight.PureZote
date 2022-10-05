using Modding;
using System.Collections.Generic;
using UnityEngine;


namespace PureZote
{
    public class PureZote : Mod
    {
        private class Variables
        {
            public List<string> prioritizedMoves = new List<string>() { "Opening Shadow Slam" };
        }
        private readonly Common common;
        private readonly Settings settings = new Settings();
        private readonly Minions minions;
        private readonly Projectiles projectiles;
        private readonly Palette palette;
        private readonly System.Random random = new System.Random();
        private Variables variables;
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
            List<(string, string)> preloadNames = new List<(string, string)>();
            foreach (var name in minions.GetPreloadNames())
                preloadNames.Add(name);
            foreach (var name in projectiles.GetPreloadNames())
                preloadNames.Add(name);
            return preloadNames;
        }
        public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
        {
            LogDebug("Initializing.");
            if (settings.enable)
            {
                LogDebug("Enabled.");
                On.PlayMakerFSM.OnEnable += PlayMakerFSMOnEnable;
                minions.LoadPrefabs(preloadedObjects);
                projectiles.LoadPrefabs(preloadedObjects);
                UnityEngine.SceneManagement.SceneManager.activeSceneChanged += ActiveSceneChanged;
                ModHooks.Instance.HeroUpdateHook += HeroUpdateHook;
            }
            else
            {
                LogDebug("Disabled.");
            }
            LogDebug("Initialized.");
        }
        private void PlayMakerFSMOnEnable(On.PlayMakerFSM.orig_OnEnable original, PlayMakerFSM fsm)
        {
            original(fsm);
            if (fsm.gameObject.scene.name == "GG_Grey_Prince_Zote" && fsm.gameObject.name == "Grey Prince" && fsm.FsmName == "Control")
            {
                LogDebug("Upgrading FSM: " + fsm.gameObject.name + " - " + fsm.FsmName + ".");
                fsm.AddCustomAction("Level 3", () =>
                {
                    fsm.FsmVariables.FindFsmInt("ActiveZotelings Max").Value = settings.maxEasyMinionCount;
                    fsm.gameObject.GetComponent<HealthManager>().hp = 3000;
                });
                fsm.AddCustomAction("Enter 1", () =>
                {
                    fsm.SetState("Enter Short");
                });
                fsm.InsertCustomAction("Send Event", () =>
                 {
                     LogDebug("Zote is stunned. Resetting certain variables.");
                     minions.variables.isSpittingHardMinions = false;
                     LogDebug("Zote is stunned. Reset certain variables.");
                 }, 0);
                fsm.InsertCustomAction("Stomp", () =>
                {
                    if (random.Next(2) == 1)
                        fsm.SetState("Shift Dir");
                }, 0);
                fsm.InsertCustomAction("Charge Fall", () =>
                {
                    if (random.Next(2) == 1)
                        fsm.SetState("Jump Antic");
                }, 0);
                void setWaveScale(PlayMakerFSM fsm1)
                {
                    var wave = fsm1.FsmVariables.GetFsmGameObject("Shockwave").Value;
                    LogDebug("Oringal wave size: " + wave.transform.localScale.x.ToString());
                    wave.transform.SetScaleX(3);
                    LogDebug("New wave size: " + wave.transform.localScale.x.ToString());
                }
                fsm.InsertCustomAction("Slash Waves L", () => setWaveScale(fsm), 4);
                fsm.InsertCustomAction("Slash Waves R", () => setWaveScale(fsm), 4);
                fsm.InsertCustomAction("B Roar Antic", () =>
                {
                    if (variables.prioritizedMoves.Count > 0)
                    {
                        var name = variables.prioritizedMoves[0];
                        if (name == "Summon Salubra Zotelings")
                        {
                            minions.variables.roarType = 3;
                            variables.prioritizedMoves.RemoveAt(0);
                            return;
                        }
                    }
                    minions.variables.roarType = random.Next(1, 3);
                }, 0);
                fsm.RemoveAction("Aim Jump", 5);
                fsm.InsertCustomAction("Fall Through?", () =>
                {
                    if (variables.prioritizedMoves.Count > 0)
                    {
                        var name = variables.prioritizedMoves[0];
                        if (name == "Opening Shadow Slam")
                        {
                            fsm.SendEvent("GO THROUGH");
                            variables.prioritizedMoves.RemoveAt(0);
                        }
                    }
                }, 0);
                fsm.InsertCustomAction("Idle Start", () =>
                {
                    if (fsm.gameObject.GetComponent<HealthManager>().hp <= 2800 && !minions.variables.touchedCheckpoint1)
                    {
                        LogDebug("Checkpoint 1 touched. Summoning hard minions.");
                        minions.variables.touchedCheckpoint1 = true;
                        minions.variables.hardMinionQueue.Add("Salubra Zoteling");
                        minions.variables.hardMinionQueue.Add("Fat Zoteling");
                    }
                    if (fsm.gameObject.GetComponent<HealthManager>().hp <= 1800 && !minions.variables.touchedCheckpoint2)
                    {
                        LogDebug("Checkpoint 2 touched. Summoning hard minions.");
                        minions.variables.touchedCheckpoint2 = true;
                        minions.variables.hardMinionQueue.Add("Fat Zoteling");
                        minions.variables.hardMinionQueue.Add("Fat Zoteling");
                    }
                    if (fsm.gameObject.GetComponent<HealthManager>().hp <= 800 && !minions.variables.touchedCheckpoint3)
                    {
                        LogDebug("Checkpoint 3 touched. Summoning hard minions.");
                        minions.variables.touchedCheckpoint3 = true;
                        variables.prioritizedMoves.Add("Summon Salubra Zotelings");
                    }
                    if (minions.variables.hardMinionQueue.Count > 0)
                    {
                        LogDebug("Hard minion queue is not empty. Going to spit.");
                        minions.variables.isSpittingHardMinions = true;
                        fsm.SetState("Spit Set");
                    }
                    if (variables.prioritizedMoves.Count > 0)
                    {
                        var name = variables.prioritizedMoves[0];
                        if (name == "Opening Shadow Slam")
                        {
                            fsm.SetState("Set Jumps");
                        }
                        else if (name == "Summon Salubra Zotelings")
                        {
                            fsm.SetState("B Roar Antic");
                        }
                    }
                }, 0);
                fsm.InsertCustomAction("Ft Waves", () =>
                {
                    var prefab = projectiles.prefabs["traitorLordWave"];
                    var wave = Object.Instantiate(prefab);
                    wave.transform.position = new Vector3(fsm.gameObject.transform.position.x, 2, 1);
                    wave.GetComponent<Rigidbody2D>().velocity = new Vector2(12, 0);
                    wave = Object.Instantiate(prefab);
                    wave.transform.position = new Vector3(fsm.gameObject.transform.position.x, 2, 1);
                    wave.transform.localScale = new Vector3(-1, 1, 1);
                    wave.GetComponent<Rigidbody2D>().velocity = new Vector2(-12, 0);
                    fsm.SendEvent("FINISHED");
                }, 1);
                fsm.InsertCustomAction("Spit Antic", () =>
                {
                    if (minions.variables.isSpittingHardMinions)
                    {
                        LogDebug("Allowing to go off limit for hard minions.");
                        fsm.FsmVariables.FindFsmInt("ActiveZotelings").Value = 0;
                    }
                    else
                    {
                        fsm.FsmVariables.FindFsmInt("ActiveZotelings").Value = minions.variables.minionCount;
                    }
                }, 2);
                void Spit(PlayMakerFSM fsm1)
                {
                    LogDebug("Spitting.");
                    var zoteling = fsm1.FsmVariables.FindFsmGameObject("Zoteling").Value;
                    zoteling.GetComponent<Renderer>().enabled = false;
                    UnityEngine.Object.Destroy(zoteling.GetComponent<DamageHero>());
                    GameObject minion;
                    Minions.Settings settings;
                    if (minions.variables.isSpittingHardMinions)
                    {
                        LogDebug("Retrieving hard minions.");
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
                    LogDebug("Spat.");
                }
                fsm.RemoveAction("Spit L", 7);
                fsm.AddCustomAction("Spit L", () => Spit(fsm));
                fsm.RemoveAction("Spit R", 7);
                fsm.AddCustomAction("Spit R", () => Spit(fsm));
                fsm.InsertCustomAction("Respit?", () =>
                {
                    if (minions.variables.isSpittingHardMinions)
                    {
                        LogDebug("Spitting hard minions. Respit if and only the queue is not empty");
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
                LogDebug("Upgraded FSM: " + fsm.gameObject.name + " - " + fsm.FsmName + ".");
                variables = new Variables();
                minions.variables = new Minions.Variables();
            }
            minions.UpgradeFSM(fsm);
        }
        private void ActiveSceneChanged(UnityEngine.SceneManagement.Scene from, UnityEngine.SceneManagement.Scene to)
        {
            LogDebug("Scene loaded: " + to.name + ".");
        }
        private void HeroUpdateHook()
        {
            if (settings.enableTeleportation && Input.GetKeyDown(KeyCode.F2))
            {
                LogDebug("Teleporting to scene GG_Grey_Prince_Zote.");
                UnityEngine.SceneManagement.SceneManager.LoadScene("GG_Grey_Prince_Zote");
                LogDebug("Teleported to scene GG_Grey_Prince_Zote.");
            }
        }
    }
}
