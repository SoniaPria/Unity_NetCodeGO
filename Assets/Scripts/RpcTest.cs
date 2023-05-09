using Unity.Netcode;
using UnityEngine;

public class RpcTest : NetworkBehaviour
{
	public override void OnNetworkSpawn()
	{
		// Envia un RPC al servidor en el cliente 
		// que tiene el NetworkObject que tenga esta instancia de NetoworkBehaviour
		if (!IsServer && IsOwner)
		{
			TestServerRpc(0, NetworkObjectId);
		}
	}

	[ClientRpc]
	void TestClientRpc(int value, ulong sourceNetworkObjectId)
	{
		// Debug.Log($"El Cliente recibe el RPC #{value} en NetworkObject #{sourceNetworkObjectId}");

		// Sólo se envía un RPC al servidor en el cliente que posee el NetworkObject 
		// que posee esta instancia de NetworkBehaviour
		if (IsOwner)
		{
			TestServerRpc(value + 1, sourceNetworkObjectId);
		}
	}

	[ServerRpc]
	void TestServerRpc(int value, ulong sourceNetworkObjectId)
	{
		// Debug.Log($"El Server recibe el RPC #{value} en NetworkObject #{sourceNetworkObjectId}");

		TestClientRpc(value, sourceNetworkObjectId);
	}
}
