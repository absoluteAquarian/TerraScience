using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using Terraria;
using Terraria.UI;
using TerraScience.Content.TileEntities;
using TerraScience.Content.Tiles.Multitiles;
using TerraScience.Content.Tiles.Multitiles.EnergyMachines;
using TerraScience.Content.Tiles.Multitiles.EnergyMachines.Basic;
using TerraScience.Content.Tiles.Multitiles.EnergyMachines.Storage;
using TerraScience.Content.UI.Energy;
using TerraScience.Content.UI.Energy.Generators;
using TerraScience.Content.UI.Energy.Storage;
using TerraScience.Utilities;

namespace TerraScience.Content.UI {
	public class MachineUILoader {
		private Dictionary<string, UserInterface> interfaces;
		private Dictionary<string, MachineUI> states;

		public UserInterface GetInterface(string name) => interfaces[name];
		public T GetState<T>(string name) where T : MachineUI => states[name] as T;

		//Public so i can access it in ReinforcedFurnaceEntity
		public GameTime lastUpdateUIGameTime;

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

				AddUI(nameof(SaltExtractor), new SaltExtractorUI());
				AddUI(nameof(ScienceWorkbench), new ScienceWorkbenchUI());
				AddUI(nameof(ReinforcedFurnace), new ReinforcedFurnaceUI());
				AddUI(nameof(BlastFurnace), new BlastFurnaceUI());
				AddUI(nameof(AirIonizer), new AirIonizerUI());
				AddUI(nameof(Electrolyzer), new ElectrolyzerUI());
				AddUI(nameof(BasicWindTurbine), new BasicWindTurbineUI());
				AddUI(nameof(BasicBattery), new BasicBatteryUI());
				AddUI(nameof(AutoExtractinator), new AutoExtractinatorUI());
				AddUI(nameof(BasicSolarPanel), new BasicSolarPanelUI());
				AddUI(nameof(Greenhouse), new GreenhouseUI());

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
			MachineUI machineState = states[name];

			machineState.PlayOpenSound();

			machineState.Active = true;
			machineState.UIEntity = entity;
			machineState.UIEntity.ParentState = machineState;
			machineState.UIEntity.MachineName = machineState.MachineName;
			machineState.UIDelay = 10;

			if(!machineState.CheckedForSavedItemCount){
				machineState.CheckedForSavedItemCount = true;
				
				machineState.DoSavedItemsCheck();

				machineState.UIEntity.LoadSlots();
			}

			interfaces[name].SetState(machineState);
		}

		public void HideUI(string name) {
			MachineUI machineState = states[name];

			machineState.PlayCloseSound();

			machineState.PreClose();

			machineState.UIEntity.SaveSlots();
			machineState.ClearSlots();
			machineState.UIEntity.ParentState = null;
			machineState.UIEntity = null;

			machineState.CheckedForSavedItemCount = false;
			machineState.Active = false;

			interfaces[name].SetState(null);
		}
	}
}