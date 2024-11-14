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
        
        //���� ���� ����
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
        //��� Ŭ���̾�Ʈ�� �� ����
        if (photonView.IsMine == false)
            return;

        //�������ڸ� �� ����
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

        //�÷��̾ ������ ��� Ŭ���̾�Ʈ���� ��Ȱ��ȭ��
        photonView.RPC("DisablePlayer", RpcTarget.All);

        //�ڷ�ƾ ����� ���� GameScene ã��
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
        //�������� �� ��
        //���� �����͸� ������ ���
        if (stream.IsWriting)
        {
            stream.SendNext(bodyRenderer.material.color.r);
            stream.SendNext(bodyRenderer.material.color.g);
            stream.SendNext(bodyRenderer.material.color.b);

            stream.SendNext(currentHp);
            stream.SendNext(hpSlider.value);
        }

        //�������ڰ� �ƴ� �� (�ޱ⸸ ��)
        //���� �����͸� �޴� ���
        //�����͸� ���� �� 1,2,3 ������ �������� ���� ���� 1,2,3 ���� �޾ƾ� ��
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
