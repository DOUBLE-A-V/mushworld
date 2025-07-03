using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InvManager;
using DG.Tweening;
using DG.Tweening.Plugins.Options;
using Unity.VisualScripting;

public class ItemsUser : MonoBehaviour
{
	[SerializeField] private Player player;
	[SerializeField] private UI ui;


	public enum UseMethod
	{
		none = 0,
		hit = 1,
		throwProjectile = 2,
		art = 3
	}
	
	public static class UsingMethods
	{
		static private void resetItemRotation()
		{
			player.usingItemObject.transform.DORotate(new Vector3(0, 0, 0), 0.5f);
		}
		
		public static Player player;
		static public void hit(Item item)
		{
			player.usingItemObject.transform.rotation = Quaternion.Euler(0, 0, 0);
			player.usingItemObject.transform.DOKill();
			if (player.right)
			{
				player.usingItemObject.transform.DORotate(new Vector3(0, 0, -90), 0.1f).onComplete = resetItemRotation;	
			}
			else
			{
				player.usingItemObject.transform.DORotate(new Vector3(0, 0, 90), 0.1f).onComplete = resetItemRotation;
			}
		}

		static public void throwProjectile(Item item)
		{
			player.ui.worldItemManager.throwProjectile(item, item.stringData[0], player.usingItemObject.transform.position, player.ui.worldItemManager.getMouseWorldPosition());
		}

		static public void art()
		{
			player.ui.showArt();
		}
	}
	
	public void useItem(Item item)
	{
		switch (item.useMethod)
		{
			case UseMethod.none:
				break;
			case UseMethod.hit:
				UsingMethods.hit(item);
				break;
			case UseMethod.throwProjectile:
				UsingMethods.throwProjectile(item);
				break;
			case UseMethod.art:
				UsingMethods.art();
				break;
		}
		if (item.removeAfterUse)
		{
			Item.removeItem(item);
		}
	}
}
