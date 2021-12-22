using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using TerraScience.API.CrossMod.MagicStorage;
using TerraScience.Content.Tiles.Multitiles;
using TerraScience.Content.UI;
using TerraScience.Systems;

namespace TerraScience.Content.TileEntities {
	public class MagicStorageConnectorEntity : MachineEntity{
		public override int MachineTile => ModContent.TileType<MagicStorageConnector>();

		public override int SlotsCount => 0;

		public override bool UpdateReaction() => false;

		public override void ReactionComplete(){ }

		//This tile needs a special use case for these methods
		internal override bool CanInputItem(int slot, Item item) => false;

		internal override int[] GetInputSlots() => new int[0];

		internal override int[] GetOutputSlots() => new int[0];

		public Point16 GetConnectedMagicStorageSystem()
			=> MagicStorageHandler.handler.ModIsActive && !MagicStorageHandler.DelayInteractionsDueToWorldSaving
				? MagicStorageConnectorUI.FindMagicStorageSystem(Position)
				: MagicStorageConnectorUI.badCheck;

		public override bool HijackCanBeInteractedWithItemNetworks(out bool canInteract, out bool canInput, out bool canOutput){
			canInteract = canInput = canOutput = MagicStorageHandler.handler.ModIsActive;
			return true;
		}

		public override bool HijackCanBeInput(Item item, out bool canInput){
			canInput = MagicStorageHandler.handler.ModIsActive && !MagicStorageHandler.DelayInteractionsDueToWorldSaving && StrongRef_MagicStorageCanBeInput(item);
			return true;
		}

		private bool StrongRef_MagicStorageCanBeInput(Item item){
			var center = MagicStorageConnectorUI.FindMagicStorageSystem(Position);

			if(center == MagicStorageConnectorUI.badCheck)
				return false;  //No storage system found

			return MagicStorageHandler.TryDepositItem(item, center, checkOnly: true, out _);
		}

		public override bool HijackGetItemInventory(out Item[] inventory){
			Point16 pos = GetConnectedMagicStorageSystem();
			inventory = null;

			if(pos == MagicStorageConnectorUI.badCheck)
				return true;

			inventory = MagicStorageHandler.TryGetItems(pos)?.ToArray();
			return true;
		}

		public override bool HijackExtractItem(Item[] inventory, int slot, int toExtract, out Item item){
			Point16 systemCoord = GetConnectedMagicStorageSystem();

			Item extract = inventory[slot].Clone();
			extract.stack = toExtract;

			MagicStorageHandler.TryWithdrawItems(systemCoord, extract, checkOnly: false, out item);

			MagicStorageHandler.GUIRefreshPending = true;

			return true;
		}

		public override bool HijackSimulateInput(Item incoming, List<ItemNetworkPath> paths, out bool success){
			success = false;
			if(!MagicStorageHandler.handler.ModIsActive)
				return true;

			Point16 pos = GetConnectedMagicStorageSystem();

			if(pos == MagicStorageConnectorUI.badCheck)
				return true;

			//Input the path items, then this one
			bool completeDeposit;
			foreach(var path in paths){
				var item = ItemIO.Load(path.itemData);

				MagicStorageHandler.TryDepositItem(item, pos, checkOnly: true, out completeDeposit);

				//If one of the path items would result in the system overflowing, terminate immediately
				if(!completeDeposit)
					return true;
			}

			//Check this item
			MagicStorageHandler.TryDepositItem(incoming, pos, checkOnly: true, out completeDeposit);
			success = completeDeposit;
			return true;
		}

		public override bool HijackInsertItem(ItemNetworkPath incoming, out bool sendBack){
			Item data = ItemIO.Load(incoming.itemData);
			Point16 systemCoord = GetConnectedMagicStorageSystem();

			sendBack = false;
			if(systemCoord == MagicStorageConnectorUI.badCheck)
				return false;

			MagicStorageHandler.TryDepositItem(data, systemCoord, checkOnly: false, out bool completeDeposit);

			MagicStorageHandler.GUIRefreshPending = true;

			if(!completeDeposit){
				//Update the path item and send it back
				incoming.itemData = ItemIO.Save(data);
				sendBack = true;
			}else
				sendBack = false;

			return true;
		}
	}
}
