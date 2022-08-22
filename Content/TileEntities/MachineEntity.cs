using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using TerraScience.API.Networking;
using TerraScience.Content.TileEntities.Energy.Generators;
using TerraScience.Content.Tiles;
using TerraScience.Content.UI;
using TerraScience.Systems;
using TerraScience.Utilities;

namespace TerraScience.Content.TileEntities {
	public abstract class MachineEntity : ModTileEntity{
		private Item[] slots;

		public Item GetItem(int slot) => slots[slot];

		internal ref Item GetItemSlotRef(int slot) => ref slots[slot];

		internal void ValidateSlots(int intendedLength){
			//Possible if the multitile was just placed
			if(slots?.Length != intendedLength){
				slots = new Item[intendedLength];
				for(int i = 0; i < intendedLength; i++)
					slots[i] = new Item();
			}
		}

		/// <summary>
		/// The multiplier for the reaction progress.
		/// </summary>
		public float ReactionSpeed = 1f;

		/// <summary>
		/// The progress for the current reaction.
		/// Range: [0, 100]
		/// </summary>
		public float ReactionProgress = 0f;

		public bool ReactionInProgress = false;

		public MachineUI ParentState;

		public string MachineName;

		public abstract int MachineTile{ get; }

		public abstract int SlotsCount{ get; }

		public sealed override bool IsTileValidForEntity(int i, int j){
			Tile tile = Framing.GetTileSafely(i, j);
			return tile.HasTile && tile.TileType == MachineTile && tile.TileFrameX == 0 && tile.TileFrameY == 0;
		}

		public virtual void PreUpdateReaction(){ }

		/// <summary>
		/// Update <seealso cref="ReactionProgress"/> here.  Return true to indicate that a reaction is supposed to happen, false otherwise.
		/// </summary>
		public abstract bool UpdateReaction();

		/// <summary>
		/// Called after <seealso cref="ReactionComplete()"/>, but not necessarily when a reaction is completed.
		/// </summary>
		public virtual void PostUpdateReaction(){ }

		/// <summary>
		/// Actions that should happen when a reaction is complete goes here.
		/// </summary>
		public abstract void ReactionComplete();

		/// <summary>
		/// Always called.  General update task after the reaction has been handled.
		/// </summary>
		public virtual void PostReaction(){ }

		/// <summary>
		/// If this machine must have its UI open in order to update.
		/// </summary>
		public virtual bool RequiresUI => false;

		public sealed override void SaveData(TagCompound tag) {
			tag.Set("machineInfo", new TagCompound() {
				[nameof(ReactionSpeed)] = ReactionSpeed,
				[nameof(ReactionProgress)] = ReactionProgress,
				[nameof(ReactionInProgress)] = ReactionInProgress
			});
			tag.Set("slots", new TagCompound() {
				//Lots of unnecessary data is saved, but that's fine due to the small amount of extra bytes used
				// TODO: refactor ItemIO.Save/ItemIO.Load to get rid of this extra info
				["items"] = slots.Length == 0 ? null : slots.Select(i => ItemIO.Save(i)).ToList()
			});
			tag.Set("extra", ExtraSave());
		}

		public virtual TagCompound ExtraSave() => null;

		public sealed override void Load(Mod mod){
			TagCompound info = tag.GetCompound("machineInfo");
			ReactionSpeed = info.GetFloat(nameof(ReactionSpeed));
			ReactionProgress = info.GetFloat(nameof(ReactionProgress));
			ReactionInProgress = info.GetBool(nameof(ReactionInProgress));

			TagCompound tagSlots = tag.GetCompound("slots");
			List<TagCompound> items = tagSlots.GetList<TagCompound>("items") as List<TagCompound> ?? new List<TagCompound>();
			
			slots = items.Select(ItemIO.Load).ToArray();

			TagCompound extra = tag.GetCompound("extra");
			if(extra != null)
				ExtraLoad(extra);
		}

		public virtual void ExtraLoad(TagCompound tag){ }

		public sealed override void Update(){
			if(RequiresUI && !(ParentState?.Active ?? false))
				return;

			if(this is GeneratorEntity && !updating)
				return;

			ValidateSlots(SlotsCount);

			PreUpdateReaction();

			if(ReactionInProgress && UpdateReaction()){
				if(ReactionProgress >= 100){
					ReactionComplete();

					//In case a derived class forgets to reset the reaction progress
					if(ReactionProgress >= 100)
						ReactionProgress -= 100;
				}

				PostUpdateReaction();
			}

			PostReaction();
		}

		internal bool updating = false;
		//NOTE: ModTileEntity.PreGlobalUpdate() is called from a singleton for some fucking reason

		internal virtual bool SetItemsToParentUIWhenClosing => true;

		public void SaveSlots(){
			if(!SetItemsToParentUIWhenClosing)
				return;

			slots = new Item[ParentState.SlotsLength];

			for(int i = 0; i < ParentState.SlotsLength; i++){
				int type = ParentState.GetSlot(i).StoredItem.type;
				int stack = ParentState.GetSlot(i).StoredItem.stack;

				Item item = new Item();
				item.SetDefaults(type);
				item.stack = stack;

				slots[i] = item;
			}
		}

		public void LoadSlots(){
			ParentState.LoadFromSlots(slots);
		}

		public override void OnKill(){
			//Force the UI to close if it's open
			if(ParentState?.Active ?? false)
				TechMod.Instance.machineLoader.HideUI(MachineName);
		}

		public sealed override void NetSend(BinaryWriter writer){
			writer.Write(ReactionInProgress);
			writer.Write(ReactionSpeed);
			writer.Write(ReactionProgress);

			writer.Write((short)slots.Length);
			for(int i = 0; i < slots.Length; i++)
				ItemIO.Send(slots[i], writer, writeStack: true);

			ExtraNetSend(writer);
		}

		/// <summary>
		/// Write extra data for net syncing, <seealso cref="Message.SyncTileEntity"/> and <seealso cref="Message.SyncMachineInfo"/>
		/// </summary>
		/// <param name="writer">The writer</param>
		public virtual void ExtraNetSend(BinaryWriter writer){ }

		public sealed override void NetReceive(BinaryReader reader){
			ReactionInProgress = reader.ReadBoolean();
			ReactionSpeed = reader.ReadSingle();
			ReactionProgress = reader.ReadSingle();

			short count = reader.ReadInt16();

			if(slots.Length < count){
				int oldLength = slots.Length;

				Item[] slotsNew = new Item[count];
				slots.CopyTo(slotsNew, 0);
				slots = slotsNew;

				for(int c = oldLength; c < count; c++)
					slots[c] = new Item();
			}else if(slots.Length > count){
				Item[] slotsNew = new Item[count];
				slots.CopyTo(slotsNew, 0);
				slots = slotsNew;
			}

			for(int i = 0; i < count; i++)
				ItemIO.Receive(slots[i], reader, readStack: true);

			ExtraNetReceive(reader);
		}

		/// <summary>
		/// Read extra data for net syncing, <seealso cref="Message.SyncTileEntity"/> and <seealso cref="Message.SyncMachineInfo"/>
		/// </summary>
		/// <param name="reader">The reader</param>
		public virtual void ExtraNetReceive(BinaryReader reader){ }

		internal SoundEffectInstance PlayCustomSound(Vector2 position, string path){
			bool nearbyMuffler = WorldGen.InWorld((int)position.X >> 4, (int)position.Y >> 4) && MachineMufflerTile.AnyMufflersNearby(position);
			SoundStyle style = new SoundStyle($"Sounds/Custom/{path}");
			style.Volume = nearbyMuffler ? 0.1f : 1f;
			return SoundEngine.PlaySound(style, (int)position.X, (int)position.Y);
		}

		// These may be invalid.
		//internal void PlaySound(int type, Vector2 position, int style = 1){
		//	bool nearbyMuffler = WorldGen.InWorld((int)position.X >> 4, (int)position.Y >> 4) && MachineMufflerTile.AnyMufflersNearby(position);

		//	SoundEngine.PlaySound(type, new Vector2((int)position.X, (int)position.Y), volumeScale: nearbyMuffler ? 0.1f : 1f);
		//}

		internal void PlaySound(SoundStyle type, Vector2 position){
			bool nearbyMuffler = WorldGen.InWorld((int)position.X >> 4, (int)position.Y >> 4) && MachineMufflerTile.AnyMufflersNearby(position);
			type.Volume = nearbyMuffler ? 0.1f : 1f;
			SoundEngine.PlaySound(type, new Vector2((int)position.X, (int)position.Y));
		}

		//internal void PlaySound(int type, int x = -1, int y = -1, int style = 1){
		//	bool nearbyMuffler = WorldGen.InWorld(x, y) && MachineMufflerTile.AnyMufflersNearby(new Vector2(x, y));

		//	SoundEngine.PlaySound(type, new Vector2(x, y), style, volumeScale: nearbyMuffler ? 0.1f : 1f);
		//}

		internal abstract int[] GetInputSlots();

		internal abstract int[] GetOutputSlots();

		/// <summary>
		/// Allows custom behaviour for when calling <seealso cref="MiscUtils.RetrieveItem(MachineEntity, int)"/>
		/// </summary>
		/// <param name="slot">The slot to get the item from</param>
		/// <param name="item">The retrieved item</param>
		/// <returns>Whether the behaviour was overwritten</returns>
		public virtual bool HijackRetrieveItem(int slot, out Item item){
			item = null;
			return false;
		}

		/// <summary>
		/// The function the entity should fall back to for detecing if an incoming item is valid when its parent UI state is not active.
		/// You can assume that <paramref name="slot"/> refers to an "input item" slot
		/// </summary>
		internal abstract bool CanInputItem(int slot, Item item);

		/// <summary>
		/// Allows for custom behaviour when trying to determine if an item can be input into the machine
		/// </summary>
		/// <param name="item">The incoming item</param>
		/// <param name="canInput">If the item can be input into the machine</param>
		/// <returns>Whether the behaviour was overwritten</returns>
		public virtual bool HijackCanBeInput(Item item, out bool canInput){
			canInput = false;
			return false;
		}

		public bool CanBeInput(Item item){
			if(HijackCanBeInput(item, out bool canInput))
				return canInput;

			int stack = item.stack;
			int[] inputSlots = GetInputSlots();

			foreach(int slot in inputSlots){
				if((ParentState?.GetSlot(slot).ValidItemFunc(item) ?? false) || CanInputItem(slot, item)){
					Item slotItem = this.RetrieveItem(slot);

					if(slotItem.IsAir)
						return true;
					else if(slotItem.type == item.type){
						if(slotItem.stack + stack <= slotItem.maxStack)
							return true;
						else
							stack -= slotItem.maxStack - slotItem.stack;
					}
				}
			}

			return stack <= 0;
		}

		/// <summary>
		/// Allows for custom behaviour when item networks try to register this machine as a "connected machine"
		/// </summary>
		/// <param name="canInteract">Whether this machine should interact with the network</param>
		/// <returns>Whether the behaviour was overwritten</returns>
		public virtual bool HijackCanBeInteractedWithItemNetworks(out bool canInteract, out bool canInput, out bool canOutput){
			canInteract = false;
			canInput = false;
			canOutput = false;
			return false;
		}

		/// <summary>
		/// Allows for custom behaviour when inserting items into this machine.
		/// The incoming item is guaranteed to be able to be inserted into the machine.
		/// </summary>
		/// <param name="incoming">The incoming item</param>
		/// <param name="sendBack">Whether to send it back to the network or not</param>
		/// <returns>Whether the behaviour was overwritten</returns>
		public virtual bool HijackInsertItem(ItemNetworkPath incoming, out bool sendBack){
			sendBack = false;
			return false;
		}

		public void InputItemFromNetwork(ItemNetworkPath incoming, out bool sendBack){
			Item data = ItemIO.Load(incoming.itemData);
			sendBack = true;

			if(!CanBeInput(data))
				return;

			if(HijackInsertItem(incoming, out sendBack))
				return;

			int[] inputSlots = GetInputSlots();

			foreach(int slot in inputSlots){
				if(!CanInputItem(slot, data))
					continue;

				Item slotItem = this.RetrieveItem(slot);
				if(slotItem.IsAir || slotItem.type == data.type){
					if(slotItem.IsAir){
						slots[slot] = data.Clone();

						if(ParentState?.Active ?? false)
							ParentState.LoadFromSlots(slots);

						slotItem = this.RetrieveItem(slot);
						slotItem.stack = 0;
					}

					if(slotItem.stack + data.stack > slotItem.maxStack){
						data.stack -= slotItem.maxStack - slotItem.stack;
						slotItem.stack = slotItem.maxStack;
					}else if(slotItem.stack < slotItem.maxStack){
						slotItem.stack += data.stack;
						data.stack = 0;

						sendBack = false;
						break;
					}else{
						sendBack = true;
						break;
					}
				}
			}

			if(sendBack)
				incoming.itemData = ItemIO.Save(data);
		}

		/// <summary>
		/// Allows for custom behaviour when getting the "items" from this machine
		/// </summary>
		/// <param name="inventory">The inventory to be used when trying to extract items</param>
		/// <returns>Whether the behaviour was overwritten</returns>
		public virtual bool HijackGetItemInventory(out Item[] inventory){
			inventory = null;
			return false;
		}

		/// <summary>
		/// Allows for custom behaviour when extracting items from this machine
		/// </summary>
		/// <param name="inventory">The inventory to extract from</param>
		/// <param name="slot">The slot in the inventory to extract from</param>
		/// <param name="toExtract">How many items are expected to be extracted</param>
		/// <param name="item">The extracted item</param>
		/// <returns>Whether the behaviour was overwritten</returns>
		public virtual bool HijackExtractItem(Item[] inventory, int slot, int toExtract, out Item item){
			item = null;
			return false;
		}

		/// <summary>
		/// Allows for custom behaviour when checking to see if an item should be extracted to be put in this machine's inventory
		/// </summary>
		/// <param name="incoming">The item that would be extracted</param>
		/// <param name="paths">The existing items in the item network</param>
		/// <param name="success">If the simulation resulted in the <paramref name="incoming"/> being inserted into thsi machine's inventory</param>
		/// <returns>Whether the behaviour was overwritten</returns>
		public virtual bool HijackSimulateInput(Item incoming, IEnumerable<ItemNetworkPath> paths, out bool success){
			success = false;
			return false;
		}

		/// <summary>
		/// Allows for logic to occur whenever an item is extracted from the machine
		/// </summary>
		/// <param name="extractInventory">The item inventory.  This array is either the result of getting the slots via <seealso cref="GetOutputSlots"/> or <seealso cref="HijackGetItemInventory(out Item[])"/></param>
		/// <param name="slot">The slot in <paramref name="extractInventory"/> that was extracted from</param>
		/// <param name="item">The item extracted from the slot</param>
		public virtual void OnItemExtracted(Item[] extractInventory, int slot, Item item){ }

		public override bool Equals(object obj)
			=> obj is MachineEntity entity && Position == entity.Position;

		public override int GetHashCode() => base.GetHashCode();

		public static bool operator ==(MachineEntity first, MachineEntity second)
			=> first?.Position == second?.Position;

		public static bool operator !=(MachineEntity first, MachineEntity second)
			=> first?.Position != second?.Position;
	}
}
