using UnityEngine;

public class NetworkProperties : MonoBehaviour
{
    /*
    super hacky way of adding properties to the network manager
    i think it has to do with NetworkCustom being inherited so custom fields are not serialized or don't show up in the inspector
    */

    public GameObject gameManagerPrefab;

    NetworkCustom networkManager;

    [ExecuteAlways]
    void Update()
    {
        // set the prefab references in the network manager
        if (networkManager == null)
            networkManager = FindObjectOfType<NetworkCustom>();
        else
        {
            networkManager.gameManagerPrefab = gameManagerPrefab;
        }
    }
}