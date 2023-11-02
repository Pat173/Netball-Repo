using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerNetwork : NetworkBehaviour
{
    public NetworkVariable<float> playerHealth = new NetworkVariable<float>(100);

    public NetworkVariable<bool> playerHasBall = new NetworkVariable<bool>(false);

    private void Update()
    {
        if (!IsOwner) return;

        float moveSpeed = 3;

        if (Input.GetKey(KeyCode.W)) transform.Translate(Vector3.up * moveSpeed * Time.deltaTime);
        if (Input.GetKey(KeyCode.S)) transform.Translate(Vector3.down * moveSpeed * Time.deltaTime);
        if (Input.GetKey(KeyCode.D)) transform.Translate(Vector3.right * moveSpeed * Time.deltaTime);
        if (Input.GetKey(KeyCode.A)) transform.Translate(Vector3.left * moveSpeed * Time.deltaTime);

        if(Input.GetKey(KeyCode.B)) playerHasBall.Value = true;

        if (Input.GetKey(KeyCode.C)) playerHealth.Value -= 5;

    }
}
