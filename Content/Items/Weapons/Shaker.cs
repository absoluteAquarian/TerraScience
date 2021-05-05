using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerraScience.Content.Items.Materials;
using TerraScience.Utilities;

namespace TerraScience.Content.Items.Weapons{
	public class Shaker : ModItem{
		public override string Texture => $"TerraScience/Content/Items/Weapons/{Name}";

		public override bool CloneNewInstances => true;

		public override bool Autoload(ref string name) => false;

		private readonly string displayName;
		private readonly string tooltip;
		private readonly Func<int> ammoType;
		private readonly Action<Item> itemDefaults;

		public Shaker(){ }

		public Shaker(string display, string tooltip, Func<int> ammo, Action<Item> defaults){
			displayName = display;
			this.tooltip = tooltip;
			ammoType = ammo;
			itemDefaults = defaults;
		}

		public override void SetStaticDefaults(){
			DisplayName.SetDefault(displayName);
			Tooltip.SetDefault(tooltip);
		}

		public override void SetDefaults(){
			itemDefaults(item);
			item.melee = false;
			item.ranged = true;
			item.magic = false;
			item.thrown = false;
			item.summon = false;
			item.noMelee = true;
			item.useStyle = ItemUseStyleID.SwingThrow;
			item.useTime = item.useAnimation = 24;
			item.shootSpeed = 8;
			item.useAmmo = ammoType();
			item.width = 22;
			item.height = 38;
		}

		public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack){
			Vector2 speed = new Vector2(speedX, speedY);

			for(int i = 0; i < 40; i++){
				Vector2 spawn = position + (Vector2.Normalize(speed) * (24 + Main.rand.NextFloat(-8, 8))).RotatedByRandom(MathHelper.ToRadians(20));
				Vector2 velocity = speed.RotatedByRandom(MathHelper.ToRadians(5)) * Main.rand.NextFloat(0.785f, 1.215f);

				Projectile.NewProjectile(spawn, velocity, type, damage, knockBack, player.whoAmI);
			}

			return false;
		}

		public override void AddRecipes(){
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ModContent.ItemType<Shaker_Empty>());
			recipe.AddIngredient(ammoType(), 4);
			recipe.AddTile(TileID.WorkBenches);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}
}
