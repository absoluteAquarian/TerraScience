using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace TerraScience.Dusts{
	public class ElementGasDust : ModDust{
		public override Color? GetAlpha(Dust dust, Color lightColor){
			//If we've stored data, use it
			if(dust.customData is Color color)
				return color;
			return null;
		}

		public override void OnSpawn(Dust dust){
			dust.fadeIn = 1f;
			dust.noGravity = true;
			dust.frame = new Rectangle(0, 0, 7, 7);
		}
	}
}
