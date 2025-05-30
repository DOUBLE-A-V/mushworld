using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InvManager;
using TMPro;
using Unity.VisualScripting;
using Inventory = InvManager.InventoryManager;
using Item = InvManager.Item;
using UnityEngine.UI;

public class UI : MonoBehaviour
{
	[SerializeField] GameObject cellPrefab;
	[SerializeField] TMP_Text inventoryTitle;
	[SerializeField] TMP_Text subInventoryTitle;
	public float cellSize = 100;
	public Inventory nowInventory;
	public Inventory subInventory;

	List<GameObject> cells = new List<GameObject>();
	List<GameObject> items = new List<GameObject>();
	
	List<GameObject> subCells = new List<GameObject>();
	List<GameObject> subItems = new List<GameObject>();

	public const int CLOSED = 0;
	public const int OPENED = 1;
	public const int SUB_OPENED = 2;
	
	public int state = 0;
	
	public void closeInventory()
	{
		foreach (GameObject cell in cells)
		{
			Destroy(cell);
		}

		foreach (GameObject cell in subCells)
		{
			Destroy(cell);
		}

		foreach (GameObject item in items)
		{
			Destroy(item);
		}

		foreach (GameObject item in subItems)
		{
			Destroy(item);
		}

		cells.Clear();
		subCells.Clear();
		
		items.Clear();
		subItems.Clear();
		
		nowInventory = null;
		subInventory = null;
		
		inventoryTitle.gameObject.SetActive(false);
		subInventoryTitle.gameObject.SetActive(false);
		state = CLOSED;
	}
	public void openInventory(string title, Inventory inventory)
	{
		Vector2 startPos;
		float invOffset = 200f;
		bool sub = nowInventory != null;
		if (!sub)
		{
			nowInventory = inventory;
			state = OPENED;
		}
		else
		{
			startPos = new Vector2(Screen.width / 4f - (float)nowInventory.size.x / 2 * cellSize, Screen.height / 2 + (float)nowInventory.size.y / 2 * cellSize);
			
			invOffset = cells[0].transform.position.x - startPos.x;
			
			subInventory = inventory;
			foreach (GameObject cell in cells)
			{
				cell.transform.position -= new Vector3(invOffset, 0);
			}

			foreach (GameObject item in items)
			{
				item.transform.position -= new Vector3(invOffset, 0);
			}
			state = SUB_OPENED;
		}

		
		if (sub)
		{
			subInventoryTitle.gameObject.SetActive(true);
			subInventoryTitle.text = title;
			
			startPos = new Vector2(Screen.width / 1.25f - (float)inventory.size.x / 2 * cellSize, Screen.height / 2 + (float)inventory.size.y / 2 * cellSize);

			inventoryTitle.transform.position -= new Vector3(invOffset, 0);
			subInventoryTitle.transform.position = startPos + cellSize * new Vector2(1, 1);
			subInventoryTitle.transform.position = new Vector2(Screen.width / 1.25f, subInventoryTitle.transform.position.y);
		}
		else
		{
			inventoryTitle.gameObject.SetActive(true);
			inventoryTitle.text = title;
			startPos = new Vector2(Screen.width / 2 - (float)inventory.size.x / 2 * cellSize, Screen.height / 2 + (float)inventory.size.y / 2 * cellSize);
			
			inventoryTitle.transform.position = startPos + cellSize * new Vector2(1, 1);
			inventoryTitle.transform.position = new Vector2(Screen.width / 2, inventoryTitle.transform.position.y);
		}
		for (int i = 0; i < inventory.size.y; i++)
		{
			for (int j = 0; j < inventory.size.x; j++)
			{
				GameObject cell = Instantiate(cellPrefab, transform);
				cell.transform.position = startPos + new Vector2(j * cellSize, i * cellSize * -1);
				RectTransform trans = cell.GetComponent<RectTransform>();
				Vector2 oldSize = trans.rect.size;
				Vector2 deltaSize = new Vector2(cellSize, cellSize) - oldSize;
				trans.offsetMin = trans.offsetMin - new Vector2(deltaSize.x * trans.pivot.x, deltaSize.y * trans.pivot.y);
				trans.offsetMax = trans.offsetMax + new Vector2(deltaSize.x * (1f - trans.pivot.x), deltaSize.y * (1f - trans.pivot.y));
				cells.Add(cell);
			}
		}
		updateItems(sub);
	}

	public void updateItems(bool sub = false)
	{
		List<Item> invItems;
		if (sub)
		{
			invItems = subInventory.items;
		}
		else
		{
			invItems = nowInventory.items;
		}
		foreach (Item item in invItems)
		{
			GameObject itemObject;
			if (item.prefab != null)
			{
				itemObject = Instantiate(ComponentHolderProtocol.GameObject(Resources.Load("items/" + item.prefab)), transform);	
			}
			else
			{
				itemObject = Instantiate(ComponentHolderProtocol.GameObject(Resources.Load("items/notexture")), transform);
			}

			if (sub)
			{
				subItems.Add(itemObject);
			}
			else
			{
				items.Add(itemObject);
			}

			Vector2 startPos;
			if (sub)
			{
				startPos = new Vector2(Screen.width / 1.25f - (float)subInventory.size.x / 2 * cellSize, Screen.height / 2 + (float)subInventory.size.y / 2 * cellSize);
			}
			else
			{
				startPos = new Vector2(Screen.width / 2 - (float)nowInventory.size.x / 2 * cellSize, Screen.height / 2 + (float)nowInventory.size.y / 2 * cellSize);	
			}
			
			RectTransform trans = itemObject.GetComponent<RectTransform>();
			Vector2 oldSize = trans.rect.size;
			Vector2 deltaSize = new Vector2(cellSize * item.size.x, cellSize * item.size.y) - oldSize;
			trans.offsetMin = trans.offsetMin - new Vector2(deltaSize.x * trans.pivot.x, deltaSize.y * trans.pivot.y);
			trans.offsetMax = trans.offsetMax + new Vector2(deltaSize.x * (1f - trans.pivot.x), deltaSize.y * (1f - trans.pivot.y));
			
			itemObject.GetComponent<ItemObject>().itemID = item.id;
			itemObject.GetComponent<ItemObject>().itemName = item.name;

			itemObject.transform.position = startPos + new Vector2(item.position.x, item.position.y * -1) * cellSize + (item.size - new Vector2(1, 1)) * cellSize/2 * new Vector2(1, -1);
		}
	}
}
