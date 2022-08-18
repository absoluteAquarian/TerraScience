using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using TerraScience.API.UI;
using TerraScience.Content.ID;
using TerraScience.Content.Items.Tools;
using TerraScience.Content.TileEntities.Energy;
using TerraScience.Content.Tiles.Multitiles.EnergyMachines;
using TerraScience.Utilities;
using UIItemSlot = TerraScience.API.UI.UIItemSlot;

namespace TerraScience.Content.UI.Energy{
	public class ElectrolyzerUI : PoweredMachineUI{
		public override string Header => "Electrolyzer";

		public override int TileType => ModContent.TileType<Electrolyzer>();

		public UIMachineGauge gaugeGas1;
		public UIMachineGauge gaugeGas2;

		internal override void PanelSize(out int width, out int height){
			width = 360;
			height = 340;
		}

		internal override void InitializeText(List<UIText> text){
			UIText waterValues = new UIText("None: 0 / 0 L", 1.3f) {
				HAlign = 0.5f
			};
			waterValues.Top.Set(58, 0);
			text.Add(waterValues);

			UIText charge = new UIText("Charge: 0V", 1.3f) {
				HAlign = 0.5f
			};
			charge.Top.Set(87, 0);
			text.Add(charge);

			UIText power = new UIText("Power: 0 / 0TF"){
				HAlign = 0.5f
			};
			power.Top.Set(116, 0);
			text.Add(power);
		}

		internal override void InitializeSlots(List<UIItemSlot> slots){
			UIItemSlot battery = new UIItemSlot(){
				HAlign = 0.2f,
				ValidItemFunc = item => item.IsAir || item.type == ModContent.ItemType<Battery9V>()
			};
			battery.Top.Set(180, 0);
			slots.Add(battery);

			UIItemSlot gas1Input = new UIItemSlot(){
				HAlign = 0.45f,
				ValidItemFunc = item => item.IsAir || (item.ModItem is Capsule capsule && capsule.FluidType == MachineFluidID.None)
			};
			gas1Input.Top.Set(160, 0);
			slots.Add(gas1Input);

			UIItemSlot gas1Output = new UIItemSlot(){
				HAlign = 0.48f,
				ValidItemFunc = item => item.IsAir
			};
			gas1Output.Top.Set(230, 0);
			slots.Add(gas1Output);

			UIItemSlot gas2Input = new UIItemSlot(){
				HAlign = 0.8f,
				ValidItemFunc = item => item.IsAir || (item.ModItem is Capsule capsule && capsule.FluidType == MachineFluidID.None)
			};
			gas2Input.Top.Set(160, 0);
			slots.Add(gas2Input);

			UIItemSlot gas2Output = new UIItemSlot(){
				HAlign = 0.83f,
				ValidItemFunc = item => item.IsAir
			};
			gas2Output.Top.Set(230, 0);
			slots.Add(gas2Output);
		}

		internal override void InitializeOther(UIPanel panel){
			PanelSize(out int width, out int height);

			const int buffer = 16;

			gaugeGas1 = new UIMachineGauge();
			gaugeGas1.Left.Set(buffer - 12, 0);
			gaugeGas1.Top.Set(height - 210 - buffer - 4, 0);
			panel.Append(gaugeGas1);

			gaugeGas2 = new UIMachineGauge();
			gaugeGas2.Left.Set(width - 32 - buffer - 8, 0);
			gaugeGas2.Top.Set(height - 210 - buffer - 4, 0);
			panel.Append(gaugeGas2);
		}

		internal override void UpdateText(List<UIText> text){
			ElectrolyzerEntity ee = UIEntity as ElectrolyzerEntity;

			text[0].SetText($"{ee.FluidEntries[0].id.ProperEnumName()}: {UIDecimalFormat(ee.FluidEntries[0].current)}L / {Math.Round(ee.FluidEntries[0].max)}L");
			text[1].SetText($"Charge: {UIDecimalFormat(ee.CurBatteryCharge)}V");
			text[2].SetText(GetFluxString());
		}

		internal override void UpdateEntity(){
			ElectrolyzerEntity entity = UIEntity as ElectrolyzerEntity;

			gaugeGas1.fluidName = entity.FluidEntries[1].id.ProperEnumName();
			gaugeGas1.fluidCur = entity.FluidEntries[1].current;
			gaugeGas1.fluidMax = entity.FluidEntries[1].max;
			gaugeGas1.fluidColor = entity.FluidEntries[1].current <= 0f ? Color.Transparent : Capsule.GetBackColor(entity.FluidEntries[1].id);

			gaugeGas2.fluidName = entity.FluidEntries[2].id.ProperEnumName();
			gaugeGas2.fluidCur = entity.FluidEntries[2].current;
			gaugeGas2.fluidMax = entity.FluidEntries[2].max;
			gaugeGas2.fluidColor = entity.FluidEntries[2].current <= 0f ? Color.Transparent : Capsule.GetBackColor(entity.FluidEntries[2].id);
		}
	}
}
