using Unity.Netcode;
using UnityEngine;

namespace HelloWorld
{
	public class HelloWorldPlayer : NetworkBehaviour
	{
		public NetworkVariable<Vector3> Position = new NetworkVariable<Vector3>();

		void Start()
		{
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
		}

		public override void OnNetworkSpawn()
		{
			if (IsOwner)
			{
				Move();
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

		public void MoveToLeft()
		{
			if (!IsOwner) { return; }
			if (NetworkManager.Singleton.IsServer)
			{
				// Calculamos a posición que se pide por teclado
				Vector3 newPosition = transform.position + Vector3.left;
				// Actualizamos a posición na variable de rede
				Position.Value += Vector3.left;

				Debug.Log($"{gameObject.name}.HelloWorldPlayer.MoveToLeft");
				Debug.Log($"\t new.position: {newPosition}");
			}
			else
			{
				SubmitLeftPositionRequestServerRpc();
			}
		}

		[ServerRpc]
		void SubmitLeftPositionRequestServerRpc(ServerRpcParams rpcParams = default)
		{
			Position.Value += Vector3.left;

			Debug.Log($"{gameObject.name}.HelloWorldPlayer.SubmitLeftPositionRequestServerRpc");
			Debug.Log($"\t new.position: {Position.Value}");
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
			if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
			{
				Debug.Log($"{gameObject.name}.HelloWorldPlayer.Update");
				Debug.Log($"\t Input A | Input left arrow");

				MoveToLeft();
			}


			// Asigna a posición da variable de rede ao game object
			transform.position = Position.Value;

			// Debug.Log($"{gameObject.name}.HelloWorldPlayer.Update");
			// Debug.Log($"\t transform.position: {transform.position}");
		}
	}
}