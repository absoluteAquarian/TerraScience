using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using TerraScience.Content.Tiles.Multitiles;
using TerraScience.Content.UI.Elements;

namespace TerraScience.Content.TileEntities{
	public class SaltExtractorEntity : ModTileEntity{
		/// <summary>
		/// How much water is stored in the Salt Extractor in Liters.
		/// </summary>
		public float StoredWater = 0f;

		/// <summary>
		/// How much salt is stored in the Salt Extractor.
		/// Once this reaches 1f, a Sodium Chloride item and H2O gas is spawned.
		/// Conversion rate is 2L of water per Sodium Chloride generated.
		/// </summary>
		public float StoredSalt = 0f;

		public static readonly float MaxWater = 10f;

		/// <summary>
		/// How quickly the water is evaporated and salt is created.
		/// </summary>
		public float ReactionSpeed = 1f;

		/// <summary>
		/// The progress for the current reaction.
		/// Range: [0, 100]
		/// </summary>
		public float ReactionProgress = 0f;

		public bool ReactionInProgress = false;

		/// <summary>
		/// The max delay between water placements into the machine.
		/// </summary>
		public const int MaxPlaceDelay = 12;
		public int WaterPlaceDelay = 0;

		public override void Load(TagCompound tag){
			StoredWater = tag.GetFloat(nameof(StoredWater));
			StoredSalt = tag.GetFloat(nameof(StoredSalt));
			ReactionSpeed = tag.GetFloat(nameof(ReactionSpeed));
			ReactionProgress = tag.GetFloat(nameof(ReactionProgress));
			ReactionInProgress = tag.GetBool(nameof(ReactionInProgress));
		}

		public override TagCompound Save()
			=> new TagCompound(){
				[nameof(StoredWater)] = StoredWater,
				[nameof(StoredSalt)] = StoredSalt,
				[nameof(ReactionSpeed)] = ReactionSpeed,
				[nameof(ReactionProgress)] = ReactionProgress,
				[nameof(ReactionInProgress)] = ReactionInProgress
			};

		//The spawn and despawn code is handled elsewhere, so just return true
		public override bool ValidTile(int i, int j){
			Tile tile = Main.tile[i, j];
			return tile != null && tile.active() && tile.type == ModContent.TileType<SaltExtractor>() && tile.frameX == 0 && tile.frameY == 0;
		}

		public override void Update(){
			if(ReactionInProgress){
				float litersLostPerSecond = 0.05f;
				float reaction = ReactionSpeed * litersLostPerSecond / 60f;
				StoredWater -= reaction;
				StoredSalt += reaction / 2f;

				ReactionProgress = StoredSalt * 100;

				if(StoredSalt >= 1f){
					StoredSalt--;

					UIItemSlot itemSlot = ModContent.GetInstance<TerraScience>().saltExtracterLoader.saltExtractorUI.itemSlot;

					if (itemSlot.StoredItem.type != mod.ItemType("SodiumChloride"))
						itemSlot.SetItem(mod.ItemType("SodiumChloride"));
					else if(itemSlot.StoredItem.stack < 100)
						itemSlot.StoredItem.stack++;

					//TerraScience.SpawnScienceItem(Position.X * 16 + 32, Position.Y * 16 + 24, 16, 16, Compound.SodiumChloride);
					//TerraScience.SpawnScienceItem(Position.X * 16 + 48, Position.Y * 16 + 8, 16, 16, Compound.Water, 1, new Vector2(Main.rand.NextFloat(-1.5f, 1.5f), Main.rand.NextFloat(-2.25f, -4f)));
				}

				//Reaction happens faster and faster as it "heats up", but cools down very quickly
				ReactionSpeed *= 1f + 0.0392f / 60f;

				//Reaction speed caps at 2x speed
				if(ReactionSpeed > 2f)
					ReactionSpeed = 2f;
			}else{
				ReactionSpeed *= 1f - 0.0943f / 60f;

				if(ReactionSpeed < 1f)
					ReactionSpeed = 1f;
			}

			//If there isn't any water left, pause the reaction
			if(StoredWater <= 0f && ReactionInProgress){
				ReactionInProgress = false;
				StoredWater = 0f;
				ReactionProgress = 0f;
			}

			//Update the delay timer
			if(WaterPlaceDelay > 0)
				WaterPlaceDelay--;
		}
	}
}
