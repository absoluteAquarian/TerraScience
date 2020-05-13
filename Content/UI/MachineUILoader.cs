using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;
using TerraScience.API.UI;
using TerraScience.Content.API.UI;
using TerraScience.Content.TileEntities;
using TerraScience.Utilities;

namespace TerraScience.Content.UI {
	public class MachineUILoader {
		private Dictionary<string, UserInterface> interfaces;
		private Dictionary<string, MachineUI> states;

		public UserInterface SaltExtractorInterface => GetInterface(nameof(TileUtils.Structures.SaltExtractor));
		public SaltExtractorUI SaltExtractorState => GetState<SaltExtractorUI>(nameof(TileUtils.Structures.SaltExtractor));
		public UserInterface ScienceWorkbenchInterface => GetInterface(nameof(TileUtils.Structures.ScienceWorkbench));
		public ScienceWorkbenchUI ScienceWorkbenchState => GetState<ScienceWorkbenchUI>(nameof(TileUtils.Structures.ScienceWorkbench));

		public UserInterface GetInterface(string name) => interfaces[name];
		public T GetState<T>(string name) where T : MachineUI => states[name] as T;

		private GameTime lastUpdateUIGameTime;

		public static bool LeftClick => curMouse.LeftButton == ButtonState.Pressed && oldMouse.LeftButton == ButtonState.Released;

		private static MouseState oldMouse;
		private static MouseState curMouse;

		/// <summary>
		/// Called on Mod.Load
		/// </summary>
		public void Load() {
			if (!Main.dedServ) {
				interfaces = new Dictionary<string, UserInterface>();
				states = new Dictionary<string, MachineUI>();

				AddUI(nameof(TileUtils.Structures.SaltExtractor), new SaltExtractorUI());
				AddUI(nameof(TileUtils.Structures.ScienceWorkbench), new ScienceWorkbenchUI());

				// Activate calls Initialize() on the UIState if not initialized, then calls OnActivate and then calls Activate on every child element
				foreach(var state in states.Values)
					state.Activate();
			}
		}

		private void AddUI(string name, MachineUI state){
			state.MachineName = name;
			interfaces.Add(name, new UserInterface());
			states.Add(name, state);
		}

		/// <summary>
		/// Called on Mod.UpdateUI
		/// </summary>
		public void UpdateUI(GameTime gameTime) {
			lastUpdateUIGameTime = gameTime;

			oldMouse = curMouse;
			curMouse = Mouse.GetState();

			foreach(var ui in interfaces.Values)
				if (ui?.CurrentState != null)
					ui.Update(gameTime);
		}

		/// <summary>
		/// Called on Mod.ModifyInterfaceLayers
		/// </summary>
		public void ModifyInterfaceLayers(List<GameInterfaceLayer> layers) {
			int mouseTextIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));

			if (mouseTextIndex != -1) {
				layers.Insert(mouseTextIndex, new LegacyGameInterfaceLayer(
					"TerraScience: Machine UI",
					delegate {
						foreach(var ui in interfaces.Values)
							if (lastUpdateUIGameTime != null && ui.CurrentState != null)
								ui.Draw(Main.spriteBatch, lastUpdateUIGameTime);

						return true;
					}, InterfaceScaleType.UI));
			}
		}

		/// <summary>
		/// Called on Mod.Unload
		/// </summary>
		public void Unload(){
			interfaces = null;
			states = null;
		}

		public void ShowUI(string name, MachineEntity entity) {
			Main.PlaySound(SoundID.MenuOpen);

			MachineUI machineState = states[name];

			machineState.Active = true;
			machineState.UIEntity = entity;
			machineState.UIEntity.ParentState = machineState;

			if(!machineState.CheckedForSavedItemCount){
				machineState.CheckedForSavedItemCount = true;
				
				machineState.DoSavedItemsCheck();
			}

			interfaces[name].SetState(states[name]);
		}

		public void HideUI(string name) {
			Main.PlaySound(SoundID.MenuClose);

			MachineUI machineState = states[name];

			machineState.PreClose();
			machineState.CheckedForSavedItemCount = false;
			machineState.Active = false;

			interfaces[name].SetState(null);
		}
	}
}