using System.Collections.Generic;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using TerraScience.API.UI;
using TerraScience.Content.TileEntities.Energy.Generators;
using TerraScience.Content.Tiles.Multitiles.EnergyMachines.Basic;

namespace TerraScience.Content.UI.Energy.Generators{
	public class BasicWindTurbineUI : GeneratorUI{
		public override string Header => "Wind Turbine";

		public override int TileType => ModContent.TileType<BasicWindTurbine>();

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

			UIText boost = new UIText("<Boost>"){
				HAlign = 0.5f
			};
			boost.Top.Set(120, 0);
			text.Add(boost);

			UIText boost2 = new UIText("<Boost>"){
				HAlign = 0.5f
			};
			boost.Top.Set(140, 0);
			text.Add(boost2);
		}

		internal override void UpdateText(List<UIText> text){
			text[0].SetText(GetFluxString());
			text[1].SetText(GetGenerationString());

			BasicWindTurbineEntity entity = UIEntity as BasicWindTurbineEntity;

			if(entity.rainBoost && !entity.sandstormBoost){
				text[2].SetText("Power Output Boosted by Rain");
				text[3].SetText("");
			}else if(!entity.rainBoost && entity.sandstormBoost){
				text[2].SetText("Power Output Boosted by Sandstorm");
				text[3].SetText("");
			}else if(entity.rainBoost && entity.sandstormBoost){
				text[2].SetText("Power Output Boosted by Rain");
				text[3].SetText("Power Output Boosted by Sandstorm");
			}else{
				text[2].SetText("");
				text[3].SetText("");
			}
		}

		internal override void InitializeSlots(List<UIItemSlot> slots){
			//No items lol
		}
	}
}
