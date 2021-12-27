using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using TerraScience.Content.ID;
using TerraScience.Content.Items.Placeable.Machines.Energy;
using TerraScience.Content.Items.Tools;
using TerraScience.Content.TileEntities.Energy;
using TerraScience.Utilities;

namespace TerraScience.Content.Tiles.Multitiles.EnergyMachines{
	public class LiquidDuplicator : Machine{
		public override void GetDefaultParams(out string mapName, out uint width, out uint height, out int itemType){
			mapName = "Liquid Duplicator";
			width = 3;
			height = 3;
			itemType = ModContent.ItemType<LiquidDuplicatorItem>();
		}

		public override bool HandleMouse(Point16 pos)
			=> TileUtils.HandleMouse<LiquidDuplicatorEntity>(this, pos, () => true);

		public override void PostDraw(int i, int j, SpriteBatch spriteBatch){
			GetDefaultParams(out _, out uint width, out uint height, out _);

			Tile tile = Framing.GetTileSafely(i, j);
			Point16 frame = tile.TileCoord();
			Point16 pos = new Point16(i, j) - frame;
			bool lastTile = frame.X == width - 1 && frame.Y == height - 1;

			if(MiscUtils.TryGetTileEntity(pos, out LiquidDuplicatorEntity entity) && lastTile && entity.ReactionInProgress){
				MachineFluidID id = MiscUtils.GetFluidIDFromItem(entity.RetrieveItem(0).type);

				if(id == MachineFluidID.None)
					return;

				Vector2 offset = MiscUtils.GetLightingDrawOffset();

				Vector2 drawPos = entity.Position.ToVector2() * 16 - Main.screenPosition + offset;

				Color color = Capsule.GetBackColor(id);

				spriteBatch.Draw(this.GetEffectTexture("liquid"), drawPos, null, color);
			}
		}
	}
}
