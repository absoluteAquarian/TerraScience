/*using System;
using TerraScience.API.CrossMod.MagicStorage;

namespace TerraScience.API.Edits.Detours{
	public static partial class TML{
		internal static void TileIO_SaveTileEntities(Action orig){
			MagicStorageHandler.DelayInteractionsDueToWorldSaving = true;

			orig();

			MagicStorageHandler.DelayInteractionsDueToWorldSaving = false;
		}
	}
}
*/