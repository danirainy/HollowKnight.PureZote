using Modding;
using System.Collections.Generic;
using UnityEngine;
using Satchel;


namespace PureZote
{
    public class PureZote : Mod
    {
        private class Variables
        {
            public List<string> prioritizedMoves = new() { "Opening Shadow Slam" };
        }
        private readonly Common common;
        private readonly Settings settings = new();
        private readonly Minions minions;
        private readonly Projectiles projectiles;
        private readonly Palette palette;
        private readonly System.Random random = new();
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
                    fsm.gameObject.GetComponent<HealthManager>().hp = 3000;
                });
                FsmUtil.AddCustomAction(fsm, "Enter 1", () =>
                {
                    fsm.SetState("Enter Short");
                });
                FsmUtil.InsertCustomAction(fsm, "Send Event", () =>
                 {
                     Log("Zote is stunned. Resetting certain variables.");
                     minions.variables.isSpittingHardMinions = false;
                     Log("Zote is stunned. Reset certain variables.");
                 }, 0);
                FsmUtil.InsertCustomAction(fsm, "Stomp", () =>
                {
                    if (random.Next(2) == 1)
                        fsm.SetState("Shift Dir");
                }, 0);
                FsmUtil.InsertCustomAction(fsm, "Charge Fall", () =>
                {
                    if (random.Next(2) == 1)
                        fsm.SetState("Jump Antic");
                }, 0);
                void setWaveScale(PlayMakerFSM fsm)
                {
                    var wave = fsm.FsmVariables.GetFsmGameObject("Shockwave").Value;
                    Log("Oringal wave size: " + wave.transform.localScale.x.ToString());
                    wave.transform.SetScaleX(3);
                    Log("New wave size: " + wave.transform.localScale.x.ToString());
                }
                FsmUtil.InsertCustomAction(fsm, "Slash Waves L", () => setWaveScale(fsm), 4);
                FsmUtil.InsertCustomAction(fsm, "Slash Waves R", () => setWaveScale(fsm), 4);
                FsmUtil.InsertCustomAction(fsm, "B Roar Antic", () =>
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
                FsmUtil.RemoveAction(fsm, "Aim Jump", 5);
                FsmUtil.InsertCustomAction(fsm, "Fall Through?", () =>
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
                FsmUtil.InsertCustomAction(fsm, "Idle Start", () =>
                {
                    if (fsm.gameObject.GetComponent<HealthManager>().hp <= 2800 && !minions.variables.touchedCheckpoint1)
                    {
                        Log("Checkpoint 1 touched. Summoning hard minions.");
                        minions.variables.touchedCheckpoint1 = true;
                        minions.variables.hardMinionQueue.Add("Salubra Zoteling");
                        minions.variables.hardMinionQueue.Add("Fat Zoteling");
                    }
                    if (fsm.gameObject.GetComponent<HealthManager>().hp <= 1800 && !minions.variables.touchedCheckpoint2)
                    {
                        Log("Checkpoint 2 touched. Summoning hard minions.");
                        minions.variables.touchedCheckpoint2 = true;
                        minions.variables.hardMinionQueue.Add("Fat Zoteling");
                        minions.variables.hardMinionQueue.Add("Fat Zoteling");
                    }
                    if (fsm.gameObject.GetComponent<HealthManager>().hp <= 800 && !minions.variables.touchedCheckpoint3)
                    {
                        Log("Checkpoint 3 touched. Summoning hard minions.");
                        minions.variables.touchedCheckpoint3 = true;
                        variables.prioritizedMoves.Add("Summon Salubra Zotelings");
                    }
                    if (minions.variables.hardMinionQueue.Count > 0)
                    {
                        Log("Hard minion queue is not empty. Going to spit.");
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
                FsmUtil.InsertCustomAction(fsm, "Ft Waves", () =>
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
                Log("Upgraded FSM: " + fsm.gameObject.name + " - " + fsm.FsmName + ".");
                variables = new();
                minions.variables = new();
                fsm.gameObject.GetComponent<tk2dSprite>().color = Color.magenta;
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
