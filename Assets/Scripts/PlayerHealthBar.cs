using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthBar : NetworkBehaviour
{
    [SerializeField] Image healthbar;
    float maxHealth;
    float health;
    PlayerNetwork player;

    public override void OnNetworkSpawn()
    {
        player = GetComponent<PlayerNetwork>();
        maxHealth = player.playerHealth.Value;
        health = player.playerHealth.Value;
    }

    private void Update()
    {
        if (health != player.playerHealth.Value)
        {
            health = player.playerHealth.Value;
            float normalizedHealth = health / maxHealth;
            healthbar.fillAmount = normalizedHealth;
        }
    }
}
