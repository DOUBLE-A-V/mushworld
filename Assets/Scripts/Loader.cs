using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEngine;

public class Loader : MonoBehaviour
{
	public static Dictionary<string, WorldItem> worldItems = new Dictionary<string, WorldItem>();
	public static Dictionary<string, ItemObject> itemObjects = new Dictionary<string, ItemObject>();

	private static Dictionary<string, NPC> npcs = new Dictionary<string, NPC>();
	
	public static List<NPC> islandNPCs = new List<NPC>();
	public static List<WorldItem> islandWorldItems = new List<WorldItem>();

	private static string workDir = System.IO.Directory.GetCurrentDirectory();
	[SerializeField] private WorldItemManager worldItemManager;

	[Serializable]
	private class itemGenParams
	{
		public string itemName;
		public List<IslandType> islandsForGen;
	}
	
	[SerializeField] private List<itemGenParams> itemsGenParams;
	public enum IslandType
	{
		none = 0
	}
	
	public static int islandNum = -1;
	public IslandType islandType = IslandType.none;
	
	public static void loadItemPrefab(string itemName)
	{
		Debug.Log(itemName);
		worldItems[itemName] = Resources.Load<WorldItem>("worldItems/" + itemName);
		itemObjects[itemName] = Resources.Load<ItemObject>("items/" + itemName);
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

	public static void createNPC(string npcName, bool generateTrades = false, IslandType islandType = IslandType.none)
	{
		
	}
	
	public static void clearCurrentIsland()
	{
		foreach (NPC npc in islandNPCs)
		{
			Destroy(npc.gameObject);
		}
		islandNPCs.Clear();
		foreach (WorldItem worldItem in islandWorldItems)
		{
			Destroy(worldItem.gameObject);
		}
		islandWorldItems.Clear();

		foreach (string key in worldItems.Keys)
		{
			freeItemPrefab(key, false);
		}
		worldItems.Clear();
		itemObjects.Clear();
		npcs.Clear();
	}
	
	public static void loadNPC(string npcData)
	{
		List<string> splitData = new List<string>(npcData.Split("\n", StringSplitOptions.RemoveEmptyEntries));

		if (!npcs.ContainsKey(splitData[0]))
		{
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

		int count = 0;
		string data;
		foreach (NPC npc in islandNPCs)
		{
			data = npc.npcName;
			foreach (NPC.Trade trade in npc.tradesList)
			{
				data += "\n:trade";
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
		foreach (WorldItem worldItem in islandWorldItems)
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
	}

	void Update()
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
	}
}
