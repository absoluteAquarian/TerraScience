using System.Diagnostics;
using Terraria.ModLoader;

namespace TerraScience.Common.CrossMod {
	/// <summary>
	/// An object representing a possibly-loaded mod
	/// </summary>
	[DebuggerDisplay("{DebuggerDisplayString,nq}")]
	public sealed class ModReference {
		public readonly Mod Mod;
		public readonly string Name;

		internal string DebuggerDisplayString => $"Mod: {Name}, Loaded: {Mod is not null}";

		public ModReference(string mod) {
			Name = mod;
			ModLoader.TryGetMod(mod, out Mod);
		}
	}
}
