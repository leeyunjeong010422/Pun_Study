using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameScene : MonoBehaviourPunCallbacks
{
    [SerializeField] Button gameOverButton;
    [SerializeField] Button leaveRoomButton;

    private void Start()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            gameOverButton.interactable = true;
        }
        else
        {
            gameOverButton.interactable = false;
        }

        PlayerSpawn();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        PhotonNetwork.LoadLevel("LobbyScene");
    }

    public override void OnLeftRoom()
    {
        PhotonNetwork.LoadLevel("LobbyScene");
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            gameOverButton.interactable = true;
        }
        else
        {
            gameOverButton.interactable = false;
        }
    }

    public void GameOver()
    {
        if (PhotonNetwork.IsMasterClient == false)
            return;

        PhotonNetwork.CurrentRoom.IsOpen = true;
        PhotonNetwork.LoadLevel("LobbyScene");
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }

    //플레이어 랜덤 스폰
    private void PlayerSpawn()
    {
        Vector3 randomPos = new Vector3(Random.Range(-5f, 5f), 0.398f, Random.Range(-5f, 5f));

        PhotonNetwork.Instantiate("GameObject/Player", randomPos, Quaternion.identity);
    }

    // 플레이어 리스폰 함수
    public void RespawnPlayer(PlayerController player)
    {
        StartCoroutine(RespawnCoroutine(player));
    }

    //플레이어가 죽으면 사용되는 코루틴 (3초 후 리스폰)
    private IEnumerator RespawnCoroutine(PlayerController player)
    {
        yield return new WaitForSeconds(3f);

        Vector3 randomPos = new Vector3(Random.Range(-5f, 5f), 0.398f, Random.Range(-5f, 5f));
        player.photonView.RPC("RespawnPlayer", RpcTarget.All, randomPos);
    }
}
