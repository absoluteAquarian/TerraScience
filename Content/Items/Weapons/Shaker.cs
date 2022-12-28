using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerraScience.Content.Items.Materials;

namespace TerraScience.Content.Items.Weapons{
    [Autoload(false)]
    public class Shaker : ModItem {
        public override string Texture => $"TerraScience/Content/Items/Weapons/{Name}";

        private readonly string displayName;
        private readonly string tooltip;
        private readonly Func<int> ammoType;
        private readonly Action<Item> itemDefaults;

        public override string Name { get; }

        public override bool IsLoadingEnabled(Mod mod){
            return false;
        }

        public Shaker() { }

        public Shaker(string internalName, string display, string tooltip, Func<int> ammo, Action<Item> defaults) {
            Name = internalName ?? base.Name;
            displayName = display;
            this.tooltip = tooltip;
            ammoType = ammo;
            itemDefaults = defaults;
        }

        public override void SetStaticDefaults() {
            DisplayName.SetDefault(displayName);
            Tooltip.SetDefault(tooltip);
        }

        public override void SetDefaults() {
            itemDefaults(Item);
            Item.DamageType = DamageClass.Ranged;
            Item.noMelee = true;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTime = Item.useAnimation = 24;
            Item.shootSpeed = 8;
            Item.useAmmo = ammoType();
            Item.width = 22;
            Item.height = 38;
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
            for (int i = 0; i < 40; i++) {
                Vector2 spawn = position + (Vector2.Normalize(velocity) * (24 + Main.rand.NextFloat(-8, 8))).RotatedByRandom(MathHelper.ToRadians(20));
                Vector2 speed = velocity.RotatedByRandom(MathHelper.ToRadians(5)) * Main.rand.NextFloat(0.785f, 1.215f);

                Projectile.NewProjectile(player.GetSource_ItemUse_WithPotentialAmmo(Item, ammoType()), spawn, speed, type, damage, knockback, player.whoAmI);
            }
        }

        public override bool CanShoot(Player player) => false;

        public override void AddRecipes() {
            CreateRecipe(1)
                .AddIngredient(ModContent.ItemType<Shaker_Empty>())
                .AddIngredient(ammoType(), 4)
                .AddTile(TileID.WorkBenches)
                .Register();
        }
    }
}
