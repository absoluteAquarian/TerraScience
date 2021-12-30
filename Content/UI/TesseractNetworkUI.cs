using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader.UI;
using Terraria.UI;
using TerraScience.API.UI;
using TerraScience.Content.TileEntities;
using TerraScience.Systems;

namespace TerraScience.Content.UI{
	public class TesseractNetworkUI : UIState{
		private UIDragablePanel panel;

		private UIText header;

		private UITesseractKnownPanel[] visibleKnownNetworkPanels;
		private UIText[] visibleKnownNetworks;
		private UIPanel knownPanel;
		private UIPanel createPanel;
		private UIPanel configPanel;

		private ClickableButton setNetwork;
		private ClickableButton createNetwork;
		private ClickableButton destroyNetwork;

		private UIPanel currentPanel;
		private UIPanel inputPassword;

		//The UI that this network UI is bound to
		internal TesseractUI boundUI;

		internal TesseractEntity BoundEntity => boundUI?.UIEntity as TesseractEntity;
		internal string BoundNetwork => (boundUI?.UIEntity as TesseractEntity)?.BoundNetwork;

		public int currentPage;
		public int pageMax;

		private string wantedBoundNetwork;
		private UIText oldWantedBoundNetwork;

		private string pendingNetworkName;
		private string pendingNetworkPassword;
		private bool pendingNetworkPrivate;

		private string passwordSuccessMessage;
		private string passwordFailMessage;

		private TesseractNetwork.Entry passwordParent;
		private Action<TesseractNetwork.Entry> onPasswordSuccess;

		internal void Open(){
			knownPanel.Remove();
			createPanel.Remove();
			configPanel.Remove();

			currentPanel = knownPanel;

			panel.Append(knownPanel);

			header.SetText("Tesseract Network");
		}

		public override void OnInitialize(){
			//Make the panel
			panel = new UIDragablePanel();

			panel.Width.Set(400, 0);
			panel.Height.Set(320, 0);
			panel.HAlign = panel.VAlign = 0.5f;
			Append(panel);

			//Make the header text
			header = new UIText("Terract Network", 1, true) {
				HAlign = 0.5f
			};
			header.Top.Set(10, 0);
			panel.Append(header);

			//Known Networks menu
			knownPanel = new UIPanel();
			knownPanel.Width.Set(0, 0.65f);
			knownPanel.Height.Set(250, 0);
			knownPanel.Left.Set(20, 0);
			knownPanel.Top.Set(50, 0);
			panel.Append(knownPanel);

			UIText dummy = new UIText("Very Cool Network\nPrivate: no");

			float dummyHeight = dummy.GetDimensions().Height;
			int rows = (int)(knownPanel.GetInnerDimensions().Height / dummyHeight - 40);

			visibleKnownNetworkPanels = new UITesseractKnownPanel[rows];
			visibleKnownNetworks = new UIText[rows];

			float top = 0;
			for(int i = 0; i < rows; i++){
				UITesseractKnownPanel networkPanel = visibleKnownNetworkPanels[i] = new UITesseractKnownPanel(i);
				networkPanel.SetPadding(4);
				networkPanel.Width.Set(0, 1f);
				networkPanel.Height.Set(dummyHeight - 8, 0);
				networkPanel.Left.Set(0, 0);
				networkPanel.Top.Set(top, 0);
				networkPanel.OnClick += (evt, e) => {
					var text = GetTextElement(e as UITesseractKnownPanel);

					if(oldWantedBoundNetwork is null){
						if(text.Text != BoundNetwork){
							wantedBoundNetwork = text.Text;

							text.TextColor = Color.Green;
						}

						oldWantedBoundNetwork = text;
						return;
					}

					if(oldWantedBoundNetwork.Text != BoundNetwork)
						oldWantedBoundNetwork.TextColor = Color.White;

					if(text.Text != BoundNetwork){
						text.TextColor = Color.Green;

						wantedBoundNetwork = text.Text;
					}else
						wantedBoundNetwork = null;

					oldWantedBoundNetwork = text;
				};

				var panelDims = networkPanel.GetDimensions();

				UIText networkText = visibleKnownNetworks[i] = new UIText("");
				networkPanel.Append(networkText);

				UIImageButton config = new UIImageButton(UICommon.ButtonModConfigTexture);
				config.Left.Set(panelDims.Width - config.Width.Pixels, 0);
				config.Top.Set(top + panelDims.Height / 2 - config.Height.Pixels, 0);
				config.OnClick += (evt, e) => {
					TesseractNetwork.TryGetEntry(GetTextElement(e.Parent as UITesseractKnownPanel).Text, out var entry);

					passwordParent = entry;

					onPasswordSuccess += passwordEntry => {
						knownPanel.Remove();

						currentPanel = configPanel;

						panel.Append(configPanel);
					};

					if(entry.entryIsPrivate)
						currentPanel.Append(inputPassword);
					else{
						onPasswordSuccess?.Invoke(entry);
						CloseInputPasswordMenu();
					}
				};
				config.SetVisibility(whenActive: 1f, whenInactive: 0.65f);
				networkPanel.Append(config);

				top += dummyHeight;
			}

			//Set the other buttons
			var knownDims = knownPanel.GetDimensions();

			UIText pageNum = new UIText("Page 0/0"){
				HAlign = 0.5f
			};
			pageNum.Top.Set(-20, 1f);
			knownPanel.Append(pageNum);
			
			var numDims = pageNum.GetDimensions();
			var numPos = numDims.Position();

			ClickableButton pageBack = new ClickableButton("<-");
			pageBack.OnClick += (evt, e) => UpdatePage(pageNum, -1);
			pageBack.Left.Set(numPos.X - 50, 0);
			pageBack.Top.Set(numPos.Y, 0);
			knownPanel.Append(pageBack);

			ClickableButton pageNext = new ClickableButton("->");
			pageNext.OnClick += (evt, e) => UpdatePage(pageNum, 1);
			pageNext.Left.Set(numPos.X + numDims.Width + 50, 0);
			pageNext.Top.Set(numPos.Y, 0);
			knownPanel.Append(pageNext);

			setNetwork = new ClickableButton("Apply");
			setNetwork.Left.Set(20, 0);
			setNetwork.Top.Set(-30, 1f);
			setNetwork.OnClick += (evt, e) => {
				if(currentPanel != knownPanel)
					return;

				if(wantedBoundNetwork != null)
					BoundEntity.BoundNetwork = wantedBoundNetwork;

				wantedBoundNetwork = null;
				oldWantedBoundNetwork = null;
			};
			panel.Append(setNetwork);

			createNetwork = new ClickableButton("Create");
			createNetwork.Left.Set(80, 0);
			createNetwork.Top.Set(-30, 1f);
			createNetwork.OnClick += (evt, e) => {
				if(currentPanel != knownPanel)
					return;

				knownPanel.Remove();
				panel.Append(createPanel);

				header.SetText("Create a Network");
			};
			panel.Append(createNetwork);

			inputPassword = new UIPanel();
			inputPassword.Width.Set(200, 0f);
			inputPassword.Height.Set(75, 0f);

			void CloseInputPasswordMenu(){
				passwordParent = null;
				onPasswordSuccess = null;

				passwordFailMessage = null;
				passwordSuccessMessage = null;

				inputPassword.Remove();
			}

			UITextPrompt inputPasswordPrompt = new UITextPrompt(Language.GetText("Mods.TerraScience.InputPassword"));
			inputPasswordPrompt.Width.Set(-20, 1f);
			inputPasswordPrompt.Height.Set(32f, 0f);
			inputPasswordPrompt.Left.Set(10, 0f);
			inputPasswordPrompt.Top.Set(10, 0f);
			inputPasswordPrompt.OnEnterPressed += e => {
				if(passwordParent is null)
					return;

				var prompt = e as UITextPrompt;

				if(prompt.Text != passwordParent.password)
					Main.NewText("[TESSERACT] Access denied.  " + passwordFailMessage, Color.Red);
				else{
					Main.NewText("[TESSERACT] Access granted.  " + passwordSuccessMessage, Color.Green);

					onPasswordSuccess?.Invoke(passwordParent);
				}

				CloseInputPasswordMenu();
			};
			inputPassword.Append(inputPasswordPrompt);

			ClickableButton inputPasswordCancel = new ClickableButton("Cancel");
			inputPasswordCancel.Left.Set(-80, 1f);
			inputPasswordCancel.Top.Set(50, 0f);
			inputPasswordCancel.OnClick += (evt, e) => CloseInputPasswordMenu();
			inputPassword.Append(inputPasswordCancel);

			destroyNetwork = new ClickableButton("Delete");
			destroyNetwork.Left.Set(140, 0);
			destroyNetwork.Top.Set(-30, 1f);
			destroyNetwork.OnClick += (evt, e) => {
				if(currentPanel != knownPanel || wantedBoundNetwork is null || !TesseractNetwork.TryGetEntry(wantedBoundNetwork, out var entry))
					return;

				passwordParent = entry;

				onPasswordSuccess += passwordEntry => {
					TesseractNetwork.RemoveEntry(passwordEntry);

					if(BoundNetwork == wantedBoundNetwork)
						BoundEntity.BoundNetwork = null;

					TesseractNetwork.UpdateNetworkUIEntries(visibleKnownNetworks, ref currentPage, out pageMax, BoundNetwork);

					wantedBoundNetwork = null;
					oldWantedBoundNetwork = null;
				};

				if(entry.entryIsPrivate){
					passwordSuccessMessage = $"Deleting network \"{entry.name}\".";
				
					currentPanel.Append(inputPassword);
				}else{
					onPasswordSuccess?.Invoke(entry);
					CloseInputPasswordMenu();
				}
			};
			panel.Append(destroyNetwork);

			//Create Network menu
			createPanel = new UIPanel();
			createPanel.Width.Set(360, 0);
			createPanel.Height.Set(250, 0);
			createPanel.Left.Set(20, 0);
			createPanel.Top.Set(50, 0);

			UITextPrompt namePrompt = new UITextPrompt(Language.GetText("Mods.TerraScience.EnterTessseractNetworkName"));
			namePrompt.Left.Set(20, 0f);
			namePrompt.Top.Set(20, 0f);
			namePrompt.Width.Set(-40, 1f);
			namePrompt.Height.Set(32f, 0f);
			namePrompt.OnChanged += e => pendingNetworkName = (e as UITextPrompt).Text;
			createPanel.Append(namePrompt);

			UITextPrompt passwordPrompt = new UITextPrompt(Language.GetText("Mods.TerraScience.EnterTessseractNetworkPassword")){
				HideTextWhenDrawn = true
			};
			passwordPrompt.Left.Set(20, 0f);
			passwordPrompt.Top.Set(80, 0f);
			passwordPrompt.Width.Set(-40, 1f);
			passwordPrompt.Height.Set(32f, 0f);
			passwordPrompt.OnChanged += e => pendingNetworkPassword = (e as UITextPrompt).Text;
			createPanel.Append(passwordPrompt);

			UIToggleLabel networkPrivate = new UIToggleLabel("Private network", defaultState: false);
			networkPrivate.Left.Set(30, 0f);
			networkPrivate.Top.Set(150, 0f);
			//Public networks don't have passwords
			networkPrivate.OnClick += (evt, e) => passwordPrompt.CanInteractWithMouse = pendingNetworkPrivate = !pendingNetworkPrivate;
			createPanel.Append(networkPrivate);

			UIToggleLabel hidePassword = new UIToggleLabel("Hide password", defaultState: true);
			hidePassword.Left.Set(120, 0f);
			hidePassword.Top.Set(150, 0f);
			hidePassword.OnClick += (evt, e) => passwordPrompt.HideTextWhenDrawn = !passwordPrompt.HideTextWhenDrawn;
			createPanel.Append(hidePassword);

			ClickableButton createConfirm = new ClickableButton("Create Network");
			createConfirm.Left.Set(20, 0f);
			createConfirm.Top.Set(-40, 1f);
			createConfirm.OnClick += (evt, e) => {
				if(string.IsNullOrWhiteSpace(pendingNetworkName) || (pendingNetworkPrivate && string.IsNullOrWhiteSpace(pendingNetworkPassword)))
					return;

				if(TesseractNetwork.TryGetEntry(pendingNetworkName, out _)){
					Main.NewText($"[TESSERACT] Network of name \"{pendingNetworkName}\" already exists.", Color.Red);
					return;
				}

				TesseractNetwork.Entry entry = new TesseractNetwork.Entry(pendingNetworkName,
					pendingNetworkPrivate,
					pendingNetworkPassword);

				TesseractNetwork.RegisterEntry(entry);

				namePrompt.Reset();
				passwordPrompt.Reset();
				networkPrivate.SetState(false);

				pendingNetworkName = "";
				pendingNetworkPassword = "";
				pendingNetworkPrivate = false;

				Main.PlaySound(SoundID.MenuTick);

				Main.NewText($"[TESSERACT] Network of name \"{entry.name}\" (private: {entry.entryIsPrivate}) was created.");

				header.SetText("Tesseract Network");

				currentPanel = knownPanel;
				createPanel.Remove();
				panel.Append(knownPanel);
			};
			createPanel.Append(createConfirm);

			//Edit Config menu
			configPanel = new UIPanel();
			configPanel.Width.Set(360, 0);
			configPanel.Height.Set(120, 0);
			configPanel.Left.Set(20, 0);
			configPanel.Top.Set(50, 0);

			UITextPrompt configPasswordPrompt = new UITextPrompt(Language.GetText("Mods.TerraScience.EnterNewNetworkPassword")){
				HideTextWhenDrawn = true
			};
			configPasswordPrompt.Left.Set(20, 0f);
			configPasswordPrompt.Top.Set(20, 0f);
			configPasswordPrompt.Width.Set(-40, 1f);
			configPasswordPrompt.Height.Set(32f, 0f);
			configPasswordPrompt.OnChanged += e => pendingNetworkPassword = (e as UITextPrompt).Text;
			configPanel.Append(configPasswordPrompt);

			UIToggleLabel configNetworkPrivate = new UIToggleLabel("Private network", defaultState: false);
			configNetworkPrivate.Left.Set(30, 0f);
			configNetworkPrivate.Top.Set(90, 0f);
			//Public networks don't have passwords
			configNetworkPrivate.OnClick += (evt, e) => configPasswordPrompt.CanInteractWithMouse = pendingNetworkPrivate = !pendingNetworkPrivate;
			configPanel.Append(configNetworkPrivate);

			UIToggleLabel configHidePassword = new UIToggleLabel("Hide password", defaultState: true);
			configHidePassword.Left.Set(120, 0f);
			configHidePassword.Top.Set(90, 0f);
			configHidePassword.OnClick += (evt, e) => configPasswordPrompt.HideTextWhenDrawn = !configPasswordPrompt.HideTextWhenDrawn;
			configPanel.Append(configHidePassword);

			ClickableButton configSave = new ClickableButton("Save");
			configSave.Left.Set(240, 0f);
			configSave.Top.Set(90, 0f);
			configSave.OnClick += (evt, e) => {
				TesseractNetwork.UpdateIsPrivate(passwordParent.name, pendingNetworkPrivate);
				TesseractNetwork.UpdatePassword(passwordParent.name, pendingNetworkPrivate ? pendingNetworkPassword : null);

				currentPanel = knownPanel;

				configPanel.Remove();
				panel.Append(knownPanel);

				Main.PlaySound(SoundID.MenuTick);
			};
			configPanel.Append(configSave);
		}

		public UIText GetTextElement(UITesseractKnownPanel panel)
			=> visibleKnownNetworks[panel.PageSlot];

		private void UpdatePage(UIText pageNum, int offset){
			currentPage += offset;

			int oldPage = currentPage;
			TesseractNetwork.UpdateNetworkUIEntries(visibleKnownNetworks, ref currentPage, out pageMax, BoundNetwork);

			//Hide all panels, then make only the ones with non-empty text visible
			for(int i = 0; i < visibleKnownNetworkPanels.Length; i++)
				visibleKnownNetworkPanels[i].Remove();

			for(int i = 0; i < visibleKnownNetworks.Length; i++)
				if(visibleKnownNetworks[i].Text != "")
					knownPanel.Append(visibleKnownNetworkPanels[i]);

			//It would be weird for sounds to play when the page didn't change
			if(currentPage != oldPage){
				Main.PlaySound(SoundID.MenuTick);
				oldWantedBoundNetwork = null;
				wantedBoundNetwork = null;
			}

			pageNum.SetText($"Page {currentPage}/{pageMax}");
		}

		public override void Update(GameTime gameTime){
			base.Update(gameTime);

			Main.playerInventory = true;

			if(Main.LocalPlayer.GetModPlayer<TerraSciencePlayer>().InventoryKeyPressed){
				TechMod.Instance.machineLoader.tesseractNetworkInterface.SetState(null);
				Main.playerInventory = false;

				oldWantedBoundNetwork = null;
				wantedBoundNetwork = null;

				currentPanel = knownPanel;
				panel.Append(knownPanel);

				createPanel.Remove();
				configPanel.Remove();

				currentPage = 1;
			}
		}
	}
}
