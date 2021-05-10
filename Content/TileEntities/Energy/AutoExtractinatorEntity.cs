using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using TerraScience.Content.Tiles.Multitiles.EnergyMachines;
using TerraScience.Systems.Energy;
using TerraScience.Utilities;

namespace TerraScience.Content.TileEntities.Energy{
	public class AutoExtractinatorEntity : PoweredMachineEntity{
		public override int MachineTile => ModContent.TileType<AutoExtractinator>();

		public override int SlotsCount => 21;

		public override TerraFlux FluxCap => new TerraFlux(2000f);

		public override TerraFlux FluxUsage => new TerraFlux(200f / 60f);

		public float shifterTopSin, shifterBottomSin;

		public int frameRand = -1;
		public int frameRand2 = -1;

		public long storedCoins;

		public override TagCompound ExtraSave(){
			var tag = base.ExtraSave();
			tag.Add("coins", storedCoins);
			return tag;
		}

		public override void ExtraLoad(TagCompound tag){
			base.ExtraLoad(tag);
			storedCoins = tag.GetLong("coins");
		}

		public override void PreUpdateReaction(){
			bool hasFlux = CheckFluxRequirement(FluxUsage, use: false);
			ReactionInProgress = !this.RetrieveItem(0).IsAir && hasFlux;

			//Check that all slots aren't full.  If they are, abort early
			bool allFull = true;
			for(int i = 1; i < SlotsCount; i++)
				if(this.RetrieveItem(i).IsAir)
					allFull = false;

			if(allFull)
				ReactionInProgress = false;

			if(frameRand == -1 || frameRand2 == -1){
				frameRand = Main.rand.Next(0, 3);
				frameRand2 = Main.rand.Next(0, 3);
			}
		}

		public override bool UpdateReaction(){
			CheckFluxRequirement(FluxUsage, use: true);

			//Machine speeds up as it works
			ReactionSpeed *= 1f + 0.0132f / 60f;

			const float max = 5;
			if(ReactionSpeed > max)
				ReactionSpeed = max;

			//5s to extract one item
			ReactionProgress += 100f / 5f / 60f * ReactionSpeed;

			//One rotation every 1.5 seconds
			float factor = ReactionSpeed / (max / 2);
			shifterTopSin += MathHelper.ToRadians(6f * 1f / 1.5f) * factor;
			//1.5 rotations per second
			shifterBottomSin -= MathHelper.ToRadians(6f * 1.5f) * factor;

			return true;
		}

		public override void ReactionComplete(){
			ReactionProgress = 0;

			Item input = this.RetrieveItem(0);

			VanillaMethods.Player_ExtractinatorUse(ItemID.Sets.ExtractinatorMode[input.type], out int type, out int stack);

			input.stack--;
			if(input.stack <= 0)
				input.TurnToAir();

			if(type == ItemID.CopperCoin)
				storedCoins += stack;
			else if(type == ItemID.SilverCoin)
				storedCoins += stack * 100;
			else if(type == ItemID.GoldCoin)
				storedCoins += stack * 10000;
			else if(type == ItemID.PlatinumCoin)
				storedCoins += stack * 100000;
			else{
				//Find the first slot that the items can stack to.  If that stack isn't enough, overflow to the next slot
				for(int i = 1; i < SlotsCount; i++){
					Item item = this.RetrieveItem(i);
					if(item.IsAir){
						item.SetDefaults(type);
						item.type = type;
						item.stack = stack;
						break;
					}

					if(item.type == type && item.stack < item.maxStack){
						if(item.stack + stack <= item.maxStack){
							item.stack += stack;
							break;
						}else{
							stack -= item.maxStack - item.stack;
							item.stack = item.maxStack;
						}
					}
				}
			}

			if(type > 0 && stack > 0)
				Main.PlaySound(SoundID.Grab, TileUtils.TileEntityCenter(this, MachineTile));

			frameRand = frameRand2;
			frameRand2 = Main.rand.Next(0, 3);
		}

		public override void PostReaction(){
			if(!ReactionInProgress && this.RetrieveItem(0).IsAir){
				ReactionSpeed *= 1f - 0.73f / 60f;

				if(ReactionSpeed < 1f)
					ReactionSpeed = 1f;

				ReactionProgress = 0;
			}
		}

		public override void OnKill(){
			base.OnKill();

			SpawnCoinsOnLocalPlayer();
		}

		internal void SpawnCoinsOnLocalPlayer(){
			if(storedCoins == 0)
				return;

			var coins = Utils.CoinsSplit(storedCoins);
			if(coins[0] > 0)
				Main.LocalPlayer.QuickSpawnItem(ItemID.CopperCoin, coins[0]);
			if(coins[1] > 0)
				Main.LocalPlayer.QuickSpawnItem(ItemID.SilverCoin, coins[1]);
			if(coins[2] > 0)
				Main.LocalPlayer.QuickSpawnItem(ItemID.GoldCoin, coins[2]);
			if(coins[3] > 0)
				Main.LocalPlayer.QuickSpawnItem(ItemID.PlatinumCoin, coins[3]);

			storedCoins = 0;
		}
	}
}
