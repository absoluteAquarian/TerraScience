using SerousEnergyLib.API.Machines;
using SerousEnergyLib.API.Machines.UI;
using SerousEnergyLib.Systems;
using System.Collections.Generic;
using System;
using Terraria.Localization;
using TerraScience.Common.UI.Elements;
using Microsoft.Xna.Framework;
using TerraScience.Content.MachineEntities;
using SerousEnergyLib.API.Energy;

namespace TerraScience.Common.UI.Machines {
	public class BatteryUI : BaseMachineUI {
		public override string DefaultPage => "Storage";

		public override bool IsUpgradesPageOpen => CurrentPage is BasicUpgradesPage;

		protected override LocalizedText GetMenuOptionLocalization(string key) {
			if (key == "Upgrades")
				return Language.GetText("Mods.TerraScience.MachineText.DefaultPage.Upgrades");

			return Language.GetText("Mods.TerraScience.MachineText.Battery.Page." + key);
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
			width = 400;
			height = 250;
		}

		public class MainPage : BaseMachineUIPage {
			public PowerGauge gauge;

			public MainPage(BaseMachineUI parent) : base(parent, "Storage") { }

			public override void OnInitialize() {
				gauge = new PowerGauge(1, 180);
				gauge.Top.Set(20, 0f);
				gauge.HAlign = 0.5f;
				Append(gauge);
			}

			public override void Update(GameTime gameTime) {
				if (UIHandler.ActiveMachine is BatteryEntity machine) {
					// Update the power gauge
					var storage = machine.PowerStorage;
					var id = machine.EnergyID;

					if (EnergyConversions.Get(id) is EnergyTypeID type) {
						gauge.CurrentPower = EnergyConversions.ConvertFromTerraFlux(storage.CurrentCapacity, id);
						gauge.SetMaxCapacity(EnergyConversions.ConvertFromTerraFlux(storage.MaxCapacity, id));
						gauge.TypeIDShortName = type.ShortName;
						gauge.Color = type.Color;
					}
				}

				base.Update(gameTime);
			}
		}
	}
}
