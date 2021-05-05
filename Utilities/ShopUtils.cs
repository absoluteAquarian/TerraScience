using Terraria;

namespace TerraScience.Utilities{
	public static class ShopUtils{
		public static void AddItemToShop(Chest shop, ref int nextSlot, int type)
			=> shop.item[nextSlot++].SetDefaults(type);
	}
}
