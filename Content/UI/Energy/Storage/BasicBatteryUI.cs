using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.UI.Elements;
using TerraScience.Content.API.UI;
using TerraScience.Content.UI.Energy.Generators;
using TerraScience.Utilities;

namespace TerraScience.Content.UI.Energy.Storage{
	public class BasicBatteryUI : GeneratorUI{
		public override string Header => "Battery";

		public override Tile[,] Structure => TileUtils.Structures.BasicBattery;

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
