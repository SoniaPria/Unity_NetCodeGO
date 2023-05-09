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

		public void MoveOnRequest(int key)
		{
			if (!IsOwner) { return; }

			Vector3 direction;

			// Debug.Log($"{gameObject.name}.HelloWorldPlayer.MoveOnRequest({key})");

			switch (key)
			{
				case 1:
					direction = Vector3.forward;
					break;

				case 2:
					direction = Vector3.right;
					break;

				case 3:
					direction = Vector3.back;
					break;

				case 4:
					direction = Vector3.left;
					break;

				default:
					direction = Vector3.zero;

					// Debug.Log($"\t A tecla pulsada non é valida para mover");
					break;
			}

			// Debug.Log($"\t New direction: {direction}");

			if (NetworkManager.Singleton.IsServer)
			{
				// Actualizamos a posición na variable de rede
				Position.Value += direction;

				// Debug.Log($"\t new.position: {Position.Value}");
			}
			else
			{
				SubmitToPositionRequestServerRpc(direction);
			}
		}

		[ServerRpc]
		void SubmitToPositionRequestServerRpc(Vector3 direction, ServerRpcParams rpcParams = default)
		{
			Position.Value += direction;

			// Debug.Log($"{gameObject.name}.HelloWorldPlayer.SubmitToPositionRequestServerRpc({direction})");
			// Debug.Log($"\t new.position: {Position.Value}");
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
			if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
			{
				// Debug.Log($"{gameObject.name}.HelloWorldPlayer.Update");
				// Debug.Log($"\t Input W | Input Up arrow");

				MoveOnRequest(1);
			}

			if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
			{
				// Debug.Log($"{gameObject.name}.HelloWorldPlayer.Update");
				// Debug.Log($"\t Input D | Input Right arrow");

				MoveOnRequest(2);
			}

			if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
			{
				// Debug.Log($"{gameObject.name}.HelloWorldPlayer.Update");
				// Debug.Log($"\t Input S | Input Down arrow");

				MoveOnRequest(3);
			}

			if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
			{
				// Debug.Log($"{gameObject.name}.HelloWorldPlayer.Update");
				// Debug.Log($"\t Input A | Input Left arrow");

				MoveOnRequest(4);
			}

			// Asigna a posición da variable de rede ao game object
			transform.position = Position.Value;
		}
	}
}