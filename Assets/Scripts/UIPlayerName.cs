using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

[RequireComponent(typeof(TMP_Text))]
public class UIPlayerName : MonoBehaviour
{
    [HideInInspector]
    public ulong playerId;

    void Update()
    {
        if (PlayerList.Instance.Players.ContainsKey(playerId))
        {
            PlayerNetwork player = PlayerList.Instance.Players[playerId];
            // if name doesnt match, update and rebuild layout
            if (player.playerName.Value != GetComponent<TMP_Text>().text)
            {
                GetComponent<TMP_Text>().text = player.playerName.Value.ToString();
                LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<TMP_Text>().rectTransform);
            }
        }
    }
}
