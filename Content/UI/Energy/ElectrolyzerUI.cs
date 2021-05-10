using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using TerraScience.Content.API.UI;
using TerraScience.Content.Items.Tools;
using TerraScience.Content.TileEntities.Energy;
using TerraScience.Content.Tiles.Multitiles.EnergyMachines;
using TerraScience.Utilities;

namespace TerraScience.Content.UI.Energy{
	public class ElectrolyzerUI : PoweredMachineUI{
		public override string Header => "Electrolyzer";

		public override int TileType => ModContent.TileType<Electrolyzer>();

		internal override void PanelSize(out int width, out int height){
			width = 300;
			height = 300;
		}

		internal override void InitializeText(List<UIText> text){
			UIText waterValues = new UIText("0L / 0L", 1.3f) {
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
				HAlign = 0.08f,
				ValidItemFunc = item => item.IsAir || item.type == ModContent.ItemType<Battery9V>()
			};
			battery.Top.Set(190, 0);
			slots.Add(battery);

			UIItemSlot itemSlot_Hydros = new UIItemSlot {
				HAlign = 0.4f,
				ValidItemFunc = item => item.IsAir
			};
			itemSlot_Hydros.Top.Set(222, 0);
			slots.Add(itemSlot_Hydros);

			UIItemSlot itemSlot_Oxys = new UIItemSlot{
				HAlign = 0.8f,
				ValidItemFunc = item => item.IsAir
			};
			itemSlot_Oxys.Top.Set(222, 0);
			slots.Add(itemSlot_Oxys);
		}

		internal override void UpdateText(List<UIText> text){
			ElectrolyzerEntity ee = UIEntity as ElectrolyzerEntity;

			text[0].SetText($"{UIDecimalFormat(ee.StoredLiquid)}L / {Math.Round(ElectrolyzerEntity.MaxLiquid)}L");
			text[1].SetText($"Charge: {UIDecimalFormat(ee.CurBatteryCharge)}V");
			text[2].SetText(GetFluxString());
		}
	}
}
