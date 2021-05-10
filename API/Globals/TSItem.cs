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
	}
}
