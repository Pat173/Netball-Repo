using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class Ball : NetworkBehaviour
{
    
    
    private CircleCollider2D ballCollider;

    public override void OnNetworkSpawn()
    {
        
        ballCollider = GetComponent<CircleCollider2D>();

        
    }

    private void Update()
    {
        transform.Translate(Vector3.up * Time.deltaTime* 2);
    }

    
}
