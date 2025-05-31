using System;
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
	public float cellSize = 50;
	public Inventory nowInventory;
	public Inventory subInventory;

	List<Cell> cells = new List<Cell>();
	List<ItemObject> items = new List<ItemObject>();
	
	List<Cell> subCells = new List<Cell>();
	List<ItemObject> subItems = new List<ItemObject>();

	public const int CLOSED = 0;
	public const int OPENED = 1;
	public const int SUB_OPENED = 2;
	
	public int state = 0;

	public Cell touchingCell;

	private bool isDragging = false;
	private Vector2 draggingOffset;
	private ItemObject draggingItem;

	private Vector2 cellStartPos;
	private Vector2 subCellStartPos;
	private Vector2 saveItemPos;
	
	private void updateTouchingCell()
	{
		foreach (Cell cell in cells)
		{
			if (cell.touching)
			{
				touchingCell = cell;
				break;
			}
		}

		foreach (Cell cell in subCells)
		{
			if (cell.touching)
			{
				touchingCell = cell;
				break;
			}
		}
	}
	void Update()
	{
		updateTouchingCell();
		updateDragging();
		if (Input.GetKeyUp(KeyCode.G))
		{
			Debug.Log("now inventory:");
			nowInventory.printInventory();
			Debug.Log("sub inventory:");
			subInventory.printInventory();
		}
	}

	private void updateDragging()
	{
		List<ItemObject> checkList;
		bool sub = false;
		if (touchingCell != false)
		{
			if (touchingCell.sub)
			{
				sub = true;
				checkList = subItems;
			}
			else
			{
				checkList = items;
			}
		}
		else
		{
			return;
		}

		Inventory inventoryUsing;
		Vector2 cellStartPosUsing;
		if (sub)
		{
			inventoryUsing = subInventory;
			cellStartPosUsing = subCellStartPos;
		}
		else
		{
			inventoryUsing = nowInventory;
			cellStartPosUsing = cellStartPos;
		}
		
		if (draggingItem)
		{
			Item item = Item.getItem(draggingItem.itemID);
			if (draggingItem.dragging && !isDragging)
			{
				isDragging = true;
				draggingOffset = touchingCell.position - inventoryUsing.getItem(draggingItem.itemID).position;
				saveItemPos = inventoryUsing.getItem(draggingItem.itemID).position;
			} else if (draggingItem.dragging)
			{
				if (new Vector2(touchingCell.position.x - draggingOffset.x,
					    touchingCell.position.y - draggingOffset.y) != saveItemPos)
				{
					Vector2 newPos = new Vector2(touchingCell.position.x - draggingOffset.x, touchingCell.position.y - draggingOffset.y);
					Vector2 bestPos = new Vector2(0, 0);
					for (int i = 0; i < inventoryUsing.size.y; i++)
					{
						for (int j = 0; j < inventoryUsing.size.x; j++)
						{
							Vector2 difference = newPos - new Vector2(j, i);
							Vector2 bestDifference = newPos - bestPos;
							if (inventoryUsing.checkSuitable(item, new Vector2(j, i)) &&
							    (Math.Abs(difference.x) + Math.Abs(difference.y)) / 2 < (Math.Abs(bestDifference.x) +
								    Math.Abs(bestDifference.y)) / 2)
							{
								bestPos = new Vector2(j, i);
							}
						}
					}
					if (inventoryUsing.checkSuitable(item, bestPos))
					{
						draggingItem.smoothMove(cellStartPosUsing + bestPos * new Vector2(1, -1) * 
							cellSize + (item.size - new Vector2(1, 1)) * cellSize / 2 * new Vector2(1, -1), 0.5f);
						saveItemPos = bestPos;
					}
				}
			} else if (isDragging)
			{
				if (sub && !draggingItem.sub)
				{
					subItems.Add(draggingItem);
					items.Remove(draggingItem);
					draggingItem.sub = true;
				}
				else if (!sub && draggingItem.sub)
				{
					items.Add(draggingItem);
					subItems.Remove(draggingItem);
					draggingItem.sub = false;
				}
				inventoryUsing.insertItem(Item.getItem(draggingItem.itemID), saveItemPos);
				isDragging = false;
				draggingItem = null;
			}
		}
		else
		{
			foreach (ItemObject item in checkList)
			{
				if (item.dragging && !isDragging)
				{
					isDragging = true;
					draggingOffset = touchingCell.position - inventoryUsing.getItem(item.itemID).position;
					saveItemPos = inventoryUsing.getItem(item.itemID).position;
					draggingItem = item;

					if (draggingItem.sub)
					{
						subInventory.removeItem(draggingItem.itemID);
					}
					else
					{
						nowInventory.removeItem(draggingItem.itemID);
					}
				}
			}	
		}
	}
	
	public void closeInventory()
	{
		foreach (Cell cell in cells)
		{
			Destroy(cell.gameObject);
		}

		foreach (Cell cell in subCells)
		{
			Destroy(cell.gameObject);
		}

		foreach (ItemObject item in items)
		{
			Destroy(item.gameObject);
		}

		foreach (ItemObject item in subItems)
		{
			Destroy(item.gameObject);
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
			
			cellStartPos = startPos;
			
			invOffset = cells[0].transform.position.x - startPos.x;
			
			subInventory = inventory;
			foreach (Cell cell in cells)
			{
				cell.gameObject.transform.position -= new Vector3(invOffset, 0);
			}

			foreach (ItemObject item in items)
			{
				item.gameObject.transform.position -= new Vector3(invOffset, 0);
			}
			state = SUB_OPENED;
		}

		
		if (sub)
		{
			subInventoryTitle.gameObject.SetActive(true);
			subInventoryTitle.text = title;
			
			startPos = new Vector2(Screen.width / 1.25f - (float)inventory.size.x / 2 * cellSize, Screen.height / 2 + (float)inventory.size.y / 2 * cellSize);
			subCellStartPos = startPos;

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

		if (!sub)
		{
			cellStartPos = startPos;
		}
		for (int i = 0; i < inventory.size.y; i++)
		{
			for (int j = 0; j < inventory.size.x; j++)
			{
				Cell cell = Instantiate(cellPrefab, transform).GetComponent<Cell>();
				cell.position = new Vector2(j, i);
				cell.cellSize = cellSize;
				cell.gameObject.transform.position = startPos + new Vector2(j * cellSize, i * cellSize * -1);
				cell.sub = sub; 
				RectTransform trans = cell.gameObject.GetComponent<RectTransform>();
				Vector2 oldSize = trans.rect.size;
				Vector2 deltaSize = new Vector2(cellSize, cellSize) - oldSize;
				trans.offsetMin = trans.offsetMin - new Vector2(deltaSize.x * trans.pivot.x, deltaSize.y * trans.pivot.y);
				trans.offsetMax = trans.offsetMax + new Vector2(deltaSize.x * (1f - trans.pivot.x), deltaSize.y * (1f - trans.pivot.y));
				if (sub)
				{
					subCells.Add(cell);
				}
				else
				{
					cells.Add(cell);	
				}
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
			ItemObject itemObject;
			if (item.prefab != null)
			{
				itemObject = Instantiate(ComponentHolderProtocol.GameObject(Resources.Load("items/" + item.prefab)), transform).GetComponent<ItemObject>();	
			}
			else
			{
				itemObject = Instantiate(ComponentHolderProtocol.GameObject(Resources.Load("items/notexture")), transform).GetComponent<ItemObject>();
			}

			if (sub)
			{
				subItems.Add(itemObject);
				itemObject.sub = true;
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

			itemObject.size = item.size * cellSize;
			
			RectTransform trans = itemObject.gameObject.GetComponent<RectTransform>();
			Vector2 oldSize = trans.rect.size;
			Vector2 deltaSize = new Vector2(cellSize * item.size.x, cellSize * item.size.y) - oldSize;
			trans.offsetMin = trans.offsetMin - new Vector2(deltaSize.x * trans.pivot.x, deltaSize.y * trans.pivot.y);
			trans.offsetMax = trans.offsetMax + new Vector2(deltaSize.x * (1f - trans.pivot.x), deltaSize.y * (1f - trans.pivot.y));
			
			itemObject.itemID = item.id;
			itemObject.itemName = item.name;

			itemObject.gameObject.transform.position = startPos + new Vector2(item.position.x, item.position.y * -1) * cellSize + (item.size - new Vector2(1, 1)) * cellSize/2 * new Vector2(1, -1);
		}

		foreach (Cell cell in cells)
		{
			cell.transform.SetSiblingIndex(0);
		}
		
		foreach (Cell cell in subCells)
		{
			cell.transform.SetSiblingIndex(0);
		}
	}
}
