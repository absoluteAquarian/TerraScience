using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using TerraScience.API.Interfaces;
using TerraScience.Content.TileEntities;
using TerraScience.Content.Tiles;
using TerraScience.Systems.Pathfinding;
using TerraScience.Utilities;

namespace TerraScience.Systems.Pipes{
	internal class Timer{
		public int value;
	}

	public class ItemNetwork : Network<ItemPipe, ItemTransportTile>{
		public List<ItemNetworkPath> paths;

		/// <summary>
		/// The chest IDs in <seealso cref="Main.chest"/>[] that are connected to this item network
		/// </summary>
		public List<int> chests;

		public List<Point16> pipesConnectedToChests;
		public List<Point16> pipesConnectedToMachines;

		internal Dictionary<Point16, Timer> pumpTimers;

		internal Dictionary<Point16, List<ItemPathResult>> pumpPathsToMachines;
		internal Dictionary<Point16, List<ItemPathResult>> pumpPathsToChests;

		private HashSet<Point16> pumpsToRefresh;

		//The cache of the list of valid path targets, indexed by item type
		internal Dictionary<int, HashSet<Point16>> cachedValidItemPaths;

		internal override JunctionType Type => JunctionType.Items;

		public ItemNetwork() : base(false){
			Init();
		}

		public ItemNetwork(bool ignoreID = false) : base(ignoreID){
			Init();
		}

		private void Init(){
			paths = new List<ItemNetworkPath>();
			pipesConnectedToChests = new List<Point16>();
			pipesConnectedToMachines = new List<Point16>();
			pumpTimers = new Dictionary<Point16, Timer>();
			chests = new List<int>();
			pumpPathsToMachines = new Dictionary<Point16, List<ItemPathResult>>();
			pumpPathsToChests = new Dictionary<Point16, List<ItemPathResult>>();
			pumpsToRefresh = new HashSet<Point16>();
			cachedValidItemPaths = new Dictionary<int, HashSet<Point16>>();

			OnInitialPlace += pos => {
				Tile tile = Framing.GetTileSafely(pos);
				if(!(ModContent.GetModTile(tile.type) is ItemPumpTile))
					InformPathsOfNetUpdate(pos);
			};

			OnEntryKill += pos => {
				Tile tile = Framing.GetTileSafely(pos);
				if(!(ModContent.GetModTile(tile.type) is ItemPumpTile))
					InformPathsOfNetUpdate(pos);
			};

			OnClear += () => {
				chests.Clear();
				pipesConnectedToChests.Clear();
				pipesConnectedToMachines.Clear();
				pumpPathsToMachines.Clear();
				pumpPathsToChests.Clear();
				cachedValidItemPaths.Clear();
			};

			OnEntryPlace += pos => {
				int chestUp = ChestUtils.FindChestByGuessingImproved(pos.X, pos.Y - 1);
				if(chestUp > -1){
					if(!pipesConnectedToChests.Contains(pos))
						pipesConnectedToChests.Add(pos);
					if(!chests.Contains(chestUp))
						chests.Add(chestUp);
				}

				int chestLeft = ChestUtils.FindChestByGuessingImproved(pos.X - 1, pos.Y);
				if(chestLeft > -1){
					if(!pipesConnectedToChests.Contains(pos))
						pipesConnectedToChests.Add(pos);
					if(!chests.Contains(chestLeft))
						chests.Add(chestLeft);
				}

				int chestRight = ChestUtils.FindChestByGuessingImproved(pos.X + 1, pos.Y);
				if(chestRight > -1){
					if(!pipesConnectedToChests.Contains(pos))
						pipesConnectedToChests.Add(pos);
					if(!chests.Contains(chestRight))
						chests.Add(chestRight);
				}

				int chestDown = ChestUtils.FindChestByGuessingImproved(pos.X, pos.Y + 1);
				if(chestDown > -1){
					if(!pipesConnectedToChests.Contains(pos))
						pipesConnectedToChests.Add(pos);
					if(!chests.Contains(chestDown))
						chests.Add(chestDown);
				}

				if(TileEntityUtils.TryFindMachineEntity(pos + new Point16(0, -1), out MachineEntity _)){
					if(!pipesConnectedToMachines.Contains(pos))
						pipesConnectedToMachines.Add(pos);
				}

				if(TileEntityUtils.TryFindMachineEntity(pos + new Point16(-1, 0), out MachineEntity _)){
					if(!pipesConnectedToMachines.Contains(pos))
						pipesConnectedToMachines.Add(pos);
				}

				if(TileEntityUtils.TryFindMachineEntity(pos + new Point16(1, 0), out MachineEntity _)){
					if(!pipesConnectedToMachines.Contains(pos))
						pipesConnectedToMachines.Add(pos);
				}

				if(TileEntityUtils.TryFindMachineEntity(pos + new Point16(0, 1), out MachineEntity _)){
					if(!pipesConnectedToMachines.Contains(pos))
						pipesConnectedToMachines.Add(pos);
				}

				Tile tile = Framing.GetTileSafely(pos);
				if(ModContent.GetModTile(tile.type) is ItemPumpTile){
					if(!pumpTimers.ContainsKey(pos))
						pumpTimers.Add(pos, new Timer(){ value = 18 });

					if(ID != -1)
						pumpsToRefresh.Add(pos);
				}else if(ID != -1){
					var pumpMachines = FilterPumps(pumpPathsToMachines, pos);
					var pumpChests = FilterPumps(pumpPathsToChests, pos);

					foreach(var start in pumpMachines.Union(pumpChests).ToList())
						pumpsToRefresh.Add(start);
				}
			};

			OnEntryKill += pos => {
				if(pipesConnectedToChests.Remove(pos)){
					//Only check if chests were there if this pipe was actually connected to a chest
					int chestUp = ChestUtils.FindChestByGuessingImproved(pos.X, pos.Y - 1);
					if(chestUp > -1)
						chests.Remove(chestUp);

					int chestLeft = ChestUtils.FindChestByGuessingImproved(pos.X - 1, pos.Y);
					if(chestLeft > -1)
						chests.Remove(chestLeft);

					int chestRight = ChestUtils.FindChestByGuessingImproved(pos.X + 1, pos.Y);
					if(chestRight > -1)
						chests.Remove(chestRight);

					int chestDown = ChestUtils.FindChestByGuessingImproved(pos.X, pos.Y + 1);
					if(chestDown > -1)
						chests.Remove(chestDown);
				}

				pipesConnectedToMachines.Remove(pos);

				//If any paths had their items on this tile, make them drop
				for(int i = 0; i < paths.Count; i++){
					if(paths[i].worldCenter.ToTileCoordinates16() == pos){
						paths[i].SpawnInWorld();
						i--;
					}
				}

				Tile tile = Framing.GetTileSafely(pos);
				if(ModContent.GetModTile(tile.type) is ItemPumpTile){
					pumpTimers.Remove(pos);

					pumpPathsToMachines.Remove(pos);
					pumpPathsToChests.Remove(pos);
				}else if(ID != -1){
					var pumpMachines = FilterPumps(pumpPathsToMachines, pos);
					var pumpChests = FilterPumps(pumpPathsToChests, pos);

					foreach(var start in pumpMachines.Union(pumpChests).ToList())
						pumpsToRefresh.Add(start);
				}
			};

			PostEntryKill += () => {
				for(int i = 0; i < paths.Count; i++){
					if(!HasEntryAt(paths[i].worldCenter.ToTileCoordinates16())){
						paths[i].Delete();
						paths.RemoveAt(i);
						i--;
					}
				}

				cachedValidItemPaths.Clear();
			};

			PostRefreshConnections += () => {
				if(ID == -1)
					return;

				foreach(var pump in pumpsToRefresh)
					RefreshPaths(pump);

				pumpsToRefresh.Clear();
				cachedValidItemPaths.Clear();
			};
		}

		private static List<Point16> FilterPumps(Dictionary<Point16, List<ItemPathResult>> dictionary, Point16 pos)
			=> dictionary.Where(kvp => kvp.Value.Any(t => t.list.Any(p => pos == p)))  //Only update paths that have this tile to speed up processing time
				.Select(kvp => kvp.Key)
				.ToList();

		internal void RefreshPaths(Point16 pump){
			//IDs of -1 are created from AStar.SimulateItemNetwork
			if(!(ModContent.GetModTile(Framing.GetTileSafely(pump).type) is ItemPumpTile) || ID == -1)
				return;

			if(!pumpPathsToMachines.TryGetValue(pump, out var machineList))
				pumpPathsToMachines.Add(pump, machineList = new List<ItemPathResult>());
			else
				machineList.Clear();

			foreach(var pipeMachine in pipesConnectedToMachines){
				var path = AStar.SimulateItemNetwork(this, pump, pipeMachine, out float travelTime);

				if(path != null && travelTime > 0){
					path.Reverse();

					machineList.Add(new ItemPathResult(){ time = travelTime, list = path });
				}
			}

			if(machineList.Count > 0)
				pumpPathsToMachines[pump] = new List<ItemPathResult>(machineList.OrderBy(path => path.time));

			if(!pumpPathsToChests.TryGetValue(pump, out var chestList))
				pumpPathsToChests.Add(pump, chestList = new List<ItemPathResult>());
			else
				chestList.Clear();

			foreach(var pipeChest in pipesConnectedToChests){
				var path = AStar.SimulateItemNetwork(this, pump, pipeChest, out float travelTime);

				if(path != null && travelTime > 0){
					path.Reverse();

					chestList.Add(new ItemPathResult(){ time = travelTime, list = path });
				}
			}

			if(chestList.Count > 0)
				pumpPathsToChests[pump] = new List<ItemPathResult>(chestList.OrderBy(path => path.time));
		}

		public override TagCompound Save(){
			var dirs = SavePumpDirs();

			return new TagCompound(){
				["paths"] = paths.Count == 0 ? null : paths.Select(p => p.Save()).ToList(),
				["pumpPositions"] = dirs?.Count > 0 ? dirs.Select(t => t.Item1).ToList() : null,
				["pumpDirs"] = dirs?.Count > 0 ? dirs.Select(t => t.Item2).ToList() : null,
				["pumpTimerLocations"] = pumpTimers.Keys.ToList(),
				["pumpTimerValues"] = pumpTimers.Values.Select(t => (byte)t.value).ToList()
			};
		}

		public override void Load(TagCompound tag){
			if(tag.GetList<Point16>("pumpPositions") is List<Point16> dirPos && tag.GetList<short>("pumpDirs") is List<short> dirDirs){
				if(dirPos.Count == dirDirs.Count)
					for(int i = 0; i < dirPos.Count; i++)
						Framing.GetTileSafely(dirPos[i]).frameX = (short)(dirDirs[i] * 18);
				else
					TechMod.Instance.Logger.Error("Network data was modified by an external program (entries: \"pumpPositions\", \"pumpDirs\")");
			}

			pumpTimers = new Dictionary<Point16, Timer>();
			if(tag.GetList<Point16>("pumpTimerLocations") is List<Point16> pumpKeys && tag.GetList<byte>("pumpTimerValues") is List<byte> pumpValues){
				if(pumpKeys.Count == pumpValues.Count)
					for(int i = 0; i < pumpKeys.Count; i++)
						pumpTimers.Add(pumpKeys[i], new Timer(){ value = pumpValues[i] });
				else
					TechMod.Instance.Logger.Error("Network data was modified by an external program (entries: \"pumpTimerLocations\", \"pumpTimerValues\")");
			}

			RefreshConnections(NetworkCollection.ignoreCheckLocation);

			if(tag.GetList<TagCompound>("paths") is List<TagCompound> tags){
				paths = tags.Select(t => ItemNetworkPath.Load(t)).ToList();

				foreach(var path in paths){
					path.itemNetwork = this;

					if(path.Path != null){
						//Ensure the path is correct.  If it is not, make it be recalculated
						foreach(var tile in path.Path){
							if(!HasEntryAt(tile))
								goto logRefreshedPath;
						}

						if(path.Path.Count > 0){
							//Check the destination machine.  If it won't accept the item, force the item path to be recalculated
							Item item = ItemIO.Load(path.itemData);

							Point16 target = path.Path.Last();

							MachineEntity machine = null;
							Chest targetChest = null;
							if((machine = ItemNetworkPath.FindConnectedMachine(this, target, item)) is null && (targetChest = ItemNetworkPath.FindConnectedChest(this, target, item, out _)) is null)
								goto logRefreshedPath;

							//"item.stack" is modified here, hence the need to clone it earlier
							var usepaths = paths.Where(p => p.wander || p.finalTarget == target || (p?.Path?.Count > 0 && p.Path.Last() == target));
							bool successful = machine != null
								? ItemNetworkPath.SimulateMachineInput(machine, item, usepaths)
								: ItemNetworkPath.SimulateChestInput(targetChest, item, usepaths);

							if(!successful)
								goto logRefreshedPath;
						}
					}

					continue;
logRefreshedPath:
					path.needsPathRefresh = true;
					path.delayPathCalc = false;

					bool release = TechMod.Release;
					if(!release)
						TechMod.Instance.Logger.Debug($"[ItemNetwork/#{ID}] Network item #{path.id} had an invalid saved path.  Forcing a new path on next update.");
				}
			}else
				paths = new List<ItemNetworkPath>();
		}

		public override TagCompound CombineSave()
			=> new TagCompound(){
				["items"] = paths.Select(p => p.Save()).ToList(),
				["pumpTimerLocations"] = pumpTimers.Keys.ToList(),
				["pumpTimerValues"] = pumpTimers.Values.Select(t => (byte)t.value).ToList()
			};

		public override void LoadCombinedData(TagCompound up, TagCompound left, TagCompound right, TagCompound down){
			if(up?.GetList<TagCompound>("items") is List<TagCompound> upList)
				paths.AddRange(upList.Select(t => ItemNetworkPath.Load(t)));
			if(left?.GetList<TagCompound>("items") is List<TagCompound> leftList)
				paths.AddRange(leftList.Select(t => ItemNetworkPath.Load(t)));
			if(right?.GetList<TagCompound>("items") is List<TagCompound> rightList)
				paths.AddRange(rightList.Select(t => ItemNetworkPath.Load(t)));
			if(down?.GetList<TagCompound>("items") is List<TagCompound> downList)
				paths.AddRange(downList.Select(t => ItemNetworkPath.Load(t)));

			foreach(var path in paths){
				path.itemNetwork = this;
				path.needsPathRefresh = true;
			}
			
			LoadCombinedPumps(up);
			LoadCombinedPumps(left);
			LoadCombinedPumps(right);
			LoadCombinedPumps(down);
		}

		private void LoadCombinedPumps(TagCompound tag){
			if(tag?.GetList<Point16>("pumpTimerLocations") is List<Point16> pumps && tag?.GetList<byte>("pumpTimerValues") is List<byte> timers)
				for(int i = 0; i < pumps.Count; i++)
					if(!pumpTimers.ContainsKey(pumps[i]))
						pumpTimers.Add(pumps[i], new Timer(){ value = timers[i] });
		}

		public override void SplitDataAcrossNetworks(Point16 splitOrig){
			NetworkCollection.HasItemPipeAt(splitOrig + new Point16(0, -1), out ItemNetwork upNet);
			NetworkCollection.HasItemPipeAt(splitOrig + new Point16(-1, 0), out ItemNetwork leftNet);
			NetworkCollection.HasItemPipeAt(splitOrig + new Point16(1, 0), out ItemNetwork rightNet);
			NetworkCollection.HasItemPipeAt(splitOrig + new Point16(0, 1), out ItemNetwork downNet);

			foreach(var path in paths){
				path.needsPathRefresh = true;

				var tile = path.worldCenter.ToTileCoordinates16();

				//Directions that both belong to one network shouldn't cause problems due to me using if-else
				if(upNet?.HasEntryAt(tile) ?? false){
					path.itemNetwork = upNet;
					upNet.paths.Add(path);
				}else if(leftNet?.HasEntryAt(tile) ?? false){
					path.itemNetwork = leftNet;
					leftNet.paths.Add(path);
				}else if(rightNet?.HasEntryAt(tile) ?? false){
					path.itemNetwork = rightNet;
					rightNet.paths.Add(path);
				}else if(downNet?.HasEntryAt(tile) ?? false){
					path.itemNetwork = downNet;
					downNet.paths.Add(path);
				}
			}
		}

		public List<(Point16, short)> SavePumpDirs(){
			var pumps = GetEntries().Where(e => ModContent.GetModTile(Framing.GetTileSafely(e.Position).type) is ItemPumpTile).ToList();

			return pumps.Count == 0 ? null : pumps.Select(e => (e.Position, (short)(Framing.GetTileSafely(e.Position).frameX / 18))).ToList();
		}

		private void InformPathsOfNetUpdate(Point16? updated = null){
			if(updated is Point16 pos){
				foreach(var path in paths){
					var pathPos = path?.Path;

					if(pathPos != null && pathPos.Any(p => pos == p))  //Only update items that would pass over this tile to speed up processing time
						path.needsPathRefresh = true;
				}
			}
		}

		public void RemovePath(int id){
			for(int i = 0; i < paths.Count; i++){
				if(paths[i].id == id){
					paths[i].Delete();
					paths.RemoveAt(i);
					return;
				}
			}
		}

		public bool HasChestAt(Point16 tilePos){
			//ChestUtils.FindChestByGuessingImproved searches a 2x2 area and returns the ID if any of the 4 tiles in that area have a chest
			int chestID = ChestUtils.FindChestByGuessingImproved(tilePos.X, tilePos.Y);
			return Framing.GetTileSafely(tilePos).active() && chestID != -1 && chests.Contains(chestID);
		}

		public List<Func<Item, bool>> ValidMachineInputFuncs(){
			//Iterates through all connected machines and returns a series of checks that an item being pumped into the network must satisfy
			//  before checking the connected chests
			return ConnectedMachines.Select(machine => (Func<Item, bool>)(item => machine.CanBeInput(item))).ToList();
		}

		//Used by Pathfinding.AStar, so we only need to copy the important data
		public override INetwork Clone()
			=> new ItemNetwork(ignoreID: true){
				Hash = new HashSet<ItemPipe>(this.Hash),
				ConnectedMachines = new List<MachineEntity>(this.ConnectedMachines),
				chests = new List<int>(this.chests)
			};

		public override string ToString() => $"ID: {ID}, Items: {paths.Count}, Pumps: {pumpTimers.Count}";
	}
}
