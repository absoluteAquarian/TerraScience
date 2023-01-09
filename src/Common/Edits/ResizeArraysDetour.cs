using MonoMod.RuntimeDetour.HookGen;
using SerousCommonLib.API;
using System.Reflection;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;

namespace TerraScience.Common.Edits {
	public sealed class ResizeArraysDetour : Edit {
		private delegate void orig_ModContent_ResizeArrays(bool unloading);
		private delegate void hook_ModContent_ResizeArrays(orig_ModContent_ResizeArrays orig, bool unloading);
		private static readonly MethodInfo ModContent_ResizeArrays = typeof(ModContent).GetMethod("ResizeArrays", BindingFlags.NonPublic | BindingFlags.Static);

		private static event hook_ModContent_ResizeArrays On_ModContent_ResizeArrays {
			add => HookEndpointManager.Add<hook_ModContent_ResizeArrays>(ModContent_ResizeArrays, value);
			remove => HookEndpointManager.Remove<hook_ModContent_ResizeArrays>(ModContent_ResizeArrays, value);
		}

		public override void LoadEdits() {
			On_ModContent_ResizeArrays += Hook_ModContent_ResizeArrays;
		}

		public override void UnloadEdits() {
			On_ModContent_ResizeArrays -= Hook_ModContent_ResizeArrays;
		}

		private static void Hook_ModContent_ResizeArrays(orig_ModContent_ResizeArrays orig, bool unloading) {
			orig(unloading);

			LoaderUtils.ResetStaticMembers(typeof(TechMod.Sets), true);
		}
	}
}
