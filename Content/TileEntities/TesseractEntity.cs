using System;
using System.IO;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using TerraScience.API.UI;
using TerraScience.Content.ID;
using TerraScience.Content.TileEntities.Energy.Storage;
using TerraScience.Content.Tiles.Multitiles;
using TerraScience.Systems;
using TerraScience.Systems.Energy;
using TerraScience.Utilities;

namespace TerraScience.Content.TileEntities{
	public class TesseractEntity : Battery, IFluidMachine{
		public override int MachineTile => ModContent.TileType<Tesseract>();

		//5 slots for items, 4 slots each for adding/removing liquids and gases
		public override int SlotsCount => 5 + 4;

		//Duplicate entries are needed for proper I/O usage
		public FluidEntry[] FluidEntries{ get; set; } = new FluidEntry[]{
			//Fluid
			new FluidEntry(max: 500f, isInput: true, null),
			new FluidEntry(max: 500f, isInput: false, null)
		};

		public int FluidPlaceDelay{ get; set; }

		public override TerraFlux ImportRate => new TerraFlux(500f / 60f);

		public override TerraFlux ExportRate => new TerraFlux(500f / 60f);

		public override TerraFlux FluxCap => new TerraFlux(8000f);

		public override TerraFlux StoredFlux{
			get => TesseractNetwork.TryGetEntry(boundNet, out var entry) ? entry.flux : TerraFlux.Zero;
			set{
				if(TesseractNetwork.TryGetEntry(boundNet, out var entry))
					entry.flux = value;
			}
		}

		internal event Action OnNetworkChange;

		private string boundNet;
		internal string BoundNetwork{
			get => boundNet;
			set{
				if(boundNet != value){
					var old = boundNet;
					boundNet = value != null && TesseractNetwork.TryGetEntry(value, out _) ? value : null;

					if(boundNet != old)
						OnNetworkChange?.Invoke();
				}
			}
		}

		internal override bool SetItemsToParentUIWhenClosing => false;

		public override TagCompound ExtraSave()
			=> new TagCompound(){
				["boundNetwork"] = boundNet
			};

		public override void ExtraLoad(TagCompound tag){
			boundNet = tag.GetString("boundNetwork");
		}

		public override void ExtraNetSend(BinaryWriter writer){
			writer.Write(boundNet);
		}

		public override void ExtraNetReceive(BinaryReader reader){
			BoundNetwork = reader.ReadString();
		}

		public override TerraFlux GetPowerGeneration(int ticks) => TerraFlux.Zero;

		public override void ReactionComplete(){ }

		public void TryExportFluid(Point16 pumpPos){
			if(!TesseractNetwork.TryGetEntry(boundNet, out var entry))
				return;

			this.TryExportFluids(pumpPos, 1);

			entry.fluid.id = FluidEntries[0].id = FluidEntries[1].id;
			entry.fluid.current = FluidEntries[0].current = FluidEntries[1].current;
			entry.fluid.max = FluidEntries[0].max = FluidEntries[1].max;
		}

		public void TryImportFluid(Point16 pipePos){
			if(!TesseractNetwork.TryGetEntry(boundNet, out var entry))
				return;

			this.TryImportFluids(pipePos, 0);

			entry.fluid.id = FluidEntries[1].id = FluidEntries[0].id;
			entry.fluid.current = FluidEntries[1].current = FluidEntries[0].current;
			entry.fluid.max = FluidEntries[1].max = FluidEntries[0].max;
		}

		public override void PreUpdateReaction(){
			if(boundNet != null && TesseractNetwork.TryGetEntry(boundNet, out var entry)){
				//Set the stuff in the entity to match the network entry
				for(int i = 0; i < 5; i++)
					GetItemSlotRef(i) = entry.items[i];

				for(int i = 0; i < FluidEntries.Length; i++){
					FluidEntries[i].id = entry.fluid.id;
					FluidEntries[i].current = entry.fluid.current;
					FluidEntries[i].max = entry.fluid.max;
				}
			}else{
				for(int i = 0; i < SlotsCount; i++)
					GetItemSlotRef(i) = new Item();

				for(int i = 0; i < FluidEntries.Length; i++){
					FluidEntries[i].id = MachineFluidID.None;
					FluidEntries[i].current = 0;
					FluidEntries[i].max = 500f;
				}
			}
		}

		public override bool UpdateReaction()
			=> false;

		internal override bool CanInputItem(int slot, Item item)
			=> TesseractNetwork.TryGetEntry(boundNet, out _);

		internal override int[] GetInputSlots()
			=> new int[]{ 0, 1, 2, 3, 4 };

		internal override int[] GetOutputSlots()
			=> new int[]{ 0, 1, 2, 3, 4 };

		public override bool HijackGetItemInventory(out Item[] inventory){
			if(!TesseractNetwork.TryGetEntry(boundNet, out var entry)){
				inventory = null;
				return true;
			}

			inventory = entry.items;
			return true;
		}

		public override bool HijackRetrieveItem(int slot, out Item item){
			if(slot >= 5){
				item = null;
				return false;
			}

			if(!TesseractNetwork.TryGetEntry(boundNet, out var entry)){
				item = null;
				return false;
			}

			item = entry.items[slot];
			return true;
		}

		public override bool HijackCanBeInput(Item item, out bool canInput){
			canInput = false;
			if(!TesseractNetwork.TryGetEntry(BoundNetwork, out var entry))
				return true;

			int stack = item.stack;

			for(int slot = 0; slot < entry.items.Length; slot++){
				Item slotItem = entry.items[slot];

				if(slotItem.IsAir){
					canInput = true;
					return true;
				}else if(slotItem.type == item.type){
					if(slotItem.stack + stack <= slotItem.maxStack){
						canInput = true;
						return true;
					}else
						stack -= slotItem.maxStack - slotItem.stack;
				}
			}

			return true;
		}

		public override bool HijackInsertItem(ItemNetworkPath incoming, out bool sendBack){
			Item data = ItemIO.Load(incoming.itemData);
			sendBack = true;

			if(!TesseractNetwork.TryGetEntry(BoundNetwork, out var entry))
				return true;

			for(int slot = 0; slot < entry.items.Length; slot++){
				Item slotItem = entry.items[slot];

				if(slotItem.IsAir || slotItem.type == data.type){
					if(slotItem.IsAir){
						entry.items[slot] = data.Clone();
						ParentState?.GetSlot(slot).SetItem(slotItem);
					}

					if(slotItem.stack + data.stack > slotItem.maxStack){
						data.stack -= slotItem.maxStack - slotItem.stack;
						slotItem.stack = slotItem.maxStack;

						ParentState?.GetSlot(slot).SetItem(slotItem);
					}else if(slotItem.stack < slotItem.maxStack){
						slotItem.stack += data.stack;
						data.stack = 0;

						sendBack = false;
						ParentState?.GetSlot(slot).SetItem(slotItem);
						break;
					}else{
						sendBack = true;
						break;
					}
				}
			}

			if(sendBack)
				incoming.itemData = ItemIO.Save(data);

			return true;
		}

		public override bool HijackCanBeInteractedWithItemNetworks(out bool canInteract, out bool canInput, out bool canOutput)
			=> canInteract = canInput = canOutput = TesseractNetwork.TryGetEntry(boundNet, out _);

		public override void OnItemExtracted(Item[] extractInventory, int slot, Item item){
			//Sets "UIItemSlot.storedItem", which affects what item is drawn
			ParentState?.GetSlot(slot).SetItem(extractInventory[slot].Clone());
		}
	}
}
