# Unity NetCode for GameObjects

## Práctica 2.- Movemento multiplayer en rede local

### Requisitos

1. Main project segundo o Manual de Unity: [Get started with NGO](https://docs-multiplayer.unity3d.com/netcode/current/tutorials/get-started-ngo)
2. Inhabilitar o NetworkTransform para todas as instancias de _Player_ para esta práctica.

### Obxectivo

Partindo do proxecto base NGO 'HelloWorld' da docu de Unity Multiplayer Networking, trátase de facer que as capsulas (Player gameObjects) respondan aos movementos esquerda, dereita, arriba e abaixo e de asegurarse de que cada movemento se reproduza en rede (que o player se mova en todos os equipos).

Esto débese conseguir sen usar o Network Transform para esta práctica, só variables Network e chamadas RPC
