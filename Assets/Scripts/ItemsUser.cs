using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InvManager;

public class ItemsUser : MonoBehaviour
{
	[SerializeField] private Player player;
	[SerializeField] private UI ui;


	public enum UseMethod
	{
		none = 0,
		test = 1
	}

	private class UsingMethods
	{
		static public void test()
		{
			Debug.Log("test using");
		}
	}
	
	public void useItem(Item item)
	{
		switch (item.useMethod)
		{
			case UseMethod.none:
				break;
			case UseMethod.test:
				UsingMethods.test();
				break;
		}
		if (item.removeAfterUse)
		{
			Item.removeItem(item);
		}
	}
}
