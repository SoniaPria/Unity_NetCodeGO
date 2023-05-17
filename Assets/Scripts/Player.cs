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
            if (IsOwner)
            {
                // Pide ao server una posición aleatoria 
                // para que non se vexan todos no mesmo punto

                MoveServerRpc(GetRandomPosition2D());
            }
        }

        bool IsJumping() { return transform.position.y > 1.05f; }


        Vector3 GetRandomPosition2D()
        {
            return new Vector3(Random.Range(-3f, 3f), 1f, Random.Range(-3f, 3f));
        }


        [ServerRpc]
        void MoveServerRpc(Vector3 direction, ServerRpcParams rpcParams = default)
        {
            transform.position += direction;
        }

        [ServerRpc]
        void JumpServerRpc(ServerRpcParams rpcParams = default)
        {
            Rigidbody rb = GetComponent<Rigidbody>();
            rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
        }

        void Update()
        {
            if (!IsOwner) { return; }

            if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            {

                MoveServerRpc(Vector3.forward);
            }

            if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            {

                MoveServerRpc(Vector3.right);
            }

            if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            {

                MoveServerRpc(Vector3.back);
            }

            if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            {

                MoveServerRpc(Vector3.left);

            }

            if (Input.GetButtonDown("Jump") && !IsJumping())
            {

                JumpServerRpc();
            }
        }
    }
}