using System;
using System.Numerics;

namespace DungeonCrawl
{

	enum GameState
	{
		CharacterCreation,
		GameLoop,
		Inventory,
		DeathScreen,
		Quit
	}
	enum PlayerTurnResult
	{
		TurnOver,
		NewTurn,
		OpenInventory,
		NextLevel,
		BackToGame
	}

	internal class PlayerCharacter
	{
		public string name;
		public int hitpoints;
		public int maxHitpoints;
		public Item weapon;
		public Item armor;
		public int gold;
		public Vector2 position;
		public List<Item> inventory;
	}

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
		public int quality; // means different things depending on the type
		public Vector2 position;
		public ItemType type;
	}


	internal class Program
	{
		const int INFO_HEIGHT = 6;
		const int COMMANDS_WIDTH = 12;
		const int ENEMY_CHANCE = 3;
		const int ITEM_CHANCE = 4;

		// Room generation 
		const int ROOM_AMOUNT = 12;
		const int ROOM_MIN_W = 4;
		const int ROOM_MAX_W = 12;
		const int ROOM_MIN_H = 4;
		const int ROOM_MAX_H = 8;

		static void Main(string[] args)
		{
			List<Monster> monsters = null;
			List<Item> items = null;
			PlayerCharacter player = null;
			Map currentLevel = null;
			Random random = new Random();

			List<int> dirtyTiles = new List<int>();
			List<string> messages = new List<string>();

			// Main loop
			GameState state = GameState.CharacterCreation;
			while (state != GameState.Quit)
			{
				switch (state)
				{
					case GameState.CharacterCreation:
						// Character creation screen
						player = CreateCharacter();
						Console.CursorVisible = false;
						Console.Clear();

						// Map Creation 
						currentLevel = CreateMap(random);

						// Enemy init
						monsters = CreateEnemies(currentLevel, random);
						// Item init
						items = CreateItems(currentLevel, random);
						// Player init
						PlacePlayerToMap(player, currentLevel);
						PlaceStairsToMap(currentLevel);
						state = GameState.GameLoop;
						break;
					case GameState.GameLoop:
						DrawMap(currentLevel, dirtyTiles);
						dirtyTiles.Clear();
						DrawEnemies(monsters);
						DrawItems(items);

						DrawPlayer(player);
						DrawCommands();
						DrawInfo(player, monsters, items, messages);
						// Draw map
						// Draw information
						// Wait for player command
						// Process player command
						while (true)
						{
							messages.Clear();
							PlayerTurnResult result = DoPlayerTurn(currentLevel, player, monsters, items, dirtyTiles, messages);
							DrawInfo(player, monsters, items, messages);
							if (result == PlayerTurnResult.TurnOver)
							{
								break;
							}
							else if (result == PlayerTurnResult.OpenInventory)
							{
								Console.Clear();
								state = GameState.Inventory;
								break;
							}
							else if (result == PlayerTurnResult.NextLevel)
							{
								currentLevel = CreateMap(random);
								monsters = CreateEnemies(currentLevel, random);
								items = CreateItems(currentLevel, random);
								PlacePlayerToMap(player, currentLevel);
								PlaceStairsToMap(currentLevel);
								Console.Clear();
								break;
							}
						}
						// Either do computer turn or wait command again
						// Do computer turn
						// Process enemies
						ProcessEnemies(monsters, currentLevel, player, dirtyTiles, messages);

						DrawInfo(player, monsters, items, messages);

						// Is player dead?
						if (player.hitpoints < 0)
						{
							state = GameState.DeathScreen;
						}

						break;
					case GameState.Inventory:
						// Draw inventory 
						PlayerTurnResult inventoryResult = DrawInventory(player, messages);
						if (inventoryResult == PlayerTurnResult.BackToGame)
						{
							state = GameState.GameLoop;
							DrawMapAll(currentLevel);
							DrawInfo(player, monsters, items, messages);
						}
						// Read player command
						// Change back to game loop
						break;
					case GameState.DeathScreen:
						DrawEndScreen(random);
						// Animation is over
						Console.SetCursorPosition(Console.WindowWidth/2 - 4, Console.WindowHeight / 2);
						Print("YOU DIED", ConsoleColor.Yellow);
						Console.SetCursorPosition(Console.WindowWidth/2 - 4, Console.WindowHeight / 2 + 1);
						while(true)
						{ 
							Print("Play again (y/n)", ConsoleColor.Gray);
							ConsoleKeyInfo answer = Console.ReadKey();
							if (answer.Key == ConsoleKey.Y)
							{
								state = GameState.CharacterCreation;
								break;
							}
							else if (answer.Key == ConsoleKey.N)
							{
								state = GameState.Quit;
								break;
							}
						}
						break;
				};
			}
			Console.ResetColor();
			Console.Clear();
			Console.CursorVisible = true;
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

		static void DrawBrickBg()
		{
			// Draw tiles
			Console.BackgroundColor = ConsoleColor.DarkGray;
			for (int y = 0; y < Console.WindowHeight; y++)
			{
				Console.SetCursorPosition(0, y);
				for (int x = 0; x < Console.WindowWidth; x++)
				{
					if ((x + y) % 3 == 0)
					{
						Print("|", ConsoleColor.Black);
					}
					else
					{
						Print(" ", ConsoleColor.DarkGray);
					}
				}
			}
		}

		static void DrawRectangle(int x, int y, int width, int height, ConsoleColor color)
		{
			Console.BackgroundColor = color;
			for (int dy = y; dy < y + height; dy++)
			{
				Console.SetCursorPosition(x, dy);
				for (int dx = x; dx < x + width; dx++)
				{
					Print(" ");
				}
			}
		}

		static void DrawRectangleBorders(int x, int y, int width, int height, ConsoleColor color, string symbol)
		{
			Console.SetCursorPosition(x, y);
			Console.ForegroundColor = color;
			for (int dx = x; dx < x + width; dx++)
			{
				Print(symbol);
			}

			for (int dy = y; dy < y + height; dy++)
			{
				Console.SetCursorPosition(x, dy);

				Print(symbol);
				Console.SetCursorPosition(x + width - 1, dy);
				Print(symbol);
			}
		}
		static void DrawEndScreen(Random random)
		{
			// Run death animation: blood flowing down the screen in columns
			// Wait until keypress
			byte[] speeds = new byte[Console.WindowWidth];
			byte[] ends = new byte[Console.WindowWidth];
			for (int i = 0; i < speeds.Length; i++)
			{
				speeds[i] = (byte)random.Next(1, 4);
				ends[i] = 0;
			}
			Console.BackgroundColor = ConsoleColor.DarkRed;
			Console.ForegroundColor = ConsoleColor.White;
			
			
			for (int row = 0; row < Console.WindowHeight - 2; row++)
			{
				Console.SetCursorPosition(0, row);
				for (int i = 0; i < Console.WindowWidth; i++)
				{
					Console.Write(" ");
				}
				Thread.Sleep(100);
			}
			
		}

		/*
		 * Character functions
		 */
		static PlayerCharacter CreateCharacter()
		{
			PlayerCharacter character = new PlayerCharacter();
			character.name = "";
			character.hitpoints = 20;
			character.maxHitpoints = character.hitpoints;
			character.gold = 0;
			character.weapon = null;
			character.armor = null;
			character.inventory = new List<Item>();

			Console.Clear();
			DrawBrickBg();

			// Draw entrance
			Console.BackgroundColor = ConsoleColor.Black;
			int doorHeight = (int)(Console.WindowHeight * (3.0f / 4.0f));
			int doorY = Console.WindowHeight - doorHeight;
			int doorWidth = (int)(Console.WindowWidth * (3.0f / 5.0f));
			int doorX = Console.WindowWidth / 2 - doorWidth / 2;

			DrawRectangle(doorX, doorY, doorWidth, doorHeight, ConsoleColor.Black);
			DrawRectangleBorders(doorX + 1, doorY + 1, doorWidth - 2, doorHeight - 2, ConsoleColor.Blue, "|");
			DrawRectangleBorders(doorX + 3, doorY + 3, doorWidth - 6, doorHeight - 6, ConsoleColor.DarkBlue, "|");

			Console.SetCursorPosition(Console.WindowWidth / 2 - 8, Console.WindowHeight / 2);
			Print("Welcome Brave Adventurer!");
			Console.SetCursorPosition(Console.WindowWidth / 2 - 8, Console.WindowHeight / 2 + 1);
			Print("What is your name?", ConsoleColor.Yellow);
			while (string.IsNullOrEmpty(character.name))
			{
				character.name = Console.ReadLine();
			}
			Print($"Welcome {character.name}!", ConsoleColor.Yellow);

			return character;
		}

		static void GiveItem(PlayerCharacter character, Item item)
		{
			// Inventory order
			// Weapons
			// Armors
			// Potions
			switch (item.type)
			{
				case ItemType.Weapon:
					if ((character.weapon != null && character.weapon.quality < item.quality)
						|| character.weapon == null)
					{
						character.weapon = item;
					}
					character.inventory.Insert(0, item);
					break;
				case ItemType.Armor:
					if ((character.armor != null && character.armor.quality < item.quality)
						|| character.armor == null)
					{
						character.armor = item;
					}
					int armorIndex = 0;
					while (armorIndex < character.inventory.Count && character.inventory[armorIndex].type == ItemType.Weapon)
					{
						armorIndex++;
					}
					character.inventory.Insert(armorIndex, item);
					break;
				case ItemType.Potion:
					character.inventory.Add(item);
					break;
				case ItemType.Treasure:
					character.gold += item.quality;
					break;
			}

		}

		static int GetCharacterDamage(PlayerCharacter character)
		{
			if (character.weapon != null)
			{
				return character.weapon.quality;
			}
			else
			{
				return 1;
			}
		}
		static int GetCharacterDefense(PlayerCharacter character)
		{
			if (character.armor != null)
			{
				return character.armor.quality;
			}
			else
			{
				return 0;
			}
		}

		static void UseItem(PlayerCharacter character, Item item, List<string> messages)
		{
			switch(item.type)
			{
				case ItemType.Weapon:
					character.weapon = item;
					messages.Add($"You are now wielding a {item.name}");
					break;
				case ItemType.Armor:
					character.armor = item;
					messages.Add($"You equip {item.name} on yourself.");
					break;
				case ItemType.Potion:
					character.hitpoints += item.quality;
					if (character.hitpoints > character.maxHitpoints)
					{
						character.maxHitpoints = character.hitpoints;
					}
					messages.Add($"You drink a potion and gain {item.quality} hitpoints");
					character.inventory.Remove(item);
					break;
			}
		}

		static void AddRoom(Map level, int boxX, int boxY, int boxWidth, int boxHeight, Random random)
		{
			int width = random.Next(ROOM_MIN_W, boxWidth);
			int height = random.Next(ROOM_MIN_H, boxHeight);
			int sx = boxX + random.Next(0, boxWidth - width);
			int sy = boxY + random.Next(0, boxHeight - height);
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
							level.Tiles[ti] = Map.Tile.Door;
						}
						else
						{
							level.Tiles[ti] = Map.Tile.Wall;
						}
					}
				}
			}
		}

		static Map CreateMap(Random random)
		{
			Map level = new Map();

			level.width = Console.WindowWidth - COMMANDS_WIDTH;
			level.height = Console.WindowHeight - INFO_HEIGHT;
			level.Tiles = new Map.Tile[level.width * level.height];

			// Create perimeter wall
			for (int y = 0; y < level.height; y++)
			{
				for (int x = 0; x < level.width; x++)
				{
					int ti = y * level.width + x;
					if (y == 0 || x == 0 || y == level.height - 1 || x == level.width - 1)
					{
						level.Tiles[ti] = Map.Tile.Wall;
					}
					else
					{
						level.Tiles[ti] = Map.Tile.Floor;
					}
				}
			}

			int roomRows = 3;
			int roomsPerRow = 6;
			int boxWidth = (Console.WindowWidth - COMMANDS_WIDTH - 2) / roomsPerRow;
			int boxHeight = (Console.WindowHeight - INFO_HEIGHT - 2) / roomRows;
			for (int roomRow = 0; roomRow < roomRows; roomRow++)
			{
				for (int roomColumn = 0; roomColumn < roomsPerRow; roomColumn++)
				{
					AddRoom(level, roomColumn * boxWidth + 1, roomRow * boxHeight + 1, boxWidth, boxHeight, random);
				}
			}

			// Add enemies and items
			for (int y = 0; y < level.height; y++)
			{
				for (int x = 0; x < level.width; x++)
				{
					int ti = y * level.width + x;
					if (level.Tiles[ti] == Map.Tile.Floor)
					{
						int chance = random.Next(100);
						if (chance < ENEMY_CHANCE)
						{
							level.Tiles[ti] = Map.Tile.Monster;
							continue;
						}

						chance = random.Next(100);
						if (chance < ITEM_CHANCE)
						{
							level.Tiles[ti] = Map.Tile.Item;
						}
					}
				}
			}

			// Find starting place for player
			for (int i = 0; i < level.Tiles.Length; i++)
			{
				if (level.Tiles[i] == Map.Tile.Floor)
				{
					level.Tiles[i] = Map.Tile.Player;
					break;
				}
			}

			return level;
		}

		static Monster CreateMonster(string name, int hitpoints, char symbol, ConsoleColor color, Vector2 position)
		{
			Monster monster = new Monster();
			monster.name = name;
			monster.hitpoints = hitpoints;
			monster.symbol = symbol;
			monster.color = color;
			monster.position = position;
			return monster;
		}

		static Monster CreateRandomMonster(Random random, Vector2 position)
		{
			int type = random.Next(4);
			return type switch
			{
				0 => CreateMonster("Goblin", 5, 'g', ConsoleColor.Green, position),
				1 => CreateMonster("Bat Man", 2, 'M', ConsoleColor.Magenta, position),
				2 => CreateMonster("Orc", 15, 'o', ConsoleColor.Red, position),
				3 => CreateMonster("Bunny", 1, 'B', ConsoleColor.Yellow, position)
			};
		}
		static Item CreateRandomItem(Random random, Vector2 position)
		{
			ItemType type = Enum.GetValues<ItemType>()[random.Next(4)];
			Item i = type switch
			{
				ItemType.Treasure => CreateItem("Book", type, 2, position),
				ItemType.Weapon => CreateItem("Sword", type, 3, position),
				ItemType.Armor => CreateItem("Helmet", type, 1, position),
				ItemType.Potion => CreateItem("Apple Juice", type, 1, position)
			};
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
					if (level.Tiles[ti] == Map.Tile.Monster)
					{
						Monster m = CreateRandomMonster(random, new Vector2(x, y));
						monsters.Add(m);
						level.Tiles[ti] = (sbyte)Map.Tile.Floor;
					}
				}
			}
			return monsters;
		}

		static Item CreateItem(string name, ItemType type, int quality, Vector2 position)
		{
			Item i = new Item();
			i.name = name;
			i.type = type;
			i.quality = quality;
			i.position = position;
			return i;
		}
		static List<Item> CreateItems(Map level, Random random)
		{
			List<Item> items = new List<Item>();

			for (int y = 0; y < level.height; y++)
			{
				for (int x = 0; x < level.width; x++)
				{
					int ti = y * level.width + x;
					if (level.Tiles[ti] == Map.Tile.Item)
					{
						Item m = CreateRandomItem(random, new Vector2(x, y));
						items.Add(m);
						level.Tiles[ti] = (sbyte)Map.Tile.Floor;
					}
				}
			}
			return items;
		}


		static void PlacePlayerToMap(PlayerCharacter character, Map level)
		{
			for (int i = 0; i < level.Tiles.Length; i++)
			{
				if (level.Tiles[i] == Map.Tile.Player)
				{
					level.Tiles[i] = Map.Tile.Floor;
					int px = i % level.width;
					int py = i / level.width;

					character.position = new Vector2(px, py);
					break;
				}	
			}
		}
		static void PlaceStairsToMap(Map level)
		{
			for (int i = level.Tiles.Length-1; i >= 0; i--)
			{
				if (level.Tiles[i] == Map.Tile.Floor)
				{
					level.Tiles[i] = Map.Tile.Stairs;
					break;
				}
			}
		}

		static void DrawTile(byte x, byte y, Map.Tile tile)
		{
			Console.SetCursorPosition(x, y);
			switch (tile)
			{
				case Map.Tile.Floor:
					Print(".", ConsoleColor.Gray); break;

				case Map.Tile.Wall:
					Print("#", ConsoleColor.DarkGray); break;

				case Map.Tile.Door:
					Print("+", ConsoleColor.Yellow); break;
				case Map.Tile.Stairs:
					Print(">", ConsoleColor.Yellow); break;

				default: break;
			}
		}
		static void DrawMapAll(Map level)
		{
			for (byte y = 0; y < level.height; y++)
			{
				for (byte x = 0; x < level.width; x++)
				{
					int ti = y * level.width + x;
					Map.Tile tile = (Map.Tile)level.Tiles[ti];
					DrawTile(x, y, tile);
				}
			}
		}

		static void DrawMap(Map level, List<int> dirtyTiles)
		{
			if (dirtyTiles.Count == 0)
			{
				DrawMapAll(level);
			}
			else
			{
				foreach (int dt in dirtyTiles)
				{
					byte x = (byte)(dt % level.width);
					byte y = (byte)(dt / level.width);
					Map.Tile tile = (Map.Tile)level.Tiles[dt];
					DrawTile(x, y, tile);
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

		static void DrawCommands()
		{
			int cx = Console.WindowWidth - COMMANDS_WIDTH + 1;
			int ln = 1;
			Console.SetCursorPosition(cx, ln); ln++;
			Print(":Commands:", ConsoleColor.Yellow);
			Console.SetCursorPosition(cx, ln); ln++;
			Print("I", ConsoleColor.Cyan); Print("nventory", ConsoleColor.White);
		}

		static void DrawInfo(PlayerCharacter player, List<Monster> enemies, List<Item> items, List<string> messages)
		{
			int infoLine1 = Console.WindowHeight - INFO_HEIGHT;
			Console.SetCursorPosition(0, infoLine1);
			Print($"{player.name}: hp ({player.hitpoints}/{player.maxHitpoints}) gold ({player.gold}) ", ConsoleColor.White);
			int damage = 1;
			if (player.weapon != null)
			{
				damage = player.weapon.quality;
			}
			Print($"Weapon damage: {damage} ");
			int armor = 0;
			if (player.armor != null)
			{
				armor = player.armor.quality;
			}
			Print($"Armor: {armor} ");



			// Print last INFO_HEIGHT -1 messages
			DrawRectangle(0, infoLine1 + 1, Console.WindowWidth, INFO_HEIGHT - 2, ConsoleColor.Black);
			Console.SetCursorPosition(0, infoLine1 + 1);
			int firstMessage = 0;
			if (messages.Count > (INFO_HEIGHT - 1))
			{
				firstMessage = messages.Count - (INFO_HEIGHT - 1);
			}
			for (int i = firstMessage; i < messages.Count; i++)
			{
				Print(messages[i], ConsoleColor.Yellow);
			}
		}

		static PlayerTurnResult DrawInventory(PlayerCharacter character, List<string> messages)
		{
			Console.SetCursorPosition(1, 1);
			PrintLine("Inventory. Select item by inputting the number next to it. Invalid input closes inventory");
			ItemType currentType = ItemType.Weapon;
			PrintLine("Weapons", ConsoleColor.DarkCyan);
			for (int i = 0; i < character.inventory.Count; i++)
			{
				Item it = character.inventory[i];
				if (currentType == ItemType.Weapon && it.type == ItemType.Armor)
				{
					currentType = ItemType.Armor;
					PrintLine("Armors", ConsoleColor.DarkRed);
				}
				else if (currentType == ItemType.Armor && it.type == ItemType.Potion)
				{
					currentType = ItemType.Potion;
					PrintLine("Potions", ConsoleColor.DarkMagenta);
				}
				Print($"{i} ", ConsoleColor.Cyan);
				PrintLine($"{it.name} ({it.quality})", ConsoleColor.White);
			}
			while (true)
			{
				Print("Choose item: ", ConsoleColor.Yellow);
				string choiceStr = Console.ReadLine();
				int selectionindex = 0;
				if (int.TryParse(choiceStr, out selectionindex))
				{
					if (selectionindex >= 0 && selectionindex < character.inventory.Count)
					{
						UseItem(character, character.inventory[selectionindex], messages);
						break;
					}
				}
				else
				{
					messages.Add("No such item");
				}
			};
			return PlayerTurnResult.BackToGame;
		}

		static int PositionToTileIndex(Vector2 position, Map level)
		{
			return (int)position.X + (int)position.Y * level.width;
		}

		static bool DoPlayerTurnVsEnemies(PlayerCharacter character, List<Monster> enemies, Vector2 destinationPlace, List<string> messages)
		{
			// Check enemies
			bool hitEnemy = false;
			Monster toRemoveMonster = null;
			foreach (Monster enemy in enemies)
			{
				if (enemy.position == destinationPlace)
				{
					int damage = GetCharacterDamage(character);
					messages.Add($"You hit {enemy.name} for {damage}!");
					enemy.hitpoints -= damage;
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

		static bool DoPlayerTurnVsItems(PlayerCharacter character, List<Item> items, Vector2 destinationPlace, List<string> messages)
		{
			// Check items
			Item toRemoveItem = null;
			foreach (Item item in items)
			{
				if (item.position == destinationPlace)
				{
					string itemMessage = $"You find a ";
					switch (item.type)
					{
						case ItemType.Armor:
							itemMessage += $"{item.name}, it fits you well";
							break;
						case ItemType.Weapon:
							itemMessage += $"{item.name} to use in battle";
							break;
						case ItemType.Potion:
							itemMessage += $"potion of {item.name}";
							break;
						case ItemType.Treasure:
							itemMessage += $"valuable {item.name} and get {item.quality} gold!";
							break;
					};
					messages.Add(itemMessage);
					toRemoveItem = item;
					GiveItem(character, item);
					break;
				}
			}
			if (toRemoveItem != null)
			{
				items.Remove(toRemoveItem);
			}
			return false;
		}

		static PlayerTurnResult DoPlayerTurn(Map level, PlayerCharacter character, List<Monster> enemies, List<Item> items, List<int> dirtyTiles, List<string> messages)
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
				else if (key.Key == ConsoleKey.I)
				{
					return PlayerTurnResult.OpenInventory;
				}
			}

			int startTile = PositionToTileIndex(character.position, level);
			Vector2 destinationPlace = character.position + playerMove;

			if (DoPlayerTurnVsEnemies(character, enemies, destinationPlace, messages))
			{
				return PlayerTurnResult.TurnOver;
			}

			if (DoPlayerTurnVsItems(character, items, destinationPlace, messages))
			{
				return PlayerTurnResult.TurnOver;
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
				messages.Add("You open a door");
				character.position = destinationPlace;
				dirtyTiles.Add(startTile);
			}
			else if (destination == Map.Tile.Wall)
			{
				messages.Add("You hit a wall");
			}
			else if (destination == Map.Tile.Stairs)
			{
				messages.Add("You find stairs leading down");
				return PlayerTurnResult.NextLevel;
			}

			return PlayerTurnResult.TurnOver;
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

		static void ProcessEnemies(List<Monster> enemies, Map level, PlayerCharacter character, List<int> dirtyTiles, List<string> messages)
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
						// TODO: Random change for armor to protect?
						int damage = 1;
						damage -= GetCharacterDefense(character);
						if (damage <= 0)
						{
							damage = 1;
						}
						character.hitpoints -= damage;
						messages.Add($"{enemy.name} hits you for {damage} damage!");
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
