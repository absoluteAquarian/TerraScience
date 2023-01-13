using Microsoft.Xna.Framework;
using SerousEnergyLib.API.Machines;
using SerousEnergyLib.API.Machines.UI;
using SerousEnergyLib.Systems;
using System;
using System.Collections.Generic;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;
using TerraScience.Common.UI.Elements;
using TerraScience.Content.MachineEntities;

namespace TerraScience.Common.UI.Machines {
	public class ReinforcedFurnaceUI : BaseMachineUI {
		public override string DefaultPage => "Furnace";

		public override bool IsUpgradesPageOpen => CurrentPage is BasicUpgradesPage;

		protected override LocalizedText GetMenuOptionLocalization(string key) {
			if (key == "Upgrades")
				return Language.GetText("Mods.TerraScience.MachineText.DefaultPage.Upgrades");

			return Language.GetText("Mods.TerraScience.MachineText.ReinforcedFurnace.Page." + key);
		}

		protected override IEnumerable<string> GetMenuOptions() {
			yield return "Furnace";
			yield return "Upgrades";
		}

		protected override BaseMachineUIPage InitPage(string page) {
			return page switch {
				"Furnace" => new MainPage(this),
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
			width = 450;
			height = 500;
		}

		public class MainPage : BaseMachineUIPage {
			public Thermostat thermostat;
			public MachineInventoryItemSlot inputSlot;
			public MachineInventoryItemSlotZone itemZone;

			public BasicThinArrow arrow;

			public MainPage(BaseMachineUI parent) : base(parent, "Furnace") { }

			public override void OnInitialize() {
				thermostat = new Thermostat(5, 20, 1000, 20);
				thermostat.Left.Set(-thermostat.Width.Pixels - 10, 1f);
				thermostat.VAlign = 0.5f;
				Append(thermostat);

				inputSlot = new MachineInventoryItemSlot(0, context: ItemSlot.Context.BankItem);
				inputSlot.HAlign = 0.5f;
				inputSlot.Top.Set(65, 0f);
				Append(inputSlot);

				IInventoryMachine singleton = ModContent.GetInstance<ReinforcedFurnaceEntity>();
				itemZone = new MachineInventoryItemSlotZone(singleton.GetExportSlotsOrDefault(), maxSlotsPerRow: 5, context: ItemSlot.Context.BankItem);
				itemZone.HAlign = 0.5f;
				itemZone.Top.Set(-15 - itemZone.Height.Pixels, 1f);
				Append(itemZone);

				arrow = new BasicThinArrow(ArrowElementOrientation.Down, targetLength: 150);
				arrow.HAlign = 0.5f;
				arrow.VAlign = 0.5f;
				Append(arrow);
			}

			public override void Update(GameTime gameTime) {
				base.Update(gameTime);

				if (UIHandler.ActiveMachine is ReinforcedFurnaceEntity furnace) {
					arrow.FillPercentage = furnace.Progress.Progress;

					furnace.GetHeatTargets(out double min, out double max, out _);

					thermostat.CurrentTemperature = furnace.CurrentTemperature;
					thermostat.SetTemperatureBounds(min, max);
				}
			}
		}
	}
}
