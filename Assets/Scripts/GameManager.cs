using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static GameManager instance;

    public int minX, maxX, minY, minZ, maxZ;

    List<NetworkClient> netPlayers;

    float timeRandom, timePower;

    void OnEnable()
    {
        if (instance != null && instance != this)
        {
            Destroy(this);
        }
        instance = this;

        // Medidas do taboleiro
        minX = -4;
        maxX = 4;
        minY = 1;
        minZ = 4;
        maxZ = -4;

        // Tempos de spaneo e des/vantaxes
        timeRandom = 20f;
        timePower = 10f;
    }




    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 300, 300));
        if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
        {
            StartButtons();
        }
        else
        {
            StatusLabels();
        }

        GUILayout.EndArea();
    }

    static void StartButtons()
    {
        if (GUILayout.Button("Host")) NetworkManager.Singleton.StartHost();
        if (GUILayout.Button("Client")) NetworkManager.Singleton.StartClient();
        if (GUILayout.Button("Server")) NetworkManager.Singleton.StartServer();
    }

    static void StatusLabels()
    {
        var mode = NetworkManager.Singleton.IsHost ?
            "Host" : NetworkManager.Singleton.IsServer ? "Server" : "Client";

        GUILayout.Label("Transport: " +
            NetworkManager.Singleton.NetworkConfig.NetworkTransport.GetType().Name);
        GUILayout.Label("Mode: " + mode);
    }
}