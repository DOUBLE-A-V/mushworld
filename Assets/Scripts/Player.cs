using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using InvManager;

public class Player : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private float jumpForce;
    
    private bool pressedJump = false;
    public Vector2 movingSpeed;
    public bool controlsLocked = false;
    private bool isGrounded = false;
    private bool right = true;
    public InvManager.InventoryManager Inventory = new  InvManager.InventoryManager();
    
    public float health = 5;

    private const int DAMAGE_NORMAL = 0;
    
    private const int EFFECT_DEFENSE = 0;
    
    class Effect
    {
        public int type;
        public int strength;
        public List<int> parameters;
        public Effect(int effectType_, int strength_)
        {
            this.type = effectType_;
            this.strength = strength_;
        }
    }

    class Effects
    {
        public List<Effect> effects = new List<Effect>();

        public void addEffect(Effect effect)
        {
            foreach (Effect e in this.effects)
            {
                if (e.type == effect.type)
                {
                    e.strength += effect.strength;
                    return;
                }
            }
            this.effects.Add(effect);
        }

        public Effect addEffect(int type, int strength)
        {
            Effect effect = new Effect(type, strength);
            this.addEffect(effect);
            return effect;
        }

        public void clear()
        {
            this.effects.Clear();
        }

        public void removeEffect(Effect effect)
        {
            foreach (Effect e in this.effects)
            {
                if (e.type == effect.type)
                {
                    this.effects.Remove(e);
                    return;
                }
            }
        }
    }

    Effects effects = new Effects();
    
    void updateEffects()
    {
        effects.clear();
        foreach (InvManager.Item item in Inventory.items)
        {
            if (item.name == "shield")
            {
                effects.addEffect(EFFECT_DEFENSE, 15);
            }
        }
    }
    void Start()
    {
        new Item("null");
        Inventory.addItem("ultratest");
        Inventory.addItem("daun");
        Inventory.printInventory();
        Inventory.removeItem("ultratest");
        Inventory.printInventory();
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
                        case EFFECT_DEFENSE:
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
        RaycastHit2D hit = Physics2D.Raycast(transform.position - new Vector3(0, 1, 0), Vector2.down, 0.2f);
        if (hit.collider != false && hit.distance < 0.05f)
        {
            rb.AddForce(new Vector2(0, jumpForce));
        }
        else
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
    void compareControls()
    {
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
            right = false;
        } else if (Input.GetKey(KeyCode.D))
        {
            movingSpeed.x = speed;
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
            movingSpeed.x = 0;
        }
    }
    void Update()
    {
        if (!controlsLocked)
        {
            compareControls();
        }
    }

    private void FixedUpdate()
    {
        if ((movingSpeed.x > 0 && rb.velocity.x < movingSpeed.x) || (movingSpeed.x < 0 && rb.velocity.x > movingSpeed.x))
        {
            rb.velocity = new Vector2(movingSpeed.x, rb.velocity.y);
        } else if (movingSpeed.x == 0)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
        }
    }
}
