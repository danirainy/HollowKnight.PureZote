using System.Collections.Generic;
using UnityEngine;
using Modding;
using Satchel;


namespace PureZote
{
    public class Projectiles
    {
        private readonly Mod mod_;
        private readonly Common common;
        public readonly Dictionary<string, GameObject> prefabs = new();
        public Projectiles(Mod mod)
        {
            mod_ = mod;
            common = new Common(mod);
        }
        private void Log(object message) => mod_.Log(message);
        public List<(string, string)> GetPreloadNames()
        {
            return new List<(string, string)>
            {
                 ("GG_Traitor_Lord","Battle Scene"),
            };
        }
        private void LoadTraitorLordWavePrefab(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
        {
            var battleScene = preloadedObjects["GG_Traitor_Lord"]["Battle Scene"];
            var traitorLord = battleScene.transform.Find("Wave 3").gameObject.transform.Find("Mantis Traitor Lord").gameObject;
            var fsms = traitorLord.GetComponents<PlayMakerFSM>();
            PlayMakerFSM fsm = null;
            for (int i = 0; i < fsms.Length; i++)
            {
                if (fsms[i].FsmName == "Mantis")
                {
                    fsm = fsms[i];
                }
            }
            var wave = FsmUtil.GetAction<HutongGames.PlayMaker.Actions.SpawnObjectFromGlobalPool>(fsm, "Waves", 0).gameObject.Value;
            wave.transform.Find("slash_core").gameObject.transform.Find("hurtbox").gameObject.GetComponent<DamageHero>().damageDealt = 1;
            prefabs["traitorLordWave"] = wave;
        }
        public void LoadPrefabs(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
        {
            LoadTraitorLordWavePrefab(preloadedObjects);
        }
        public void UpgradeFSM(PlayMakerFSM fsm)
        {
        }
    }
}
