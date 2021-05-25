using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using Terraria;
using Terraria.ModLoader;
using TerraScience.Content.Items.Placeable;

namespace TerraScience.API.Edits.MSIL{
	public static partial class Vanilla{
#pragma warning disable IDE0051
		private static void Main_DrawPlayer(ILContext il){
			ILCursor c = new ILCursor(il);

			ILHelper.CompleteLog(c, beforeEdit: true);

			/*   Edit:  Adding a specific conditional for item frames on the held item drawing
			 *   
			 *   Main.itemTexture[type18],
			 *   new Vector2((float)((int)(vector.X - Main.screenPosition.X)), (float)((int)(vector.Y - Main.screenPosition.Y))),
			 *   new Rectangle?(new Rectangle(0, 0, Main.itemTexture[type18].Width, Main.itemTexture[type18].Height)),
			 *      ^ Edit goes where this parameter is
			 *   
			 *   IL_C098: sub
			 *   IL_C099: conv.i4
			 *   IL_C09A: conv.r4
			 *   IL_C09B: newobj    instance void [Microsoft.Xna.Framework]Microsoft.Xna.Framework.Vector2::.ctor(float32, float32)
			 *     === EDIT GOES HERE ===
			 *   IL_C0A0: ldc.i4.0
			 *   IL_C0A1: ldc.i4.0
			 *   IL_C0A2: ldsfld    class [Microsoft.Xna.Framework.Graphics]Microsoft.Xna.Framework.Graphics.Texture2D[] Terraria.Main::itemTexture
			 *   IL_C0A7: ldloc     V_299
			 *   IL_C0AB: nop
			 *   IL_C0AC: nop
			 *   IL_C0AD: ldelem.ref
			 *   IL_C0AE: callvirt  instance int32 [Microsoft.Xna.Framework.Graphics]Microsoft.Xna.Framework.Graphics.Texture2D::get_Width()
			 *   IL_C0B3: ldsfld    class [Microsoft.Xna.Framework.Graphics]Microsoft.Xna.Framework.Graphics.Texture2D[] Terraria.Main::itemTexture
			 *   IL_C0B8: ldloc     V_299
			 *   IL_C0BC: nop
			 *   IL_C0BD: nop
			 *   IL_C0BE: ldelem.ref
			 *   IL_C0BF: callvirt  instance int32 [Microsoft.Xna.Framework.Graphics]Microsoft.Xna.Framework.Graphics.Texture2D::get_Height()
			 *   IL_C0C4: newobj    instance void [Microsoft.Xna.Framework]Microsoft.Xna.Framework.Rectangle::.ctor(int32, int32, int32, int32)
			 *   IL_C0C9: newobj    instance void valuetype [mscorlib]System.Nullable`1<valuetype [Microsoft.Xna.Framework]Microsoft.Xna.Framework.Rectangle>::.ctor(!0)
			 *   IL_C0CE: ldarg.1
			 *     === BRANCH TO ABOVE STATEMENT IS ADDED ===
			 */

			if(!c.TryGotoNext(MoveType.Before, i => i.MatchNewobj(ILHelper.Vector2_ctor_float_float),
				i => i.MatchLdcI4(0),
				i => i.MatchLdcI4(0),
				i => i.MatchLdsfld(ILHelper.Main_itemTexture),
				i => i.MatchLdloc(299),
				i => i.MatchNop(),
				i => i.MatchNop(),
				i => i.MatchLdelemRef(),
				i => i.MatchCallvirt(ILHelper.Texture2D_get_Width),
				i => i.MatchLdsfld(ILHelper.Main_itemTexture),
				i => i.MatchLdloc(299),
				i => i.MatchNop(),
				i => i.MatchNop(),
				i => i.MatchLdelemRef(),
				i => i.MatchCallvirt(ILHelper.Texture2D_get_Height),
				i => i.MatchNewobj(ILHelper.Rectangle_ctor_int_int_int_int),
				i => i.MatchNewobj(ILHelper.Nullable_Rectangle_ctor_Rectangle)))
				goto bad_il;

			c.Index++;
			//Put a branch target here for the "false" case
			ILLabel origCode = c.MarkLabel();

			//Go to after the nullable ctor, add a branch target there, then go back
			ILLabel atNullableRectangle_ctor = c.DefineLabel();
			c.Index += 15;
			c.MarkLabel(atNullableRectangle_ctor);
			c.Index -= 15;
			//Emit the things
			c.Emit(OpCodes.Ldloc, 299);
			c.EmitDelegate<Func<int, bool>>(itemType => itemType == ModContent.ItemType<TransportJunctionItem>());
			c.Emit(OpCodes.Brfalse, origCode);
			c.Emit(OpCodes.Ldarg_1);
			c.EmitDelegate<Func<Player, Rectangle>>(player => new Rectangle(player.HeldItem.placeStyle * 18, 0, 18, 18));
			c.Emit(OpCodes.Br, atNullableRectangle_ctor);

			ILHelper.UpdateInstructionOffsets(c);

			ILHelper.CompleteLog(c, beforeEdit: false);

			return;
bad_il:
			TechMod.Instance.Logger.Error("Could not fully edit method \"Terraria.Main.DrawPlayer(Player, Vector2, float, Vector2, [float])\"");
		}
	}
}
