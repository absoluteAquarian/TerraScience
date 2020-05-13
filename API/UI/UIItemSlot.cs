using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.GameInput;
using Terraria.UI;
using TerraScience.API.UI;

namespace TerraScience.Content.API.UI {
	public class UIItemSlot : UIElement {
		private int Context { get; set; }

		public float Scale { get; private set; }

		public Item StoredItem => storedItem;

		private Item storedItem;

		private Item storedItemBeforeHandle;

		public bool ItemChanged => StoredItem != null && storedItemBeforeHandle != null && StoredItem.IsNotTheSameAs(storedItemBeforeHandle);

		public Func<Item, bool> ValidItemFunc;

		public UIItemSlot(int context = ItemSlot.Context.BankItem, float scale = 1f) {
			Context = context;
			Scale = scale;

			storedItem = new Item();
			storedItem.SetDefaults();

			Width.Set(Main.inventoryBack9Texture.Width * scale, 0f);
			Height.Set(Main.inventoryBack9Texture.Height * scale, 0f);
		}

		protected override void DrawSelf(SpriteBatch spriteBatch) {
			float oldScale = Main.inventoryScale;
			Main.inventoryScale = Scale;
			Rectangle rectangle = GetDimensions().ToRectangle();

			if (ContainsPoint(Main.MouseScreen) && !PlayerInput.IgnoreMouseInterface) {
				Main.LocalPlayer.mouseInterface = true;

				if(Parent is UIDragablePanel panel)
					panel.Dragging = false;

				if (ValidItemFunc == null || ValidItemFunc(Main.mouseItem)) {
					// Handle handles all the click and hover actions based on the context.
					storedItemBeforeHandle = StoredItem.Clone();
					ItemSlot.Handle(ref storedItem, Context);
				}
			}

			// Draw draws the slot itself and Item. Depending on context, the color will change, as will drawing other things like stack counts.
			ItemSlot.Draw(spriteBatch, ref storedItem, Context, rectangle.TopLeft());

			Main.inventoryScale = oldScale;
		}

		public void SetItem(Item item, int stack = 1) {
			storedItem.SetDefaults(item.type);
			storedItem.stack = stack;
		}

		public void SetItem(int itemType) => storedItem.SetDefaults(itemType);
	}
}