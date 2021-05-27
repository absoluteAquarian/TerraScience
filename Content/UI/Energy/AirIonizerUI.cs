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
	public class AirIonizerUI : PoweredMachineUI{
		public override string Header => "Matter Energizer";

		public override int TileType => ModContent.TileType<AirIonizer>();

		internal override void PanelSize(out int width, out int height){
			width = 470;
			height = 270;
		}

		internal override void InitializeText(List<UIText> text){
			UIText charge = new UIText("Charge: 0V"){
				HAlign = 0.5f
			};
			charge.Top.Set(48, 0);
			text.Add(charge);

			UIText power = new UIText("Power: 0 / 0TF"){
				HAlign = 0.5f
			};
			power.Top.Set(77, 0);
			text.Add(power);

			UIText progress = new UIText("Progress: 0%"){
				HAlign = 0.5f
			};
			progress.Top.Set(108, 0);
			text.Add(progress);
		}

		internal override void InitializeSlots(List<UIItemSlot> slots){
			UIItemSlot input = new UIItemSlot(){
				ValidItemFunc = item => item.IsAir || AirIonizerEntity.recipes.ContainsKey(item.type),
				HAlign = 0.38f
			};
			input.Top.Set(150, 0);
			slots.Add(input);

			UIItemSlot battery = new UIItemSlot(){
				ValidItemFunc = item => item.IsAir || item.type == ModContent.ItemType<Battery9V>()
			};
			battery.Top.Set(130, 0);
			battery.Left.Set(40, 0);
			slots.Add(battery);

			UIItemSlot output = new UIItemSlot(){
				ValidItemFunc = item => item.IsAir,
				HAlign = 0.62f
			};
			output.Top.Set(150, 0);
			slots.Add(output);
		}

		internal override void UpdateText(List<UIText> text){
			AirIonizerEntity ions = UIEntity as AirIonizerEntity;

			text[0].SetText($"Charge: {UIDecimalFormat(ions.CurBatteryCharge)}V");
			text[1].SetText(GetFluxString());
			text[2].SetText($"Progress: {UIDecimalFormat(UIEntity.ReactionProgress)}%");
		}
	}
}
