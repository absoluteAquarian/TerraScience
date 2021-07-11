using MagicStorage;
using MagicStorage.Components;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using TerraScience.Content.TileEntities;
using TerraScience.Utilities;

namespace TerraScience.API.CrossMod.MagicStorage{
	public static class MagicStorageHandler{
		public static ModHandler handler;

		public static bool GUIRefreshPending;

		public static bool DelayInteractionsDueToWorldSaving;

		public static int ItemType(string name) => handler.Instance?.ItemType(name) ?? 0;

		public static int TileType(string name) => handler.Instance?.TileType(name) ?? 0;

		public static IEnumerable<Item> TryGetItems(Point16 tileCoord)
			=> handler.ModIsActive && !DelayInteractionsDueToWorldSaving ? StrongRef_TryGetItems(tileCoord) : null;

		private static IEnumerable<Item> StrongRef_TryGetItems(Point16 tileCoord){
			if(!(StrongRef_HasStorageHeartAt(tileCoord) || StrongRef_HasStorageAccessAt(tileCoord) || StrongRef_HasRemoteStorageAccessAt(tileCoord)))
				return null;

			Tile tile = Framing.GetTileSafely(tileCoord);
			ModTile mTile = ModContent.GetModTile(tile.type);

			if(!tile.active())
				return null;

			Point16 origin = tile.TileCoord();

			if(!(mTile is StorageAccess access))
				return null;

			return access.GetHeart(tileCoord.X - origin.X, tileCoord.Y - origin.Y).GetStoredItems();
		}

		public static bool RefreshGUIs()
			=> handler.ModIsActive && StrongRef_RefreshGUIs();

		private static bool StrongRef_RefreshGUIs(){
			if(Main.LocalPlayer.GetModPlayer<StoragePlayer>().ViewingStorage().X < 0)  //Not in one of the GUIs
				return false;

			//Calls CraftingGUI.RefreshItems instead if the player is in the crafting GUI
			StorageGUI.RefreshItems();
			return true;
		}

		public static bool HasStorageHeartAt(Point16 tileCoord)
			=> handler.ModIsActive && StrongRef_HasStorageHeartAt(tileCoord);

		private static bool StrongRef_HasStorageHeartAt(Point16 tileCoord){
			Tile tile = Framing.GetTileSafely(tileCoord);
			ModTile mTile = ModContent.GetModTile(tile.type);

			if(!tile.active())
				return false;

			return mTile is StorageHeart;
		}

		public static bool HasStorageAccessAt(Point16 tileCoord)
			=> handler.ModIsActive && StrongRef_HasStorageAccessAt(tileCoord);

		private static bool StrongRef_HasStorageAccessAt(Point16 tileCoord){
			Tile tile = Framing.GetTileSafely(tileCoord);
			ModTile mTile = ModContent.GetModTile(tile.type);

			if(!tile.active())
				return false;

			return mTile is StorageAccess access && access.ItemType(0, 0) == ItemType("StorageAccess");
		}

		public static bool HasRemoteStorageAccessAt(Point16 tileCoord)
			=> handler.ModIsActive && StrongRef_HasStorageAccessAt(tileCoord);

		private static bool StrongRef_HasRemoteStorageAccessAt(Point16 tileCoord){
			Tile tile = Framing.GetTileSafely(tileCoord);
			ModTile mTile = ModContent.GetModTile(tile.type);

			if(!tile.active())
				return false;

			return mTile is RemoteAccess access && access.ItemType(0, 0) == ItemType("RemoteAccess");
		}

		public static bool HasStorageUnitAt(Point16 tileCoord)
			=> handler.ModIsActive && StrongRef_HasStorageUnitAt(tileCoord);

		private static bool StrongRef_HasStorageUnitAt(Point16 tileCoord){
			Tile tile = Framing.GetTileSafely(tileCoord);
			ModTile mTile = ModContent.GetModTile(tile.type);

			if(!tile.active())
				return false;

			return mTile is StorageUnit;
		}

		/// <summary>
		/// Attempts to store the input item, <paramref name="item"/>, into a Magic Storage system at the tile position, <paramref name="tileCoord"/>.
		/// </summary>
		/// <param name="item">The item to be deposited.
		/// <para>If the operation was completely successful, this item will be converted into an "air" item.</para>
		/// <para>If the operation was only partially successful, its stack will be reduced.</para></param>
		/// <param name="tileCoord">The tile coordinate</param>
		/// <param name="completeDeposit">Whether the deposit was partially (<see langword="false"/>) or completely (<see langword="true"/>) successful.</param>
		/// <returns>If the item was able to at least be partially deposited into the Magic Storage system</returns>
		public static bool TryDepositItem(Item item, Point16 tileCoord, bool checkOnly, out bool completeDeposit){
			completeDeposit = false;
			return handler.ModIsActive && !DelayInteractionsDueToWorldSaving && StrongRef_TryDepositItem(item, tileCoord, checkOnly, out completeDeposit);
		}

		private static bool StrongRef_TryDepositItem(Item item, Point16 tileCoord, bool checkOnly, out bool completeDeposit){
			completeDeposit = false;
			if(!(StrongRef_HasStorageHeartAt(tileCoord) || StrongRef_HasStorageAccessAt(tileCoord) || StrongRef_HasRemoteStorageAccessAt(tileCoord)))
				return false;

			Tile tile = Framing.GetTileSafely(tileCoord);
			ModTile mTile = ModContent.GetModTile(tile.type);

			if(!tile.active())
				return false;

			Point16 origin = tile.TileCoord();

			if(!(mTile is StorageAccess access))
				return false;

			Item clone = item.Clone();
			Item clone2 = item.Clone();

			var heart = access.GetHeart(tileCoord.X - origin.X, tileCoord.Y - origin.Y);
			heart.DepositItem(clone);

			clone2.stack -= clone.stack;

			//If the item was completely deposited, then it will be air
			completeDeposit = clone.IsAir;

			if(checkOnly && clone2.stack > 0){
				//We're just checking if the item can be deposited.  Withdraw it immediately
				heart.TryWithdraw(clone2);
			}

			return true;
		}

		public static bool TryWithdrawItems(Point16 tileCoord, Item item, bool checkOnly, out Item withdrawn){
			withdrawn = null;
			return handler.ModIsActive && !DelayInteractionsDueToWorldSaving && StrongRef_TryWithdrawItems(tileCoord, item, checkOnly, out withdrawn);
		}

		private static bool StrongRef_TryWithdrawItems(Point16 tileCoord, Item item, bool checkOnly, out Item withdrawn){
			withdrawn = null;
			if(!(StrongRef_HasStorageHeartAt(tileCoord) || StrongRef_HasStorageAccessAt(tileCoord) || StrongRef_HasRemoteStorageAccessAt(tileCoord)))
				return false;

			Tile tile = Framing.GetTileSafely(tileCoord);
			ModTile mTile = ModContent.GetModTile(tile.type);

			if(!tile.active())
				return false;

			Point16 origin = tile.TileCoord();

			if(!(mTile is StorageAccess access))
				return false;

			var heart = access.GetHeart(tileCoord.X - origin.X, tileCoord.Y - origin.Y);
			withdrawn = heart.TryWithdraw(item);

			if(checkOnly && !withdrawn.IsAir)
				heart.DepositItem(withdrawn);

			return withdrawn != null;
		}

		public static bool TryWithdrawItemsToMachine(Point16 tileCoord, MachineEntity entity, Chest simulation, bool checkOnly, int stackExtracted, out Item toWithdraw){
			//Only support withdrawing to machines
			toWithdraw = null;
			return handler.ModIsActive && !DelayInteractionsDueToWorldSaving && StrongRef_TryWithdrawItems(tileCoord, entity, simulation, checkOnly, stackExtracted, out toWithdraw);
		}

		private static bool StrongRef_TryWithdrawItems(Point16 tileCoord, MachineEntity entity, Chest simulation, bool checkOnly, int stackExtracted, out Item toWithdraw){
			toWithdraw = null;
			if(!(StrongRef_HasStorageHeartAt(tileCoord) || StrongRef_HasStorageAccessAt(tileCoord) || StrongRef_HasRemoteStorageAccessAt(tileCoord)))
				return false;

			Tile tile = Framing.GetTileSafely(tileCoord);
			ModTile mTile = ModContent.GetModTile(tile.type);

			if(!tile.active())
				return false;

			Point16 origin = tile.TileCoord();

			if(!(mTile is StorageAccess access))
				return false;

			var heart = access.GetHeart(tileCoord.X - origin.X, tileCoord.Y - origin.Y);
			var items = heart.GetStoredItems();

			foreach(var item in items){
				Item clone = item.Clone();
				clone.stack = Math.Min(stackExtracted, clone.stack);

				if(entity.CanBeInput(clone) && simulation.TryInsertItems(clone)){
					heart.TryWithdraw(clone);

					toWithdraw = clone;
					break;
				}
			}

			if(toWithdraw != null){
				if(checkOnly)
					heart.DepositItem(toWithdraw);

				return true;
			}

			return false;
		}
	}
}
