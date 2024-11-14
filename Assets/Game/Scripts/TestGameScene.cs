using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using UnityEngine;

//게임씬에서 바로 테스트 할 수 있도록 함
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
        options.IsVisible = false; //비공개방으로 설정

        PhotonNetwork.JoinOrCreateRoom(RoomName, options, null);
    }

    public override void OnJoinedRoom()
    {
        StartCoroutine(StartDelayRoutine());
    }

    IEnumerator StartDelayRoutine()
    {
        yield return new WaitForSeconds(1f); //네트워크 준비에 필요한 시간 주기
        TestGameStart();
    }
    public void TestGameStart()
    {
        Debug.Log("게임 시작");
        //TODO: 테스트용 게임 시작 부분

        PlayerSpawn();
    }

    public void PlayerSpawn()
    {
        Vector3 randomPos = new Vector3(Random.Range(-5f, 5f), 0.3980001f, Random.Range(-5f, 5f));
       
        //플레이어 생성 요청
        PhotonNetwork.Instantiate("GameObject/TPS Player", randomPos, Quaternion.identity);
    }
}
