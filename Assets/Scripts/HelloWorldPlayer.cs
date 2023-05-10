using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace HelloWorld
{
	public class HelloWorldPlayer : NetworkBehaviour
	{
		public NetworkVariable<Vector3> Position = new NetworkVariable<Vector3>();
		public NetworkVariable<int> PlayerColor;

		[SerializeField]
		List<Material> playerColors;

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
			ChangeColor();
		}


		// Método de instanciado de players
		public override void OnNetworkSpawn()
		{
			if (IsOwner)
			{
				Move();
			}
		}

		public void ChangeColor()
		{
			Debug.Log($"{gameObject.name}.HelloWorldPlayer.ChangeColor");

			List<int> playerColorsFree = new List<int>();

			for (int i = 0; i < playerColors.Count; i++)
			{
				playerColorsFree.Add(i);
			}
			// Debug.Log($"\t playerColors: {playerColorsFree}");


			int takenColor;
			foreach (ulong uid in NetworkManager.Singleton.ConnectedClientsIds)
			{
				takenColor = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(uid).GetComponent<HelloWorldPlayer>().PlayerColor.Value;

				playerColorsFree.Remove(takenColor);
			}

			// Debug.Log($"\t playerColorsFree: {playerColorsFree}");


			int rdmColor = Random.Range(0, playerColorsFree.Count);

			if (NetworkManager.Singleton.IsServer)
			{
				PlayerColor.Value = playerColorsFree[rdmColor];
				Debug.Log($"\t PlayerColor: {PlayerColor.Value}");
			}
			else
			{
				SubmitPlayerColorServerRpc(playerColorsFree[rdmColor]);
			}
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
		void SubmitPlayerColorServerRpc(int color, ServerRpcParams rpcParams = default)
		{
			PlayerColor.Value = color;

			Debug.Log($"{gameObject.name}.HelloWorldPlayer.SubmitPlayerColorServerRpc");
			Debug.Log($"\t PlayerColor: {PlayerColor.Value}");
		}

		static Vector3 GetRandomPositionOnPlane()
		{
			return new Vector3(Random.Range(-3f, 3f), 1f, Random.Range(-3f, 3f));
		}

		void Update()
		{
			transform.position = Position.Value;
			mr.material = playerColors[PlayerColor.Value];
		}
	}
}
