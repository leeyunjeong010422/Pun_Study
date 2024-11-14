using Photon.Pun;
using TMPro;
using UnityEngine;

public class LoginPanel : MonoBehaviour
{
    [SerializeField] TMP_InputField idInputField;

    private void Start()
    {
        //플레이어 랜덤 이름 생성
        idInputField.text = $"Player {Random.Range(1000, 10000)}";
    }

    public void Login()
    {
        if (idInputField.text == "")
        {
            Debug.LogWarning("아이디를 입력해야 접속이 가능합니다");
            return;
        }

        //서버에 요청
        //PhotonNetwork.~~~ 으로 서버에 요청 진행 가능
        PhotonNetwork.LocalPlayer.NickName = idInputField.text;
        
        //포톤 세팅 파일에 있는 내용을 가지고 접속 신청을 함
        PhotonNetwork.ConnectUsingSettings();
    }
}
