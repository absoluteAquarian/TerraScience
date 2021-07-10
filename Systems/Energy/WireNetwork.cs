using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using TerraScience.API.Interfaces;
using TerraScience.Content.TileEntities;
using TerraScience.Content.TileEntities.Energy;
using TerraScience.Content.TileEntities.Energy.Generators;
using TerraScience.Content.TileEntities.Energy.Storage;
using TerraScience.Content.Tiles;
using TerraScience.Content.Tiles.Energy;

namespace TerraScience.Systems.Energy{
	public class WireNetwork : Network<TFWire, TFWireTile>{
		public TerraFlux totalExportedFlux;

		public TerraFlux StoredFlux{ get; internal set; }

		public TerraFlux Capacity{ get; internal set; }

		public TerraFlux ExportRate{ get; internal set; }

		public TerraFlux ImportRate{ get; internal set; }

		private bool needsRateRefresh;

		internal override JunctionType Type => JunctionType.Wires;

		public WireNetwork() : base(){
			OnClear += () => {
				Capacity = new TerraFlux(0f);
				totalExportedFlux = new TerraFlux(0f);
				needsRateRefresh = true;
			};
			OnEntryPlace += pos => {
				Tile tile = Framing.GetTileSafely(pos.X, pos.Y);
				var mTile = ModContent.GetModTile(tile.type);
				
				TerraFlux cap = new TerraFlux(0f);

				if(mTile is TFWireTile wire)
					cap = wire.Capacity;
				else if(mTile is TransportJunction junction)
					cap = ModContent.GetInstance<TFWireTile>().Capacity;
				else
					return;

				Capacity += cap;

				needsRateRefresh = true;
			};
			OnEntryKill += pos => {
				Tile tile = Framing.GetTileSafely(pos.X, pos.Y);
				var mTile = ModContent.GetModTile(tile.type);
				TerraFlux cap = new TerraFlux(0f);
				if(mTile is TFWireTile wire)
					cap = wire.Capacity;
				else if(mTile is TransportJunction junction)
					cap = ModContent.GetInstance<TFWireTile>().Capacity;
				else
					return;

				Capacity -= cap;
				StoredFlux -= cap;

				if((float)StoredFlux < 0)
					StoredFlux = new TerraFlux(0f);

				needsRateRefresh = true;
			};
			PostRefreshConnections += () => {
				if(StoredFlux > Capacity)
					StoredFlux = Capacity;
			};
		}

		public override TagCompound Save()
			=> new TagCompound(){
				["flux"] = (float)StoredFlux
			};

		public override void Load(TagCompound tag){
			StoredFlux = new TerraFlux(tag.GetFloat("flux"));

			RefreshConnections(NetworkCollection.ignoreCheckLocation);
		}

		public override TagCompound CombineSave()
			=> new TagCompound(){
				["flux"] = (float)StoredFlux
			};

		public override void LoadCombinedData(TagCompound up, TagCompound left, TagCompound right, TagCompound down){
			TerraFlux total = new TerraFlux(up?.GetFloat("flux") ?? 0);
			total += new TerraFlux(left?.GetFloat("flux") ?? 0);
			total += new TerraFlux(right?.GetFloat("flux") ?? 0);
			total += new TerraFlux(down?.GetFloat("flux") ?? 0);

			StoredFlux = total;
		}

		public override void SplitDataAcrossNetworks(Point16 splitOrig){
			float factor = (float)StoredFlux / (float)Capacity;

			if(NetworkCollection.HasWireAt(splitOrig + new Point16(0, -1), out WireNetwork upNet))
				upNet.StoredFlux = upNet.Capacity * factor;
			if(NetworkCollection.HasWireAt(splitOrig + new Point16(-1, 0), out WireNetwork leftNet))
				leftNet.StoredFlux = leftNet.Capacity * factor;
			if(NetworkCollection.HasWireAt(splitOrig + new Point16(1, 0), out WireNetwork rightNet))
				rightNet.StoredFlux = rightNet.Capacity * factor;
			if(NetworkCollection.HasWireAt(splitOrig + new Point16(0, 1), out WireNetwork downNet))
				downNet.StoredFlux = downNet.Capacity * factor;
		}

		private void RefreshRates(){
			if(!needsRateRefresh)
				return;

			needsRateRefresh = false;

			TerraFlux import = new TerraFlux(0f);
			TerraFlux export = new TerraFlux(0f);

			int count = 0;

			var inst = ModContent.GetInstance<TFWireTile>();

			foreach(var wire in Hash){
				ModTile tile = ModContent.GetModTile(Framing.GetTileSafely(wire.Position).type);

				if(tile is null)
					continue;

				if(tile is TransportJunction){
					import += inst.ImportRate;
					export += inst.ExportRate;
				}else if(tile is TFWireTile wireTile){
					import += wireTile.ImportRate;
					export += wireTile.ExportRate;
				}else
					continue;

				count++;
			}

			if(count == 0){
				ImportRate = new TerraFlux(0f);
				ExportRate = new TerraFlux(0f);
			}else{
				ImportRate = import / count;
				ExportRate = export / count;
			}
		}

		/// <summary>
		/// Sends the <paramref name="flux"/> from the <paramref name="source"/> machine to this network
		/// </summary>
		/// <param name="source">The source of the TF</param>
		/// <param name="flux">Terra Flux; the power unit for TerraScience machines</param>
		internal void ImportFlux(GeneratorEntity source, ref TerraFlux flux){
			RefreshRates();

			List<PoweredMachineEntity> machines = new List<PoweredMachineEntity>();

			foreach(PoweredMachineEntity machine in ConnectedMachines)
				if((!(machine is GeneratorEntity) || machine is Battery) && !object.ReferenceEquals(machine, source))
					machines.Add(machine);

			//More machines = less flux to each machine
			if(machines.Count > 0){
				TerraFlux receive = flux;

				//Too much power coming in.  Send the rest of it back to the machine
				if(receive > ImportRate){
					receive = ImportRate;
					flux -= receive;
				}else
					flux = new TerraFlux(0f);

				if(StoredFlux + receive <= Capacity){
					//Able to put energy into the system
					
					if(source.StoredFlux >= receive){
						//Able to remove energy from the machine
						source.StoredFlux -= receive;
					}else{
						receive = source.StoredFlux;
						source.StoredFlux = new TerraFlux(0f);
					}

					StoredFlux += receive;
				}else{
					//Energy would overflow.  Put the remainder back into the generator
					TerraFlux diff = Capacity - StoredFlux;
					StoredFlux = Capacity;

					source.StoredFlux -= diff;
				}
			}
		}

		/// <summary>
		/// Exports Terra Flux (TF) to the machines connected to this network
		/// </summary>
		internal void ExportFlux(){
			RefreshRates();

			//Send the flux to a machine, but only if it's not a generator
			List<PoweredMachineEntity> machines = new List<PoweredMachineEntity>();

			foreach(PoweredMachineEntity machine in ConnectedMachines)
				if(!(machine is GeneratorEntity) || machine is Battery)
					machines.Add(machine);

			if(machines.Count > 0){
				foreach(var machine in machines){
					float export = Math.Max((float)ExportRate, (float)(machine is Battery battery ? battery.ImportRate : machine.FluxUsage));
					TerraFlux send = new TerraFlux(Math.Min((float)StoredFlux, export));

					TerraFlux origSend = send;

					if((float)send <= 0f)
						break;

					StoredFlux -= send;

					machine.ImportFlux(ref send);

					totalExportedFlux += origSend - send;

					StoredFlux += send;
				}
			}

			if(StoredFlux > Capacity)
				StoredFlux = Capacity;
		}

		public override INetwork Clone()
			=> new WireNetwork(){
				Hash = new HashSet<TFWire>(this.Hash),
				ConnectedMachines = new List<MachineEntity>(ConnectedMachines),
				needsRateRefresh = true
			};

		public override string ToString() => $"ID: {ID}, Flux: {StoredFlux} / {Capacity} TF, Exported Flux: {totalExportedFlux} TF";
	}
}
