using Terraria.ModLoader;

namespace TerraScience.API.CrossMod{
	public class ModHandler{
		public Mod Instance{ get; private set; }

		public bool ModIsActive => Instance != null;

		public void Load(string mod){
			Instance = ModLoader.TryGetMod(mod, out Mod instance) ? instance : null;
		}

		public void Unload(){
			Instance = null;
		}
	}
}
