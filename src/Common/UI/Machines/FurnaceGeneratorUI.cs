using Microsoft.Xna.Framework;
using SerousEnergyLib.API.Energy;
using SerousEnergyLib.API.Machines;
using SerousEnergyLib.API.Machines.UI;
using SerousEnergyLib.Systems;
using System;
using System.Collections.Generic;
using Terraria.Localization;
using Terraria.UI;
using TerraScience.Common.UI.Elements;
using TerraScience.Content.MachineEntities;

namespace TerraScience.Common.UI.Machines {
	public class FurnaceGeneratorUI : BaseMachineUI {
		public override string DefaultPage => "Generator";

		public override bool IsUpgradesPageOpen => CurrentPage is BasicUpgradesPage;

		protected override LocalizedText GetMenuOptionLocalization(string key) {
			if (key == "Upgrades")
				return Language.GetText("Mods.TerraScience.MachineText.DefaultPage.Upgrades");

			return Language.GetText("Mods.TerraScience.MachineText.FurnaceGenerator.Page." + key);
		}

		protected override IEnumerable<string> GetMenuOptions() {
			yield return "Generator";
			yield return "Upgrades";
		}

		protected override BaseMachineUIPage InitPage(string page) {
			return page switch {
				"Generator" => new MainPage(this),
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
			width = 350;
			height = 300;
		}

		public class MainPage : BaseMachineUIPage {
			public MachineInventoryItemSlot input;

			public BasicThinArrow arrow;

			public PowerGauge gauge;

			public MainPage(BaseMachineUI parent) : base(parent, "Generator") { }

			public override void OnInitialize() {
				input = new MachineInventoryItemSlot(0, context: ItemSlot.Context.BankItem);
				input.Left.Set(10, 0f);
				input.VAlign = 0.5f;
				Append(input);

				arrow = new BasicThinArrow(ArrowElementOrientation.Right, targetLength: 180);
				arrow.HAlign = 0.5f;
				arrow.VAlign = 0.5f;
				Append(arrow);

				gauge = new PowerGauge(1, 200);
				gauge.Left.Set(-40, 1f);
				gauge.VAlign = 0.5f;
				Append(gauge);
			}

			public override void Update(GameTime gameTime) {
				base.Update(gameTime);

				if (UIHandler.ActiveMachine is FurnaceGeneratorEntity entity) {
					arrow.FillPercentage = entity.Progress.Progress;

					var storage = entity.PowerStorage;
					var id = entity.EnergyID;

					if (EnergyConversions.Get(id) is EnergyTypeID type) {
						gauge.CurrentPower = EnergyConversions.ConvertFromTerraFlux(storage.CurrentCapacity, id);
						gauge.SetMaxCapacity(EnergyConversions.ConvertFromTerraFlux(storage.MaxCapacity, id));
						gauge.TypeIDShortName = type.ShortName;
						gauge.Color = type.Color;
					}
				}
			}
		}
	}
}
