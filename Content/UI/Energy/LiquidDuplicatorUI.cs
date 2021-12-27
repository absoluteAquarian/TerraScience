using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using TerraScience.API.UI;
using TerraScience.Content.ID;
using TerraScience.Content.Items.Tools;
using TerraScience.Content.TileEntities.Energy;
using TerraScience.Content.Tiles.Multitiles.EnergyMachines;
using TerraScience.Utilities;

namespace TerraScience.Content.UI.Energy{
	public class LiquidDuplicatorUI : PoweredMachineUI{
		public override string Header => "Liquid Duplicator";

		public override int TileType => ModContent.TileType<LiquidDuplicator>();

		public UIMachineGauge gauge;

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

			UIText progress = new UIText("Progress: 0%"){
				HAlign = 0.5f
			};
			progress.Top.Set(87, 0);
			text.Add(progress);

			UIText power = new UIText("Power: 0 / 0TF"){
				HAlign = 0.5f
			};
			power.Top.Set(116, 0);
			text.Add(power);
		}

		internal override void InitializeSlots(List<UIItemSlot> slots){
			UIItemSlot input = new UIItemSlot(){
				HAlign = 0.2f,
				ValidItemFunc = item => item.IsAir || MiscUtils.GetFluidIDFromItem(item.type) != MachineFluidID.None
			};
			input.Top.Set(180, 0);
			slots.Add(input);
		}

		internal override void InitializeOther(UIPanel panel){
			PanelSize(out int width, out int height);

			const int buffer = 16;

			gauge = new UIMachineGauge();
			gauge.Left.Set(width - 32 - buffer * 2.5f - 8, 0);
			gauge.Top.Set(height - 250 - buffer - 4, 0);
			panel.Append(gauge);
		}

		internal override void UpdateText(List<UIText> text){
			LiquidDuplicatorEntity ee = UIEntity as LiquidDuplicatorEntity;

			text[0].SetText($"Duplicating: {ee.FluidEntries[0].id.ProperEnumName()}");
			text[1].SetText($"Progress: {UIDecimalFormat(UIEntity.ReactionProgress)}%");
			text[2].SetText(GetFluxString());
		}

		internal override void UpdateEntity(){
			LiquidDuplicatorEntity entity = UIEntity as LiquidDuplicatorEntity;

			gauge.fluidName = entity.FluidEntries[0].id.ProperEnumName();
			gauge.fluidCur = entity.FluidEntries[0].current;
			gauge.fluidMax = entity.FluidEntries[0].max;
			gauge.fluidColor = entity.FluidEntries[0].current <= 0f ? Color.Transparent : Capsule.GetBackColor(entity.FluidEntries[0].id);
		}
	}
}
