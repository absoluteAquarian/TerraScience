using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using TerraScience.Content.Tiles.Multitiles;
using TerraScience.Systems;
using TerraScience.Utilities;

namespace TerraScience.Content.TileEntities{
	public class ItemCacheEntity : MachineEntity{
		public override int MachineTile => ModContent.TileType<ItemCache>();

		public override int SlotsCount => 3;

		public bool locked;
		internal int lockItemType;

		//Normally I'd have to worry about making sure the parent UI and this entity sync up, but since the data's only being held in
		//  this entity, there shouldn't be any issues, hopefully
		internal List<Item> items;
		public int ItemIndex{ get; private set; }

		/// <summary>
		/// How many stacks' worth of items can be stored in this item cache
		/// </summary>
		public virtual int MaxStacks => 10;

		public int GetTotalItems(){
			int ret = 0;

			foreach(var item in items)
				ret += item.stack;
			
			return ret;
		}

		public int GetPerStackMax(){
			Item top = this.RetrieveItem(-1);

			if(lockItemType > 0){
				Item item = new Item();
				item.netDefaults(lockItemType);

				return item.maxStack;
			}

			return top.IsAir ? 0 : top.maxStack;
		}

		public override TagCompound ExtraSave(){
			TagCompound tag = base.ExtraSave() ?? new TagCompound();
			tag.Add("locked", locked);
			tag.Add("lockItem", SaveType(lockItemType));
			tag.Add("items", items.Count == 0 ? null : items.Select(i => ItemIO.Save(i)).ToList());
			tag.Add("idx", ItemIndex);
			return tag;
		}

		private TagCompound SaveType(int type){
			ModItem mItem;
			return type < ItemID.Count
				? new TagCompound(){
					["mod"] = "Terraria",
					["id"] = type
				}
				: new TagCompound(){
					["mod"] = (mItem = ModContent.GetModItem(type)).mod.Name,
					["name"] = mItem.Name
				};
		}

		public override void ExtraLoad(TagCompound tag){
			base.ExtraLoad(tag);

			locked = tag.GetBool("locked");

			lockItemType = LoadType(tag.GetCompound("lockItem"));

			items = tag.GetList<TagCompound>("items") is List<TagCompound> list
				? list.Select(t => ItemIO.Load(t)).ToList()
				: new List<Item>(){ new Item() };

			ItemIndex = tag.GetInt("idx");
		}

		private int LoadType(TagCompound tag){
			var modString = tag.GetString("mod");
			if(modString == "Terraria")
				return tag.GetInt("id");

			return ModLoader.GetMod(modString).ItemType(tag.GetString("name"));
		}

		public override void ExtraNetReceive(BinaryReader reader){
			locked = reader.ReadBoolean();

			lockItemType = reader.ReadUInt16();

			ItemIndex = reader.ReadUInt16();

			ushort count = reader.ReadUInt16();

			if(items.Count < count){
				//Add dummy items
				for(int c = items.Count; c < count; c++)
					items.Add(new Item());
			}else{
				//Remove extra items
				items.RemoveRange(count, items.Count - count);
			}

			for(int i = 0; i < count; i++)
				ItemIO.Receive(items[i], reader, readStack: true);
		}

		public override void ExtraNetSend(BinaryWriter writer){
			writer.Write(locked);

			writer.Write((ushort)lockItemType);

			writer.Write((ushort)ItemIndex);

			writer.Write((ushort)items.Count);
			for(int i = 0; i < items.Count; i++)
				ItemIO.Send(items[i], writer, writeStack: true);
		}

		//These two methods have their use overwritten by the Hijack hooks
		internal override int[] GetInputSlots() => new int[0];

		internal override int[] GetOutputSlots() => new int[0];

		public override void PreUpdateReaction(){
			if(items is null)
				items = new List<Item>(){ new Item() };
		}

		public override bool UpdateReaction() => false;

		public override void ReactionComplete(){ }

		internal override bool CanInputItem(int slot, Item item){
			//Item must be able to be input
			HijackCanBeInput(item, out bool canInput);
			return canInput;
		}

		public override bool HijackCanBeInput(Item item, out bool canInput){
			//Gotta define the applicability here
			canInput = false;
			if(locked && item.type != lockItemType){
				//Incoming item must be the same type as the lock item
				return true;
			}

			//All that matters is that the item has the same type and that there's still room for at least part of the stack
			//If there's only room for part of the stack, the item insertion code will handle that
			Item existing = this.RetrieveItem(-1);

			canInput = (ItemIndex == 0 && existing.IsAir)
				|| (ItemIndex < MaxStacks - 1 && item.type == existing.type)
				|| (ItemIndex == MaxStacks - 1 && existing.stack < existing.maxStack && item.type == existing.type);

			return true;
		}

		public override bool HijackRetrieveItem(int slot, out Item item){
			if(slot == -1){
				//A slot of -1 means to access the internal list
				item = items[ItemIndex];
				return true;
			}

			item = null;
			return false;
		}

		public override bool HijackInsertItem(ItemNetworkPath incoming, out bool sendBack){
			Item data = ItemIO.Load(incoming.itemData);
			sendBack = false;

			//If the cache is on the last slot, fill up the item as much as possible and send back the rest, if applicable
			//If it isn't, then fill up the current item and move to the next one if need be
			Item existing = this.RetrieveItem(-1);

			//In case the existing item is empty, make sure it has the correct type and max stack
			if(existing.IsAir){
				existing.netDefaults(data.type);
				existing.stack = 0;
			}

			if(ItemIndex == MaxStacks - 1){
				if(data.stack + existing.stack > existing.maxStack){
					//Remove as much as possible from the incoming item and send the rest back
					data.stack -= existing.maxStack - existing.stack;
					existing.stack = existing.maxStack;

					incoming.itemData = ItemIO.Save(data);
					
					sendBack = true;
				}else if(existing.stack < existing.maxStack){
					//Remove the entire stack since there's room
					existing.stack += data.stack;
					data.stack = 0;
				}else
					sendBack = true;
			}else{
				if(data.stack + existing.stack > existing.maxStack){
					do{
						if(data.stack + existing.stack > existing.maxStack){
							data.stack -= existing.maxStack - existing.stack;
							existing.stack = existing.maxStack;

							ItemIndex++;

							//Add a new item
							items.Add(new Item());
						}else if(existing.stack < existing.maxStack){
							existing.stack += data.stack;
							data.stack = 0;
						}else{
							sendBack = true;
							return true;
						}

						existing = this.RetrieveItem(-1);

						if(existing.IsAir){
							existing.netDefaults(data.type);
							existing.stack = 0;
						}
						//Loop until we either run out of incoming items or we've reached the last item in the cache
					}while(data.stack > 0 && ItemIndex < MaxStacks - 1);

					if(data.stack + existing.stack > existing.maxStack){
						//Remove as much as possible from the incoming item and send the rest back
						data.stack -= existing.maxStack - existing.stack;
						existing.stack = existing.maxStack;

						incoming.itemData = ItemIO.Save(data);
					
						sendBack = true;
					}else if(existing.stack < existing.maxStack){
						//Remove the entire stack since there's room
						existing.stack += data.stack;
						data.stack = 0;
					}else
						sendBack = true;
				}else if(existing.stack < existing.maxStack){
					//Remove the entire stack since there's room
					existing.stack += data.stack;
					data.stack = 0;
				}else
					sendBack = true;
			}

			return true;
		}

		public override bool HijackCanBeInteractedWithItemNetworks(out bool canInteract, out bool canInput, out bool canOutput)
			=> canInteract = canInput = canOutput = true;

		public override bool HijackGetItemInventory(out Item[] inventory){
			inventory = items.ToArray();
			return true;
		}

		public override bool HijackExtractItem(Item[] inventory, int slot, int toExtract, out Item item){
			//Failsafe in case items were extracted before they were added
			if(items.Count == 0)
				items.Add(new Item());
			item = null;

			Item current = this.RetrieveItem(-1);
			if(current.IsAir)
				return true;

			int extracted = 0;
			int type = current.netID;

			if(current.stack < toExtract){
				do{
					extracted += current.stack;
					toExtract -= current.stack;
					current.stack = 0;

					if(ItemIndex > 0){
						//Remove the item since it's not needed anymore
						items.RemoveAt(ItemIndex);

						ItemIndex--;

						current = this.RetrieveItem(-1);
					}else
						current.type = locked ? lockItemType : ItemID.None;
				}while(current.stack > 0 && toExtract > 0);
			}else{
				//Extraction could happen from just one item
				extracted = toExtract;
				current.stack -= toExtract;

				if(current.stack == 0){
					//Only remove the item if it's not the last one
					if(ItemIndex > 0){
						items.RemoveAt(ItemIndex);

						ItemIndex--;
					}else
						current.type = locked ? lockItemType : ItemID.None;
				}
			}

			item = new Item();
			item.netDefaults(type);
			item.stack = extracted;

			return true;
		}

		public override bool HijackSimulateInput(Item incoming, IEnumerable<ItemNetworkPath> paths, out bool success){
			success = true;
			
			//Make a clone of the current items, then revert it back once the simulation is complete
			List<Item> clone = new List<Item>(){ new Item() };
			for(int i = 0; i < items.Count; i++)
				clone.Add(items[i].Clone());

			List<Item> old = items;
			items = clone;

			int oldIdx = ItemIndex;
			foreach(var path in paths){
				var item = ItemIO.Load(path.itemData);

				success &= this.CanBeInput(item);
				
				if(success)
					this.InputItemFromNetwork(path, out _);
				else{
					//Abort, abort!
					items = old;
					ItemIndex = oldIdx;
					return true;
				}
			}

			success &= this.CanBeInput(incoming);
			
			//Revert the machine to its state before the simulation
			items = old;
			ItemIndex = oldIdx;
			return true;
		}
	}
}
