﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;
using TerraScience.Content.TileEntities;

namespace TerraScience.Content.UI {

    public class MachineUILoader : ModSystem {
		private Dictionary<string, UserInterface> interfaces;
		private Dictionary<string, MachineUI> states;

		public UserInterface tesseractNetworkInterface;
		public TesseractNetworkUI tesseractNetworkState;

		public UserInterface GetInterface(string name) => interfaces[name];
		public T GetState<T>(string name) where T : MachineUI => states[name] as T;

		//Public so i can access it in ReinforcedFurnaceEntity
		public GameTime lastUpdateUIGameTime;

		public static bool LeftClick => curMouse.LeftButton == ButtonState.Pressed && oldMouse.LeftButton == ButtonState.Released;

		internal static MouseState oldMouse;
		internal static MouseState curMouse;

        public static MachineUILoader Instance => ModContent.GetInstance<MachineUILoader>();


        /// <summary>
        /// Called on Mod.Load
        /// </summary>
        public override void Load() {
			if (!Main.dedServ) {
				interfaces = new Dictionary<string, UserInterface>();
				states = new Dictionary<string, MachineUI>();

                var types = TechMod.types;
				foreach(var type in types){
					//Ignore abstract classes
					if(type.IsAbstract)
						continue;

					if(typeof(MachineUI).IsAssignableFrom(type)){
						//Not abstract and doesn't have a parameterless ctor?  Throw an exception
						if(type.GetConstructor(Type.EmptyTypes) is null)
							throw new TypeLoadException($"Machine UI type \"{type.FullName}\" does not have a parameterless constructor.");

						//Machine UI type.  Try to link the UI to the machine
						MachineUI state = Activator.CreateInstance(type) as MachineUI;
						ModTile machine = ModContent.GetModTile(state.TileType);

						if(machine is null)
							throw new TypeLoadException($"Machine UI type \"{type.FullName}\" had an invalid return value for its \"TileType\" getter property.");

						//Register the UI
						string name = machine.GetType().Name;
						state.MachineName = name;
						interfaces.Add(name, new UserInterface());
						states.Add(name, state);
					}
				}

				// Activate calls Initialize() on the UIState if not initialized, then calls OnActivate and then calls Activate on every child element
				foreach(var state in states.Values)
					state.Activate();

				tesseractNetworkInterface = new UserInterface();
				tesseractNetworkState = new TesseractNetworkUI();

				tesseractNetworkState.Activate();
			}
		}

		public event Action OnUpdateOnce;

		/// <summary>
		/// Called on Mod.UpdateUI
		/// </summary>
		public override void UpdateUI(GameTime gameTime) {
			lastUpdateUIGameTime = gameTime;

			oldMouse = curMouse;
			curMouse = Mouse.GetState();

			foreach(var ui in interfaces.Values)
				if (ui?.CurrentState != null)
					ui.Update(gameTime);

			tesseractNetworkInterface?.Update(gameTime);

			if(OnUpdateOnce != null){
				OnUpdateOnce();
				OnUpdateOnce = null;
			}
		}

		/// <summary>
		/// Called on Mod.ModifyInterfaceLayers
		/// </summary>
		public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers) {
			int mouseTextIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));

			if (mouseTextIndex != -1) {
				layers.Insert(mouseTextIndex - 1, new LegacyGameInterfaceLayer(
					"TerraScience: Machine UI",
					delegate {
						foreach(var ui in interfaces.Values)
							if (lastUpdateUIGameTime != null && ui.CurrentState != null)
								ui.Draw(Main.spriteBatch, lastUpdateUIGameTime);

						if(tesseractNetworkInterface.CurrentState != null)
							tesseractNetworkInterface.Draw(Main.spriteBatch, lastUpdateUIGameTime);

						return true;
					}, InterfaceScaleType.UI));
			}
		}

		/// <summary>
		/// Called on Mod.Unload
		/// </summary>
		public override void Unload(){
			interfaces = null;
			states = null;

			tesseractNetworkInterface = null;
			tesseractNetworkState = null;
		}

		public void ShowUI(string name, MachineEntity entity) {
			MachineUI machineState = states[name];

			machineState.PlayOpenSound();

			machineState.PreOpen();

			machineState.Active = true;
			machineState.UIEntity = entity;
			machineState.UIEntity.ParentState = machineState;
			machineState.UIEntity.MachineName = machineState.MachineName;
			machineState.UIDelay = 10;

			if(!machineState.CheckedForSavedItemCount){
				machineState.CheckedForSavedItemCount = true;
				
				machineState.DoSavedItemsCheck();
			}

			machineState.UIEntity.LoadSlots();

			interfaces[name].SetState(machineState);
			interfaces[name].IsVisible = true;

			machineState.PostOpen();
		}

		public void HideUI(string name) {
			MachineUI machineState = states[name];

			machineState.IsClosing = true;

			machineState.PlayCloseSound();

			machineState.PreClose();

			machineState.UIEntity.SaveSlots();
			machineState.ClearSlots();
			machineState.UIEntity.ParentState = null;
			machineState.UIEntity = null;

			machineState.CheckedForSavedItemCount = false;
			machineState.Active = false;

			interfaces[name].SetState(null);
            interfaces[name].IsVisible = false;

            machineState.PostClose();

			machineState.IsClosing = false;
		}
	}
}