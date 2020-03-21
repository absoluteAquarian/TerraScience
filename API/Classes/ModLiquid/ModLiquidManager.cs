using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Terraria;
using Terraria.GameContent.Liquid;

namespace TerraScience.API.Classes.ModLiquid {
	internal static class ModLiquidManager {
		internal static int LastAddedLiquidID = 4;

		internal static void RunInLiquidEvent(ModLiquid.InLiquidEventHandler inLiquidEvent) {
			for (int i = 0; i < Main.maxPlayers; i++) {
				Player player = Main.player[i];

				if (Collision.WetCollision(player.position, player.width, player.height)) {
					ModLiquid.InLiquidEventHandler handler = inLiquidEvent;
					handler?.Invoke(player);
				}
			}
		}

		[SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")]
		internal static void SwapDrawWater(On.Terraria.Main.orig_DrawWater orig, Main self, bool bg, int Style, float Alpha) {
			if (!Lighting.NotRetro) {
				self.oldDrawWater(bg, Style, Alpha);
				return;
			}

			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();
			Vector2 drawOffset = (Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange, Main.offScreenRange)) - Main.screenPosition;
			LiquidRenderer.Instance.Draw(Main.spriteBatch, drawOffset, Style, Alpha, bg);
			ModLiquidRenderer.Instance.Draw(Main.spriteBatch, drawOffset, Style, Alpha, bg);

			if (!bg)
				TimeLogger.DrawTime(4, stopwatch.Elapsed.TotalMilliseconds);
		}

		internal static void EditDrawWaters(ILContext il) {
			ILCursor c = new ILCursor(il);
			c.Goto(0);

			Type type = typeof(LiquidRenderer);
			Type urtype = typeof(ModLiquidRenderer);

			if (c.TryGotoNext(i => i.MatchCallvirt(type.GetMethod(nameof(LiquidRenderer.PrepareDraw))))) {
				c.Index++; // move next
				c.Emit(OpCodes.Ldsfld, urtype.GetField(nameof(LiquidRenderer.Instance))); // load ur field
				c.Emit(OpCodes.Ldloc, 5); // load local field
				c.Emit(OpCodes.Callvirt, urtype.GetMethod(nameof(LiquidRenderer.PrepareDraw))); // ur method call
			}
		}

		/// <summary>
		/// 0 is water, 1 is lava, 2 is honey
		/// </summary>
		/// <param name="tile"></param>
		/// <returns></returns>
		internal static int NewLiquidType(this Tile tile) => tile.NewLiquidType(out _);

		/// <summary>
		/// 0 is water, 1 is lava, 2 is honey
		/// </summary>
		/// <param name="tile"></param>
		/// <returns></returns>
		internal static int NewLiquidType(this Tile tile, out string internalName) {
			internalName = string.Empty;

			if (tile.liquidType() == 0) {
				internalName = "Water";
				return 0;
			}
			else if (tile.liquidType() == 1) {
				internalName = "Lava";
				return 1;
			}
			else if (tile.liquidType() == 2) {
				internalName = "Honey";
				return 2;
			}

			return 0; //The logic for getting a tile type
		}

		internal static int NewLiquidType(this Tile tile, int tileType) {
			//Set tiles liquid type and render it based on vanilla code.

			return tileType;
		}
	}
}