using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using TerraScience.API.Interfaces;
using TerraScience.Content.TileEntities;
using TerraScience.Content.TileEntities.Energy;
using TerraScience.Content.TileEntities.Energy.Generators;
using TerraScience.Content.Tiles;
using TerraScience.Content.Tiles.Multitiles;
using TerraScience.Systems.Energy;
using TerraScience.Systems.Pathfinding;
using TerraScience.Systems.Pipes;
using TerraScience.Utilities;

namespace TerraScience.Systems {
	public static class NetworkCollection{
		internal static List<WireNetwork> wireNetworks;
		internal static List<ItemNetwork> itemNetworks;
		internal static List<FluidNetwork> fluidNetworks;

		internal delegate INetworkable typeCtor(Point16 location, INetwork network);

		internal static Dictionary<Type, typeCtor> networkTypeCtors;

		public static INetwork CombineNetworks<T, E>(params T[] networks) where T : INetwork, INetwork<E>, new() where E : struct, INetworkable, INetworkable<E>{
			if(networks.Length == 0)
				throw new Exception("Error -- length zero, cannot combine no networks");

			if(networks.Length == 1)
				return networks[0];

			INetwork<E> network = networks[0];

			var ctor = networkTypeCtors[typeof(E)];

			for(int i = 1; i < networks.Length; i++){
				foreach(var hash in networks[i].Hash){
					E newHash = (E)ctor(hash.Position, network);
					network.AddEntry(newHash);
				}

				foreach(var machine in networks[i].ConnectedMachines)
					network.AddMachine(machine);
			}

			return network;
		}

		public static void EnsureNetworkIsInitialized(){
			if(wireNetworks is null)
				wireNetworks = new List<WireNetwork>();
			if(itemNetworks is null)
				itemNetworks = new List<ItemNetwork>();
			if(fluidNetworks is null)
				fluidNetworks = new List<FluidNetwork>();

			if(MachineMufflerTile.mufflers is null)
				MachineMufflerTile.mufflers = new List<Point16>();
		}

		public static void ResetNetworkInfo(){
			foreach(var network in wireNetworks)
				network.totalExportedFlux = new TerraFlux(0f);
		}

		private static void InternalCleanup<TNet, TEntry>(List<TNet> list) where TNet : class, INetwork, INetwork<TEntry>, new() where TEntry : struct, INetworkable, INetworkable<TEntry>{
			if(list is null)
				return;

			for(int i = 0; i < list.Count; i++){
				if(!(list[i] is TNet net)){
					//Remove null networks
					list.RemoveAt(i);
					i--;
				}else{
					//Remove any wires not actually in the network
					net.Cleanup();

					if(net.GetEntries().Count == 0) {
						//Remove empty networks
						list.RemoveAt(i);
						i--;
					}
				}
			}

			if(list.Count < 2)
				return;

			//If two networks overlap BUT NOT ON A JUNCTION, connect them together
			for(int i = 0; i < list.Count - 1; i++){
				var entries = list[i].GetEntries();

				for(int w = 0; w < entries.Count; w++){
					for(int c = i + 1; c < list.Count; c++){
						if(list[c].HasEntryAt(entries[w].Position) && !(ModContent.GetModTile(Framing.GetTileSafely(entries[w].Position).type) is TransportJunction)){
							TNet combined = (TNet)CombineNetworks<TNet, TEntry>(list[i], list[c]);
							list.Add(combined);

							list.RemoveAt(i);
							list.RemoveAt(c - 1);

							i--;

							goto forceNextCheck;
						}
					}
				}

forceNextCheck: ;
			}
		}

		public static void CleanupNetworks(){
			InternalCleanup<WireNetwork, TFWire>(wireNetworks);
			InternalCleanup<ItemNetwork, ItemPipe>(itemNetworks);
			InternalCleanup<FluidNetwork, FluidPipe>(fluidNetworks);
		}

		public static void Unload(){
			wireNetworks = null;
			itemNetworks = null;
			fluidNetworks = null;

			//Setting this to null here causes problems... for whatever reason
			//MachineMufflerTile.mufflers = null;
		}

		private static void LoadNetwork<TNet, TEntry>(TagCompound tag, string entry, List<TNet> networks) where TNet : class, INetwork, INetwork<TEntry>, new() where TEntry : struct, INetworkable, INetworkable<TEntry>{
			if(tag.GetList<Point16>(entry) is List<Point16> list){
				var ctor = networkTypeCtors[typeof(TEntry)];

				for(int i = 0; i < list.Count; i++){
					Point16 point = list[i];
					TNet network = new TNet();

					network.AddEntry((TEntry)ctor(point, network));

					network.RefreshConnections(ignoreCheckLocation);

					networks.Add(network);
				}
			}
		}

		public static void Load(TagCompound tag){
			EnsureNetworkIsInitialized();

			LoadNetwork<WireNetwork, TFWire>(tag, "wires", wireNetworks);
			LoadNetwork<ItemNetwork, ItemPipe>(tag, "items", itemNetworks);
			LoadNetwork<FluidNetwork, FluidPipe>(tag, "fluids", fluidNetworks);

			if(tag.GetList<Point16>("junctionPositions") is List<Point16> junctionPoints && tag.GetList<string>("junctionTypes") is List<string> junctionConnections){
				if(junctionPoints.Count == junctionConnections.Count){
					for(int i = 0; i < junctionPoints.Count; i++){
						Point16 pos = junctionPoints[i];
						string enumName = junctionConnections[i];

						var tile = Framing.GetTileSafely(pos);
						if(tile.type == ModContent.TileType<TransportJunction>() && Enum.TryParse(enumName, out JunctionMerge merge))
							tile.frameX = (short)(JunctionMergeable.FindMergeIndex(merge) * 18);
					}
				}else
					TechMod.Instance.Logger.Error("Network data was modified by an external program (entries: \"junctionPositions\", \"junctionTypes\")");
			}

			if(tag.GetCompound("networkData") is TagCompound data){
				if(data.GetList<TagCompound>("wires") is List<TagCompound> wireNetData){
					if(wireNetworks.Count == wireNetData.Count){
						for(int i = 0; i < wireNetData.Count; i++)
							wireNetworks[i].Load(wireNetData[i]);
					}else
						TechMod.Instance.Logger.Error("Network data was modified by an external program (entry: \"networkData.wires\")");
				}

				if(data.GetList<TagCompound>("items") is List<TagCompound> itemNetData){
					if(itemNetworks.Count == itemNetData.Count){
						for(int i = 0; i < itemNetData.Count; i++)
							itemNetworks[i].Load(itemNetData[i]);
					}else
						TechMod.Instance.Logger.Error("Network data was modified by an external program (entry: \"networkData.items\")");
				}

				if(data.GetList<TagCompound>("fluids") is List<TagCompound> fluidNetData){
					if(fluidNetworks.Count == fluidNetData.Count){
						for(int i = 0; i < fluidNetData.Count; i++)
							fluidNetworks[i].Load(fluidNetData[i]);
					}else
						TechMod.Instance.Logger.Error("Network data was modified by an external program (entry: \"networkData.fluids\")");
				}
			}

			CleanupNetworks();
		}

		//Save the location of the wires and connected machines
		//Also, save the location and merge types for all junctions.  For some reason, they default to a faulty frameX/frameY on world load
		//Pumps also need to be saved
		public static TagCompound Save(){
			var hash = SaveJunctions();

			TagCompound tag = new TagCompound(){
				["wires"] = wireNetworks.Count == 0 ? null : wireNetworks.Select(net => net.GetEntries()[0].Position).ToList(),
				["items"] = itemNetworks.Count == 0 ? null : itemNetworks.Select(net => net.GetEntries()[0].Position).ToList(),
				["fluids"] = fluidNetworks.Count == 0 ? null : fluidNetworks.Select(net => net.GetEntries()[0].Position).ToList(),
				["junctionPositions"] = hash.Count == 0 ? null : hash.ToList(),
				["junctionTypes"] = hash.Count == 0 ? null : hash.Select(p
					=> JunctionMergeable.mergeTypes[Framing.GetTileSafely(p).frameX / 18].EnumName()
						?? throw new Exception($"TerraScience internal error -- Junction frame was invalid (Merge ID: {JunctionMergeable.mergeTypes[Framing.GetTileSafely(p).frameX / 18]})")).ToList(),
				["networkData"] = new TagCompound(){
					["wires"] = wireNetworks.Count == 0 ? null : wireNetworks.Select(net => net.Save()).ToList(),
					["items"] = itemNetworks.Count == 0 ? null : itemNetworks.Select(net => net.Save()).ToList(),
					["fluids"] = fluidNetworks.Count == 0 ? null : fluidNetworks.Select(net => net.Save()).ToList()
				}
			};

			//Only null the networks if the player is leaving the world... Otherwise the automatic saving will cause bad things to happen
			if(Main.gameMenu)
				Unload();

			return tag;
		}

		private static HashSet<Point16> SaveJunctions(){
			HashSet<Point16> hash = new HashSet<Point16>();

			void ModifyJunctionHash<TNet, TEntry>(List<TNet> nets) where TNet : class, INetwork, INetwork<TEntry>, new() where TEntry : struct, INetworkable, INetworkable<TEntry>{
				foreach(var network in nets){
					var entries = network.GetEntries();
					if(entries.Count == 0)
						continue;

					foreach(var entry in entries)
						if(ModContent.GetModTile(Framing.GetTileSafely(entry.Position).type) is TransportJunction)
							hash.Add(entry.Position);
				}
			}

			ModifyJunctionHash<WireNetwork, TFWire>(wireNetworks);
			ModifyJunctionHash<ItemNetwork, ItemPipe>(itemNetworks);
			ModifyJunctionHash<FluidNetwork, FluidPipe>(fluidNetworks);

			return hash;
		}

		internal static readonly Point16 ignoreCheckLocation = new Point16(-1, -1);

		private static readonly Stopwatch networkWatch = new Stopwatch();
		private static readonly Stopwatch networkWatch2 = new Stopwatch();

		public static double ItemNetworkPumpUpdateTime{ get; private set; }

		public static double ItemNetworkMovingItemsUpdateTime{ get; private set; }

		public static double FluidNetworkUpdateTime{ get; private set; }

		public static void UpdateItemNetworks(){
			bool restartTimers = true;
			bool updateTimers = Main.GameUpdateCount % 10 == 0;

			foreach(var network in itemNetworks){
				if(updateTimers){
					if(restartTimers)
						networkWatch.Restart();
					else
						networkWatch.Start();
				}

				foreach(var timer in network.pumpTimers){
					timer.Value.value--;

					if(timer.Value.value < 0){
						timer.Value.value = 18;

						//Export an item
						Tile tile = Framing.GetTileSafely(timer.Key);
						ModTile mTile = ModContent.GetModTile(tile.type);
						if(!(mTile is ItemPumpTile pump))
							continue;

						//Check MagicStorage compatability
						Item[] inventory;
						MachineEntity entity;
						if((entity = pump.GetConnectedMachine(timer.Key)) != null){
							var outputs = entity.GetOutputSlots();

							//No output slots?  Can't pump items out of this...
							bool hijacked;
							if(!(hijacked = entity.HijackGetItemInventory(out inventory)) && outputs.Length == 0)
								continue;

							//If the inventory getting was hijacked, but the inventory is null, then abort immediately
							if(hijacked && inventory is null)
								continue;

							if(!hijacked){
								inventory = new Item[outputs.Length];

								for(int i = 0; i < inventory.Length; i++)
									inventory[i] = entity.RetrieveItem(outputs[i]);
							}
						}else if(pump.GetConnectedChest(timer.Key) is Chest chest)
							inventory = chest.item;
						else
							continue;

						if(!(network.pumpPathsToMachines.TryGetValue(timer.Key, out List<ItemPathResult> path) && CheckItemInsertion(network, inventory, timer.Key, pump, path, inputMachine: entity))){
							if(network.pumpPathsToChests.TryGetValue(timer.Key, out path))
								CheckItemInsertion(network, inventory, timer.Key, pump, path, inputMachine: null);
						}
					}
				}

				if(updateTimers){
					networkWatch.Stop();

					if(restartTimers)
						networkWatch2.Restart();
					else
						networkWatch2.Start();
				}

				for(int i = 0; i < network.paths.Count; i++){
					var path = network.paths[i];
					path.Update();

					if(path.removed)
						i--;
				}

				if(updateTimers)
					networkWatch2.Stop();

				restartTimers = false;
			}

			if(updateTimers){
				ItemNetworkPumpUpdateTime = networkWatch.Elapsed.TotalSeconds;
				ItemNetworkMovingItemsUpdateTime = networkWatch2.Elapsed.TotalSeconds;
			}
		}

		private static bool CheckItemInsertion(ItemNetwork network, Item[] extractInventory, Point16 pumpTile, ItemPumpTile pump, List<ItemPathResult> pathfinding, MachineEntity inputMachine){
			int stack = pump.StackPerExtraction;

			//Loop over the exporting inventory
			//Find the first valid item can be input into one of the paths
			for(int i = 0; i < extractInventory.Length; i++){
				Item original = extractInventory[i];

				if(original.IsAir)
					continue;

				Item item = extractInventory[i].Clone();

				//Prevent extracting more items than exist in a stack
				item.stack = Math.Min(stack, item.maxStack);

				for(int m = 0; m < pathfinding.Count; m++){
					var list = pathfinding[m].list;

					if(list.Count <= 0)
						continue;

					Point16 target = list[list.Count - 1];

					MachineEntity machine = null;
					Chest targetChest = null;
					if((machine = ItemNetworkPath.FindConnectedMachine(network, target, item)) is null && (targetChest = ItemNetworkPath.FindConnectedChest(network, target, item, out _)) is null)
						continue;

					//"item.stack" is modified here, hence the need to clone it earlier
					var usepaths = network.paths.Where(p => p.wander || p.finalTarget == target || (p?.Path?.Count > 0 && p.Path.Last() == target));
					bool successful = machine != null
						? ItemNetworkPath.SimulateMachineInput(machine, item, usepaths)
						: ItemNetworkPath.SimulateChestInput(targetChest, item, usepaths);

					//If it was successful, do the things
					// TODO: move the item extraction code to MachineEntity
					if(successful){
						int stackToExtract = item.stack;
						ItemNetworkPath newPath;
						if(inputMachine != null && inputMachine.HijackExtractItem(extractInventory, i, stackToExtract, out Item extracted)){
							//Hijacked, but couldn't extract anything
							if(extracted is null)
								return false;

							newPath = ItemNetworkPath.CreateObject(extracted, network, pumpTile, pathOverride: list);
						}else{
							original.stack -= stackToExtract;

							newPath = ItemNetworkPath.CreateObject(item, network, pumpTile, pathOverride: list);
						}

						if(newPath?.Path != null){
							newPath.moveDir = -(pump.GetBackwardsOffset(pumpTile) - pumpTile).ToVector2();
							network.paths.Add(newPath);
						}

						return true;
					}
				}
			}

			return false;
		}

		public static void UpdateFluidNetworks(){
			if(Main.GameUpdateCount % 10 == 0)
				networkWatch.Restart();

			foreach(var network in fluidNetworks){
				foreach(var timer in network.pumpTimers){
					timer.Value.value--;

					if(timer.Value.value < 0){
						timer.Value.value = 34;

						Tile tile = Framing.GetTileSafely(timer.Key);
						ModTile mTile = ModContent.GetModTile(tile.type);
						if(!(mTile is FluidPumpTile pump))
							continue;

						if(pump.GetConnectedMachine(timer.Key) is MachineEntity entity)
							(entity as IFluidMachine)?.TryExportFluid(timer.Key);
					}
				}

				foreach(var pipe in network.pipesConnectedToMachines){
					if(!(ModContent.GetModTile(Framing.GetTileSafely(pipe).type) is FluidTransportTile))
						continue;

					if(TileEntityUtils.TryFindMachineEntity(pipe + new Point16(0, -1), out MachineEntity entity) && network.ConnectedMachines.Contains(entity))
						(entity as IFluidMachine)?.TryImportFluid(pipe);

					if(TileEntityUtils.TryFindMachineEntity(pipe + new Point16(-1, 0), out entity) && network.ConnectedMachines.Contains(entity))
						(entity as IFluidMachine)?.TryImportFluid(pipe);

					if(TileEntityUtils.TryFindMachineEntity(pipe + new Point16(1, 0), out entity) && network.ConnectedMachines.Contains(entity))
						(entity as IFluidMachine)?.TryImportFluid(pipe);

					if(TileEntityUtils.TryFindMachineEntity(pipe + new Point16(0, 1), out entity) && network.ConnectedMachines.Contains(entity))
						(entity as IFluidMachine)?.TryImportFluid(pipe);
				}
			}

			if(Main.GameUpdateCount % 10 == 0){
				networkWatch.Stop();

				FluidNetworkUpdateTime = networkWatch.Elapsed.TotalSeconds;
			}
		}

		public static void OnWirePlace(Point16 location)
			=> OnThingPlace<WireNetwork, TFWire>(location, wireNetworks);

		public static void OnItemPipePlace(Point16 location)
			=> OnThingPlace<ItemNetwork, ItemPipe>(location, itemNetworks);

		public static void OnFluidPipePlace(Point16 location)
			=> OnThingPlace<FluidNetwork, FluidPipe>(location, fluidNetworks);

		private static void OnThingPlace<TNet, TEntry>(Point16 location, List<TNet> networks) where TNet : class, INetwork, INetwork<TEntry>, new() where TEntry : struct, INetworkable, INetworkable<TEntry>{
			//Check if a network is adjacent to this entry
			//If there is one, connect the entry to the network
			//If the entry would be connected to multiple of them, then combine the networks

			Point16 up = new Point16(location.X, location.Y - 1);
			Point16 left = new Point16(location.X - 1, location.Y);
			Point16 right = new Point16(location.X + 1, location.Y);
			Point16 down = new Point16(location.X, location.Y + 1);

			Tile center = Framing.GetTileSafely(location.X, location.Y);
			bool centerIsJunction = center.type == ModContent.TileType<TransportJunction>();
			JunctionMerge merge = !centerIsJunction ? JunctionMerge.None : JunctionMergeable.mergeTypes[center.frameX / 18];
			bool junctionHasWires = (merge & JunctionMerge.Wires_All) != 0;
			bool junctionHasItems = (merge & JunctionMerge.Items_All) != 0;
			bool junctionHasFluids = (merge & JunctionMerge.Fluids_All) != 0;

			TagCompound upData = null, leftData = null, rightData = null, downData = null;

			//Find the networks to connect
			List<TNet> networksToConnect = new List<TNet>();
			for(int i = 0; i < networks.Count; i++){
				TNet network = networks[i];

				network.GetMergeInfo(out JunctionMerge leftRight, out JunctionMerge upDown, out _);

				bool hasUp = network.HasEntryAt(up) && network.CanCombine(location, up - location);
				bool hasLeft = network.HasEntryAt(left) && network.CanCombine(location, left - location);
				bool hasRight = network.HasEntryAt(right) && network.CanCombine(location, right - location);
				bool hasDown = network.HasEntryAt(down) && network.CanCombine(location, down - location);
				bool anyEntryInAnyDirection = hasUp || hasLeft || hasRight || hasDown;

				if(hasUp && upData is null)
					upData = network.CombineSave();
				if(hasLeft && leftData is null)
					leftData = network.CombineSave();
				if(hasRight && rightData is null)
					rightData = network.CombineSave();
				if(hasDown && downData is null)
					downData = network.CombineSave();

				bool normalEntryValid = !centerIsJunction && anyEntryInAnyDirection;
				
				bool junctionUpDownIsValid = (merge & upDown) != 0 && (hasUp || hasDown);
				bool junctionLeftRightIsValid = (merge & leftRight) != 0 && (hasLeft || hasRight);

				bool junctionValid = centerIsJunction && (junctionHasWires || junctionHasItems || junctionHasFluids) && (junctionUpDownIsValid || junctionLeftRightIsValid);

				if(normalEntryValid || junctionValid){
					networksToConnect.Add(network);
					networks.RemoveAt(i);
					i--;
				}
			}

			//Then combine them if necessary (or make a new one)
			TNet newNetwork;
			if(networksToConnect.Count == 0)
				newNetwork = new TNet();
			else
				newNetwork = (TNet)CombineNetworks<TNet, TEntry>(networksToConnect.ToArray());

			//Then create the entry and add it
			TEntry entry = (TEntry)networkTypeCtors[typeof(TEntry)](location, newNetwork);
			newNetwork.AddEntry(entry);
			newNetwork.RefreshConnections(ignoreCheckLocation);

			newNetwork.LoadCombinedData(upData, leftData, rightData, downData);

			networks.Add(newNetwork);
		}

		public static void OnWireKill(Point16 location)
			=> OnThingKill<WireNetwork, TFWire>(location, wireNetworks);

		public static void OnItemPipeKill(Point16 location)
			=> OnThingKill<ItemNetwork, ItemPipe>(location, itemNetworks);

		public static void OnFluidPipeKill(Point16 location)
			=> OnThingKill<FluidNetwork, FluidPipe>(location, fluidNetworks);

		private static void OnThingKill<TNet, TEntry>(Point16 location, List<TNet> networks) where TNet : class, INetwork, INetwork<TEntry>, new() where TEntry : struct, INetworkable, INetworkable<TEntry>{
			//Separate the adjacent entries into unique networks, then have those networks refresh their entries/machines
			Point16 up = location + new Point16(0, -1);
			Point16 left = location + new Point16(-1, 0);
			Point16 right = location + new Point16(1, 0);
			Point16 down = location + new Point16(0, 1);

			bool centerHasThing = HasEntryAt<TNet, TEntry>(location, networks, out TNet net);
			if(!centerHasThing)
				return;

			net.RemoveEntryAt(location);
			
			//Find the networks
			bool hasEntryUp = HasEntryAt<TNet, TEntry>(up, networks, out TNet netUp);
			bool hasEntryLeft = HasEntryAt<TNet, TEntry>(left, networks, out TNet netLeft);
			bool hasEntryRight = HasEntryAt<TNet, TEntry>(right, networks, out TNet netRight);
			bool hasEntryDown = HasEntryAt<TNet, TEntry>(down, networks, out TNet netDown);

			void ProcessEntry(Point16 dir, bool hasEntry){
				bool netExists = HasEntryAt<TNet, TEntry>(dir, networks, out TNet existing);
				if(hasEntry && !netExists){
					TNet network = new TNet();
					TEntry entry = (TEntry)networkTypeCtors[typeof(TEntry)](dir, network);
					network.AddEntry(entry);

					network.RefreshConnections(location);
					networks.Add(network);

					network.InvokePostEntryKill();
				}
			}

			for(int i = 0; i < networks.Count; i++){
				var id = networks[i].ID;
				if(id == net?.ID || id == netUp?.ID || id == netLeft?.ID || id == netRight?.ID || id == netDown?.ID){
					networks.RemoveAt(i);
					i--;
				}
			}

			//Try to add a new network for each valid direction
			ProcessEntry(up, hasEntryUp);
			ProcessEntry(left, hasEntryLeft);
			ProcessEntry(right, hasEntryRight);
			ProcessEntry(down, hasEntryDown);

			net.SplitDataAcrossNetworks(location);

			CleanupNetworks();
		}

		public static void RemoveMachine(MachineEntity entity){
			foreach(WireNetwork network in wireNetworks)
				if(network.ConnectedMachines.Contains(entity))
					network.RemoveMachine(entity);
			foreach(ItemNetwork network in itemNetworks){
				if(network.ConnectedMachines.Contains(entity)){
					network.RemoveMachine(entity);
					
					Point16 checkOrig = entity.Position - new Point16(1, 1);
					(ModContent.GetModTile(entity.MachineTile) as Machine).GetDefaultParams(out _, out uint width, out uint height, out _);

					for(int cx = checkOrig.X; cx < checkOrig.X + width + 2; cx++){
						for(int cy = checkOrig.Y; cy < checkOrig.Y + height + 2; cy++){
							//Ignore the corners
							if((cx == 0 && cy == 0) || (cx == width + 1 && cy == 0) || (cx == 0 && cy == height + 1) || (cx == width + 1 && cy == height + 1))
								continue;

							Point16 test = new Point16(cx, cy);
							if(network.pipesConnectedToMachines.Contains(test))
								network.pipesConnectedToMachines.Remove(test);
						}
					}
				}
			}
			foreach(FluidNetwork network in fluidNetworks)
				if(network.ConnectedMachines.Contains(entity))
					network.RemoveMachine(entity);
		}

		public static bool HasWireAt(Point16 location, out WireNetwork net)
			=> HasEntryAt<WireNetwork, TFWire>(location, wireNetworks, out net);

		public static bool HasItemPipeAt(Point16 location, out ItemNetwork net)
			=> HasEntryAt<ItemNetwork, ItemPipe>(location, itemNetworks, out net);

		public static bool HasFluidPipeAt(Point16 location, out FluidNetwork net)
			=> HasEntryAt<FluidNetwork, FluidPipe>(location, fluidNetworks, out net);

		internal static bool HasEntryAt<TNet, TEntry>(Point16 location, List<TNet> nets, out TNet net) where TNet : class, INetwork, INetwork<TEntry>, new() where TEntry : struct, INetworkable, INetworkable<TEntry>{
			foreach(TNet network in nets){
				if(network.HasEntryAt(location)){
					net = network;
					return true;
				}
			}

			net = null;
			return false;
		}

		public static List<WireNetwork> GetWireNetworksConnectedTo(PoweredMachineEntity entity){
			var nets = new List<WireNetwork>();

			//For some reason, "network.ConnectedMachines.Contains(entity)" doesn't work.  Oh well
			foreach(var network in wireNetworks)
				foreach(var machine in network.ConnectedMachines)
					if(machine.Position == entity.Position)
						nets.Add(network);

			return nets;
		}

		internal static void SendPowerToMachines(){
			//Force power-generating entities to update and import their power to the network before any machines attempt to use said power
			foreach(var entity in TileEntity.ByPosition.Values){
				if(entity is GeneratorEntity gen){
					gen.updating = true;
					gen.Update();
					gen.updating = false;
				}
			}

			foreach(var network in wireNetworks)
				network.ExportFlux();
		}
	}
}
