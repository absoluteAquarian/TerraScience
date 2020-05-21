using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.UI.Elements;
using TerraScience.Content.API.UI;
using TerraScience.Content.Items;
using TerraScience.Utilities;

namespace TerraScience.Content.UI{
	public class AirIonizerUI : MachineUI{
		public override string Header => "Air Ionizer";

		public override Tile[,] Structure => TileUtils.Structures.AirIonizer;

		internal override void PanelSize(out int width, out int height){
			width = 400;
			height = 250;
		}

		internal override void InitializeText(List<UIText> text){
			//No text; do nothing
		}

		internal override void InitializeSlots(List<UIItemSlot> slots){
			//Copied from ScienceWorkbenchUI lul
			int top = 100;
			int origLeft = 30, left;

			//10 slots, 5 per row
			for(int r = 0; r < 2; r++){
				left = origLeft;
				for(int c = 0; c < 5; c++){
					//Can't place items, only remove them
					UIItemSlot slot = new UIItemSlot(){
						ValidItemFunc = item => item.IsAir
					};
					slot.Left.Set(left, 0);
					slot.Top.Set(top, 0);

					left += Main.inventoryBack9Texture.Width + 15;

					slots.Add(slot);
				}
				top += Main.inventoryBack9Texture.Height + 15;
			}
		}
	}
}
