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
using DG.Tweening;

public class Player : MonoBehaviour
{
    private static readonly int State1 = Animator.StringToHash("state");
    [SerializeField] private float speed;
    [SerializeField] public Rigidbody2D rb;
    [SerializeField] private float jumpForce;
    [SerializeField] public UI ui;

    [SerializeField] private float usingItemMoveTime;

    [SerializeField] public List<Item.ItemPresets> itemsPresets;
    [SerializeField] private CameraScript cam;
    [SerializeField] private Animator animator;
    
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

    public bool requestEnableCam = false;
    
    Effects.Effects effects = new Effects.Effects();
    
    private Vector3 targetRotation = Vector3.zero;

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
            foreach (Item.EffectPreset effect in item.effects)
            {
                if (effect.time == 0)
                {
                    effects.addEffect(effect.type, effect.strength, EffectSource.item, item.id);   
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
        
            foreach (Item.EffectPreset effect in item.effects)
            {
                if (effect.time != 0)
                {
                    addList.Add(new Effect(effect.type, effect.strength, EffectSource.item, item.id, effect.time));
                }
                else
                {
                    removeList.Add(effects.getEffectBySource(EffectSource.item, item.id, effect.type));
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
        Item.itemsPresets = itemsPresets;
        
        sprite = GetComponent<SpriteRenderer>();
        
        Inventory.refactorCells(new Vector2(9, 6));
        useInventory.refactorCells(new Vector2(6, 2));
        
        Inventory.addItem(new Item("apple", new Vector2(5, 3)));
        Inventory.insertItem(new Item("apple", new Vector2(1, 1)), new Vector2(5, 5));
        
        updateItemsEffects();
        
        cam = Camera.main.GetComponent<CameraScript>();
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
                        case EffectType.defense:
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

    private void resetRotation()
    {
        transform.rotation = Quaternion.Euler(0, 0, 0);
    }
    
    void jump()
    {
        RaycastHit2D hit;
        bool jumped = false;
        bool wasNPC = false;
        for (int j = -10; j < 11; j++)
        {
            float i = j / 10f;
            hit = Physics2D.Raycast(transform.position - new Vector3((float)i/10f, 1, 0), Vector2.down, 0.01f);
            if (hit.collider && !hit.collider.CompareTag("npc"))
            {
                rb.AddForce(new Vector2(0, jumpForce));
                jumped = true;
                break;
            } else if (hit.collider)
            {
                wasNPC = true;
            }
        }
        if (!jumped || wasNPC)
        {
            if (right)
            {
                for (int i = 5; i >= -5; i--)
                {
                    float y = (float)i / 10;
                    hit = Physics2D.Raycast(transform.position + new Vector3(0.4f, 0.5f, 0), Vector2.right, 0.05f);
                    if (!hit.collider && Physics2D.Raycast(transform.position + new Vector3(0.4f, y, 0), Vector2.right, 0.05f))
                    {
                        rb.velocity = Vector2.zero;
                        rb.AddForce(new Vector2(0, jumpForce*1.2f));
                        targetRotation = new Vector3(0, 0, -1);
                        transform.rotation = Quaternion.Euler(0, 0, -1);
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
                    if (!hit.collider && Physics2D.Raycast(transform.position + new Vector3(-0.4f, y, 0), Vector2.left,
                            0.05f))
                    {
                        rb.velocity = Vector2.zero;
                        rb.AddForce(new Vector2(0, jumpForce*1.2f));
                        targetRotation = new Vector3(0, 0, 360);
                        transform.rotation = Quaternion.Euler(0, 0, 1);
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
        usingItemSprite.sprite = Instantiate(Loader.worldItems[item.name].GetComponent<SpriteRenderer>().sprite);
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
        if (Input.GetKeyUp(KeyCode.E) && !ui.worldItemManager.worldItemDragging)
        {
            toggleInventory();
        }
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
    }

    public void toggleInventory()
    {
        if (ui.state == 0)
        {
            ui.openInventory("инвентарь", Inventory);
            ui.showInventoryUI();
        }
        else if (ui.state != ui.TRADE_OPENED)
        {
            ui.refreshUsingItems();
            ui.closeInventory();
            ui.hideInventoryUI();
        }
    }
    void Update()
    {
        if (!controlsLocked && !ui.commandLine.gameObject.activeInHierarchy)
        {
            compareControls();
        }
        effects.updateTimers(Time.deltaTime);

        if (transform.position.y < -20)
        {
            transform.position = Vector2.zero;
            nextIsland();
        }

        if (requestEnableCam && rb.velocity.y == 0)
        {
            requestEnableCam = false;
            cam.followPlayer = true;
        }
    }

    private void nextIsland()
    {
        ui.loader.nextIsland();
        Sprite sprite = ui.loader.islandObject.GetComponent<SpriteRenderer>().sprite;
        transform.position = new Vector2((sprite.rect.width * ui.loader.islandObject.transform.localScale.x / 100f)/2f - 2, 10);
        Camera.main.transform.position = new Vector3((sprite.rect.width * ui.loader.islandObject.transform.localScale.x / 100f)/2f - 2, 0, -10);
        rb.velocity = new Vector2(0, 0.1f);
        Camera.main.gameObject.GetComponent<CameraScript>().followPlayer = false;
        requestEnableCam = true;
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
        
        if (rb.velocity.y == 0)
        {
            if ((Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D)) && !controlsLocked)
            {
                animator.SetInteger(State1, 1);
            }
            else
            {
                animator.SetInteger(State1, 2);
            }
        }

        if (targetRotation != Vector3.zero)
        {
            Debug.Log(transform.rotation.eulerAngles);
            transform.rotation = Quaternion.Euler(Vector3.Lerp(transform.rotation.eulerAngles, targetRotation, 0.2f));
            Debug.Log(transform.rotation.eulerAngles);
            if (360 - Math.Abs(transform.rotation.eulerAngles.z) > 355)
            {
                targetRotation = Vector3.zero;
                resetRotation();
            }
        }
    }
}
