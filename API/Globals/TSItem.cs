using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;
using TerraScience.Content.Items.Materials;

namespace TerraScience.API.Globals{
	public class TSItem : GlobalItem{
		public override void SetDefaults(Item item){
			if(item.type == ItemID.SandBlock || item.type == ItemID.EbonsandBlock || item.type == ItemID.CrimsandBlock || item.type == ItemID.PearlsandBlock)
				ItemID.Sets.ExtractinatorMode[item.type] = item.type;
		}

		public override void ExtractinatorUse(int extractType, ref int resultType, ref int resultStack){
			WeightedRandom<(int, int, int)> wRand = new WeightedRandom<(int, int, int)>(Main.rand);

			if(extractType == ItemID.SandBlock || extractType == ItemID.EbonsandBlock || extractType == ItemID.CrimsandBlock || extractType == ItemID.PearlsandBlock){
				/*  Spawn pool:
				 *  
				 *  1-3 Silicon (5%)
				 */
				wRand.Add((ModContent.ItemType<Silicon>(), 1, 3), 0.05);

				if(extractType == ItemID.EbonsandBlock){
					wRand.Add((ItemID.RottenChunk, 1, 2), 0.03);
					wRand.Add((ItemID.VilePowder, 1, 1), 0.02);
					if(Main.hardMode){
						wRand.Add((ItemID.CursedFlame, 1, 4), 0.015);
						wRand.Add((ItemID.SoulofNight, 1, 3), 0.005);
					}
				}else if(extractType == ItemID.CrimsandBlock){
					wRand.Add((ItemID.Vertebrae, 1, 2), 0.03);
					wRand.Add((ItemID.ViciousPowder, 1, 1), 0.02);
					if(Main.hardMode){
						wRand.Add((ItemID.Ichor, 1, 4), 0.015);
						wRand.Add((ItemID.SoulofNight, 1, 3), 0.005);
					}
				}else if(extractType == ItemID.PearlsandBlock){
					wRand.Add((ItemID.CrystalShard, 1, 8), 0.03);
					wRand.Add((ItemID.PixieDust, 1, 5), 0.06);
					wRand.Add((ItemID.UnicornHorn, 1, 1), 0.008);
					wRand.Add((ItemID.SoulofLight, 1, 3), 0.005);
				}
			}else{
				//Don't do logic for types this class doesn't support
				return;
			}

			//Calculate the remaining total
			double usedTotal = 0;
			foreach(var t in wRand.elements)
				usedTotal += t.Item2;

			wRand.Add((-1, 0, 0), 1.0 - usedTotal);

			(int type, int stackMin, int stackMax) = wRand.Get();

			if(type == -1){
				resultType = ItemID.None;
				resultStack = 0;
			}else{
				resultType = type;
				resultStack = Main.rand.Next(stackMin, stackMax + 1);
			}
		}

		public override void ModifyTooltips(Item item, List<TooltipLine> tooltips){
			//Add to the tooltip description if it's a Golden Key
			if(item.type == ItemID.GoldenKey){
				int index = tooltips.FindLastIndex(line => line.Mod == "Terraria" && line.Name.StartsWith("Tooltip"));

				tooltips.Insert(++index, new TooltipLine(TechMod.Instance, "ItemCacheUse", "Right click an Item Cache machine while holding this to lock/unlock it"));
			}
		}
	}
}
