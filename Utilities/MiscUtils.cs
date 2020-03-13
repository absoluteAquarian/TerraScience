using Microsoft.Xna.Framework;
using System;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace TerraScience.Utilities {
	public static class MiscUtils {
		/// <summary>
		/// Blends the two colours together with a 50% bias.
		/// </summary>
		/// <param name="color"></param>
		/// <param name="otherColor"></param>
		public static Color Blend(Color color, Color otherColor)
			=> FadeBetween(color, otherColor, 0.5f);

		/// <summary>
		/// Blends the two colours with the given % bias towards "toColor".  Thanks direwolf420!
		/// </summary>
		/// <param name="fromColor">The original colour.</param>
		/// <param name="toColor">The colour being blended towards</param>
		/// <param name="fadePercent">The % bias towards "toColor".  Range: [0,1]</param>
		public static Color FadeBetween(Color fromColor, Color toColor, float fadePercent)
			=> fadePercent == 0f ? fromColor : new Color(fromColor.ToVector4() * (1f - fadePercent) + toColor.ToVector4() * fadePercent);

		public static T ParseToEnum<T>(string name) where T : Enum
			=> (T)Enum.Parse(typeof(T), name);

		public static bool TryGetTileEntity<T>(Point16 position, out T tileEntity) where T : TileEntity{
			tileEntity = null;
			if(TileEntity.ByPosition.ContainsKey(position))
				tileEntity = TileEntity.ByPosition[position] as T;	//'as' will make 'tileEntity' null if the TileEntity at the position isn't the same type
			return tileEntity != null;
		}

		public static bool HeldItemCanPlaceWater(this Player player)
			=> player.HeldItem.type == ItemID.WaterBucket || player.HeldItem.type == ItemID.BottomlessBucket;

		public static Vector2 ScreenCenter()
			=> new Vector2(Main.screenWidth, Main.screenHeight) / 2f;
	}
}
