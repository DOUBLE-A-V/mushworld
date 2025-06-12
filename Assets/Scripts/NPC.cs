using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InvManager;

public class NPC : MonoBehaviour
{
    [SerializeField] private Player player;
    
    [SerializeField] Dictionary<string, int> buyDict = new Dictionary<string, int>();
    [SerializeField] Dictionary<string, int> sellDict = new Dictionary<string, int>();
    [System.Serializable]
    public class Trade
    {
        public List<string> products;
        public List<string> required;
    }
    
    [SerializeField] List<Trade> tradesList = new List<Trade>();

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
            if (player.Inventory.getItem(product.Key, product.Value).Count == 0)
            {
                return false;
            }
        }

        return true;
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
            }
        }
    }
}
