using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InvManager;

public class WorldItem : MonoBehaviour
{
    public uint id = 0;
    public string itemName = null;

    public bool touching = false;

    private bool tipShowed = false;
    
    private UI ui;
    private Player player;
    private SpriteRenderer sprite;
    public Vector2 size;
    
    public Rigidbody2D rb;
    private BoxCollider2D boxCollider;
    
    private bool visible = true;

    private int frameCount = 0;
    
    public void changeSize(Vector2 itemSize)
    {
        if (!ui)
        {
            ui = GameObject.Find("UI").GetComponent<UI>();
            sprite = GetComponent<SpriteRenderer>();
            boxCollider = GetComponent<BoxCollider2D>();
            rb = GetComponent<Rigidbody2D>();
        }
        
        Vector2 pixelSize = itemSize * ui.cellSize / 100;
        
        size = pixelSize;

        transform.localScale = pixelSize / sprite.size;
    }
    
    void disablePhysics()
    {
        boxCollider.enabled = false;
        rb.simulated = false;
    }

    void enablePhysics()
    {
        boxCollider.enabled = true;
        rb.simulated = true;
    }

    public Vector2 getLocalMousePosition()
    {
        return transform.InverseTransformPoint(UnityEngine.Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y,
            10f)));
    }
    
    private void Start()
    {
        ui = GameObject.Find("UI").GetComponent<UI>();
        player = GameObject.Find("player").GetComponent<Player>();
        
        rb = gameObject.GetComponent<Rigidbody2D>();
        boxCollider = gameObject.GetComponent<BoxCollider2D>();
        
        sprite = GetComponent<SpriteRenderer>();
    }

    void OnMouseEnter()
    {
        touching = true;
    }

    void OnMouseExit()
    {
        touching = false;
    }
    
    void Update()
    {
        frameCount++;
        if (frameCount == 10)
        {
            if (Math.Abs(transform.position.x - player.transform.position.x) > 12.5)
            {
                visible = false;
            }
            else
            {
                visible = true;
            }
        }
        if (!visible)
        {
            if (rb.velocity == Vector2.zero && rb.simulated)
            {
                disablePhysics();
            }
        }
        else
        {
            if (!rb.simulated)
            {
                enablePhysics();
            }
        }
        
        if (ui.state == 0)
        {
            if (touching)
            {
                ui.showTakeTip(itemName);
                tipShowed = true;
            }
            else if (tipShowed)
            {
                ui.hideTakeTip();
                tipShowed = false;
            }
        }
        
        if (Input.GetKeyUp(KeyCode.F))
        {
            if (touching && ui.state == 0)
            {
                if (player.Inventory.addItem(Item.getItem(id)))
                {
                    ui.worldItemManager.worldItems.Remove(this);
                    ui.hideTakeTip();
                    Destroy(gameObject);
                }
            }
        }
    }
}
