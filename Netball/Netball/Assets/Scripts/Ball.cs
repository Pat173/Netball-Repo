using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Ball : NetworkBehaviour
{
    public static Ball instance;

    public bool attachedToPlayer = false;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        if (attachedToPlayer == false)
        {
            transform.Translate(Vector2.right * 5 * Time.deltaTime);
        }
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        attachedToPlayer = true;
        transform.parent = collision.transform;
        transform.localPosition = Vector2.zero;
       
        Debug.Log(collision.GetComponent<NetworkObject>().OwnerClientId);
    }
}
