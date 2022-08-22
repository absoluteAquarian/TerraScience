using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.ObjectData;
using Terraria.UI;
using TerraScience.Content.ID;
using TerraScience.Content.Items.Materials;
using TerraScience.Content.Items.Placeable.Machines;
using TerraScience.Content.Items.Tools;
using TerraScience.Content.TileEntities;
using TerraScience.Content.TileEntities.Energy;
using TerraScience.Content.Tiles.Multitiles;
using TerraScience.Content.UI;
using TerraScience.Systems;

namespace TerraScience.Utilities{
	public static class TileUtils{
		public static ushort SupportType => (ushort)ModContent.TileType<MachineSupport>();
		public static ushort BlastFurnaceType => (ushort)ModContent.TileType<BlastBrickTile>();

		public static Dictionary<int, MachineEntity> tileToEntity;

		public static Dictionary<int, string> tileToStructureName;

		public static void RegisterAllEntities(){
			var types = TechMod.types;
			foreach(var type in types){
				if(type.IsAbstract)
					continue;

				if(typeof(MachineEntity).IsAssignableFrom(type)){
					var entity = TechMod.Instance.Find<ModTileEntity>(type.Name) as MachineEntity;
					var tileType = entity.MachineTile;

					Type tileTypeInst = TileLoader.GetTile(tileType).GetType();

					tileToEntity.Add(tileType, entity);
					tileToStructureName.Add(tileType, tileTypeInst.Name);

					TechMod.Instance.Logger.Debug($"Registered tile type \"{tileTypeInst.FullName}\" (ID: {tileType}) with entity \"{type.FullName}\"");
				}
			}
		}

		public static Vector2 TileEntityCenter(TileEntity entity, int tileType) {
			Machine tile = ModContent.GetModTile(tileType) as Machine;
			tile.GetDefaultParams(out _, out uint width, out uint height, out _);

			Point16 topLeft = entity.Position;
			Point16 size = new Point16((int)width, (int)height);
			Vector2 worldTopLeft = topLeft.ToVector2() * 16;
			return worldTopLeft + size.ToVector2() * 8; // * 16 / 2
		}

		public static Point16 Frame(this Tile tile)
			=> new Point16(tile.TileFrameX, tile.TileFrameY);

		public static Point16 TileCoord(this Tile tile)
			=> new Point16(tile.TileFrameX / 18, tile.TileFrameY / 18);

		public static Texture2D GetEffectTexture(this ModTile multitile, string effect){
			try{
				return ModContent.Request<Texture2D>($"TerraScience/Content/Tiles/Multitiles/Overlays/Effect_{multitile.Name}_{effect}").Value;
			}catch{
				throw new ContentLoadException($"Could not find overlay texture \"{effect}\" for machine \"{multitile.Name}\"");
			}
		}

		public static string GetExampleTexturePath(this ModTile multitile, string example)
			=> $"TerraScience/Content/Tiles/Multitiles/Examples/Example_{multitile.Name}_{example}";

		public static void KillMachine(int i, int j, int type){
			Tile tile = Main.tile[i, j];
			Machine mTile = ModContent.GetModTile(type) as Machine;
			mTile.GetDefaultParams(out _, out _, out _, out int itemType);

			int itemIndex = Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 16, 16, itemType);
			MachineItem item = Main.item[itemIndex].ModItem as MachineItem;

			Point16 tePos = new Point16(i, j) - tile.TileCoord();
			if(TileEntity.ByPosition.ContainsKey(tePos)){
				MachineEntity tileEntity = TileEntity.ByPosition[tePos] as MachineEntity;
				//Drop any items the entity contains
				if(tileEntity.SlotsCount > 0){
					for(int slot = 0; slot < tileEntity.SlotsCount; slot++){
						Item drop = tileEntity.RetrieveItem(slot);

						//Drop the item and copy over any important data
						if(drop.type > ItemID.None && drop.stack > 0){
							int dropIndex = Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 16, 16, drop.type, drop.stack);
							if(drop.ModItem != null) {
								TagCompound dropInfo = new TagCompound();
								drop.ModItem.SaveData(dropInfo);
								Main.item[dropIndex].ModItem.LoadData(dropInfo);
							}
						}

						tileEntity.ClearItem(slot);
					}
				}

				tileEntity.SaveData(item.entityData);

				//Remove this machine from the wire networks if it's a powered machine
				if(tileEntity is PoweredMachineEntity pme)
					NetworkCollection.RemoveMachine(pme);

				tileEntity.Kill(i, j);

				if(Main.netMode == NetmodeID.MultiplayerClient)
					NetMessage.SendData(MessageID.TileEntitySharing, remoteClient: -1, ignoreClient: Main.myPlayer, number: tileEntity.ID);
			}
		}

		public static void MultitileDefaults(ModTile tile, string mapName, int type, uint width, uint height){
			Main.tileNoAttach[type] = true;
			Main.tileFrameImportant[type] = true;

			TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop | AnchorType.SolidBottom, (int)width, 0);
			TileObjectData.newTile.CoordinateHeights = MiscUtils.Create1DArray(16, height);
			TileObjectData.newTile.CoordinateWidth = 16;
			TileObjectData.newTile.Height = (int)height;
			TileObjectData.newTile.Width = (int)width;
			TileObjectData.newTile.CoordinatePadding = 2;
			TileObjectData.newTile.LavaDeath = false;
			TileObjectData.newTile.WaterDeath = false;
			TileObjectData.newTile.LavaPlacement = LiquidPlacement.NotAllowed;
			TileObjectData.newTile.WaterPlacement = LiquidPlacement.NotAllowed;
			TileObjectData.newTile.Origin = new Point16((int)width / 2, (int)height - 1);
			TileObjectData.newTile.UsesCustomCanPlace = true;

			TileObjectData.addTile(type);

			ModTranslation name = tile.CreateMapEntryName();
			name.SetDefault(mapName);
			tile.AddMapEntry(new Color(0xd1, 0x89, 0x32), name);

			tile.MineResist = 3f;
			//Metal sound
			tile.HitSound = SoundID.Tink;
		}

		public static bool HandleMouse<TEntity>(Machine machine, Point16 tilePos, Func<bool> additionalCondition) where TEntity : MachineEntity{
			if(MiscUtils.TryGetTileEntity(tilePos, out TEntity entity) && (additionalCondition?.Invoke() ?? true)){
				TechMod instance = TechMod.Instance;
				string name = tileToStructureName[instance.Find<ModTile>(machine.Name).Type];

				UserInterface ui = instance.machineLoader.GetInterface(name);

				//Force the current one to close if another one of the same type is going to be opened
				if(ui.CurrentState is MachineUI mui && mui.UIEntity.Position != tilePos)
					instance.machineLoader.HideUI(mui.MachineName);

				if(ui.CurrentState == null)
					instance.machineLoader.ShowUI(name, entity);
				else
					instance.machineLoader.HideUI(name);

				return true;
			}

			return false;
		}

		public static bool TryExtractItems(this Chest chest, int stackToExtract, List<int> slots, out Item item){
			int id = Chest.FindChest(chest.x, chest.y);

			void SyncSlot(int slot){
				if(Main.netMode != NetmodeID.SinglePlayer)
					NetMessage.SendData(MessageID.SyncChestItem, number: id, number2: slot, number3: 0);
			}

			item = null;
			for(int i = 0; i < slots.Count; i++){
				Item slotItem = chest.item[slots[i]];
				if(slotItem.IsAir)
					continue;

				if(item is null || slotItem.type == item.type){
					if(item is null){
						//Just use this item directly
						item = slotItem.Clone();
						
						if(item.stack > stackToExtract){
							item.stack = stackToExtract;
							slotItem.stack -= stackToExtract;
							stackToExtract = 0;

							SyncSlot(i);
						}else{
							stackToExtract -= item.stack;
							slotItem.stack = 0;

							SyncSlot(i);
						}
					}else{
						//Add to the stack of the output item
						if(slotItem.stack < stackToExtract){
							stackToExtract -= slotItem.stack;
							item.stack += slotItem.stack;
							slotItem.stack = 0;

							SyncSlot(i);
						}else{
							item.stack += stackToExtract;
							slotItem.stack -= stackToExtract;
							stackToExtract = 0;

							SyncSlot(i);
						}
					}
				}

				if(stackToExtract <= 0)
					break;
			}

			return item != null;
		}

		public static bool IsFull(this Chest chest, Item incoming){
			int stack = incoming.stack;

			for(int i = 0; i < chest.item.Length; i++){
				Item chestItem = chest.item[i];

				if(chestItem.IsAir)
					return false;
				else if(chestItem.type == incoming.type){
					if(chestItem.stack + incoming.stack <= chestItem.maxStack)
						return false;
					else
						stack -= chestItem.maxStack - chestItem.stack;
				}
			}

			//Full stack couldn't be put into the chest
			return stack <= 0;
		}

		public static bool TryPlaceLiquidInMachine<T>(Machine machine, Point16 pos) where T : MachineEntity, IFluidMachine{
			Tile tile = Framing.GetTileSafely(pos);

			Point16 orig = pos - tile.TileCoord();

			if(MiscUtils.TryGetTileEntity(orig, out T entity) && entity.FluidPlaceDelay <= 0){
				var id = MiscUtils.GetFluidIDFromItem(Main.LocalPlayer.HeldItem.type);

				//null validTypes means any liquid can be put in the slot
				int index;
				if (id == MachineFluidID.None || (index = Array.FindIndex(entity.FluidEntries, entry => entry.isInput && (entry.validTypes is null || Array.Exists(entry.validTypes, t => t == id)))) == -1 || (entity.FluidEntries[index].id != MachineFluidID.None && entity.FluidEntries[index].id != id))
					return false;

				var use = entity.FluidEntries[index];

				if (use.current + 1 > use.max)
					return false;

				if(use.id == MachineFluidID.None)
					use.id = id;

				use.current += 1;

				if(use.current > use.max)
					use.current = use.max;

				entity.FluidPlaceDelay = ElectrolyzerEntity.MaxPlaceDelay;

				SoundEngine.PlaySound(SoundID.Splash, TileEntityCenter(entity, machine.Type));

				Item heldItem = Main.LocalPlayer.HeldItem;

				//And give the player back the container they used (unless it's the bottomless bucket)
				if (heldItem.type == ItemID.WaterBucket) {
					Main.LocalPlayer.HeldItem.stack--;
					Main.LocalPlayer.QuickSpawnItem(Main.LocalPlayer.GetSource_TileInteraction(tile.TileCoord().X, tile.TileCoord().Y), ItemID.EmptyBucket);
				} else if (heldItem.type == ModContent.ItemType<Vial_Water>() || heldItem.type == ModContent.ItemType<Vial_Saltwater>()) {
					Main.LocalPlayer.HeldItem.stack--;
					Main.LocalPlayer.QuickSpawnItem(Main.LocalPlayer.GetSource_TileInteraction(tile.TileCoord().X, tile.TileCoord().Y), ModContent.ItemType<EmptyVial>());
				}

				//Success
				return true;
			}

			return false;
		}
	}

	public enum TileSlopeVariant{
		Solid,
		DownLeft,
		DownRight,
		UpLeft,
		UpRight,
		HalfBrick
	}
}
