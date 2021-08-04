using Terraria;
using Terraria.ID;
using TerraScience.Content.Tiles.Multitiles.EnergyMachines;

namespace TerraScience.Content.Items.Placeable.Machines.Energy{
	public class ComposterItem : MachineItem<Composter>{
		public override string ItemName => "Composter";

		public override string ItemTooltip => "Crushes plants into Dirt";

		public override void SafeSetDefaults(){
			Item.width = 32;
			Item.height = 30;
			Item.scale = 0.9f;
			Item.rare = ItemRarityID.Green;
			Item.value = Item.buyPrice(silver: 1);
		}
	}
}
