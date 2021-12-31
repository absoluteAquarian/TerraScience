using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ReLogic.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameInput;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;
using TerraScience.Content.UI;
using TerraScience.Utilities;

namespace TerraScience.API.UI{
	//A lot of this code was taken directly from Magic Storage's UISearchBar class
	public class UITextPrompt : UIElement{
		private static List<UITextPrompt> prompts;

		private const int PADDING = 4;

		private readonly LocalizedText defaultText = Language.GetText("Mods.TerraScience.DefaultPromptText");
		private int cursorPosition = 0;
		public bool HasFocus{ get; private set; } = false;
		private int cursorTimer = 0;

		public event Action<UIElement> OnChanged;
		public event Action<UIElement> OnEnterPressed;

		public string Text{ get; private set; } = string.Empty;

		public bool HideTextWhenDrawn{ get; set; }

		public bool CanInteractWithMouse{ get; set; } = true;

		public UITextPrompt(){
			SetPadding(PADDING);
		}

		public UITextPrompt(LocalizedText defaultText) : this(){
			this.defaultText = defaultText;
		}

		internal static void Load(){
			prompts = new List<UITextPrompt>();
		}

		internal static void Unload(){
			prompts = null;
		}

		public void Reset(){
			Text = string.Empty;
			cursorPosition = 0;
			HasFocus = false;
			CheckBlockInput();
		}

		public override void Update(GameTime gameTime){
			cursorTimer++;
			cursorTimer %= 60;

			Rectangle dim = UIUtils.GetFullRectangle(this);
			MouseState mouse = MachineUILoader.curMouse;
			bool mouseOver = mouse.X > dim.X && mouse.X < dim.X + dim.Width && mouse.Y > dim.Y && mouse.Y < dim.Y + dim.Height;

			if(CanInteractWithMouse){
				if(MachineUILoader.LeftClick && Parent != null){
					if(!HasFocus && mouseOver){
						HasFocus = true;
						CheckBlockInput();
					}else if(HasFocus && !mouseOver){
						HasFocus = false;
						CheckBlockInput();
						cursorPosition = Text.Length;
					}
				}else if(MachineUILoader.curMouse.RightButton == ButtonState.Pressed && MachineUILoader.oldMouse.RightButton == ButtonState.Released && Parent != null && HasFocus && !mouseOver){
					HasFocus = false;
					cursorPosition = Text.Length;
					CheckBlockInput();
				}else if(MachineUILoader.curMouse.RightButton == ButtonState.Pressed && MachineUILoader.oldMouse.RightButton == ButtonState.Released && mouseOver){
					if(Text.Length > 0){
						Text = string.Empty;
						cursorPosition = 0;
					}
				}
			}else
				HasFocus = false;  //Force a de-focus

			if(HasFocus){
				PlayerInput.WritingText = true;
				Main.instance.HandleIME();
				string prev = Text;

				if(cursorPosition < Text.Length && Text.Length > 0)
					prev = prev.Remove(cursorPosition);

				string newString = Main.GetInputText(prev);
				bool changed = false;

				if(!newString.Equals(prev)){
					int newStringLength = newString.Length;

					if(prev != Text)
						newString += Text.Substring(cursorPosition);

					Text = newString;
					cursorPosition = newStringLength;
					changed = true;
				}

				if(KeyTyped(Keys.Delete) && Text.Length > 0 && cursorPosition <= Text.Length - 1){
					Text = Text.Remove(cursorPosition, 1);
					changed = true;
				}

				if(KeyTyped(Keys.Left) && cursorPosition > 0)
					cursorPosition--;

				if(KeyTyped(Keys.Right) && cursorPosition < Text.Length)
					cursorPosition++;

				if(KeyTyped(Keys.Home))
					cursorPosition = 0;

				if(KeyTyped(Keys.End))
					cursorPosition = Text.Length;

				if((Main.keyState.IsKeyDown(Keys.LeftControl) || Main.keyState.IsKeyDown(Keys.RightControl)) && KeyTyped(Keys.Back)){
					Text = string.Empty;
					cursorPosition = 0;
					changed = true;
				}

				if(changed)
					OnChanged?.Invoke(this);

				if(KeyTyped(Keys.Enter))
					OnEnterPressed?.Invoke(this);

				if(KeyTyped(Keys.Enter) || KeyTyped(Keys.Tab) || KeyTyped(Keys.Escape)){
					HasFocus = false;
					CheckBlockInput();
				}
			}

			base.Update(gameTime);
		}

		protected override void DrawSelf(SpriteBatch spriteBatch){
			Texture2D texture = ModContent.GetTexture("TerraScience/API/UI/UITextPromptBackground");
			CalculatedStyle dim = GetDimensions();
			int innerWidth = (int)dim.Width - 2 * PADDING;
			int innerHeight = (int)dim.Height - 2 * PADDING;

			Color color = CanInteractWithMouse ? Color.White : Color.Gray;

			spriteBatch.Draw(texture, dim.Position(), new Rectangle(0, 0, PADDING, PADDING), color);
			spriteBatch.Draw(texture, new Rectangle((int)dim.X + PADDING, (int)dim.Y, innerWidth, PADDING), new Rectangle(PADDING, 0, 1, PADDING), color);
			spriteBatch.Draw(texture, new Vector2(dim.X + PADDING + innerWidth, dim.Y), new Rectangle(PADDING + 1, 0, PADDING, PADDING), color);
			spriteBatch.Draw(texture, new Rectangle((int)dim.X, (int)dim.Y + PADDING, PADDING, innerHeight), new Rectangle(0, PADDING, PADDING, 1), color);
			spriteBatch.Draw(texture, new Rectangle((int)dim.X + PADDING, (int)dim.Y + PADDING, innerWidth, innerHeight), new Rectangle(PADDING, PADDING, 1, 1), color);
			spriteBatch.Draw(texture, new Rectangle((int)dim.X + PADDING + innerWidth, (int)dim.Y + PADDING, PADDING, innerHeight), new Rectangle(PADDING + 1, PADDING, PADDING, 1), color);
			spriteBatch.Draw(texture, new Vector2(dim.X, dim.Y + PADDING + innerHeight), new Rectangle(0, PADDING + 1, PADDING, PADDING), color);
			spriteBatch.Draw(texture, new Rectangle((int)dim.X + PADDING, (int)dim.Y + PADDING + innerHeight, innerWidth, PADDING), new Rectangle(PADDING, PADDING + 1, 1, PADDING), color);
			spriteBatch.Draw(texture, new Vector2(dim.X + PADDING + innerWidth, dim.Y + PADDING + innerHeight), new Rectangle(PADDING + 1, PADDING + 1, PADDING, PADDING), color);

			bool isEmpty = Text.Length == 0;
			string drawText = isEmpty ? defaultText.Value : HideTextWhenDrawn ? new string('*', Text.Length) : Text;
			DynamicSpriteFont font = Main.fontMouseText;
			Vector2 size = font.MeasureString(drawText);
			float scale = innerHeight / size.Y;

			if(isEmpty && HasFocus){
				drawText = string.Empty;
				isEmpty = false;
			}

			Color textColor = CanInteractWithMouse ? Color.Black : Color.DarkSlateGray;
			if(isEmpty)
				textColor *= 0.75f;

			spriteBatch.DrawString(font, drawText, new Vector2(dim.X + PADDING, dim.Y + PADDING), textColor, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
			if(!isEmpty && HasFocus && cursorTimer < 30){
				float drawCursor = font.MeasureString(drawText.Substring(0, cursorPosition)).X * scale;
				spriteBatch.DrawString(font, "|", new Vector2(dim.X + PADDING + drawCursor, dim.Y + PADDING), textColor, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
			}
		}

		public static bool KeyTyped(Keys key)
			=> Main.keyState.IsKeyDown(key) && !Main.oldKeyState.IsKeyDown(key);

		private static void CheckBlockInput(){
			Main.blockInput = false;

			foreach(var prompt in prompts){
				if(prompt.HasFocus){
					Main.blockInput = true;
					break;
				}
			}
		}

		public static bool AnyPromptHasFocus(){
			foreach(var prompt in prompts)
				if(prompt.HasFocus)
					return true;

			return false;
		}
	}
}
