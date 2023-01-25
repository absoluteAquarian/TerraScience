using SerousEnergyLib.API.Machines;
using SerousEnergyLib.API.Machines.UI;
using SerousEnergyLib.Systems;
using System.Collections.Generic;
using System;
using Terraria.Localization;
using TerraScience.Common.UI.Elements;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;
using Microsoft.Xna.Framework;
using TerraScience.Content.MachineEntities;
using System.Text;
using Terraria.ModLoader;
using SerousEnergyLib.API.Energy;

namespace TerraScience.Common.UI.Machines {
	public class WindTurbineUI : BaseMachineUI {
		public override string DefaultPage => "Turbine";

		public override bool IsUpgradesPageOpen => CurrentPage is BasicUpgradesPage;

		protected override LocalizedText GetMenuOptionLocalization(string key) {
			if (key == "Upgrades")
				return Language.GetText("Mods.TerraScience.MachineText.DefaultPage.Upgrades");

			return Language.GetText("Mods.TerraScience.MachineText.WindTurbine.Page." + key);
		}

		protected override IEnumerable<string> GetMenuOptions() {
			yield return "Turbine";
			yield return "Upgrades";
		}

		protected override BaseMachineUIPage InitPage(string page) {
			return page switch {
				"Turbine" => new MainPage(this),
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
			height = 320;
		}

		public class MainPage : BaseMachineUIPage {
			public PowerGauge gauge;

			public UIText boostText;
			public UIText boostPower;

			private const string BoostIdentifierKey = "Mods.TerraScience.MachineText.WindTurbine.BoostText.Boosts";

			public MainPage(BaseMachineUI parent) : base(parent, "Turbine") { }

			public override void OnInitialize() {
				gauge = new PowerGauge(1, 180);
				gauge.Top.Set(20, 0f);
				gauge.HAlign = 0.5f;
				Append(gauge);

				string soNoBoost = Language.GetTextValue("Mods.TerraScience.MachineText.WindTurbine.BoostText.None");

				boostText = new UIText(Language.GetTextValue(BoostIdentifierKey, soNoBoost)) {
					IsWrapped = true,
					Width = StyleDimension.Fill,
					WrappedTextBottomPadding = 0,
					TextOriginX = 0
				};
				boostText.Left.Set(8, 0);
				boostText.Top.Set(220, 0);
				Append(boostText);

				boostPower = new UIText("") {
					IsWrapped = true,
					Width = StyleDimension.Fill,
					WrappedTextBottomPadding = 0,
					TextOriginX = 0
				};
				boostPower.Left.Set(8, 0);
			}

			public override void Update(GameTime gameTime) {
				if (UIHandler.ActiveMachine is WindTurbineEntity machine) {
					// Update the boost text
					StringBuilder boosts = new();

					void AddText(string key) {
						if (boosts.Length > 0)
							boosts.Append(", ");

						boosts.Append(Language.GetTextValue("Mods.TerraScience.MachineText.WindTurbine.BoostText." + key));
					}

					if (machine.raining)
						AddText("Rain");

					if (machine.storming)
						AddText("Storm");

					if (machine.sandstorm)
						AddText("Sandstorm");

					if (machine.blizzard)
						AddText("Blizzard");

					if (boosts.Length == 0) {
						// No boosts
						string soNoBoost = Language.GetTextValue("Mods.TerraScience.MachineText.WindTurbine.BoostText.None");

						boostText.SetText(Language.GetTextValue(BoostIdentifierKey, soNoBoost));
						boostPower.Remove();
					} else {
						boostText.SetText(Language.GetTextValue(BoostIdentifierKey, boosts.ToString()));

						StatModifier modifier = machine.GetBoostModifier();

						boostPower.SetText(Language.GetTextValue("Mods.TerraScience.MachineText.WindTurbine.BoostText.BoostPower", modifier.Additive, modifier.Flat));

						if (boostPower.Parent is null) {
							boostPower.Top.Set(220 + boostText.MinHeight.Pixels + 4, 0);
							Append(boostPower);
						}
					}

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
