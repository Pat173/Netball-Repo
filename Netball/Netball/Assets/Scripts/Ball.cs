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

        InitialBallMovement(new Vector2(-5,0),new Vector2(5,1));
    }
    
    private void InitialBallMovement(Vector2 startPos, Vector2 dir)
    {
        transform.position = new Vector3(startPos.x, startPos.y, 0);
        ballRb.AddForce(dir.normalized *5, ForceMode2D.Impulse);
    }
}
