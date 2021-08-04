using Terraria;
using Terraria.ID;
using TerraScience.Content.Tiles.Multitiles.EnergyMachines.Basic;

namespace TerraScience.Content.Items.Placeable.Machines.Energy.Generators{
	public class BasicThermoGeneratorItem : MachineItem<BasicThermoGenerator>{
		public override string ItemName => "Basic Thermal Generator";

		public override string ItemTooltip => "Converts certain foods and wooden items into Terra Flux (TF)";

		public override void SafeSetDefaults(){
			Item.width = 30;
			Item.height = 30;
			Item.scale = 0.82f;
			Item.rare = ItemRarityID.Blue;
			Item.value = Item.buyPrice(silver: 4, copper: 20);
		}
	}
}
