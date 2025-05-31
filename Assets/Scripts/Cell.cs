using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Unity.VisualStudio.Editor;
using UnityEngine;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;

public class Cell : MonoBehaviour
{
	public bool touching = false;
	public float cellSize = 50;
	public Vector2 position;
	public bool sub = false;
	void Update()
	{
		Vector2 mousePos = Input.mousePosition;
		if (mousePos.x > transform.position.x - cellSize / 2 && mousePos.x < transform.position.x + cellSize / 2 &&
		    mousePos.y > transform.position.y - cellSize / 2 && mousePos.y < transform.position.y + cellSize / 2)
		{
			touching = true;
		}
		else
		{
			touching = false;
		}
	}
}
