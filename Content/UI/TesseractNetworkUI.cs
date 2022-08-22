using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
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
		private UITextPrompt inputPasswordPrompt;
		private UIToggleLabel inputHidePassword;

		private UITextPrompt namePrompt, passwordPrompt;
		private UIToggleLabel networkPrivate, hidePassword;

		private UIText pageNum;

		//The UI that this network UI is bound to
		internal TesseractUI boundUI;

		internal TesseractEntity BoundEntity => boundUI?.UIEntity as TesseractEntity;
		internal string BoundNetwork => (boundUI?.UIEntity as TesseractEntity)?.BoundNetwork;

		public int currentPage = 1;
		private int pageMax = 1;
		public int PageMax => pageMax;

		private string wantedBoundNetwork;
		private UIText oldWantedBoundNetwork;

		private string pendingNetworkName;
		private string pendingNetworkPassword;
		private bool pendingNetworkPrivate;

		private string passwordSuccessMessage;
		private string passwordFailMessage;

		private TesseractNetwork.Entry passwordParent;
		private TesseractNetwork.Entry configTarget;
		private Action<TesseractNetwork.Entry> onPasswordSuccess;

		internal void Open(TesseractUI uiToBindTo){
			if(boundUI != null)
				return;

			knownPanel.Remove();
			createPanel.Remove();
			configPanel.Remove();

			currentPanel = knownPanel;

			panel.Append(knownPanel);

			header.SetText("Tesseract Network");

			boundUI = uiToBindTo;

			TechMod.Instance.machineLoader.tesseractNetworkInterface.SetState(this);

			UpdatePage(0);
		}

		internal void Close(){
			if(boundUI is null)
				return;

			TechMod.Instance.machineLoader.tesseractNetworkInterface.SetState(null);

			oldWantedBoundNetwork = null;
			wantedBoundNetwork = null;

			currentPanel = knownPanel;
			panel.Append(knownPanel);

			createPanel.Remove();
			configPanel.Remove();

			currentPage = 1;

			boundUI = null;

			namePrompt.Reset();
			passwordPrompt.Reset();
			passwordPrompt.HideTextWhenDrawn = true;
			passwordPrompt.CanInteractWithMouse = false;
			networkPrivate.SetState(false);

			pendingNetworkName = "";
			pendingNetworkPassword = "";
			pendingNetworkPrivate = false;

			CloseInputPasswordMenu();
		}

		private void CloseInputPasswordMenu(){
			passwordParent = null;
			onPasswordSuccess = null;

			passwordFailMessage = null;
			passwordSuccessMessage = null;

			inputPassword.Remove();

			inputHidePassword.SetState(true);

			inputPasswordPrompt.Reset();
			inputPasswordPrompt.HideTextWhenDrawn = true;
		}

		private string GetTextNetworkName(UIText text)
			=> text.Text.Substring(0, text.Text.IndexOf('\n'));

		public override void OnInitialize(){
			//Make the panel
			panel = new UIDragablePanel();

			panel.Width.Set(600, 0);
			panel.Height.Set(500, 0);
			panel.HAlign = panel.VAlign = 0.5f;
			Append(panel);

			//Make the header text
			header = new UIText("Terract Network", 1, true) {
				HAlign = 0.5f
			};
			header.Top.Set(10, 0);
			panel.Append(header);

			ClickableButton close = new ClickableButton("[X]");
			close.Left.Set(-50, 1f);
			close.Top.Set(8, 0f);
			close.OnClick += (evt, e) => Close();
			panel.Append(close);

			//Known Networks menu
			knownPanel = new UIPanel();
			knownPanel.Width.Set(0, 0.65f);
			knownPanel.Height.Set(350, 0);
			knownPanel.Left.Set(20, 0);
			knownPanel.Top.Set(50, 0);
			panel.Append(knownPanel);

			float textHeight = FontAssets.MouseText.Value.MeasureString("Very Cool Network\nPrivate: no").Y;

			const int knownPanelBufferBottom = 40;
			int rows = (int)((knownPanel.GetInnerDimensions().Height - knownPanelBufferBottom - 30) / textHeight);

			visibleKnownNetworkPanels = new UITesseractKnownPanel[rows];
			visibleKnownNetworks = new UIText[rows];

			float top = 0;
			for(int i = 0; i < rows; i++){
				UITesseractKnownPanel networkPanel = visibleKnownNetworkPanels[i] = new UITesseractKnownPanel(i);
				networkPanel.SetPadding(4);
				networkPanel.Width.Set(0, 1f);
				networkPanel.Height.Set(textHeight, 0);
				networkPanel.Left.Set(0, 0);
				networkPanel.Top.Set(top, 0);
				networkPanel.OnClick += (evt, e) => {
					var uiPanel = e as UITesseractKnownPanel;
					var text = GetTextElement(uiPanel);

					if(oldWantedBoundNetwork is null){
						if(text.Text != BoundNetwork){
							wantedBoundNetwork = GetTextNetworkName(text);

							text.TextColor = Color.Green;
						}

						oldWantedBoundNetwork = text;
						return;
					}

					if(oldWantedBoundNetwork.Text != BoundNetwork)
						oldWantedBoundNetwork.TextColor = Color.White;

					if(text.Text != BoundNetwork){
						text.TextColor = Color.Green;

						wantedBoundNetwork = GetTextNetworkName(text);
					}else
						wantedBoundNetwork = null;

					oldWantedBoundNetwork = text;
				};

				UIText networkText = visibleKnownNetworks[i] = new UIText("");
				networkPanel.Append(networkText);

				UIImageButton config = new UIImageButton(UICommon.ButtonModConfigTexture);
				config.Left.Set(-config.Width.Pixels - 10, 1f);
				config.Top.Set(-config.Height.Pixels / 2f, 0.5f);
				config.OnClick += (evt, e) => {
					if(inputPassword.Parent != null || !TesseractNetwork.TryGetEntry(GetTextNetworkName(GetTextElement(e.Parent as UITesseractKnownPanel)), out var entry) || !entry.entryIsPrivate)
						return;

					if(TerraSciencePlayer.LocalPlayerHasAdmin){
						Main.NewText("[TESSERACT] Administrator privileges detected.  Bypassing password requirement.", Color.Green);
						return;
					}

					configTarget = passwordParent = entry;

					onPasswordSuccess += passwordEntry => {
						TechMod.Instance.machineLoader.OnUpdateOnce += () => {
							knownPanel.Remove();

							currentPanel = configPanel;

							panel.Append(configPanel);
						};
					};

					currentPanel.Append(inputPassword);
				};
				config.SetVisibility(whenActive: 1f, whenInactive: 0.65f);
				networkPanel.Append(config);

				top += textHeight + 30 - 16;
			}

			//Set the other buttons
			pageNum = new UIText("Page 1/1"){
				HAlign = 0.5f
			};
			pageNum.Top.Set(-knownPanelBufferBottom + 15, 1f);
			knownPanel.Append(pageNum);

			ClickableButton pageBack = new ClickableButton("<-");
			pageBack.OnClick += (evt, e) => UpdatePage(-1);
			pageBack.Left.Set(-60 - pageBack.MinWidth.Pixels / 2f, 0.5f);
			pageBack.Top.Set(-knownPanelBufferBottom, 1f);
			knownPanel.Append(pageBack);

			ClickableButton pageNext = new ClickableButton("->");
			pageNext.OnClick += (evt, e) => UpdatePage(1);
			pageNext.Left.Set(20 + pageBack.MinWidth.Pixels / 2f, 0.5f);
			pageNext.Top.Set(-knownPanelBufferBottom, 1f);
			knownPanel.Append(pageNext);

			setNetwork = new ClickableButton("Apply");
			setNetwork.Left.Set(20, 0);
			setNetwork.Top.Set(-40, 1f);
			setNetwork.OnClick += (evt, e) => {
				if(inputPassword.Parent != null || currentPanel != knownPanel)
					return;

				if(wantedBoundNetwork != null)
					BoundEntity.BoundNetwork = wantedBoundNetwork;
				else
					return;

				wantedBoundNetwork = null;
				oldWantedBoundNetwork = null;

				SoundEngine.PlaySound(SoundID.MenuTick);

				Main.NewText($"[Tesseract] Successfully bound entity to network \"{BoundNetwork}\".", Color.Green);

				UpdatePage(0);
			};
			panel.Append(setNetwork);

			createNetwork = new ClickableButton("Create");
			createNetwork.Left.Set(150, 0);
			createNetwork.Top.Set(-40, 1f);
			createNetwork.OnClick += (evt, e) => {
				if(inputPassword.Parent != null || currentPanel != knownPanel)
					return;

				TechMod.Instance.machineLoader.OnUpdateOnce += () => {
					knownPanel.Remove();
					panel.Append(createPanel);
				};

				header.SetText("Create a Network");

				SoundEngine.PlaySound(SoundID.MenuTick);
			};
			panel.Append(createNetwork);

			inputPassword = new UIPanel();
			inputPassword.Width.Set(400, 0f);
			inputPassword.Height.Set(75, 0f);

			inputHidePassword = new UIToggleLabel("Hide password", defaultState: true);
			inputHidePassword.Left.Set(30, 0f);
			inputHidePassword.Top.Set(50, 0f);
			inputPassword.Append(inputHidePassword);

			inputPasswordPrompt = new UITextPrompt(Language.GetText("Mods.TerraScience.InputPassword"));
			inputPasswordPrompt.Width.Set(-20, 1f);
			inputPasswordPrompt.Height.Set(32f, 0f);
			inputPasswordPrompt.Left.Set(10, 0f);
			inputPasswordPrompt.Top.Set(10, 0f);
			inputPasswordPrompt.OnEnterPressed += e => {
				if(passwordParent is null)
					return;

				var prompt = e as UITextPrompt;

				bool release = TechMod.Release;
				if(!release)
					Main.NewText($"Checking input password \"{prompt.Text}\" against entry password \"{passwordParent.password}\"...");

				if(prompt.Text != passwordParent.password)
					Main.NewText("[TESSERACT] Access denied.  " + passwordFailMessage, Color.Red);
				else{
					Main.NewText("[TESSERACT] Access granted.  " + passwordSuccessMessage, Color.Green);

					onPasswordSuccess?.Invoke(passwordParent);
				}

				inputHidePassword.SetState(true);
				inputPasswordPrompt.HideTextWhenDrawn = true;

				TechMod.Instance.machineLoader.OnUpdateOnce += CloseInputPasswordMenu;
			};
			inputPassword.Append(inputPasswordPrompt);

			//Have to set it here since "inputPasswordPrompt" doesn't exist above
			inputHidePassword.OnClick += (evt, e) => inputPasswordPrompt.HideTextWhenDrawn = !inputPasswordPrompt.HideTextWhenDrawn;

			ClickableButton inputPasswordCancel = new ClickableButton("Cancel");
			inputPasswordCancel.Left.Set(-80, 1f);
			inputPasswordCancel.Top.Set(50, 0f);
			inputPasswordCancel.OnClick += (evt, e) => TechMod.Instance.machineLoader.OnUpdateOnce += CloseInputPasswordMenu;
			inputPassword.Append(inputPasswordCancel);

			destroyNetwork = new ClickableButton("Delete");
			destroyNetwork.Left.Set(280, 0);
			destroyNetwork.Top.Set(-40, 1f);
			destroyNetwork.OnClick += (evt, e) => {
				if(inputPassword.Parent != null || currentPanel != knownPanel || wantedBoundNetwork is null || !TesseractNetwork.TryGetEntry(wantedBoundNetwork, out var entry))
					return;

				passwordParent = entry;

				onPasswordSuccess += passwordEntry => {
					TesseractNetwork.RemoveEntry(passwordEntry);

					if(BoundNetwork == wantedBoundNetwork)
						BoundEntity.BoundNetwork = null;

					TesseractNetwork.UpdateNetworkUIEntries(visibleKnownNetworks, ref currentPage, out pageMax, BoundNetwork);

					wantedBoundNetwork = null;
					oldWantedBoundNetwork = null;

					UpdatePage(0);
				};

				if(entry.entryIsPrivate && !TerraSciencePlayer.LocalPlayerHasAdmin){
					passwordSuccessMessage = $"Deleting network \"{entry.name}\".";
				
					currentPanel.Append(inputPassword);
				}else{
					if(TerraSciencePlayer.LocalPlayerHasAdmin)
						Main.NewText("[TESSERACT] Administrator privileges detected.  Bypassing password requirement.", Color.Green);

					onPasswordSuccess?.Invoke(entry);
					TechMod.Instance.machineLoader.OnUpdateOnce += CloseInputPasswordMenu;
				}

				SoundEngine.PlaySound(SoundID.MenuTick);
			};
			panel.Append(destroyNetwork);

			//Create Network menu
			createPanel = new UIPanel();
			createPanel.Width.Set(360, 0);
			createPanel.Height.Set(250, 0);
			createPanel.Left.Set(20, 0);
			createPanel.Top.Set(50, 0);

			namePrompt = new UITextPrompt(Language.GetText("Mods.TerraScience.EnterTessseractNetworkName"));
			namePrompt.Left.Set(20, 0f);
			namePrompt.Top.Set(20, 0f);
			namePrompt.Width.Set(-40, 1f);
			namePrompt.Height.Set(32f, 0f);
			namePrompt.OnChanged += e => pendingNetworkName = (e as UITextPrompt).Text;
			createPanel.Append(namePrompt);

			passwordPrompt = new UITextPrompt(Language.GetText("Mods.TerraScience.EnterTessseractNetworkPassword")){
				HideTextWhenDrawn = true,
				CanInteractWithMouse = false
			};
			passwordPrompt.Left.Set(20, 0f);
			passwordPrompt.Top.Set(80, 0f);
			passwordPrompt.Width.Set(-40, 1f);
			passwordPrompt.Height.Set(32f, 0f);
			passwordPrompt.OnChanged += e => pendingNetworkPassword = (e as UITextPrompt).Text;
			createPanel.Append(passwordPrompt);

			networkPrivate = new UIToggleLabel("Private network", defaultState: false);
			networkPrivate.Left.Set(30, 0f);
			networkPrivate.Top.Set(150, 0f);
			//Public networks don't have passwords
			networkPrivate.OnClick += (evt, e) => passwordPrompt.CanInteractWithMouse = pendingNetworkPrivate = !pendingNetworkPrivate;
			createPanel.Append(networkPrivate);

			hidePassword = new UIToggleLabel("Hide password", defaultState: true);
			hidePassword.Left.Set(200, 0f);
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
				passwordPrompt.HideTextWhenDrawn = true;
				passwordPrompt.CanInteractWithMouse = false;
				networkPrivate.SetState(false);

				pendingNetworkName = "";
				pendingNetworkPassword = "";
				pendingNetworkPrivate = false;

				SoundEngine.PlaySound(SoundID.MenuTick);

				Main.NewText($"[TESSERACT] Network of name \"{entry.name}\" (private: {entry.entryIsPrivate}) was created.", Color.Green);

				header.SetText("Tesseract Network");

				TechMod.Instance.machineLoader.OnUpdateOnce += () => {
					currentPanel = knownPanel;
					createPanel.Remove();
					panel.Append(knownPanel);
				};
			};
			createPanel.Append(createConfirm);

			//Edit Config menu
			configPanel = new UIPanel();
			configPanel.Width.Set(360, 0);
			configPanel.Height.Set(150, 0);
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

			UIToggleLabel configHidePassword = new UIToggleLabel("Hide password", defaultState: true);
			configHidePassword.Left.Set(30, 0f);
			configHidePassword.Top.Set(60, 0f);
			configHidePassword.OnClick += (evt, e) => configPasswordPrompt.HideTextWhenDrawn = !configPasswordPrompt.HideTextWhenDrawn;
			configPanel.Append(configHidePassword);

			ClickableButton configSave = new ClickableButton("Save");
			configSave.Left.Set(240, 0f);
			configSave.Top.Set(90, 0f);
			configSave.OnClick += (evt, e) => {
				TesseractNetwork.UpdateIsPrivate(configTarget.name, pendingNetworkPrivate);
				TesseractNetwork.UpdatePassword(configTarget.name, pendingNetworkPrivate ? pendingNetworkPassword : null);

				TechMod.Instance.machineLoader.OnUpdateOnce += () => {
					currentPanel = knownPanel;

					configPanel.Remove();
					panel.Append(knownPanel);
				};

				SoundEngine.PlaySound(SoundID.MenuTick);

				Main.NewText($"[TESSERACT] Network \"{configTarget.name}\" had its password changed.");

				configTarget = null;

				pendingNetworkPassword = "";
				pendingNetworkPrivate = false;

				configHidePassword.SetState(true);
				configPasswordPrompt.HideTextWhenDrawn = true;
			};
			configPanel.Append(configSave);
		}

		public UIText GetTextElement(UITesseractKnownPanel panel)
			=> visibleKnownNetworks[panel.PageSlot];

		private void UpdatePage(int offset){
			currentPage += offset;

			int oldPage = currentPage;
			TesseractNetwork.UpdateNetworkUIEntries(visibleKnownNetworks, ref currentPage, out pageMax, BoundNetwork);

			TechMod.Instance.machineLoader.OnUpdateOnce += () => {
				//Hide all panels, then make only the ones with non-empty text visible
				for(int i = 0; i < visibleKnownNetworkPanels.Length; i++)
					visibleKnownNetworkPanels[i].Remove();

				for(int i = 0; i < visibleKnownNetworks.Length; i++)
					if(visibleKnownNetworks[i].Text != "")
						knownPanel.Append(visibleKnownNetworkPanels[i]);
			};

			//It would be weird for sounds to play when the page limits have been reached
			if(currentPage == oldPage){
				SoundEngine.PlaySound(SoundID.MenuTick);
				oldWantedBoundNetwork = null;
				wantedBoundNetwork = null;
			}

			pageNum.SetText($"Page {currentPage}/{pageMax}");
		}

		public override void Update(GameTime gameTime){
			base.Update(gameTime);

			Main.playerInventory = true;

			if(Main.LocalPlayer.GetModPlayer<TerraSciencePlayer>().InventoryKeyPressed){
				Main.playerInventory = false;
				Close();
			}
		}
	}
}
