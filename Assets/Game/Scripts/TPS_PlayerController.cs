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
    [SerializeField] Vector3 cameraOffset = new Vector3(0, 5, -7); //카메라 기본 위치 (임의 설정)
    [SerializeField] float mouseSensitivity = 2f; //감도

    private Color playerColor;
    [SerializeField] Renderer bodyRenderer;

    private float cameraPitch; //카메라 위아래 회전 각도 설정할 때 사용
    private Vector3 playerPosition; //플레이어 위치
    private Quaternion playerRotation; //플레이어 회전

    [SerializeField] Slider hpSlider;

    private void Start()
    {
        bodyRenderer = GetComponentInChildren<Renderer>();

        //로컬 플레이어인 경우 실행
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

    //로컬 플레이어의 카메라를 설정
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
        //모든 클라이언트가 할 내용
        if (!photonView.IsMine)
        {
            //위치와 회전 동기화
            transform.position = Vector3.Lerp(transform.position, playerPosition, Time.deltaTime * 10);
            transform.rotation = Quaternion.Lerp(transform.rotation, playerRotation, Time.deltaTime * 10);
            return;
        }

        //소유권자만 할 내용
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
        cameraPitch = Mathf.Clamp(cameraPitch, -10f, 20f); //카메라 위아래 각도 제한

        UpdateCameraPosition();
    }

    private void UpdateCameraPosition()
    {
        if (playerCamera != null)
        {
            playerCamera.transform.position = transform.position + Quaternion.Euler(cameraPitch, transform.eulerAngles.y, 0) * cameraOffset;
            playerCamera.transform.LookAt(transform.position + Vector3.up * 1.5f); //플레이어 약간 위를 바라봄
        }
    }

    private void Fire()
    {
        Vector3 color = new Vector3(playerColor.r, playerColor.g, playerColor.b); //플레이어 색상 벡터로 변환
        photonView.RPC("FireRPC", RpcTarget.All, muzzlePoint.position, muzzlePoint.rotation, color);
    }

    [PunRPC]
    private void FireRPC(Vector3 position, Quaternion rotation, Vector3 colorVector)
    {
        GameObject bulletInstance = Instantiate(bulletPrefab, position, rotation);
        Bullet bulletScript = bulletInstance.GetComponent<Bullet>();

        Color color = new Color(colorVector.x, colorVector.y, colorVector.z);
        bulletScript.SetBulletColor(color); //총알의 색상 설정
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
        //소유권자 일 때
        //변수 데이터를 보내는 경우
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

            playerPosition = (Vector3)stream.ReceiveNext();
            playerRotation = (Quaternion)stream.ReceiveNext();
        }
    }
}
