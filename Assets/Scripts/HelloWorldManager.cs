using Unity.Netcode;
using UnityEngine;

namespace HelloWorld
{
    public class HelloWorldManager : MonoBehaviour
    {
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
                SubmitNewPosition();
                SubmitNewColor();
            }

            GUILayout.EndArea();
        }

        static void StartButtons()
        {
            if (GUILayout.Button("Host"))
                NetworkManager.Singleton.StartHost();
            if (GUILayout.Button("Client"))
                NetworkManager.Singleton.StartClient();
            if (GUILayout.Button("Server"))
                NetworkManager.Singleton.StartServer();
        }

        static void StatusLabels()
        {
            var mode = NetworkManager.Singleton.IsHost
                ? "Host"
                : NetworkManager.Singleton.IsServer
                    ? "Server"
                    : "Client";

            GUILayout.Label(
                "Transport: "
                    + NetworkManager.Singleton.NetworkConfig.NetworkTransport.GetType().Name
            );
            GUILayout.Label("Mode: " + mode);
        }

        static void SubmitNewPosition()
        {
            if (
                GUILayout.Button(
                    NetworkManager.Singleton.IsServer ? "Move" : "Request Position Change"
                )
            )
            {
                if (NetworkManager.Singleton.IsServer && !NetworkManager.Singleton.IsClient)
                {
                    // Debug.Log($"HelloWorldManager.SubmitNewPosition IsServer");
                    foreach (ulong uid in NetworkManager.Singleton.ConnectedClientsIds)
                    {
                        NetworkManager.Singleton.SpawnManager
                            .GetPlayerNetworkObject(uid)
                            .GetComponent<HelloWorldPlayer>()
                            .Move();
                    }
                }
                else
                {
                    // Debug.Log($"HelloWorldManager.SubmitNewPosition IsClient");
                    var playerObject = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();
                    var player = playerObject.GetComponent<HelloWorldPlayer>();
                    player.Move();
                }
            }
        }

        static void SubmitNewColor()
        {
            if (
                GUILayout.Button(
                    NetworkManager.Singleton.IsServer ? "Change Color" : "Request Color Change"
                )
            )
            {
                if (NetworkManager.Singleton.IsServer && !NetworkManager.Singleton.IsClient)
                {
                    // Debug.Log($"HelloWorldManager.SubmitNewColor IsServer");
                    foreach (ulong uid in NetworkManager.Singleton.ConnectedClientsIds)
                    {
                        NetworkManager.Singleton.SpawnManager
                            .GetPlayerNetworkObject(uid)
                            .GetComponent<HelloWorldPlayer>()
                            .ChangeColor();
                    }
                }
                else
                {
                    // Debug.Log($"HelloWorldManager.SubmitNewColor IsClient");
                    var playerObject = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();
                    var player = playerObject.GetComponent<HelloWorldPlayer>();
                    player.ChangeColor();
                }
            }
        }
    }
}
