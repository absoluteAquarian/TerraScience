using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using TerraScience.Systems.TemperatureSystem;

namespace TerraScience.API.Classes.ModLiquid {
	public class ModLiquid {
		public DefaultTemperature DefaultTemp { get; private set; }

		public string InternalName { get; private set; } = string.Empty;

		public string DisplayName { get; set; } = string.Empty;

		public Rectangle GetRectangle { get; private set; }

		public float CurrentTemperature => TemperatureSystem.CalculateLiquidTemp(this);

		public ModLiquid(string internalName, string displayName, DefaultTemperature defaultTemp) {
			InternalName = internalName;
			DisplayName = displayName;
			DefaultTemp = defaultTemp;
		}

		public event OnUpdateEventHandler OnUpdate;

		public event InLiquidEventHandler InLiquid;

		public delegate void OnUpdateEventHandler();

		public delegate void InLiquidEventHandler(Player player);

		internal void Update() {
			OnUpdateEventHandler handler = OnUpdate;
			handler?.Invoke();

			ModLiquidManager.RunInLiquidEvent(GetRectangle, InLiquid);
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