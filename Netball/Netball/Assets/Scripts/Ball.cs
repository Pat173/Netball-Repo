using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class Ball : NetworkBehaviour
{

    [SerializeField] LayerMask obstacle;
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

    private void OnTriggerEnter2D(Collider2D other)
    {
        // if (other.gameObject.layer != obstacle) return;

        Vector2 normal = Vector2.zero;
        ContactPoint2D[] contacts = new ContactPoint2D[1];
        if (other.GetContacts(contacts) == 1)
        {
            normal = contacts[0].normal;
            // Use the 'normal' vector for further calculations
        }

        Vector2 incidentDirection = ballRigidBody.velocity.normalized;
        Vector2 reflectionDirection = Vector2.Reflect(incidentDirection, normal);


    }

}
