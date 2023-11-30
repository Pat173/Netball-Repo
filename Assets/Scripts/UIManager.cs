using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.UI;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;

public class UIManager : MonoBehaviour
{
    const string pattern = @"^(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$";

    string ipInput;
    string usernameInput;

    public Transform playerListLobbyParent;
    public GameObject playerLobbyUIPrefab;
    public TMP_Text hostIpText;

    public Color disconnectedColor;
    public Color connectedColor;
    public Image connectionIndicator;
    public TMP_Text connectionText;

    List<UIPlayerName> playerLobbyUIs = new List<UIPlayerName>();

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

    public void StartHost(bool outbound = false)
    {
        if (NetworkManager.Singleton.IsHost)
        {

        }
        else
        {
            UnityTransport transport = NetworkManager.Singleton.gameObject.GetComponent<UnityTransport>();

            if (outbound)
            {
                transport.ConnectionData.Address = GetOutboundAddress();
                hostIpText.text = GetOutboundAddress();
            }
            else
            {
                transport.ConnectionData.Address = "127.0.0.1";
                hostIpText.text = GetLocalAddress();
            }

            NetworkManager.Singleton.StartHost();
            LayoutRebuilder.ForceRebuildLayoutImmediate(hostIpText.rectTransform);
        }
    }

    public void Update()
    {
        // add 1 prefab for each player in the player list   
        foreach (var player in PlayerList.Instance.Players)
        {
            // make sure each player is only added once
            if (!playerLobbyUIs.Exists(x => x.playerId == player.Value.playerId.Value))
            {
                GameObject playerLobbyUIInstance = Instantiate(playerLobbyUIPrefab);
                playerLobbyUIInstance.transform.SetParent(playerListLobbyParent);

                // get tmp text component
                UIPlayerName uiPlayerName = playerLobbyUIInstance.GetComponent<UIPlayerName>();
                uiPlayerName.playerId = player.Value.playerId.Value;

                playerLobbyUIInstance.transform.localScale = Vector3.one;

                playerLobbyUIs.Add(uiPlayerName);
            }
            player.Value.nameText.text = player.Value.playerName.Value.ToString();
        }

        // make sure if a player leaves, the ui element is removed
        foreach (var playerLobbyUI in playerLobbyUIs)
        {
            if (!PlayerList.Instance.Players.ContainsKey(playerLobbyUI.playerId))
            {
                playerLobbyUIs.Remove(playerLobbyUI);
                Destroy(playerLobbyUI.gameObject);
            }
        }

        // if connected, change color of connection indicator
        if (NetworkManager.Singleton.IsConnectedClient || NetworkManager.Singleton.IsHost)
        {
            connectionIndicator.color = connectedColor;
            connectionText.text = "Status: Connected";
        }
        else
        {
            connectionIndicator.color = disconnectedColor;
            connectionText.text = "Status: Disconnected";
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
        GUIUtility.systemCopyBuffer = GetOutboundAddress();
    }

    public static string GetOutboundAddress()
    {
        string externalIpString = new WebClient().DownloadString("http://ipv4.icanhazip.com").Replace("\\r\\n", "").Replace("\\n", "").Trim();
        var externalIp = IPAddress.Parse(externalIpString);

        return externalIp.ToString();
    }

    public static string GetLocalAddress()
    {
        foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
        {
            if (ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 || ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
            {
                foreach (UnicastIPAddressInformation ip in ni.GetIPProperties().UnicastAddresses)
                {
                    if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                    {
                        return ip.Address.ToString();
                    }
                }
            }
        }

        return "127.0.0.1";
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
        }
    }

    public void ShowUI(bool show)
    {
        gameObject.SetActive(show);
    }
}
