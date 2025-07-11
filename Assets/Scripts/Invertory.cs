using System;
using System.Collections.Generic;
using System.Threading;
using JetBrains.Annotations;
using UnityEngine;
using Effects;


namespace InvManager
{
    public class Item
    {
        // all items presets
        [Serializable]
        public class EffectPreset
        {
            public EffectType type;
            public int strength;
            public int time;
        }
        
        [Serializable]
        public class ItemPresets
        {
            public string itemName;
            public List<string> stringData;
            public List<float> floatData;
            public Vector2 size = Vector2.one;
            public List<EffectPreset> effects;
            public bool eatable;
            public bool usable;
            public ItemsUser.UseMethod useMethod;
            public bool removeAfterUse;
            public bool trackMouse;
            public Vector2 usingScale;
        }
        
        public static List<ItemPresets> itemsPresets = new List<ItemPresets>();
        
        static public uint idCounter = 0;
        static public List<Item> allItems = new List<Item>();
       
        public Vector2 usingScale = Vector2.one;
        public uint id = 0;
        public int itemPartId = -1;
        public string name;
        public Vector2 size = new Vector2(1, 1);
        public Vector2 position = new Vector2(-1, -1);
        public List<string> stringData = new List<string>();
        public List<float> floatData = new List<float>();
        public uint realItemId = 0;
        public List<EffectPreset> effects = new List<EffectPreset>();
        public bool eatable = false;
        public bool usable = false;
        public ItemsUser.UseMethod useMethod = 0;
        public bool removeAfterUse = false;
        public bool trackMouse = false;
        public static Item getItem(string itemName)
        {
            foreach (Item item in allItems)
            {
                if (item.name == itemName)
                {
                    return item;
                }
            }

            return null;
        }

        public static Item getItem(uint id)
        {
            foreach (Item item in allItems)
            {
                if (item.id == id)
                {
                    return item;
                }   
            }
            return null;
        }

        public static void removeItem(uint id)
        {
            foreach (Item item in allItems)
            {
                if (item.id == id)
                {
                    allItems.Remove(item);
                    break;
                }
            }
        }

        public static void removeItem(Item item)
        {
            allItems.Remove(item);
        }
        public Item(Item item)
        {
            this.id = item.id;
            this.name = item.name;
            this.itemPartId = item.itemPartId;

            this.removeAfterUse = item.removeAfterUse;
            this.trackMouse = item.trackMouse;
            
            string itemName = item.name;
            
            this.stringData.AddRange(item.stringData);
            this.floatData.AddRange(item.floatData);
            this.effects.AddRange(item.effects);
            
            this.size = item.size;
            this.position = item.position;
            this.eatable = item.eatable;
            this.usable = item.usable;
            this.useMethod = item.useMethod;
            
            this.usingScale = item.usingScale;
            
            allItems.Add(this);
        }

        ~Item()
        {
            allItems.Remove(this);
        }

        public void delete()
        {
            allItems.Remove(this);
        }
        
        public Item(string itemName, Vector2? size = null, List<EffectPreset> effects = null, bool eatable = false, bool usable = false, ItemsUser.UseMethod useMethod = 0, bool removeAfterUse = false, bool trackMouse = false)
        {
            ItemPresets thisPresets = null;
            foreach (ItemPresets presets in itemsPresets)
            {
                if (presets.itemName == itemName)
                {
                    thisPresets = presets;
                    break;
                }
            }
            
            idCounter++;
            this.id = idCounter;
            this.name = itemName;
            this.eatable = eatable;
            this.usable = usable;
            this.useMethod = useMethod;
            this.removeAfterUse = removeAfterUse;
            this.trackMouse = trackMouse;

            if (thisPresets != null)
            {
                this.size = thisPresets.size;
                this.stringData = thisPresets.stringData;
                this.floatData = thisPresets.floatData;
                this.effects = thisPresets.effects;
                this.eatable = thisPresets.eatable;
                this.usable = thisPresets.usable;
                this.useMethod = thisPresets.useMethod;
                this.removeAfterUse = thisPresets.removeAfterUse;
                this.trackMouse = thisPresets.trackMouse;
                this.usingScale = thisPresets.usingScale;
            }

            if (size != null)
            {
                this.size = size.Value;
            }

            if (effects != null)
            {
                this.effects.AddRange(effects);
            }
            allItems.Add(this);
        }
    }
    public class InventoryManager
    {
        public List<List<Item>> cells = new List<List<Item>>()
        {
            new List<Item>() {null, null, null, null, null},
            new List<Item>() {null, null, null, null, null},
            new List<Item>() {null, null, null, null, null},
            new List<Item>() {null, null, null, null, null},
        };
        public Vector2 size = new Vector2(5, 4);
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
                        Vector2 calc = countPosition - i2.position;
                        lineText += i2.name + " #" + i2.itemPartId.ToString() + "|";
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
        
        public void refactorCells(Vector2 size_)
        {
            cells.Clear();
            for (int i = 0; i < size_.y; i++)
            {
                cells.Add(new List<Item>());
                for (int j = 0; j < size_.x; j++)
                {
                    cells[cells.Count-1].Add(null);
                }
            }

            this.size = size_;
        }
        
        public Item addItem(string itemName)
        {
            Item item = new Item(itemName);
            if (addItem(item))
            {
                return item;   
            }
            return null;
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
            if (position.x > this.size.x - 1 || position.y > this.size.y - 1 || position.x < 0 || position.y < 0)
            {
                return Item.getItem("null");
            }
            return this.cells[(int)position.y][(int)position.x];
        }

        public Item removeItem(uint id)
        {
            Item itemSave = Item.getItem(id);

            if (itemSave != null)
            {
                foreach (Item i in items)
                {
                    if (i.id == id)
                    {
                        items.Remove(i);
                        break;
                    }
                }
            
                Vector2 countPos = new Vector2(0, 0);
                List<Vector2> posesRemoving = new List<Vector2>();
                foreach (List<Item> line in cells)
                {
                    countPos.x = 0;
                    foreach (Item item in line)
                    {
                        if (item != null)
                        {
                            if (item.realItemId == id || (item.realItemId == 0 && item.id == id))
                            {
                                item.realItemId = id;
                                posesRemoving.Add(countPos);
                                if (item.itemPartId != 0)
                                {
                                    item.delete();
                                }
                            }
                        }
                        countPos.x++;
                    }
                    countPos.y++;
                }
                foreach (Vector2 pos in posesRemoving)
                {
                    setCell(null, pos);
                }
            }

            if (itemSave != null)
            {
                itemSave.realItemId = 0;
            }
            return itemSave;
        }

        public Item removeItem(string itemName)
        {
            foreach (Item i in items)
            {
                if (i.name == itemName)
                {
                    return removeItem(i.id);
                }
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
            cells[(int)position.y][(int)position.x] = item;
        }

        public bool hasItem(string itemName)
        {
            foreach (Item i in items)
            {
                if (i.name == itemName)
                {
                    return true;
                }
            }
            return false;
        }

        public bool hasItem(uint id)
        {
            foreach (Item i in items)
            {
                if (i.id == id)
                {
                    return true;
                }
            }
            return false;
        }

        public Item getRealItem(Item item)
        {
            return Item.getItem(item.realItemId);
        }

        public bool insertItem(Item item, Vector2 position)
        {
            if (checkSuitable(item, position))
            {
                Vector2 savePos = position;
                for (int i = 0; i < item.size.y; i++)
                {
                    position.x = savePos.x;
                    for (int j = 0; j < item.size.x; j++)
                    {
                        Item itemBase = new Item(item);
                        itemBase.itemPartId = (int)(j + i * item.size.x);
                        itemBase.realItemId = item.id;
                        if (itemBase.itemPartId == 0)
                        {
                            itemBase.id = item.id;
                            items.Add(itemBase);
                        }
                        setCell(itemBase, position);
                        itemBase.position = position;
                        position.x++;
                    }
                    position.y++;
                }
                item.delete();
                return true;
            }

            return false;
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
                        Item itemBase = new Item(item);
                        itemBase.itemPartId = (int)(j + i * item.size.x);
                        itemBase.realItemId = item.id;
                        if (itemBase.itemPartId == 0)
                        {
                            itemBase.id = item.id;
                            items.Add(itemBase);
                        }
                        setCell(itemBase, nowPos);
                        itemBase.position = nowPos;
                        nowPos.x++;
                    }
                    nowPos.y++;
                }
                item.delete();
                return true;
            }
            return false;
        }
    }
}