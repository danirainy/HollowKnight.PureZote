using Modding;
using UnityEngine;

namespace PureZote
{
	public class Palette
	{
		private readonly Mod mod_;
		public Palette(Mod mod) => mod_ = mod;
		public void SetLighting(On.SceneManager.orig_SetLighting original, Color ambientLightColor, float ambientLightIntensity)
		{
			if (GameManager.instance != null && GameManager.instance.IsGameplayScene() && GameManager.instance.sceneName == "GG_Grey_Prince_Zote")
			{
				ambientLightColor = new Color(0.25f, 0.25f, 0.25f, 1);
			}
			original(ambientLightColor, ambientLightIntensity);
		}
	}
}