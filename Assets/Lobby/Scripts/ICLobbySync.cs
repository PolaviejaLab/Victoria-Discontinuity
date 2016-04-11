using UnityEngine;
using UnityEngine.Networking;
using System.Collections;


/**
 * Script responsible for synchronizing the list of
 * participants in the lobby across the network.
 *
 * It should be instantiated using NetworkServer.Spawn(),
 * adding it to the scene manually will NOT work.
 */
public class ICLobbySync : NetworkBehaviour 
{
    /**
     * Initiate refresh of participant list, should
     * be called by the server.
     */
    public void UpdateClientList()
    {
        CmdUpdateClientList();
    }


    /**
     * Transmits the participant list from
     * the server to all individual clients.
     */
    [Command]
    void CmdUpdateClientList() {
        ICLobbyController lobbyController = GameObject.Find("Lobby").GetComponent<ICLobbyController>();

        RpcClearParticipantList();
        foreach(var item in lobbyController.participantList.items) {
            RpcAddParticipant(item.Key, item.Value);
        }
    }


    /**
     * RPC invoked by server to clear
     * the participant list on the clients.
     */
    [ClientRpc]
    void RpcClearParticipantList()
    {
        ICLobbyController lobbyController = GameObject.Find("Lobby").GetComponent<ICLobbyController>();

        if(lobbyController.isClient)
            lobbyController.participantList.items.Clear();
    }


    /**
     * RPC invoked by the server to add an
     * item to the participant list.
     */
    [ClientRpc]
    void RpcAddParticipant(string key, string value)
    {
        ICLobbyController lobbyController = GameObject.Find("Lobby").GetComponent<ICLobbyController>();

        if(lobbyController.isClient)
            lobbyController.participantList.items.Add(key, value);
    }

}
