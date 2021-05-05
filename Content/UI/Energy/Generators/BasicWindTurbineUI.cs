using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.UI.Elements;
using TerraScience.Content.API.UI;
using TerraScience.Content.UI.Energy.Generators;
using TerraScience.Utilities;

namespace TerraScience.Content.UI.Generators{
	public class BasicWindTurbineUI : GeneratorUI{
		public override string Header => "Wind Turbine";

		public override Tile[,] Structure => TileUtils.Structures.BasicWindTurbine;

		internal override void PanelSize(out int width, out int height){
			width = 300;
			height = 200;
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
		}

		internal override void UpdateText(List<UIText> text){
			text[0].SetText(GetFluxString());
			text[1].SetText(GetGenerationString());
		}

		internal override void InitializeSlots(List<UIItemSlot> slots){
			//No items lol
		}
	}
}
