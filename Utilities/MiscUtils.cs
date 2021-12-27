using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using TerraScience.Content.ID;
using TerraScience.Content.Items.Materials;
using TerraScience.Content.TileEntities;
using TerraScience.Content.Tiles.Multitiles;

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

		public static MachineFluidID GetFluidIDFromItem(int type){
			if(type == ItemID.WaterBucket || type == ItemID.BottomlessBucket || type == ModContent.ItemType<Vial_Water>())
				return MachineFluidID.LiquidWater;
			else if(type == ModContent.ItemType<Vial_Saltwater>())
				return MachineFluidID.LiquidSaltwater;
			else if(type == ItemID.LavaBucket)
				return MachineFluidID.LiquidLava;
			else if(type == ItemID.HoneyBucket)
				return MachineFluidID.LiquidHoney;
			return MachineFluidID.None;
		}

		public static bool IsLiquidID(this MachineFluidID id)
			=> id.EnumName().Contains("Liquid");

		public static bool IsGasID(this MachineFluidID id)
			=> id.EnumName().Contains("Gas");

		public static Vector2 ScreenCenter()
			=> new Vector2(Main.screenWidth, Main.screenHeight) / 2f;

		public static T[] Create1DArray<T>(T value, uint length){
			T[] arr = new T[length];
			for(uint i = 0; i < length; i++)
				arr[i] = value;
			return arr;
		}

		public static Vector2 GetLightingDrawOffset(){
			bool doOffset = Lighting.NotRetro;
			if(!doOffset && Main.GameZoomTarget != 1)
				doOffset = true;

			return doOffset ? new Vector2(12) * 16 : Vector2.Zero;
		}

		public static int GetIconType(this Machine machine){
			machine.GetDefaultParams(out _, out _, out _, out int type);
			return type;
		}

		public static Item RetrieveItem(this MachineEntity entity, int slot)
			=> entity.HijackRetrieveItem(slot, out Item item)
				? item
				: !(entity.ParentState?.Active ?? false)
					? entity.GetItem(slot)
					: entity.ParentState.GetSlot(slot).StoredItem;

		public static void ClearItem(this MachineEntity entity, int slot)
			=> entity.RetrieveItem(slot).TurnToAir();

		/// <summary>
		/// Converts this one-dimentional array to a two-dimensional array whose dimensions are the given <paramref name="width"/> and <paramref name="height"/>.
		/// </summary>
		public static T[,] To2DArray<T>(this T[] arr, int width, int height){
			if(arr.Length != width * height)
				throw new ArgumentException($"Array length does not match width and height parameters (length: {arr.Length}, result columns: {width}, result rows: {height})");

			T[,] newArr = new T[width, height];

			int arrIndex = 0;
			for(int y = 0; y < height; y++){
				for(int x = 0; x < width; x++){
					newArr[x, y] = arr[arrIndex++];
				}
			}

			return newArr;
		}

		public static string EnumName<T>(this T id) where T : Enum => Enum.GetName(typeof(T), id);

		public static string ProperEnumName<T>(this T id) where T : Enum => Regex.Replace(id.EnumName(), "([A-Z])", " $1").Trim();

		public static Color MixLightColors(Color light, Color source) => new Color(light.ToVector3() * source.ToVector3());

		public static bool TrueForAny<T>(this List<T> list, Predicate<T> predicate){
			for(int i = 0; i < list.Count; i++){
				if(predicate(list[i]))
					return true;
			}

			return false;
		}

		public static T[] Populate<T>(this T[] arr, Func<T> defaultValue) where T : class{
			for(int i = 0; i < arr.Length; i++)
				arr[i] = defaultValue();
			return arr;
		}

		public static T[] Populate<T>(this T[] arr, T defaultValue) where T : struct{
			for(int i = 0; i < arr.Length; i++)
				arr[i] = defaultValue;
			return arr;
		}

		public static void Resize(this ref Rectangle? rect, int widthOffset = 0, int heightOffset = 0){
			if(rect is Rectangle r)
				rect = new Rectangle(r.X, r.Y, r.Width + widthOffset, r.Height + heightOffset);
		}

		public static double GetElapsedMicroseconds(this Stopwatch watch)
			=> watch.ElapsedTicks * 1e6d / Stopwatch.Frequency;

		public static double GetElapsedNanoseconds(this Stopwatch watch)
			=> watch.ElapsedTicks * 1e9d / Stopwatch.Frequency;

		public static void FindAndModify(List<TooltipLine> tooltips, string searchPhrase, Func<string> replacePhrase){
			int searchIndex = tooltips.FindIndex(t => t.text.Contains(searchPhrase));
			if(searchIndex >= 0)
				tooltips[searchIndex].text = tooltips[searchIndex].text.Replace(searchPhrase, replacePhrase());
		}

		public static void FindAndInsertLines(List<TooltipLine> tooltips, string searchLine, Func<string> replaceLines){
			int searchIndex = tooltips.FindIndex(t => t.text == searchLine);
			if(searchIndex >= 0){
				tooltips.RemoveAt(searchIndex);

				string lines = replaceLines();

				int inserted = 0;
				foreach(var line in lines.Split(new[]{ '\n' }, StringSplitOptions.RemoveEmptyEntries)){
					tooltips.Insert(searchIndex++, new TooltipLine(TechMod.Instance, "DebugToolLine" + inserted, line));
					inserted++;
				}
			}
		}
	}
}
