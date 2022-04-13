using System.Collections.Generic;
using UnityEngine;
using Modding;
using Satchel;
using HutongGames.PlayMaker.Actions;


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
        private void Log(object message) => mod_.LogDebug(message);
        public List<(string, string)> GetPreloadNames()
        {
            return new List<(string, string)>
            {
                 ("GG_Traitor_Lord", "Battle Scene"),
            };
        }
        private void LoadTraitorLordWavePrefab(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
        {
            var battleScene = preloadedObjects["GG_Traitor_Lord"]["Battle Scene"];
            var traitorLord = battleScene.transform.Find("Wave 3").gameObject.transform.Find("Mantis Traitor Lord").gameObject;
            var fsm = traitorLord.LocateMyFSM("Mantis");
            var wave = fsm.GetAction<SpawnObjectFromGlobalPool>("Waves", 0).gameObject.Value;
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
