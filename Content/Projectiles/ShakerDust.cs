using Microsoft.Xna.Framework;
using Terraria.ModLoader;
using TerraScience.Utilities;

namespace TerraScience.Content.Projectiles{
	[Autoload(false)]
	public class ShakerDust : ModProjectile{
		public override string Texture => $"TerraScience/Content/Projectiles/ShakerDust";

		private readonly string displayName;
		private readonly Color drawColor;

		public override string Name{ get; }

		public ShakerDust(){ }

		public ShakerDust(string internalName, string name, Color color){
			Name = internalName ?? base.Name;
			displayName = name;
			drawColor = color;
		}

		public override void SetStaticDefaults(){
			DisplayName.SetDefault(displayName);
		}

		public override void SetDefaults(){
			Projectile.width = 2;
			Projectile.height = 2;
			Projectile.penetrate = 1;
			Projectile.friendly = true;
			Projectile.timeLeft = 180;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 5;
		}

		public override Color? GetAlpha(Color lightColor)
			=> MiscUtils.Blend(lightColor, drawColor);

		public override void AI(){
			//ai[0] == 0, waiting to hit a tile; apply slight gravity and friction
			//ai[0] == 1, hit a tile, start to fade out slowly and die once invisible
			if(Projectile.ai[0] == 0){
				Projectile.velocity.X *= 1f - 0.678f / 60f;
				Projectile.velocity.Y += 6.528f / 60f;
			}else if(Projectile.ai[0] == 1){
				Projectile.alpha += 3;

				if(Projectile.alpha >= 255)
					Projectile.Kill();
			}
		}

		public override bool OnTileCollide(Vector2 oldVelocity){
			if(Projectile.ai[0] == 0){
				Projectile.ai[0] = 1;
				Projectile.velocity = Vector2.Zero;
				Projectile.position = Projectile.oldPosition;
			}

			return false;
		}
	}
}
