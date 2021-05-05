namespace TerraScience.Systems.TemperatureSystem {
	public struct DefaultTemperature {
		public float SummerTemperature { get; private set; }

		public float AutumnTemperature { get; private set; }

		public float WinterTemperature { get; private set; }

		public float SpringTemperature { get; private set; }

		public DefaultTemperature(float summerTemp, float autumnTemp, float winterTemp, float springTemp) {
			SummerTemperature = summerTemp;
			AutumnTemperature = autumnTemp;
			WinterTemperature = winterTemp;
			SpringTemperature = springTemp;
		}

		public DefaultTemperature(float temperature) : this() {
			SummerTemperature = temperature;
			AutumnTemperature = temperature;
			WinterTemperature = temperature;
			SpringTemperature = temperature;
		}

		public override bool Equals(object obj) {
			return obj is DefaultTemperature temperature &&
				   SummerTemperature == temperature.SummerTemperature &&
				   AutumnTemperature == temperature.AutumnTemperature &&
				   WinterTemperature == temperature.WinterTemperature &&
				   SpringTemperature == temperature.SpringTemperature;
		}

		public override int GetHashCode() {
			var hashCode = 1295548665;
			hashCode = hashCode * -1521134295 + SummerTemperature.GetHashCode();
			hashCode = hashCode * -1521134295 + AutumnTemperature.GetHashCode();
			hashCode = hashCode * -1521134295 + WinterTemperature.GetHashCode();
			hashCode = hashCode * -1521134295 + SpringTemperature.GetHashCode();
			return hashCode;
		}

		public static bool operator ==(DefaultTemperature left, DefaultTemperature right) => left.Equals(right);

		public static bool operator !=(DefaultTemperature left, DefaultTemperature right) => !(left == right);
	}
}