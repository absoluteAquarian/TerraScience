using SerousEnergyLib.API.Machines;
using SerousEnergyLib.API.Machines.UI;
using System;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace TerraScience.Common.Systems {
	/// <summary>
	/// The central class for storing and retrieving machine UIs
	/// </summary>
	public class MachineUISingletons : ModSystem {
		private static readonly Dictionary<int, BaseMachineUI> machineUIs = new();

		/// <summary>
		/// Registers a <see cref="BaseMachineUI"/> instance and ties it to a machine entity type
		/// </summary>
		/// <param name="ui">The instance</param>
		public static void RegisterUI<T>(BaseMachineUI ui) where T : ModTileEntity, IMachine {
			ArgumentNullException.ThrowIfNull(ui);

			machineUIs[ModContent.TileEntityType<T>()] = ui;
		}

		/// <inheritdoc cref="RegisterUI{T}(BaseMachineUI)"/>
		/// <param name="type">The ID of the machine entity to tie the <see cref="BaseMachineUI"/> instance to</param>
		public static void RegisterUI(int type, BaseMachineUI ui) {
			if (!TileEntity.manager.TryGetTileEntity(type, out ModTileEntity entity) || entity is not IMachine)
				throw new ArgumentException("Specified type was not a ModTileEntity or was not an IMachine", nameof(type));

			ArgumentNullException.ThrowIfNull(ui);

			machineUIs[type] = ui;
		}

		/// <summary>
		/// Retrieves the <see cref="BaseMachineUI"/> instance tied to a machine entity type
		/// </summary>
		/// <returns>A valid <see cref="BaseMachineUI"/> if one was registered for this machine entity type, <see langword="null"/> otherwise.</returns>
		public static BaseMachineUI GetInstance<T>() where T : ModTileEntity, IMachine => machineUIs.TryGetValue(ModContent.TileEntityType<T>(), out var ui) ? ui : null;

		/// <inheritdoc cref="GetInstance{T}"/>
		/// <param name="type">The ID of the machine entity to retrieve the <see cref="BaseMachineUI"/> instance from</param>
		public static BaseMachineUI GetInstance(int type) {
			if (!TileEntity.manager.TryGetTileEntity(type, out ModTileEntity entity) || entity is not IMachine)
				throw new ArgumentException("Specified type was not a ModTileEntity or was not an IMachine", nameof(type));

			return machineUIs.TryGetValue(type, out var ui) ? ui : null;
		}

		public override void Unload() {
			machineUIs.Clear();
		}
	}
}
