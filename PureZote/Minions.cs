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
        private readonly List<GameObject> prefabs = new();
        private List<Settings> settings;
        private readonly System.Random random = new();
        public Minions(Mod mod) =>mod_= mod;
        private void Log(string message) => mod_.Log(message);
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
            var names = new List<(string, string)>
            {
                ("Tall Zotes", "Zote Crew Tall"),
                ("Fat Zotes", "Zote Crew Fat (1)"),
                ("Dormant Warriors", "Zote Crew Normal (1)"),
                ("Zotelings", "Ordeal Zoteling"),
                ("Zotelings", "Ordeal Zoteling"),
                ("Zote Salubra",""),
                ("Zote Fluke",""),
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
            foreach ((string group, string instance) in names)
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
                prefabs.Add(minion);
                Log("Minion added: " + minion.name + ".");
            }
        }
        public void UpgradeFSM(PlayMakerFSM fsm)
        {
            if (fsm.gameObject.scene.name == "GG_Grey_Prince_Zote" && fsm.gameObject.name == "Grey Prince" && fsm.FsmName == "Control")
            {
                void Spit(PlayMakerFSM fsm)
                {
                    Log("Spitting.");
                    var zoteling = fsm.FsmVariables.FindFsmGameObject("Zoteling").Value;
                    zoteling.GetComponent<Renderer>().enabled = false;
                    var index = random.Next(prefabs.Count);
                    var minion = Object.Instantiate(prefabs[index]);
                    var settings_ = settings[index];
                    minion.SetActive(true);
                    minion.SetActiveChildren(true);
                    minion.transform.position = zoteling.transform.position;
                    if (settings_.enableSpittingVelocity_)
                    {
                        minion.GetComponent<Rigidbody2D>().velocity = zoteling.GetComponent<Rigidbody2D>().velocity;
                    }
                    Log("Spat.");
                }
                Log("Upgrading FSM: " + fsm.gameObject.name + " - " + fsm.FsmName + ".");
                FsmUtil.RemoveAction(fsm, "Spit Antic", 3);
                FsmUtil.RemoveAction(fsm, "Spit L", 7);
                FsmUtil.AddCustomAction(fsm, "Spit L", () => Spit(fsm));
                FsmUtil.RemoveAction(fsm, "Spit R", 7);
                FsmUtil.AddCustomAction(fsm, "Spit R", () => Spit(fsm));
                Log("Upgraded FSM: " + fsm.gameObject.name + " - " + fsm.FsmName + ".");
            }
            else if (fsm.gameObject.scene.name == "GG_Grey_Prince_Zote" && fsm.gameObject.name == "Zote Crew Tall(Clone)" && fsm.FsmName == "Control")
            {
                Log("Upgrading FSM: " + fsm.gameObject.name + " - " + fsm.FsmName + ".");
                FsmUtil.AddTransition(fsm, "Dormant", "FINISHED", "Multiply");
                FsmUtil.RemoveAction(fsm, "Spawn Antic", 1);
                FsmUtil.RemoveAction(fsm, "Spawn Antic", 3);
                FsmUtil.AddCustomAction(fsm, "Spawn Antic", () => { fsm.SendEvent("FINISHED"); });
                FsmUtil.RemoveAction(fsm, "Tumble Out", 2);
                Log("Upgraded FSM: " + fsm.gameObject.name + " - " + fsm.FsmName + ".");
            }
            else if (fsm.gameObject.scene.name == "GG_Grey_Prince_Zote" && fsm.gameObject.name == "Zote Crew Fat (1)(Clone)" && fsm.FsmName == "Control")
            {
                Log("Upgrading FSM: " + fsm.gameObject.name + " - " + fsm.FsmName + ".");
                FsmUtil.AddTransition(fsm, "Dormant", "FINISHED", "Multiply");
                FsmUtil.RemoveAction(fsm, "Spawn Antic", 1);
                FsmUtil.RemoveAction(fsm, "Spawn Antic", 3);
                FsmUtil.RemoveAction(fsm, "Spawn Antic", 5);
                FsmUtil.AddCustomAction(fsm, "Spawn Antic", () => { fsm.SendEvent("FINISHED"); });
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
                Log("Upgraded FSM: " + fsm.gameObject.name + " - " + fsm.FsmName + ".");
            }
            else if (fsm.gameObject.scene.name == "GG_Grey_Prince_Zote" && fsm.gameObject.name == "Ordeal Zoteling(Clone)" && fsm.FsmName == "Control")
            {
                Log("Upgrading FSM: " + fsm.gameObject.name + " - " + fsm.FsmName + ".");
                FsmUtil.AddTransition(fsm, "Dormant", "FINISHED", "Ball");
                FsmUtil.RemoveAction(fsm, "Ball", 2);
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
                FsmUtil.RemoveAction(fsm, "Sucking", 8);
                FsmUtil.InsertAction(fsm, "Sucking", ghostMovement, 8);
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
