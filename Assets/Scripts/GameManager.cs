using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static GameManager instance;

    // Propiedades públicas para establecer os límites do taboleiros
    public int minX, maxX, minY, minZ, maxZ;

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

    public override void OnNetworkSpawn()
    {
        // Só o servidor pode otorgar premio ou castigo
        if (IsServer) { StartCoroutine(CoRandomBoonBane()); }
    }

    IEnumerator CoRandomBoonBane()
    {
        while (true)
        {
            // pasados 20 seguntos escóllese un netPlayer aleatoriamente
            yield return new WaitForSeconds(timeRandom);

            int rdmIndex = Random.Range(0, NetworkManager.Singleton.ConnectedClientsList.Count);
            ulong rdmUID = NetworkManager.Singleton.ConnectedClientsIds[rdmIndex];

            var rdmPlayerObject = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(rdmUID);
            var rdmPlayer = rdmPlayerObject.GetComponent<Player>();

            // Método ClientRpc do player des/afortunado
            bool isBoon = Random.Range(0, 2) == 0 ? true : false;
            rdmPlayer.SetBoonBaneClientRpc(isBoon);

            // Os cambios duran 10s
            yield return new WaitForSeconds(timePower);

            // O netPlayer recupera as propiedades de orixe
            rdmPlayer.ResetBoonBaneClientRpc();
        }

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