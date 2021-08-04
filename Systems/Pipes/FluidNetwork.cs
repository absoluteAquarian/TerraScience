﻿using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using TerraScience.API.Interfaces;
using TerraScience.Content.ID;
using TerraScience.Content.TileEntities;
using TerraScience.Content.Tiles;
using TerraScience.Utilities;

namespace TerraScience.Systems.Pipes{
	public class FluidNetwork : Network<FluidPipe, FluidTransportTile>{
		internal override JunctionType Type => JunctionType.Fluids;

		// TODO: make these not enums for fluids from other mods
		public MachineGasID gasType;
		public MachineLiquidID liquidType;
		
		public List<Point16> pipesConnectedToMachines;

		internal Dictionary<Point16, Timer> pumpTimers;

		public float StoredFluid{ get; internal set; }

		public float Capacity{ get; internal set; }

		public FluidNetwork() : base(){
			pipesConnectedToMachines = new List<Point16>();
			pumpTimers = new Dictionary<Point16, Timer>();

			OnEntryPlace += pos => {
				Tile tile = Framing.GetTileSafely(pos);

				if(ModContent.GetModTile(tile.type) is FluidPumpTile){
					Capacity += ModContent.GetInstance<FluidTransportTile>().Capacity;

					if(!pumpTimers.ContainsKey(pos))
						pumpTimers.Add(pos, new Timer(){ value = 34 });
				}else if(ModContent.GetModTile(tile.type) is FluidTransportTile transport)
					Capacity += transport.Capacity;

				if(TileEntityUtilities.TryFindMachineEntity(pos + new Point16(0, -1), out MachineEntity entity) && (entity is ILiquidMachine || entity is IGasMachine))
					pipesConnectedToMachines.Add(pos);

				if(TileEntityUtilities.TryFindMachineEntity(pos + new Point16(-1, 0), out entity) && (entity is ILiquidMachine || entity is IGasMachine)){
					if(!pipesConnectedToMachines.Contains(pos))
						pipesConnectedToMachines.Add(pos);
				}

				if(TileEntityUtilities.TryFindMachineEntity(pos + new Point16(1, 0), out entity) && (entity is ILiquidMachine || entity is IGasMachine)){
					if(!pipesConnectedToMachines.Contains(pos))
						pipesConnectedToMachines.Add(pos);
				}

				if(TileEntityUtilities.TryFindMachineEntity(pos + new Point16(0, 1), out entity) && (entity is ILiquidMachine || entity is IGasMachine)){
					if(!pipesConnectedToMachines.Contains(pos))
						pipesConnectedToMachines.Add(pos);
				}
			};

			OnEntryKill += pos => {
				Tile tile = Framing.GetTileSafely(pos);
				float cap = 0;
				if(ModContent.GetModTile(tile.type) is FluidPumpTile){
					cap = ModContent.GetInstance<FluidTransportTile>().Capacity;

					pumpTimers.Remove(pos);
				}else if(ModContent.GetModTile(tile.type) is FluidTransportTile transport)
					cap += transport.Capacity;

				Capacity -= cap;
				StoredFluid -= cap;

				pipesConnectedToMachines.Remove(pos);
			};

			PostRefreshConnections += () => {
				if(StoredFluid > Capacity)
					StoredFluid = Capacity;
			};
		}

		public List<(Point16, short)> SavePumpDirs(){
			var pumps = GetEntries().Where(e => ModContent.GetModTile(Framing.GetTileSafely(e.Position).type) is FluidPumpTile).ToList();

			return pumps.Count == 0 ? null : pumps.Select(e => (e.Position, (short)(Framing.GetTileSafely(e.Position).frameX / 18))).ToList();
		}

		public override TagCompound Save(){
			var dirs = SavePumpDirs();

			return new TagCompound(){
				["gas"] = gasType.EnumName(),
				["liquid"] = liquidType.EnumName(),
				["stored"] = StoredFluid,
				["pumpPositions"] = dirs?.Count > 0 ? dirs.Select(t => t.Item1).ToList() : null,
				["pumpDirs"] = dirs?.Count > 0 ? dirs.Select(t => t.Item2).ToList() : null,
				["pumpTimerLocations"] = pumpTimers.Keys.ToList(),
				["pumpTimerValues"] = pumpTimers.Values.Select(t => (byte)t.value).ToList()
			};
		}

		public override void Load(TagCompound tag){
			if(tag.GetString("gas") is string gasName)
				gasType = (MachineGasID)Enum.Parse(typeof(MachineGasID), gasName);
			else
				gasType = MachineGasID.None;

			if(tag.GetString("liquid") is string liquidName)
				liquidType = (MachineLiquidID)Enum.Parse(typeof(MachineLiquidID), liquidName);
			else
				liquidType = MachineLiquidID.None;

			StoredFluid = tag.GetFloat("stored");

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
		}

		public override TagCompound CombineSave()
			=> new TagCompound(){
				["gas"] = gasType.EnumName(),
				["liquid"] = liquidType.EnumName(),
				["stored"] = StoredFluid,
				["pumpTimerLocations"] = pumpTimers.Keys.ToList(),
				["pumpTimerValues"] = pumpTimers.Values.Select(t => (byte)t.value).ToList()
			};

		public override void LoadCombinedData(TagCompound up, TagCompound left, TagCompound right, TagCompound down){
			if(up?.GetString("gas") is string upGas)
				gasType = (MachineGasID)Enum.Parse(typeof(MachineGasID), upGas);
			else if(left?.GetString("gas") is string leftGas)
				gasType = (MachineGasID)Enum.Parse(typeof(MachineGasID), leftGas);
			else if(right?.GetString("gas") is string rightGas)
				gasType = (MachineGasID)Enum.Parse(typeof(MachineGasID), rightGas);
			else if(down?.GetString("gas") is string downGas)
				gasType = (MachineGasID)Enum.Parse(typeof(MachineGasID), downGas);

			if(up?.GetString("liquid") is string upLiquid)
				liquidType = (MachineLiquidID)Enum.Parse(typeof(MachineLiquidID), upLiquid);
			else if(left?.GetString("liquid") is string leftLiquid)
				liquidType = (MachineLiquidID)Enum.Parse(typeof(MachineLiquidID), leftLiquid);
			else if(right?.GetString("liquid") is string rightLiquid)
				liquidType = (MachineLiquidID)Enum.Parse(typeof(MachineLiquidID), rightLiquid);
			else if(down?.GetString("liquid") is string downLiquid)
				liquidType = (MachineLiquidID)Enum.Parse(typeof(MachineLiquidID), downLiquid);

			StoredFluid = (up?.GetFloat("stored") ?? 0)
				+ (left?.GetFloat("stored") ?? 0)
				+ (right?.GetFloat("stored") ?? 0)
				+ (down?.GetFloat("stored") ?? 0);

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

		public override bool CanCombine(Point16 orig, Point16 dir){
			NetworkCollection.HasFluidPipeAt(orig + dir, out FluidNetwork thisNet);
			NetworkCollection.HasFluidPipeAt(orig - dir, out FluidNetwork otherNet);
			var otherAxis = new Point16(dir.Y, dir.X);
			NetworkCollection.HasFluidPipeAt(orig + otherAxis, out FluidNetwork axisNet);
			NetworkCollection.HasFluidPipeAt(orig - otherAxis, out FluidNetwork otherAxisNet);

			Tile center = Framing.GetTileSafely(orig);
			if(ModContent.GetModTile(center.type) is TransportJunction){
				//At this point, only one axis is being handled, so that's fine
				JunctionMerge merge = JunctionMergeable.mergeTypes[center.frameX / 18];

				if(((merge & JunctionMerge.Fluids_LeftRight) != 0 && dir.X != 0) || ((merge & JunctionMerge.Fluids_UpDown) != 0 && dir.Y != 0))
					return otherNet is null || (otherNet.liquidType == thisNet.liquidType && otherNet.gasType == thisNet.gasType);
				return false;
			}

			//Not a junction at the center.  Need to check every direction
			//All directions just need to be checked against this one
			bool hasConnOther = otherNet != null;
			bool hasConnAxis = axisNet != null;
			bool hasConnAxisOther = otherAxisNet != null;

			return (!hasConnOther || (otherNet.liquidType == thisNet.liquidType && otherNet.gasType == thisNet.gasType))
				&& (!hasConnAxis || (axisNet.liquidType == thisNet.liquidType && axisNet.gasType == thisNet.gasType))
				&& (!hasConnAxisOther || (otherAxisNet.liquidType == thisNet.liquidType && otherAxisNet.gasType == thisNet.gasType));
		}

		public override void SplitDataAcrossNetworks(Point16 splitOrig){
			if(Capacity <= 0)
				return;  //Uh oh

			float factor = StoredFluid / Capacity;

			if(NetworkCollection.HasFluidPipeAt(splitOrig + new Point16(0, -1), out FluidNetwork up))
				up.StoredFluid = up.Capacity * factor;
			if(NetworkCollection.HasFluidPipeAt(splitOrig + new Point16(-1, 0), out FluidNetwork left))
				left.StoredFluid = left.Capacity * factor;
			if(NetworkCollection.HasFluidPipeAt(splitOrig + new Point16(1, 0), out FluidNetwork right))
				right.StoredFluid = right.Capacity * factor;
			if(NetworkCollection.HasFluidPipeAt(splitOrig + new Point16(0, 1), out FluidNetwork down))
				down.StoredFluid = down.Capacity * factor;
		}

		public override INetwork Clone()
			=> new FluidNetwork(){
				Hash = new HashSet<FluidPipe>(this.Hash),
				ConnectedMachines = new List<MachineEntity>(this.ConnectedMachines)
			};

		public override string ToString() => $"ID: {ID}, Fluid: {StoredFluid} / {Capacity} L, Gas Type: {gasType}, Liquid Type: {liquidType}";
	}
}
