using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using TerraScience.API.UI;
using TerraScience.Content.TileEntities.Energy;
using TerraScience.Content.Tiles.Multitiles.EnergyMachines;

namespace TerraScience.Content.UI.Energy{
	public class PulverizerUI : PoweredMachineUI{
		public override string Header => "Pulverizer";

		public override int TileType => ModContent.TileType<Pulverizer>();

		internal override void PanelSize(out int width, out int height){
			width = 500;
			height = 300;
		}

		internal override void InitializeText(List<UIText> text){
			UIText power = new UIText("Power: 0 / 0TF"){
				HAlign = 0.5f
			};
			power.Top.Set(48, 0);
			text.Add(power);

			UIText reactionSpeed = new UIText("Speed Multiplier: 1x", 1.3f) {
				HAlign = 0.5f
			};
			reactionSpeed.Top.Set(78, 0);
			text.Add(reactionSpeed);
		}

		internal override void InitializeSlots(List<UIItemSlotWrapper> slots){
			//Copied from ScienceWorkbenchUI lul
			int top = 150;
			int origLeft = 90, left;
			const int buffer = 8;

			UIItemSlotWrapper input = new UIItemSlotWrapper(){
				ValidItemFunc = item => item.IsAir || PulverizerEntity.inputToOutputs.ContainsKey(item.type),
				VAlign = 0.5f
			};
			input.Left.Set(20, 0);
			slots.Add(input);

			//12 slots, 6 per row
			for(int r = 0; r < 2; r++){
				left = origLeft;
				for(int c = 0; c < 6; c++){
					//Can't place items, only remove them
					UIItemSlotWrapper slot = new UIItemSlotWrapper(){
						ValidItemFunc = item => item.IsAir
					};
					slot.Left.Set(left, 0);
					slot.Top.Set(top, 0);

					left += TextureAssets.InventoryBack9.Value.Width + buffer;

					slots.Add(slot);
				}
				top += TextureAssets.InventoryBack9.Value.Height + buffer;
			}
		}

		internal override void UpdateText(List<UIText> text){
			text[0].SetText(GetFluxString());
			text[1].SetText($"Speed Multiplier: {UIDecimalFormat(UIEntity.ReactionSpeed)}x");
		}
	}
}
