using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InvManager;

public class ItemsUser : MonoBehaviour
{
	[SerializeField] private Player player;
	[SerializeField] private UI ui;
	
	
	public class UsingMethod
	{
		public const int NONE = 0;
		public const int TEST = 1;
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
			case UsingMethod.NONE:
				break;
			case UsingMethod.TEST:
				UsingMethods.test();
				break;
		}

		if (item.removeAfterUse)
		{
			Item.removeItem(item);
		}
	}
}
