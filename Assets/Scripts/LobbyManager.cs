using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public enum EGameState
{
    LOBBY,
    GAME,
    END
}

public class LobbyManager : NetworkBehaviour
{
    UIManager uiManager;
    public Transform ball;

    NetworkVariable<EGameState> gameState = new NetworkVariable<EGameState>(EGameState.LOBBY, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public EGameState State { get => gameState.Value; set => gameState.Value = value; }

    void Start()
    {
        uiManager = FindObjectOfType<UIManager>();
    }

    public override void OnNetworkSpawn()
    {

    }

    public void Update()
    {
        if (State == EGameState.GAME && NetworkManager.Singleton.IsServer)
        {
            uiManager.ShowUI(false);

            // Check if any player has the ball
            PlayerNetwork it = null;
            List<PlayerNetwork> alivePlayers = new List<PlayerNetwork>();
            List<PlayerNetwork> deadPlayers = new List<PlayerNetwork>();

            foreach (var player in PlayerList.Instance.Players)
            {
                if (player.Value.playerState.Value == PlayerNetwork.EPlayerState.Alive || player.Value.playerState.Value == PlayerNetwork.EPlayerState.It)
                    alivePlayers.Add(player.Value);
                else if (player.Value.playerState.Value == PlayerNetwork.EPlayerState.Dead)
                    deadPlayers.Add(player.Value);

                if (player.Value.playerState.Value == PlayerNetwork.EPlayerState.It)
                    it = player.Value;

                if (player.Value == it)
                {
                    player.Value.playerHealth.Value -= player.Value.damageRate * Time.deltaTime;
                }

                if (player.Value.playerHealth.Value <= 0)
                {
                    player.Value.KillPlayer();
                    player.Value.KillPlayerServerRpc();
                    player.Value.KillPlayerClientRpc();
                }
            }

            if (alivePlayers.Count <= 1)
            {
                EndGame();
            }
            else if (it == null && GameObject.FindWithTag("Ball") == null)
            {
                Transform spawnedBall = Instantiate(ball);

                Ball ballScript = spawnedBall.GetComponent<Ball>();
                ballScript.noFriction = true;

                spawnedBall.GetComponent<NetworkObject>().Spawn(true);
                spawnedBall.GetComponent<Rigidbody2D>().AddForce(new Vector2(Random.Range(-5, 5), Random.Range(-5, 5)), ForceMode2D.Impulse);
            }
        }
        else if (State == EGameState.END && NetworkManager.Singleton.IsServer)
        {
            foreach (var player in PlayerList.Instance.Players)
            {
                player.Value.RespawnPlayer();
                player.Value.RespawnPlayerServerRpc();
                player.Value.RespawnPlayerClientRpc();
            }
        }

        if (State == EGameState.LOBBY)
        {
            uiManager.ShowUI(true);
        }

        if (State == EGameState.GAME)
        {
            uiManager.ShowUI(false);
        }

        if (State == EGameState.END)
        {
            uiManager.ShowUI(true);
        }
    }

    public void StartGame()
    {
        State = EGameState.GAME;
        Debug.Log("Game started");
    }

    public void EndGame()
    {
        State = EGameState.END;
        Debug.Log("Game ended");
    }
}
