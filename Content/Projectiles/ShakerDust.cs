using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ModLoader;
using TerraScience.Utilities;

namespace TerraScience.Content.Projectiles{
	public class ShakerDust : ModProjectile{
		public override string Texture => $"TerraScience/Content/Projectiles/ShakerDust";

		public override bool CloneNewInstances => true;

		public override bool Autoload(ref string name) => false;

		private readonly string displayName;
		private readonly Color drawColor;

		public ShakerDust(){ }

		public ShakerDust(string name, Color color){
			displayName = name;
			drawColor = color;
		}

		public override void SetStaticDefaults(){
			DisplayName.SetDefault(displayName);
		}

		public override void SetDefaults(){
			projectile.width = 2;
			projectile.height = 2;
			projectile.penetrate = 1;
			projectile.friendly = true;
			projectile.timeLeft = 180;
			projectile.usesLocalNPCImmunity = true;
			projectile.localNPCHitCooldown = 5;
		}

		public override Color? GetAlpha(Color lightColor)
			=> MiscUtils.Blend(lightColor, drawColor);

		public override void AI(){
			//ai[0] == 0, waiting to hit a tile; apply slight gravity and friction
			//ai[0] == 1, hit a tile, start to fade out slowly and die once invisible
			if(projectile.ai[0] == 0){
				projectile.velocity.X *= 1f - 0.678f / 60f;
				projectile.velocity.Y += 6.528f / 60f;
			}else if(projectile.ai[0] == 1){
				projectile.alpha += 3;

				if(projectile.alpha >= 255)
					projectile.Kill();
			}
		}

		public override bool OnTileCollide(Vector2 oldVelocity){
			if(projectile.ai[0] == 0){
				projectile.ai[0] = 1;
				projectile.velocity = Vector2.Zero;
				projectile.position = projectile.oldPosition;
			}

			return false;
		}
	}
}
