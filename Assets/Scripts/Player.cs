using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Player : NetworkBehaviour
{
    NetworkVariable<int> StartingLine = new NetworkVariable<int>();

    // Lista de cores: 0: LemonYellow, 1: Green, 2: Red
    [SerializeField]
    List<Material> playerColors;


    // Compoñente MeshRenderes do GameObject
    MeshRenderer mr;

    // float jumpForce, period;
    float period, initPeriod, boonPeriod, banePeriod;

    float jumpForce;

    public override void OnNetworkSpawn()
    {
        period = 4.5f;
        initPeriod = period;
        boonPeriod = 2f;
        banePeriod = 8f;
        jumpForce = 6f;

        mr = GetComponent<MeshRenderer>();

        // Default material
        mr.material = playerColors[0];

        if (IsOwner)
        {
            Initialize();
        }
    }

    void Initialize()
    {
        // Valores do GameObject Player no equipo cliente
        // Pide ao server que o sitúe no punto de saída
        // nun carril de carreira aleatorio entre os que non están ocupados

        SetStartingLinePositionServerRpc();
    }


    float MobileSmoothStep(float seconds)
    {
        float value = MobilePingPong(seconds);
        float min = 0f;
        float max = 1f;

        // public static float Clamp(float value, float min, float max);
        // @return float entre min y max

        float myClamp = Mathf.Clamp(value, min, max);

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

        return myPingPong;
    }


    // Retorna un carril libre para o player corredor infinito entre os dispoñibles
    int GetFreeStartingAxis()
    {
        int playerPosition, freePosition;
        bool isFree = true;

        int minX = GameManager.instance.minX;
        int maxX = GameManager.instance.maxX;

        do
        {
            freePosition = Random.Range(minX, maxX);

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

        return freePosition;
    }

    // Posición 3D inicial do corredor infinito
    Vector3 GetStartPointPosition()
    {
        // Situar al player en el punto de salida de su carril
        float x = (float)StartingLine.Value;
        float y = transform.position.y;
        float z = (float)GameManager.instance.minZ;

        return new Vector3(x, y, z);
    }

    // Posición 3D final do corredor infinito
    Vector3 GetEndPointPosition()
    {
        // Situar al player en el punto de salida de su carril
        float x = (float)StartingLine.Value;
        float y = transform.position.y;
        float z = (float)GameManager.instance.maxZ;

        return new Vector3(x, y, z);
    }

    // Arroutadas do server
    // Aleatoriamente establece unha des/vantase de velocidade
    // e se distingue pola cor do Mesh do netPlayer

    [ClientRpc]
    public void SetBoonBaneClientRpc(bool isBoon, ClientRpcParams clientRpcParams = default)
    {
        if (isBoon)
        {
            period = boonPeriod;
            mr.material = playerColors[1];
        }

        else
        {
            period = banePeriod;
            mr.material = playerColors[2];
        }
    }

    // Restablecimento dos valores normais de Play

    [ClientRpc]
    public void ResetBoonBaneClientRpc(ClientRpcParams clientRpcParams = default)
    {
        period = initPeriod;
        mr.material = playerColors[0];
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
    [ServerRpc]
    void JumpServerRpc(ServerRpcParams rpcParams = default)
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    void Update()
    {
        if (!IsOwner) { return; }

        // Movimento infinito entre 2 puntos de forma suavizada en 'period' segundos
        if (period != 0f)
        {
            SmoothLinearMovementServerRpc(period);
        }

        if (Input.GetButtonDown("Jump"))
        {
            Debug.Log($"{gameObject.name}.Jump()");
            JumpServerRpc();
            // gameObject.GetComponent<Rigidbody>().AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

        }
    }
}
