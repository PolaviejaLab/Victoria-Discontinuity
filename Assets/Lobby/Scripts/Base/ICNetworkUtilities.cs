/**
 * Utility functions for the network lobby
 *
 * Copyright (c) 2015-2016 Ivar Clemens for Champalimaud Centre for the Unknown, Lisbon
 */
using UnityEngine;
using System.Collections;

class ICNetworkUtilities
{
    /**
     * Returns an instance of the ICEventfulNetworkManager.
     */
    public static ICEventfulNetworkManager GetNetworkManager()
    {
        GameObject networkManagerObject = GameObject.Find("NetworkManager");

        if(!networkManagerObject)
            networkManagerObject = new GameObject();

        ICEventfulNetworkManager networkManager = networkManagerObject.GetComponent<ICEventfulNetworkManager>();

        if(!networkManager) {
            networkManager = networkManagerObject.AddComponent<ICEventfulNetworkManager>();
            networkManager.dontDestroyOnLoad = true;
        }

        return networkManager;
    }


    /**
     * Returns an instance of the ICEventfulNetworkDiscovery.
     */
    public static ICEventfulNetworkDiscovery GetNetworkDiscovery()
    {
        GameObject networkManagerObject = GameObject.Find("NetworkManager");

        if(!networkManagerObject)
            networkManagerObject = new GameObject();

        ICEventfulNetworkDiscovery networkDiscovery = networkManagerObject.GetComponent<ICEventfulNetworkDiscovery>();

        if(!networkDiscovery) {
            networkDiscovery = networkManagerObject.AddComponent<ICEventfulNetworkDiscovery>();
            networkDiscovery.useNetworkManager = false;
        }

        return networkDiscovery;
    }
}

