using System;
using System.Collections.Generic;
using Terraria.ModLoader.IO;
using TerraScience.API.CrossMod.MagicStorage;

namespace TerraScience.API.Edits.Detours{
	public static partial class TML{
		internal static List<TagCompound> TileIO_SaveTileEntities(Func<List<TagCompound>> orig){
			MagicStorageHandler.DelayInteractionsDueToWorldSaving = true;

			var list = orig();

			MagicStorageHandler.DelayInteractionsDueToWorldSaving = false;

			return list;
		}
	}
}
