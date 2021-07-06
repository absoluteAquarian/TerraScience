using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using TerraScience.API.UI;
using TerraScience.Content.Items.Tools;
using TerraScience.Content.TileEntities;
using TerraScience.Content.Tiles.Multitiles;
using TerraScience.Utilities;

namespace TerraScience.Content.UI{
	public class FluidTankUI : MachineUI{
		public override string Header => "Fluid Tank";

		public override int TileType => ModContent.TileType<FluidTank>();

		public UIMachineGauge gaugeLiquid;
		public UIMachineGauge gaugeGas;

		internal override void PanelSize(out int width, out int height){
			width = 250;
			height = 300;
		}

		internal override void InitializeSlots(List<UIItemSlot> slots){ }

		internal override void InitializeText(List<UIText> text){ }

		internal override void InitializeOther(UIPanel panel){
			PanelSize(out int width, out int height);

			const int buffer = 16;

			gaugeLiquid = new UIMachineGauge();
			gaugeLiquid.Left.Set(buffer * 2.5f - 12, 0);
			gaugeLiquid.Top.Set(height - 220 - buffer - 4, 0);
			panel.Append(gaugeLiquid);

			gaugeGas = new UIMachineGauge();
			gaugeGas.Left.Set(width - 32 - buffer * 2.5f - 8, 0);
			gaugeGas.Top.Set(height - 220 - buffer - 4, 0);
			panel.Append(gaugeGas);
		}

		internal override void UpdateEntity(){
			FluidTankEntity entity = UIEntity as FluidTankEntity;

			gaugeLiquid.fluidName = entity.LiquidEntries[0].id.ProperEnumName();
			gaugeLiquid.fluidCur = entity.LiquidEntries[0].current;
			gaugeLiquid.fluidMax = entity.LiquidEntries[0].max;
			gaugeLiquid.fluidColor = entity.LiquidEntries[0].current <= 0f ? Color.Transparent : Capsule.GetBackColor(entity.LiquidEntries[0].id);

			gaugeGas.fluidName = entity.GasEntries[0].id.ProperEnumName();
			gaugeGas.fluidCur = entity.GasEntries[0].current;
			gaugeGas.fluidMax = entity.GasEntries[0].max;
			gaugeGas.fluidColor = entity.GasEntries[0].current <= 0f ? Color.Transparent : Capsule.GetBackColor(entity.GasEntries[0].id);
		}
	}
}
