using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Pun.UtilityScripts;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

public class RoomPanel : MonoBehaviour
{
    [SerializeField] PlayerEntry[] playerEntries;
    [SerializeField] Button startButton;

    //�濡 ���Ծ��� ��
    private void OnEnable()
    {
        UpdatePlayers();

        PlayerNumbering.OnPlayerNumberingChanged += UpdatePlayers;

        PhotonNetwork.LocalPlayer.SetReady(false);
    }

    private void OnDisable()
    {
        PlayerNumbering.OnPlayerNumberingChanged -= UpdatePlayers;
    }

    public void UpdatePlayers()
    {
        foreach (PlayerEntry entry in playerEntries)
        {
            entry.SetEmpty();
        }

        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if (player.GetPlayerNumber() == -1)
                continue;

            int number = player.GetPlayerNumber();
            playerEntries[number].SetPlayer(player);
        }

        if (PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            startButton.interactable = CheakAllReady();
        }
        else
        {
            startButton.interactable = false;
        }
    }

    public void EnterPlayer(Player newPlayer)
    {
        Debug.Log($"{newPlayer.NickName} ����");
        UpdatePlayers();
    }

    public void ExitPlayer(Player otherPlayer)
    {
        Debug.Log($"{otherPlayer.NickName} ����");
        UpdatePlayers();
    }

    public void UpdatePlayerProperty(Player targetPlyer, Hashtable properties)
    {
        //���� Ŀ���� ������Ƽ�� ������ ���� READY Ű�� ����
        if (properties.ContainsKey(CustomProperty.READY))
        {
            UpdatePlayers();
        }
    }

    private bool CheakAllReady()
    {
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if (player.GetReady() == false)
                return false;
        }

        return true;
    }

    public void StartGame()
    {
        PhotonNetwork.LoadLevel("Game");

        //���� ���߿� ������ �� ����
        PhotonNetwork.CurrentRoom.IsOpen = false;
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }
}
