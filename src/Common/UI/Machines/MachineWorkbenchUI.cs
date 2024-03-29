﻿using Microsoft.Xna.Framework;
using SerousCommonLib.UI;
using SerousEnergyLib.API.CrossMod;
using SerousEnergyLib.API.Machines;
using SerousEnergyLib.API.Machines.UI;
using SerousEnergyLib.Items;
using SerousEnergyLib.Systems;
using SerousEnergyLib.Tiles;
using System;
using System.Collections.Generic;
using System.Text;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.Map;
using Terraria.ModLoader;
using Terraria.UI;
using TerraScience.Common.Systems;
using TerraScience.Common.UI.Elements;
using TerraScience.Content.Items.Machines;
using TerraScience.Content.MachineEntities;

namespace TerraScience.Common.UI.Machines {
	public class MachineWorkbenchUI : BaseMachineUI {
		public override string DefaultPage => "Workbench";

		public override bool IsUpgradesPageOpen => false;  // Machine Workbench does not have upgrades

		protected override LocalizedText GetMenuOptionLocalization(string key) {
			return Language.GetText("Mods.TerraScience.MachineText.MachineWorkbench.Page." + key);
		}

		protected override IEnumerable<string> GetMenuOptions() {
			yield return "Workbench";
		}

		protected override BaseMachineUIPage InitPage(string page) {
			return page switch {
				"Workbench" => new MainPage(this),
				_ => throw new ArgumentException("Unknown page: " + page, nameof(page))
			};
		}

		public override void GetDefaultPanelDimensions(out int width, out int height) {
			width = 700;
			height = 600;
		}

		protected override void OnClose() {
			if (UIHandler.ActiveMachine is MachineWorkbenchEntity machine && !machine.Inventory[0].IsAir)
				IInventoryMachine.DropItemInInventory(machine, 0, quickSpawn: true);

			if (GetPage<MainPage>("Workbench") is MainPage page) {
				page.ClearDisplay();

				page.leftDisplay = null;
				page.rightDisplay = null;

				page.UpdateItemDisplay(null, null, new Item());
			}
		}

		public class MainPage : BaseMachineUIPage {
			public MachineInventoryItemSlot machineSlot;
			
			private MachineWorkbenchRecipeIngredientItemSlotZone recipeIngredientSlots;
			private UIText recipeIngredients, recipeTiles, recipeConditions;
			private Recipe lastKnownRecipe;

			public MachineWorkbenchDisplay leftDisplay, rightDisplay;
			private MachineWorkbenchRegistry activeRegistry;

			public UIPanel panelDisplays, panelStats, panelDescription, panelRecipe;
			public UIText stats, description;

			public NewUIList list;
			private NewUIScrollbar scroll;

			private const string DefaultStatText = "Place a machine item in the slot to see statistics for its machine";

			private const string DefaultDescriptionText = "The description for the machine would go here";

			private const float MaxDisplaySize = 80f;

			public MainPage(BaseMachineUI parent) : base(parent, "Workbench") {
				OnPageSelected += CheckItem;
			}

			public override void OnInitialize() {
				base.OnInitialize();

				// Initialize scrolling elements
				list = new();
				list.SetPadding(0);
				list.Width.Set(-20, 1f);
				list.Height.Set(0, 0.96f);
				list.Left.Set(20, 0);
				list.Top.Set(0, 0.05f);

				scroll = new();
				scroll.Width.Set(20, 0);
				scroll.Height.Set(0, 0.885f);
				scroll.Left.Set(-20, 1f);
				scroll.Top.Set(0, 0.1f);

				list.SetScrollbar(scroll);
				list.Append(scroll);
				list.ListPadding = 10;
				Append(list);

				// Initialize the item slot elements
				machineSlot = new MachineInventoryItemSlot(0, context: ItemSlot.Context.BankItem) {
					ValidItemFunc = static item => item.IsAir || item.ModItem is BaseMachineItem,
					hoverText = Language.GetTextValue("Mods.TerraScience.MachineText.MachineWorkbench.SlotText.Input")
				};
				machineSlot.OnUpdateItem += UpdateItemDisplay;
				machineSlot.Left.Set(15, 0);

				list.Add(machineSlot);

				recipeIngredientSlots = new();

				// Initialize the text elements
				stats = new UIText(DefaultStatText) {
					IsWrapped = true,
					Width = StyleDimension.Fill,
					WrappedTextBottomPadding = 10,
					TextOriginX = 0
				};

				description = new UIText(DefaultDescriptionText) {
					IsWrapped = true,
					Width = StyleDimension.Fill,
					WrappedTextBottomPadding = 10,
					TextOriginX = 0
				};

				recipeIngredients = new UIText("") {
					IsWrapped = true,
					Width = StyleDimension.Fill,
					WrappedTextBottomPadding = 0,
					TextOriginX = 0
				};
				recipeIngredients.Left.Set(8, 0);

				recipeTiles = new UIText("") {
					IsWrapped = true,
					Width = StyleDimension.Fill,
					WrappedTextBottomPadding = 0,
					TextOriginX = 0
				};
				recipeTiles.Left.Set(8, 0);

				recipeConditions = new UIText("") {
					IsWrapped = true,
					Width = StyleDimension.Fill,
					WrappedTextBottomPadding = 0,
					TextOriginX = 0
				};
				recipeConditions.Left.Set(8, 0);

				// Initialize the panels that align the elements
				panelDisplays = new UIPanel();
				panelDisplays.Width.Set(180, 0f);
				panelDisplays.Height.Set(MaxDisplaySize, 0f);
				panelDisplays.Left.Set(20, 0f);

				panelStats = new UIPanel();
				panelStats.Width.Set(-80, 1f);
				panelStats.Left.Set(20, 0);

				panelStats.Append(stats);
				list.Add(panelStats);

				panelDescription = new UIPanel();
				panelDescription.Width.Set(-80, 1f);
				panelDescription.Left.Set(20, 0);

				panelDescription.Append(description);
				list.Add(panelDescription);

				panelRecipe = new UIPanel();
				panelRecipe.Width.Set(-80, 1f);
				panelRecipe.Height.Set(200, 1f);  // Large value to ensure that text wrapping works
				panelRecipe.Left.Set(20, 0);

				panelRecipe.Append(recipeIngredients);
				panelRecipe.Append(recipeIngredientSlots);
				panelRecipe.Append(recipeTiles);
				panelRecipe.Append(recipeConditions);

				// Force a recalculate to ensure that all widths/heights are correct
				Recalculate();

				SetMachineText(DefaultStatText, DefaultDescriptionText);
			}

			private bool resetRecipePanel;
			private Recipe pendingRecipe;

			private void CheckItem() {
				if (UIHandler.ActiveMachine is MachineWorkbenchEntity machine)
					UpdateItemDisplay(machine, null, machine.Inventory[0]);
			}

			internal void UpdateItemDisplay(IInventoryMachine machine, Item oldItem, Item newItem) {
				if (newItem.IsAir || newItem.ModItem is not BaseMachineItem machineItem) {
					SetMachineText(DefaultStatText, DefaultDescriptionText);

					pendingRecipe = null;

					resetRecipePanel = true;
					return;
				}

				ModTile tile = TileLoader.GetTile(machineItem.MachineTile);
				if (tile is not IMachineTile machineTile)
					throw new InvalidOperationException("InventoryMachine.MachineTile did not refer to an IMachineTile instance");

				if (tile is not IMachineWorkbenchViewableMachine registryTile)
					throw new InvalidOperationException("InventoryMachine.MachineTile did not refer to an IMachineWorkbenchViewableMachine instance");

				// Update the stats
				machineTile.GetMachineDimensions(out uint width, out uint height);
				activeRegistry = registryTile.GetRegistry();

				SetMachineText(
					statsText: $"Machine: {Language.GetTextValue($"Mods.{tile.Mod.Name}.MapObject.{tile.Name}")}" +
						$"\nSize: {width} x {height} blocks" +
						$"\n{string.Join("\n", activeRegistry.GetDescriptorLines())}",
					descriptionText: Language.GetTextValue($"Mods.{tile.Mod.Name}.MachineText.{tile.Name}.Description"));

				UpdateDisplays();

				resetRecipePanel = true;
			}

			private void SetMachineText(string statsText, string descriptionText) {
				stats.SetText(statsText);

				panelStats.Height.Set(stats.MinHeight.Pixels + 8, 0f);

				panelStats.Recalculate();

				description.SetText(descriptionText);

				panelDescription.Height.Set(description.MinHeight.Pixels + 8, 0f);

				panelDescription.Recalculate();
			}

			private void InitRecipeSlots(Recipe recipe) {
				if (object.ReferenceEquals(lastKnownRecipe, recipe))
					return;

				lastKnownRecipe = recipe;

				if (recipe is null) {
					list.Remove(panelRecipe);
					recipeIngredientSlots.InitializeSlots(null, 0);
					return;
				}

				// Ensure that the width/height are properly set...
				panelRecipe.Recalculate();

				const int maxColumns = 11;

				float top = 0;

				// Add the ingredient slots
				if (recipe.requiredItem.Count > 0 || recipe.acceptedGroups.Count > 0) {
					recipeIngredients.SetText(Language.GetTextValue("Mods.TerraScience.MachineText.MachineWorkbench.RecipeText.Ingredients", ""));

					top += recipeIngredients.MinHeight.Pixels + 4;

					recipeIngredientSlots.Top.Set(top, 0f);
					recipeIngredientSlots.InitializeSlots(recipe, maxColumns);

					top += recipeIngredientSlots.Height.Pixels + 8;
				} else {
					recipeIngredients.SetText(Language.GetTextValue("Mods.TerraScience.MachineText.MachineWorkbench.RecipeText.Ingredients", Language.GetTextValue("LegacyInterface.23")));

					recipeIngredientSlots.Remove();

					top += recipeIngredients.MinHeight.Pixels + 8;
				}

				static void AddText(StringBuilder sb, string text) {
					if (sb.Length > 0)
						sb.Append(", ");

					sb.Append(text);
				}

				// Add the tile text
				string text;
				if (recipe.requiredTile.Count > 0) {
					StringBuilder tileText = new StringBuilder();

					foreach (int tile in recipe.requiredTile)
						AddText(tileText, Lang.GetMapObjectName(MapHelper.TileToLookup(tile, 0)));

					text = tileText.ToString();
				} else
					text = Language.GetTextValue("LegacyInterface.23");

				recipeTiles.SetText(Language.GetTextValue("Mods.TerraScience.MachineText.MachineWorkbench.RecipeText.Tiles", text));
				recipeTiles.Top.Set(top, 0f);

				top += recipeTiles.MinHeight.Pixels + 8;

				// Add the condition text
				if (recipe.Conditions.Count > 0) {
					StringBuilder conditionText = new StringBuilder();

					foreach (var condition in recipe.Conditions)
						AddText(conditionText, condition.Description);
				} else
					text = Language.GetTextValue("LegacyInterface.23");

				recipeConditions.SetText(Language.GetTextValue("Mods.TerraScience.MachineText.MachineWorkbench.RecipeText.Conditions", text));
				recipeConditions.Top.Set(top, 0f);

				top += recipeConditions.MinHeight.Pixels;

				panelRecipe.Height.Set(top + 10, 0f);
				panelRecipe.Recalculate();
			}

			public override void Update(GameTime gameTime) {
				if (resetRecipePanel) {
					list.Remove(panelDisplays);
					list.Remove(panelStats);
					list.Remove(panelDescription);
					list.Remove(panelRecipe);
					
					InitRecipeSlots(pendingRecipe);

					list.Add(panelDisplays);
					list.Add(panelStats);
					list.Add(panelDescription);

					if (pendingRecipe is not null)
						list.Add(panelRecipe);

					pendingRecipe = null;
					resetRecipePanel = false;
				}

				// If there's a machine in the slot and it has multiple recipes, choose a random recipe that creates it every 3 seconds
				// Otherwise, display the one recipe it has or nothing if it has no recipe
				if (UIHandler.ActiveMachine is not MachineWorkbenchEntity || machineSlot.StoredItem.IsAir || machineSlot.StoredItem.ModItem is not BaseMachineItem item || activeRegistry is null) {
					SetDisplay(ref leftDisplay, null, left: true);
					SetDisplay(ref rightDisplay, null, left: false);
					InitRecipeSlots(null);
					list.Remove(panelDisplays);
					activeRegistry = null;

					base.Update(gameTime);
					return;
				}

				if (activeRegistry is not null)
					UpdateDisplays();

				if (RecipeCache.MachineItemToRecipes.TryGetValue(item.Type, out var recipes) || (item is ICraftableMachineItem alt && RecipeCache.MachineItemToRecipes.TryGetValue(alt.AlternativeItemType, out recipes))) {
					if (recipes.Length == 1) {
						InitRecipeSlots(recipes[0]);

						if (panelRecipe.Parent is null)
							list.Add(panelRecipe);
					} else if (recipes.Length > 1) {
						if (Main.GameUpdateCount % 180 == 0) {
							InitRecipeSlots(ChooseRandomRecipeExceptForCurrentRecipe(recipes));

							if (panelRecipe.Parent is null)
								list.Add(panelRecipe);
						}
					} else
						list.Remove(panelRecipe);  // No recipe
				}

				base.Update(gameTime);
			}

			private Recipe ChooseRandomRecipeExceptForCurrentRecipe(Recipe[] recipes) {
				if (recipes.Length == 0)
					return null;

				if (recipes.Length == 1)
					return recipes[0];

				Recipe recipe;
				do {
					recipe = Main.rand.Next(recipes);
				} while (object.ReferenceEquals(lastKnownRecipe, recipe));

				return recipe;
			}

			internal void ClearDisplay() {
				activeRegistry = null;
				SetDisplay(ref leftDisplay, null, left: true);
				SetDisplay(ref rightDisplay, null, left: false);
			}

			private void UpdateDisplays() {
				var left = activeRegistry.GetFirstDisplay(Main.GameUpdateCount);
				var right = activeRegistry.GetSecondDisplay?.Invoke(Main.GameUpdateCount);

				// If the left display is null, but the right one isn't, make the right display be the left display
				if (left is null && right is not null) {
					left = right;
					right = null;
				}

				// Set the displays
				SetDisplay(ref leftDisplay, left, left: true);
				SetDisplay(ref rightDisplay, right, left: false);

				rightDisplay?.Left.Set(leftDisplay.Width.Pixels + 10, 0f);

				float width = leftDisplay.Left.Pixels + leftDisplay.Width.Pixels + (rightDisplay?.Parent is null ? 0 : rightDisplay.Width.Pixels + 10);
				panelDisplays.Width.Set(width + 20, 0f);

				float height = rightDisplay?.PaddingRight is null ? leftDisplay.Height.Pixels : Math.Max(leftDisplay.Height.Pixels, rightDisplay.Height.Pixels);
				panelDisplays.Height.Set(height + 20, 0f);

				panelDisplays.Recalculate();
			}

			private void SetDisplay(ref MachineWorkbenchDisplay display, MachineRegistryDisplayAnimationState state, bool left) {
				if (state is null) {
					display?.Remove();
					return;
				}

				bool create = display?.Parent is null;

				int width = state.frame.Width;
				int height = state.frame.Height;

				int max = Math.Max(width, height);

				float entryWidth = Math.Min(max, MaxDisplaySize);
				float entryHeight = Math.Min(max, MaxDisplaySize);

				float scale = max > MaxDisplaySize ? MaxDisplaySize / max : 1f;

				if (create)
					display = new MachineWorkbenchDisplay(state.asset, state.frame);
				else
					display.SetImage(state.asset, state.frame);

				display.Left.Set(0, 0f);
				display.Top.Set(0, 0f);
				display.Width.Set(entryWidth, 0f);
				display.Height.Set(entryHeight, 0f);
				display.Scale = scale;

				if (create)
					panelDisplays.Append(display);
			}
		}
	}
}
