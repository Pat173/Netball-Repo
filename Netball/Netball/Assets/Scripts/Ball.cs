using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class Ball : NetworkBehaviour
{
    
    private Rigidbody2D ballRb;
    private CircleCollider2D ballCollider;

    public override void OnNetworkSpawn()
    {
        ballRb = GetComponent<Rigidbody2D>();
        ballCollider = GetComponent<CircleCollider2D>();

        
    }

    private void Update()
    {
        transform.Translate(Vector3.up * Time.deltaTime* 2);
    }

    public void InitialBallMovement(Vector2 startPos, Vector2 dir)
    {
        transform.position = new Vector3(startPos.x, startPos.y, 0);
        ballRb.AddForce(dir.normalized *5, ForceMode2D.Impulse);
    }
}
