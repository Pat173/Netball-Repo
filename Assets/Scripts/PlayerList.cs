using UnityEngine;
using System.Collections.Generic;

public class PlayerList : Singleton<PlayerList>
{
    public Dictionary<ulong, PlayerNetwork> Players = new Dictionary<ulong, PlayerNetwork>();

    public void PlayerAdd(ulong id, PlayerNetwork player)
    {
        Players.Add(id, player);
        Debug.Log($"Player added: {id}");
    }

    public void PlayerRemove(ulong id)
    {
        Players.Remove(id);
        Debug.Log($"Player removed: {id}");
    }

    public PlayerNetwork GetLocalPlayer()
    {
        foreach (var player in Players)
        {
            if (player.Value.IsLocalPlayer)
            {
                return player.Value;
            }
        }
        return null;
    }
}
