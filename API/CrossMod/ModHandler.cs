using Terraria.ModLoader;

namespace TerraScience.API.CrossMod{
	public class ModHandler{
		public Mod Instance{ get; private set; }

		public bool ModIsActive => Instance != null;

		public void Load(string mod){
			Instance = ModLoader.GetMod(mod);
		}

		public void Unload(){
			Instance = null;
		}
	}
}
