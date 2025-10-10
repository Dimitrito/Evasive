using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 20f;      // скорость пули
    public float lifeTime = 3f;    // время жизни
    public int damage = 10;        // урон

    void Start()
    {
        Destroy(gameObject, lifeTime); // уничтожаем через заданное время
    }

    void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Игрок получил " + damage + " урона!");
        }

        Destroy(gameObject);
    }
}