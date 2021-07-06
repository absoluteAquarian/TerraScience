using Terraria.DataStructures;
using Terraria.ObjectData;
using TerraScience.Systems;
using TerraScience.Systems.Pipes;

namespace TerraScience.API.Edits.Detours{
	public static partial class Vanilla{
		private static int Chest_AfterPlacement_Hook(On.Terraria.Chest.orig_AfterPlacement_Hook orig, int x, int y, int type, int style, int direction){
			int ret = orig(x, y, type, style, direction);

			if(ret != -1){
				//A chest was able to be placed.  Try to add this chest to nearby item networks
				Point16 coords = new Point16(x, y);
				TileObjectData.OriginToTopLeft(type, style, ref coords);

				coords -= new Point16(1, 1);

				//"coord" is the top-left corner of the chest
				for(int checkY = coords.Y; checkY < coords.Y + 4; checkY++){
					for(int checkX = coords.X; checkX < coords.X + 4; checkX++){
						int relX = checkX - coords.X;
						int relY = checkY - coords.Y;

						//Ignore corners
						if((relX == 0 && relY == 0) || (relX == 0 && relY == 3) || (relX == 3 && relY == 0) || (relX == 3 && relY == 3))
							continue;

						var point = new Point16(checkX, checkY);
						if(NetworkCollection.HasItemPipeAt(point, out ItemNetwork net)){
							if(!net.chests.Contains(ret)){
								//Add the chest to the network
								net.chests.Add(ret);

								if(!net.pipesConnectedToChests.Contains(point))
									net.pipesConnectedToChests.Add(point);
							}
						}
					}
				}
			}

			return ret;
		}
	}
}
