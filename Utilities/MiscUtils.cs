using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using TerraScience.Content.API.UI;
using TerraScience.Content.Items.Materials;
using TerraScience.Content.TileEntities;
using static TerraScience.Content.TileEntities.SaltExtractorEntity;

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

		public static bool HeldItemIsViableForSaltExtractor(this Player player, Point16 pos){
			if(!TryGetTileEntity(pos, out SaltExtractorEntity se))
				return false;

			int[] types = new int[]{
				ItemID.WaterBucket,
				ItemID.BottomlessBucket,
				ModContent.ItemType<Vial_Water>(),
				ModContent.ItemType<Vial_Saltwater>()
			};

			if(!types.Contains(player.HeldItem.type))
				return false;

			//Liquid: none
			if(se.LiquidType == SE_LiquidType.None)
				return true;

			//Liquid: water
			if((player.HeldItem.type == ItemID.WaterBucket || player.HeldItem.type == ItemID.BottomlessBucket || player.HeldItem.type == ModContent.ItemType<Vial_Water>())
				&& se.LiquidType == SE_LiquidType.Water)
				return true;

			//Liquid: saltwater
			if(player.HeldItem.type == ModContent.ItemType<Vial_Saltwater>() && se.LiquidType == SE_LiquidType.Saltwater)
				return true;

			//Bad
			return false;
		}

		public static Vector2 ScreenCenter()
			=> new Vector2(Main.screenWidth, Main.screenHeight) / 2f;

		/// <summary>
		/// Parses an Element or Compound name to its Enum value.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="value">The value returned.  Defaults to 'null' if the name couldn't be found.</param>
		/// <returns></returns>
		public static bool TryParseUnknownName(string name, out Enum value){
			value = null;
			if(Enum.GetNames(typeof(Element)).Contains(name))
				value = ParseToEnum<Element>(name);
			else if(Enum.GetNames(typeof(Compound)).Contains(name))
				value = ParseToEnum<Compound>(name);
			return value != null;
		}

		public static T[] Create1DArray<T>(T value, uint length){
			T[] arr = new T[length];
			for(uint i = 0; i < length; i++)
				arr[i] = value;
			return arr;
		}

		//This added offset is needed to draw the bars at the right positions during different lighting modes
		public static Vector2 GetLightingDrawOffset() => Lighting.NotRetro ? new Vector2(12) * 16 : Vector2.Zero;
	}
}
