using System.IO;
using Terraria.ID;
using Terraria.ModLoader;
using TerraScience.Common.Systems;

namespace TerraScience {
	public class TechMod : Mod {
		public static TechMod Instance => ModContent.GetInstance<TechMod>();

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
				public static float[] ConversionDuration = Factory.CreateCustomSet(4f * 60,
					ItemID.BorealWood, 5f * 60,
					ItemID.RichMahogany, 3.5f * 60,
					ItemID.PalmWood, 3.75f * 60);
			}
		}
	}
}
