using System.Collections.Generic;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using TerraScience.API.UI;
using TerraScience.Content.TileEntities.Energy;
using TerraScience.Content.Tiles.Multitiles.EnergyMachines;

namespace TerraScience.Content.UI.Energy{
	public class ComposterUI : PoweredMachineUI{
		public override int TileType => ModContent.TileType<Composter>();

		public override string Header => "Composter";

		internal override void PanelSize(out int width, out int height){
			width = 300;
			height = 250;
		}

		internal override void InitializeText(List<UIText> text){
			UIText power = new UIText("Power: 0 / 0TF"){
				HAlign = 0.5f
			};
			power.Top.Set(48, 0);
			text.Add(power);

			UIText progress = new UIText("Progress: 0%", 1.3f){
				HAlign = 0.5f
			};
			progress.Top.Set(78, 0);
			text.Add(progress);

			UIText compostProgress = new UIText("Composting: 0%", 1.3f){
				HAlign = 0.5f
			};
			compostProgress.Top.Set(118, 0);
			text.Add(compostProgress);
		}

		internal override void InitializeSlots(List<UIItemSlot> slots){
			UIItemSlot input = new UIItemSlot(){
				ValidItemFunc = item => item.IsAir || (UIEntity as ComposterEntity).CanInputItem(0, item),
				HAlign = 0.2f
			};
			input.Top.Set(160, 0);
			slots.Add(input);

			UIItemSlot output = new UIItemSlot(){
				ValidItemFunc = item => item.IsAir,
				HAlign = 0.8f
			};
			output.Top.Set(160, 0);
			slots.Add(output);
		}

		internal override void UpdateText(List<UIText> text){
			text[0].SetText(GetFluxString());
			text[1].SetText($"Progress: {UIDecimalFormat(UIEntity.ReactionProgress)}%");
			text[2].SetText($"Composting: {UIDecimalFormat((UIEntity as ComposterEntity).compostProgress)}%");
		}
	}
}
