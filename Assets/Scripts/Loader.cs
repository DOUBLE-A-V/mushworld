using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Loader : MonoBehaviour
{
	static public Dictionary<string, WorldItem> worldItems = new Dictionary<string, WorldItem>();
	static public Dictionary<string, ItemObject> itemObjects = new Dictionary<string, ItemObject>();

	static public Dictionary<string, NPC> npcs = new Dictionary<string, NPC>();
	
	static public void loadItemPrefab(string itemName)
	{
		Debug.Log(itemName);
		worldItems[itemName] = Resources.Load<WorldItem>("worldItems/" + itemName);
		itemObjects[itemName] = Resources.Load<ItemObject>("items/" + itemName);
	}

	static public void freeItemPrefab(string itemName)
	{
		Destroy(worldItems[itemName].gameObject);
		Destroy(itemObjects[itemName].gameObject);
		
		worldItems.Remove(itemName);
		itemObjects.Remove(itemName);
	}

	static public void loadNPCPrefab(string npcName)
	{
		npcs[npcName] = Resources.Load<NPC>("npcs/" + npcName);
	}

	static public void freeNPCPrefab(string npcName)
	{
		Destroy(npcs[npcName].gameObject);
		npcs.Remove(npcName);
	}

	void Start()
	{
		loadItemPrefab("notexture");
		loadNPCPrefab("npc");
	}
}
