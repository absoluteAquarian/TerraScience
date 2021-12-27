using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using TerraScience.API.UI;
using TerraScience.Content.ID;
using TerraScience.Content.Items.Tools;
using TerraScience.Content.TileEntities;
using TerraScience.Content.Tiles.Multitiles;
using TerraScience.Utilities;

namespace TerraScience.Content.UI{
	public class TesseractUI : MachineUI{
		public override string Header => "Tesseract";

		public override int TileType => ModContent.TileType<Tesseract>();

		public UIMachineGauge gaugeLiquid;
		public UIMachineGauge gaugeGas;

		internal override void PanelSize(out int width, out int height){
			width = 250;
			height = 500;
		}

		internal override void InitializeSlots(List<UIItemSlot> slots){
			PanelSize(out int width, out int height);

			int left = width / 2;
			int slotWidth = Main.inventoryBack9Texture.Width;
			int slotHeight = Main.inventoryBack9Texture.Height;

			left -= (int)(slotWidth * 2.5f);
			left -= 12;

			int top = height - slotHeight - 20;

			//Set the item slots
			for(int i = 0; i < 5; i++){
				UIItemSlot slot = new UIItemSlot(){
					ValidItemFunc = null
				};
				slot.Left.Set(left, 0);
				slot.Top.Set(top, 0);

				slots.Add(slot);

				left += slotWidth + 12;
			}

			//Set the liquid exchange slots
			left = width / 2 - slotWidth - 30;

			top = height / 2 - 100;

			UIItemSlot liquidIn = new UIItemSlot(){
				ValidItemFunc = item => {
					var entity = UIEntity as TesseractEntity;
					var liquidSlot = entity.FluidEntries[0].id;

					return item.IsAir || (item.modItem is Capsule capsule && (liquidSlot == MachineFluidID.None || capsule.FluidType == liquidSlot));
				}
			};
			liquidIn.Left.Set(left, 0);
			liquidIn.Top.Set(top, 0);
			slots.Add(liquidIn);
			
			left += 10;
			top += slotHeight + 10;

			UIItemSlot liquidInEmptyCapsules = new UIItemSlot(){
				ValidItemFunc = item => item.IsAir
			};
			liquidInEmptyCapsules.Left.Set(left, 0);
			liquidInEmptyCapsules.Top.Set(top, 0);
			slots.Add(liquidInEmptyCapsules);
			
			left -= 10;
			top += slotHeight + 30;

			UIItemSlot liquidOutEmptyCapsules = new UIItemSlot(){
				ValidItemFunc = item => item.IsAir || (item.modItem is Capsule capsule && capsule.FluidType == MachineFluidID.None)
			};
			liquidOutEmptyCapsules.Left.Set(left, 0);
			liquidOutEmptyCapsules.Top.Set(top, 0);
			slots.Add(liquidOutEmptyCapsules);

			left += 10;
			top += slotHeight + 10;

			UIItemSlot liquidOut = new UIItemSlot(){
				ValidItemFunc = item => item.IsAir
			};
			liquidOut.Left.Set(left, 0);
			liquidOut.Top.Set(top, 0);
			slots.Add(liquidOut);

			//Set the gas exchange slots
			left = width / 2 + 20;

			top = height / 2 - 100;

			UIItemSlot gasIn = new UIItemSlot(){
				ValidItemFunc = item => {
					var entity = UIEntity as TesseractEntity;
					var liquidSlot = entity.FluidEntries[0].id;

					return item.IsAir || (item.modItem is Capsule capsule && (liquidSlot == MachineFluidID.None || capsule.FluidType == liquidSlot));
				}
			};
			gasIn.Left.Set(left, 0);
			gasIn.Top.Set(top, 0);
			slots.Add(gasIn);
			
			left += 10;
			top += slotHeight + 10;

			UIItemSlot gasInEmptyCapsules = new UIItemSlot(){
				ValidItemFunc = item => item.IsAir
			};
			gasInEmptyCapsules.Left.Set(left, 0);
			gasInEmptyCapsules.Top.Set(top, 0);
			slots.Add(gasInEmptyCapsules);
			
			left -= 10;
			top += slotHeight + 30;

			UIItemSlot gasOutEmptyCapsules = new UIItemSlot(){
				ValidItemFunc = item => item.IsAir || (item.modItem is Capsule capsule && capsule.FluidType == MachineFluidID.None)
			};
			gasOutEmptyCapsules.Left.Set(left, 0);
			gasOutEmptyCapsules.Top.Set(top, 0);
			slots.Add(gasOutEmptyCapsules);

			left += 10;
			top += slotHeight + 10;

			UIItemSlot gasOut = new UIItemSlot(){
				ValidItemFunc = item => item.IsAir
			};
			gasOut.Left.Set(left, 0);
			gasOut.Top.Set(top, 0);
			slots.Add(gasOut);
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

			UIText boundNetwork = new UIText("Bound Network: None", 1.3f) {
				HAlign = 0.5f
			};
			boundNetwork.Top.Set(116, 0);
			text.Add(boundNetwork);
		}

		internal override void InitializeOther(UIPanel panel){
			PanelSize(out int width, out int height);

			const int buffer = 16;

			gaugeLiquid = new UIMachineGauge();
			gaugeLiquid.Left.Set(buffer * 2.5f - 12, 0);
			gaugeLiquid.Top.Set(height - 320 - buffer - 4, 0);
			panel.Append(gaugeLiquid);

			gaugeGas = new UIMachineGauge();
			gaugeGas.Left.Set(width - 32 - buffer * 2.5f - 8, 0);
			gaugeGas.Top.Set(height - 320 - buffer - 4, 0);
			panel.Append(gaugeGas);
		}

		internal override void UpdateEntity(){
			TesseractEntity entity = UIEntity as TesseractEntity;

			gaugeLiquid.fluidName = entity.FluidEntries[0].id.ProperEnumName();
			gaugeLiquid.fluidCur = entity.FluidEntries[0].current;
			gaugeLiquid.fluidMax = entity.FluidEntries[0].max;
			gaugeLiquid.fluidColor = entity.FluidEntries[0].current <= 0f ? Color.Transparent : Capsule.GetBackColor(entity.FluidEntries[0].id);

			gaugeGas.fluidName = entity.FluidEntries[2].id.ProperEnumName();
			gaugeGas.fluidCur = entity.FluidEntries[2].current;
			gaugeGas.fluidMax = entity.FluidEntries[2].max;
			gaugeGas.fluidColor = entity.FluidEntries[2].current <= 0f ? Color.Transparent : Capsule.GetBackColor(entity.FluidEntries[2].id);
		}
	}
}
