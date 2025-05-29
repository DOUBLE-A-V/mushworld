using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InvManager;
using Unity.VisualScripting;
using Inventory = InvManager.InventoryManager;
using Item = InvManager.Item;
using UnityEngine.UI;

public class UI : MonoBehaviour
{
	[SerializeField] GameObject cellPrefab;
	public float cellSize = 50;
	public Inventory nowInventory;
	public void openInventory(string title, Inventory inventory)
	{
		nowInventory = inventory;
		Vector2 startPos = new Vector2(Screen.width / 2 - (float)inventory.size.x / 2 * cellSize, Screen.height / 2 - (float)inventory.size.y / 2 * cellSize);
		for (int i = 0; i < inventory.size.y; i++)
		{
			for (int j = 0; j < inventory.size.x; j++)
			{
				GameObject cell = Instantiate(cellPrefab, transform);
				cell.transform.position = startPos + new Vector2(j * cellSize, i * cellSize);
				RectTransform trans = cell.GetComponent<RectTransform>();
				Vector2 oldSize = trans.rect.size;
				Vector2 deltaSize = new Vector2(cellSize, cellSize) - oldSize;
				trans.offsetMin = trans.offsetMin - new Vector2(deltaSize.x * trans.pivot.x, deltaSize.y * trans.pivot.y);
				trans.offsetMax = trans.offsetMax + new Vector2(deltaSize.x * (1f - trans.pivot.x), deltaSize.y * (1f - trans.pivot.y));
			}
		}
	}

	public void updateItems()
	{
		foreach (Item item in nowInventory.items)
		{
			GameObject itemObject = Instantiate(ComponentHolderProtocol.GameObject(Resources.Load("items/" + item.prefab)), transform);
		}
	}
}
