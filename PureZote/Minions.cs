using System.Collections.Generic;
using UnityEngine;
using Modding;
using Satchel;
using HutongGames.PlayMaker.Actions;


namespace PureZote
{
    public class Minions
    {
        public class Settings
        {
            public Settings(float velocityScale)
            {
                velocityScale_ = velocityScale;
            }
            public float velocityScale_;
        }
        public class Variables
        {
            public int minionCount = 0;
            public readonly List<string> hardMinionQueue = new();
            public bool isSpittingHardMinions = false;
            public bool touchedCheckpoint1 = false;
            public bool touchedCheckpoint2 = false;
            public bool touchedCheckpoint3 = false;
            public int roarType = 0;
        }
        private readonly Mod mod_;
        private readonly Common common;
        private readonly Dictionary<string, GameObject> prefabs = new();
        public readonly List<GameObject> easyMinionPrefabs = new();
        public readonly List<Settings> easyMinionSettings = new();
        public readonly Dictionary<string, GameObject> hardMinionPrefabs = new();
        public readonly Dictionary<string, Settings> hardMinionSettings = new();
        private readonly System.Random random = new();
        public Variables variables;
        public Minions(Mod mod)
        {
            mod_ = mod;
            common = new Common(mod);
        }
        private void Log(object message) => mod_.LogDebug(message);
        public List<(string, string)> GetPreloadNames()
        {
            return new List<(string, string)>
            {
                 ("GG_Mighty_Zote","Battle Control"),
            };
        }
        public void LoadPrefabs(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
        {
            var battleControl = preloadedObjects["GG_Mighty_Zote"]["Battle Control"];
            var names = new List<(string, string, string)>
            {
                ("Tall Zotes", "Zote Crew Tall", "Tall Zoteling"),
                ("Fat Zotes", "Zote Crew Fat (1)", "Fat Zoteling"),
                ("Dormant Warriors", "Zote Crew Normal (1)", "Normal Zoteling"),
                ("Zotelings", "Ordeal Zoteling", "Ordeal Zoteling"),
                ("Zote Salubra", "", "Salubra Zoteling"),
                ("Zote Fluke", "", "Fluke Zoteling"),
                ("Zote Thwomp", "", "Thwomp Zoteling"),
            };
            foreach ((string group, string instance, string name) in names)
            {
                GameObject minion;
                if (instance != "")
                {
                    minion = battleControl.transform.Find(group).gameObject.transform.Find(instance).gameObject;
                }
                else
                {
                    minion = battleControl.transform.Find(group).gameObject;
                }
                Object.Destroy(minion.GetComponent<PersistentBoolItem>());
                Object.Destroy(minion.GetComponent<ConstrainPosition>());
                prefabs[name] = minion;
                Log("Minion added: " + minion.name + ".");
            }
            foreach ((string name, Settings settings) in new List<(string, Settings)>
            {
                ("Tall Zoteling", new Settings(1)),
                ("Normal Zoteling", new Settings(1)),
                ("Ordeal Zoteling", new Settings(1)),
            })
            {
                easyMinionPrefabs.Add(prefabs[name]);
                easyMinionSettings.Add(settings);
            }
            foreach ((string name, Settings settings) in new List<(string, Settings)>
            {
                ("Fat Zoteling", new Settings(1)),
                ("Salubra Zoteling", new Settings(0.5f)),
                ("Fluke Zoteling", new Settings(1)),
                ("Thwomp Zoteling", new Settings(1)),
            })
            {
                hardMinionPrefabs[name] = prefabs[name];
                hardMinionSettings[name] = settings;
            }
        }
        public void UpgradeFSM(PlayMakerFSM fsm)
        {
            if (fsm.gameObject.scene.name == "GG_Grey_Prince_Zote" && fsm.gameObject.name == "Zote Crew Tall(Clone)" && fsm.FsmName == "Control")
            {
                Log("Upgrading FSM: " + fsm.gameObject.name + " - " + fsm.FsmName + ".");
                fsm.AddTransition("Dormant", "FINISHED", "Multiply");
                fsm.RemoveAction("Spawn Antic", 1);
                fsm.RemoveAction("Spawn Antic", 3);
                fsm.AddCustomAction("Spawn Antic", () => fsm.SendEvent("FINISHED"));
                fsm.RemoveAction("Tumble Out", 2);
                fsm.RemoveAction("Death", 0);
                fsm.AddCustomAction("Death Reset", () =>
                {
                    Object.Destroy(fsm.gameObject);
                    variables.minionCount -= 1;
                });
                Log("Upgraded FSM: " + fsm.gameObject.name + " - " + fsm.FsmName + ".");
            }
            else if (fsm.gameObject.scene.name == "GG_Grey_Prince_Zote" && fsm.gameObject.name == "Zote Crew Fat (1)(Clone)" && fsm.FsmName == "Control")
            {
                Log("Upgrading FSM: " + fsm.gameObject.name + " - " + fsm.FsmName + ".");
                fsm.AddTransition("Dormant", "FINISHED", "Multiply");
                fsm.RemoveAction("Spawn Antic", 1);
                fsm.RemoveAction("Spawn Antic", 3);
                fsm.RemoveAction("Spawn Antic", 5);
                fsm.AddCustomAction("Spawn Antic", () => fsm.SendEvent("FINISHED"));
                fsm.RemoveAction("Tumble Out", 2);
                fsm.RemoveAction("Dr", 1);
                fsm.AddCustomAction("Dr", () =>
                {
                    if (fsm.gameObject.transform.position.x < HeroController.instance.transform.position.x)
                    {
                        fsm.SendEvent("R");
                    }
                    else
                    {
                        fsm.SendEvent("L");
                    }
                });
                fsm.AddCustomAction("Death Reset", () =>
                {
                    Object.Destroy(fsm.gameObject);
                    variables.minionCount -= 1;
                });
                Log("Upgraded FSM: " + fsm.gameObject.name + " - " + fsm.FsmName + ".");
            }
            else if (fsm.gameObject.scene.name == "GG_Grey_Prince_Zote" && fsm.gameObject.name == "Zote Crew Normal (1)(Clone)" && fsm.FsmName == "Control")
            {
                Log("Upgrading FSM: " + fsm.gameObject.name + " - " + fsm.FsmName + ".");
                fsm.AddTransition("Dormant", "FINISHED", "Multiply");
                fsm.RemoveAction("Spawn Antic", 1);
                fsm.RemoveAction("Spawn Antic", 3);
                fsm.AddCustomAction("Spawn Antic", () => { fsm.SendEvent("FINISHED"); });
                fsm.RemoveAction("Tumble Out", 2);
                fsm.RemoveAction("Death", 0);
                fsm.AddCustomAction("Death Reset", () =>
                {
                    Object.Destroy(fsm.gameObject);
                    variables.minionCount -= 1;
                });
                Log("Upgraded FSM: " + fsm.gameObject.name + " - " + fsm.FsmName + ".");
            }
            else if (fsm.gameObject.scene.name == "GG_Grey_Prince_Zote" && fsm.gameObject.name == "Ordeal Zoteling(Clone)" && fsm.FsmName == "Control")
            {
                Log("Upgrading FSM: " + fsm.gameObject.name + " - " + fsm.FsmName + ".");
                fsm.AddTransition("Dormant", "FINISHED", "Ball");
                fsm.RemoveAction("Ball", 2);
                fsm.AddCustomAction("Reset", () =>
                {
                    Object.Destroy(fsm.gameObject);
                    variables.minionCount -= 1;
                });
                Log("Upgraded FSM: " + fsm.gameObject.name + " - " + fsm.FsmName + ".");
            }
            else if (fsm.gameObject.scene.name == "GG_Grey_Prince_Zote" && fsm.gameObject.name == "Zote Salubra(Clone)" && fsm.FsmName == "Control")
            {
                Log("Upgrading FSM: " + fsm.gameObject.name + " - " + fsm.FsmName + ".");
                fsm.AddTransition("Dormant", "FINISHED", "Appear");
                fsm.RemoveAction("Appear", 3);
                fsm.RemoveAction("Appear", 5);
                fsm.RemoveAction("Idle", 0);
                fsm.RemoveAction("Idle", 0);
                var ghostMovement = FsmUtil.GetAction<HutongGames.PlayMaker.Actions.GhostMovement>(fsm, "Sucking", 8);
                ghostMovement.xPosMin = 9;
                ghostMovement.xPosMax = 44;
                fsm.RemoveAction("Dead", 1);
                fsm.AddCustomAction("Dead", () =>
                {
                    Object.Destroy(fsm.gameObject);
                    variables.minionCount -= 1;
                });
                Log("Upgraded FSM: " + fsm.gameObject.name + " - " + fsm.FsmName + ".");
            }
            else if (fsm.gameObject.scene.name == "GG_Grey_Prince_Zote" && fsm.gameObject.name == "Zote Fluke(Clone)" && fsm.FsmName == "Control")
            {
                Log("Upgrading FSM: " + fsm.gameObject.name + " - " + fsm.FsmName + ".");
                fsm.AddTransition("Dormant", "FINISHED", "Pos");
                fsm.InsertCustomAction("Pos", () =>
                {
                    fsm.FsmVariables.GetFsmFloat("X Pos").Value = fsm.gameObject.transform.position.x;
                }, 1);
                fsm.RemoveAction("Pos", 4);
                Log("Upgraded FSM: " + fsm.gameObject.name + " - " + fsm.FsmName + ".");
            }
            else if (fsm.gameObject.scene.name == "GG_Grey_Prince_Zote" && fsm.gameObject.name == "Zote Thwomp(Clone)" && fsm.FsmName == "Control")
            {
                Log("Upgrading FSM: " + fsm.gameObject.name + " - " + fsm.FsmName + ".");
                fsm.AddTransition("Dormant", "FINISHED", "Set Pos");
                fsm.InsertCustomAction("Set Pos", () =>
                {
                    fsm.FsmVariables.GetFsmFloat("X Pos").Value = fsm.gameObject.transform.position.x;
                    fsm.gameObject.transform.Find("Enemy Crusher").gameObject.SetActive(false);
                }, 1);
                fsm.RemoveAction("Set Pos", 2);
                fsm.RemoveAction("Slam", 5);
                fsm.RemoveAction("Down", 6);
                fsm.InsertCustomAction("Waves", () =>
                {
                    fsm.SendEvent("FINISHED");
                }, 0);
                fsm.RemoveAction("Rise", 7);
                Log("Upgraded FSM: " + fsm.gameObject.name + " - " + fsm.FsmName + ".");
            }
            else if (fsm.gameObject.scene.name == "GG_Grey_Prince_Zote" && fsm.gameObject.name.StartsWith("Zote Balloon") && fsm.FsmName == "Control")
            {
                Log("Upgrading FSM: " + fsm.gameObject.name + " - " + fsm.FsmName + ".");
                fsm.AddCustomAction("Set Pos", () =>
                {
                    int index = variables.roarType;
                    if (index == 0 || GameObject.Find("Zote Crew Fat (1)(Clone)") != null || GameObject.Find("Zote Salubra(Clone)"))
                    {
                        return;
                    }
                    else if (index == 1)
                    {
                        GameObject minion = hardMinionPrefabs["Fluke Zoteling"];
                        minion = Object.Instantiate(minion);
                        minion.SetActive(true);
                        minion.SetActiveChildren(true);
                        minion.transform.position = new Vector3(fsm.gameObject.transform.position.x, 6, fsm.gameObject.transform.position.z);
                        minion.transform.SetScaleX(0.5f * minion.transform.localScale.x);
                        minion.transform.SetScaleY(0.5f * minion.transform.localScale.y);
                        minion.transform.SetScaleZ(0.5f * minion.transform.localScale.z);
                    }
                    else if (index == 2)
                    {
                        GameObject minion = hardMinionPrefabs["Thwomp Zoteling"];
                        minion = Object.Instantiate(minion);
                        minion.SetActive(true);
                        minion.SetActiveChildren(true);
                        minion.transform.position = new Vector3(fsm.gameObject.transform.position.x, 23.4f, fsm.gameObject.transform.position.z);
                        minion.transform.SetScaleX(0.5f * minion.transform.localScale.x);
                        minion.transform.SetScaleY(0.5f * minion.transform.localScale.y);
                        minion.transform.SetScaleZ(0.5f * minion.transform.localScale.z);
                    }
                    else
                    {
                        GameObject minion = hardMinionPrefabs["Salubra Zoteling"];
                        minion = Object.Instantiate(minion);
                        minion.name = "Salubra Zoteling Final";
                        minion.SetActive(true);
                        minion.SetActiveChildren(true);
                        minion.transform.position = new Vector3(fsm.gameObject.transform.position.x, 10, fsm.gameObject.transform.position.z);
                        minion.GetComponent<PlayMakerFSM>().AddCustomAction("Idle", () =>
                        {
                            var voice = minion.transform.Find("Voice").gameObject;
                            voice.SetActive(false);
                        });
                        var ghostMovement = minion.GetComponent<PlayMakerFSM>().GetAction<GhostMovement>("Sucking", 8);
                        ghostMovement.yPosMin = ghostMovement.yPosMin.Value + 5;
                        ghostMovement.yPosMax = ghostMovement.yPosMax.Value + 5;

                    }
                    fsm.SetState("Dormant");
                });
                Log("Upgraded FSM: " + fsm.gameObject.name + " - " + fsm.FsmName + ".");
            }
        }
    }
}
