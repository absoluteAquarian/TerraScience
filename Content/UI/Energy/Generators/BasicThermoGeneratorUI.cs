using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.ModLoader;
using TerraScience.Content.API.UI;
using TerraScience.Content.Items.Materials;
using TerraScience.Content.Tiles.Multitiles.EnergyMachines.Basic;

namespace TerraScience.Content.UI.Energy.Generators{
	public class BasicThermoGeneratorUI : GeneratorUI{
		public override string Header => "Thermal Generator";

		public override int TileType => ModContent.TileType<BasicThermoGenerator>();

		internal override void PanelSize(out int width, out int height){
			width = 450;
			height = 300;
		}

		internal override void InitializeText(List<UIText> text){
			UIText powerAmount = new UIText("Power: 0/0 TF"){
				HAlign = 0.5f
			};
			powerAmount.Top.Set(58, 0);
			text.Add(powerAmount);

			UIText powerGen = new UIText("0.00 TF/s"){
				HAlign = 0.5f
			};
			powerGen.Top.Set(87, 0);
			text.Add(powerGen);

			UIText progress = new UIText("Progress: 0%"){
				HAlign = 0.5f
			};
			progress.Top.Set(120, 0);
			text.Add(progress);
		}

		public static bool ValidItem(Item item){
			string name = Lang.GetItemNameValue(item.type);
			return item.IsAir || name.Contains("Wood") || name.Contains("wood") || item.buffType == BuffID.WellFed || item.buffType == BuffID.Tipsy || item.type == ModContent.ItemType<Coal>();
		}

		internal override void InitializeSlots(List<UIItemSlot> slots){
			UIItemSlot fuel = new UIItemSlot(){
				HAlign = 0.5f,
				ValidItemFunc = item => ValidItem(item)
			};
			fuel.Top.Set(200, 0);
			slots.Add(fuel);
		}

		internal override void UpdateText(List<UIText> text){
			text[0].SetText(GetFluxString());
			text[1].SetText(GetGenerationString());
			text[2].SetText($"Progress: {UIDecimalFormat(UIEntity.ReactionProgress)}%");
		}
	}
}
