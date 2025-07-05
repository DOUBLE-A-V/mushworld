using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float damage = 0;
    public float speed = 0;
    private bool throwed = false;
    
    private Vector2 from = Vector2.zero;
    private Vector2 to = Vector2.zero;
    
    
    public void throwMe(Vector2 fromPos, Vector2 toPos, float throwSpeed)
    {
        transform.localScale = new Vector3(3, 3, 1);
        transform.position = fromPos;
        speed = throwSpeed; 
        from = fromPos;
        to = toPos;
        throwed = true;
        
        Vector2 direction = (new Vector3(toPos.x, toPos.y) - transform.position).normalized;
        transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "npc")
        {
            NPC npc = collision.gameObject.GetComponent<NPC>();
            if (npc.enemy)
            {
                npc.doDamage(damage);
            }
        } else if (collision.gameObject.tag == "player")
        {
            Player player = collision.gameObject.GetComponent<Player>();
            player.doDamage(0, damage);
        }

        Destroy(gameObject);
    }
    
    void FixedUpdate()
    {
        if (throwed)
        {
            Vector2 add = (to - from).normalized * speed;
            transform.position += new Vector3(add.x, add.y, 0);
            if (Vector2.Distance(Vector2.zero, transform.position) > 30)
            {
                Destroy(gameObject);
            }
        }
    }
}
