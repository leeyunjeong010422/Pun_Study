using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviourPun, IPunObservable
{
    [SerializeField] float speed;
    [SerializeField] Transform muzzlePoint;
    [SerializeField] GameObject bulletPrefab;

    [SerializeField] const int MaxHp = 100;
    [SerializeField] int currentHp;

    private Color playerColor;
    [SerializeField] Renderer bodyRenderer;

    [SerializeField] Slider hpSlider;

    private void Start()
    {
        bodyRenderer = GetComponentInChildren<Renderer>();
        
        //랜덤 색상 지정
        if (photonView.IsMine)
        {
            playerColor = Random.ColorHSV();
        }

        bodyRenderer.material.color = playerColor;

        currentHp = MaxHp;

        if (hpSlider != null)
        {
            hpSlider.maxValue = MaxHp;
            hpSlider.value = currentHp;
        }
    }

    private void Update()
    {
        //모든 클라이언트가 할 내용
        if (photonView.IsMine == false)
            return;

        //소유권자만 할 내용
        Move();

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Fire();
        }

        if (hpSlider != null)
        {
            hpSlider.value = currentHp;
        }
    }

    public void Move()
    {
        Vector3 moveDir = new Vector3();
        moveDir.x = Input.GetAxisRaw("Horizontal");
        moveDir.z = Input.GetAxisRaw("Vertical");

        if (moveDir == Vector3.zero)
            return;

        transform.Translate(moveDir.normalized * speed * Time.deltaTime, Space.World);
        transform.forward = moveDir.normalized;
    }

    private void Fire()
    {
        photonView.RPC("FireRPC", RpcTarget.All, muzzlePoint.position, muzzlePoint.rotation);
    }

    [PunRPC]    
    private void FireRPC(Vector3 position, Quaternion rotation)
    {
        Instantiate(bulletPrefab, position, rotation);
    }

    public void TakeDamage(int damage)
    {
        currentHp -= damage;

        if (currentHp <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        currentHp = 0;

        //플레이어가 죽으면 모든 클라이언트에게 비활성화함
        photonView.RPC("DisablePlayer", RpcTarget.All);

        //코루틴 사용을 위한 GameScene 찾기
        GameScene scene = FindObjectOfType<GameScene>();
        if (scene != null)
        {
            //scene.RespawnPlayer(this);
        }
    }

    [PunRPC]
    private void DisablePlayer()
    {
        gameObject.SetActive(false);
    }

    [PunRPC]
    private void RespawnPlayer(Vector3 position)
    {
        transform.position = position;
        currentHp = MaxHp;
        gameObject.SetActive(true);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        //소유권자 일 때
        //변수 데이터를 보내는 경우
        if (stream.IsWriting)
        {
            stream.SendNext(bodyRenderer.material.color.r);
            stream.SendNext(bodyRenderer.material.color.g);
            stream.SendNext(bodyRenderer.material.color.b);

            stream.SendNext(currentHp);
            stream.SendNext(hpSlider.value);
        }

        //소유권자가 아닐 때 (받기만 함)
        //변수 데이터를 받는 경우
        //데이터를 보낼 때 1,2,3 순으로 보냈으면 받을 때도 1,2,3 으로 받아야 함
        else if (stream.IsReading)
        {
            Color color = new Color();
            color.r = (float)stream.ReceiveNext();
            color.g = (float)stream.ReceiveNext();
            color.b = (float)stream.ReceiveNext();

            bodyRenderer.material.color = color;

            currentHp = (int)stream.ReceiveNext();
            hpSlider.value = (float)stream.ReceiveNext();
        }
    }
}
