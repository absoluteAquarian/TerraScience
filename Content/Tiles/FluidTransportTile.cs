using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using TerraScience.Content.Items.Placeable;
using TerraScience.Systems;

namespace TerraScience.Content.Tiles{
	public class FluidTransportTile : JunctionMergeable{
		public override JunctionType MergeType => JunctionType.Fluids;

		public virtual float Capacity => 0.25f;  //0.25L

		public override void SafeSetDefaults(){
			AddMapEntry(Color.DarkBlue);
			drop = ModContent.ItemType<FluidTransport>();
		}

		public override void PlaceInWorld(int i, int j, Item item){
			base.PlaceInWorld(i, j, item);

			NetworkCollection.OnFluidPipePlace(new Point16(i, j));
		}

		public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem){
			if(!fail){
				//Tile was mined.  Update the networks
				NetworkCollection.OnFluidPipeKill(new Point16(i, j));
			}
		}
	}
}
