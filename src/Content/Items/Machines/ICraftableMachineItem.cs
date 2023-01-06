using SerousEnergyLib.Items;
using Terraria.ModLoader;

namespace TerraScience.Content.Items.Machines {
	/// <summary>
	/// An interface used to indicate that a machine item has a recipe located in another item
	/// </summary>
	public interface ICraftableMachineItem {
		/// <summary>
		/// An alternative item ID to look for when searching for recipes that create this item
		/// </summary>
		int AlternativeItemType { get; }
	}

	/// <inheritdoc cref="ICraftableMachineItem"/>
	public interface ICraftableMachineItem<T> : ICraftableMachineItem where T : BaseMachineItem {
		int ICraftableMachineItem.AlternativeItemType => ModContent.ItemType<T>();
	}
}
