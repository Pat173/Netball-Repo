using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;
using Unity.VisualScripting;
using UnityEngine;

public class Ball : NetworkBehaviour
{
    private CircleCollider2D ballCollider;
    private Rigidbody2D ballRigidBody;

    public float bulletSpeed = 10;

    public override void OnNetworkSpawn()
    {
        ballCollider = GetComponent<CircleCollider2D>();
        ballRigidBody = GetComponent<Rigidbody2D>();

        float angleInDegrees = transform.eulerAngles.z + 90f; // Get the object's rotation in degrees
        float angleInRadians = angleInDegrees * Mathf.Deg2Rad;

        Vector2 direction = new Vector2(Mathf.Cos(angleInRadians), Mathf.Sin(angleInRadians));

        ballRigidBody.AddForce(direction * bulletSpeed, ForceMode2D.Impulse);
    }
}
