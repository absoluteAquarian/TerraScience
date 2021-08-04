﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.GameInput;
using Terraria.UI;
using TerraScience.Content.UI;

namespace TerraScience.API.UI {
	public class UIItemSlotWrapper : UIElement {
		// TODO: CaptionedUIItemSlot; inherits from UIItemSlotWrapper and displays some text above itself, might be useful for machine slot info

		private int Context { get; set; }

		public float Scale { get; private set; }

		public Item StoredItem => storedItem;

		private Item storedItem;

		private Item storedItemBeforeHandle;

		public bool ItemChanged => StoredItem != null && storedItemBeforeHandle != null && StoredItem.IsNotTheSameAs(storedItemBeforeHandle);
		public bool ItemTypeChanged => (StoredItem?.type ?? -1) != (storedItemBeforeHandle?.type ?? -2);

		public Func<Item, bool> ValidItemFunc;

		public UIItemSlotWrapper(int context = ItemSlot.Context.BankItem, float scale = 1f) {
			Context = context;
			Scale = scale;

			storedItem = new Item();
			storedItem.SetDefaults();

			Width.Set(TextureAssets.InventoryBack9.Value.Width * scale, 0f);
			Height.Set(TextureAssets.InventoryBack9.Value.Height * scale, 0f);
		}

		protected override void DrawSelf(SpriteBatch spriteBatch) {
			float oldScale = Main.inventoryScale;
			Main.inventoryScale = Scale;
			Rectangle rectangle = GetDimensions().ToRectangle();

			//Lazy hardcoding lol
			bool ignoreClicks = Parent is UIDragablePanel panel && panel.Parent is MachineUI ui && ui.UIDelay > 0;
			if ((ContainsPoint(Main.MouseScreen) && !PlayerInput.IgnoreMouseInterface)) {
				Main.LocalPlayer.mouseInterface = true;

				if(Parent is UIDragablePanel panel2)
					panel2.Dragging = false;

				if (!ignoreClicks && (ValidItemFunc == null || ValidItemFunc(Main.mouseItem))) {
					// Handle handles all the click and hover actions based on the context.
					storedItemBeforeHandle = StoredItem.Clone();
					ItemSlot.Handle(ref storedItem, Context);
				}
			}

			// Draw draws the slot itself and Item. Depending on context, the color will change, as will drawing other things like stack counts.
			ItemSlot.Draw(spriteBatch, ref storedItem, Context, rectangle.TopLeft());

			Main.inventoryScale = oldScale;
		}

		public void SetItem(Item item){
			storedItem = item.Clone();
		}

		public void SetItem(int itemType, int stack = 1){
			storedItem.SetDefaults(itemType);
			StoredItem.stack = stack;
		}
	}
}