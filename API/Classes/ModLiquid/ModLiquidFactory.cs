using System.Collections.Generic;
using Terraria.ModLoader;
using TerraScience.Systems.TemperatureSystem;

namespace TerraScience.API.Classes.ModLiquid {
	// Instance of this class in ModContent.GetInstance<TerraScience>().LiquidFactory where it calls LiquidFactory.Load in Mod.Load.
	public class ModLiquidFactory {
        public Dictionary<string, ModLiquid> Liquids { get; } = new Dictionary<string, ModLiquid>();

		public ModLiquid Create(string internalName, string displayName, string texturePath, DefaultTemperature defaultTemp) {
			var liquid = new ModLiquid(internalName, displayName, texturePath, defaultTemp);
            Liquids.Add(liquid.InternalName, liquid);

			ModLiquidRenderer.Instance.textures.Add(ModContent.GetTexture(texturePath));

			ModLiquidManager.LastAddedLiquidID++;

			ModContent.GetInstance<TerraScience>().temperatureSystem.DefaultLiquidTemps.Add(liquid.InternalName, liquid.DefaultTemp);

			return liquid;
		}
	}
}