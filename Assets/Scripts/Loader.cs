using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;
using InvManager;
using Unity.VisualScripting;

public class Loader : MonoBehaviour
{
	public static Dictionary<string, WorldItem> worldItems = new Dictionary<string, WorldItem>();
	public static Dictionary<string, ItemObject> itemObjects = new Dictionary<string, ItemObject>();

	private static Dictionary<string, NPC> npcs = new Dictionary<string, NPC>();
	
	public static Dictionary<string, Projectile> projectiles = new Dictionary<string, Projectile>();
	
	public static List<NPC> islandNPCs = new List<NPC>();

	public static string workDir = System.IO.Directory.GetCurrentDirectory();
	[SerializeField] private WorldItemManager worldItemManager;
	[SerializeField] private UI ui;
	[SerializeField] private List<string> islandTypes;
	[SerializeField] private List<string> allNPCs;

	[SerializeField] private List<string> allEnemies;

	public GameObject islandObject = null;

	private IslandGenParams currentIslandGenParams = null;
	
	[Serializable]
	public class ItemGenParams
	{
		public string name;
		public List<GenTag> genTags;
	}

	[Serializable]
	public class IslandGenParams
	{
		public string islandType;
		public List<GenTag> genTags;

		public IslandGenParams(string islandType, List<GenTag> genTags)
		{
			this.islandType = islandType;
			this.genTags = genTags;
		}
	}
	
	[SerializeField] private List<ItemGenParams> itemsGenParams;
	[SerializeField] private List<IslandGenParams> islandsGenParams;
	public enum GenTag
	{
		none = 0,
		wood = 1,
		stone = 2,
		water = 3,
		brick = 4,
		vegetation = 5,
		butter = 6,
		diamond = 7,
		rubine = 8,
		iron = 9,
		gold = 10,
		silver = 11,
		plastic = 12,
		gas = 13,
		fire = 14,
		titanium = 15,
		extrahot = 16,
		copper = 17,
		rope = 18,
		glass = 19,
		coal = 20,
		meat = 21
	}
	
	public int islandNum = 0;
	public string islandType = "none";
	
	public void nextIsland()
	{
		if (System.IO.Directory.Exists(workDir + "/islands/" + (islandNum + 1)))
		{
			saveCurrentIsland();
			clearCurrentIsland();
			islandNum++;
			loadIsland(islandNum+1);
		}
		else
		{
			saveCurrentIsland();
			islandNum++;
			genIsland();
		}
	}
	
	public static void loadItemPrefab(string itemName)
	{
		worldItems[itemName] = Resources.Load<WorldItem>("worldItems/" + itemName);
		itemObjects[itemName] = Resources.Load<ItemObject>("items/" + itemName);
	}

	public static void loadProjectilePrefab(string projectileName)
	{
		projectiles[projectileName] = Resources.Load<Projectile>("projectiles/" + projectileName);
	}

	public static void freeItemPrefab(string itemName, bool removeFromList = true)
	{
		if (removeFromList)
		{
			worldItems.Remove(itemName);
			itemObjects.Remove(itemName);
		}
	}

	public static void loadNPCPrefab(string npcName)
	{
		npcs[npcName] = Resources.Load<NPC>("npcs/" + npcName);
	}

	public NPC createNPC(string npcName, bool generateTrades = false)
	{
		if (!npcs.ContainsKey(npcName))
		{
			loadNPCPrefab(npcName);
		}
		
		NPC npc = Instantiate<NPC>(npcs[npcName]);
		if (generateTrades)
		{
			npc.genTrades(itemsGenParams, currentIslandGenParams);
		}
		islandNPCs.Add(npc);
		return npc;
	}

	public void genIsland()
	{
		clearCurrentIsland();

		if (islandNum == 0)
		{
			islandType = "first";
		}
		else
		{
			islandTypes.Remove("first");
			islandType = islandTypes[Random.Range(0, islandTypes.Count-1)];	
			islandTypes.Add("first");
		}
		
		islandObject = Instantiate(Resources.Load<GameObject>("islands/" + islandType.ToString()));

		foreach (IslandGenParams genParams in islandsGenParams)
		{
			if (genParams.islandType == islandType)
			{
				currentIslandGenParams = new IslandGenParams(genParams.islandType, new List<GenTag>(genParams.genTags));
			}
		}

		int targetRandomAdd = Random.Range(1, 4);
		
		int randomAdded = 0;

		
		int counter = 0;
		while (randomAdded != targetRandomAdd && counter < 128)
		{
			GenTag genTag;
			genTag = (GenTag)Random.Range(0, Enum.GetValues(typeof(GenTag)).Cast<GenTag>().ToList().Count);
			if (!currentIslandGenParams.genTags.Contains(genTag))
			{
				currentIslandGenParams.genTags.Add(genTag);
				randomAdded++;
			}

			counter++;
		}
		if (islandNum != 0)
		{
			for (int i = 0; i < Random.Range(1, 3); i++)
			{
				genNPC();
			}
		}

		if (Random.Range(1, 1) == 1)
		{
			genEnemy();
		}

		if (System.IO.File.Exists(workDir + "/prebuilds/" + islandType + ".pbd"))
		{
			loadPrebuildData();
		}

		saveCurrentIsland();
	}

	private void genEnemy()
	{
		string enemyName = allEnemies[Random.Range(0, allEnemies.Count-1)];
		NPC enemy = Instantiate(Resources.Load<NPC>("npcs/" + enemyName));
		enemy.transform.position = new Vector3(Random.Range(-8, 8), 8, 0);
		islandNPCs.Add(enemy);
	}

	private void genNPC()
	{
		NPC npc = createNPC(allNPCs[Random.Range(0, allNPCs.Count-1)], true);

		Sprite sprite = islandObject.GetComponent<SpriteRenderer>().sprite;

		Vector2 rect = new Vector2(sprite.rect.width/100, sprite.rect.height/100);
		
		npc.transform.position = new Vector2(Random.Range(rect.x / -2 + 2, rect.x / 2 - 2), rect.y/2);
		RaycastHit2D hit;
		sprite = npc.GetComponent<SpriteRenderer>().sprite;
		rect = new Vector2(sprite.rect.width/100, sprite.rect.height/100);
		int wasState = 0;
		hit = Physics2D.Raycast(transform.position - new Vector3(0, rect.y), Vector2.down, 0.01f);
		if (hit.collider && hit.collider.CompareTag("world"))
		{
			wasState = 0;
		} else if (!hit.collider)
		{
			wasState = 1;
		}

		int counter = 0;
		while (counter < 4096)
		{
			hit = Physics2D.Raycast(npc.transform.position - new Vector3(0, rect.y/2), Vector2.down, 0.01f);
			if (hit.collider && hit.collider.CompareTag("world"))
			{
				if (wasState == 0)
				{
					npc.transform.position += new Vector3(0, 0.1f);
				}
				else
				{
					break;
				}
			} else if (!hit.collider)
			{
				if (wasState == 1)
				{
					npc.transform.position -= new Vector3(0, 0.1f);
				}
				else
				{
					break;
				}
			}
			counter++;
		}
		foreach (NPC.Trade trade in npc.tradesList)
		{
			foreach (string item in trade.required)
			{
				if (!worldItems.ContainsKey(item))
				{
					loadItemPrefab(item);
				}
			}

			foreach (string product in trade.products)
			{
				if (!worldItems.ContainsKey(product))
				{
					loadItemPrefab(product);
				}
			}
		}
	}
	
	public void clearCurrentIsland()
	{
		foreach (NPC npc in islandNPCs)
		{
			Destroy(npc.gameObject);
		}
		islandNPCs.Clear();
		foreach (WorldItem worldItem in worldItemManager.worldItems)
		{
			Destroy(worldItem.gameObject);
		}
		worldItemManager.worldItems.Clear();

		foreach (string key in worldItems.Keys)
		{
			freeItemPrefab(key, false);
		}
		worldItems.Clear();
		itemObjects.Clear();
		npcs.Clear();

		Destroy(islandObject);
		islandObject = null;
		currentIslandGenParams = null;
	}
	
	public static void loadNPC(string npcData)
	{
		List<string> splitData = new List<string>(npcData.Split("\n", StringSplitOptions.RemoveEmptyEntries));

		if (!npcs.ContainsKey(splitData[0]))
		{
			Debug.Log(splitData[0]);
			loadNPCPrefab(splitData[0]);
		}

		NPC npc = Instantiate<NPC>(npcs[splitData[0]]);
		int count = 0;
		if (splitData.Contains(":trade"))
		{
			npc.tradesList.Clear();
		}
		foreach (string line in splitData)
		{
			if (line == ":trade")
			{
				List<string> products = new List<string>(splitData[count + 1].Split(";", StringSplitOptions.RemoveEmptyEntries));
				List<string> required = new List<string>(splitData[count + 2].Split(";", StringSplitOptions.RemoveEmptyEntries));
				foreach (string product in products)
				{
					if (!worldItems.ContainsKey(product))
					{
						loadItemPrefab(product);
					}
				}

				foreach (string item in required)
				{
					if (!worldItems.ContainsKey(item))
					{
						loadItemPrefab(item);
					}
				}
				npc.tradesList.Add(new NPC.Trade(products, required));
			}
			else if (line == ":position")
			{
				string[] posSplit = splitData[count + 1].Split(";", StringSplitOptions.RemoveEmptyEntries);
				npc.transform.position = new Vector2(float.Parse(posSplit[0].Replace(".", ",")), float.Parse(posSplit[1].Replace(".", ",")));
			}
			count++;
		}
		
		islandNPCs.Add(npc);
	}

	public void loadItem(string itemData)
	{
		List<string> splitData = new List<string>(itemData.Split("\n"));

		if (!worldItems.ContainsKey(splitData[0]))
		{
			loadItemPrefab(splitData[0]);
		}
		
		string[] posSplit = splitData[1].Split(";", StringSplitOptions.RemoveEmptyEntries);

		WorldItem item = worldItemManager.createItem(
			new Vector2(float.Parse(posSplit[0].Replace(".", ",")), float.Parse(posSplit[1].Replace(".", ","))),
			splitData[0]);
		item.transform.rotation = Quaternion.Euler(0, 0, float.Parse(splitData[2].Replace(".", ",")));
	}

	public void saveCurrentIsland()
	{
		string islandDir = workDir + "/islands/" + islandNum.ToString();
		
		if (System.IO.Directory.Exists(islandDir))
		{
			System.IO.Directory.Delete(islandDir, true);
		}
		System.IO.Directory.CreateDirectory(islandDir);
		System.IO.Directory.CreateDirectory(islandDir + "/npcs");
		System.IO.Directory.CreateDirectory(islandDir + "/items");
		
		System.IO.File.WriteAllText(islandDir + "/data", islandType.ToString());
		
		int count = 0;
		string data;
		foreach (NPC npc in islandNPCs)
		{
			data = npc.prefabName;
			foreach (NPC.Trade trade in npc.tradesList)
			{
				data += "\n:trade\n";
				string temp = "";
				foreach (string product in trade.products)
				{
					temp += product + ";";
				}

				data += temp + "\n";
				temp = "";
				foreach (string item in trade.required)
				{
					temp += item + ";";
				}
				data += temp;
			}
			data += "\n:position\n";
			data += npc.transform.position.x.ToString() + ";" + npc.transform.position.y.ToString();
			
			System.IO.File.WriteAllText(islandDir + "/npcs/" + count.ToString(), data);
			count++;
		}

		count = 0;
		foreach (WorldItem worldItem in worldItemManager.worldItems)
		{
			data = worldItem.itemName;
			data += "\n";
			data += worldItem.transform.position.x.ToString() + ";" + worldItem.transform.position.y.ToString();
			data += "\n";
			data += worldItem.rb.rotation.ToString();
			System.IO.File.WriteAllText(islandDir + "/items/" + count.ToString(), data);
			
			count++;
		}
	}
	
	public static void freeNPCPrefab(string npcName)
	{
		Destroy(npcs[npcName].gameObject);
		npcs.Remove(npcName);
	}

	void Start()
	{
		loadItemPrefab("notexture");
		loadNPCPrefab("npc");
		if (System.IO.File.Exists(workDir + "/save.txt"))
		{
			loadSave();
		}
		else
		{
			loadStart();
		}
	}

	private void loadStart()
	{
		islandNum = 0;
		genIsland();
		
		Sprite sprite = islandObject.GetComponent<SpriteRenderer>().sprite;
		ui.player.transform.position = new Vector2((sprite.rect.width * ui.loader.islandObject.transform.localScale.x / 100f)/2f - 2, 10);
		Camera.main.transform.position = new Vector3((sprite.rect.width * ui.loader.islandObject.transform.localScale.x / 100f)/2f - 2, 0, -10);
		ui.player.rb.velocity = new Vector2(0, 0.1f);
		Camera.main.gameObject.GetComponent<CameraScript>().followPlayer = false;
		ui.player.requestEnableCam = true;
	}

	private void loadSave()
	{
		List<Item> removeList = new List<Item>();
		foreach (Item item in ui.player.Inventory.items)
		{
			removeList.Add(item);
		}

		foreach (Item item in removeList)
		{
			Item.removeItem(ui.player.Inventory.removeItem(item.id));
		}
		removeList.Clear();
		foreach (Item item in ui.player.useInventory.items)
		{
			removeList.Add(item);
		}

		foreach (Item item in removeList)
		{
			Item.removeItem(ui.player.useInventory.removeItem(item.id));
		}
		removeList.Clear();
		
		
		List<string> data = new List<string>(System.IO.File.ReadAllLines(workDir + "/save.txt"));
		islandNum = int.Parse(data[0]);
		Item.idCounter = uint.Parse(data[1]);
		
		string[] posSplit = data[2].Split(';');
		
		ui.player.transform.position = new Vector2(float.Parse(posSplit[0].Replace(".", ",")), float.Parse(posSplit[1].Replace(".", ",")));

		data.RemoveAt(0);
		data.RemoveAt(0);
		data.RemoveAt(0);
		
		bool readingActiveCells = false;
		
		foreach (string line in data)
		{
			if (!readingActiveCells)
			{
				if (line == ":activecells")
				{
					readingActiveCells = true;
					continue;
				}
				else
				{
					string[] split = line.Split(" ");
					Item item = new Item(split[0]);
					item.id = uint.Parse(split[2]);
					posSplit = split[1].Split(";");
					ui.player.Inventory.insertItem(item, new Vector2(float.Parse(posSplit[0].Replace(".", ",")), float.Parse(posSplit[1].Replace(".", ","))));
				}	
			}
			else
			{
				string[] split = line.Split(" ");
				Item item = new Item(split[0]);
				item.id = uint.Parse(split[2]);
				posSplit = split[1].Split(";");
				ui.player.useInventory.insertItem(item, new Vector2(float.Parse(posSplit[0].Replace(".", ",")), float.Parse(posSplit[1].Replace(".", ","))));
			}
		}
		loadIsland(islandNum);
	}
	
	public void makeSave()
	{
		string data = islandNum.ToString();
		data += "\n" + Item.idCounter;
		data += "\n" + ui.player.transform.position.x + ";" +  ui.player.transform.position.y;
		foreach (Item item in ui.player.Inventory.items)
		{
			
			data += "\n" + item.name + " " + item.position.x.ToString() + ";" + item.position.y.ToString() + " " + item.id;
		}

		data += "\n:activecells";

		foreach (Item item in ui.player.useInventory.items)
		{
			data += "\n" + item.name + " " + item.position.x.ToString() + ";" + item.position.y.ToString() + " " + item.id;
		}
		
		System.IO.File.WriteAllText(workDir + "/save.txt", data);
		saveCurrentIsland();
	}

	public void exitGame()
	{
		makeSave();
		
		Application.Quit();
	}

	public void resetProgress()
	{
		foreach (string dir in System.IO.Directory.GetDirectories(workDir + "/islands"))
		{
			System.IO.Directory.Delete(dir, true);
		}
		System.IO.File.Delete(workDir + "/save.txt");
		
		List<Item> removeList = new List<Item>();
		foreach (Item item in ui.player.Inventory.items)
		{
			removeList.Add(item);
		}

		foreach (Item item in removeList)
		{
			Item.removeItem(ui.player.Inventory.removeItem(item.id));
		}
		removeList.Clear();
		foreach (Item item in ui.player.useInventory.items)
		{
			removeList.Add(item);
		}

		foreach (Item item in removeList)
		{
			Item.removeItem(ui.player.useInventory.removeItem(item.id));
		}
		removeList.Clear();
		
		loadStart();
		ui.closeMenu();
		ui.player.health = 5;
		ui.refreshHealthText(ui.player.health);
	}
	
	void Update()
	{
		if (!ui.commandLine.gameObject.activeInHierarchy)
		{
			if (Input.GetKeyDown(KeyCode.H))
			{
				Debug.Log("saving");
				islandNum = 1;
				saveCurrentIsland();
			}

			if (Input.GetKeyDown(KeyCode.N))
			{
				Debug.Log("loading island");
				clearCurrentIsland();
				loadIsland(1);
			}

			if (Input.GetKeyDown(KeyCode.J))
			{
				Debug.Log("next island");
				nextIsland();
			}	
		}

		if (Input.GetKeyDown(KeyCode.LeftAlt))
		{
			makeSave();
		}
	}

	
	
	void loadPrebuildData()
	{
		List<string> prebuildItems = System.IO.File.ReadAllLines(workDir + "/prebuilds/" + islandType + ".pbd").ToList();
		
		foreach (string item in prebuildItems)
		{
			List<string> splitData = new List<string>(item.Split(";", StringSplitOptions.RemoveEmptyEntries));
			if (!worldItems.ContainsKey(splitData[0]))
			{
				loadItemPrefab(splitData[0]);
			}
			Vector2 pos = new Vector2(float.Parse(splitData[1].Replace(".", ",")), float.Parse(splitData[2].Replace(".", ",")));
			worldItemManager.createItem(pos, splitData[0]).transform.rotation = Quaternion.Euler(0, 0, float.Parse(splitData[3].Replace(".", ",")));
		}
	}
	
	void loadIsland(int num)
	{
		string islandDir = workDir + "/islands/" + num.ToString();
		
		foreach (string file in System.IO.Directory.GetFiles(islandDir + "/npcs"))
		{
			loadNPC(System.IO.File.ReadAllText(file));
		}

		foreach (string file in System.IO.Directory.GetFiles(islandDir + "/items"))
		{
			loadItem(System.IO.File.ReadAllText(file));
		}
		islandType = System.IO.File.ReadAllText(islandDir + "/data");
		islandObject = Instantiate(Resources.Load("islands/" + islandType.ToString()) as GameObject);
	}
}
