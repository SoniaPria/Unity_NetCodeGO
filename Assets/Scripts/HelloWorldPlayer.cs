using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace HelloWorld
{
    public class HelloWorldPlayer : NetworkBehaviour
    {
        public NetworkVariable<Vector3> Position = new NetworkVariable<Vector3>();

        // Variable de red para guardar color de cada player
        public NetworkVariable<int> PlayerColor;

        [SerializeField]
        List<Material> playerColors;

        List<int> playerColorsFree;

        //int rdmColor;

        MeshRenderer mr;

        void Start()
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

        // Método de instanciado de players
        public override void OnNetworkSpawn()
        {
            if (IsOwner)
            {
                Move();
                ChangeColor();
            }
        }

        public void ChangeColor()
        {
            if (NetworkManager.Singleton.IsServer)
            {
                Debug.Log($"{gameObject.name}.HelloWorldPlayer.ChangeColor");

                // Asignación de color no PlayerHost
                int rdmColor = GetRandomColor();
                PlayerColor.Value = playerColorsFree[rdmColor];
                Debug.Log($"\t PlayerColor: {PlayerColor.Value}");
            }
            else
            {
                SubmitPlayerColorServerRpc();
            }
        }

        [ServerRpc]
        void SubmitPlayerColorServerRpc(ServerRpcParams rpcParams = default)
        {
            Debug.Log($"{gameObject.name}.HelloWorldPlayer.SubmitPlayerColorServerRpc");

            int rdmColor = GetRandomColor();
            PlayerColor.Value = playerColorsFree[rdmColor];

            Debug.Log($"\t PlayerColor: {PlayerColor.Value}");
        }

        int GetRandomColor()
        {
            // Eliminando colores existentes da lista de índices de colores libres
            int takenColor = 0;
            foreach (ulong uid in NetworkManager.Singleton.ConnectedClientsIds)
            {
                takenColor = NetworkManager.Singleton.SpawnManager
                    .GetPlayerNetworkObject(uid)
                    .GetComponent<HelloWorldPlayer>()
                    .PlayerColor.Value;

                playerColorsFree.Remove(takenColor);

                Debug.Log($"\t Eliminado color: {takenColor} como disponible");
            }

            // Color aleatorio da lista de disponibles
            int rdmColor = Random.Range(0, playerColorsFree.Count);

            return rdmColor;
        }

        public void Move()
        {
            if (NetworkManager.Singleton.IsServer)
            {
                var randomPosition = GetRandomPositionOnPlane();
                transform.position = randomPosition;
                Position.Value = randomPosition;

                Debug.Log($"HelloWorldPlayer.Move");
            }
            else
            {
                SubmitPositionRequestServerRpc();
            }
        }

        [ServerRpc]
        void SubmitPositionRequestServerRpc(ServerRpcParams rpcParams = default)
        {
            Position.Value = GetRandomPositionOnPlane();
        }

        static Vector3 GetRandomPositionOnPlane()
        {
            return new Vector3(Random.Range(-3f, 3f), 1f, Random.Range(-3f, 3f));
        }

        void Update()
        {
            transform.position = Position.Value;

            if (mr.material != playerColors[PlayerColor.Value])
            {
                // Debug.Log($"{gameObject.name}.HelloWorldPlayer.Update");
                // Debug.Log($"\t cambiamos a color: {PlayerColor.Value}");

                mr.material = playerColors[PlayerColor.Value];
            }
        }
    }
}
