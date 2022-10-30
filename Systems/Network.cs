using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using TerraScience.API.Interfaces;
using TerraScience.Content.TileEntities;
using TerraScience.Content.TileEntities.Energy;
using TerraScience.Content.Tiles;
using TerraScience.Content.Tiles.Multitiles;
using TerraScience.Utilities;

namespace TerraScience.Systems{
	public abstract class Network<T, TTile> : INetwork<T> where T : struct, INetworkable, INetworkable<T> where TTile : JunctionMergeable{
		public HashSet<T> Hash { get; set; } = new HashSet<T>();
		public List<MachineEntity> ConnectedMachines { get; set; } = new List<MachineEntity>();

		//Used to make finding wires faster...
		public int ID{ get; private set; } = -1;

		private static int nextID = 0;

		internal event Action OnClear;
		/// <summary>
		/// Called only once on the entry being directly placed by the player
		/// </summary>
		internal event Action<Point16> OnInitialPlace;
		/// <summary>
		/// Called for each entry in the network when refreshing its paths and connections
		/// </summary>
		internal event Action<Point16> OnEntryPlace;
		/// <summary>
		/// Called only once for the entry being directly removed by the player
		/// </summary>
		internal event Action<Point16> OnEntryKill;
		internal event Action PostEntryKill;
		internal event Action PostRefreshConnections;

		public Network(bool ignoreID = false){
			if(!ignoreID)
				ID = nextID++;
		}

		public virtual TagCompound Save() => null;

		public virtual void Load(TagCompound tag){ }

		/// <summary>
		/// Return data that should be shared when combining networks
		/// </summary>
		public virtual TagCompound CombineSave() => null;

		public virtual void LoadCombinedData(TagCompound up, TagCompound left, TagCompound right, TagCompound down){ }

		public virtual void SplitDataAcrossNetworks(Point16 splitOrig){ }

		public virtual bool CanCombine(Point16 orig, Point16 dir) => true;

		public override bool Equals(object obj)
			=> obj is Network<T, TTile> network && ID == network.ID;

		public override int GetHashCode()
			=> ID;

		public static bool operator ==(Network<T, TTile> first, Network<T, TTile> second)
			=> first?.ID == second?.ID;

		public static bool operator !=(Network<T, TTile> first, Network<T, TTile> second)
			=> first?.ID != second?.ID;

		internal abstract JunctionType Type{ get; }

		public void AddEntry(T entry){
			if(Hash.Add(entry)){
				OnInitialPlace?.Invoke(entry.Position);
				OnEntryPlace?.Invoke(entry.Position);
			}
		}

		public void AddMachine(MachineEntity entity){
			if(!ConnectedMachines.Contains(entity)) 
				ConnectedMachines.Add(entity);
		}

		public List<T> GetEntries()
			=> Hash.ToList();

		public bool HasEntry(T entry)
			=> Hash.Contains(entry);

		public bool HasEntryAt(Point16 location){
			//Short-circuit bad inputs
			if(location.X < 0 || location.X >= Main.maxTilesX || location.Y < 0 || location.Y >= Main.maxTilesY)
				return false;

			T entry = (T)NetworkCollection.networkTypeCtors[typeof(T)](location, this);

			return Hash.Contains(entry);
		}

		public bool HasMachineAt(Point16 location){
			Tile tile = Framing.GetTileSafely(location);
			if(!(ModContent.GetModTile(tile.TileType) is Machine))
				return false;

			Point16 origin = location - tile.TileCoord();
			return ConnectedMachines.Any(m => m.Position == origin);
		}

		public void GetMergeInfo(out JunctionMerge leftRight, out JunctionMerge upDown, out JunctionMerge all){
			switch(Type){
				case JunctionType.Wires:
					leftRight = JunctionMerge.Wires_LeftRight;
					upDown = JunctionMerge.Wires_UpDown;
					all = JunctionMerge.Wires_All;
					break;
				case JunctionType.Items:
					leftRight = JunctionMerge.Items_LeftRight;
					upDown = JunctionMerge.Items_UpDown;
					all = JunctionMerge.Items_All;
					break;
				case JunctionType.Fluids:
					leftRight = JunctionMerge.Fluids_LeftRight;
					upDown = JunctionMerge.Fluids_UpDown;
					all = JunctionMerge.Fluids_All;
					break;
				default:
					throw new ArgumentException("Invalid network type detected: " + Type.EnumName());
			}
		}

		public void RefreshConnections(Point16 ignoreLocation){
			//Start from the first entry, then keep adding adjacent entries and machines until the network is fully combed through
			var oldEntires = new HashSet<T>(Hash);
			T startEntry = oldEntires.ToList()[0];

			//Clear the collections
			Hash.Clear();
			ConnectedMachines.Clear();
			OnClear?.Invoke();

			//Initialize some vars
			// TODO: initialize these lists into HashSets for faster code
			List<Point16> pos = new List<Point16>(){
				startEntry.Position
			};
			OnEntryPlace?.Invoke(startEntry.Position);

			List<Point16> tiles = new List<Point16>();
			int junction = ModContent.TileType<TransportJunction>();

			bool Valid(Point16 p) => p.X >= 0 && p.X < Main.maxTilesX && p.Y >= 0 && p.Y < Main.maxTilesY;

			bool TryAddEntry(Point16 orig, Point16 dir, bool recursion = false){
				if(orig + dir == ignoreLocation || pos.Contains(orig + dir))
					return false;

				if(Valid(orig + dir)){
					Tile tile = Framing.GetTileSafely(orig);
					Tile dirTile = Framing.GetTileSafely(orig + dir);

					ModTile mTile = ModContent.GetModTile(tile.TileType);
					ModTile dirMTile = ModContent.GetModTile(dirTile.TileType);

					if(dirMTile is TTile || (dirMTile is ItemPumpTile && Type == JunctionType.Items) || (dirMTile is FluidPumpTile && Type == JunctionType.Fluids)){
						//Just add the tile damnit
						pos.Add(orig + dir);

						OnEntryPlace?.Invoke(orig + dir);

						return true;
					}else if(dirMTile is TransportJunction){
						GetMergeInfo(out JunctionMerge leftRight, out JunctionMerge upDown, out JunctionMerge all);

						JunctionMerge entryMerge = dir.X != 0 ? leftRight : upDown;
						JunctionMerge merge = entryMerge;
						JunctionMerge dirMerge = JunctionMergeable.mergeTypes[dirTile.TileFrameX / 18];

						JunctionMerge mask = merge & all;
						JunctionMerge dirMask = dirMerge & all;

						if((dirMask & mask) != 0){
							//The two should be able to connect
							pos.Add(orig + dir);
							OnEntryPlace?.Invoke(orig + dir);

							if(!recursion && Valid(orig + dir + dir)){
								Tile axisTile = Framing.GetTileSafely(orig + dir + dir);
								ModTile axisMTile = ModContent.GetModTile(axisTile.TileType);

								JunctionMerge axisMerge = axisTile.TileType == junction ? JunctionMergeable.mergeTypes[axisTile.TileFrameX / 18] : entryMerge;
								JunctionMerge axisMask = axisMerge & all;

								if((axisMask & mask) != 0){
									pos.Add(orig + dir + dir);
									OnEntryPlace?.Invoke(orig + dir + dir);
								}
							}

							return true;
						}
					}
				}

				return false;
			}

			void TryAddMachine(Point16 dir){
				if(dir != ignoreLocation || Valid(dir))
					return;
				Tile tile = Framing.GetTileSafely(dir);

				//Must be a machine tile
				if(!TileUtils.tileToEntity.ContainsKey(tile.TileType))
					return;

				//Top-leftmost tile
				Point16 origin = dir - tile.TileCoord();

				if(!tiles.Contains(origin) && TileEntity.ByPosition.ContainsKey(origin)){
					var entity = TileEntity.ByPosition[origin] as MachineEntity;

					if((entity is PoweredMachineEntity && Type == JunctionType.Wires) || (((entity.HijackCanBeInteractedWithItemNetworks(out bool canInteract, out bool canInput, out bool canOutput) && canInteract) || canInput || entity.GetInputSlots().Length > 0 || canOutput || entity.GetOutputSlots().Length > 0) && Type == JunctionType.Items) || (entity is IFluidMachine && Type == JunctionType.Fluids))
						tiles.Add(origin);
				}
			}

			//Keep on looping
			//As new positions are added, the loop will run for longer and longer
			//Positions already added will be skipped over
			for(int i = 0; i < pos.Count; i++){
				Point16 loc = pos[i];

				//Junction tiles and entries across their connections are added in TryAddJunctionOrJunctionEntry
				//Thus, they need to be skipped here
				if(ModContent.GetModTile(Framing.GetTileSafely(loc.X, loc.Y).TileType) is TransportJunction)
					continue;

				if(!TryAddEntry(loc, new Point16(0, -1)))
					TryAddMachine(loc + new Point16(0, -1));
				if(!TryAddEntry(loc, new Point16(0, 1)))
					TryAddMachine(loc + new Point16(0, 1));

				if(!TryAddEntry(loc, new Point16(-1, 0)))
					TryAddMachine(loc + new Point16(-1, 0));
				if(!TryAddEntry(loc, new Point16(1, 0)))
					TryAddMachine(loc + new Point16(1, 0));
			}

			//Convert the positions back to wires and machines...
			Hash = new HashSet<T>(pos.Select(p => (T)NetworkCollection.networkTypeCtors[typeof(T)](p, this)));
			ConnectedMachines = new List<MachineEntity>(tiles.Select(p => TileEntity.ByPosition[p] as MachineEntity));

			Cleanup();

			PostRefreshConnections?.Invoke();
		}

		public void InvokePostEntryKill() => PostEntryKill?.Invoke();

		public void RemoveEntry(T entry){
			if(Hash.Remove(entry))
				OnEntryKill?.Invoke(entry.Position);
		}

		public void RemoveEntryAt(Point16 location){
			var entries = GetEntries();

			for(int i = 0; i < entries.Count; i++){
				if(entries[i].Position == location){ 
					OnEntryKill?.Invoke(entries[i].Position);

					entries.RemoveAt(i);
					i--;
				}
			}
		}

		public void RemoveMachine(MachineEntity entity){
			ConnectedMachines.Remove(entity);

			OnEntryKill?.Invoke(entity.Position);
		}

		private bool ValidTile(Tile tile){
			if(!tile.HasTile || tile.TileType < TileID.Count)
				return false;

			var modTile = ModContent.GetModTile(tile.TileType);
			return modTile is TTile || modTile is TransportJunction || (Type == JunctionType.Items && modTile is ItemPumpTile) || (Type == JunctionType.Fluids && modTile is FluidPumpTile);
		}

		public void Cleanup(){
			var entries = GetEntries();

			for(int i = 0; i < entries.Count; i++){
				if(!ValidTile(Framing.GetTileSafely(entries[i].Position))){
					OnEntryKill?.Invoke(entries[i].Position);

					entries.RemoveAt(i);
					i--;
				}
			}

			Hash = new HashSet<T>(entries);
		}

		public abstract INetwork Clone();
	}
}
