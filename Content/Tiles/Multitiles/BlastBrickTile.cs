using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerraScience.Content.Items.Placeable;

namespace TerraScience.Content.Tiles.Multitiles{
	public class BlastBrickTile : ModTile{
		public override void SetStaticDefaults(){
			AddMapEntry(Color.DarkGray);

			Main.tileSolid[Type] = true;
			Main.tileBlockLight[Type] = true;
			Main.tileMerge[Type][TileID.GrayBrick] = true;
			Main.tileMerge[Type][TileID.RedBrick] = true;
			Main.tileMergeDirt[Type] = true;

			ItemDrop = ModContent.ItemType<BlastBrick>();

			MineResist = 3f;
			MinPick = 45;

			SoundType = SoundID.Tink;

			DustType = 54;
		}
	}
}
