using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public class PlayerNetwork : NetworkBehaviour
{
    public NetworkVariable<float> playerHealth = new NetworkVariable<float>(100, NetworkVariableReadPermission.Everyone,NetworkVariableWritePermission.Owner);

    public NetworkVariable<bool> playerHasBall = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private Vector2 mousePos;
    private Camera cam;
    private Rigidbody2D rb;
    [SerializeField] private GameObject ballPrefab;
    public GameObject graphics;
    public GameObject ballIndicator;
    public Transform shootPos;

    public override void OnNetworkSpawn()
    {
        cam = Camera.main;
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {

        if (!IsOwner) return;

        float moveSpeed = 3;

        if (Input.GetKey(KeyCode.W)) transform.Translate(Vector3.up * moveSpeed * Time.deltaTime);
        if (Input.GetKey(KeyCode.S)) transform.Translate(Vector3.down * moveSpeed * Time.deltaTime);
        if (Input.GetKey(KeyCode.D)) transform.Translate(Vector3.right * moveSpeed * Time.deltaTime);
        if (Input.GetKey(KeyCode.A)) transform.Translate(Vector3.left * moveSpeed * Time.deltaTime);

        if (Input.GetKey(KeyCode.C)) playerHealth.Value -= 5;

        mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        Vector2 lookDir = mousePos - rb.position;
        float angle = Mathf.Atan2(lookDir.y,lookDir.x)* Mathf.Rad2Deg -90f;
        graphics.transform.rotation = Quaternion.Euler(0,0,angle);

        if (playerHasBall.Value == true)
        {
            if (Input.GetMouseButtonDown(0))
            {
                OnBallShootServerRpc(lookDir);
                
                ballIndicator.SetActive(false);
                
        
                
            }
        }

    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (!IsOwner) return;

        if (col.GetComponent<Ball>() != null)
        {
            OnBallPickUpServerRpc();
            
            ballIndicator.SetActive(true);

            var ballNetworkObject = col.gameObject.GetComponent<NetworkObject>();
            OnDestroyBallServerRpc(new NetworkObjectReference(ballNetworkObject));
        }
    }

    [ServerRpc (RequireOwnership =false)]
    private void OnDestroyBallServerRpc(NetworkObjectReference ball)
    {
        if (ball.TryGet(out NetworkObject ballObject))
        {
            ballObject.Despawn(true);
            Destroy(ballObject.gameObject);
        }
    }

    [ServerRpc (RequireOwnership = false)]
    private void OnBallPickUpServerRpc()
    {
        playerHasBall.Value = true;
        PickUpBall();
        OnBallPickUpClientRpc();
    }

    [ClientRpc]
    private void OnBallPickUpClientRpc()
    {
        PickUpBall();
    }

    private void PickUpBall()
    {
        ballIndicator.SetActive(true);
        // playerHasBall.Value = true;
    }
    
    [ServerRpc (RequireOwnership = false)]
    public void OnBallShootServerRpc(Vector2 dir)
    {
        playerHasBall.Value = false;
        Shoot(dir);
        // ballIndicator.SetActive(false);
        // playerHasBall.Value = false;
        OnBallShootClientRpc(dir);
    }
    
    [ClientRpc]
    public void OnBallShootClientRpc(Vector2 dir)
    {
        ballIndicator.SetActive(false);
        // playerHasBall.Value = false;
    }

    void Shoot(Vector2 dir)
    {
        Debug.Log("Shoot");
        GameObject spawnedBall = Instantiate(ballPrefab,shootPos.position,shootPos.rotation);

        NetworkObject networkObject = spawnedBall.GetComponent<NetworkObject>();
        networkObject.Spawn(true);

        //spawnedBall.GetComponent<Rigidbody2D>().AddForce(dir*5,ForceMode2D.Impulse);

    }
}
