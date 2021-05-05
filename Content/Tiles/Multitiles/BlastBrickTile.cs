using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerraScience.Content.Items.Placeable;

namespace TerraScience.Content.Tiles.Multitiles{
	public class BlastBrickTile : ModTile{
		public override void SetDefaults(){
			AddMapEntry(Color.DarkGray);

			Main.tileSolid[Type] = true;
			Main.tileBlockLight[Type] = true;
			Main.tileMerge[Type][TileID.GrayBrick] = true;
			Main.tileMerge[Type][TileID.RedBrick] = true;
			Main.tileMergeDirt[Type] = true;

			drop = ModContent.ItemType<BlastBrick>();

			mineResist = 3f;
			minPick = 45;

			soundType = SoundID.Tink;

			dustType = 54;
		}
	}
}
