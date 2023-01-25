using Microsoft.Xna.Framework;
using SerousEnergyLib.API.Energy;
using SerousEnergyLib.API.Fluid;
using SerousEnergyLib.API.Machines;
using SerousEnergyLib.API.Machines.UI;
using SerousEnergyLib.Systems;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;
using TerraScience.Common.UI.Elements;
using TerraScience.Content.MachineEntities;

namespace TerraScience.Common.UI.Machines {
	public class GreenhouseUI : BaseMachineUI {
		public override string DefaultPage => "Greenhouse";

		public override bool IsUpgradesPageOpen => CurrentPage is BasicUpgradesPage;

		protected override LocalizedText GetMenuOptionLocalization(string key) {
			if (key == "Upgrades")
				return Language.GetText("Mods.TerraScience.MachineText.DefaultPage.Upgrades");

			return Language.GetText("Mods.TerraScience.MachineText.Greenhouse.Page." + key);
		}

		protected override IEnumerable<string> GetMenuOptions() {
			yield return "Greenhouse";
			yield return "Upgrades";
		}

		protected override BaseMachineUIPage InitPage(string page) {
			return page switch {
				"Greenhouse" => new MainPage(this),
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
			public FluidGaugeThin fluidGauge;
			public PowerGauge powerGauge;

			public MachineInventoryItemSlot soilSlot;
			public MachineInventoryItemSlot modifierSlot;
			public MachineInventoryItemSlot plantSlot;

			public MachineInventoryItemSlotZone itemZone;

			public BasicThinArrow arrow;

			public MainPage(BaseMachineUI parent) : base(parent, "Greenhouse") { }

			public override void OnInitialize() {
				powerGauge = new PowerGauge(1, 400);
				powerGauge.Left.Set(-40, 1f);
				powerGauge.VAlign = 0.5f;
				Append(powerGauge);

				fluidGauge = new FluidGaugeThin(1d, pixelWidth: 32, pixelHeight: 400);
				fluidGauge.Left.Set(powerGauge.Left.Pixels - fluidGauge.Width.Pixels - 8, 1f);
				fluidGauge.VAlign = 0.5f;
				Append(fluidGauge);

				plantSlot = new MachineInventoryItemSlot(2, context: ItemSlot.Context.BankItem) {
					ValidItemFunc = static item => item.IsAir || TechMod.Sets.Greenhouse.IsPlant[item.type]
				};
				plantSlot.Left.Set(-40, 0f);
				plantSlot.Top.Set(65, 0f);
				plantSlot.HAlign = 0.5f;
				plantSlot.hoverText = Language.GetTextValue("Mods.TerraScience.MachineText.Greenhouse.SlotText.Plant");

				modifierSlot = new MachineInventoryItemSlot(1, context: ItemSlot.Context.BankItem, scale: 0.85f) {
					ValidItemFunc = IsItemAllowedAsModifier
				};
				modifierSlot.Left.Set(-40 - modifierSlot.Width.Pixels - 10, 0f);
				modifierSlot.Top.Set(45, 0f);
				modifierSlot.HAlign = 0.5f;
				modifierSlot.hoverText = Language.GetTextValue("Mods.TerraScience.MachineText.Greenhouse.SlotText.Modifier");
				Append(modifierSlot);
				Append(plantSlot);

				soilSlot = new MachineInventoryItemSlot(0, context: ItemSlot.Context.BankItem, scale: 0.85f) {
					ValidItemFunc = static item => item.IsAir || TechMod.Sets.Greenhouse.IsSoil[item.type]
				};
				soilSlot.Left.Set(-40 - soilSlot.Width.Pixels - 10, 0f);
				soilSlot.Top.Set(45 + modifierSlot.Height.Pixels + 4, 0f);
				soilSlot.HAlign = 0.5f;
				soilSlot.hoverText = Language.GetTextValue("Mods.TerraScience.MachineText.Greenhouse.SlotText.Soil");
				Append(soilSlot);

				IInventoryMachine singleton = ModContent.GetInstance<GreenhouseEntity>();
				itemZone = new MachineInventoryItemSlotZone(singleton.GetExportSlotsOrDefault(), maxSlotsPerRow: 5, context: ItemSlot.Context.BankItem);
				itemZone.Left.Set(-40, 0f);
				itemZone.Top.Set(-15 - itemZone.Height.Pixels, 1f);
				itemZone.HAlign = 0.5f;
				Append(itemZone);

				arrow = new BasicThinArrow(ArrowElementOrientation.Down, targetLength: 150);
				arrow.Left.Set(-40, 0f);
				arrow.HAlign = 0.5f;
				arrow.VAlign = 0.5f;
				Append(arrow);
			}

			private bool IsItemAllowedAsModifier(Item item) {
				if (item.IsAir)
					return true;

				if (!TechMod.Sets.Greenhouse.IsSoilModifier[item.type])
					return false;

				var soil = soilSlot.StoredItem;

				return !soil.IsAir && TechMod.Sets.Greenhouse.SoilAllowsModifier.TryGetValue(soil.type, out var allowed) && allowed[item.type];
			}

			public override void Update(GameTime gameTime) {
				if (UIHandler.ActiveMachine is GreenhouseEntity entity) {
					// Update the fluid tank element
					var fluidStorage = entity.FluidStorage[0];

					fluidGauge.CurrentCapacity = fluidStorage.CurrentCapacity;
					fluidGauge.SetMaxCapacity(fluidStorage.MaxCapacity);

					if (!fluidStorage.IsEmpty && fluidStorage.FluidType != FluidTypeID.None)
						fluidGauge.Color = fluidStorage.FluidID.FluidColor;

					// Update the power gauge element
					arrow.FillPercentage = entity.Progress.Progress;

					var powerStorage = entity.PowerStorage;
					var id = entity.EnergyID;

					if (EnergyConversions.Get(id) is EnergyTypeID type) {
						powerGauge.CurrentPower = EnergyConversions.ConvertFromTerraFlux(powerStorage.CurrentCapacity, id);
						powerGauge.SetMaxCapacity(EnergyConversions.ConvertFromTerraFlux(powerStorage.MaxCapacity, id));
						powerGauge.TypeIDShortName = type.ShortName;
						powerGauge.Color = type.Color;
					}
				}

				base.Update(gameTime);
			}
		}
	}
}
