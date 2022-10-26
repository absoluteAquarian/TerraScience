using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.RuntimeDetour.HookGen;
using System;
using System.Collections.Generic;
using System.Reflection;
using Terraria.ModLoader;
//using TerraScience.API.CrossMod.MagicStorage;

namespace TerraScience.API.Edits.Direct{
	public static class Manager{
		private static readonly List<Hook> detours = new List<Hook>();
		private static readonly List<(MethodInfo, Delegate)> delegates = new List<(MethodInfo, Delegate)>();

		private static readonly Dictionary<string, MethodInfo> cachedMethods = new Dictionary<string, MethodInfo>();

		public static void Load(){
			/*
			try{
				MonoModHooks.RequestNativeAccess();

				//Usage:  Making absolutely sure that Magic Storage systems can't be interacted with while the world is saving stuff
				if(MagicStorageHandler.handler.ModIsActive){
					DetourHook(typeof(Mod).Assembly.GetType("Terraria.ModLoader.IO.TileIO", throwOnError: true).GetCachedMethod("SaveTileEntities"),
						typeof(Detours.TML).GetCachedMethod(nameof(Detours.TML.TileIO_SaveTileEntities)));
				}
			}catch(Exception ex){
				throw new Exception("An error occurred while doing patching in TerraScience." +
					"\nReport this error to the mod devs and disable the mod in the meantime." +
					"\n\n\n" + ex.ToString());
			}
			*/
		}

		private static MethodInfo GetCachedMethod(this Type type, string method){
			string key = $"{type.FullName}::{method}";
			if(cachedMethods.TryGetValue(key, out MethodInfo value))
				return value;

			return cachedMethods[key] = type.GetMethod(method, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
		}

		public static void Unload(){
			foreach(var hook in detours)
				hook.Undo();

			foreach((MethodInfo method, Delegate hook) in delegates)
				HookEndpointManager.Unmodify(method, hook);
		}

		private static void IntermediateLanguageHook(MethodInfo orig, MethodInfo modify){
			Delegate hook = Delegate.CreateDelegate(typeof(ILContext.Manipulator), modify);
			delegates.Add((orig, hook));
			HookEndpointManager.Modify(orig, hook);
		}

		private static void DetourHook(MethodInfo orig, MethodInfo modify){
			Hook hook = new Hook(orig, modify);
			detours.Add(hook);
			hook.Apply();
		}
	}
}
