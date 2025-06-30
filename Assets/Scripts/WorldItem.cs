using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Xml.Schema;
using UnityEngine;
using InvManager;

public class WorldItem : MonoBehaviour
{
    [SerializeField] private Vector2 defaultSize;
    public uint id = 0;
    public string itemName = null;

    public bool touching = false;

    private bool tipShowed = false;
    
    private UI ui;
    private Player player;
    private SpriteRenderer sprite;
    [SerializeField] public Vector2 size;
    
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

        
        Vector2 pixelSize = (((size / defaultSize) / 0.32f) * (ui.cellSize / 100f)) * itemSize;

        transform.localScale = pixelSize / size;
        size = pixelSize;
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
            if (Vector2.Distance(transform.position, player.transform.position) > 15)
            {
                visible = false;
            }
            else
            {
                visible = true;
            }
            frameCount = 0;
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
                ui.showInteractTip(itemName, "F чтобы подобрать");
                tipShowed = true;
            }
            else if (tipShowed)
            {
                ui.hideInteractTip();
                tipShowed = false;
            }
        }
        if (Input.GetKeyUp(KeyCode.F) && !ui.commandLine.gameObject.activeInHierarchy)
        {
            if (touching && ui.state == 0)
            {
                if (player.Inventory.addItem(Item.getItem(id)))
                {
                    ui.worldItemManager.worldItems.Remove(this);
                    ui.hideInteractTip();
                    Destroy(gameObject);
                }
            }
        }
    }
}
