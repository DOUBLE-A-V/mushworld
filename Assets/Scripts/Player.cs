using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private float jumpForce;

    private bool pressedJump = false;
    public Vector2 movingSpeed;
    public bool controlsLocked = false;
    void Start()
    {
        
    }
    
    

    void jump()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position - new Vector3(0, 1, 0), Vector2.down, 0.2f);
        if (hit.collider != false && hit.distance < 0.05f)
        {
            rb.AddForce(new Vector2(0, jumpForce));
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
        } else if (Input.GetKey(KeyCode.D))
        {
            movingSpeed.x = speed;
        }
        else
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
            rb.AddForce(movingSpeed);
        }
    }
}
