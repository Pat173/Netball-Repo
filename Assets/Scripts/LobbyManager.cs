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
    public Transform ball;

    NetworkVariable<EGameState> gameState = new NetworkVariable<EGameState>(EGameState.LOBBY, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public EGameState State { get => gameState.Value; set => gameState.Value = value; }

    public override void OnNetworkSpawn()
    {

    }

    public void Update()
    {
        if (!NetworkManager.Singleton.IsServer) return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (State == EGameState.LOBBY)
                StartGame();
            else if (State == EGameState.GAME)
                EndGame();
        }

        if (State == EGameState.LOBBY)
        {

        }
        else if (State == EGameState.GAME)
        {
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
                spawnedBall.GetComponent<NetworkObject>().Spawn(true);
                spawnedBall.GetComponent<Rigidbody2D>().AddForce(new Vector2(Random.Range(-5, 5), Random.Range(-5, 5)), ForceMode2D.Impulse);
            }
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
