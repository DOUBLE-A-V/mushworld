using System;
using System.Collections.Generic;
using UnityEngine;
namespace InvManager
{
    public class Item
    {
        static private Dictionary<string, List<string>> itemsStringDataPresets =  new Dictionary<string, List<string>>();
        static private Dictionary<string, List<float>> itemsFloatDataPresets =  new Dictionary<string, List<float>>();
        static private Dictionary<string, Vector2> itemsSizeDataPresets =  new Dictionary<string, Vector2>()
        {
            { "ultratest", new Vector2(3, 3) }
        };
        
        static private uint idCounter = 0;
        static public List<Item> allItems = new List<Item>();
       
        public uint id = 0;
        public int itemPartId = -1;
        public string name;
        public Vector2 size = new Vector2(1, 1);
        public Vector2 position = new Vector2(-1, -1);
        public List<string> stringData = new List<string>();
        public List<float> floatData = new List<float>();

        public Item(Item item)
        {
            this.id = item.id;
            this.name = item.name;
            this.itemPartId = item.itemPartId;
            
            string itemName = item.name;
            
            if (itemsStringDataPresets.ContainsKey(itemName))
            {
                foreach (string el in itemsStringDataPresets[itemName])
                {
                    this.stringData.Add(el);
                }
            }

            if (itemsFloatDataPresets.ContainsKey(itemName))
            {
                foreach (float el in itemsFloatDataPresets[itemName])
                {
                    this.floatData.Add(el);
                }   
            }

            if (itemsSizeDataPresets.ContainsKey(itemName))
            {
                this.size = itemsSizeDataPresets[itemName];
            }
            allItems.Add(this);
        }

        ~Item()
        {
            allItems.Remove(this);
        }
        public Item(string itemName)
        {
            idCounter++;
            this.id = idCounter;
            this.name = itemName;
            if (itemsStringDataPresets.ContainsKey(itemName))
            {
                foreach (string el in itemsStringDataPresets[itemName])
                {
                    this.stringData.Add(el);
                }
            }

            if (itemsFloatDataPresets.ContainsKey(itemName))
            {
                foreach (float el in itemsFloatDataPresets[itemName])
                {
                    this.floatData.Add(el);
                }   
            }

            if (itemsSizeDataPresets.ContainsKey(itemName))
            {
                this.size = itemsSizeDataPresets[itemName];
            }
            allItems.Add(this);
        }
    }
    public class InventoryManager
    {
        public List<List<Item>> cells = new List<List<Item>>()
        {
            new List<Item>() {null, null, null, null},
            new List<Item>() {null ,null, null, null},
            new List<Item>() {null, null, null, null},
            new List<Item>() {null, null, null, null},
        };
        public List<Item> items = new List<Item>();

        public void printInventory()
        {
            Vector2 countPosition = new Vector2(0, 0);
            string finalString = "------------------------------------------------------------\n";
            foreach (List<Item> i in cells)
            {
                string lineText = "|";
                countPosition.x = 0;
                foreach (Item i2 in i)
                {
                    if (i2 != null)
                    {
                        if (i2.position == countPosition)
                        {
                            lineText += i2.name + "|";
                        }
                        else
                        {
                            Vector2 calc = countPosition - i2.position;
                            lineText += i2.name + " #" + i2.itemPartId.ToString() + "|";
                        }
                    }
                    else
                    {
                        lineText += "null" + "|";
                    }
                    countPosition.x++;
                }

                finalString += lineText + "\n";
                countPosition.y++;
                finalString += "------------------------------------------------------------\n";;
            }
            Debug.Log(finalString);
        }
        
        public void refactorCells(Vector2 size)
        {
            cells.Clear();
            for (int i = 0; i < size.y; i++)
            {
                cells.Add(new List<Item>());
                for (int j = 0; j < size.x; j++)
                {
                    cells[cells.Count-1].Add(null);
                }
            }
        }
        
        public Item addItem(string itemName)
        {
            Item item = new Item(itemName);
            addItem(item);
            return item;
        }

        public Item getItem(string itemName)
        {
            foreach (Item i in items)
            {
                if (i.name == itemName)
                {
                    return i;
                }
            }
            return null;
        }

        public List<Item> getItem(string itemName, int amount)
        {
            List<Item> foundItems = new List<Item>();
            int count = 0;
            foreach (Item i in items)
            {
                if (i.name == itemName)
                {
                    foundItems.Add(i);
                    count++;
                }

                if (count == amount)
                {
                    return foundItems;
                }
            }

            if (count != amount)
            {
                return null;
            }
            return foundItems;
        }
        
        public Item getItem(uint id)
        {
            foreach (Item i in items)
            {
                if (i.id == id)
                {
                    return i;
                }
            }
            return null;
        }

        public Item getCell(Vector2 position)
        {
            Vector2 countPos = new Vector2(0, 0);
            foreach (List<Item> line in cells)
            {
                countPos.x = 0;
                foreach (Item i in line)
                {
                    if (countPos == position)
                    {
                        return i;
                    }
                    countPos.x++;
                }
                countPos.y++;
            }
            return null;
        }

        public bool checkSuitable(Item item, Vector2 position)
        {
            for (int i = 0; i < item.size.y; i++)
            {
                for (int j = 0; j < item.size.x; j++)
                {
                    if (getCell(position + new Vector2(j, i)) != null)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public void setCell(Item item, Vector2 position)
        {
            Debug.Log(position);
            cells[(int)position.y][(int)position.x] = item;
        }
        public bool addItem(Item item)
        {
            Vector2 nowPos = new Vector2(0, 0);
            bool checkPassed = false;
            foreach (List<Item> line in cells)
            {
                nowPos.x = 0;
                foreach (Item cell in line)
                {
                    if (checkSuitable(item, nowPos))
                    {
                        checkPassed = true;
                        break;
                    }

                    nowPos.x++;
                }

                if (checkPassed)
                {
                    break;
                }
                nowPos.y++;
            }

            Vector2 savePos = nowPos;
            if (checkPassed)
            {
                for (int i = 0; i < item.size.y; i++)
                {
                    nowPos.x = savePos.x;
                    for (int j = 0; j < item.size.x; j++)
                    {
                        Item itemBase = new Item(item.name);
                        itemBase.itemPartId = (int)(j + i * item.size.x);
                        setCell(itemBase, nowPos);
                        nowPos.x++;
                    }
                    nowPos.y++;
                }
            }
            return false;
        }
    }
}