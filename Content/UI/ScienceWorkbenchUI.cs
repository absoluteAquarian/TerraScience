using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
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
		public readonly string texture;
		public readonly Rectangle? frame;

		public RegistryAnimation(string texture){
			this.texture = texture;
			frame = null;
		}

		public RegistryAnimation(string texture, Rectangle? frame){
			this.texture = texture;
			this.frame = frame;
		}

		public RegistryAnimation(string texture, int frameX = 0, int frameY = 0, int columnCount = 1, int rowCount = 1){
			this.texture = texture;
			var tex = ModContent.GetTexture(texture);
			frame = tex.Frame(columnCount, rowCount, frameX, frameY);
		}

		public RegistryAnimation(string texture, uint frameX = 0, uint frameY = 0, uint columnCount = 1, uint rowCount = 1){
			this.texture = texture;
			var tex = ModContent.GetTexture(texture);
			frame = tex.Frame((int)columnCount, (int)rowCount, (int)frameX, (int)frameY);
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
			width = 400;
			height = 550;
		}

		internal override void UpdateEntity(){
			// TODO: update UI stuff containing pictures, information, etc.
			Item slot = UIEntity.RetrieveItem(0);

			if(slot.IsAir){
				if(display1 != null)
					RemoveDisplay(ref display1);
				if(display2 != null)
					RemoveDisplay(ref display2);

				if(panelStats.HasChild(statsText))
					panelStats.RemoveChild(statsText);
				if(panelDescription.HasChild(descriptionText))
					panelDescription.RemoveChild(descriptionText);

				statsText.SetText("");
				descriptionText.SetText("");

				for(int i = 1; i < UIEntity.SlotsCount; i++)
					GetSlot(0).SetItem(new Item());

				return;
			}

			if(!(slot.modItem is MachineItem machine))
				return;

			var registry = machine.ItemRegistry;
			Machine tile = ModContent.GetModTile(machine.TileType) as Machine;
			tile.GetDefaultParams(out _, out uint width, out uint height, out int itemType);

			if(statsText.Text == "" || GetSlot(0).ItemTypeChanged){
				if(!panelStats.HasChild(statsText))
					panelStats.Append(statsText);
				if(!panelDescription.HasChild(descriptionText))
					panelDescription.Append(descriptionText);

				//Set the stats
				statsText.SetText($"Machine: {machine.Name}" +
					$"\nSize: {width}x{height} tiles" +
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

			if(display1 != null)
				AppendElement(display1);

			if(display2 != null)
				AppendElement(display2);

			//Update the ingredient slots
			RecipeIngredientSet set = DatalessMachineInfo.recipeIngredients[itemType];

			for(int i = 0; i < UIEntity.SlotsCount - 1; i++)
				GetSlot(i + 1).SetItem(set[i]);
		}

		private string GetMachineClassification(){
			StringBuilder classification = new StringBuilder();

			if(UIEntity is Battery)
				classification.Append("Terra Flux Storage");
			else if(UIEntity is GeneratorEntity)
				classification.Append("Terra Flux Generator");
			else if(UIEntity is PoweredMachineEntity)
				classification.Append("Powered Machine");
			else
				classification.Append("Generic Machine");

			bool hasLiquid = UIEntity is ILiquidMachine;
			bool hasGas = UIEntity is IGasMachine;
			UIEntity.HijackCanBeInteractedWithItemNetworks(out _, out bool canInput, out bool canOutput);
			bool hasInput = canInput || UIEntity.GetInputSlots().Length > 0;
			bool hasOutput = canOutput || UIEntity.GetOutputSlots().Length > 0;

			if(hasInput && hasOutput)
				classification.Append(", Input/Output");
			else if(hasInput && !hasOutput)
				classification.Append(", Input");
			else if(!hasInput && hasOutput)
				classification.Append(", Output");

			if(hasLiquid)
				classification.Append(", Liquids");

			if(hasGas)
				classification.Append(", Gases");

			return classification.ToString();
		}

		internal override void InitializeText(List<UIText> text) {
			// TODO: text for displaying what machine is in the slot
			statsText = new UIText("");
			statsText.Left.Set(100, 0);
			statsText.Top.Set(90, 0);

			descriptionText = new UIText("");
			descriptionText.Left.Set(40, 0);
			descriptionText.Top.Set(230, 0);
		}

		internal override void InitializeSlots(List<UIItemSlot> slots){
			slots.Add(item = new UIItemSlot(){
				ValidItemFunc = item => item.IsAir || item.modItem is MachineItem
			});
			item.Left.Set(40, 0);
			item.Top.Set(70, 0);

			//Recipe slots
			for(int i = 0; i < UIEntity.SlotsCount - 1; i++){
				UIItemSlot slot = new UIItemSlot(scale: 0.8f){
					IgnoreClicks = true  //Allow the item info to be viewed, just not interacted with
				};
				slot.Left.Set(20 + i * Main.inventoryBack9Texture.Width * 0.8f, 0);
				slot.Top.Set(400, 0);

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
		}

		public override void PreClose(){
			if(!item.StoredItem.IsAir){
				Main.LocalPlayer.QuickSpawnClonedItem(item.StoredItem, item.StoredItem.stack);
				item.SetItem(new Item());
			}
		}

		private void SetDisplay(ref UIScienceWorkbenchDisplay display, RegistryAnimation registry, bool leftDisplay){
			if(display != null)
				RemoveElement(display);

			var texture = ModContent.GetTexture(registry.texture);
			
			int maxDim = Math.Max(texture.Width, texture.Height);
			float scale = maxDim > 80 ? 80f / maxDim : 1f;
			const int topBase = 450;
			float top = texture.Height * scale < 80 ? topBase + 80 - texture.Height * scale : topBase;

			if(display is null)
				display = new UIScienceWorkbenchDisplay(registry.texture, registry.frame);
			else
				display.SetImage(registry.texture, registry.frame);
			display.Left.Set(leftDisplay ? 20 : 120, 0);
			display.Top.Set(top, 0);
			display.Scale = scale;
		}

		private void RemoveDisplay(ref UIScienceWorkbenchDisplay display){
			if(display != null)
				RemoveElement(display);
		}
	}
}
