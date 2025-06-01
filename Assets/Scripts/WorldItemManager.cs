using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InvManager;

public class WorldItemManager : MonoBehaviour
{
    [SerializeField] private Player player;
    
    private List<WorldItem> worldItems = new List<WorldItem>();
    
    public WorldItem createItem(Vector2 worldPosition, string itemName, Vector2? size = null, List<int[]> effects = null, bool eatable = false)
    {
        Item item = new Item(itemName, size, effects, eatable);
        WorldItem worldItem = Instantiate(Resources.Load<WorldItem>("worldItems/" + itemName));
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

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.R))
        {
            createItem(
                UnityEngine.Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y,
                    10f)), "apple", new Vector2(2, 10));
        }
    }
}
