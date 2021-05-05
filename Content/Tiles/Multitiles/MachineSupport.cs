using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace TerraScience.Content.Tiles.Multitiles{
	public class MachineSupport : ModTile{
		public override void SetDefaults(){
			Main.tileSolid[Type] = true;
			Main.tileBlockLight[Type] = true;

			AddMapEntry(new Color(30, 30, 30));

			dustType = 82;
			drop = ModContent.ItemType<Items.Placeable.MachineSupportItem>();
			soundType = 21;
			soundStyle = 1;

			mineResist = 2;
		}
	}
}
