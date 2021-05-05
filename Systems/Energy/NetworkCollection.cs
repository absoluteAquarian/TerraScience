using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader.IO;
using TerraScience.Content.TileEntities.Energy;

namespace TerraScience.Systems.Energy{
	public static class NetworkCollection{
		private static List<WireNetwork> networks;

		public static void EnsureNetworkIsInitialized(){
			if(networks is null)
				networks = new List<WireNetwork>();
		}

		public static void CleanupNetworks(){
			for(int i = 0; i < networks.Count; i++){
				WireNetwork net = networks[i];

				if(net is null){
					//Remove null networks
					networks.RemoveAt(i);
					i--;
				}else{
					//Remove any wires not actually in the network
					net.Cleanup();

					if(net.GetWires().Count == 0){
						//Remove empty networks
						networks.RemoveAt(i);
						i--;
					}
				}
			}
		}

		public static void Unload(){
			networks = null;
		}

		public static void Load(TagCompound tag){
			if(tag.GetList<Point16>("wires") is List<Point16> list){
				for(int i = 0; i < list.Count; i++){
					Point16 point = list[i];
					WireNetwork network = new WireNetwork();

					network.AddWire(new TFWire(point, network));

					network.RefreshConnections();

					networks.Add(network);
				}
			}
		}

		//Save the location of the wires and connected machines
		public static TagCompound Save()
			=> new TagCompound(){
				["wires"] = networks?.Count == 0 ? null : networks.Select(net => net.GetWires()[0].location).ToList()
			};

		public static void OnWirePlace(Point16 location){
			//Check if a wire network is adjacent to this wire
			//If there is one, connect the wire to the network
			//If the wire would be connected to multiple of them, then combine the networks

			Point16 up = new Point16(location.X, location.Y - 1);
			Point16 left = new Point16(location.X - 1, location.Y);
			Point16 right = new Point16(location.X + 1, location.Y);
			Point16 down = new Point16(location.X, location.Y + 1);

			//Find the networks to connect
			List<WireNetwork> networksToConnect = new List<WireNetwork>();
			for(int i = 0; i < networks.Count; i++){
				WireNetwork network = networks[i];

				if(network.HasWireAt(up) || network.HasWireAt(left) || network.HasWireAt(right) || network.HasWireAt(down)){
					networksToConnect.Add(network);
					networks.RemoveAt(i);
					i--;
				}
			}

			//Then combine them if necessary (or make a new one)
			WireNetwork newNetwork;
			if(networksToConnect.Count == 0)
				newNetwork = new WireNetwork();
			else if(networks.Count == 1)
				newNetwork = networksToConnect[0];
			else
				newNetwork = WireNetwork.CombineNetworks(networksToConnect.ToArray());

			//Then create the wire and add it
			TFWire wire = new TFWire(location, newNetwork);
			newNetwork.AddWire(wire);
			newNetwork.RefreshConnections();
			networks.Add(newNetwork);

		//	Main.NewText($"Wire location updated at [X: {location.X}, Y: {location.Y}] | Network count: {networks.Count}");
		}

		public static void OnWireKill(Point16 location){
			//Remove the wire at this location from its network
			for(int i = 0; i < networks.Count; i++){
				if(networks[i].HasWireAt(location)){
					networks[i].RemoveWireAt(location);

			//		Main.NewText($"Wire removed at location [X: {location.X}, Y: {location.Y}] from Network [ID: {networks[i].id}]");
				}
			}

			//Separate the adjacent wires into unique networks, then have those networks refresh their wires/machines
			Point16 up = location + new Point16(0, -1);
			Point16 left = location + new Point16(-1, 0);
			Point16 right = location + new Point16(1, 0);
			Point16 down = location + new Point16(0, 1);

			//Find the networks
			bool hasWireUp = false, hasWireLeft = false, hasWireRight = false, hasWireDown = false;
			for(int i = 0; i < networks.Count; i++){
				WireNetwork network = networks[i];

				hasWireUp |= network.HasWireAt(up);
				hasWireLeft |= network.HasWireAt(left);
				hasWireRight |= network.HasWireAt(right);
				hasWireDown |= network.HasWireAt(down);

				if(network.HasWireAt(up) || network.HasWireAt(left) || network.HasWireAt(right) || network.HasWireAt(down)){
					networks.RemoveAt(i);
					i--;
				}

				if(hasWireUp && hasWireLeft && hasWireRight && hasWireDown)
					break;
			}

			void ProcessWire(Point16 dir, bool hasWire){
				if(hasWire){
					WireNetwork network = new WireNetwork();
					TFWire wire = new TFWire(dir, network);
					network.AddWire(wire);
					network.RefreshConnections();
					networks.Add(network);
				}
			}

			//Try to add a new network for each valid direction
			ProcessWire(up, hasWireUp);
			ProcessWire(left, hasWireLeft);
			ProcessWire(right, hasWireRight);
			ProcessWire(down, hasWireDown);

			//Then merge any networks that end up sharing wires
			if(hasWireUp)
				OnWirePlace(up);
			if(hasWireLeft)
				OnWirePlace(left);
			if(hasWireRight)
				OnWirePlace(right);
			if(hasWireDown)
				OnWirePlace(down);
		}

		public static List<WireNetwork> GetNetworksConnectedTo(PoweredMachineEntity entity)
			=> networks.Where(net => net.connectedMachines.Contains(entity)).ToList();

		public static void RemoveMachine(PoweredMachineEntity entity){
			foreach(WireNetwork network in networks)
				if(network.connectedMachines.Contains(entity))
					network.RemoveMachine(entity);
		}

		public static bool HasWireAt(Point16 location, out WireNetwork net){
			foreach(WireNetwork network in networks){
				if(network.HasWireAt(location)){
					net = network;
					return true;
				}
			}

			net = null;
			return false;
		}
	}
}
