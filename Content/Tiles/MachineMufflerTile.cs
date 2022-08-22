using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.ObjectData;
using TerraScience.Content.Items.Placeable;
using TerraScience.Content.TileEntities;
using TerraScience.Utilities;

namespace TerraScience.Content.Tiles{
	public class MachineMufflerTile : ModTile{
		public static List<Point16> mufflers;

		public static bool AnyMufflersNearby(Vector2 checkPos){
			//40 tile radius
			const float range = 40 * 16;

			foreach(var point in mufflers)
				if(Vector2.DistanceSquared(checkPos, point.ToWorldCoordinates()) < range * range)
					return true;

			return false;
		}

		public static bool AnyMufflersNearby(Point16 checkPos)
			=> AnyMufflersNearby(checkPos.ToWorldCoordinates());

		public static bool AnyMufflersNearby(MachineEntity entity)
			=> AnyMufflersNearby(TileUtils.TileEntityCenter(entity, entity.MachineTile));

		public override void SetStaticDefaults(){
			Main.tileNoAttach[Type] = true;
			Main.tileFrameImportant[Type] = true;
			Main.tileBlockLight[Type] = true;

			TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
			TileObjectData.newTile.Origin = new Point16(0, 1);
			TileObjectData.newTile.CoordinateHeights = new[]{ 16, 16 };
			TileObjectData.newTile.StyleHorizontal = true;
			TileObjectData.newTile.Style = 0;
			TileObjectData.addTile(Type);

			ModTranslation name = CreateMapEntryName();
			name.SetDefault("Machine Muffler");
			AddMapEntry(Color.White, name);
		}

		public override void PlaceInWorld(int i, int j, Item item){
			mufflers.Add(new Point16(i, j - 1));
			mufflers.Add(new Point16(i + 1, j - 1));
			mufflers.Add(new Point16(i, j));
			mufflers.Add(new Point16(i + 1, j));
		}

		public override void KillMultiTile(int i, int j, int frameX, int frameY){
			Point16 orig = new Point16(i - frameX / 18, j - frameY / 18);

			mufflers.Remove(orig);
			mufflers.Remove(orig + new Point16(1, 0));
			mufflers.Remove(orig + new Point16(0, 1));
			mufflers.Remove(orig + new Point16(1, 1));
			Item.NewItem(new Vector2(i * 16, j * 16), new Vector2(32, 32), ModContent.ItemType<MachineMuffler>());
		}
	}
}
