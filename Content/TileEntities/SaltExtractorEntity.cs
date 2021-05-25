using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using TerraScience.Content.ID;
using TerraScience.Content.Items.Materials;
using TerraScience.Content.Tiles.Multitiles;
using TerraScience.Utilities;

namespace TerraScience.Content.TileEntities{
	public class SaltExtractorEntity : MachineEntity, ILiquidMachine{
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

		public override int SlotsCount => 1;

		public override void ExtraLoad(TagCompound tag){
			StoredLiquid = tag.GetFloat(nameof(StoredLiquid));
			this.LoadLiquids(tag, failsafeArrayLength: 1);
		}

		public override TagCompound ExtraSave(){
			TagCompound tag = new TagCompound(){
				[nameof(StoredLiquid)] = StoredLiquid
			};

			this.SaveLiquids(tag);

			return tag;
		}

		public override int MachineTile => ModContent.TileType<SaltExtractor>();

		public MachineLiquidID[] LiquidTypes{ get; set; }
		public float[] StoredLiquidAmounts{ get; set; }

		public override bool UpdateReaction(){
			float litersLostPerSecond = 0f;
			if(LiquidTypes[0] == MachineLiquidID.Water)
				litersLostPerSecond = 0.05f;
			else if(LiquidTypes[0] == MachineLiquidID.Saltwater)
				litersLostPerSecond = 0.075f;

			float reaction = ReactionSpeed * litersLostPerSecond / 60f;
			StoredLiquid -= reaction;

			if(LiquidTypes[0] == MachineLiquidID.Water)
				ReactionProgress += reaction * 0.5f * 100;
			else if(LiquidTypes[0] == MachineLiquidID.Saltwater)
				ReactionProgress += reaction * 1.5f * 100;

			return true;
		}

		public override void ReactionComplete(){
			Item salt = this.RetrieveItem(0);

			if(salt.IsAir){
				salt.SetDefaults(ModContent.ItemType<Salt>());
				salt.stack = 0;
			}

			salt.stack++;

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

			if(!ReactionInProgress){
				ReactionSpeed *= 1f - 0.0943f / 60f;

				if(ReactionSpeed < 1f)
					ReactionSpeed = 1f;
			}

			//If there isn't any water left, pause the reaction
			if(StoredLiquid <= 0f && ReactionInProgress){
				ReactionInProgress = false;
				StoredLiquid = 0f;
				LiquidTypes[0] = MachineLiquidID.None;
			}

			//Stop the reaction if more than 99 salt items are stored
			if(salt.stack >= 100)
				ReactionInProgress = false;

			//Update the delay timer
			if(WaterPlaceDelay > 0)
				WaterPlaceDelay--;
		}

		internal override int[] GetInputSlots() => new int[0];

		internal override int[] GetOutputSlots() => new int[]{ SlotsCount - 1 };

		internal override bool CanInputItem(int slot, Item item) => false;
	}
}
