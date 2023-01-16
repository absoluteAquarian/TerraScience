using Microsoft.Xna.Framework;
using SerousEnergyLib.API.Fluid;
using SerousEnergyLib.API.Machines;
using SerousEnergyLib.API.Machines.UI;
using SerousEnergyLib.Systems;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Localization;
using Terraria.UI;
using TerraScience.Common.UI.Elements;
using TerraScience.Content.MachineEntities;

namespace TerraScience.Common.UI.Machines {
	public class FluidTankUI : BaseMachineUI {
		public override string DefaultPage => "Storage";
		public override bool IsUpgradesPageOpen => CurrentPage is BasicUpgradesPage;

		protected override LocalizedText GetMenuOptionLocalization(string key) {
			if (key == "Upgrades")
				return Language.GetText("Mods.TerraScience.MachineText.DefaultPage.Upgrades");

			return Language.GetText("Mods.TerraScience.MachineText.FluidTank.Page." + key);
		}

		protected override IEnumerable<string> GetMenuOptions() {
			yield return "Storage";
			yield return "Upgrades";
		}

		protected override BaseMachineUIPage InitPage(string page) {
			return page switch {
				"Storage" => new MainPage(this),
				"Upgrades" => new BasicUpgradesPage(this),
				_ => throw new ArgumentException("Unknown page: " + page, nameof(page))
			};
		}

		public override void Refresh() {
			if (UIHandler.ActiveMachine is not IMachine machine)
				return;

			if (GetPage<BasicUpgradesPage>("Upgrades") is BasicUpgradesPage page)
				page.Refresh(machine.Upgrades.Count + 1, maxSlotsPerRow: 7);
		}

		public override void GetDefaultPanelDimensions(out int width, out int height) {
			width = 460;
			height = 400;
		}

		public class MainPage : BaseMachineUIPage {
			public FluidGauge gauge;

			public MachineInventoryItemSlot fluidImportInput;
			public MachineInventoryItemSlot fluidImportOutput;
			public MachineInventoryItemSlot fluidExportInput;
			public MachineInventoryItemSlot fluidExportOutput;

			public MainPage(BaseMachineUI parent) : base(parent, "Storage") { }

			public override void OnInitialize() {
				// Max will be overwritten during runtime
				gauge = new FluidGauge(1d, pixelWidth: 250, pixelHeight: 200) {
					HAlign = 0.5f,
					VAlign = 0.5f
				};

				Append(gauge);

				fluidImportInput = new MachineInventoryItemSlot(0, ItemSlot.Context.BankItem) {
					ValidItemFunc = static item => item.IsAir || FluidTankEntity.ItemIsValidForImport(UIHandler.ActiveMachine as FluidTankEntity, item, insertionSlot: true),
					VAlign = 0.35f
				};
				fluidImportInput.OnUpdateItem += UpdateParent;

				fluidImportInput.Left.Set(20, 0f);

				Append(fluidImportInput);

				fluidImportOutput = new MachineInventoryItemSlot(1, ItemSlot.Context.BankItem) {
					ValidItemFunc = static item => item.IsAir,
					VAlign = 0.65f
				};
				fluidImportOutput.OnUpdateItem += UpdateParent;

				fluidImportOutput.Left.Set(20, 0f);

				Append(fluidImportOutput);

				fluidExportInput = new MachineInventoryItemSlot(2, ItemSlot.Context.BankItem) {
					ValidItemFunc = static item => item.IsAir || FluidTankEntity.ItemIsValidForImport(UIHandler.ActiveMachine as FluidTankEntity, item, insertionSlot: false),
					VAlign = 0.35f
				};
				fluidExportInput.OnUpdateItem += UpdateParent;

				fluidExportInput.Left.Set(-65, 1f);

				Append(fluidExportInput);

				fluidExportOutput = new MachineInventoryItemSlot(3, ItemSlot.Context.BankItem) {
					ValidItemFunc = static item => item.IsAir,
					VAlign = 0.65f
				};
				fluidExportOutput.OnUpdateItem += UpdateParent;

				fluidExportOutput.Left.Set(-65, 1f);

				Append(fluidExportOutput);
			}

			private void UpdateParent(IInventoryMachine machine, Item oldItem, Item newItem) => parentUI.NeedsToRecalculate = true;

			public override void Update(GameTime gameTime) {
				base.Update(gameTime);

				if (UIHandler.ActiveMachine is FluidTankEntity entity) {
					var storage = entity.FluidStorage[0];

					gauge.CurrentCapacity = storage.CurrentCapacity;
					gauge.SetMaxCapacity(storage.MaxCapacity);

					if (!storage.IsEmpty && storage.FluidType != FluidTypeID.None)
						gauge.Color = storage.FluidID.FluidColor;
				}
			}
		}
	}
}
