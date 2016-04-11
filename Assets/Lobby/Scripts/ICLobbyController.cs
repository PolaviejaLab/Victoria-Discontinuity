using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

using System;
using System.Collections;


/**
 * Handle lobby screen where participants wait for
 * clients to connect before starting the experiment.
 * There are two "versions" of this screen:
 *  - On the server it calls StartHost on the NetworkManager
 *    and will broadcast to the network. It also spawns a
 *    ICLobbySync that synchronized the client lists.
 *
 *  - On the client it will connect to the host.
 */
public class ICLobbyController : MonoBehaviour 
{
    public ICExperimentSetupController experimentSetup;

    public ICListBox participantList;
    public Button startButton;
    public Button cancelButton;

    public ICLobbySync syncScriptPrefab;
    private ICLobbySync syncScript = null;

    private bool _isClient = false;
    private bool _isServer = false;   

    public bool isClient { get { return _isClient; } }
    public bool isServer { get { return _isServer; } }


    /**
     * Register the ICLobbySync script as spawnable.
     */
    void Start()
    {        
        if(!syncScriptPrefab) throw new Exception("syncScript field not set.");
        ClientScene.RegisterPrefab(syncScriptPrefab.gameObject);
    }


    /**
     * Verify that all fields are set, and setup command listeners.
     */
    void Awake()
    {
        if(!experimentSetup) throw new Exception("experimentSetup field not set.");
        if(!cancelButton) throw new Exception("cancelButton field not set.");
        if(!startButton) throw new Exception("startButton field not set.");

        cancelButton.onClick.RemoveAllListeners();
        cancelButton.onClick.AddListener(() => { experimentSetup.Cancel(); });
    }


    /**
     * Update participant list - server side update
     */
    private void UpdateParticipantList()
    {
        var networkManager = ICNetworkUtilities.GetNetworkManager();

        Debug.Log("Updating list of participants, found: " + networkManager.connections.Count.ToString());

        // Update server-side participant list
        participantList.items.Clear();
        for(var i = 0 ; i < networkManager.connections.Count; i++) {
            participantList.items.Add(
                networkManager.connections[i].address,
                networkManager.connections[i].address);
        }

        // Invoke update function on clients
        if(syncScript) 
            syncScript.UpdateClientList();
    }


    /**
     * Setup event listeners for the server-mode.
     */
    private void SetupServerEventListeners(ICEventfulNetworkManager networkManager)
    {
        networkManager.ClientConnect.RemoveAllListeners();
        networkManager.ClientDisconnect.RemoveAllListeners();
        networkManager.ServerConnect.RemoveAllListeners();
        networkManager.ServerDisconnect.RemoveAllListeners();
        networkManager.ServerReady.RemoveAllListeners();

        networkManager.ClientConnect.AddListener((client) => { UpdateParticipantList(); });
        networkManager.ClientDisconnect.AddListener((client) => { UpdateParticipantList(); });
        networkManager.ServerConnect.AddListener((client) => { UpdateParticipantList(); });
        networkManager.ServerDisconnect.AddListener((client) => { UpdateParticipantList(); });
        networkManager.ServerReady.AddListener((client) => { UpdateParticipantList(); });
    }

   
    /**
     * Start the lobby in server-mode.
     */
    public void StartServer(ICExperiment experiment)
    {
        var networkDiscovery = ICNetworkUtilities.GetNetworkDiscovery();
        var networkManager = ICNetworkUtilities.GetNetworkManager();

        // Mark as server
        _isServer = true;

        // Update listeners
        SetupServerEventListeners(networkManager);

        // Start server and initialize broadcast
        networkManager.StartHost();

        networkDiscovery.Initialize();
        networkDiscovery.broadcastData = 
            "NetworkManager:" + 
            networkManager.networkAddress + ":" +
            networkManager.networkPort + ":" +
            experiment.getDisplayName();

        if(!networkDiscovery.StartAsServer()) {
            throw new Exception("StartAsServer returned false in ICLobbyController.");
        }

        // Enable start button
        startButton.enabled = true;
        startButton.onClick.RemoveAllListeners();
        startButton.onClick.AddListener(() => { experimentSetup.StartExperiment(experiment); });

        // Remove old synchronization script
        if(syncScript) {
            NetworkServer.Destroy(syncScript.gameObject);
            Destroy(syncScript.gameObject);
        }

        // Spawn synchronization script on the network
        syncScript = GameObject.Instantiate(syncScriptPrefab).GetComponent<ICLobbySync>();
        NetworkServer.Spawn(syncScript.gameObject);
        syncScript.transform.SetParent(gameObject.transform);
    }


    /**
     * Start lobby in client-mode.
     */
    public void StartClient(string address, int port)
    {
        var networkManager = ICNetworkUtilities.GetNetworkManager();

        // Mark as client
        _isClient = true;

        Debug.Log("Connecting to '" + address + "' at port " + port.ToString());

        // Setup event listeners       
        networkManager.ClientError.RemoveAllListeners();
        networkManager.ClientError.AddListener((conn, code) => { Debug.LogError("Error while connecting to server, code: " + code.ToString()); });

        networkManager.ClientConnect.RemoveAllListeners();
        networkManager.ClientConnect.AddListener((conn) => { Debug.Log("Connecting to server..."); });

        // Connect to server
        networkManager.networkAddress = address;
        networkManager.networkPort = port;
        networkManager.StartClient();

        // Disable start button
        startButton.enabled = false;
        startButton.onClick.RemoveAllListeners();
    }


    /**
     * Stop broadcasting, used when starting the experiment.
     */
    public void StopBroadcast()
    {
        var networkDiscovery = ICNetworkUtilities.GetNetworkDiscovery();

        if(networkDiscovery.isServer)
            networkDiscovery.StopBroadcast();
    }


    /**
     * Stop everything, used when cancelling the lobby.
     */
    public void StopAll()
    {
        var networkDiscovery = ICNetworkUtilities.GetNetworkDiscovery();
        var networkManager = ICNetworkUtilities.GetNetworkManager();

        if(_isServer) {
            networkManager.StopHost();
            if(networkDiscovery.isServer)
                networkDiscovery.StopBroadcast();
            _isServer = false;
        }

        if(isClient) {
            networkManager.StopClient();
            _isClient = false;
        }
    }
}
