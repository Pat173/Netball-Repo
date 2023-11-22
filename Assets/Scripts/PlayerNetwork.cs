using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Multiplayer.Netball.ClientAuthority;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlayerNetwork : NetworkBehaviour
{
    public enum EPlayerState
    {
        Alive,
        It,
        Dead
    }

    public NetworkVariable<ulong> playerId = new NetworkVariable<ulong>(0);
    public NetworkVariable<FixedString32Bytes> playerName = new NetworkVariable<FixedString32Bytes>("Test", NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<EPlayerState> playerState = new NetworkVariable<EPlayerState>(EPlayerState.Alive);
    public NetworkVariable<float> playerHealth = new NetworkVariable<float>(200);
    public NetworkVariable<bool> hasBall = new NetworkVariable<bool>(false);

    private Vector2 mousePos;
    private Camera cam;
    private Rigidbody2D rb;
    [SerializeField] private GameObject ballPrefab;
    [SerializeField] public float damageRate;

    private GameObject spawnedBall;

    public GameObject graphics;
    public GameObject ballIndicator;
    public Transform shootPos;
    public UIManager uiManager;

    public SpriteRenderer graphicColor;
    public List<Color> colors = new List<Color>();

    public override void OnNetworkSpawn()
    {
        cam = Camera.main;
        rb = GetComponent<Rigidbody2D>();

        playerId.Value = OwnerClientId;
        PlayerList.Instance.PlayerAdd(playerId.Value, this);

        if (IsOwner)
        {
            uiManager = FindObjectOfType<UIManager>();
            playerName.Value = uiManager.UsernameInput;
        }
        
        InıtPlayerPosServerRpc();
        graphicColor.color = colors[(int)OwnerClientId];
    }

    [ServerRpc (RequireOwnership = false)]
    void InıtPlayerPosServerRpc()
    {
        transform.position = new Vector2(Random.Range(-5, 5), -3f);
    }

    private void Update()
    {
        if (!IsOwner) return;
        if(playerState.Value == EPlayerState.Dead) return;
        float moveSpeed = 3;

        if (Input.GetKey(KeyCode.W)) transform.Translate(Vector3.up * moveSpeed * Time.deltaTime);
        if (Input.GetKey(KeyCode.S)) transform.Translate(Vector3.down * moveSpeed * Time.deltaTime);
        if (Input.GetKey(KeyCode.D)) transform.Translate(Vector3.right * moveSpeed * Time.deltaTime);
        if (Input.GetKey(KeyCode.A)) transform.Translate(Vector3.left * moveSpeed * Time.deltaTime);

        if (Input.GetKeyDown(KeyCode.C)) playerHealth.Value -= 5;

        mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        Vector2 lookDir = mousePos - rb.position;
        float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg - 90f;
        graphics.transform.rotation = Quaternion.Euler(0, 0, angle);

        if (playerState.Value == EPlayerState.It)
        {
            if (Input.GetMouseButtonDown(0))
            {
                OnBallShootServerRpc(lookDir);
            }

            if (Input.GetMouseButtonDown(1) && hasBall.Value)
            {
                OnBallRecallServerRpc();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (!IsOwner) return;

        if (col.GetComponent<Ball>() != null)
        {
            OnBallPickUpServerRpc();

            var ballNetworkObject = col.gameObject.GetComponent<NetworkObject>();
            var pointer = new NetworkObjectReference(ballNetworkObject);
            OnDestroyBallServerRpc(pointer);
            OnDestroyBallClientRpc(pointer);
        }
    }

    //*************
    //PICKUP BALL
    //*************

    [ServerRpc(RequireOwnership = false)]
    private void OnDestroyBallServerRpc(NetworkObjectReference ball)
    {
        if (ball.TryGet(out NetworkObject ballObject))
        {
            ballObject.Despawn(true);
            Destroy(ballObject.gameObject);
        }
    }

    [ClientRpc]
    private void OnDestroyBallClientRpc(NetworkObjectReference ball)
    {
        if (ball.TryGet(out NetworkObject ballObject))
        {
            ballObject.Despawn(true);
            Destroy(ballObject.gameObject);
        }
    }

    [ServerRpc(RequireOwnership = true)]
    private void OnBallPickUpServerRpc()
    {
        hasBall.Value = false;
        playerState.Value = EPlayerState.It;

        foreach (var player in PlayerList.Instance.Players)
        {
            if (player.Value.playerId.Value == playerId.Value) continue;

            player.Value.hasBall.Value = false;

            var health = player.Value.playerHealth.Value;
            player.Value.playerState.Value = health > 0 ? EPlayerState.Alive : EPlayerState.Dead;
        }

        OnBallPickUpClientRpc();
    }

    [ClientRpc]
    private void OnBallPickUpClientRpc()
    {
        ballIndicator.SetActive(true);
    }

    //*************
    //SHOOTING BALL
    //*************

    [ServerRpc(RequireOwnership = false)]
    public void OnBallShootServerRpc(Vector2 dir)
    {
        if (hasBall.Value == false)
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
        hasBall.Value = true;
        Debug.Log("Shoot");
        spawnedBall = Instantiate(ballPrefab, shootPos.position, shootPos.rotation);

        NetworkObject networkObject = spawnedBall.GetComponent<NetworkObject>();
        networkObject.Spawn(true);
    }

    //*************
    //RECALL BALL
    //*************

    [ServerRpc(RequireOwnership = false)]
    public void OnBallRecallServerRpc()
    {
        hasBall.Value = false;
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

    //DESTROY PLAYER

    [ServerRpc(RequireOwnership = true)]
    public void OnLostGameServerRpc()
    {
        this.gameObject.SetActive(false);
    }

    [ClientRpc]
    public void OnLostGameClientRpc()
    {
        this.gameObject.SetActive(false);
    }

    public void KillPlayer()
    {
        hasBall.Value = false;
        playerHealth.Value = 0;
        playerState.Value = EPlayerState.Dead;
    }

    void HidePlayer(bool hide = true)
    {
        graphics.SetActive(!hide);
        GetComponent<Collider2D>().enabled = !hide;
        GetComponent<Rigidbody2D>().simulated = !hide;
    }

    [ClientRpc]
    public void KillPlayerClientRpc()
    {
        HidePlayer();
        ballIndicator.SetActive(false);
    }

    [ServerRpc(RequireOwnership = false)]
    public void KillPlayerServerRpc()
    {
        HidePlayer();
        ballIndicator.SetActive(false);
    }

    public void RespawnPlayer()
    {
        hasBall.Value = false;
        playerHealth.Value = 200;
        playerState.Value = EPlayerState.Alive;
    }

    [ClientRpc]
    public void RespawnPlayerClientRpc()
    {
        HidePlayer(false);
    }

    [ServerRpc(RequireOwnership = false)]
    public void RespawnPlayerServerRpc()
    {
        HidePlayer(false);
    }
}
