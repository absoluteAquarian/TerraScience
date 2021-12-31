using Microsoft.Xna.Framework;
using Terraria.ModLoader;

namespace TerraScience.Content.Commands{
	public class TesseractNetworkAdmin : ModCommand{
		public override string Command => "tnadmin";

		public override string Description => "Gives/removes the Administrator status on the local player for Tesseract Networks.";

		public override string Usage => "/tnadmin";

		public override CommandType Type => CommandType.Chat;

		public override void Action(CommandCaller caller, string input, string[] args){
			bool release = TechMod.Release;
			if(release){
				caller.Reply("This command can only be used in Debug mode.", Color.Red);
				return;
			}

			ref bool admin = ref caller.Player.GetModPlayer<TerraSciencePlayer>().tesseractAdmin;
			admin = !admin;

			caller.Reply("Tesseract Network Administrator privileges have been [c/" + (admin ? "00ff00:granted" : "ff0000:removed") + "].");
		}
	}
}
