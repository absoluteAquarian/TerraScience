using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerraScience.Systems.TemperatureSystem;

namespace TerraScience.API.Classes.ModLiquid {
	public class ModLiquid {
		public DefaultTemperature DefaultTemp { get; private set; }

		public string InternalName { get; private set; } = string.Empty;

		public int Type { get; private set; }

		public string DisplayName { get; set; } = string.Empty;

		public Rectangle GetRectangle { get; private set; } //Is vanilla UseItem in Main??

		public float CurrentTemperature => TemperatureSystem.CalculateLiquidTemp(this);

		public Texture2D Texture { get; private set; }

		public ModLiquid(string internalName, string displayName, string texturePath, DefaultTemperature defaultTemp) {
			InternalName = internalName;
			DisplayName = displayName;
			DefaultTemp = defaultTemp;
			Texture = ModContent.GetInstance<TerraScience>().GetTexture(texturePath);
			Type = ModLiquidManager.LastAddedLiquidID;
		}

		public void SpawnLiquid(int i, int j, byte liquidAmount) {
			Tile tile = Main.tile[i, j];

			//tile.liquidType(Type); // Type is the liquid type. 0 is water. 1 is lava. 2 is honey.
			tile.liquid = liquidAmount;

			WorldGen.SquareTileFrame(i, j);

			if (Main.netMode == NetmodeID.MultiplayerClient)
				NetMessage.sendWater(i, j);
		}

		public event OnUpdateEventHandler OnUpdate;

		public event InLiquidEventHandler InLiquid;

		public delegate void OnUpdateEventHandler();

		public delegate void InLiquidEventHandler(Player player);

		internal void Update() {
			OnUpdateEventHandler handler = OnUpdate;
			handler?.Invoke();

			ModLiquidManager.RunInLiquidEvent(GetRectangle, InLiquid);

			//GetRectangle = 
		}

		public override string ToString() => InternalName;

		public override bool Equals(object obj) {
			return obj is ModLiquid liquid &&
				   EqualityComparer<DefaultTemperature>.Default.Equals(DefaultTemp, liquid.DefaultTemp) &&
				   InternalName == liquid.InternalName &&
				   DisplayName == liquid.DisplayName &&
				   CurrentTemperature == liquid.CurrentTemperature;
		}

		public override int GetHashCode() {
			var hashCode = -345371403;
			hashCode = hashCode * -1521134295 + DefaultTemp.GetHashCode();
			hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(InternalName);
			hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(DisplayName);
			hashCode = hashCode * -1521134295 + CurrentTemperature.GetHashCode();
			return hashCode;
		}
	}
}