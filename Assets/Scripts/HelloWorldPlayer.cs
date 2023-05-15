using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace HelloWorld
{
    public class HelloWorldPlayer : NetworkBehaviour
    {
        public NetworkVariable<Vector3> Position = new NetworkVariable<Vector3>();

        // Variable de rede para gardar o índice de color de cada net player
        public NetworkVariable<int> PlayerColor;

        // Lista de colores
        [SerializeField]
        List<Material> playerColors;

        // Lista de colores dispoñibles
        List<int> playerColorsFree;

        MeshRenderer mr;

        // Método de instanciado de players
        // Orden de execución: Awake do Server, OnNetworkSpanw, Awake do Client, Start ...
        public override void OnNetworkSpawn()
        {
            InitValues();

            if (IsOwner)
            {
                Move();
                ChangeColor();
            }
        }

        void Start() { }

        void InitValues()
        {
            // Dev
            var ngo = GetComponent<NetworkObject>();
            string uid = ngo.NetworkObjectId.ToString();

            if (ngo.IsOwnedByServer)
            {
                gameObject.name = $"HostPlayer_{uid}";
            }
            else if (ngo.IsOwner)
            {
                gameObject.name = $"LocalPlayer_{uid}";
            }
            else
            {
                gameObject.name = "Net_Player_" + uid;
            }

            Debug.Log($"{gameObject.name}.HelloWorldPlayer");
            Debug.Log($"\t IsLocalPlayer: {ngo.IsLocalPlayer}");
            Debug.Log($"\t IsOwner: {ngo.IsOwner}");
            Debug.Log($"\t IsOwnedByServer: {ngo.IsOwnedByServer}");
            // --- end Dev

            mr = GetComponent<MeshRenderer>();

            playerColorsFree = new List<int>();

            for (int i = 0; i < playerColors.Count; i++)
            {
                playerColorsFree.Add(i);
            }
        }

        public void ChangeColor()
        {
            Debug.Log($"{gameObject.name}.HelloWorldPlayer.ChangeColor");

            if (NetworkManager.Singleton.IsServer)
            {
                SetRandomColor();
            }
            else
            {
                SubmitPlayerColorServerRpc();
            }

            Debug.Log($"\t PlayerColor: {PlayerColor.Value}");
        }

        [ServerRpc]
        void SubmitPlayerColorServerRpc(ServerRpcParams rpcParams = default)
        {
            SetRandomColor();
        }

        void SetRandomColor()
        {
            Debug.Log($"{gameObject.name}.HelloWorldPlayer.SetRandomColor");

            // Eliminando colores existentes da lista de índices de colores libres
            int takenColor = 0;

            // Percorrendo os Players conectados
            foreach (ulong uid in NetworkManager.Singleton.ConnectedClientsIds)
            {
                // Capturando o índice de color asignado
                takenColor = NetworkManager.Singleton.SpawnManager
                    .GetPlayerNetworkObject(uid)
                    .GetComponent<HelloWorldPlayer>()
                    .PlayerColor.Value;

                // Eliminando color asignado da lista de cores libres
                playerColorsFree.Remove(takenColor);

                Debug.Log($"\t Eliminado color: {takenColor} como disponible");
            }

            // Índice de cor aleatorio da lista de disponibles
            int rdmColor = Random.Range(0, playerColorsFree.Count);

            // Agregar a cor anterior á lista de cores libres
            playerColorsFree.Add(PlayerColor.Value);

            // Asignar a nova cor
            PlayerColor.Value = playerColorsFree[rdmColor];
        }

        public void Move()
        {
            if (NetworkManager.Singleton.IsServer)
            {
                Debug.Log($"{gameObject.name}.HelloWorldPlayer.Move in Server");

                Position.Value = GetRandomPositionOnPlane();
            }

            else
            {
                SubmitPositionRequestServerRpc();
            }
        }

        [ServerRpc]
        void SubmitPositionRequestServerRpc(ServerRpcParams rpcParams = default)
        {
            Debug.Log($"{gameObject.name}.HelloWorldPlayer.Move in ServerRpc");

            Position.Value = GetRandomPositionOnPlane();
        }

        static Vector3 GetRandomPositionOnPlane()
        {
            return new Vector3(Random.Range(-3f, 3f), 1f, Random.Range(-3f, 3f));
        }

        void Update()
        {
            if (transform.position != Position.Value)
            {
                Debug.Log($"{gameObject.name}.HelloWorldPlayer.Update transform.position");
                transform.position = Position.Value;
            }

            if (mr.material != playerColors[PlayerColor.Value])
            {
                // Debug.Log($"{gameObject.name}.HelloWorldPlayer.Update");
                // Debug.Log($"\t cambiamos a color: {PlayerColor.Value}");

                mr.material = playerColors[PlayerColor.Value];
            }
        }
    }
}
