using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows;
using System.Net;
using System.Net.Sockets;

public class UIManager : MonoBehaviour
{
    const string pattern = @"^(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$";

    string ipInput;
    string usernameInput;

    public Transform playerListLobbyParent;
    public GameObject playerLobbyUIPrefab;
    public TMP_Text hostIpText;

    PlayerList playerList;
    List<PlayerNetwork> players = new List<PlayerNetwork>();

    public string IpInput
    {
        get => ipInput;
        set => ipInput = value;
    }

    public string UsernameInput
    {
        get => usernameInput;
        set => usernameInput = value;
    }

    public void StartHost()
    {
        if (NetworkManager.Singleton.IsHost)
        {

        }
        else
        {
            NetworkManager.Singleton.StartHost();

            hostIpText.text = GetLocalIPAddress();
            LayoutRebuilder.ForceRebuildLayoutImmediate(hostIpText.rectTransform);

            NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerNetwork>().playerName.Value = usernameInput;
        }
    }

    public void Update()
    {
        // add 1 prefab for each player in the player list
        if (playerList == null)
            playerList = FindObjectOfType<PlayerList>();
        else
        {
            foreach (var player in playerList.Players)
            {
                if (!players.Contains(player.Value))
                {
                    players.Add(player.Value);

                    GameObject playerLobbyUIInstance = Instantiate(playerLobbyUIPrefab);
                    playerLobbyUIInstance.transform.SetParent(playerListLobbyParent);

                    // get tmp text component
                    TMP_Text tmpText = playerLobbyUIInstance.GetComponent<TMP_Text>();
                    tmpText.text = player.Value.playerName.Value.ToString();

                    playerLobbyUIInstance.transform.localScale = Vector3.one;
                }
            }
        }
    }

    public void StartGame()
    {
        if (!NetworkManager.Singleton.IsServer) return;

        LobbyManager lobbyManager = FindObjectOfType<LobbyManager>();
        if (lobbyManager != null)
        {
            lobbyManager.StartGame();
        }
    }

    public void CopyIp()
    {
        GUIUtility.systemCopyBuffer = GetLocalIPAddress();
    }

    public static string GetLocalIPAddress()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                return ip.ToString();
            }
        }

        return "unknown";
    }

    public void ClientConnect()
    {
        if (NetworkManager.Singleton.IsClient)
        {

        }
        else
        {
            UnityTransport transport = NetworkManager.Singleton.gameObject.GetComponent<UnityTransport>();
            if (!Regex.IsMatch(ipInput, pattern))
            {
                Debug.Log("ip not valid");
                return;
            }

            transport.ConnectionData.Address = ipInput;
            NetworkManager.Singleton.StartClient();
            NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerNetwork>().playerName.Value = usernameInput;
        }
    }

    public void ShowUI(bool show)
    {
        gameObject.SetActive(show);
    }
}
