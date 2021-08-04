﻿using MagicStorage.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using TerraScience.API.CrossMod.MagicStorage;
using TerraScience.API.Networking;
using TerraScience.Content.TileEntities.Energy.Generators;
using TerraScience.Content.Tiles;
using TerraScience.Content.UI;
using TerraScience.Systems;
using TerraScience.Utilities;
using Terraria.Audio;

namespace TerraScience.Content.TileEntities{
	public abstract class MachineEntity : ModTileEntity{
		private readonly List<Item> slots = new List<Item>();

		public Item GetItem(int slot) => slots[slot];

		internal void ValidateSlots(int intendedLength){
			//Possible if the multitile was just placed
			if(slots.Count != intendedLength){
				slots.Clear();
				for(int i = 0; i < intendedLength; i++){
					Item item = new Item();
					slots.Add(item);
				}
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

		public sealed override bool ValidTile(int i, int j){
			Tile tile = Framing.GetTileSafely(i, j);
			return tile.IsActive && tile.type == MachineTile && tile.frameX == 0 && tile.frameY == 0;
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

		public sealed override TagCompound Save()
			=> new TagCompound(){
				["machineInfo"] = new TagCompound(){
					[nameof(ReactionSpeed)] = ReactionSpeed,
					[nameof(ReactionProgress)] = ReactionProgress,
					[nameof(ReactionInProgress)] = ReactionInProgress
				},
				["slots"] = new TagCompound(){
					//Lots of unnecessary data is saved, but that's fine due to the small amount of extra bytes used
					// TODO: refactor ItemIO.Save/ItemIO.Load to get rid of this extra info
					["items"] = slots.Count == 0 ? null : slots.Select(i => ItemIO.Save(i)).ToList()
				},
				["extra"] = ExtraSave()
			};

		public virtual TagCompound ExtraSave() => null;

		public sealed override void Load(TagCompound tag){
			TagCompound info = tag.GetCompound("machineInfo");
			ReactionSpeed = info.GetFloat(nameof(ReactionSpeed));
			ReactionProgress = info.GetFloat(nameof(ReactionProgress));
			ReactionInProgress = info.GetBool(nameof(ReactionInProgress));

			TagCompound tagSlots = tag.GetCompound("slots");
			List<TagCompound> items = tagSlots.GetList<TagCompound>("items") as List<TagCompound> ?? new List<TagCompound>();
			
			foreach(var c in items)
				slots.Add(ItemIO.Load(c));

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

		public void SaveSlots(){
			slots.Clear();

			for(int i = 0; i < ParentState.SlotsLength; i++){
				int type = ParentState.GetSlot(i).StoredItem.type;
				int stack = ParentState.GetSlot(i).StoredItem.stack;

				Item item = new Item();
				item.SetDefaults(type);
				item.stack = stack;

				slots.Add(item);
			}
		}

		public void LoadSlots(){
			ParentState.LoadToSlots(slots);
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

			writer.Write((short)slots.Count);
			for(int i = 0; i < slots.Count; i++)
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

			if(slots.Count < count){
				for(int c = slots.Count; c < count; c++)
					slots.Add(new Item());
			}else if(slots.Count > count)
				slots.RemoveRange(count, slots.Count - count);

			for(int i = 0; i < count; i++)
				ItemIO.Receive(slots[i], reader, readStack: true);

			ExtraNetReceive(reader);
		}

		/// <summary>
		/// Read extra data for net syncing, <seealso cref="Message.SyncTileEntity"/> and <seealso cref="Message.SyncMachineInfo"/>
		/// </summary>
		/// <param name="reader">The reader</param>
		public virtual void ExtraNetReceive(BinaryReader reader){ }

		internal static SoundEffectInstance PlayCustomSound(Vector2 position, string path){
			bool nearbyMuffler = WorldGen.InWorld((int)position.X >> 4, (int)position.Y >> 4) && MachineMufflerTile.AnyMufflersNearby(position);

			return TechMod.Instance.PlayCustomSound(position, path, volume: nearbyMuffler ? 0.1f : 1f);
			//return SoundEngine.PlaySound(SoundLoader.customSoundType, (int)position.X, (int)position.Y, TechMod.Instance.GetSoundSlot(Terraria.ModLoader.SoundType.Custom, $"Sounds/Custom/{path}"), volumeScale: nearbyMuffler ? 0.1f : 1f);
		}

		internal static void PlaySound(int type, Vector2 position, int style = 1){
			bool nearbyMuffler = WorldGen.InWorld((int)position.X >> 4, (int)position.Y >> 4) && MachineMufflerTile.AnyMufflersNearby(position);

			SoundEngine.PlaySound(type, (int)position.X, (int)position.Y, style, volumeScale: nearbyMuffler ? 0.1f : 1f);
		}

		internal static void PlaySound(LegacySoundStyle type, Vector2 position){
			bool nearbyMuffler = WorldGen.InWorld((int)position.X >> 4, (int)position.Y >> 4) && MachineMufflerTile.AnyMufflersNearby(position);

			SoundEngine.PlaySound(type.SoundId, (int)position.X, (int)position.Y, type.Style, volumeScale: nearbyMuffler ? 0.1f : 1f);
		}

		internal static void PlaySound(int type, int x = -1, int y = -1, int style = 1){
			bool nearbyMuffler = WorldGen.InWorld(x, y) && MachineMufflerTile.AnyMufflersNearby(new Vector2(x, y));

			SoundEngine.PlaySound(type, x, y, style, volumeScale: nearbyMuffler ? 0.1f : 1f);
		}

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
				Item slotItem;
				if((ParentState?.GetSlot(slot).ValidItemFunc(item) ?? false) || CanInputItem(slot, item)){
					slotItem = this.RetrieveItem(slot);

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
		public virtual bool HijackCanBeInteractedWithItemNetworks(out bool canInteract){
			canInteract = false;
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
							ParentState.LoadToSlots(slots);

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
		public virtual bool HijackSimulateInput(Item incoming, List<ItemNetworkPath> paths, out bool success){
			success = false;
			return false;
		}

		public override bool Equals(object obj)
			=> obj is MachineEntity entity && Position == entity.Position;

		public override int GetHashCode() => base.GetHashCode();

		public static bool operator ==(MachineEntity first, MachineEntity second)
			=> first?.Position == second?.Position;

		public static bool operator !=(MachineEntity first, MachineEntity second)
			=> first?.Position != second?.Position;
	}
}
