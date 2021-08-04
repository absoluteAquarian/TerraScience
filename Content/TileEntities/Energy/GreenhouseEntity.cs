using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;
using TerraScience.Content.Items.Materials;
using TerraScience.Content.Tiles.Multitiles.EnergyMachines;
using TerraScience.Systems.Energy;
using TerraScience.Utilities;

namespace TerraScience.Content.TileEntities.Energy{
	public class GreenhouseEntity : PoweredMachineEntity{
		public override int MachineTile => ModContent.TileType<Greenhouse>();

		//acorn / herb seed, block type, block modifier and 3 output slots
		public override int SlotsCount => 6;

		public override TerraFlux FluxCap => new TerraFlux(8000);

		public override TerraFlux FluxUsage => new TerraFlux(150f / 60f);

		public int saplingRand = -1;

		public override void PreUpdateReaction(){
			ReactionSpeed = !CheckFluxRequirement(FluxUsage, use: false) ? 0.05f : 1f;

			//Check that all slots aren't full.  If they are, abort early
			bool allFull = true;
			for(int i = 3; i < SlotsCount; i++)
				if(this.RetrieveItem(i).IsAir)
					allFull = false;

			//Slot 0 = acorn or seed
			//Slot 1 = block
			var input = this.RetrieveItem(0);
			bool hasInput = !input.IsAir;
			bool hasBlock = !this.RetrieveItem(1).IsAir;
			ReactionInProgress = hasInput && hasBlock && !allFull;

			if(!hasInput)
				ReactionProgress = 0;

			if(saplingRand == -1)
				saplingRand = Main.rand.Next(input.type == ItemID.PumpkinSeed ? 6 : 3);
		}

		public override bool UpdateReaction(){
			CheckFluxRequirement(FluxUsage, use: true);

			//2min to grow saplings, 1min 15s to grow herbs, 3min to grow pumpkins
			var input = this.RetrieveItem(0);
			float time = input.type == ItemID.Acorn ? 2 : (input.type == ItemID.PumpkinSeed ? 3 : 1.25f);
			time *= 3600;
			ReactionProgress += 100f / time * ReactionSpeed;

			return ReactionProgress >= 100;
		}

		public override void ReactionComplete(){
			ReactionProgress = 0;

			Item input = this.RetrieveItem(0);
			Item block = this.RetrieveItem(1);
			Item modifier = this.RetrieveItem(2);
			
			saplingRand = Main.rand.Next(input.type == ItemID.PumpkinSeed ? 6 : 3);

			int resultType = -1, resultStack;
			switch(input.type){
				case ItemID.Acorn:
					if(block.type == ItemID.SandBlock)
						resultType = ItemID.PalmWood;
					else if(block.type == ItemID.SnowBlock)
						resultType = ItemID.BorealWood;
					else if(block.type == ItemID.DirtBlock){
						switch(modifier.type){
							case ItemID.GrassSeeds:
								resultType = ItemID.Wood;
								break;
							case ItemID.CorruptSeeds:
								resultType = ItemID.Ebonwood;
								break;
							case ItemID.CrimsonSeeds:
								resultType = ItemID.Shadewood;
								break;
							case ItemID.HallowedSeeds:
								resultType = ItemID.Pearlwood;
								break;
							default:
								if(modifier.type == ModContent.ItemType<Vial_Saltwater>())
									resultType = ItemID.PalmWood;
								break;
						}
					}else if(block.type == ItemID.MudBlock){
						switch(modifier.type){
							case ItemID.JungleGrassSeeds:
								resultType = ItemID.RichMahogany;
								break;
						}
					}
					break;
				case ItemID.DaybloomSeeds:
					resultType = ItemID.Daybloom;
					break;
				case ItemID.BlinkrootSeeds:
					resultType = ItemID.Blinkroot;
					break;
				case ItemID.WaterleafSeeds:
					resultType = ItemID.Waterleaf;
					break;
				case ItemID.ShiverthornSeeds:
					resultType = ItemID.Shiverthorn;
					break;
				case ItemID.DeathweedSeeds:
					resultType = ItemID.Deathweed;
					break;
				case ItemID.MoonglowSeeds:
					resultType = ItemID.Moonglow;
					break;
				case ItemID.FireblossomSeeds:
					resultType = ItemID.Fireblossom;
					break;
				case ItemID.MushroomGrassSeeds:
					resultType = ItemID.GlowingMushroom;
					break;
				case ItemID.Cactus:
					resultType = ItemID.Cactus;
					break;
				case ItemID.PumpkinSeed:
					resultType = ItemID.Pumpkin;
					break;
			}

			if(input.type == ItemID.Acorn){
				resultStack = Main.rand.Next(10, 21);

				if(Main.rand.NextFloat() < 0.03f)
					resultStack = Main.rand.Next(21, 36);
			}else if(input.type == ItemID.Cactus)
				resultStack = Main.rand.Next(2, 21);
			else if(input.type == ItemID.PumpkinSeed)
				resultStack = Main.rand.Next(5, 16);
			else{
				WeightedRandom<int> wRand = new WeightedRandom<int>(Main.rand);
				wRand.Add(1, 95d);
				wRand.Add(2, 4.75d);
				wRand.Add(3, 0.25d);
				resultStack = wRand.Get();
			}

			if(resultType < 0 || resultStack < 0)
				return;

			InsertItems(resultType, resultStack);

			//Random chance to get extra seeds or acorns
			if(Main.rand.NextFloat() < 0.06f)
				InsertItems(input.type, 1);

			PlaySound(SoundID.Grab, TileUtils.TileEntityCenter(this, MachineTile));
		}

		private void InsertItems(int resultType, int resultStack){
			//Find the first slot that the items can stack to.  If that stack isn't enough, overflow to the next slot
			for(int i = 3; i < SlotsCount; i++){
				Item item = this.RetrieveItem(i);
				if(item.IsAir){
					item.SetDefaults(resultType);
					item.type = resultType;
					item.stack = resultStack;
					break;
				}

				if(item.type == resultType && item.stack < item.maxStack){
					if(item.stack + resultStack <= item.maxStack){
						item.stack += resultStack;
						break;
					}else{
						resultStack -= item.maxStack - item.stack;
						item.stack = item.maxStack;
					}
				}
			}
		}

		internal override int[] GetInputSlots() => System.Array.Empty<int>();

		internal override int[] GetOutputSlots() => new int[]{ 3, 4, 5 };

		internal override bool CanInputItem(int slot, Item item) => false;
	}
}
