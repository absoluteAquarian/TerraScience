using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.Utilities;
using TerraScience.Content.Items.Tools;
using TerraScience.Utilities;

namespace TerraScience.Content.NPCs.Bill{
	public class BillNPC : ModNPC{
		public override string Texture => "TerraScience/Content/NPCs/Bill/NPC_19_OLD";

		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Bill Nye the Science Guy");
			Main.npcFrameCount[NPC.type] = Main.npcFrameCount[NPCID.ArmsDealer];
			NPCID.Sets.ExtraFramesCount[NPC.type] = NPCID.Sets.ExtraFramesCount[NPCID.ArmsDealer];
			NPCID.Sets.AttackFrameCount[NPC.type] = 4;
			NPCID.Sets.DangerDetectRange[NPC.type] = NPCID.Sets.DangerDetectRange[NPCID.DyeTrader];
			NPCID.Sets.AttackType[NPC.type] = 3;	//Overhead sword swing
			NPCID.Sets.AttackTime[NPC.type] = 16;
			NPCID.Sets.AttackAverageChance[NPC.type] = 30;
			NPCID.Sets.HatOffsetY[NPC.type] = NPCID.Sets.HatOffsetY[NPCID.ArmsDealer];
		}

		public override void SetDefaults(){
			NPC.townNPC = true;
			NPC.friendly = true;
			NPC.width = 18;
			NPC.height = 40;
			NPC.aiStyle = 7;
			NPC.damage = 24;
			NPC.defense = 18;
			NPC.lifeMax = 250;
			NPC.HitSound = SoundID.NPCHit1;
			NPC.DeathSound = SoundID.NPCDeath1;
            NPC.knockBackResist = 0.5f;
			AnimationType = NPCID.ArmsDealer;
		}

		public override void SetChatButtons(ref string button, ref string button2){
			button = Language.GetTextValue("LegacyInterface.28");  //"Shop"
			button2 = "Menus";
		}

		public override string GetChat(){
			WeightedRandom<string> text = new WeightedRandom<string>(Main.rand);

			text.Add("Science rules!");
			text.Add("Inertia is a property of matter.");
			text.Add("Consider the following: buying from my shop!");
			text.Add("What? No, I'm not going to talk about that. Science is about solving problems, not about discussing philosophy!");

			int wizard = NPC.FindFirstNPC(NPCID.Wizard);
			if(wizard >= 0 && Main.rand.NextBool(4))
				text.Add($"Can you tell {Main.npc[wizard].GivenName} to stop giving me his weird potions?");

			int guide = NPC.FindFirstNPC(NPCID.Guide);
			if(guide >= 0 && NPC.downedBoss3 && !Main.hardMode && Main.rand.NextBool(4))
				text.Add($"Hmm... {Main.npc[guide].GivenName} seems distressed about something. I wonder what it could be?");

			int witchDoctor = NPC.FindFirstNPC(NPCID.WitchDoctor);
			if(witchDoctor >= 0 && Main.rand.NextBool(4))
				text.Add($"I keep trying to tell {Main.npc[witchDoctor].GivenName} that I don't believe his voodoo mumbo-jumbo, but he just won't seem to listen!");

			int barkeep = NPC.FindFirstNPC(NPCID.DD2Bartender);
			if(barkeep >= 0 && Main.rand.NextBool(4))
				text.Add("There has to be some sort of scientific reason behind the portals created by the Old One's Army, there just has to be!  I refuse to believe that it's magic!");

			if(Main.LocalPlayer.HasItem(ItemID.PortalGun))
				text.Add("That's an interesting... tool you've got there.  From Apple-ture Labs you say?  Now that's thinking with portals!");

			return text.Get();
		}

		public override void OnChatButtonClicked(bool firstButton, ref bool shop){
			if(firstButton)
				shop = true;
		}

		public override void SetupShop(Chest shop, ref int nextSlot){
			ShopUtils.AddItemToShop(shop, ref nextSlot, ModContent.ItemType<Battery9V>());
		}

		public override void TownNPCAttackStrength(ref int damage, ref float knockback){
			damage = NPC.damage;
			knockback = 4f;
		}

		public override void TownNPCAttackCooldown(ref int cooldown, ref int randExtraCooldown){
			cooldown = 35;
			randExtraCooldown = 20;
		}

		public override void DrawTownAttackSwing(ref Texture2D item, ref int itemSize, ref float scale, ref Vector2 offset){
			item = TextureAssets.Item[ModContent.ItemType<Crowbar>()].Value;
			scale = 0.9f;
		}

		public override void TownNPCAttackSwing(ref int itemWidth, ref int itemHeight) {
			itemWidth = 30;
			itemHeight = 30;
		}

		public override bool CanGoToStatue(bool toKingStatue) => toKingStatue;

		public override void ModifyNPCLoot(NPCLoot loot){
			loot.Add(new Terraria.GameContent.ItemDropRules.CommonDrop(ModContent.ItemType<Crowbar>(), 1, 1, 1));
		}
	}
}
