using System.Collections.Generic;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using TerraScience.API.UI;
using TerraScience.Content.Tiles.Multitiles.EnergyMachines.Storage;
using TerraScience.Content.UI.Energy.Generators;

namespace TerraScience.Content.UI.Energy.Storage{
	public class BasicBatteryUI : GeneratorUI{
		public override string Header => "Battery";

		public override int TileType => ModContent.TileType<BasicBattery>();

		internal override void PanelSize(out int width, out int height){
			width = 300;
			height = 120;
		}

		internal override void InitializeText(List<UIText> text){
			UIText powerAmount = new UIText("Power: 0/0 TF"){
				HAlign = 0.5f
			};
			powerAmount.Top.Set(58, 0);
			text.Add(powerAmount);
		}

		internal override void UpdateText(List<UIText> text){
			text[0].SetText(GetFluxString());
		}

		internal override void InitializeSlots(List<UIItemSlot> slots){
			//No items lol
		}
	}
}
