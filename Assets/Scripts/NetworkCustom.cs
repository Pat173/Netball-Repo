using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(NetworkProperties))]
public class NetworkCustom : NetworkManager
{
    public GameObject gameManagerPrefab;
    private GameObject gameManagerInstance;

    // hook OnServerStarted and OnServerStopped to spawn and destroy the game manager
    void Start()
    {
        base.OnServerStarted += hkServerStarted;
    }

    public void hkServerStarted()
    {
        // if not server, don't spawn the game manager
        if (!IsServer) return;

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
