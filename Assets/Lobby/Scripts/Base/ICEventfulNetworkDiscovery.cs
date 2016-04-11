using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

using System;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public class ServerListUpdatedEvent: UnityEvent { }


/**
 * Holds information about discovered servers.
 */
public struct DiscoveredServer {
    public string Address;
    public string Data;

    public DateTime Timestamp;
}


/**
 * Impementation of NetworkDiscovery class that
 * exposes UnityEvents that can be subscribed to and
 * keeps track of available servers.
 */
public class ICEventfulNetworkDiscovery : NetworkDiscovery
{
    public Dictionary<string, DiscoveredServer> servers = new Dictionary<string, DiscoveredServer>();

    [SerializeField]
    public ServerListUpdatedEvent OnServerListUpdated = new ServerListUpdatedEvent();


    /**
     * Removes servers which haven't been seen for the last 5
     * broadcastIntervals. Returns whether servers have been
     * removed.
     */
    private bool PurgeOldServers()
    {
        bool hasChanged = false;

        foreach(var item in servers) {
            var offset = item.Value.Timestamp - DateTime.Today;

            if(offset.Milliseconds > 5 * broadcastInterval) {
                Debug.Log("Purging server, age: " + offset.Milliseconds.ToString());
                servers.Remove(item.Key);
                hasChanged = true;
            }
        }

        return hasChanged;
    }


    /**
     * Called when a broadcast is received, updates the list of
     * available servers and invokes the OnNewServer callback.
     */
    public override void OnReceivedBroadcast(string fromAddress, string data)
    {
        Debug.Log("Adding: " + fromAddress + "/" + data);

        // Update information on current broadcast
        if(servers.ContainsKey(fromAddress)) {
            DiscoveredServer server = servers[fromAddress];
            server.Data = data;
            server.Timestamp = DateTime.Today;
        } else {
            DiscoveredServer server = new DiscoveredServer();
            server.Address = fromAddress;
            server.Data = data;
            server.Timestamp = DateTime.Today;
            servers.Add(fromAddress, server);
        }

        PurgeOldServers();
        OnServerListUpdated.Invoke();
    }

    /*void Update()
    {
        if(PurgeOldServers())
            OnServerListUpdated.Invoke();
    }*/
}