using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerStateLogger : MonoBehaviourPunCallbacks
{
    [SerializeField] ClientState state;

    private void Update()
    {
        if (state == PhotonNetwork.NetworkClientState)
            return;

        state = PhotonNetwork.NetworkClientState;
        Debug.Log($"[Pun] {state}");
    }
}
