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

    //�÷��̾� ���� ����
    private void PlayerSpawn()
    {
        Vector3 randomPos = new Vector3(Random.Range(-5f, 5f), 0.398f, Random.Range(-5f, 5f));

        PhotonNetwork.Instantiate("GameObject/Player", randomPos, Quaternion.identity);
    }

    // �÷��̾� ������ �Լ�
    public void RespawnPlayer(PlayerController player)
    {
        StartCoroutine(RespawnCoroutine(player));
    }

    //�÷��̾ ������ ���Ǵ� �ڷ�ƾ (3�� �� ������)
    private IEnumerator RespawnCoroutine(PlayerController player)
    {
        yield return new WaitForSeconds(3f);

        Vector3 randomPos = new Vector3(Random.Range(-5f, 5f), 0.398f, Random.Range(-5f, 5f));
        player.photonView.RPC("RespawnPlayer", RpcTarget.All, randomPos);
    }
}
