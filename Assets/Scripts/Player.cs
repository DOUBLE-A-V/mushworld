using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.VisualScripting;
using UnityEngine;
using InvManager;
using Effects;
using Effect = Effects.Effect;
using Debug = UnityEngine.Debug;

public class Player : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private float jumpForce;
    [SerializeField] public UI ui;

    [SerializeField] private float usingItemMoveTime;
    
    private bool pressedJump = false;
    public Vector2 movingSpeed;
    public bool controlsLocked = false;
    private bool isGrounded = false;
    public bool right = true;
    public InvManager.InventoryManager Inventory = new  InvManager.InventoryManager();
    public InventoryManager useInventory = new InventoryManager();
    [SerializeField] GameObject usingItemObject;
    private SpriteRenderer usingItemSprite;
    private SpriteRenderer sprite;

    public int usingItemNum = -1;

    public Vector2 usingItemOffset = Vector2.zero;
    
    
    public List<Item> itemsUsing = new List<Item>();
    public Item usingItem = null;
    
    public float health = 5;

    private const int DAMAGE_NORMAL = 0;

    Effects.Effects effects = new Effects.Effects();

    public void clearUsingItem()
    {
        usingItem = null;
        usingItemObject.SetActive(false);
        usingItemNum = -1;
    }
    
    void updateItemsEffects()
    {
        effects.clearItemsEffects();
        foreach (Item item in Inventory.items)
        {
            foreach (int[] effect in item.effects)
            {
                if (effect[2] == 0)
                {
                    effects.addEffect(effect[0], effect[1], EffectSource.item, item.id);   
                }
            }
        }
        effects.print();
    }

    public void eatItem(string name)
    {
        foreach (Item item in Inventory.items)
        {
            if (item.name == name && item.eatable)
            {
                eatItem(item);
            }
        }
    }
    public void eatItem(Item item)
    {
        if (item.eatable)
        {
            List<Effect> removeList = new List<Effect>();
            List<Effect> addList = new List<Effect>();
        
            foreach (int[] effect in item.effects)
            {
                if (effect[2] != 0)
                {
                    addList.Add(new Effect(effect[0], effect[1], EffectSource.item, item.id, effect[2]));
                }
                else
                {
                    removeList.Add(effects.getEffectBySource(EffectSource.item, item.id, effect[0]));
                }
            }

            foreach (Effect effect in removeList)
            {
                effects.removeEffect(effect);
            }
            removeList.Clear();

            foreach (Effect effect in addList)
            {
                effects.addEffect(effect);
            }
            addList.Clear();
        
            if (Inventory.hasItem(item.id))
            {
                Inventory.removeItem(item.id);
            }
            Item.removeItem(item.id);
        }
    }
    void Start()
    {
        Loader.loadItemPrefab("apple");
        usingItemSprite = usingItemObject.GetComponent<SpriteRenderer>();
        sprite = GetComponent<SpriteRenderer>();
        
        InventoryManager subInventory = new InventoryManager();
        new Item("null");
        new Item("apple", new Vector2(1, 1));
        
        Inventory.refactorCells(new Vector2(9, 6));
        useInventory.refactorCells(new Vector2(6, 2));
        
        Inventory.addItem(new Item("apple", new Vector2(5, 3), new List<int[]>()
        {
            new int[3] { 1, 15, 5 }
        }));
        Inventory.insertItem(new Item("apple", new Vector2(1, 1), new List<int[]>()
        {
            new int[3] { 1, 15, 5 }
        }), new Vector2(5, 5));
        
        for (int i = 0; i < 9; i++)
        {
            subInventory.addItem(new Item("apple", new Vector2(1, 1), new List<int[]>()
            {
                new int[3] { 1, 15, 5 }
            }));
        }
        subInventory.addItem(new Item("apple", new Vector2(2, 2), new List<int[]>()
        {
            new int[3] { 1, 15, 5 }
        }));
        
        updateItemsEffects();
        Inventory.printInventory();
        ui.openInventory("fuck", Inventory);
        ui.openInventory("fuck 2", subInventory);
    }

    public float affectDamage(int type, int amount)
    {
        switch (type)
        {
            case DAMAGE_NORMAL:
                foreach (Effect effect in effects.effects)
                {
                    switch (effect.type)
                    {
                        case Effect.EFFECT_DEFENSE:
                            amount = (effect.strength / 100) * amount;
                            break;
                    }
                }

                break;
        }
        return amount;
    }
    void doDamage(int damageType, int amount)
    {
        float finalAmount = affectDamage(damageType, amount);
        changeHealth(finalAmount);
    }
    void changeHealth(float amount)
    {
        health += amount;
    }

    void jump()
    {
        RaycastHit2D hit;
        bool jumped = false;
        for (int i = -4; i < 5; i++)
        {
            hit = Physics2D.Raycast(transform.position - new Vector3((float)i/10f, 1, 0), Vector2.down, 0.01f);
            if (hit.collider)
            {
                rb.AddForce(new Vector2(0, jumpForce));
                jumped = true;
                break;
            }
        }
        if (!jumped)
        {
            if (right)
            {
                for (int i = 5; i >= -5; i--)
                {
                    float y = (float)i / 10;
                    hit = Physics2D.Raycast(transform.position + new Vector3(0.4f, 0.5f, 0), Vector2.right, 0.05f);
                    if (!hit.collider && Physics2D.Raycast(transform.position + new Vector3(0.4f, y, 0), Vector2.right, 0.05f))
                    {
                        float addY = 0.1f;
                        while (true)
                        {
                            hit = Physics2D.Raycast(transform.position + new Vector3(0.4f, y + addY, 0), Vector2.right, 0.05f);
                            if (!hit.collider)
                            {
                                rb.velocity = Vector2.zero;
                                transform.position += new Vector3(0.5f, y + addY + 0.5f, 0);
                                break;
                            }
                            addY += 0.05f;
                        }

                        break;
                    }  
                }
            }
            else
            {
                for (int i = 5; i >= -5; i--)
                {
                    float y = (float)i / 10;
                    hit = Physics2D.Raycast(transform.position + new Vector3(-0.4f, 0.5f, 0), Vector2.left, 0.05f);
                    if (!hit.collider && Physics2D.Raycast(transform.position + new Vector3(-0.4f, y, 0), Vector2.left, 0.05f))
                    {
                        float addY = 0.1f;
                        while (true)
                        {
                            hit = Physics2D.Raycast(transform.position + new Vector3(-0.4f, y + addY, 0), Vector2.left, 0.05f);
                            if (!hit.collider)
                            {
                                rb.velocity = Vector2.zero;
                                transform.position += new Vector3(-0.5f, y + addY + 0.5f, 0);
                                break;
                            }
                            addY += 0.05f;
                        }

                        break;
                    }  
                }
            }
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (rb.velocity.y == 0)
        {
            isGrounded = true;
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (rb.velocity.y > 0)
        {
            isGrounded = false;
        }
    }

    private void selectItem(Item item)
    {
        int count = 0;
        usingItemNum = -1;
        foreach (Item item2 in itemsUsing)
        {
            if (item2.id == item.id)
            {
                usingItemNum = count;
                break;
            }
            count++;
        }
        
        usingItemObject.transform.position = transform.position;
        usingItemObject.SetActive(true);
        usingItem = item;
        usingItemSprite.sprite = Instantiate(Resources.Load<Sprite>("sprites/items/" + item.name));
        changeDirection(right);
        ui.refreshUsingItemNumText();
    }

    private void changeDirection(bool isRight = true)
    {
        right = isRight;
        sprite.flipX = !right;
        if (usingItem != null)
        {
            if (!usingItem.trackMouse)
            {
                usingItemSprite.flipX = !right;
            }
            else
            {
                usingItemSprite.flipX = false;
            }
        }
        if (right)
        {
            usingItemOffset = new Vector3(1, 0);
        }
        else
        {
            usingItemOffset = new Vector3(-1, 0);
        }
    }

    public void dropUsingItem()
    {
        useInventory.removeItem(usingItem.id);
        usingItemObject.SetActive(false);
        
        if (right)
        {
            ui.worldItemManager.createItem(transform.position + new Vector3(1, 0), usingItem);	
        }
        else
        {
            ui.worldItemManager.createItem(transform.position - new Vector3(1, 0), usingItem);
        }
        
        clearUsingItem();
        ui.clearUsingItemIcon();
    }

    public void useUsingItem()
    {
        if (usingItem.removeAfterUse)
        {
            useInventory.removeItem(usingItem.id);
            itemsUsing.Remove(usingItem);
        }
        if (usingItem.eatable)
        {
            eatItem(usingItem);
        }
        else
        {
            ui.itemsUser.useItem(usingItem);
        }

        if (usingItem.removeAfterUse)
        {
            clearUsingItem();
            ui.clearUsingItemIcon();
        }
    }
    
    private void compareControls()
    {
        if (ui.state == ui.CLOSED && usingItem != null)
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                dropUsingItem();
            }

            if (Input.GetMouseButtonDown(1))
            {
                useUsingItem();
            }
        }
        
        bool itemChanged = false;
        for (int i = 0; i < 6; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                if (itemsUsing.Count > i)
                {
                    selectItem(itemsUsing[i]);
                    itemChanged = true;
                    break;
                }
                else
                {
                    ui.clearUsingItemIcon();
                    usingItemObject.SetActive(false);
                }
            }
        }
        if (itemChanged)
        {
            ui.refreshUsingItemIcon(usingItem.name);
        }
        
        if (Input.GetKey(KeyCode.W) ||  Input.GetKey(KeyCode.Space))
        {
            if (!pressedJump)
            {
                jump();
            }
            pressedJump = true;
        }
        else
        {
            pressedJump = false;
        }

        if (Input.GetKey(KeyCode.A))
        {
            movingSpeed.x = speed * -1;
            if (right)
            {
                changeDirection(false);
            }
            right = false;
        } else if (Input.GetKey(KeyCode.D))
        {
            movingSpeed.x = speed;
            if (!right)
            {
                changeDirection(true);
            }
            right = true;
        }
        else
        {
            movingSpeed.x = 0;
        }

        RaycastHit2D hit = new RaycastHit2D();
        if (Input.GetKey(KeyCode.A))
        {
            hit = Physics2D.Raycast(transform.position + new Vector3(-0.4f, -0.5f, 0), Vector2.left, 0.0001f);
        } else if (Input.GetKey(KeyCode.D))
        {
            hit  = Physics2D.Raycast(transform.position + new Vector3(0.4f, -0.5f, 0), Vector2.right, 0.0001f);
        }
        if (hit.collider)
        {
            if (!hit.collider.CompareTag("worldItem"))
            {
                movingSpeed.x = 0;
            }
        }
    }

    public void toggleInventory()
    {
        if (ui.state == 0)
        {
            ui.openInventory("инвентарь", Inventory);
            ui.showInventoryUI();
        }
        else
        {
            ui.refreshUsingItems();
            ui.closeInventory();
            ui.hideInventoryUI();   
        }
    }
    void Update()
    {
        if (!controlsLocked)
        {
            compareControls();
        }
        effects.updateTimers(Time.deltaTime);
        if (Input.GetKeyUp(KeyCode.E) && !ui.worldItemManager.worldItemDragging)
        {
            toggleInventory();
        }
    }

    private void FixedUpdate()
    {
        if (usingItem != null)
        {
            if (usingItem.trackMouse)
            {
                usingItemObject.transform.LookAt(G.getMouseWorldPosition());
                usingItemObject.transform.rotation = new Quaternion(0, 0, usingItemObject.transform.rotation.z, 1);
            }
        }
        
        if ((movingSpeed.x > 0 && rb.velocity.x < movingSpeed.x) || (movingSpeed.x < 0 && rb.velocity.x > movingSpeed.x))
        {
            rb.velocity = new Vector2(movingSpeed.x, rb.velocity.y);
        } else if (movingSpeed.x == 0)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
        }

        if (usingItem != null)
        {
            usingItemObject.transform.position = Vector3.Lerp(usingItemObject.transform.position, transform.position + new Vector3(usingItemOffset.x, usingItemOffset.y), usingItemMoveTime);   
        }
    }
}
