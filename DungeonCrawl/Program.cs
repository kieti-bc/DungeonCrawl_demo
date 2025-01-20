using System.Numerics;

namespace DungeonCrawl
{
	internal class PlayerCharacter
	{
		public string name;
		public int hitpoints;
		public int gold;
		public Vector2 position;
	}

	internal class Map
	{
		public int mapWidth;
		public int mapHeight;
		int[] Tiles;
	}

	internal class Monster
	{
		public string name;
		public int hitpoints;
		public char symbol;
		public Vector2 position;
	}

	internal enum ItemType
	{
		Weapon,
		Armor,
		Potion,
		Treasure
	}

	internal class Item
	{
		public string name;
		public int value;
		public Vector2 position;
		public ItemType type;
	}


	internal class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine("Welcome Brave Adventurer!");

			// Character creation 
			PlayerCharacter player = CreateCharacter();

			// Map Creation 
			Map level1 = CreateMap();

			// Enemy init
			List<Monster> monsters = CreateEnemies(level1);
			// Item init
			List<Item> items = CreateItems(level1);
			// Player init
			PlacePlayerToMap(player, level1);

			// Game loop
			while(true)
			{
				DrawMap(level1);
				DrawInfo(player, monsters, items);
				// Draw map
				// Draw information
				// Wait for player command
				// Process player command
				while(DoPlayerTurn(level1, player, monsters, items) == false);
				// Either do computer turn or wait command again
				ProcessEnemies(monsters);
				// Do computer turn
				// Process enemies
			}
		}

		static PlayerCharacter CreateCharacter()
		{
			PlayerCharacter character = new PlayerCharacter();


			return character;
		}

		static Map CreateMap()
		{
			Map level = new Map();


			return level;
		}

		static List<Monster> CreateEnemies(Map level)
		{
			List<Monster> monsters = new List<Monster>();

			return monsters;
		}

		static List<Item> CreateItems(Map level)
		{
			List<Item> items = new List<Item>();

			return items;
		}

		static void PlacePlayerToMap(PlayerCharacter character, Map level)
		{

		}
		static void DrawMap(Map level)
		{

		}
		static void DrawInfo(PlayerCharacter player, List<Monster> enemies, List<Item> items)
		{

		}

		// Return true if turn is over
		static bool DoPlayerTurn(Map level, PlayerCharacter character, List<Monster> enemies, List<Item> items)
		{
			return false;
		}

		static void ProcessEnemies(List<Monster> enemies)
		{

		}
	}
}
