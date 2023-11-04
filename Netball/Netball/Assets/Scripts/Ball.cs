using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class Ball : NetworkBehaviour
{
    public static Ball instance;

    public bool attachedToPlayer = false;

    [SerializeField] private SpriteRenderer ballGraphics;
    private Rigidbody2D ballRb;
    private CircleCollider2D ballCollider;
    private NetworkRigidbody2D ballNetworkRigidbody2D;
    public override void OnNetworkSpawn()
    {
        ballRb = GetComponent<Rigidbody2D>();
        ballCollider = GetComponent<CircleCollider2D>();
        ballNetworkRigidbody2D = GetComponent<NetworkRigidbody2D>();
        
        InitialBallMovement(new Vector2(-5,0),new Vector2(5,1));
    }
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);
    }

    

    private void OnTriggerEnter2D(Collider2D collision)
    {
        attachedToPlayer = true;
        ballGraphics.enabled = false;
        ballCollider.enabled = false;
        ballRb.simulated = false;
        ballNetworkRigidbody2D.enabled = false;
        ballRb.velocity = Vector2.zero;

        collision.GetComponent<PlayerNetwork>().OnBallPickUpClientRPC(); 
        
        Debug.Log(collision.GetComponent<NetworkObject>().OwnerClientId);
    }

    [ServerRpc]
    public void ShootBallServerRPC(Vector2 shootPos, Vector2 dir)
    {
        ShootBallClientRPC(shootPos,dir);
        transform.position = new Vector3(shootPos.x, shootPos.y, 0);
        attachedToPlayer = false;
        ballCollider.enabled = true;
        ballRb.simulated = true;
        ballNetworkRigidbody2D.enabled = true;
        ballGraphics.enabled = true;
        ballRb.AddForce(dir.normalized *5, ForceMode2D.Impulse);
        
    }
    
    [ClientRpc]
    public void ShootBallClientRPC(Vector2 shootPos, Vector2 dir)
    {
        transform.position = new Vector3(shootPos.x, shootPos.y, 0);
        attachedToPlayer = false;
        ballCollider.enabled = true;
        ballRb.simulated = true;
        ballNetworkRigidbody2D.enabled = true;
        ballGraphics.enabled = true;
        ballRb.AddForce(dir.normalized *5, ForceMode2D.Impulse);
        
    }

    private void InitialBallMovement(Vector2 startPos, Vector2 dir)
    {
        transform.position = new Vector3(startPos.x, startPos.y, 0);
        ballRb.AddForce(dir.normalized *5, ForceMode2D.Impulse);
    }
}
