using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;

public class LobbyScene : MonoBehaviourPunCallbacks
{
    public enum Panel { Login, Menu, Lobby, Room }

    [SerializeField] LoginPanel loginPanel;
    [SerializeField] MainPanel menuPanel;
    [SerializeField] RoomPanel roomPanel;
    [SerializeField] LobbyPanel lobbyPanel;

    private void Start()
    {
        //마스터 클라이언트랑 항상 똑같은 씬으로 로드
        PhotonNetwork.AutomaticallySyncScene = true;

        if (PhotonNetwork.InRoom)
        {
            SetActivePanel(Panel.Room);
        }

        else if (PhotonNetwork.InLobby)
        {
            SetActivePanel(Panel.Lobby);
        }

        else if (PhotonNetwork.IsConnected)
        {
            SetActivePanel(Panel.Menu);
        }

        else
        {
            SetActivePanel(Panel.Login);
        }
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("접속 성공");
        SetActivePanel(Panel.Menu);
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log($"접속이 끊겼습니다! cause: {cause}");
        SetActivePanel(Panel.Login);
    }

    public override void OnCreatedRoom()
    {
        Debug.Log("방 생성 성공");
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.LogWarning($"방 생성 실패, 사유: {message}");
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("방 입장 성공");
        SetActivePanel(Panel.Room);
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        roomPanel.EnterPlayer(newPlayer);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        roomPanel.ExitPlayer(otherPlayer);
    }

    //방안에 있는 플레이어 중 정보를 변경한 플레이어가 있었을 때
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        roomPanel.UpdatePlayerProperty(targetPlayer, changedProps);
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogWarning($"방 입장 실패, 사유: {message}");
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.LogWarning($"랜덤 매칭 실패, 사유: {message}");
    }

    public override void OnLeftRoom()
    {
        Debug.Log("방 퇴장 성공");
        SetActivePanel(Panel.Menu);
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("로비 입장 성공");
        SetActivePanel(Panel.Lobby);
    }

    public override void OnLeftLobby()
    {
        Debug.Log("로비 퇴장 성공");
        lobbyPanel.ClearRoomEntries();
        SetActivePanel(Panel.Menu);
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        //방의 목록이 변경이 있는 경우 서버에서 보내는 정보들
        //주의사항
        //1. 처음 로비 입장 시: 모든 방 목록을 전달
        //2. 입장 중 방 목록이 변경되는 경우: 변경된 방 목록만 전달

        lobbyPanel.UpdateRoomList(roomList);
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        Debug.Log($"{newMasterClient.NickName}으로 방장이 바뀌었습니다.");
    }

    private void SetActivePanel(Panel panel)
    {
        loginPanel.gameObject.SetActive(panel == Panel.Login);
        menuPanel.gameObject.SetActive(panel == Panel.Menu);
        roomPanel.gameObject.SetActive(panel == Panel.Room);
        lobbyPanel.gameObject.SetActive(panel == Panel.Lobby);
    }
}
