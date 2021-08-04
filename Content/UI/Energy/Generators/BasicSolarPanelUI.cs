using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using TerraScience.API.UI;
using TerraScience.Content.TileEntities.Energy.Generators;
using TerraScience.Content.Tiles.Multitiles.EnergyMachines.Basic;

namespace TerraScience.Content.UI.Energy.Generators{
	public class BasicSolarPanelUI : GeneratorUI{
		public override string Header => "Basic Solar Panel";

		public override int TileType => ModContent.TileType<BasicSolarPanel>();

		internal override void PanelSize(out int width, out int height){
			width = 380;
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

			BasicSolarPanelEntity entity = UIEntity as BasicSolarPanelEntity;

			if(!Main.dayTime){
				text[2].SetText("Sun Not Visible");
				text[3].SetText("");
			}else{
				if(entity.eclipseReduce && !entity.rainReduce){
					text[2].SetText("Power Output Reduced by Eclipse");
					text[3].SetText("");
				}else if(!entity.eclipseReduce && entity.rainReduce){
					text[2].SetText("Power Output Reduced by Rain");
					text[3].SetText("");
				}else if(entity.eclipseReduce && entity.rainReduce){
					text[2].SetText("Power Output Reduced by Eclipse");
					text[3].SetText("Power Output Reduced by Rain");
				}else{
					text[2].SetText("");
					text[3].SetText("");
				}
			}
		}

		internal override void InitializeSlots(List<UIItemSlotWrapper> slots){
			//No items lol
		}
	}
}
