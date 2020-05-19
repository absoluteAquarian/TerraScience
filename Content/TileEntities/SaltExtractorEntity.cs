using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using TerraScience.Content.API.UI;
using TerraScience.Content.Tiles.Multitiles;
using TerraScience.Utilities;

namespace TerraScience.Content.TileEntities{
	public class SaltExtractorEntity : MachineEntity{
		/// <summary>
		/// How much liquid is stored in the Salt Extractor in Liters.
		/// </summary>
		public float StoredLiquid = 0f;

		/// <summary>
		/// How much salt is stored in the Salt Extractor.
		/// Once this reaches 1f, a Sodium Chloride item and H2O gas is spawned.
		/// Conversion rate is 2L of water per Sodium Chloride generated.
		/// </summary>
		public float StoredSalt = 0f;

		/// <summary>
		/// How many salt items are stored in the extractor.
		/// </summary>
		public int StoredSaltItems = 0;

		/// <summary>
		/// How many water (H20) items are stored in the extractor.
		/// </summary>
		public int StoredWaterItems = 0;

		public static readonly float MaxLiquid = 10f;

		/// <summary>
		/// The max delay between water placements into the machine.
		/// </summary>
		public const int MaxPlaceDelay = 12;
		public int WaterPlaceDelay = 0;

		public SE_LiquidType LiquidType = SE_LiquidType.None;

		public override void ExtraLoad(TagCompound tag){
			StoredLiquid = tag.GetFloat(nameof(StoredLiquid));
			StoredSalt = tag.GetFloat(nameof(StoredSalt));
			StoredSaltItems = tag.GetInt(nameof(StoredSaltItems));
			StoredWaterItems = tag.GetInt(nameof(StoredWaterItems));
			LiquidType = (SE_LiquidType)tag.GetInt(nameof(LiquidType));
		}

		public override TagCompound ExtraSave()
			=> new TagCompound(){
				[nameof(StoredLiquid)] = StoredLiquid,
				[nameof(StoredSalt)] = StoredSalt,
				[nameof(StoredSaltItems)] = StoredSaltItems,
				[nameof(StoredWaterItems)] = StoredWaterItems,
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
				StoredSalt += reaction * 0.5f;
			else if(LiquidType == SE_LiquidType.Saltwater)
				StoredSalt += reaction * 1.5f;

			ReactionProgress = StoredSalt * 100;

			return true;
		}

		public override void ReactionComplete(){
			UIItemSlot itemSlot_Salt = TerraScience.Instance.machineLoader.SaltExtractorState.ItemSlot_Salt;
			UIItemSlot itemSlot_Water = TerraScience.Instance.machineLoader.SaltExtractorState.ItemSlot_Water;

			StoredSalt--;

			int saltType = mod.ItemType(CompoundUtils.CompoundName(Compound.SodiumChloride, false));
			int waterType = mod.ItemType(CompoundUtils.CompoundName(Compound.Water, false));

			if (itemSlot_Salt.StoredItem.type != saltType)
				itemSlot_Salt.SetItem(saltType);
			else if(itemSlot_Salt.StoredItem.stack < 100)
				itemSlot_Salt.StoredItem.stack++;
			if(itemSlot_Water.StoredItem.type != waterType)
				itemSlot_Water.SetItem(waterType);
			else if(itemSlot_Water.StoredItem.stack < 100)
				itemSlot_Water.StoredItem.stack++;

			StoredSaltItems++;
			StoredWaterItems++;

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
			UIItemSlot itemSlot_Salt = TerraScience.Instance.machineLoader.SaltExtractorState.ItemSlot_Salt;
			UIItemSlot itemSlot_Water = TerraScience.Instance.machineLoader.SaltExtractorState.ItemSlot_Water;

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
			if(itemSlot_Salt.StoredItem.stack >= 100 || itemSlot_Water.StoredItem.stack >= 100)
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
