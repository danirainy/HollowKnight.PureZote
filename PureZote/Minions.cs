using System.Collections.Generic;
using UnityEngine;
using Modding;
using Satchel;


namespace PureZote
{
    public class Minions
    {
        public class Settings
        {
            public Settings(bool enableSpittingVelocity = true)
            {
                enableSpittingVelocity_ = enableSpittingVelocity;
            }
            public bool enableSpittingVelocity_;
        }
        private readonly Mod mod_;
        private readonly Common common;
        private readonly Dictionary<string, GameObject> prefabs = new();
        public readonly List<GameObject> spittedPrefabs = new();
        public int easyMinionsCount = 0;
        public List<Settings> settings;
        public Minions(Mod mod) {
            mod_ = mod;
            common = new Common(mod);
        }
        private void Log(object message) => mod_.Log(message);
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
            };
            settings = new List<Settings>
            {
                new Settings(),
                new Settings(),
                new Settings(),
                new Settings(),
                new Settings(),
                new Settings(false),
                new Settings(false),
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
            foreach (string name in new List<string> 
            {
                "Tall Zoteling",
                "Normal Zoteling",
                "Ordeal Zoteling",
            })
            {
                spittedPrefabs.Add(prefabs[name]);
            }
        }
        public void UpgradeFSM(PlayMakerFSM fsm)
        {
            if (fsm.gameObject.scene.name == "GG_Grey_Prince_Zote" && fsm.gameObject.name == "Zote Crew Tall(Clone)" && fsm.FsmName == "Control")
            {
                Log("Upgrading FSM: " + fsm.gameObject.name + " - " + fsm.FsmName + ".");
                FsmUtil.AddTransition(fsm, "Dormant", "FINISHED", "Multiply");
                FsmUtil.RemoveAction(fsm, "Spawn Antic", 1);
                FsmUtil.RemoveAction(fsm, "Spawn Antic", 3);
                FsmUtil.AddCustomAction(fsm, "Spawn Antic", () => fsm.SendEvent("FINISHED"));
                FsmUtil.RemoveAction(fsm, "Tumble Out", 2);
                FsmUtil.RemoveAction(fsm, "Death", 0);
                FsmUtil.AddCustomAction(fsm, "Death Reset", () => 
                {
                    Object.Destroy(fsm.gameObject); 
                    easyMinionsCount -= 1;
                });
                Log("Upgraded FSM: " + fsm.gameObject.name + " - " + fsm.FsmName + ".");
            }
            else if (fsm.gameObject.scene.name == "GG_Grey_Prince_Zote" && fsm.gameObject.name == "Zote Crew Fat (1)(Clone)" && fsm.FsmName == "Control")
            {
                Log("Upgrading FSM: " + fsm.gameObject.name + " - " + fsm.FsmName + ".");
                FsmUtil.AddTransition(fsm, "Dormant", "FINISHED", "Multiply");
                FsmUtil.RemoveAction(fsm, "Spawn Antic", 1);
                FsmUtil.RemoveAction(fsm, "Spawn Antic", 3);
                FsmUtil.RemoveAction(fsm, "Spawn Antic", 5);
                FsmUtil.AddCustomAction(fsm, "Spawn Antic", () => fsm.SendEvent("FINISHED"));
                FsmUtil.RemoveAction(fsm, "Tumble Out", 2);
                FsmUtil.RemoveAction(fsm, "Dr", 1);
                FsmUtil.AddCustomAction(fsm, "Dr", () =>
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
                Log("Upgraded FSM: " + fsm.gameObject.name + " - " + fsm.FsmName + ".");
            }
            else if (fsm.gameObject.scene.name == "GG_Grey_Prince_Zote" && fsm.gameObject.name == "Zote Crew Normal (1)(Clone)" && fsm.FsmName == "Control")
            {
                Log("Upgrading FSM: " + fsm.gameObject.name + " - " + fsm.FsmName + ".");
                FsmUtil.AddTransition(fsm, "Dormant", "FINISHED", "Multiply");
                FsmUtil.RemoveAction(fsm, "Spawn Antic", 1);
                FsmUtil.RemoveAction(fsm, "Spawn Antic", 3);
                FsmUtil.AddCustomAction(fsm, "Spawn Antic", () => { fsm.SendEvent("FINISHED"); });
                FsmUtil.RemoveAction(fsm, "Tumble Out", 2);
                FsmUtil.RemoveAction(fsm, "Death", 0);
                FsmUtil.AddCustomAction(fsm, "Death Reset", () =>
                {
                    Object.Destroy(fsm.gameObject);
                    easyMinionsCount -= 1;
                });
                Log("Upgraded FSM: " + fsm.gameObject.name + " - " + fsm.FsmName + ".");
            }
            else if (fsm.gameObject.scene.name == "GG_Grey_Prince_Zote" && fsm.gameObject.name == "Ordeal Zoteling(Clone)" && fsm.FsmName == "Control")
            {
                Log("Upgrading FSM: " + fsm.gameObject.name + " - " + fsm.FsmName + ".");
                FsmUtil.AddTransition(fsm, "Dormant", "FINISHED", "Ball");
                FsmUtil.RemoveAction(fsm, "Ball", 2);
                FsmUtil.AddCustomAction(fsm, "Reset", () =>
                {
                    Object.Destroy(fsm.gameObject);
                    easyMinionsCount -= 1;
                });
                Log("Upgraded FSM: " + fsm.gameObject.name + " - " + fsm.FsmName + ".");
            }
            else if (fsm.gameObject.scene.name == "GG_Grey_Prince_Zote" && fsm.gameObject.name == "Zote Salubra(Clone)" && fsm.FsmName == "Control")
            {
                Log("Upgrading FSM: " + fsm.gameObject.name + " - " + fsm.FsmName + ".");
                FsmUtil.AddTransition(fsm, "Dormant", "FINISHED", "Appear");
                FsmUtil.RemoveAction(fsm, "Appear", 3);
                FsmUtil.RemoveAction(fsm, "Idle", 0);
                FsmUtil.RemoveAction(fsm, "Idle", 0);
                var ghostMovement = FsmUtil.GetAction<HutongGames.PlayMaker.Actions.GhostMovement>(fsm, "Sucking", 8);
                ghostMovement.xPosMin = 6;
                ghostMovement.xPosMax = 47;
                Log("Upgraded FSM: " + fsm.gameObject.name + " - " + fsm.FsmName + ".");
            }
            else if (fsm.gameObject.scene.name == "GG_Grey_Prince_Zote" && fsm.gameObject.name == "Zote Fluke(Clone)" && fsm.FsmName == "Control")
            {
                Log("Upgrading FSM: " + fsm.gameObject.name + " - " + fsm.FsmName + ".");
                FsmUtil.AddTransition(fsm, "Dormant", "FINISHED", "Pos");
                FsmUtil.RemoveAction(fsm, "Pos", 3);
                Log("Upgraded FSM: " + fsm.gameObject.name + " - " + fsm.FsmName + ".");
            }
        }
    }
}
