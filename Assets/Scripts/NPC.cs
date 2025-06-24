using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using InvManager;
using Unity.VisualScripting;
using Random = UnityEngine.Random;

public class NPC : MonoBehaviour
{
    private Player player;
    
    private bool touching = false;
    [SerializeField] public string npcName;
    private bool tipShowed = false;
    
    [System.Serializable]
    public class Trade
    {
        public List<string> products;
        public List<string> required;

        public Trade(List<string> products, List<string> required)
        {
            this.products = products;
            this.required = required;
        }
    }
    
    [SerializeField] public List<Trade> tradesList = new List<Trade>();

    void Start()
    {
        player = FindFirstObjectByType<Player>();
    }

    private Trade genTrade(List<Loader.ItemGenParams> itemsGenParams, Loader.IslandGenParams islandGenParams)
    {
        Trade trade = new Trade(new List<string>(), new List<string>());
        List<Loader.GenTag> needGenTags = new List<Loader.GenTag>();
        Loader.ItemGenParams productGenParams = null;
        int counter = 0;
        
        while (productGenParams == null && counter < 256)
        {
            foreach (Loader.ItemGenParams itemGenParams in itemsGenParams)
            {
                bool canbe = true;
                needGenTags.Clear();
                foreach (Loader.GenTag genTag in itemGenParams.genTags)
                {
                    if (!islandGenParams.genTags.Contains(genTag))
                    {
                        canbe = false;
                        needGenTags.Add(genTag);
                    }
                }

                if (!canbe && Random.Range(0, 2) == 1)
                {
                    productGenParams = itemGenParams;
                    break;
                }
            }

            counter++;
        }
        if (productGenParams != null)
        {
            trade.products.Add(productGenParams.name);
            int first = Random.Range(0, needGenTags.Count);
            int second = Random.Range(0, needGenTags.Count);
            foreach (Loader.ItemGenParams itemGenParams in itemsGenParams)
            {
                if (itemGenParams != productGenParams)
                {
                    if ((needGenTags.Count > 1 && itemGenParams.genTags.Contains(needGenTags[first]) &&
                         itemGenParams.genTags.Contains(needGenTags[second])) ||
                        (needGenTags.Count == 1 && itemGenParams.genTags.Contains(needGenTags[first])))
                    {
                        if (!trade.required.Contains(itemGenParams.name))
                        {
                            trade.required.Add(itemGenParams.name);
                            if (Random.Range(0, 1) == 1)
                            {
                                break;
                            }
                        }
                    }
                }
            }
            return trade;
        }
        else
        {
            return null;
        }
    }
    
    public void genTrades(List<Loader.ItemGenParams> itemsGenParams, Loader.IslandGenParams islandGenParams)
    {
        tradesList.Clear();
        
		int randomNum = Random.Range(1, 8);

        for (int i = 0; i < randomNum; i++)
        {
            Trade trade = genTrade(itemsGenParams, islandGenParams);
            if (trade != null)
            {
                tradesList.Add(trade);
            }
        }
    }
    
    private void Update()
    {
        if (touching && player.ui.state == player.ui.CLOSED && !player.ui.commandLine.gameObject.activeInHierarchy)
        {
            tipShowed = true;
            player.ui.showInteractTip(npcName, "F чтобы взаимодействовать", false);
            if (Input.GetKeyUp(KeyCode.F))
            {
                player.ui.openTradeUI(tradesList, this);
            }
        }
        else if (tipShowed)
        {
            tipShowed = false;
            player.ui.hideInteractTip();
        }
    }

    public bool checkCanTrade(Trade trade)
    {
        Dictionary<string, int> productsAmount = new Dictionary<string, int>();
        
        foreach (string product in trade.required)
        {
            if (!productsAmount.ContainsKey(product))
            {
                productsAmount[product] = 1;
            }
            else
            {
                productsAmount[product] += 1;   
            }
        }

        foreach (KeyValuePair<string, int> product in productsAmount)
        {
            
            if (player.Inventory.getItem(product.Key, product.Value) == null)
            {
                return false;
            }
        }

        return true;
    }

    private void OnMouseEnter()
    {
        touching = true;
    }

    private void OnMouseExit()
    {
        touching = false;
    }
    

    public void makeTrade(Trade trade)
    {
        if (checkCanTrade(trade))
        {
            foreach (string item in trade.required)
            {
                Item.removeItem(player.Inventory.removeItem(item));
            }

            foreach (string item in trade.products)
            {
                if (player.transform.position.x > transform.position.x)
                {
                    player.ui.worldItemManager.createItem(transform.position + new Vector3(0.5f, 0), item);
                }
                else
                {
                    player.ui.worldItemManager.createItem(transform.position - new Vector3(0.5f, 0), item);
                }
            }
        }
    }
}
