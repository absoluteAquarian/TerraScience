using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.ModLoader.IO;
using TerraScience.Content.TileEntities;
using TerraScience.Content.Tiles;

namespace TerraScience.API.Interfaces{
	public interface INetwork{
		TagCompound Save();
		void Load(TagCompound tag);

		void InvokePostEntryKill();
	}

	public interface INetwork<T> : INetwork where T : struct, INetworkable<T>{
		HashSet<T> Hash{ get; set; }
		List<MachineEntity> ConnectedMachines{ get; set; }

		int ID{ get; }

		void RefreshConnections(Point16 ignoreLocation);

		void AddEntry(T entry);
		void RemoveEntry(T entry);
		void RemoveEntryAt(Point16 location);

		void AddMachine(MachineEntity entity);
		void RemoveMachine(MachineEntity entity);

		List<T> GetEntries();

		bool HasEntry(T entry);
		bool HasEntryAt(Point16 location);

		void Cleanup();
		void GetMergeInfo(out JunctionMerge leftRight, out JunctionMerge upDown, out JunctionMerge all);

		TagCompound CombineSave();
		void LoadCombinedData(TagCompound up, TagCompound left, TagCompound right, TagCompound down);
		void SplitDataAcrossNetworks(Point16 splitOrig);
	}
}
