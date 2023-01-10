using Microsoft.Xna.Framework;
using SerousEnergyLib.API;
using SerousEnergyLib.Tiles;
using Terraria.ID;
using Terraria.ModLoader;

namespace TerraScience {
	public class TechMod : Mod {
		public static TechMod Instance => ModContent.GetInstance<TechMod>();

		public static string GetEffectPath<T>(string effect) where T : ModTile, IMachineTile {
			return $"TerraScience/Assets/Tiles/Machines/Effect_{typeof(T).Name}_{effect}";
		}

		public static class Sets {
			public static SetFactory Factory = new SetFactory(ItemLoader.ItemCount);

			public static class ReinforcedFurnace {
				/// <summary>
				/// The minimum temperature in Celsius needed to start converting an input item
				/// </summary>
				public static double[] MinimumHeatForConversion = Factory.CreateCustomSet(-1d,
					ItemID.Wood, 300d,
					ItemID.BorealWood, 300d,
					ItemID.RichMahogany, 300d,
					ItemID.Ebonwood, 300d,
					ItemID.Shadewood, 300d,
					ItemID.PalmWood, 300d,
					ItemID.Pearlwood, 300d);

				/// <summary>
				/// The amount of game ticks it takes to convert one item, before applying speed bonuses
				/// </summary>
				public static Ticks[] ConversionDuration = Factory.CreateCustomSet(Ticks.FromSeconds(4),
					ItemID.BorealWood, Ticks.FromSeconds(5),
					ItemID.RichMahogany, Ticks.FromSeconds(3.5),
					ItemID.PalmWood, Ticks.FromSeconds(3.75));

				public static MachineSpriteEffectInformation[] ItemInFurnace = Factory.CreateCustomSet(default(MachineSpriteEffectInformation));
			}
		}
	}
}
