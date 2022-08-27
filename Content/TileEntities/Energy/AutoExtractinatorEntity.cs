using Microsoft.Xna.Framework;
using System.IO;
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

		public override TerraFlux FluxUsage => new TerraFlux(140f / 60f);

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

		public override void ExtraNetSend(BinaryWriter writer){
			base.ExtraNetSend(writer);

			writer.Write(storedCoins);
		}

		public override void ExtraNetReceive(BinaryReader reader){
			base.ExtraNetReceive(reader);

			storedCoins = reader.ReadInt64();
		}

		public override void PreUpdateReaction(){
			bool hasFlux = CheckFluxRequirement(FluxUsage, use: false);
			ReactionInProgress = !this.RetrieveItem(0).IsAir && hasFlux;

			this.StopReactionIfOutputSlotsAreFull(1, SlotsCount - 1);

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
			else if(type > 0 && stack > 0){
				this.TryInsertOutput(1, SlotsCount - 1, type, stack);

				this.PlaySound(SoundID.Grab, TileUtils.TileEntityCenter(this, MachineTile));
			}

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
				Main.LocalPlayer.QuickSpawnItem(Main.LocalPlayer.GetSource_DropAsItem(), ItemID.CopperCoin, coins[0]);
			if(coins[1] > 0)
				Main.LocalPlayer.QuickSpawnItem(Main.LocalPlayer.GetSource_DropAsItem(), ItemID.SilverCoin, coins[1]);
			if(coins[2] > 0)
				Main.LocalPlayer.QuickSpawnItem(Main.LocalPlayer.GetSource_DropAsItem(), ItemID.GoldCoin, coins[2]);
			if(coins[3] > 0)
				Main.LocalPlayer.QuickSpawnItem(Main.LocalPlayer.GetSource_DropAsItem(), ItemID.PlatinumCoin, coins[3]);

			storedCoins = 0;
		}

		internal override int[] GetInputSlots() => new int[]{ 0 };

		internal override int[] GetOutputSlots() => new int[]{ 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20 };

		internal override bool CanInputItem(int slot, Item item)
			=> slot == 0 && ItemID.Sets.ExtractinatorMode[item.type] >= 0;
	}
}
