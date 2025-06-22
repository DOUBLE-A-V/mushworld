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
using TipsTranslates;
using UnityEngine.PlayerLoop;

public class UI : MonoBehaviour
{
	[SerializeField] GameObject cellPrefab;
	[SerializeField] TMP_Text inventoryTitle;
	[SerializeField] TMP_Text subInventoryTitle;
	[SerializeField] TMP_Text tip;
	[SerializeField] public Player player;
	[SerializeField] public WorldItemManager worldItemManager;
	[SerializeField] private DropDownMenu dropDownMenu;
	[SerializeField] public ItemsUser itemsUser;
	[SerializeField] private GameObject inventoryUI;
	[SerializeField] private Image usingItemIcon;
	[SerializeField] private TMP_Text usingItemNum;
	[SerializeField] private TradeObject tradeObjectPrefab;
	[SerializeField] public Loader loader;
	[SerializeField] public TMP_InputField commandLine;
	
	
	public float cellSize = 50;
	public Inventory nowInventory;
	public Inventory subInventory;

	private float inventoryOffset = 0;

	List<Cell> cells = new List<Cell>();
	public List<ItemObject> items = new List<ItemObject>();
	
	List<Cell> subCells = new List<Cell>();
	public List<ItemObject> subItems = new List<ItemObject>();
	
	private List<TradeObject> tradeObjects = new List<TradeObject>();
	
	public int CLOSED = 0;
	public int OPENED = 1;
	public int SUB_OPENED = 2;
	public int USE_OPENED = 3;
	public int TRADE_OPENED = 4;
	
	public int state = 0;

	private Cell touchingCell;

	private bool isDragging = false;
	private Vector2 draggingOffset;
	private ItemObject draggingItem;

	private Vector2 cellStartPos;
	private Vector2 subCellStartPos;
	private Vector2 saveItemPos;
	
	private List<string> commandsHistory = new List<string>();
	private int commandsHistoryIndex = 0;
	
	public void openTradeUI(List<NPC.Trade> trades, NPC fromNPC)
	{
		foreach (NPC.Trade trade in trades)
		{
			TradeObject tradeObject = Instantiate(tradeObjectPrefab, transform);
			tradeObject.customize(fromNPC, trade);
			tradeObject.setPlayer(player);
			tradeObjects.Add(tradeObject);
		}

		Vector2 pos = new Vector2(0, (float)tradeObjects.Count * 53 / 2) - new Vector2(0, 26.5f);
		
		foreach (TradeObject tradeObject in tradeObjects)
		{
			tradeObject.transform.localPosition = pos;
			pos.y -= 56;
		}
		
		state = TRADE_OPENED;
		inventoryTitle.transform.localPosition = new Vector2(0, 280);
		inventoryTitle.text = fromNPC.npcName;
		inventoryTitle.gameObject.SetActive(true);
	}

	public void refreshCanTrades()
	{
		foreach (TradeObject tradeObject in tradeObjects)
		{
			tradeObject.refreshCanTrade();
		}
	}
	
	public void showInteractTip(string interactName, string tipText, bool translate = true)
	{
		if (translate)
		{
			tip.text = Translates.translates[interactName] + "\n" + tipText;	
		}
		else
		{
			tip.text = interactName + "\n" + tipText;
		}
		tip.transform.position = Input.mousePosition + new Vector3(0, -50, 0);
		tip.gameObject.SetActive(true);
	}

	public void hideInteractTip()
	{
		tip.gameObject.SetActive(false);
	}

	public void showInventoryUI()
	{
		inventoryUI.SetActive(true);
	}

	public void hideInventoryUI()
	{
		inventoryUI.SetActive(false);
	}

	public void refreshUsingItemIcon(string itemName)
	{
		Sprite sprite = Instantiate(Loader.worldItems[itemName].GetComponent<SpriteRenderer>().sprite);
		usingItemIcon.sprite = sprite;
		usingItemIcon.gameObject.SetActive(true);
	}

	public void clearUsingItemIcon()
	{
		usingItemIcon.gameObject.SetActive(false);
		usingItemNum.gameObject.SetActive(false);
	}

	public void refreshUsingItems()
	{
		player.itemsUsing.Clear();
		foreach (List<Item> line in player.useInventory.cells)
		{
			foreach (Item item in line)
			{
				if (item != null)
				{
					if (item.itemPartId == 0)
					{
						player.itemsUsing.Add(item);
					}
				}
			}
		}

		int count = 0;
		bool usingItemFound = false;
		foreach (Item item in player.itemsUsing)
		{
			if (player.usingItem != null)
			{
				if (item.id == player.usingItem.id)
				{
					usingItemFound = true;
					if (count != player.usingItemNum)
					{
						player.clearUsingItem();
						clearUsingItemIcon();
					}
				}
			}
			count++;
		}

		if (!usingItemFound)
		{
			player.clearUsingItem();
			clearUsingItemIcon();
		}
	}

	public void refreshUsingItemNumText()
	{
		usingItemNum.gameObject.SetActive(true);
		usingItemNum.text = (player.usingItemNum + 1).ToString();
	}
	
	public void toggleUseInventory()
	{
		if (subInventory != null)
		{
			if (state != USE_OPENED)
			{
				closeSubInventory();
				openInventory("акивные слоты", player.useInventory);
				state = USE_OPENED;
			}
			else
			{
				refreshUsingItems();
				closeSubInventory();
			}
		}
		else
		{
			openInventory("акивные слоты", player.useInventory);
			state = USE_OPENED;
		}
	}
	
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

	public void replaceItem(ItemObject item)
	{
		if (item.sub)
		{
			Item invItem = Item.getItem(item.itemID);
			if (nowInventory.addItem(invItem))
			{
				invItem = nowInventory.getItem(item.itemID);
				subInventory.removeItem(item.itemID);
				subItems.Remove(item);
				items.Add(item);
				item.sub = false;
					
				item.smoothMove(cellStartPos + invItem.position * new Vector2(1, -1) * 
					cellSize + (invItem.size - new Vector2(1, 1)) * cellSize / 2 * new Vector2(1, -1), 0.5f);
			}
		}
		else
		{
			Item invItem = Item.getItem(item.itemID);
			if (subInventory.addItem(invItem))
			{
				invItem = subInventory.getItem(item.itemID);
				nowInventory.removeItem(item.itemID);
				items.Remove(item);
				subItems.Add(item);
				item.sub = true;
				item.smoothMove(subCellStartPos + invItem.position * new Vector2(1, -1) * 
					cellSize + (invItem.size - new Vector2(1, 1)) * cellSize / 2 * new Vector2(1, -1), 0.5f);
			}
		}
	}

	public void replaceAnyOtherItem(ItemObject item)
	{
		List<ItemObject> itemsList;
		if (item.sub)
		{
			itemsList = subItems;
		}
		else
		{
			itemsList = items;
		}
		
		bool otherReplaced = false;
		foreach (ItemObject item2 in itemsList)
		{
			if (item2.itemName == item.itemName && item2.itemID != item.itemID)
			{
				replaceItem(item2);
				otherReplaced = true;
				break;
			}
		}

		if (!otherReplaced)
		{
			replaceItem(item);
		}
	}

	public void dropItem(ItemObject item)
	{
		if (item.sub)
		{
			subInventory.removeItem(item.itemID);	
		}
		else
		{
			nowInventory.removeItem(item.itemID);
		}
		if (player.right)
		{
			worldItemManager.createItem(player.transform.position + new Vector3(1, 0), Item.getItem(item.itemID));	
		}
		else
		{
			worldItemManager.createItem(player.transform.position - new Vector3(1, 0), Item.getItem(item.itemID));
		}

		if (item.sub)
		{
			subItems.Remove(item);
		}
		else
		{
			items.Remove(item);	
		}
		Destroy(item.gameObject);
	}

	public void throwMenuOnItem(ItemObject item)
	{
		dropDownMenu.setCurrentItem(item);
	}

	public void destroyItem(ItemObject item)
	{
		List<ItemObject> itemsList;
		
		if (item.sub)
		{
			items.Remove(item);
		}
		else
		{
			items.Remove(item);
		}
		
		Destroy(item.gameObject);
	}
	
	public void eatItem(ItemObject item)
	{
		if (item.sub)
		{
			subItems.Remove(item);
		}
		else
		{
			items.Remove(item);
		}
		player.eatItem(Item.getItem(item.itemID));
		Destroy(item.gameObject);
	}
	public void eatItem(Item item)
	{
		foreach (ItemObject item2 in items)
		{
			if (item2.itemID == item.id)
			{
				Destroy(item2.gameObject);
				items.Remove(item2);
				break;
			}
		}

		foreach (ItemObject item2 in subItems)
		{
			if (item2.itemID == item.id)
			{
				subItems.Remove(item2);
				items.Remove(item2);
				break;
			}
		}
		player.eatItem(item);
	}

	public void useItem(ItemObject item)
	{
		itemsUser.useItem(Item.getItem(item.itemID));
	}

	public void closeTrade()
	{
		foreach (TradeObject tradeObject in tradeObjects)
		{
			Destroy(tradeObject.gameObject);
		}
		tradeObjects.Clear();
		state = CLOSED;
		inventoryTitle.gameObject.SetActive(false);
	}
	
	void Update()
	{
		updateTouchingCell();
		updateDragging();
		if (Input.GetKeyUp(KeyCode.E))
		{
			if (state == TRADE_OPENED)
			{
				closeTrade();
			}
		}

		if (commandLine.gameObject.activeInHierarchy)
		{
			if (Input.GetKeyDown(KeyCode.DownArrow))
			{
				if (commandsHistoryIndex != -1 && commandsHistoryIndex < commandsHistory.Count-1)
				{
					commandsHistoryIndex++;
					commandLine.text = commandsHistory[commandsHistoryIndex];
				}
			} else if (Input.GetKeyDown(KeyCode.UpArrow))
			{
				if (commandsHistoryIndex == -1)
				{
					commandsHistoryIndex = commandsHistory.Count - 1;
				}
				else if (commandsHistoryIndex > 0)
				{
					commandsHistoryIndex--;
				}
				
				commandLine.text = commandsHistory[commandsHistoryIndex];
			}
		}

		if (Input.GetKeyDown(KeyCode.Escape))
		{
			closeCommandLine();
		}

		if (Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyDown(KeyCode.L))
		{
			showCommandLine();
		}
	}

	private void closeCommandLine()
	{
		commandLine.gameObject.SetActive(false);
		commandLine.enabled = false;
		commandsHistoryIndex = -1;
		commandLine.text = "";
	}

	private void showCommandLine()
	{
		commandLine.enabled = true;
		commandLine.gameObject.SetActive(true);
		commandLine.Select();
		commandLine.ActivateInputField();
	}

	public void completeCurrentCommand()
	{
		commandLine.enabled = false;
		commandLine.gameObject.SetActive(false);
		completeCommand(commandLine.text);
		commandsHistory.Add(commandLine.text);
		commandsHistoryIndex = -1;
		if (commandsHistory.Count > 10)
		{
			commandsHistory.RemoveAt(0);
		}
		commandLine.text = "";
	}
	
	private void completeCommand(string command)
	{
		List<string> cmdSep = new List<string>(command.Split(' '));
		switch (cmdSep[0])
		{
			case "createItem":
				if (!Loader.worldItems.ContainsKey(cmdSep[1]))
				{
					Loader.loadItemPrefab(cmdSep[1]);
				}
				if (player.right)
				{
					worldItemManager.createItem(player.transform.position + new Vector3(0.5f, 0, 0), cmdSep[1]);	
				}
				else
				{
					worldItemManager.createItem(player.transform.position - new Vector3(0.5f, 0, 0), cmdSep[1]);
				}

				break;
			case "makePrebuildFile":
				worldItemManager.makePrebuildFile();
				break;
			case "setIsland":
				Destroy(loader.islandObject);
				loader.islandObject = Instantiate(Resources.Load("islands/" + cmdSep[1]) as GameObject);
				break;
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
					draggingOffset = touchingCell.position - Item.getItem(item.itemID).position;
					saveItemPos = Item.getItem(item.itemID).position;
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

	public void closeSubInventory()
	{
		foreach (Cell cell in subCells)
		{
			Destroy(cell.gameObject);
		}
		foreach (ItemObject item in subItems)
		{
			Destroy(item.gameObject);
		}
		
		subCells.Clear();
		subItems.Clear();
		subInventory = null;
		subInventoryTitle.gameObject.SetActive(false);
		state = OPENED;

		foreach (ItemObject item in items)
		{
			item.transform.position += new Vector3(inventoryOffset, 0);
		}

		foreach (Cell cell in cells)
		{
			cell.transform.position += new Vector3(inventoryOffset, 0);
		}
		inventoryTitle.transform.position += new Vector3(inventoryOffset, 0);
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
		inventoryOffset = 200f;
		Vector2 startPos;
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
			
			inventoryOffset = cells[0].transform.position.x - startPos.x;
			
			subInventory = inventory;
			foreach (Cell cell in cells)
			{
				cell.gameObject.transform.position -= new Vector3(inventoryOffset, 0);
			}

			foreach (ItemObject item in items)
			{
				item.gameObject.transform.position -= new Vector3(inventoryOffset, 0);
			}
			state = SUB_OPENED;
		}

		
		if (sub)
		{
			subInventoryTitle.gameObject.SetActive(true);
			subInventoryTitle.text = title;
			
			startPos = new Vector2(Screen.width / 1.25f - (float)inventory.size.x / 2 * cellSize, Screen.height / 2 + (float)inventory.size.y / 2 * cellSize);
			subCellStartPos = startPos;

			inventoryTitle.transform.position -= new Vector3(inventoryOffset, 0);
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
			itemObject = Instantiate(ComponentHolderProtocol.GameObject(Loader.itemObjects[item.name]), transform).GetComponent<ItemObject>();	

			itemObject.ui = this;

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
