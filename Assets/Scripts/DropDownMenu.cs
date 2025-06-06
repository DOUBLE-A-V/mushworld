using System;
using System.Collections;
using System.Collections.Generic;
using InvManager;
using UnityEngine;

public class DropDownMenu : MonoBehaviour
{
    [SerializeField] private Player player;
    [SerializeField] private GameObject button2;
    [SerializeField] private UI ui;
    [SerializeField] private ItemsUser itemsUser;

    private bool touching = false;
    
    private bool showed = false;

    private Item currentItem = null;
    
    public void useCurrentItem()
    {
        
        if (currentItem.eatable)
        {
            ui.eatItem(currentItem);
        } else if (currentItem.usable)
        {
            itemsUser.useItem(currentItem);
        }
        clear();
    }

    public void throwOutCurrentItem()
    {
        foreach (ItemObject item in player.ui.items)
        {
            if (item.itemID == currentItem.id)
            {
                ui.throwOutItem(item);
                clear();
                return;
            }
        }
        
        foreach (ItemObject item in player.ui.subItems)
        {
            if (item.itemID == currentItem.id)
            {
                ui.throwOutItem(item);
                clear();
                break;
            }
        }
    }

    public void clear()
    {
        currentItem = null;
        gameObject.SetActive(false);
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
        if (Input.GetMouseButtonUp(0) && !touching)
        {
            clear();
        }   
    }
    public void setCurrentItem(ItemObject item)
    {
        transform.position = Input.mousePosition;
        transform.SetSiblingIndex(-1);
        gameObject.SetActive(true);
        currentItem = Item.getItem(item.itemID);
        if (currentItem.eatable || currentItem.usable)
        {
            button2.SetActive(true);
        }
        else
        {
            button2.SetActive(false);
        }
    }
}
