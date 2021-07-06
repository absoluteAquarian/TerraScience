using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using TerraScience.Content.Items.Energy;
using TerraScience.Systems;
using TerraScience.Systems.Energy;

namespace TerraScience.Content.Tiles.Energy{
	public class TFWireTile : JunctionMergeable{
		public override JunctionType MergeType => JunctionType.Wires;

		public virtual TerraFlux Capacity => new TerraFlux(200);

		public virtual TerraFlux ImportRate => new TerraFlux(2000 / 60f);
		public virtual TerraFlux ExportRate => new TerraFlux(2000 / 60f);

		public override void SafeSetDefaults(){
			AddMapEntry(Color.Orange);
			drop = ModContent.ItemType<TFWireItem>();
		}

		public override void PlaceInWorld(int i, int j, Item item){
			base.PlaceInWorld(i, j, item);

			NetworkCollection.OnWirePlace(new Point16(i, j));
		}

		public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem){
			if(!fail){
				//Tile was mined.  Update the networks
				NetworkCollection.OnWireKill(new Point16(i, j));
			}
		}
	}
}
