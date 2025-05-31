using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;
using DG.Tweening;

public class ItemObject : MonoBehaviour
{
    public string itemName;
    public uint itemID;
    public bool sub = false;
    public Vector2 size = new Vector2(1, 1);
    public bool touching = false;
    private Image image;
    
    private Vector2 startMousePos;
	
    private bool leftPressed = false;
    private bool pressingBlocked = false;
	
    public bool dragging = false;


    private void Start()
    {
        image = GetComponent<Image>();
        DOTween.defaultEaseType = Ease.OutExpo;
    }

    public void smoothMove(Vector2 position, float time)
    {
        transform.DOMove(position, time);
    }
    
    void Update()
    {
        Vector2 mousePos = Input.mousePosition;
        if (mousePos.x > transform.position.x - size.x / 2 && mousePos.x < transform.position.x + size.x / 2 &&
            mousePos.y > transform.position.y - size.y / 2 && mousePos.y < transform.position.y + size.y / 2)
        {
            touching = true;
            image.color = new Color(0.7f, 1f, 0.7f, 1);
        }
        else
        {
            touching = false;
            image.color = new Color(1f, 1f, 1f, 1);
        }
        
        if (Input.GetMouseButton(0))
        {
            if (touching && !pressingBlocked)
            {
                if (!leftPressed)
                {
                    startMousePos = Input.mousePosition;
                } else if (Math.Abs(startMousePos.x - Input.mousePosition.x) > 4 ||
                           Math.Abs(startMousePos.y - Input.mousePosition.y) > 4)
                {
                    dragging = true;
                }
                leftPressed = true;
            }
            else
            {
                pressingBlocked = true;
            }
        }
        else
        {
            leftPressed = false;
            dragging = false;
            pressingBlocked = false;
        }
    }

}
