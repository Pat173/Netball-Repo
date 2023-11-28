using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

using TMPro;

[RequireComponent(typeof(NetworkProperties))]
public class NetworkCustom : NetworkManager
{
    public GameObject gameManagerPrefab;
    public GameObject playerLobbyUIPrefab;

    private GameObject gameManagerInstance;
    private GameObject playerLobbyUIInstance;

    // hook OnServerStarted and OnServerStopped to spawn and destroy the game manager
    void Start()
    {
        base.OnServerStarted += hkServerStarted;
    }

    public void hkServerStarted()
    {
        // if not server, don't spawn the game manager
        // if (!IsServer) return;

        gameManagerInstance = Instantiate(gameManagerPrefab);
        NetworkObject networkObject = gameManagerInstance.GetComponent<NetworkObject>();
        networkObject.Spawn();
    }

    public void hkServerStopped()
    {
        if (gameManagerInstance != null)
        {
            Destroy(gameManagerInstance);
        }
    }
}
