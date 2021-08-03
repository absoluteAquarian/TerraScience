using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using TerraScience.Content.ID;
using TerraScience.Content.Items.Materials;
using TerraScience.Content.Tiles.Multitiles;
using TerraScience.Utilities;

namespace TerraScience.Content.TileEntities{
	public class SaltExtractorEntity : MachineEntity, ILiquidMachine{
		public override int SlotsCount => 1;

		public override void ExtraLoad(TagCompound tag){
			this.LoadLiquids(tag);
		}

		public override TagCompound ExtraSave(){
			TagCompound tag = new TagCompound();

			this.SaveLiquids(tag);

			return tag;
		}

		public override void ExtraNetSend(BinaryWriter writer){
			this.SendLiquids(writer);
		}

		public override void ExtraNetReceive(BinaryReader reader){
			this.ReceiveLiquids(reader);
		}

		public override int MachineTile => ModContent.TileType<SaltExtractor>();

		public LiquidEntry[] LiquidEntries{ get; set; } = new LiquidEntry[]{
			new LiquidEntry(max: 10f, isInput: true, MachineLiquidID.Water, MachineLiquidID.Saltwater)
		};
		public int LiquidPlaceDelay{ get; set; }

		public override void PreUpdateReaction(){
			Item salt = this.RetrieveItem(0);

			if(LiquidEntries[0].current > 0 && salt.stack < 100)
				ReactionInProgress = true;
		}

		public override bool UpdateReaction(){
			float litersLostPerSecond = 0f;
			if(LiquidEntries[0].id == MachineLiquidID.Water)
				litersLostPerSecond = 0.05f;
			else if(LiquidEntries[0].id == MachineLiquidID.Saltwater)
				litersLostPerSecond = 0.075f;

			float reaction = ReactionSpeed * litersLostPerSecond / 60f;
			LiquidEntries[0].current -= reaction;

			if(LiquidEntries[0].id == MachineLiquidID.Water)
				ReactionProgress += reaction * 0.5f * 100;
			else if(LiquidEntries[0].id == MachineLiquidID.Saltwater)
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
			if(LiquidEntries[0].current <= 0f && ReactionInProgress){
				ReactionInProgress = false;
				LiquidEntries[0].current = 0f;
				LiquidEntries[0].id = MachineLiquidID.None;
			}

			//Stop the reaction if more than 99 salt items are stored
			if(salt.stack >= 100)
				ReactionInProgress = false;

			//Update the delay timer
			if(LiquidPlaceDelay > 0)
				LiquidPlaceDelay--;
		}

		internal override int[] GetInputSlots() => new int[0];

		internal override int[] GetOutputSlots() => new int[]{ SlotsCount - 1 };

		internal override bool CanInputItem(int slot, Item item) => false;

		public void TryExportLiquid(Point16 pumpPos){ }

		public void TryImportLiquid(Point16 pipePos) => this.TryImportLiquids(pipePos, 0);
	}
}
