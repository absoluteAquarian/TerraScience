﻿using Microsoft.Xna.Framework;
using System.Linq;
using Terraria.DataStructures;
using Terraria.ModLoader;
using TerraScience.Content.TileEntities;

namespace TerraScience.Content.Commands{
	public class TileEntityButcherer : ModCommand{
		public override string Command => "tsbutcher";

		public override string Description => "Butchers all TerraScience tile entities.";

		public override string Usage => "/tsbutcher <type|all>";

		public override CommandType Type => CommandType.Chat;

		public override void Action(CommandCaller caller, string input, string[] args){
			bool release = TechMod.Release;
			if(release){
				caller.Reply("This command can only be used in Debug mode.", Color.Red);
				return;
			}

			//Only one parameter can be given
			if(args.Length != 1){
				caller.Reply("Not enough or too many parameters were given.", Color.Red);
				return;
			}

			//...and that parameter must either be the class name of a ModTileEntity in this mod or "all", specifying
			// that ALL TerraScience entities are to be killed.
			if(args[0] == "all"){
				for(int i = 0; i < TileEntity.ByPosition.Count; i++){
					var te = TileEntity.ByPosition.ElementAt(i);
					if(te.Value is MachineEntity me)
						me.Kill(te.Key.X, te.Key.Y);
				}

				caller.Reply("Success! All TerraScience tile entities were killed.", Color.Green);
			}else{
				var entityType = TechMod.Instance.Find<ModTileEntity>(args[0]);

				//We tried to get an invalid ModTileEntity.  Tell the player
				if(entityType is null){
					caller.Reply("Name of ModTileEntity provided does not exist in TerraScience!", Color.Red);
					return;
				}

				//Kill all entities with this name
				for(int i = 0; i < TileEntity.ByPosition.Count; i++){
					var te = TileEntity.ByPosition.ElementAt(i);
					if(te.Value.GetType() == entityType.GetType())
						(te.Value as ModTileEntity).Kill(te.Key.X, te.Key.Y);
				}

				caller.Reply($"Success! All TerraScience tile entities with the name {args[0]} were killed.", Color.Green);
			}
		}
	}
}