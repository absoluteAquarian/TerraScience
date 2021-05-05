using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using TerraScience.Content.TileEntities.Energy;
using TerraScience.Content.TileEntities.Energy.Generators;
using TerraScience.Content.TileEntities.Energy.Storage;
using TerraScience.Content.Tiles.Energy;
using TerraScience.Utilities;

namespace TerraScience.Systems.Energy{
	public class WireNetwork{
		//Used to make finding wires faster...
		private HashSet<TFWire> hash = new HashSet<TFWire>();

		public List<PoweredMachineEntity> connectedMachines = new List<PoweredMachineEntity>();

		public readonly int id;

		private static int nextID = 0;

		public WireNetwork(){
			id = nextID++;
		}

		public override bool Equals(object obj)
			=> obj is WireNetwork network && id == network.id;

		public override int GetHashCode()
			=> id;

		public static bool operator ==(WireNetwork first, WireNetwork second)
			=> first?.id == second?.id;

		public static bool operator !=(WireNetwork first, WireNetwork second)
			=> first?.id != second?.id;

		public static WireNetwork CombineNetworks(params WireNetwork[] networks){
			WireNetwork network = new WireNetwork();

			var hashList = networks.SelectMany(n => n.hash).Distinct().ToList();
			for(int i = 0; i < hashList.Count; i++)
				hashList[i] = new TFWire(hashList[i].location, network);
			network.hash = new HashSet<TFWire>(hashList);

			network.connectedMachines = networks.SelectMany(n => n.connectedMachines).Distinct().ToList();

			return network;
		}

		public void RefreshConnections(){
			//Start from the first wire, then keep adding adjacent wires and machines until the network is fully combed through
			TFWire wire = GetWires()[0];

			//Clear the collections
			hash.Clear();
			connectedMachines.Clear();

			//Initialize some vars
			List<Point16> pos = new List<Point16>(){
				wire.location
			};
			List<Point16> tiles = new List<Point16>();
			int type = ModContent.TileType<TFWireTile>();

			//Local functions to make code shorter
			bool Valid(Point16 p) => p.X >= 0 && p.X < Main.maxTilesX && p.Y >= 0 && p.Y < Main.maxTilesY;
			void TryAddWire(Point16 dir){
				if(pos.Contains(dir))
					return;

				Tile tile = null;
				if(Valid(dir))
					tile = Framing.GetTileSafely(dir);

				if(tile?.type == type)
					pos.Add(dir);
			}
			void TryAddMachine(Point16 dir){
				if(tiles.Contains(dir))
					return;

				Tile tile = null;
				if(Valid(dir))
					tile = Framing.GetTileSafely(dir);

				//Must be a machine tile
				if(!TileUtils.tileToEntity.ContainsKey(tile.type))
					return;

				//Top-leftmost tile
				Point16 origin = dir - tile.TileCoord();

				if(TileEntity.ByPosition.ContainsKey(origin) && TileEntity.ByPosition[origin] is PoweredMachineEntity)
					tiles.Add(origin);
			}

			//Keep on looping
			//As new positions are added, the loop will run for longer and longer
			//Positions already added will be skipped over
			for(int i = 0; i < pos.Count; i++){
				Point16 loc = pos[i];

				Point16 up = loc + new Point16(0, -1);
				Point16 left = loc + new Point16(-1, 0);
				Point16 right = loc + new Point16(1, 0);
				Point16 down = loc + new Point16(0, 1);

				TryAddWire(up);
				TryAddWire(left);
				TryAddWire(right);
				TryAddWire(down);

				TryAddMachine(up);
				TryAddMachine(left);
				TryAddMachine(right);
				TryAddMachine(down);
			}

			//Convert the positions back to wires and machines...
			hash = new HashSet<TFWire>(pos.Select(p => new TFWire(p, this)));
			connectedMachines = new List<PoweredMachineEntity>(tiles.Select(p => TileEntity.ByPosition[p] as PoweredMachineEntity));
		}

		public void AddWire(TFWire wire)
			=> hash.Add(wire);

		public void RemoveWire(TFWire wire){
			hash.Remove(wire);
		}

		public void RemoveWireAt(Point16 location){
			hash.RemoveWhere(wire => wire.location == location);
		}

		public void AddMachine(PoweredMachineEntity machine){
			if(!connectedMachines.Contains(machine))
				connectedMachines.Add(machine);
		}

		public void RemoveMachine(PoweredMachineEntity machine){
			connectedMachines.Remove(machine);
		}

		/// <summary>
		/// Sends the received <paramref name="flux"/> to the machines connected to this network.
		/// </summary>
		/// <param name="flux">Terra Flux; the power unit for TerraScience machines</param>
		public void ExportFlux(GeneratorEntity source, TerraFlux flux){
			//Send the flux to a machine, but only if it's not a generator
			List<PoweredMachineEntity> machines = new List<PoweredMachineEntity>();

			foreach(PoweredMachineEntity machine in connectedMachines)
				if((!(machine is GeneratorEntity) || machine is Battery) && !object.ReferenceEquals(machine, source))
					machines.Add(machine);

			//More machines = less flux to each machine
			if(machines.Count > 0){
				source.StoredFlux -= flux;

				TerraFlux orig = flux;
				TerraFlux excess = new TerraFlux(0f);

				foreach(PoweredMachineEntity machine in machines){
					TerraFlux send = orig / machines.Count;

					if(machine is Battery battery && send > battery.ImportRate){
						TerraFlux preSend = send;
						send = battery.ImportRate;
						//Add the extra power to the excess pool
						excess += preSend - send;
					}

					machine.ImportFlux(ref send);
					excess += send;
				}

				source.ImportFlux(ref excess);
			}
		}

		public List<TFWire> GetWires()
			=> hash.ToList();

		public bool HasWire(TFWire wire)
			=> hash.Contains(wire);

		public bool HasWireAt(Point16 location){
			//Short-circuit bad inputs
			if(location.X < 0 || location.X >= Main.maxTilesX || location.Y < 0 || location.Y >= Main.maxTilesY)
				return false;

			//TFWire only checks location when placed in a hash, so that's good enough for us
			TFWire wire = new TFWire(location, this);

			return hash.Contains(wire);
		}

		public void Cleanup(){
			var wires = GetWires();

			for(int i = 0; i < wires.Count; i++){
				Point16 location = wires[i].location;
				if(Framing.GetTileSafely(location).type != ModContent.TileType<TFWireTile>()){
					wires.RemoveAt(i);
					i--;
				}
			}

			hash = new HashSet<TFWire>(wires);
		}
	}
}
