using System.Numerics;

namespace DungeonCrawl
{
	internal class Map
	{
		public enum Tile : sbyte
		{
			Floor,
			Wall,
			Door,
			Monster,
			Item,
			Player,
			Stairs
		}
		public int width;
		public int height;
		public Tile[] Tiles;

		public List<Monster> CreateEnemies(Random random)
		{
			List<Monster> monsters = new List<Monster>();

			for (int y = 0; y < height; y++)
			{
				for (int x = 0; x < width; x++)
				{
					int ti = y * width + x;
					if (Tiles[ti] == Map.Tile.Monster)
					{
						Monster m = Monster.CreateRandomMonster(random, new Vector2(x, y));
						monsters.Add(m);
						Tiles[ti] = (sbyte)Map.Tile.Floor;
					}
				}
			}
			return monsters;
		}
	}
}
