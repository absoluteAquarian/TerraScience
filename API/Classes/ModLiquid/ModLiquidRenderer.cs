using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Graphics;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace TerraScience.API.Classes.ModLiquid {
	public class ModLiquidRenderer {
		public event Action<Color[], Rectangle> WaveFilters;

		// Token: 0x06002187 RID: 8583 RVA: 0x00466C9C File Offset: 0x00464E9C
		private unsafe void InternalPrepareDraw(Rectangle drawArea) {
			Rectangle rectangle = new Rectangle(drawArea.X - 2, drawArea.Y - 2, drawArea.Width + 4, drawArea.Height + 4);
			_drawArea = drawArea;
			if (_cache.Length < rectangle.Width * rectangle.Height + 1) {
				_cache = new LiquidCache[rectangle.Width * rectangle.Height + 1];
			}
			if (_drawCache.Length < drawArea.Width * drawArea.Height + 1) {
				_drawCache = new LiquidDrawCache[drawArea.Width * drawArea.Height + 1];
			}
			if (waveMask.Length < drawArea.Width * drawArea.Height) {
				waveMask = new Color[drawArea.Width * drawArea.Height];
			}
			fixed (LiquidCache* ptr7 = &_cache[1]) {
				LiquidCache* ptr = ptr7;
				int num = rectangle.Height * 2 + 2;
				LiquidCache* ptr2 = ptr;
				for (int i = rectangle.X; i < rectangle.X + rectangle.Width; i++) {
					for (int j = rectangle.Y; j < rectangle.Y + rectangle.Height; j++) {
						Tile tile = Main.tile[i, j];
						if (tile == null) {
							tile = new Tile();
						}
						ptr2->LiquidLevel = tile.liquid / 255f;
						ptr2->IsHalfBrick = (tile.halfBrick() && ptr2[-1].HasLiquid);
						ptr2->IsSolid = (WorldGen.SolidOrSlopedTile(tile) && !ptr2->IsHalfBrick);
						ptr2->HasLiquid = (tile.liquid > 0);
						ptr2->VisibleLiquidLevel = 0f;
						ptr2->HasWall = (tile.wall > 0);
						ptr2->Type = tile.liquidType();
						if (ptr2->IsHalfBrick && !ptr2->HasLiquid) {
							ptr2->Type = ptr2[-1].Type;
						}
						ptr2++;
					}
				}
				ptr2 = ptr;
				ptr2 += num;
				for (int k = 2; k < rectangle.Width - 2; k++) {
					for (int l = 2; l < rectangle.Height - 2; l++) {
						float num2 = 0f;
						if (ptr2->IsHalfBrick && ptr2[-1].HasLiquid) {
							num2 = 1f;
						}
						else if (!ptr2->HasLiquid) {
							LiquidCache liquidCache = ptr2[-rectangle.Height];
							LiquidCache liquidCache2 = ptr2[rectangle.Height];
							LiquidCache liquidCache3 = ptr2[-1];
							LiquidCache liquidCache4 = ptr2[1];
							if (liquidCache.HasLiquid && liquidCache2.HasLiquid && liquidCache.Type == liquidCache2.Type) {
								num2 = liquidCache.LiquidLevel + liquidCache2.LiquidLevel;
								ptr2->Type = liquidCache.Type;
							}
							if (liquidCache3.HasLiquid && liquidCache4.HasLiquid && liquidCache3.Type == liquidCache4.Type) {
								num2 = Math.Max(num2, liquidCache3.LiquidLevel + liquidCache4.LiquidLevel);
								ptr2->Type = liquidCache3.Type;
							}
							num2 *= 0.5f;
						}
						else {
							num2 = ptr2->LiquidLevel;
						}
						ptr2->VisibleLiquidLevel = num2;
						ptr2->HasVisibleLiquid = (num2 != 0f);
						ptr2++;
					}
					ptr2 += 4;
				}
				ptr2 = ptr;
				for (int m = 0; m < rectangle.Width; m++) {
					for (int n = 0; n < rectangle.Height - 10; n++) {
						if (ptr2->HasVisibleLiquid && !ptr2->IsSolid) {
							ptr2->Opacity = 1f;
							ptr2->VisibleType = ptr2->Type;
							float num3 = 1f / (WATERFALL_LENGTH[ptr2->Type] + 1);
							float num4 = 1f;
							for (int num5 = 1; num5 <= WATERFALL_LENGTH[ptr2->Type]; num5++) {
								num4 -= num3;
								if (ptr2[num5].IsSolid) {
									break;
								}
								ptr2[num5].VisibleLiquidLevel = Math.Max(ptr2[num5].VisibleLiquidLevel, ptr2->VisibleLiquidLevel * num4);
								ptr2[num5].Opacity = num4;
								ptr2[num5].VisibleType = ptr2->Type;
							}
						}
						if (ptr2->IsSolid) {
							ptr2->VisibleLiquidLevel = 1f;
							ptr2->HasVisibleLiquid = false;
						}
						else {
							ptr2->HasVisibleLiquid = (ptr2->VisibleLiquidLevel != 0f);
						}
						ptr2++;
					}
					ptr2 += 10;
				}
				ptr2 = ptr;
				ptr2 += num;
				for (int num6 = 2; num6 < rectangle.Width - 2; num6++) {
					for (int num7 = 2; num7 < rectangle.Height - 2; num7++) {
						if (!ptr2->HasVisibleLiquid || ptr2->IsSolid) {
							ptr2->HasLeftEdge = false;
							ptr2->HasTopEdge = false;
							ptr2->HasRightEdge = false;
							ptr2->HasBottomEdge = false;
						}
						else {
							LiquidCache liquidCache5 = ptr2[-1];
							LiquidCache liquidCache6 = ptr2[1];
							LiquidCache liquidCache7 = ptr2[-rectangle.Height];
							LiquidCache liquidCache8 = ptr2[rectangle.Height];
							float num8 = 0f;
							float num9 = 1f;
							float num10 = 0f;
							float num11 = 1f;
							float visibleLiquidLevel = ptr2->VisibleLiquidLevel;
							if (!liquidCache5.HasVisibleLiquid) {
								num10 += liquidCache6.VisibleLiquidLevel * (1f - visibleLiquidLevel);
							}
							if (!liquidCache6.HasVisibleLiquid && !liquidCache6.IsSolid && !liquidCache6.IsHalfBrick) {
								num11 -= liquidCache5.VisibleLiquidLevel * (1f - visibleLiquidLevel);
							}
							if (!liquidCache7.HasVisibleLiquid && !liquidCache7.IsSolid && !liquidCache7.IsHalfBrick) {
								num8 += liquidCache8.VisibleLiquidLevel * (1f - visibleLiquidLevel);
							}
							if (!liquidCache8.HasVisibleLiquid && !liquidCache8.IsSolid && !liquidCache8.IsHalfBrick) {
								num9 -= liquidCache7.VisibleLiquidLevel * (1f - visibleLiquidLevel);
							}
							ptr2->LeftWall = num8;
							ptr2->RightWall = num9;
							ptr2->BottomWall = num11;
							ptr2->TopWall = num10;
							Point zero = Point.Zero;
							ptr2->HasTopEdge = ((!liquidCache5.HasVisibleLiquid && !liquidCache5.IsSolid) || num10 != 0f);
							ptr2->HasBottomEdge = ((!liquidCache6.HasVisibleLiquid && !liquidCache6.IsSolid) || num11 != 1f);
							ptr2->HasLeftEdge = ((!liquidCache7.HasVisibleLiquid && !liquidCache7.IsSolid) || num8 != 0f);
							ptr2->HasRightEdge = ((!liquidCache8.HasVisibleLiquid && !liquidCache8.IsSolid) || num9 != 1f);
							if (!ptr2->HasLeftEdge) {
								if (ptr2->HasRightEdge) {
									zero.X += 32;
								}
								else {
									zero.X += 16;
								}
							}
							if (ptr2->HasLeftEdge && ptr2->HasRightEdge) {
								zero.X = 16;
								zero.Y += 32;
								if (ptr2->HasTopEdge) {
									zero.Y = 16;
								}
							}
							else if (!ptr2->HasTopEdge) {
								if (!ptr2->HasLeftEdge && !ptr2->HasRightEdge) {
									zero.Y += 48;
								}
								else {
									zero.Y += 16;
								}
							}
							if (zero.Y == 16 && (ptr2->HasLeftEdge ^ ptr2->HasRightEdge) && (num7 + rectangle.Y) % 2 == 0) {
								zero.Y += 16;
							}
							ptr2->FrameOffset = zero;
						}
						ptr2++;
					}
					ptr2 += 4;
				}
				ptr2 = ptr;
				ptr2 += num;
				for (int num12 = 2; num12 < rectangle.Width - 2; num12++) {
					for (int num13 = 2; num13 < rectangle.Height - 2; num13++) {
						if (ptr2->HasVisibleLiquid) {
							LiquidCache liquidCache9 = ptr2[-1];
							LiquidCache liquidCache10 = ptr2[1];
							LiquidCache liquidCache11 = ptr2[-rectangle.Height];
							LiquidCache liquidCache12 = ptr2[rectangle.Height];
							ptr2->VisibleLeftWall = ptr2->LeftWall;
							ptr2->VisibleRightWall = ptr2->RightWall;
							ptr2->VisibleTopWall = ptr2->TopWall;
							ptr2->VisibleBottomWall = ptr2->BottomWall;
							if (liquidCache9.HasVisibleLiquid && liquidCache10.HasVisibleLiquid) {
								if (ptr2->HasLeftEdge) {
									ptr2->VisibleLeftWall = (ptr2->LeftWall * 2f + liquidCache9.LeftWall + liquidCache10.LeftWall) * 0.25f;
								}
								if (ptr2->HasRightEdge) {
									ptr2->VisibleRightWall = (ptr2->RightWall * 2f + liquidCache9.RightWall + liquidCache10.RightWall) * 0.25f;
								}
							}
							if (liquidCache11.HasVisibleLiquid && liquidCache12.HasVisibleLiquid) {
								if (ptr2->HasTopEdge) {
									ptr2->VisibleTopWall = (ptr2->TopWall * 2f + liquidCache11.TopWall + liquidCache12.TopWall) * 0.25f;
								}
								if (ptr2->HasBottomEdge) {
									ptr2->VisibleBottomWall = (ptr2->BottomWall * 2f + liquidCache11.BottomWall + liquidCache12.BottomWall) * 0.25f;
								}
							}
						}
						ptr2++;
					}
					ptr2 += 4;
				}
				ptr2 = ptr;
				ptr2 += num;
				for (int num14 = 2; num14 < rectangle.Width - 2; num14++) {
					for (int num15 = 2; num15 < rectangle.Height - 2; num15++) {
						if (ptr2->HasLiquid) {
							LiquidCache liquidCache19 = ptr2[-1];
							LiquidCache liquidCache13 = ptr2[1];
							LiquidCache liquidCache14 = ptr2[-rectangle.Height];
							LiquidCache liquidCache15 = ptr2[rectangle.Height];
							if (ptr2->HasTopEdge && !ptr2->HasBottomEdge && (ptr2->HasLeftEdge ^ ptr2->HasRightEdge)) {
								if (ptr2->HasRightEdge) {
									ptr2->VisibleRightWall = liquidCache13.VisibleRightWall;
									ptr2->VisibleTopWall = liquidCache14.VisibleTopWall;
								}
								else {
									ptr2->VisibleLeftWall = liquidCache13.VisibleLeftWall;
									ptr2->VisibleTopWall = liquidCache15.VisibleTopWall;
								}
							}
							else if (liquidCache13.FrameOffset.X == 16 && liquidCache13.FrameOffset.Y == 32) {
								if (ptr2->VisibleLeftWall > 0.5f) {
									ptr2->VisibleLeftWall = 0f;
									ptr2->FrameOffset = new Point(0, 0);
								}
								else if (ptr2->VisibleRightWall < 0.5f) {
									ptr2->VisibleRightWall = 1f;
									ptr2->FrameOffset = new Point(32, 0);
								}
							}
						}
						ptr2++;
					}
					ptr2 += 4;
				}
				ptr2 = ptr;
				ptr2 += num;
				for (int num16 = 2; num16 < rectangle.Width - 2; num16++) {
					for (int num17 = 2; num17 < rectangle.Height - 2; num17++) {
						if (ptr2->HasLiquid) {
							LiquidCache liquidCache16 = ptr2[-1];
							LiquidCache liquidCache20 = ptr2[1];
							LiquidCache liquidCache17 = ptr2[-rectangle.Height];
							LiquidCache liquidCache18 = ptr2[rectangle.Height];
							if (!ptr2->HasBottomEdge && !ptr2->HasLeftEdge && !ptr2->HasTopEdge && !ptr2->HasRightEdge) {
								if (liquidCache17.HasTopEdge && liquidCache16.HasLeftEdge) {
									ptr2->FrameOffset.X = Math.Max(4, (int)(16f - liquidCache16.VisibleLeftWall * 16f)) - 4;
									ptr2->FrameOffset.Y = 48 + Math.Max(4, (int)(16f - liquidCache17.VisibleTopWall * 16f)) - 4;
									ptr2->VisibleLeftWall = 0f;
									ptr2->VisibleTopWall = 0f;
									ptr2->VisibleRightWall = 1f;
									ptr2->VisibleBottomWall = 1f;
								}
								else if (liquidCache18.HasTopEdge && liquidCache16.HasRightEdge) {
									ptr2->FrameOffset.X = 32 - Math.Min(16, (int)(liquidCache16.VisibleRightWall * 16f) - 4);
									ptr2->FrameOffset.Y = 48 + Math.Max(4, (int)(16f - liquidCache18.VisibleTopWall * 16f)) - 4;
									ptr2->VisibleLeftWall = 0f;
									ptr2->VisibleTopWall = 0f;
									ptr2->VisibleRightWall = 1f;
									ptr2->VisibleBottomWall = 1f;
								}
							}
						}
						ptr2++;
					}
					ptr2 += 4;
				}
				ptr2 = ptr;
				ptr2 += num;
				fixed (LiquidDrawCache* ptr8 = &_drawCache[0]) {
					LiquidDrawCache* ptr3 = ptr8;
					fixed (Color* ptr9 = &waveMask[0]) {
						Color* ptr10 = ptr9;
						LiquidDrawCache* ptr4 = ptr3;
						Color* ptr5 = ptr10;
						for (int num18 = 2; num18 < rectangle.Width - 2; num18++) {
							for (int num19 = 2; num19 < rectangle.Height - 2; num19++) {
								if (ptr2->HasVisibleLiquid) {
									float num20 = Math.Min(0.75f, ptr2->VisibleLeftWall);
									float num21 = Math.Max(0.25f, ptr2->VisibleRightWall);
									float num22 = Math.Min(0.75f, ptr2->VisibleTopWall);
									float num23 = Math.Max(0.25f, ptr2->VisibleBottomWall);
									if (ptr2->IsHalfBrick && num23 > 0.5f) {
										num23 = 0.5f;
									}
									ptr4->IsVisible = (ptr2->HasWall || !ptr2->IsHalfBrick || !ptr2->HasLiquid);
									ptr4->SourceRectangle = new Rectangle((int)(16f - num21 * 16f) + ptr2->FrameOffset.X, (int)(16f - num23 * 16f) + ptr2->FrameOffset.Y, (int)Math.Ceiling((num21 - num20) * 16f), (int)Math.Ceiling((num23 - num22) * 16f));
									ptr4->IsSurfaceLiquid = (ptr2->FrameOffset.X == 16 && ptr2->FrameOffset.Y == 0 && num19 + rectangle.Y > Main.worldSurface - 40.0);
									ptr4->Opacity = ptr2->Opacity;
									ptr4->LiquidOffset = new Vector2((float)Math.Floor(num20 * 16f), (float)Math.Floor(num22 * 16f));
									ptr4->Type = ptr2->VisibleType;
									ptr4->HasWall = ptr2->HasWall;
									byte b = WAVE_MASK_STRENGTH[ptr2->VisibleType];
									byte b2 = (byte)(b >> 1);
									ptr5->R = b2;
									ptr5->G = b2;
									ptr5->B = VISCOSITY_MASK[ptr2->VisibleType];
									ptr5->A = b;
									LiquidCache* ptr6 = ptr2 - 1;
									if (num19 != 2 && !ptr6->HasVisibleLiquid && !ptr6->IsSolid && !ptr6->IsHalfBrick) {
										*(ptr5 - 1) = *ptr5;
									}
								}
								else {
									ptr4->IsVisible = false;
									int num24 = (!ptr2->IsSolid && !ptr2->IsHalfBrick) ? 4 : 3;
									byte b3 = WAVE_MASK_STRENGTH[num24];
									byte b4 = (byte)(b3 >> 1);
									ptr5->R = b4;
									ptr5->G = b4;
									ptr5->B = VISCOSITY_MASK[num24];
									ptr5->A = b3;
								}
								ptr2++;
								ptr4++;
								ptr5++;
							}
							ptr2 += 4;
						}
					}
				}
				ptr2 = ptr;
				for (int num25 = rectangle.X; num25 < rectangle.X + rectangle.Width; num25++) {
					for (int num26 = rectangle.Y; num26 < rectangle.Y + rectangle.Height; num26++) {
						if (ptr2->VisibleType == 1 && ptr2->HasVisibleLiquid && Dust.lavaBubbles < 200) {
							if (random.Next(700) == 0) {
								Dust.NewDust(new Vector2(num25 * 16, num26 * 16), 16, 16, 35, 0f, 0f, 0, Color.White, 1f);
							}
							if (random.Next(350) == 0) {
								int num27 = Dust.NewDust(new Vector2(num25 * 16, num26 * 16), 16, 8, 35, 0f, 0f, 50, Color.White, 1.5f);
								Main.dust[num27].velocity *= 0.8f;
								Dust expr_1205_cp_0 = Main.dust[num27];
								expr_1205_cp_0.velocity.X *= 2f;
								Dust expr_1223_cp_0 = Main.dust[num27];
								expr_1223_cp_0.velocity.Y -= random.Next(1, 7) * 0.1f;
								if (random.Next(10) == 0) {
									Dust expr_125F_cp_0 = Main.dust[num27];
									expr_125F_cp_0.velocity.Y *= random.Next(2, 5);
								}
								Main.dust[num27].noGravity = true;
							}
						}
						ptr2++;
					}
				}
			}

			WaveFilters?.Invoke(waveMask, GetCachedDrawArea());
		}

		// Token: 0x06002188 RID: 8584 RVA: 0x00467F5C File Offset: 0x0046615C
		private unsafe void InternalDraw(SpriteBatch spriteBatch, Vector2 drawOffset, int waterStyle, float globalAlpha, bool isBackgroundDraw) {
			Rectangle drawArea = _drawArea;
			Main.tileBatch.Begin();
			fixed (LiquidDrawCache* ptr3 = &_drawCache[0]) {
				LiquidDrawCache* ptr2 = ptr3;
				for (int i = drawArea.X; i < drawArea.X + drawArea.Width; i++) {
					for (int j = drawArea.Y; j < drawArea.Y + drawArea.Height; j++) {
						if (ptr2->IsVisible) {
							Rectangle sourceRectangle = ptr2->SourceRectangle;
							if (ptr2->IsSurfaceLiquid) {
								sourceRectangle.Y = 1280;
							}
							else {
								sourceRectangle.Y += animationFrame * 80;
							}
							Vector2 liquidOffset = ptr2->LiquidOffset;
							float num = ptr2->Opacity * (isBackgroundDraw ? 1f : DEFAULT_OPACITY[ptr2->Type]);
							int num2 = ptr2->Type;
							if (num2 == 0) {
								num2 = waterStyle;
								num *= (isBackgroundDraw ? 1f : globalAlpha);
							}
							else if (num2 == 2) {
								num2 = 11;
							}
							num = Math.Min(1f, num);

							Lighting.GetColor4Slice_New(i, j, out VertexColors colors, 1f);
							colors.BottomLeftColor *= num;
							colors.BottomRightColor *= num;
							colors.TopLeftColor *= num;
							colors.TopRightColor *= num;
							Main.tileBatch.Draw(textures[num2], new Vector2(i << 4, j << 4) + drawOffset + liquidOffset, new Rectangle?(sourceRectangle), colors, Vector2.Zero, 1f, SpriteEffects.None);
						}
						ptr2++;
					}
				}
			}
			Main.tileBatch.End();
		}

		// Token: 0x06002189 RID: 8585 RVA: 0x00468140 File Offset: 0x00466340
		public bool HasFullWater(int x, int y) {
			x -= _drawArea.X;
			y -= _drawArea.Y;
			int num = x * _drawArea.Height + y;
			return num < 0 || num >= _drawCache.Length || (_drawCache[num].IsVisible && !_drawCache[num].IsSurfaceLiquid);
		}

		// Token: 0x0600218A RID: 8586 RVA: 0x004681B8 File Offset: 0x004663B8
		public float GetVisibleLiquid(int x, int y) {
			x -= _drawArea.X;
			y -= _drawArea.Y;
			if (x < 0 || x >= _drawArea.Width || y < 0 || y >= _drawArea.Height) {
				return 0f;
			}
			int num = (x + 2) * (_drawArea.Height + 4) + y + 2;
			if (!_cache[num].HasVisibleLiquid) {
				return 0f;
			}
			return _cache[num].VisibleLiquidLevel;
		}

		// Token: 0x0600218B RID: 8587 RVA: 0x00468250 File Offset: 0x00466450
		public void Update(GameTime gameTime) {
			if (Main.gamePaused || !Main.hasFocus) {
				return;
			}
			float num = Main.windSpeed * 80f;
			num = MathHelper.Clamp(num, -20f, 20f);
			if (num < 0f) {
				num = Math.Min(-10f, num);
			}
			else {
				num = Math.Max(10f, num);
			}
			frameState += num * (float)gameTime.ElapsedGameTime.TotalSeconds;
			if (frameState < 0f) {
				frameState += 16f;
			}
			frameState %= 16f;
			animationFrame = (int)frameState;
		}

		// Token: 0x0600218C RID: 8588 RVA: 0x00468307 File Offset: 0x00466507
		public void PrepareDraw(Rectangle drawArea) {
			InternalPrepareDraw(drawArea);
		}

		// Token: 0x0600218D RID: 8589 RVA: 0x00468310 File Offset: 0x00466510
		public void SetWaveMaskData(ref Texture2D texture) {
			if (texture == null || texture.Width < _drawArea.Height || texture.Height < _drawArea.Width) {
				Console.WriteLine("WaveMaskData texture recreated. {0}x{1}", _drawArea.Height, _drawArea.Width);
				if (texture != null) {
					try {
						texture.Dispose();
					}
					catch (ObjectDisposedException e) {
						ModContent.GetInstance<TerraScience>().Logger.Error(e);
					}
				}
				texture = new Texture2D(Main.instance.GraphicsDevice, _drawArea.Height, _drawArea.Width, false, SurfaceFormat.Color);
			}
			texture.SetData(0, new Rectangle?(new Rectangle(0, 0, _drawArea.Height, _drawArea.Width)), waveMask, 0, _drawArea.Width * _drawArea.Height);
		}

		// Token: 0x0600218E RID: 8590 RVA: 0x00468404 File Offset: 0x00466604
		public Rectangle GetCachedDrawArea() {
			return _drawArea;
		}

		// Token: 0x0600218F RID: 8591 RVA: 0x0046840C File Offset: 0x0046660C
		public void Draw(SpriteBatch spriteBatch, Vector2 drawOffset, int waterStyle, float alpha, bool isBackgroundDraw) {
			InternalDraw(spriteBatch, drawOffset, waterStyle, alpha, isBackgroundDraw);
		}

		// Token: 0x06002190 RID: 8592 RVA: 0x0046841C File Offset: 0x0046661C
		static ModLiquidRenderer() {
			byte[] array = new byte[5];
			array[3] = byte.MaxValue;
			WAVE_MASK_STRENGTH = array;
			byte[] array2 = new byte[5];
			array2[1] = 200;
			array2[2] = 240;
			VISCOSITY_MASK = array2;
		}

		// Token: 0x04003A09 RID: 14857
		private const int ANIMATION_FRAME_COUNT = 16;

		// Token: 0x04003A0A RID: 14858
		private const int CACHE_PADDING = 2;

		// Token: 0x04003A0B RID: 14859
		private const int CACHE_PADDING_2 = 4;

		// Token: 0x04003A0C RID: 14860
		public const float MIN_LIQUID_SIZE = 0.25f;

		// Token: 0x04003A0D RID: 14861
		private static readonly int[] WATERFALL_LENGTH = new int[]
		{
			10,
			3,
			2
		};

		// Token: 0x04003A0E RID: 14862
		private static readonly float[] DEFAULT_OPACITY = new float[]
		{
			0.6f,
			0.95f,
			0.95f
		};

		// Token: 0x04003A0F RID: 14863
		private static readonly byte[] WAVE_MASK_STRENGTH;

		// Token: 0x04003A10 RID: 14864
		private static readonly byte[] VISCOSITY_MASK;

		// Token: 0x04003A11 RID: 14865
		public static ModLiquidRenderer Instance;

		// Token: 0x04003A12 RID: 14866
		public List<Texture2D> textures = new List<Texture2D>();

		// Token: 0x04003A13 RID: 14867
		private LiquidCache[] _cache = new LiquidCache[1];

		// Token: 0x04003A14 RID: 14868
		private LiquidDrawCache[] _drawCache = new LiquidDrawCache[1];

		private int animationFrame;

		private Rectangle _drawArea = new Rectangle(0, 0, 1, 1);

		private readonly UnifiedRandom random = new UnifiedRandom();

		private Color[] waveMask = new Color[1];

		private float frameState;

		// Token: 0x0200055C RID: 1372
		private struct LiquidCache {
			// Token: 0x0400472C RID: 18220
			public float LiquidLevel;

			// Token: 0x0400472D RID: 18221
			public float VisibleLiquidLevel;

			// Token: 0x0400472E RID: 18222
			public float Opacity;

			// Token: 0x0400472F RID: 18223
			public bool IsSolid;

			// Token: 0x04004730 RID: 18224
			public bool IsHalfBrick;

			// Token: 0x04004731 RID: 18225
			public bool HasLiquid;

			// Token: 0x04004732 RID: 18226
			public bool HasVisibleLiquid;

			// Token: 0x04004733 RID: 18227
			public bool HasWall;

			// Token: 0x04004734 RID: 18228
			public Point FrameOffset;

			// Token: 0x04004735 RID: 18229
			public bool HasLeftEdge;

			// Token: 0x04004736 RID: 18230
			public bool HasRightEdge;

			// Token: 0x04004737 RID: 18231
			public bool HasTopEdge;

			// Token: 0x04004738 RID: 18232
			public bool HasBottomEdge;

			// Token: 0x04004739 RID: 18233
			public float LeftWall;

			// Token: 0x0400473A RID: 18234
			public float RightWall;

			// Token: 0x0400473B RID: 18235
			public float BottomWall;

			// Token: 0x0400473C RID: 18236
			public float TopWall;

			// Token: 0x0400473D RID: 18237
			public float VisibleLeftWall;

			// Token: 0x0400473E RID: 18238
			public float VisibleRightWall;

			// Token: 0x0400473F RID: 18239
			public float VisibleBottomWall;

			// Token: 0x04004740 RID: 18240
			public float VisibleTopWall;

			// Token: 0x04004741 RID: 18241
			public byte Type;

			// Token: 0x04004742 RID: 18242
			public byte VisibleType;
		}

		// Token: 0x0200055D RID: 1373
		private struct LiquidDrawCache {
			// Token: 0x04004743 RID: 18243
			public Rectangle SourceRectangle;

			// Token: 0x04004744 RID: 18244
			public Vector2 LiquidOffset;

			// Token: 0x04004745 RID: 18245
			public bool IsVisible;

			// Token: 0x04004746 RID: 18246
			public float Opacity;

			// Token: 0x04004747 RID: 18247
			public byte Type;

			// Token: 0x04004748 RID: 18248
			public bool IsSurfaceLiquid;

			// Token: 0x04004749 RID: 18249
			public bool HasWall;
		}
	}
}
