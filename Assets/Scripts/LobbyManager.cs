using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public enum GameState
{
    LOBBY,
    GAME,
    END
}

public class LobbyManager : NetworkBehaviour
{
    public Transform ball;

    NetworkVariable<GameState> gameState = new NetworkVariable<GameState>(GameState.LOBBY, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public GameState State { get => gameState.Value; set => gameState.Value = value; }

    public override void OnNetworkSpawn()
    {
        //PlayerList.Instance.PlayerAdd(OwnerClientId, GetComponent<PlayerNetwork>());
    }

    public void Update()
    {
        if (!IsOwner) return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (State == GameState.LOBBY)
                StartGame();
            else if (State == GameState.GAME)
                EndGame();
        }


        if (State == GameState.LOBBY)
        {

        }
        else if (State == GameState.GAME)
        {
            // Check if any player has the ball
            PlayerNetwork it = null;
            foreach (var player in PlayerList.Instance.Players)
            {
                if (player.Value.hasBall.Value)
                {
                    it = player.Value;
                    break;
                }
            }

            // If no player has the ball, spawn a new one
            if (it == null)
            {
                Transform spawnedBall = Instantiate(ball);
                spawnedBall.GetComponent<NetworkObject>().Spawn(true);

                // Add random force to the ball
                spawnedBall.GetComponent<Rigidbody2D>().AddForce(new Vector2(Random.Range(-5, 5), Random.Range(-5, 5)), ForceMode2D.Impulse);
            }

            if (PlayerList.Instance.Players.Count <= 1)
            {
                EndGame();
            }
        }
    }

    public void StartGame()
    {
        State = GameState.GAME;
        Debug.Log("Game started");
    }

    public void EndGame()
    {
        State = GameState.END;
        Debug.Log("Game ended");
    }
}
