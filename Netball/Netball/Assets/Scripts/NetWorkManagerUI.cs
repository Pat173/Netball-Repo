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

public class NetWorkManagerUI : MonoBehaviour
{
    const string pattern = @"^(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$";

    [SerializeField] private Transform ball;

    [SerializeField] private Button serverButton;
    [SerializeField] private Button hostButton;
    [SerializeField] private Button clientButton;
    [SerializeField] private TMP_InputField adressField;
    [SerializeField] private InputField portField;

    private void Awake()
    {
        serverButton.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartServer();
        });
        hostButton.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartHost();
        });
        clientButton.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartClient();
        });
        adressField.onSubmit.AddListener((string myInput) =>
        {
            UnityTransport test = NetworkManager.Singleton.gameObject.GetComponent<UnityTransport>();

            
            if(Regex.IsMatch (myInput, pattern))
            {
                test.ConnectionData.Address = myInput;
            }
            else
            {
                Debug.Log("ip not valid");
            }
        });
        
    }

    private void Update()
    {
        if (UnityEngine.Input.GetKeyDown(KeyCode.T))
        {
            Transform spawnedBall = Instantiate(ball);
            spawnedBall.GetComponent<NetworkObject>().Spawn(true);
        }

    }
}
