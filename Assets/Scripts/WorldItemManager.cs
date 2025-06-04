using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using InvManager;
using Debug = UnityEngine.Debug;
using Effects;

public class WorldItemManager : MonoBehaviour
{
    [SerializeField] private Player player;
    [SerializeField] private float rotateSpeed;
    [SerializeField] private float movingSmooth;
    
    public List<WorldItem> worldItems = new List<WorldItem>();

    public WorldItem worldItemDragging = null;

    private bool leftPressed = false;
    private int turn = 0;
    
    public WorldItem createItem(Vector2 worldPosition, string itemName, Vector2? size = null, List<int[]> effects = null, bool eatable = false, bool usable = false)
    {
        Item item = new Item(itemName, size, effects, eatable, usable);
        WorldItem worldItem = Instantiate(Resources.Load<WorldItem>("worldItems/" + itemName));
        worldItem.transform.position = worldPosition;
        worldItem.id = item.id;
        worldItem.itemName = item.name;
        worldItem.changeSize(item.size);
        worldItems.Add(worldItem);
        return worldItem;
    }

    public WorldItem createItem(Vector2 worldPosition, Item item)
    {
        WorldItem worldItem = Instantiate(Resources.Load<WorldItem>("worldItems/" + item.name));
        worldItem.transform.position = worldPosition;
        worldItem.id = item.id;
        worldItem.itemName = item.name;
        worldItem.changeSize(item.size);
        worldItems.Add(worldItem);
        return worldItem;
    }

    public void removeItem(uint id)
    {
        foreach (WorldItem worldItem in worldItems)
        {
            if (worldItem.id == id)
            {
                worldItems.Remove(worldItem);
                Destroy(worldItem.gameObject);
                Item.removeItem(id);
            }
        }
    }

    private Vector3 getMouseWorldPosition()
    {
        return UnityEngine.Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10f));
    }

    void FixedUpdate()
    {
        if (turn != 0 && worldItemDragging)
        {
            worldItemDragging.rb.freezeRotation = true;
            worldItemDragging.rb.rotation += rotateSpeed * turn;
        }
        if (leftPressed)
        {
            if (player.ui.state == 0)
            {
                if (!worldItemDragging)
                {
                    foreach (WorldItem worldItem in worldItems)
                    {
                        if (worldItem.touching)
                        {
                            worldItemDragging = worldItem;
                            worldItemDragging.rb.gravityScale = 0;
                            break;
                        }
                    }
                }

                if (worldItemDragging)
                {
                    Vector3 mouseWorldPosition = getMouseWorldPosition();
                    Vector3 delta = mouseWorldPosition - worldItemDragging.transform.position;

                    float coef = movingSmooth;
                    float distance = Vector3.Distance(worldItemDragging.transform.position, mouseWorldPosition) * 5;
                    
                    Vector3 calcForce = delta.normalized * distance;

                    if (delta.x > 0 && worldItemDragging.rb.velocity.x > 0)
                    {
                        calcForce.x -= worldItemDragging.rb.velocity.x * coef;
                    } else if (delta.x < 0 && worldItemDragging.rb.velocity.x < 0)
                    {
                        calcForce.x -= worldItemDragging.rb.velocity.x * coef;
                    }

                    if (delta.y > 0 && worldItemDragging.rb.velocity.y > 0)
                    {
                        calcForce.y -= worldItemDragging.rb.velocity.y * coef;
                    } else if (delta.y < 0 && worldItemDragging.rb.velocity.y < 0)
                    {
                        calcForce.y -= worldItemDragging.rb.velocity.y * coef;
                    }
                    
                    worldItemDragging.rb.AddForce(calcForce);
                }
            }
            else if (worldItemDragging)
            {
                worldItemDragging.rb.gravityScale = 2;
                worldItemDragging.rb.centerOfMass = Vector2.zero;
                worldItemDragging.rb.freezeRotation = false;
                worldItemDragging = null;
            }
        } else if (worldItemDragging)
        {
            worldItemDragging.rb.gravityScale = 2;
            worldItemDragging.rb.centerOfMass = Vector2.zero;
            worldItemDragging.rb.freezeRotation = false;
            worldItemDragging = null;
        }
    }
    
    void Update()
    {
        leftPressed = Input.GetMouseButton(0);

        if (Input.GetKey(KeyCode.E))
        {
            turn = -1;
        } else if (Input.GetKey(KeyCode.Q))
        {
            turn = 1;
        }
        else
        {
            turn = 0;
        }
        
        if (Input.GetKeyUp(KeyCode.R))
        {
            createItem(
                UnityEngine.Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y,
                    10f)), "apple", new Vector2(2, 2), new List<int[]>()
                {
                    new int[3] {Effect.EFFECT_DEFENSE, 10, 20}
                }, true);
        }
    }
}
