using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using TerraScience.API.UI;
using TerraScience.Content.Items.Materials;
using TerraScience.Content.Tiles.Multitiles;
using TerraScience.Utilities;
using UIItemSlot = TerraScience.API.UI.UIItemSlot;

namespace TerraScience.Content.UI{
	public class BlastFurnaceUI : MachineUI{
		public override string Header => "Blast Furnace";

		public override int TileType => ModContent.TileType<BlastFurnace>();

		internal override void PanelSize(out int width, out int height){
			width = 400;
			height = 300;
		}

		internal override void InitializeSlots(List<UIItemSlot> slots){
			UIItemSlot ore = new UIItemSlot(){
				HAlign = 0.1f,
				ValidItemFunc = item => item.IsAir || ItemUtils.IsOre(item)
			};
			ore.Top.Set(150, 0);
			slots.Add(ore);

			UIItemSlot fuel = new UIItemSlot(){
				HAlign = 0.1f,
				ValidItemFunc = item => item.IsAir || item.type == ModContent.ItemType<Coal>()
			};
			fuel.Top.Set(220, 0);
			slots.Add(fuel);

			PanelSize(out int width, out int height);
			float x = width * 0.325f;
			float origX = x;
			float y = 160;

			for(int r = 0; r < 2; r++){
				x = origX;
				for(int c = 0; c < 4; c++){
					UIItemSlot result = new UIItemSlot(){
						ValidItemFunc = item => item.IsAir
					};
					result.Left.Set(x, 0);
					result.Top.Set(y, 0);

					x += TextureAssets.InventoryBack9.Value.Width + 8;
					
					slots.Add(result);
				}

				y += TextureAssets.InventoryBack9.Value.Height + 8;
			}
		}

		internal override void InitializeText(List<UIText> text){
			UIText progress = new UIText("Progress: 0%"){
				HAlign = 0.5f
			};
			progress.Top.Set(58, 0);
			text.Add(progress);
		}

		internal override void UpdateText(List<UIText> text){
			text[0].SetText($"Progress: {UIDecimalFormat(UIEntity.ReactionProgress)}%");
		}
	}
}
