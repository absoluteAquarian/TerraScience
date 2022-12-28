using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using TerraScience.Content.ID;
using TerraScience.Content.Items.Placeable.Machines;
using TerraScience.Content.Items.Tools;
using TerraScience.Content.TileEntities;
using TerraScience.Utilities;

namespace TerraScience.Content.Tiles.Multitiles{
	public class FluidTank : Machine{
		public override void GetDefaultParams(out string mapName, out uint width, out uint height, out int itemType){
			mapName = "Fluid Tank";
			width = 3;
			height = 4;
			itemType = ModContent.ItemType<FluidTankItem>();
		}

		// TODO: removing fluids from the tank

		public override bool PreHandleMouse(Point16 pos)
			=> TileUtils.TryPlaceLiquidInMachine<FluidTankEntity>(this, pos);

		public override bool HandleMouse(Point16 pos){
			var liquidItemFluidID = MiscUtils.GetFluidIDFromItem(Main.LocalPlayer.HeldItem.type);
			var capsuleFluidID = (Main.LocalPlayer.HeldItem.ModItem as Capsule)?.FluidType ?? MachineFluidID.None;

			return TileUtils.HandleMouse<FluidTankEntity>(this, pos, () => liquidItemFluidID == MachineFluidID.None && capsuleFluidID == MachineFluidID.None);
		}

		public override bool PreDraw(int i, int j, SpriteBatch spriteBatch){
			//Only draw behind if this is the top-left tile.  Otherwise, the things start drawing on top of other tiles
			if(MiscUtils.TryGetTileEntity(new Point16(i, j), out FluidTankEntity entity)){
				float curLiquidRatio = entity.FluidEntries[0].current / entity.FluidEntries[0].max;
				float curGasRatio = entity.FluidEntries[1].current / entity.FluidEntries[1].max;

				Vector2 offset = MiscUtils.GetLightingDrawOffset();

				Vector2 drawPos = entity.Position.ToVector2() * 16 - Main.screenPosition + offset;
				
				int liquidOffsetX = 6;
				int liquidOffsetY = 6;
				int liquidHeight = 52;

				Rectangle liquidRect = new Rectangle(liquidOffsetX, (int)(liquidOffsetY + liquidHeight * (1f - curLiquidRatio)), 14, (int)(liquidHeight * curLiquidRatio));

				if(curLiquidRatio > 0 && entity.FluidEntries[0].id != MachineFluidID.None)
					spriteBatch.Draw(this.GetEffectTexture("liquid"), drawPos + new Vector2(liquidRect.X, liquidRect.Y), liquidRect, MiscUtils.MixLightColors(Lighting.GetColor(i, j), Capsule.GetBackColor(entity.FluidEntries[0].id)));

				if(curGasRatio > 0 && entity.FluidEntries[1].id != MachineFluidID.None)
					spriteBatch.Draw(this.GetEffectTexture("gas"), drawPos, null, MiscUtils.MixLightColors(Lighting.GetColor(i, j), Capsule.GetBackColor(entity.FluidEntries[1].id)) * (0.65f * curGasRatio));
			}

			return true;
		}
	}
}
