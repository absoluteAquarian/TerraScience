using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerraScience.Content.Items.Placeable;

namespace TerraScience.Content.Tiles{
	public class AdvancedItemTransportTile : ItemTransportTile{
		public override float TransferProgressPerTick => 5 / 60f;  //Moves through 5 tiles per second

		public override void SafeSetDefaults(){
			AddMapEntry(Color.MediumPurple);
			drop = ModContent.ItemType<AdvancedItemTransport>();

			Main.tileMerge[TileID.Containers][Type] = true;
			Main.tileMerge[TileID.Containers2][Type] = true;
			Main.tileMerge[Type][TileID.Containers] = true;
			Main.tileMerge[Type][TileID.Containers2] = true;
		}
	}
}
