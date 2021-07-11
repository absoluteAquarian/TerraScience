using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using TerraScience.Content.Items.Placeable.Machines.Energy;
using TerraScience.Content.TileEntities.Energy;
using TerraScience.Utilities;

namespace TerraScience.Content.Tiles.Multitiles.EnergyMachines{
	public class Composter : Machine{
		public override void GetDefaultParams(out string mapName, out uint width, out uint height, out int itemType){
			mapName = "Composter";
			width = 3;
			height = 3;
			itemType = ModContent.ItemType<ComposterItem>();
		}

		public override bool HandleMouse(Point16 pos)
			=> TileUtils.HandleMouse<ComposterEntity>(this, pos, () => true);

		public override bool PreDraw(int i, int j, SpriteBatch spriteBatch){
			Point16 pos = new Point16(i, j);

			if(MiscUtils.TryGetTileEntity(pos, out ComposterEntity entity)){
				//Draw one item being crushed and another item behind it
				//Draw order:  back item -> piston -> front item
				Vector2 offset = MiscUtils.GetLightingDrawOffset();
				Vector2 drawPos = entity.Position.ToVector2() * 16 - Main.screenPosition + offset;

				var input = entity.RetrieveItem(0);

				//Draw the "queued" item
				Vector2 frontItemOffset = new Vector2(13, 23);
				Vector2 size = new Vector2(16, 16);
				if(!input.IsAir && input.stack > 1)
					spriteBatch.DrawItemInWorld(input, drawPos + frontItemOffset, size);

				//Draw the piston
				//Piston quickly slams down, then slowly moves back up
				const float MoveDownProgress = 15f;
				Vector2 pistonOffset = entity.ReactionProgress < MoveDownProgress
					? new Vector2(0, -30 + 30 * entity.ReactionProgress / MoveDownProgress)
					: new Vector2(0, -30 * (entity.ReactionProgress - MoveDownProgress) / (100f - MoveDownProgress));

				spriteBatch.Draw(this.GetEffectTexture("piston"), drawPos + pistonOffset, null, Lighting.GetColor(i, j), 0f, Vector2.Zero, 1f, SpriteEffects.None, 0);

				//Draw the item being crushed
				if(!input.IsAir){
					if(entity.ReactionProgress < MoveDownProgress){
						//Move the item down with the piston, but don't move it too far down
						Vector2 itemOffset = pistonOffset + new Vector2(0, 30);
						if(frontItemOffset.Y + size.Y / 2f + itemOffset.Y >= 47f)
							frontItemOffset.Y = 47f - size.Y / 2f;
						else
							frontItemOffset.Y += itemOffset.Y;
					}else
						frontItemOffset.Y = 47f - size.Y / 2f;

					spriteBatch.DrawItemInWorld(input, drawPos + frontItemOffset, size);
				}
			}

			return true;
		}
	}
}
