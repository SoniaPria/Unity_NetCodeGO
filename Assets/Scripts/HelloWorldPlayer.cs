using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace HelloWorld
{
    public class HelloWorldPlayer : NetworkBehaviour
    {
        public NetworkVariable<Vector3> Position = new NetworkVariable<Vector3>();

        [SerializeField]
        List<Material> playerColors;

        MeshRenderer mr;

        void Start()
        {
            mr = GetComponent<MeshRenderer>();

            var ngo = GetComponent<NetworkObject>();
            string uid = ngo.NetworkObjectId.ToString();

            // Dev
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

            ChangeColor();
        }

        public override void OnNetworkSpawn()
        {
            if (IsOwner)
            {
                Move();
            }
        }

        public void ChangeColor()
        {
            int rdm = Random.Range(0, playerColors.Count);

            //if (TakenPlayerColors.Value.Count == playerColors.Count)
            //if (PlayerColors == null || PlayerColors.Value.Count == 0)
            // { }

            Debug.Log($"{gameObject.name}.HelloWorldPlayer.ChangeColor");
            Debug.Log($"\t {playerColors}");
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

        [ServerRpc]
        void SetRandomPlayerColorServerRpc(ServerRpcParams rpcParams = default)
        {
            Debug.Log($"{gameObject.name}.HelloWorldPlayer.SetRandomPlayerColorServerRpc");
        }

        static Vector3 GetRandomPositionOnPlane()
        {
            return new Vector3(Random.Range(-3f, 3f), 1f, Random.Range(-3f, 3f));
        }

        void Update()
        {
            transform.position = Position.Value;
        }
    }
}
