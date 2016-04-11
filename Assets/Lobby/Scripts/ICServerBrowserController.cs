using UnityEngine;
using UnityEngine.UI;

using System;
using System.Collections;

public class ICServerBrowserController : MonoBehaviour 
{
    public ICExperimentSetupController experimentSetup;

    public ICListBox serverList;
    public Button connectButton;
    public Button cancelButton;



    void Awake()
    {
        if(!experimentSetup) throw new Exception("experimentSetup field is not set.");
    }


    private ICEventfulNetworkDiscovery GetNetworkDiscovery()
    {
        GameObject networkManagerObject = GameObject.Find("NetworkManager");
        if(networkManagerObject == null) return null;

        return networkManagerObject.GetComponent<ICEventfulNetworkDiscovery>();
    }


    private void CancelBrowsing()
    {
        experimentSetup.Cancel();
    }

    private void ConnectToServer()
    {
        ICEventfulNetworkDiscovery networkDiscovery = GetNetworkDiscovery();

        if(networkDiscovery.servers.ContainsKey(serverList.selectedItem)) {
            var server = networkDiscovery.servers[serverList.selectedItem];
            var parts = server.Data.Split(':');

            var address = parts[1];
            int port;
            int.TryParse(parts[2], out port);

            experimentSetup.StartClient(address, port);
        }
    }


	// Use this for initialization
	void Start () 
    {
	    if(cancelButton) {
            cancelButton.onClick.RemoveAllListeners();
            cancelButton.onClick.AddListener(CancelBrowsing);
        }

        if(connectButton) {
            connectButton.onClick.RemoveAllListeners();
            connectButton.onClick.AddListener(ConnectToServer);
        }

        Debug.Log("Server browser...");
	}

    void UpdateServers() 
    {
        var networkDiscovery = GetNetworkDiscovery();
        if(networkDiscovery == null) return;

        Debug.Log("Updating server list: " + serverList.items.Count.ToString());

        serverList.items.Clear();
        foreach(var item in networkDiscovery.servers) {
            string[] parts = item.Value.Data.Split(':');
            serverList.items.Add(item.Key, parts[parts.Length - 1]);
        }
    }

    void OnEnable() {
        var networkDiscovery = GetNetworkDiscovery();
        if(networkDiscovery == null) return;

        networkDiscovery.OnServerListUpdated.RemoveAllListeners();
        networkDiscovery.OnServerListUpdated.AddListener(UpdateServers);

        UpdateServers();

        networkDiscovery.Initialize();
        networkDiscovery.StartAsClient();

        Debug.Log("Waiting for servers...");
    }

    void OnDisable() {
        var networkDiscovery = GetNetworkDiscovery();
        if(networkDiscovery == null) return;

        if(networkDiscovery.hostId != -1 || networkDiscovery.running)
            networkDiscovery.StopBroadcast();

        Debug.Log("No longer waiting for servers.");
    }	
}
