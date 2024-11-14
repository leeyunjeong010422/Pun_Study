using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] Rigidbody rigid;
    [SerializeField] float speed;
    [SerializeField] int damage;
    [SerializeField] Renderer bulletRenderer;

    private void Start()
    {
        rigid.velocity = transform.forward * speed;
        Destroy(gameObject, 3f);
    }

    public void SetBulletColor(Color color)
    {
        bulletRenderer.material.color = color;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerController player = collision.gameObject.GetComponent<PlayerController>();
            TPS_PlayerController tps_PlayerController = collision.gameObject.GetComponent<TPS_PlayerController>();

            tps_PlayerController.TakeDamage(damage);
            //player.TakeDamage(damage);
        }

        Destroy(gameObject);
    }
}
