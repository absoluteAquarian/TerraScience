using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace TerraScience.Content.Items.Materials{
	public class UnstableSkull : ModItem{
		public override void SetStaticDefaults(){
			Tooltip.SetDefault("The skull of a Dark Caster" +
				"\nIt seems like it could phase out of existance at a moment's notice");

			Main.RegisterItemAnimation(item.type, new UnstableSkullAnimation());
		}

		public override void SetDefaults(){
			item.value = Item.buyPrice(silver: 20, copper: 35);
			item.rare = ItemRarityID.Blue;
			item.maxStack = 99;
		}
	}

	internal class UnstableSkullAnimation : DrawAnimationVertical{
		public UnstableSkullAnimation() : base(5, 6){ }

		public override void Update(){
			FrameCounter++;

			//Randomly play one of the animation sets in the spritesheet after 4 seconds
			if(Frame == -1 && FrameCounter >= 4 * 60 && Main.rand.NextBool(20)){
				FrameCounter = 0;
				Frame = Main.rand.NextBool() ? 0 : 3;
			}else if(Frame >= 0 && FrameCounter >= 60 / TicksPerFrame){
				int oldFrame = Frame;
				Frame++;

				if((oldFrame == 2 && Frame == 3) || Frame == 6)
					Frame = -1;
			}
		}

		public override Rectangle GetFrame(Texture2D texture){
			int frame = Frame == -1 ? 0 : Frame;
			return texture.Frame(1, FrameCount, 0, frame);
		}
	}
}
