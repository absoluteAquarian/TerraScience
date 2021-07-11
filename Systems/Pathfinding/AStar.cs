using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using TerraScience.Content.Tiles;
using TerraScience.Systems.Pipes;

namespace TerraScience.Systems.Pathfinding{
	public static class AStar{
		private class Entry{
			public Point16 location;

			public float travelTime;
			public int distance;

			public float Heuristic => distance + travelTime;

			public Entry parent;

			public void SetDistance(Point16 target){
				//How many tiles need to be iterated over to reach the target
				distance = Math.Abs(target.X - location.X) + Math.Abs(target.Y - location.Y);
			}

			public override bool Equals(object obj)
				=> obj is Entry entry && entry.location == location;

			public override int GetHashCode() => base.GetHashCode();

			public static bool operator ==(Entry first, Entry second)
				=> first?.location == second?.location;

			public static bool operator !=(Entry first, Entry second)
				=> first?.location != second?.location;

			public override string ToString() => $"Heuristic: {Heuristic}, Location: (X: {location.X}, Y: {location.Y})";
		}
		
		/// <summary>
		/// Finds a series of tile positions that connects <paramref name="source"/> to <paramref name="target"/> in the item network <paramref name="net"/>
		/// </summary>
		/// <param name="net">The item network to move through</param>
		/// <param name="source">The starting tile</param>
		/// <param name="target">The ending tile</param>
		public static List<Point16> SimulateItemNetwork(ItemNetwork net, Point16 source, Point16 target, out float travelTime){
			travelTime = -1;

			if((!net.HasEntryAt(source) || !net.HasEntryAt(target)) && !net.HasMachineAt(target) && !net.HasChestAt(target))
				return null;

			if(source == target)
				return new List<Point16>(){ source };

			//Make a copy of the network
			//All that matters is we get the entries, connected chests and connected machines
			ItemNetwork copy = net.Clone() as ItemNetwork;

			//Search through the network until a path to "target" is found
			var netEntries = copy.GetEntries();
			
			List<Entry> activeMaze = new List<Entry>();
			List<Entry> visitedMaze = new List<Entry>();
			
			//Add the root entry
			Entry root = new Entry(){ location = source };
			activeMaze.Add(root);

			//Keep looping while there's still entries to check
			while(activeMaze.Count > 0){
				Entry check = activeMaze.OrderBy(e => e.Heuristic).First();

				if(check.location == target){
					//Path found; construct it based on the entry parents
					List<Entry> path = new List<Entry>(){ check };
					travelTime = 0;

					while(check.parent != null){
						travelTime += check.travelTime;

						path.Add(check.parent);
						check = check.parent;
					}

					return path.Select(e => e.location).ToList();
				}

				visitedMaze.Add(check);
				activeMaze.Remove(check);

				List<Entry> walkables = GetItemWalkableEntires(net, visitedMaze, check, target);

				//Check the surrounding entries
				foreach(Entry walkable in walkables){
					Entry activeCheck;

					//If this walkable entry is already in the active list, but the existing entry has a worse heuristic, replace it
					//Otherwise, just add the walkable entry to the active list

					//List.IndexOf() ends up using the == and != operators for comparing values, which is good for this scenario
					int index = activeMaze.IndexOf(walkable);
					
					if(index >= 0){
						activeCheck = activeMaze[index];
						if(activeCheck.Heuristic > check.Heuristic){
							activeMaze.Remove(activeCheck);
							activeMaze.Add(walkable);
						}
					}else
						activeMaze.Add(walkable);
				}
			}

			//Could not find a path
			return null;
		}

		private static List<Entry> GetItemWalkableEntires(ItemNetwork net, List<Entry> existing, Entry parent, Point16 target){
			List<Entry> possible = new List<Entry>(){
				new Entry(){ location = parent.location + new Point16(0, -1), parent = parent },
				new Entry(){ location = parent.location + new Point16(-1, 0), parent = parent },
				new Entry(){ location = parent.location + new Point16(1, 0), parent = parent },
				new Entry(){ location = parent.location + new Point16(0, 1), parent = parent }
			};

			Tile parentTile = Framing.GetTileSafely(parent.location);
			if(ModContent.GetModTile(parentTile.type) is ItemPumpTile){
				//Only one direction should be accounted for
				int frameX = parentTile.frameX / 18;

				Entry keep;
				switch(frameX){
					case 0:
						keep = possible[0];
						break;
					case 1:
						keep = possible[1];
						break;
					case 2:
						keep = possible[3];
						break;
					case 3:
						keep = possible[2];
						break;
					default:
						throw new Exception($"Inner TerraScience error -- Unexpected pump tile frame (ID: {frameX})");
				}

				possible.Clear();
				possible.Add(keep);
			}

			for(int i = 0; i < possible.Count; i++){
				var possibleLoc = possible[i].location;
				Tile tile = Framing.GetTileSafely(possibleLoc);

				if(!tile.active() || existing.IndexOf(possible[i]) >= 0 || (!net.HasEntryAt(possibleLoc) && !net.HasMachineAt(possibleLoc) && !net.HasChestAt(possibleLoc)) || ModContent.GetModTile(tile.type) is ItemPumpTile){
					possible.RemoveAt(i);
					i--;
				}else{
					float time = GetItemTravelTime(possibleLoc);

					if(time < 0){
						possible.RemoveAt(i);
						i--;
					}else{
						possible[i].travelTime = parent.travelTime + time;
						possible[i].SetDistance(target);
					}
				}
			}

			return possible;
		}

		private static float GetItemTravelTime(Point16 tilePos){
			float progress = GetItemMovementProgressFactorAt(tilePos);
			
			return progress == -1 ? -1 : 1f / (60f * progress);
		}

		internal static float GetItemMovementProgressFactorAt(Point16 tilePos){
			var mTile = ModContent.GetModTile(Framing.GetTileSafely(tilePos).type);
			return mTile is null
				? -1
				: (mTile is ItemTransportTile item
					? item.TransferProgressPerTick
					: (mTile is TransportJunction
						? ModContent.GetInstance<ItemTransportTile>().TransferProgressPerTick
						: (mTile is ItemPumpTile
							? ModContent.GetInstance<ItemTransportTile>().TransferProgressPerTick
							: -1)));
		}
	}
}
