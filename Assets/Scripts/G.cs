using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class G
{
	public static Vector3 getMouseWorldPosition()
	{
		return UnityEngine.Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10f));
	}
}