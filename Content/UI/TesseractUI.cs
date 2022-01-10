using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using Terraria.ModLoader.UI;
using TerraScience.API.UI;
using TerraScience.Content.ID;
using TerraScience.Content.Items.Tools;
using TerraScience.Content.TileEntities;
using TerraScience.Content.Tiles.Multitiles;
using TerraScience.Content.UI.Energy;
using TerraScience.Systems;
using TerraScience.Utilities;

namespace TerraScience.Content.UI{
	public class TesseractUI : PoweredMachineUI{
		public override string Header => "Tesseract";

		public override int TileType => ModContent.TileType<Tesseract>();

		public UIMachineGauge gaugeFluid;

		internal override void PanelSize(out int width, out int height){
			width = 450;
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

			bool IsBoundToANetwork(){
				string net = (UIEntity as TesseractEntity).BoundNetwork;
				return net != null && TesseractNetwork.TryGetEntry(net, out _);
			}

			//Set the item slots
			for(int i = 0; i < 5; i++){
				UITesseractItemSlot slot = new UITesseractItemSlot(i, this){
					ValidItemFunc = item => IsBoundToANetwork()
				};
				slot.Left.Set(left, 0);
				slot.Top.Set(top, 0);

				slots.Add(slot);

				left += slotWidth + 12;
			}

			//Set the fluid exchange slots
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
			
			left = width / 2 + 20;

			top = height / 2 - 100;

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
		}

		internal override void InitializeText(List<UIText> text){
			UIText powerAmount = new UIText("Power: 0/0 TF"){
				HAlign = 0.5f
			};
			powerAmount.Top.Set(58, 0);
			text.Add(powerAmount);

			UIText boundNetwork = new UIText("Bound Network: None", 1.3f) {
				HAlign = 0.5f
			};
			boundNetwork.Top.Set(87, 0);
			text.Add(boundNetwork);
		}

		internal override void InitializeOther(UIPanel panel){
			PanelSize(out int width, out int height);

			const int buffer = 16;

			gaugeFluid = new UIMachineGauge();
			gaugeFluid.Left.Set(buffer * 2.5f - 12, 0);
			gaugeFluid.Top.Set(height - 320 - buffer - 4, 0);
			panel.Append(gaugeFluid);

			UIImageButton config = new UIImageButton(UICommon.ButtonModConfigTexture);
			config.Left.Set(-config.Width.Pixels - 15, 1f);
			config.Top.Set(15, 0f);
			config.OnClick += (evt, e) => TechMod.Instance.machineLoader.tesseractNetworkState.Open(this);
			panel.Append(config);
		}

		internal override void UpdateEntity(){
			TesseractEntity entity = UIEntity as TesseractEntity;

			gaugeFluid.fluidName = entity.FluidEntries[0].id.ProperEnumName();
			gaugeFluid.fluidCur = entity.FluidEntries[0].current;
			gaugeFluid.fluidMax = entity.FluidEntries[0].max;
			gaugeFluid.fluidColor = entity.FluidEntries[0].current <= 0f ? Color.Transparent : Capsule.GetBackColor(entity.FluidEntries[0].id);
		}

		internal override void UpdateText(List<UIText> text){
			text[0].SetText(GetFluxString());
			var net = (UIEntity as TesseractEntity).BoundNetwork;
			text[1].SetText(string.IsNullOrEmpty(net) ? "Not Bound to a Network" : "Bound Network: " + net);
		}

		public override void PreOpen(){
			//Force DoSavedItemsCheck() to not run
			base.CheckedForSavedItemCount = true;
		}

		public override void PostOpen(){
			var entity = UIEntity as TesseractEntity;
			entity.OnNetworkChange += UpdateItemSlots;

			bool release = TechMod.Release;
			if(!release && TesseractNetwork.TryGetEntry(entity.BoundNetwork, out var entry)){
				Main.NewText($"(UI Opening) Reading Items for Network \"{entry.name}\"...");
				for(int i = 0; i < entry.items.Length; i++){
					Item item = entry.items[i];
					Main.NewText($"  Item #{i}: {item.Name ?? "None"} {(item.stack > 1 ? $"({item.stack})" : "")}");
				}
			}
		}

		public override void PreClose(){
			var entity = UIEntity as TesseractEntity;
			entity.OnNetworkChange -= UpdateItemSlots;

			bool release = TechMod.Release;
			if(!release && TesseractNetwork.TryGetEntry(entity.BoundNetwork, out var entry)){
				Main.NewText($"(UI Closing) Reading Items for Network \"{entry.name}\"...");
				for(int i = 0; i < entry.items.Length; i++){
					Item item = entry.items[i];
					Main.NewText($"  Item #{i}: {item.Name ?? "None"} {(item.stack > 1 ? $"({item.stack})" : "")}");
				}
			}
		}

		private void UpdateItemSlots(){
			for(int i = 0; i < 5; i++){
				var slot = GetSlot(i);
				slot.OnItemChanged?.Invoke(slot.StoredItem);
			}
		}
	}
}
