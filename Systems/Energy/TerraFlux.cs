namespace TerraScience.Systems.Energy{
	public struct TerraFlux{
		private readonly float amount;

		public static readonly TerraFlux Zero = new TerraFlux(0f);

		public TerraFlux(float amount){
			this.amount = amount;
		}

		#region Operators
		public static TerraFlux operator +(TerraFlux flux, float val) => new TerraFlux(flux.amount + val);

		public static TerraFlux operator -(TerraFlux flux, float val) => new TerraFlux(flux.amount - val);

		public static TerraFlux operator *(TerraFlux flux, float val) => new TerraFlux(flux.amount * val);

		public static TerraFlux operator /(TerraFlux flux, float val) => new TerraFlux(flux.amount / val);

		public static TerraFlux operator +(float val, TerraFlux flux) => new TerraFlux(flux.amount + val);

		public static TerraFlux operator -(float val, TerraFlux flux) => new TerraFlux(flux.amount - val);

		public static TerraFlux operator *(float val, TerraFlux flux) => new TerraFlux(flux.amount * val);

		public static TerraFlux operator /(float val, TerraFlux flux) => new TerraFlux(flux.amount / val);

		public static TerraFlux operator +(TerraFlux flux, TerraFlux other) => new TerraFlux(flux.amount + other.amount);

		public static TerraFlux operator -(TerraFlux flux, TerraFlux other) => new TerraFlux(flux.amount - other.amount);

		public static TerraFlux operator *(TerraFlux flux, TerraFlux other) => new TerraFlux(flux.amount * other.amount);

		public static TerraFlux operator /(TerraFlux flux, TerraFlux other) => new TerraFlux(flux.amount / other.amount);

		public static bool operator >(TerraFlux first, TerraFlux second) => first.amount > second.amount;

		public static bool operator <(TerraFlux first, TerraFlux second) => first.amount < second.amount;

		public static bool operator ==(TerraFlux first, TerraFlux second) => first.amount == second.amount;

		public static bool operator !=(TerraFlux first, TerraFlux second) => first.amount != second.amount;

		public static bool operator >=(TerraFlux first, TerraFlux second) => first.amount >= second.amount;

		public static bool operator <=(TerraFlux first, TerraFlux second) => first.amount <= second.amount;

		public static explicit operator float(TerraFlux flux) => flux.amount;
		#endregion

		public override bool Equals(object obj) => obj is TerraFlux flux && amount == flux.amount;

		public override int GetHashCode() => amount.GetHashCode();

		public override string ToString() => amount.ToString();
	}
}
