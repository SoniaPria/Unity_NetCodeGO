using Unity.Netcode;
using UnityEngine;

namespace HelloWorld
{
    public class Player : NetworkBehaviour
    {
        float jumpForce;

        void Start()
        {
            jumpForce = 6f;
        }

        public override void OnNetworkSpawn()
        {
            InitValues();

            if (IsOwner)
            {
                // Para que non se espaneen no mesmo punto
                InitRandomPosition();
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

            Debug.Log($"{gameObject.name}.Player");
            Debug.Log($"\t IsLocalPlayer: {ngo.IsLocalPlayer}");
            Debug.Log($"\t IsOwner: {ngo.IsOwner}");
            Debug.Log($"\t IsOwnedByServer: {ngo.IsOwnedByServer}");
            // --- end Dev
        }

        void InitRandomPosition()
        {
            SubmitRandomPositionRequestServerRpc();
        }
        public void Jump()
        {
            Debug.Log($"{gameObject.name}.Player.Jump()");

            // Rigidbody rb = GetComponent<Rigidbody>();
            // rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);

            SubmitJumpRequestServerRpc();
        }

        bool IsJumping()
        {
            return transform.position.y > 1.05f;
        }

        [ServerRpc]
        void SubmitRandomPositionRequestServerRpc(ServerRpcParams rpcParams = default)
        {
            // Posición aleatoria no taboleiro
            // Transform se propaga en rede sen Network variable
            transform.position = new Vector3(Random.Range(-3f, 3f), 1f, Random.Range(-3f, 3f));
        }

        [ServerRpc]
        void SubmitInputPositionRequestServerRpc(Vector3 direction, ServerRpcParams rpcParams = default)
        {
            // Posición enviada por Input de Player
            // Transform se propaga en rede sen Network variable
            transform.position += direction;
        }

        [ServerRpc]
        void SubmitJumpRequestServerRpc(ServerRpcParams rpcParams = default)
        {
            Debug.Log($"{gameObject.name}.Player.SubmitJumpPositionRequestServerRpc()");

            // Salto enviado por Input de Player
            // Rigidbody.Physics NON se propaga en rede sen Network variable

            Rigidbody rb = GetComponent<Rigidbody>();
            rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);

            // Jump();
        }

        void Update()
        {
            if (!IsOwner) { return; }

            if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            {
                // Debug.Log($"{gameObject.name}.Player.Update");
                // Debug.Log($"\t Input W | Input Up arrow");

                SubmitInputPositionRequestServerRpc(Vector3.forward);
            }

            if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            {
                // Debug.Log($"{gameObject.name}.Player.Update");
                // Debug.Log($"\t Input D | Input Right arrow");

                SubmitInputPositionRequestServerRpc(Vector3.right);
            }

            if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            {
                // Debug.Log($"{gameObject.name}.Player.Update");
                // Debug.Log($"\t Input S | Input Down arrow");

                SubmitInputPositionRequestServerRpc(Vector3.back);
            }

            if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            {
                // Debug.Log($"{gameObject.name}.Player.Update");
                // Debug.Log($"\t Input A | Input Left arrow");

                SubmitInputPositionRequestServerRpc(Vector3.left);

            }

            if (Input.GetButtonDown("Jump") && !IsJumping())
            {
                // Debug.Log($"{gameObject.name}.Player.Update");
                // Debug.Log($"\t Input Jump");

                SubmitJumpRequestServerRpc();
            }
        }
    }
}