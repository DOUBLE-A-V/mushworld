using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InvManager;

public class NPC : MonoBehaviour
{
    private Player player;
    
    private bool touching = false;
    [SerializeField] public string npcName;
    
    [SerializeField] Dictionary<string, int> buyDict = new Dictionary<string, int>();
    [SerializeField] Dictionary<string, int> sellDict = new Dictionary<string, int>();
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
    
    [SerializeField] List<Trade> tradesList = new List<Trade>();

    void Start()
    {
        player = FindFirstObjectByType<Player>();
    }
    
    private void Update()
    {
        if (touching && player.ui.state == player.ui.CLOSED)
        {
            player.ui.showInteractTip(npcName, "F чтобы взаимодействовать", false);
            if (Input.GetKeyUp(KeyCode.F))
            {
                player.ui.openTradeUI(tradesList, this);
            }
        }
        else
        {
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
