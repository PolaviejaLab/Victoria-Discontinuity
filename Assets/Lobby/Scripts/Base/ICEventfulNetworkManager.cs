using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;

using System;
using System.Collections;
using System.Collections.Generic;

[Serializable] public class ClientConnectEvent: UnityEvent<NetworkConnection> { }
[Serializable] public class ClientDisconnectEvent: UnityEvent<NetworkConnection> { }
[Serializable] public class ClientErrorEvent: UnityEvent<NetworkConnection, int> { }
[Serializable] public class ClientNotReadyEvent: UnityEvent<NetworkConnection> { }
[Serializable] public class ClientSceneChangedEvent: UnityEvent<NetworkConnection> { }

//[Serializable] public class MatchCreateEvent: UnityEvent<CreateMatchResponse> { }
//[Serializable] public class MatchListEvent: UnityEvent<ListMatchResponse> { }

[Serializable] public class ServerConnectEvent: UnityEvent<NetworkConnection> { }
[Serializable] public class ServerDisconnectEvent: UnityEvent<NetworkConnection> { }
[Serializable] public class ServerReadyEvent: UnityEvent<NetworkConnection> { }


public class ICEventfulNetworkManager : NetworkManager 
{
    // Called on the client when connected to a server.
    [SerializeField] public ClientConnectEvent ClientConnect = new ClientConnectEvent();

    // Called on clients when disconnected from a server.
    [SerializeField] public ClientDisconnectEvent ClientDisconnect = new ClientDisconnectEvent();

    // Called on clients when a network error occurs.
    [SerializeField] public ClientErrorEvent ClientError = new ClientErrorEvent();

    // Called on clients when a servers tells the client it is no longer ready.
    [SerializeField] public ClientNotReadyEvent ClientNotReady = new ClientNotReadyEvent();

    // Called on clients when a scene has completed loaded, when the scene load was initiated by the server.
    [SerializeField] public ClientSceneChangedEvent ClientSceneChanged = new ClientSceneChangedEvent();


    // This is invoked when a match has been created.
    //[SerializeField] public MatchCreateEvent MatchCreate = new MatchCreateEvent();

    // This is invoked when a list of matches is returned from ListMatches().
    //[SerializeField] public MatchListEvent MatchList = new MatchListEvent();


    // Called on the server when a new client connects.
    [SerializeField] public ServerConnectEvent ServerConnect = new ServerConnectEvent();

    // Called on the server when a client disconnects.
    [SerializeField] public ServerDisconnectEvent ServerDisconnect = new ServerDisconnectEvent();

    // Called on the server when a client is ready.
    [SerializeField] public ServerReadyEvent ServerReady = new ServerReadyEvent();

    public List<NetworkConnection> connections = new List<NetworkConnection>();


    public override void OnClientConnect(NetworkConnection conn)
    {
        base.OnClientConnect(conn);
        ClientConnect.Invoke(conn);
    }


    public override void OnClientDisconnect(NetworkConnection conn)
    {
        ClientDisconnect.Invoke(conn);
        base.OnClientDisconnect(conn);
    }


    public override void OnClientError(NetworkConnection conn, int errorCode)
    {
        base.OnClientError(conn, errorCode);
        ClientError.Invoke(conn, errorCode);
    }


    public override void OnClientNotReady(NetworkConnection conn)
    {
        base.OnClientNotReady(conn);
        ClientNotReady.Invoke(conn);
    }


    public override void OnClientSceneChanged(NetworkConnection conn)
    {
        base.OnClientSceneChanged(conn);
        ClientSceneChanged.Invoke(conn);
    }


    /*public override void OnMatchCreate(CreateMatchResponse matchInfo)
    {
        base.OnMatchCreate(matchInfo);
        MatchCreate.Invoke(matchInfo);
    }


    public override void OnMatchList(ListMatchResponse matchList)
    {
        base.OnMatchList(matchList);
        MatchList.Invoke(matchList);
    }*/


    public override void OnServerConnect(NetworkConnection conn)
    {
        base.OnServerConnect(conn);

        connections.Add(conn);
        ServerConnect.Invoke(conn);
    }


    public override void OnServerDisconnect(NetworkConnection conn)
    {
        connections.Remove(conn);
        ServerDisconnect.Invoke(conn);

        base.OnServerDisconnect(conn);
    }

    public override void OnServerReady(NetworkConnection conn)
    {
        base.OnServerReady(conn);
        ServerReady.Invoke(conn);
    }
}
