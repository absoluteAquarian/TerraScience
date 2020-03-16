using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Terraria;
using Terraria.GameContent.Liquid;

namespace TerraScience.API.Classes.ModLiquid {
	public static class ModLiquidManager {
        internal static int LastAddedLiquidID = 3;

		internal static void RunInLiquidEvent(Rectangle liquidRect, ModLiquid.InLiquidEventHandler inLiquidEvent) {
			for (int i = 0; i < Main.maxPlayers; i++) {
				Player player = Main.player[i];

				if (player.getRect().Intersects(liquidRect)) {
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
				c.Emit(OpCodes.Ldsfld, urtype.GetField("Instance")); // load ur field
				c.Emit(OpCodes.Ldloc, 5); // load local field
				c.Emit(OpCodes.Callvirt, urtype.GetMethod("PrepareDraw")); // ur method call
			}
		}
	}
}