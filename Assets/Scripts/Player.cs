using UnityEngine;
using Unity.Netcode;

public class Player : NetworkBehaviour
{
    NetworkVariable<int> StartingLine = new NetworkVariable<int>();

    float jumpForce, period;

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
        period = 4.5f;

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

    float MobileSmoothStep(float seconds)
    {
        float value = MobilePingPong(seconds);
        float min = 0f;
        float max = 1f;

        // public static float Clamp(float value, float min, float max);
        // @return float entre min y max

        float myClamp = Mathf.Clamp(value, min, max);

        // Debug.Log($"{gameObject.name}. Clamp {myClamp}");

        // public static float SmoothStep(float from, float to, float t);
        // Interpolado entre from y to, con suavizado en los límites.

        float from = 0f;
        float to = 1f;
        float t = myClamp;

        float mySmoothStep = Mathf.SmoothStep(from, to, t);

        return mySmoothStep;
    }


    // Método propio PingPong normalizado a lenght 1 (período 1) 
    // @return float [0 - 1]

    float MobilePingPong(float seconds)
    {
        // PingPong precisa que t sea un valor autoincremental, p.ej., Time.time o Time.unscaledTime.
        // @return float [0 - lenght]

        // public static float PingPong(float t, float length = 2f);

        float t = seconds * 2;
        float lenght = 1f;

        float myPingPong = Mathf.PingPong(t, lenght);

        // Debug.Log($"{gameObject.name}.PingPong {myPingPong}");

        return myPingPong;
    }


    int GetFreeStartingAxis()
    {
        int playerPosition, freePosition;
        bool isFree = true;

        int minX = GameManager.instance.minX;
        int maxX = GameManager.instance.maxX;

        do
        {
            freePosition = Random.Range(minX, maxX);

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

    Vector3 GetStartPointPosition()
    {
        // Situar al player en el punto de salida de su carril
        float x = (float)StartingLine.Value;
        float y = (float)GameManager.instance.minY;
        float z = (float)GameManager.instance.minZ;

        return new Vector3(x, y, z);
    }

    Vector3 GetEndPointPosition()
    {
        // Situar al player en el punto de salida de su carril
        float x = (float)StartingLine.Value;
        float y = (float)GameManager.instance.minY;
        float z = (float)GameManager.instance.maxZ;

        return new Vector3(x, y, z);
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
        float y = (float)GameManager.instance.minY;
        float z = (float)GameManager.instance.minZ;

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


    // Mediante Vector3.Lerp y SmoothStep, obténse un movimento infinito entre
    // startPoint y endPoint, tardando period segundos en ida e volta
    [ServerRpc]
    void SmoothLinearMovementServerRpc(float period, ServerRpcParams rpcParams = default)
    {
        // public static Vector3 Lerp(Vector3 a, Vector3 b, float t);
        // @return Vector3 Interpolated value, equals to a + (b - a) * t.

        // Cuando t= 1, Vector3.Lerp(a, b, t) devuelve a.
        // Cuando t= 1, Vector3.Lerp(a, b, t) devuelve b.
        // Cuando t= 0,5, Vector3.Lerp(a, b, t) devuelve el punto medio entre ay b.

        Vector3 a = GetStartPointPosition();
        Vector3 b = GetEndPointPosition();

        float t = MobileSmoothStep(Time.time / period);
        transform.position = Vector3.Lerp(a, b, t);
    }

    void Update()
    {
        if (!IsOwner)
        {
            return;
        }

        // Movimento infinito entre 2 puntos de forma suavizada en 'period' segundos
        if (period != 0f)
        {
            SmoothLinearMovementServerRpc(period);
        }
    }
}
