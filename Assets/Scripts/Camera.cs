using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Unity.VisualScripting;

public class Camera : MonoBehaviour
{
    public bool followPlayer = true;
    [SerializeField] GameObject player;
    [SerializeField] float movingSpeed;
    void Start()
    {
        
    }
    private void FixedUpdate()
    {
        if (followPlayer)
        {
            transform.position = Vector3.Lerp(transform.position, new Vector3(player.transform.position.x, player.transform.position.y, transform.position.z), movingSpeed);
            //transform.DOMove(new Vector3(player.transform.position.x, player.transform.position.y, transform.position.z), movingSpeed);
        }
    }
}
