using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TPS_PlayerController : MonoBehaviourPun, IPunObservable
{
    [SerializeField] float moveSpeed;
    [SerializeField] float rotationSpeed;
    [SerializeField] Transform muzzlePoint;
    [SerializeField] GameObject bulletPrefab;

    [SerializeField] const int MaxHp = 100;
    [SerializeField] int currentHp;

    [SerializeField] Camera playerCamera;
    [SerializeField] Vector3 cameraOffset = new Vector3(0, 5, -7); //ī�޶� �⺻ ��ġ (���� ����)
    [SerializeField] float mouseSensitivity = 2f; //����

    private Color playerColor;
    [SerializeField] Renderer bodyRenderer;

    private float cameraPitch; //ī�޶� ���Ʒ� ȸ�� ���� ������ �� ���
    private Vector3 playerPosition; //�÷��̾� ��ġ
    private Quaternion playerRotation; //�÷��̾� ȸ��

    [SerializeField] Slider hpSlider;

    private void Start()
    {
        bodyRenderer = GetComponentInChildren<Renderer>();

        //���� �÷��̾��� ��� ����
        if (photonView.IsMine)
        {
            playerColor = Random.ColorHSV();
            SetupLocalCamera();
        }

        bodyRenderer.material.color = playerColor;

        currentHp = MaxHp;

        if (hpSlider != null)
        {
            hpSlider.maxValue = MaxHp;
            hpSlider.value = currentHp;
        }
    }

    //���� �÷��̾��� ī�޶� ����
    private void SetupLocalCamera()
    {
        GameObject cameraPrefab = Resources.Load<GameObject>("PlayerCamera");

        if (cameraPrefab != null)
        {
            GameObject cameraObject = Instantiate(cameraPrefab);
            playerCamera = cameraObject.GetComponent<Camera>();
            cameraObject.transform.SetParent(transform);

            UpdateCameraPosition();
        }
    }

    private void Update()
    {
        //��� Ŭ���̾�Ʈ�� �� ����
        if (!photonView.IsMine)
        {
            //��ġ�� ȸ�� ����ȭ
            transform.position = Vector3.Lerp(transform.position, playerPosition, Time.deltaTime * 10);
            transform.rotation = Quaternion.Lerp(transform.rotation, playerRotation, Time.deltaTime * 10);
            return;
        }

        //�������ڸ� �� ����
        Move();
        RotateView();

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Fire();
        }

        if (hpSlider != null)
        {
            hpSlider.value = currentHp;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void Move()
    {
        Vector3 moveDir = new Vector3();
        moveDir.x = Input.GetAxisRaw("Horizontal");
        moveDir.z = Input.GetAxisRaw("Vertical");

        if (moveDir == Vector3.zero)
            return;

        Vector3 worldMoveDir = transform.TransformDirection(moveDir).normalized;
        transform.position += worldMoveDir * moveSpeed * Time.deltaTime;
    }

    private void RotateView()
    {
        if (!photonView.IsMine)
            return;

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        transform.Rotate(Vector3.up, mouseX * rotationSpeed * Time.deltaTime);

        cameraPitch -= mouseY;
        cameraPitch = Mathf.Clamp(cameraPitch, -10f, 20f); //ī�޶� ���Ʒ� ���� ����

        UpdateCameraPosition();
    }

    private void UpdateCameraPosition()
    {
        if (playerCamera != null)
        {
            playerCamera.transform.position = transform.position + Quaternion.Euler(cameraPitch, transform.eulerAngles.y, 0) * cameraOffset;
            playerCamera.transform.LookAt(transform.position + Vector3.up * 1.5f); //�÷��̾� �ణ ���� �ٶ�
        }
    }

    private void Fire()
    {
        Vector3 color = new Vector3(playerColor.r, playerColor.g, playerColor.b); //�÷��̾� ���� ���ͷ� ��ȯ
        photonView.RPC("FireRPC", RpcTarget.All, muzzlePoint.position, muzzlePoint.rotation, color);
    }

    [PunRPC]
    private void FireRPC(Vector3 position, Quaternion rotation, Vector3 colorVector)
    {
        GameObject bulletInstance = Instantiate(bulletPrefab, position, rotation);
        Bullet bulletScript = bulletInstance.GetComponent<Bullet>();

        Color color = new Color(colorVector.x, colorVector.y, colorVector.z);
        bulletScript.SetBulletColor(color); //�Ѿ��� ���� ����
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

        photonView.RPC("DisablePlayer", RpcTarget.All);

        GameScene scene = FindObjectOfType<GameScene>();
        if (scene != null)
        {
            scene.RespawnPlayer(this);
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

            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
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

            playerPosition = (Vector3)stream.ReceiveNext();
            playerRotation = (Quaternion)stream.ReceiveNext();
        }
    }
}
