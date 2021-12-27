using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using Terraria.UI;
using TerraScience.API.UI;
using TerraScience.Content.Items.Placeable.Machines;
using TerraScience.Content.TileEntities;
using TerraScience.Content.TileEntities.Energy;
using TerraScience.Content.TileEntities.Energy.Generators;
using TerraScience.Content.TileEntities.Energy.Storage;
using TerraScience.Content.Tiles.Multitiles;
using TerraScience.Utilities;

namespace TerraScience.Content.UI {
	public struct RegistryAnimation{
		public string texture;
		public Rectangle? frame;

		public RegistryAnimation(string texture){
			this.texture = texture;
			frame = null;
		}

		public RegistryAnimation(string texture, Rectangle? frame){
			this.texture = texture;
			this.frame = frame;
		}

		public RegistryAnimation(string texture, int frameX = 0, int frameY = 0, int columnCount = 1, int rowCount = 1, int buffer = 0){
			this.texture = texture;
			var tex = ModContent.GetTexture(texture);
			frame = tex.Frame(columnCount, rowCount, frameX, frameY);
			frame.Resize(-buffer, -buffer);
		}

		public RegistryAnimation(string texture, uint frameX = 0, uint frameY = 0, uint columnCount = 1, uint rowCount = 1, uint buffer = 0){
			this.texture = texture;
			var tex = ModContent.GetTexture(texture);
			frame = tex.Frame((int)columnCount, (int)rowCount, (int)frameX, (int)frameY);
			frame.Resize(-(int)buffer, -(int)buffer);
		}
	}

	public class ScienceWorkbenchItemRegistry{
		public delegate RegistryAnimation? GetDisplay(uint currentTick);

		public readonly GetDisplay GetFirstDisplay;
		public readonly GetDisplay GetSecondDisplay;

		/// <summary>
		/// The string representing the description for the item.  Lines should be separated by a newline character
		/// </summary>
		public readonly string Description;
		/// <summary>
		/// The string indicating if/when the machine consumes Terra Flux (TF)
		/// </summary>
		public readonly string ConsumesTFLine;
		/// <summary>
		/// The string indicating if/when the machine procues Terra Flux (TF)
		/// </summary>
		public readonly string ProducesTFLine;

		public ScienceWorkbenchItemRegistry(GetDisplay firstDisplayFunc, GetDisplay secondDisplayFunc, string description, string consumeTFLine, string produceTFLine){
			GetFirstDisplay = firstDisplayFunc;
			GetSecondDisplay = secondDisplayFunc;
			Description = description;
			ConsumesTFLine = consumeTFLine;
			ProducesTFLine = produceTFLine;
		}
	}

	public class ScienceWorkbenchUI : MachineUI{
		public UIItemSlot item;

		public UIScienceWorkbenchDisplay display1, display2;
		public UIPanel panelStats, panelDescription;

		public UIText statsText, descriptionText;

		public override string Header => "Science Workbench";

		public override int TileType => ModContent.TileType<ScienceWorkbench>();

		internal override void PanelSize(out int width, out int height){
			width = 700;
			height = 600;
		}

		internal override void UpdateEntity(){
			// TODO: update UI stuff containing pictures, information, etc.
			Item slot = UIEntity.RetrieveItem(0);

			if(slot.IsAir){
				RemoveDisplay(ref display1);
				RemoveDisplay(ref display2);

				statsText.SetText("Place a machine item in the slot to the left" +
					"\nto see statistics for its machine");
				descriptionText.SetText("The description for the machine would go here");

				for(int i = 1; i < 15; i++)
					GetSlot(i).SetItem(new Item());

				return;
			}

			if(!(slot.modItem is MachineItem machine))
				return;

			var registry = machine.ItemRegistry;
			Machine tile = ModContent.GetModTile(machine.TileType) as Machine;
			tile.GetDefaultParams(out string name, out uint width, out uint height, out int itemType);

			if(GetSlot(0).ItemTypeChanged){
				//Set the stats
				statsText.SetText($"Machine: {name}" +
					$"\nSize: {width} x {height} tiles" +
					"\n" + GetMachineClassification());

				//Set the description
				StringBuilder desc = new StringBuilder(registry.Description);

				if(registry.ConsumesTFLine != null)
					desc.Append("\nConsumes TF: " + registry.ConsumesTFLine);

				if(registry.ProducesTFLine != null)
					desc.Append("\nProduces TF: " + registry.ProducesTFLine);

				descriptionText.SetText(desc.ToString());
			}

			var firstDisplay = registry.GetFirstDisplay(Main.GameUpdateCount);
			var secondDisplay = registry.GetSecondDisplay(Main.GameUpdateCount);

			//If first is null, but second isn't, treat second as first
			if(firstDisplay is null && secondDisplay != null){
				firstDisplay = secondDisplay;
				secondDisplay = default;
			}

			//Set the displays
			if(firstDisplay != null)
				SetDisplay(ref display1, firstDisplay.Value, leftDisplay: true);
				
			if(secondDisplay != null)
				SetDisplay(ref display2, secondDisplay.Value, leftDisplay: false);

			//Update the ingredient slots
			RecipeIngredientSet set = DatalessMachineInfo.recipeIngredients[itemType];

			for(int i = 0; i < 14; i++)
				GetSlot(i + 1).SetItem(set[i]);
		}

		private string GetMachineClassification(){
			StringBuilder classification = new StringBuilder();

			Item slot = UIEntity.RetrieveItem(0);
			MachineEntity entity = (slot.modItem as MachineItem).Machine;

			if(entity is Battery)
				classification.Append("Terra Flux Storage");
			else if(entity is GeneratorEntity)
				classification.Append("Terra Flux Generator");
			else if(entity is PoweredMachineEntity)
				classification.Append("Powered Machine");
			else
				classification.Append("Generic Machine");

			entity.HijackCanBeInteractedWithItemNetworks(out _, out bool canInput, out bool canOutput);
			bool hasInput = canInput || entity.GetInputSlots().Length > 0;
			bool hasOutput = canOutput || entity.GetOutputSlots().Length > 0;

			if(hasInput && hasOutput)
				classification.Append(", Input/Output");
			else if(hasInput && !hasOutput)
				classification.Append(", Input");
			else if(!hasInput && hasOutput)
				classification.Append(", Output");

			if(entity is IFluidMachine fluidMachine){
				var entries = fluidMachine.FluidEntries;

				bool hasLiquid = false, hasGas = false;

				foreach(var entry in entries){
					if(entry.validTypes is null){
						classification.Append(", Liquids, Gases");
						break;
					}

					foreach(var type in entry.validTypes){
						if(!hasLiquid && type.IsLiquidID())
							hasLiquid = true;
						else if(!hasGas && type.IsGasID())
							hasGas = true;

						if(hasLiquid && hasGas)
							goto appendLiquidOrGas;
					}
				}

appendLiquidOrGas:
				if(hasLiquid)
					classification.Append(", Liquids");

				if(hasGas)
					classification.Append(", Gases");
			}

			return classification.ToString();
		}

		internal override void InitializeText(List<UIText> text) {
			// TODO: text for displaying what machine is in the slot
			statsText = new UIText("Place a machine item in the slot to the left" +
				"\nto see statistics for its machine");
			statsText.Left.Set(0, 0);
			statsText.Top.Set(0, 0);

			descriptionText = new UIText("The description for the machine would go here");
			descriptionText.Left.Set(0, 0);
			descriptionText.Top.Set(0, 0);
		}

		internal override void InitializeSlots(List<UIItemSlot> slots){
			slots.Add(item = new UIItemSlot(){
				ValidItemFunc = item => item.IsAir || item.modItem is MachineItem
			});
			item.Left.Set(15, 0);
			item.Top.Set(70, 0);

			//Recipe slots
			for(int i = 0; i < 14; i++){
				UIItemSlot slot = new UIItemSlot(){
					IgnoreClicks = true  //Allow the item info to be viewed, just not interacted with
				};
				slot.Left.Set(10 + (i % 7) * Main.inventoryBack9Texture.Width, 0);
				slot.Top.Set(400 + Main.inventoryBack9Texture.Height * (i / 7), 0);

				slots.Add(slot);
			}
		}

		internal override void InitializeOther(UIPanel panel){
			// TODO: UI slots for open/close images, other displays and information text (scrolling???)
			panelStats = new UIPanel();
			panelStats.Width.Set(0, (panel.Width.Pixels - 100) / panel.Width.Pixels);
			panelStats.Height.Set(120, 0);
			panelStats.Left.Set(80, 0);
			panelStats.Top.Set(70, 0);
			panel.Append(panelStats);

			panelDescription = new UIPanel();
			panelDescription.Width.Set(0, (panel.Width.Pixels - 40) / panel.Width.Pixels);
			panelDescription.Height.Set(170, 0);
			panelDescription.Left.Set(20, 0);
			panelDescription.Top.Set(210, 0);
			panel.Append(panelDescription);

			panelStats.Append(statsText);
			panelDescription.Append(descriptionText);
		}

		public override void PreClose(){
			if(!item.StoredItem.IsAir){
				Main.LocalPlayer.QuickSpawnClonedItem(item.StoredItem, item.StoredItem.stack);
				item.SetItem(new Item());
			}

			RemoveDisplay(ref display1);
			RemoveDisplay(ref display2);

			//Force one final update to update the text
			UpdateEntity();
		}

		private void SetDisplay(ref UIScienceWorkbenchDisplay display, RegistryAnimation registry, bool leftDisplay){
			bool newInstance = display is null;

			var texture = ModContent.GetTexture(registry.texture);
			
			int width = registry.frame?.Width ?? texture.Width;
			int height = registry.frame?.Height ?? texture.Height;

			int maxDim = Math.Max(width, height);
			float scale = maxDim > 80 ? 80f / maxDim : 1f;
			const int topBase = 500;
			float top = height * scale < 80 ? topBase + 80 - height * scale : topBase;

			if(display is null)
				display = new UIScienceWorkbenchDisplay(registry.texture, registry.frame);
			else
				display.SetImage(registry.texture, registry.frame);

			display.Left.Set(leftDisplay ? 60 : 200, 0);
			display.Top.Set(top, 0);
			display.Scale = scale;

			if(newInstance)
				AppendElement(display);
		}

		private void RemoveDisplay(ref UIScienceWorkbenchDisplay display){
			display?.Remove();
			display = null;
		}
	}
}
