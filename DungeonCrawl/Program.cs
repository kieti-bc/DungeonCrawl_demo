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
		public enum Tile : sbyte
		{
			Floor,
			Wall,
			Monster,
			Item,
			Door
		}
		public int width;
		public int height;
		public sbyte[] Tiles;
	}

	internal class Monster
	{
		public string name;
		public Vector2 position;
		public int hitpoints;
		public char symbol;
		public ConsoleColor color;
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
		const int INFO_HEIGHT = 6;
		const int ENEMY_CHANCE = 3;
		const int ITEM_CHANCE = 4;
		const int ROOM_AMOUNT = 12;
		static void Main(string[] args)
		{
			PrintLine("Welcome Brave Adventurer!", ConsoleColor.Cyan);

			// Character creation 
			PlayerCharacter player = CreateCharacter();
			Console.CursorVisible = false;

			// Map Creation 
			Random random = new Random();
			Map level1 = CreateMap(random);

			// Enemy init
			List<Monster> monsters = CreateEnemies(level1, random);
			// Item init
			List<Item> items = CreateItems(level1, random);
			// Player init
			PlacePlayerToMap(player, level1);

			List<int> dirtyTiles = new List<int>();
			// Game loop
			while (true)
			{
				DrawMap(level1, dirtyTiles);
				dirtyTiles.Clear();
				DrawEnemies(monsters);
				DrawItems(items);

				DrawPlayer(player);
				// Draw map
				// Draw information
				// Wait for player command
				// Process player command
				string messageOut = "";
				while (true)
				{
					bool turnOver = DoPlayerTurn(level1, player, monsters, items, dirtyTiles, ref messageOut);
					DrawInfo(player, monsters, items, messageOut);
					if (turnOver)
					{
						break;
					}
				}
				// Either do computer turn or wait command again
				ProcessEnemies(monsters, level1, player, dirtyTiles, ref messageOut);
				if (messageOut != "")
				{
					DrawInfo(player, monsters, items, messageOut);
				}
				// Do computer turn
				// Process enemies

			}
		}

		static void PrintLine(string text, ConsoleColor color)
		{
			Console.ForegroundColor = color;
			Console.WriteLine(text);
		}
		static void Print(string text, ConsoleColor color)
		{
			Console.ForegroundColor = color;
			Console.Write(text);
		}
		static void PrintLine(string text)
		{
			Console.WriteLine(text);
		}
		static void Print(string text)
		{
			Console.Write(text);
		}

		static void Print(char symbol, ConsoleColor color)
		{
			Console.ForegroundColor = color;
			Console.Write(symbol);
		}
		static PlayerCharacter CreateCharacter()
		{
			PlayerCharacter character = new PlayerCharacter();
			character.name = "";
			character.hitpoints = 20;
			character.gold = 0;

			Print("What is your name?", ConsoleColor.Yellow);
			while (string.IsNullOrEmpty(character.name))
			{
				character.name = Console.ReadLine();
			}
			Print($"Welcome {character.name}!", ConsoleColor.Yellow);

			return character;
		}

		static void AddRoom(Map level, Random random)
		{
			int width = random.Next(4, 14);
			int height = random.Next(4, 16);
			int sx = random.Next(2, level.width - 2 - width);
			int sy = random.Next(2, level.height - 2 - height);
			int doorX = random.Next(1, width - 1);
			int doorY = random.Next(1, height - 1);

			// Create perimeter wall
			for (int y = 0; y < height; y++)
			{
				for (int x = 0; x < width; x++)
				{
					int ti = (sy + y) * level.width + (sx + x);
					if (y == 0 || x == 0 || y == height - 1 || x == width - 1)
					{

						if (y == doorY || x == doorX)
						{
							level.Tiles[ti] = (sbyte)Map.Tile.Door;
						}
						else
						{
							level.Tiles[ti] = (sbyte)Map.Tile.Wall;
						}
					}
				}
			}
		}

		static Map CreateMap(Random random)
		{
			Map level = new Map();

			level.width = Console.WindowWidth;
			level.height = Console.WindowHeight - INFO_HEIGHT;
			level.Tiles = new sbyte[level.width * level.height];

			// Create perimeter wall
			for (int y = 0; y < level.height; y++)
			{
				for (int x = 0; x < level.width; x++)
				{
					int ti = y * level.width + x;
					if (y == 0 || x == 0 || y == level.height - 1 || x == level.width - 1)
					{
						level.Tiles[ti] = (sbyte)Map.Tile.Wall;
					}
					else
					{
						level.Tiles[ti] = (sbyte)Map.Tile.Floor;
					}
				}
			}

			for (int r = 0; r < ROOM_AMOUNT; r++)
			{
				AddRoom(level, random);
			}

			// Add enemies and items
			for (int y = 0; y < level.height; y++)
			{
				for (int x = 0; x < level.width; x++)
				{
					int ti = y * level.width + x;
					if (level.Tiles[ti] == (sbyte)Map.Tile.Floor)
					{
						int chance = random.Next(100);
						if (chance < ENEMY_CHANCE)
						{
							level.Tiles[ti] = (sbyte)Map.Tile.Monster;
							continue;
						}

						chance = random.Next(100);
						if (chance < ITEM_CHANCE)
						{
							level.Tiles[ti] = (sbyte)Map.Tile.Item;
						}
					}
				}
			}

			return level;
		}

		static Monster CreateMonster(Random random)
		{
			Monster m = new Monster();
			int type = random.Next(4);
			if (type == 0)
			{
				m.name = "Goblin";
				m.hitpoints = 5;
				m.symbol = 'g';
				m.color = ConsoleColor.Green;
			}
			if (type == 1)
			{
				m.name = "Bat Man";
				m.hitpoints = 2;
				m.symbol = 'M';
				m.color = ConsoleColor.Magenta;
			}
			if (type == 2)
			{
				m.name = "Orc";
				m.hitpoints = 15;
				m.symbol = 'o';
				m.color = ConsoleColor.Red;
			}
			if (type == 3)
			{
				m.name = "Bunny";
				m.hitpoints = 1;
				m.symbol = 'B';
				m.color = ConsoleColor.Yellow;
			}
			return m;
		}
		static Item CreateItem(Random random)
		{
			Item i = new Item();
			int type = random.Next(4);
			if (type == 0)
			{
				i.name = "Book";
				i.type = ItemType.Treasure;
				i.value = 10;
			}
			if (type == 1)
			{
				i.name = "Sword";
				i.type = ItemType.Weapon;
				i.value = 20;
			}
			if (type == 2)
			{
				i.name = "Helmet";
				i.type = ItemType.Armor;
				i.value = 12;
			}
			if (type == 3)
			{
				i.name = "Apple juice";
				i.type = ItemType.Potion;
				i.value = 1;
			}
			return i;
		}
		static List<Monster> CreateEnemies(Map level, Random random)
		{
			List<Monster> monsters = new List<Monster>();

			for (int y = 0; y < level.height; y++)
			{
				for (int x = 0; x < level.width; x++)
				{
					int ti = y * level.width + x;
					if (level.Tiles[ti] == (sbyte)Map.Tile.Monster)
					{
						Monster m = CreateMonster(random);
						m.position = new Vector2(x, y);
						monsters.Add(m);
						level.Tiles[ti] = (sbyte)Map.Tile.Floor;
					}
				}
			}
			return monsters;
		}

		static List<Item> CreateItems(Map level, Random random)
		{
			List<Item> items = new List<Item>();

			for (int y = 0; y < level.height; y++)
			{
				for (int x = 0; x < level.width; x++)
				{
					int ti = y * level.width + x;
					if (level.Tiles[ti] == (sbyte)Map.Tile.Item)
					{
						Item m = CreateItem(random);
						m.position = new Vector2(x, y);
						items.Add(m);
						level.Tiles[ti] = (sbyte)Map.Tile.Floor;
					}
				}
			}
			return items;
		}


		static void PlacePlayerToMap(PlayerCharacter character, Map level)
		{
			character.position = new Vector2(3, 1);
		}
		static void DrawMap(Map level, List<int> dirtyTiles)
		{
			for (int y = 0; y < level.height; y++)
			{
				for (int x = 0; x < level.width; x++)
				{
					int ti = y * level.width + x;
					if (dirtyTiles.Contains(ti) || dirtyTiles.Count == 0)
					{
						Map.Tile tile = (Map.Tile)level.Tiles[ti];
						Console.SetCursorPosition(x, y);
						switch (tile)
						{
							case Map.Tile.Floor:
								Print(".", ConsoleColor.Gray); break;

							case Map.Tile.Wall:
								Print("#", ConsoleColor.DarkGray); break;

							case Map.Tile.Door:
								Print("+", ConsoleColor.Yellow); break;

							default: break;
						}
					}
				}
			}
		}
		static void DrawEnemies(List<Monster> enemies)
		{
			foreach (Monster m in enemies)
			{
				Console.SetCursorPosition((int)m.position.X, (int)m.position.Y);
				Print(m.symbol, m.color);
			}
		}

		static void DrawItems(List<Item> items)
		{
			foreach (Item m in items)
			{
				Console.SetCursorPosition((int)m.position.X, (int)m.position.Y);
				char symbol = '$';
				ConsoleColor color = ConsoleColor.Yellow;
				switch (m.type)
				{
					case ItemType.Armor:
						symbol = '[';
						color = ConsoleColor.White;
						break;
					case ItemType.Weapon:
						symbol = '}';
						color = ConsoleColor.Cyan;
						break;
					case ItemType.Treasure:
						symbol = '$';
						color = ConsoleColor.Yellow;
						break;
					case ItemType.Potion:
						symbol = '!';
						color = ConsoleColor.Red;
						break;
				}
				Print(symbol, color);
			}
		}

		static void DrawPlayer(PlayerCharacter character)
		{
			Console.SetCursorPosition((int)character.position.X, (int)character.position.Y);
			Print("@", ConsoleColor.White);
		}

		static void DrawInfo(PlayerCharacter player, List<Monster> enemies, List<Item> items, string message)
		{
			Console.SetCursorPosition(0, Console.WindowHeight - INFO_HEIGHT);
			PrintLine($"{player.name}: hp ({player.hitpoints}) gold ({player.gold})", ConsoleColor.White);
			PrintLine("                                                            ");
			PrintLine("                                                            ");
			PrintLine("                                                            ");
			Console.SetCursorPosition(0, Console.WindowHeight - INFO_HEIGHT + 1);
			PrintLine(message, ConsoleColor.Yellow);
			// Print visible monster symbols and names
			// Print visible item symbols and names
		}

		// Return true if turn is over

		static int PositionToTileIndex(Vector2 position, Map level)
		{
			return (int)position.X + (int)position.Y * level.width;
		}

		static bool DoPlayerTurnVsEnemies(PlayerCharacter character, List<Monster> enemies, Vector2 destinationPlace, ref string messageOut)
		{
			// Check enemies
			bool hitEnemy = false;
			Monster toRemoveMonster = null;
			foreach (Monster enemy in enemies)
			{
				if (enemy.position == destinationPlace)
				{
					messageOut = $"You hit {enemy.name}!";
					enemy.hitpoints -= 1;
					hitEnemy = true;
					if (enemy.hitpoints <= 0)
					{
						toRemoveMonster = enemy;
					}
				}
			}
			if (toRemoveMonster != null)
			{
				enemies.Remove(toRemoveMonster);
			}
			return hitEnemy;
		}

		static bool DoPlayerTurnVsItems(PlayerCharacter character, List<Item> items, Vector2 destinationPlace, ref string messageOut)
		{
			// Check items
			Item toRemoveItem = null;
			foreach (Item item in items)
			{
				if (item.position == destinationPlace)
				{
					messageOut = $"You find a ";
					switch (item.type)
					{
						case ItemType.Armor:
							messageOut += $"{item.name}, it fits you well";
							break;
						case ItemType.Weapon:
							messageOut += $"{item.name} to use in battle";
							break;
						case ItemType.Potion:
							messageOut += $"potion of {item.name}";
							break;
						case ItemType.Treasure:
							messageOut += $"valuable {item.name} and get {item.value} gold!";
							character.gold += item.value;
							break;
					}
					toRemoveItem = item;
					break;
				}
			}
			if (toRemoveItem != null)
			{
				items.Remove(toRemoveItem);
			}
			return false;
		}

		static bool DoPlayerTurn(Map level, PlayerCharacter character, List<Monster> enemies, List<Item> items, List<int> dirtyTiles, ref string messageOut)
		{
			Vector2 playerMove = new Vector2(0, 0);
			while (true)
			{
				ConsoleKeyInfo key = Console.ReadKey();
				if (key.Key == ConsoleKey.W || key.Key == ConsoleKey.UpArrow)
				{
					playerMove.Y = -1;
					break;
				}
				else if (key.Key == ConsoleKey.S || key.Key == ConsoleKey.DownArrow)
				{
					playerMove.Y = 1;
					break;
				}
				else if (key.Key == ConsoleKey.A || key.Key == ConsoleKey.LeftArrow)
				{
					playerMove.X = -1;
					break;
				}
				else if (key.Key == ConsoleKey.D || key.Key == ConsoleKey.RightArrow)
				{
					playerMove.X = 1;
					break;
				}
				// Other commands

			}

			int startTile = PositionToTileIndex(character.position, level);
			Vector2 destinationPlace = character.position + playerMove;

			if (DoPlayerTurnVsEnemies(character, enemies, destinationPlace, ref messageOut))
			{
				return true;
			}

			if (DoPlayerTurnVsItems(character, items, destinationPlace, ref messageOut))
			{
				return true;
			}

			// Check movement
			Map.Tile destination = GetTileAtMap(level, destinationPlace);
			if (destination == Map.Tile.Floor)
			{
				character.position = destinationPlace;
				dirtyTiles.Add(startTile);
			}
			else if (destination == Map.Tile.Door)
			{
				messageOut = "You open a door";
				character.position = destinationPlace;
				dirtyTiles.Add(startTile);
			}
			else if (destination == Map.Tile.Wall)
			{
				messageOut = "You hit a wall";
			}

			return true;
		}

		static Map.Tile GetTileAtMap(Map level, Vector2 position)
		{
			if (position.X >= 0 && position.X < level.width)
			{
				if (position.Y >= 0 && position.Y < level.height)
				{
					int ti = (int)position.Y * level.width + (int)position.X;
					return (Map.Tile)level.Tiles[ti];
				}
			}
			return Map.Tile.Wall;
		}

		static int GetDistanceBetween(Vector2 A, Vector2 B)
		{
			return (int)Vector2.Distance(A, B);
		}

		static void ProcessEnemies(List<Monster> enemies, Map level, PlayerCharacter character, List<int> dirtyTiles, ref string messageOut)
		{
			foreach (Monster enemy in enemies)
			{
			
				if (GetDistanceBetween(enemy.position, character.position) < 5)
				{
					Vector2 enemyMove = new Vector2(0, 0);

					if (character.position.X < enemy.position.X)
					{
						enemyMove.X = -1;
					}
					else if (character.position.X > enemy.position.X)
					{
						enemyMove.X = 1;
					}
					else if (character.position.Y > enemy.position.Y)
					{
						enemyMove.Y = 1;
					}
					else if (character.position.Y < enemy.position.Y)
					{
						enemyMove.Y = -1;
					}

					int startTile = PositionToTileIndex(enemy.position, level);
					Vector2 destinationPlace = enemy.position + enemyMove;
					if (destinationPlace == character.position)
					{
						messageOut += $"\n{enemy.name} hits you!";
						character.hitpoints -= 1;
					}
					else
					{ 
						Map.Tile destination = GetTileAtMap(level, destinationPlace);
						if (destination == Map.Tile.Floor)
						{
							enemy.position = destinationPlace;
							dirtyTiles.Add(startTile);
						}
						else if (destination == Map.Tile.Door)
						{
							enemy.position = destinationPlace;
							dirtyTiles.Add(startTile);
						}
						else if (destination == Map.Tile.Wall)
						{
							// NOP
						}
					}
				}
			}
		}
	}
}
