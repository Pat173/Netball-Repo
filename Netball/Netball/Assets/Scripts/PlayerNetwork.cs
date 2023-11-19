using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Multiplayer.Netball.ClientAuthority;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlayerNetwork : NetworkBehaviour
{
    public enum PLAYER_STATE
    {
        InGame,
        LoosingHealth,
        OutOfGame
    }


    public NetworkVariable<PLAYER_STATE> playerState = new NetworkVariable<PLAYER_STATE>(PLAYER_STATE.InGame);
    
    public NetworkVariable<float> playerHealth = new NetworkVariable<float>(100, NetworkVariableReadPermission.Everyone,NetworkVariableWritePermission.Owner);
    
    public NetworkVariable<bool> playerCanRecall = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    
    private Vector2 mousePos;
    private Camera cam;
    private Rigidbody2D rb;
    private BoxCollider2D collider;
    [SerializeField] private GameObject ballPrefab;
    [SerializeField] private float damageRate;

    private GameObject spawnedBall;
    
    public GameObject graphics;
    public GameObject ballIndicator;
    public Transform shootPos;

    public override void OnNetworkSpawn()
    {
        cam = Camera.main;
        rb = GetComponent<Rigidbody2D>();
        collider = GetComponent<BoxCollider2D>();
    }

    private void Update()
    {
        if (playerState.Value == PLAYER_STATE.OutOfGame) return;
        if (!IsOwner) return;

        float moveSpeed = 3;

        if (Input.GetKey(KeyCode.W)) transform.Translate(Vector3.up * moveSpeed * Time.deltaTime);
        if (Input.GetKey(KeyCode.S)) transform.Translate(Vector3.down * moveSpeed * Time.deltaTime);
        if (Input.GetKey(KeyCode.D)) transform.Translate(Vector3.right * moveSpeed * Time.deltaTime);
        if (Input.GetKey(KeyCode.A)) transform.Translate(Vector3.left * moveSpeed * Time.deltaTime);

        if (Input.GetKeyDown(KeyCode.C)) playerHealth.Value -= 5;

        mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        Vector2 lookDir = mousePos - rb.position;
        float angle = Mathf.Atan2(lookDir.y,lookDir.x)* Mathf.Rad2Deg -90f;
        graphics.transform.rotation = Quaternion.Euler(0,0,angle);

        if (playerState.Value == PLAYER_STATE.LoosingHealth)
        {
            playerHealth.Value -= damageRate * Time.deltaTime;
            
            if (playerHealth.Value <= 0)
            {
                OnLostGameServerRpc();
                collider.enabled = false;
                graphics.SetActive(false);
            }
            
            if (Input.GetMouseButtonDown(0))
            {
                OnBallShootServerRpc(lookDir);
                
                ballIndicator.SetActive(false);
            }

            if (Input.GetMouseButtonDown(1) && playerCanRecall.Value)
            {
                OnBallRecallServerRpc();
                
                ballIndicator.SetActive(true);
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
    
    //*************
    //PICKUP BALL
    //*************

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
        playerState.Value = PLAYER_STATE.LoosingHealth;
        
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
        
        foreach (var client in NetworkManager.ConnectedClients)
        {
            client.Value.PlayerObject.GetComponent<PlayerNetwork>().playerState.Value = PLAYER_STATE.InGame;
        }
        
        playerState.Value = PLAYER_STATE.LoosingHealth;
        playerCanRecall.Value = false;
    }
    
    //*************
    //SHOOTING BALL
    //*************
    
    [ServerRpc (RequireOwnership = false)]
    public void OnBallShootServerRpc(Vector2 dir)
    {
        if (playerCanRecall.Value == false)
        {
            Shoot(dir);
        }
        OnBallShootClientRpc(dir);
    }
    
    [ClientRpc]
    public void OnBallShootClientRpc(Vector2 dir)
    {
        ballIndicator.SetActive(false);
    }

    void Shoot(Vector2 dir)
    {
        playerCanRecall.Value = true;
        Debug.Log("Shoot");
        spawnedBall = Instantiate(ballPrefab,shootPos.position,shootPos.rotation);

        NetworkObject networkObject = spawnedBall.GetComponent<NetworkObject>();
        networkObject.Spawn(true);
    }
    
    //*************
    //RECALL BALL
    //*************

    [ServerRpc (RequireOwnership = false)]
    public void OnBallRecallServerRpc()
    {
        playerCanRecall.Value = false;
        RecallBall();
        OnBallRecallClientRpc();
    }
    
    [ClientRpc]
    public void OnBallRecallClientRpc()
    {
        ballIndicator.SetActive(true);
    }
    
    void RecallBall()
    {
        if (spawnedBall == null) return;
        
        NetworkObject networkObject = spawnedBall.GetComponent<NetworkObject>();
        networkObject.Despawn(true);
        Destroy(spawnedBall);
    }
    
    //*************
    //DESTROY PLAYER
    //*************
    [ServerRpc (RequireOwnership = false)]
    public void OnLostGameServerRpc()
    {
        playerState.Value = PLAYER_STATE.OutOfGame;
        collider.enabled = false;
        graphics.SetActive(false);
        OnLostGameClientRpc();
       
    }

    [ClientRpc]
    public void OnLostGameClientRpc()
    {
        playerState.Value = PLAYER_STATE.OutOfGame;
        //this.gameObject.SetActive(false);
        collider.enabled = false;
        graphics.SetActive(false);
        Debug.Log(" Ded");
        ShootBallRandomlyOnPlayerDeath();
    }
    
    public void ShootBallRandomlyOnPlayerDeath()
    {
        GameObject randomSpawnedBallInstance = Instantiate(ballPrefab,shootPos.position,shootPos.rotation);

        NetworkObject networkObject = randomSpawnedBallInstance.GetComponent<NetworkObject>();
        networkObject.Spawn(true);
        
        randomSpawnedBallInstance.GetComponent<Rigidbody2D>().AddForce(new Vector2(Random.Range(-1f,1f),Random.Range(-1f,1f)).normalized * 20, ForceMode2D.Impulse);
    }
}
