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

        void Start() { }

        // Método de instanciado de players
        public override void OnNetworkSpawn()
        {
            InitValues();

            if (IsOwner)
            {
                Move();
                ChangeColor();
            }
        }

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
                // Asignación de color no PlayerHost
                int rdmColor = GetRandomColor();
                PlayerColor.Value = playerColorsFree[rdmColor];
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
            int rdmColor = GetRandomColor();
            PlayerColor.Value = playerColorsFree[rdmColor];
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
            Debug.Log($"{gameObject.name}.HelloWorldPlayer.Move");

            if (NetworkManager.Singleton.IsServer)
            {
                var randomPosition = GetRandomPositionOnPlane();
                transform.position = randomPosition;
                Position.Value = randomPosition;
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
