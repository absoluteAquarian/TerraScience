using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using TerraScience.Content.API.UI;
using TerraScience.Content.Tiles.Multitiles;
using TerraScience.Content.UI;
using TerraScience.Utilities;

namespace TerraScience.Content.TileEntities{
	public class SaltExtractorEntity : MachineEntity{
		/// <summary>
		/// How much liquid is stored in the Salt Extractor in Liters.
		/// </summary>
		public float StoredLiquid = 0f;

		public static readonly float MaxLiquid = 10f;

		/// <summary>
		/// The max delay between water placements into the machine.
		/// </summary>
		public const int MaxPlaceDelay = 12;
		public int WaterPlaceDelay = 0;

		public SE_LiquidType LiquidType = SE_LiquidType.None;

		public override int SlotsCount => 2;

		public override void ExtraLoad(TagCompound tag){
			StoredLiquid = tag.GetFloat(nameof(StoredLiquid));
			LiquidType = (SE_LiquidType)tag.GetInt(nameof(LiquidType));
		}

		public override TagCompound ExtraSave()
			=> new TagCompound(){
				[nameof(StoredLiquid)] = StoredLiquid,
				[nameof(LiquidType)] = (int)LiquidType
			};

		public override int MachineTile => ModContent.TileType<SaltExtractor>();

		public override bool UpdateReaction(){
			float litersLostPerSecond = 0f;
			if(LiquidType == SE_LiquidType.Water)
				litersLostPerSecond = 0.05f;
			else if(LiquidType == SE_LiquidType.Saltwater)
				litersLostPerSecond = 0.075f;

			float reaction = ReactionSpeed * litersLostPerSecond / 60f;
			StoredLiquid -= reaction;

			if(LiquidType == SE_LiquidType.Water)
				ReactionProgress += reaction * 0.5f * 100;
			else if(LiquidType == SE_LiquidType.Saltwater)
				ReactionProgress += reaction * 1.5f * 100;

			return true;
		}

		public override void ReactionComplete(){
			Item salt = this.RetrieveItem(0);
			Item water = this.RetrieveItem(1);

			if(salt.IsAir){
				salt.SetDefaults(CompoundUtils.CompoundType(Compound.SodiumChloride));
				salt.stack = 0;
			}
			if(water.IsAir){
				water.SetDefaults(CompoundUtils.CompoundType(Compound.Water));
				water.stack = 0;
			}

			salt.stack++;
			water.stack++;

			Main.PlaySound(new LegacySoundStyle(SoundID.Grab, 0).WithVolume(0.5f), Position.ToWorldCoordinates());
		}

		public override void PostUpdateReaction(){
			//Reaction happens faster and faster as it "heats up", but cools down very quickly
			ReactionSpeed *= 1f + 0.0392f / 60f;

			//Reaction speed caps at 2x speed
			if(ReactionSpeed > 2f)
				ReactionSpeed = 2f;
		}

		public override void PostReaction(){
			Item salt = this.RetrieveItem(0);
			Item water = this.RetrieveItem(1);

			if(!ReactionInProgress){
				ReactionSpeed *= 1f - 0.0943f / 60f;

				if(ReactionSpeed < 1f)
					ReactionSpeed = 1f;
			}

			//If there isn't any water left, pause the reaction
			if(StoredLiquid <= 0f && ReactionInProgress){
				ReactionInProgress = false;
				StoredLiquid = 0f;
				LiquidType = SE_LiquidType.None;
			}

			//Stop the reaction if more than 99 salt or water items are stored
			if(salt.stack >= 100 || water.stack >= 100)
				ReactionInProgress = false;

			//Update the delay timer
			if(WaterPlaceDelay > 0)
				WaterPlaceDelay--;
		}

		public enum SE_LiquidType{
			None,
			Water,
			Saltwater
		}
	}
}
