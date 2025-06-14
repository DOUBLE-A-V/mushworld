using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TradeObject : MonoBehaviour
{
	private Player player;
	[SerializeField] private Button tradeButton;
	[SerializeField] private Image tradeItemIconPrefab;
	[SerializeField] private GameObject arrow;

	private NPC.Trade trade = null;
	private NPC npc = null;
	
	public void setPlayer(Player playerToSet)
	{
		player = playerToSet;
	}

	public void tradeMyTrade()
	{
		npc.makeTrade(trade);
		player.ui.refreshCanTrades();
	}

	public void refreshCanTrade()
	{
		tradeButton.interactable = npc.checkCanTrade(trade);
	}

	public void customize(NPC targetNPC, NPC.Trade targetTrade)
	{
		trade = targetTrade;
		npc = targetNPC;
		
		const int posAdd = 54;
		if (npc.checkCanTrade(trade))
		{
			tradeButton.interactable = true;
		}
		else
		{
			tradeButton.interactable = false;
		}

		Vector2 pos = new Vector2(-295, 0);
		
		foreach (string item in trade.required)
		{
			Image tradeItemIcon = Instantiate(tradeItemIconPrefab, transform);
			tradeItemIcon.sprite = Loader.worldItems[item].GetComponent<SpriteRenderer>().sprite;
			tradeItemIcon.transform.localPosition = pos;
			pos.x += posAdd;
		}

		arrow.transform.localPosition = pos;
		pos.x += posAdd;
		
		foreach (string product in trade.products)
		{
			Image tradeItemIcon = Instantiate(tradeItemIconPrefab, transform);
			tradeItemIcon.sprite = Loader.worldItems[product].GetComponent<SpriteRenderer>().sprite;
			tradeItemIcon.transform.localPosition = pos;
			pos.x += posAdd;
		}
	}
}
