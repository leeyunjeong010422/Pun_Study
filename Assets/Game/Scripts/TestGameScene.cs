using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using UnityEngine;

//���Ӿ����� �ٷ� �׽�Ʈ �� �� �ֵ��� ��
public class TestGameScene : MonoBehaviourPunCallbacks
{
    public const string RoomName = "TestRoom";

    private void Start()
    {
        PhotonNetwork.LocalPlayer.NickName = $"Player {Random.Range(1000, 10000)}";
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        RoomOptions options = new RoomOptions();
        options.MaxPlayers = 8;
        options.IsVisible = false; //����������� ����

        PhotonNetwork.JoinOrCreateRoom(RoomName, options, null);
    }

    public override void OnJoinedRoom()
    {
        StartCoroutine(StartDelayRoutine());
    }

    IEnumerator StartDelayRoutine()
    {
        yield return new WaitForSeconds(1f); //��Ʈ��ũ �غ� �ʿ��� �ð� �ֱ�
        TestGameStart();
    }
    public void TestGameStart()
    {
        Debug.Log("���� ����");
        //TODO: �׽�Ʈ�� ���� ���� �κ�

        PlayerSpawn();
    }

    public void PlayerSpawn()
    {
        Vector3 randomPos = new Vector3(Random.Range(-5f, 5f), 0.3980001f, Random.Range(-5f, 5f));
       
        //�÷��̾� ���� ��û
        PhotonNetwork.Instantiate("GameObject/TPS Player", randomPos, Quaternion.identity);
    }
}
