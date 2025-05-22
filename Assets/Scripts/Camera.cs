using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Camera : MonoBehaviour
{
    public bool followPlayer = true;
    [SerializeField] GameObject player;
    [SerializeField] float movingDistance;
    
    void Start()
    {
        
    }

    void Update()
    {
        if (followPlayer)
        {
            transform.DOMove(new Vector3(player.transform.position.x, player.transform.position.y, transform.position.z), 1f);
        }
    }
}
