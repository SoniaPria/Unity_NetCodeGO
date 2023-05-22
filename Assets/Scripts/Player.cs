using UnityEngine;
using Unity.Netcode;

public class Player : NetworkBehaviour
{
    NetworkVariable<int> StartingLine = new NetworkVariable<int>();

    float jumpForce;

    public override void OnNetworkSpawn()
    {
        DevInfoNetworkObject();

        if (IsOwner)
        {
            Initialize();
        }

        else
        {
            // Valores das instancias de players no equipo cliente
            Debug.Log($"{gameObject.name}.OnNetworkSpawn() IsOwner {IsOwner}");
        }
    }

    public override void OnNetworkDespawn()
    {
    }

    void Initialize()
    {
        // Valores do GameObject Player no equipo cliente
        // Pide ao server que o sitúe no punto de saída
        // nun carril de carreira aleatorio entre os que non están ocupados

        SetStartingLinePositionServerRpc();

        jumpForce = 6f;

        Debug.Log($"{gameObject.name}.OnNetworkSpawn() IsOwner {IsOwner}");
    }

    void DevInfoNetworkObject()
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

    public Vector3 GetPosition()
    {
        return transform.position;
    }

    bool IsJumping()
    {
        return transform.position.y > 1.05f;
    }

    Vector3 GetRandomPosition2D()
    {
        return new Vector3(Random.Range(-3f, 3f), 1f, Random.Range(-3f, 3f));
    }

    // Vector3 GetStartingLinePosition()
    // {
    //     // Buscar un carril libre
    //     StartingLine.Value = GetFreeStartingAxis();

    //     // Situar ao player no punto de inicio do seu carril
    //     float x = (float)StartingLine.Value;
    //     float y = 1f;
    //     float z = 4f;

    //     return new Vector3(x, y, z);
    // }

    int GetFreeStartingAxis()
    {
        int playerPosition, freePosition;
        bool isFree = true;

        do
        {
            freePosition = Random.Range(-4, 4);

            // Debug.Log($"{gameObject.name}.GetFreeStartLinePosition RANDOM VALUE {freePosition}");

            foreach (ulong uid in NetworkManager.Singleton.ConnectedClientsIds)
            {
                var player = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(uid);
                playerPosition = player.GetComponent<Player>().StartingLine.Value;

                isFree = true;

                if (freePosition == playerPosition)
                {
                    isFree = false;
                    break;
                }
            }
        }
        while (!isFree);

        // Debug.Log($"{gameObject.name}.GetFreeStartLinePosition @return {freePosition}");

        return freePosition;
    }




    [ServerRpc]
    public void TestServerRpc(ServerRpcParams serverRpcParams = default)
    {
        var clientId = serverRpcParams.Receive.SenderClientId;

        Debug.Log($"{gameObject.name}.TestServerRpc client ID {clientId}");
    }


    [ServerRpc]
    void SetStartingLinePositionServerRpc(ServerRpcParams rpcParams = default)
    {
        // Buscar un carril libre e gárdase na Network variable
        StartingLine.Value = GetFreeStartingAxis();

        // Situar al player en el punto de salida de su carril
        float x = (float)StartingLine.Value;
        float y = 1f;
        float z = 4f;

        Vector3 position = new Vector3(x, y, z);

        // Network Transform
        transform.position = position;
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
        if (!IsOwner)
        {
            return;
        }

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
