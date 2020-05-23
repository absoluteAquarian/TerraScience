using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using TerraScience.Content.API.UI;
using TerraScience.Content.Items;
using TerraScience.Content.Items.Tools;
using TerraScience.Content.TileEntities;
using TerraScience.Utilities;

namespace TerraScience.Content.UI{
	public class AirIonizerUI : MachineUI{
		public override string Header => "Air Ionizer";

		public override Tile[,] Structure => TileUtils.Structures.AirIonizer;

		internal override void PanelSize(out int width, out int height){
			width = 470;
			height = 250;
		}

		internal override void InitializeText(List<UIText> text){
			UIText charge = new UIText("Charge: 0V"){
				HAlign = 0.5f
			};
			charge.Top.Set(58, 0);
			text.Add(charge);
		}

		internal override void InitializeSlots(List<UIItemSlot> slots){
			//Copied from ScienceWorkbenchUI lul
			int top = 100;
			int origLeft = 30 + 70, left;

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

			UIItemSlot battery = new UIItemSlot(){
				ValidItemFunc = item => item.IsAir || item.type == ModContent.ItemType<Battery9V>(),
				VAlign = 0.5f
			};
			battery.Left.Set(20, 0);
			slots.Add(battery);
		}

		internal override void UpdateText(List<UIText> text){
			AirIonizerEntity ions = UIEntity as AirIonizerEntity;

			text[0].SetText($"Charge: {UIDecimalFormat(ions.CurBatteryCharge)}V");
		}
	}
}
