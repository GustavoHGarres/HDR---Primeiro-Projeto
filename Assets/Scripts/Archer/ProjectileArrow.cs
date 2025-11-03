using UnityEngine;

public class ProjectileArrow : MonoBehaviour
{
    [Header("Configurações")]
    public float speed = 28f;
    public float lifeTime = 6f;
    public float damage = 10f;
    public bool useGravity = false;          // seta reta por padrão
    public bool rotateToVelocity = true;     // gira para olhar para onde voa

    Rigidbody rb;
    Collider myCol;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        myCol = GetComponent<Collider>();

        // Física estável para projétil
        rb.useGravity = useGravity;
        rb.isKinematic = false;
        rb.drag = 0f;
        rb.angularDrag = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    // Chamada pelo inimigo na hora do evento de animação
    public void Launch(Vector3 direction, Collider[] ignoreColliders = null)
    {
        direction.y = 0f; // evita mirar no chão caso o spawn esteja inclinado
        direction = direction.sqrMagnitude > 0.0001f ? direction.normalized : transform.forward;

        // Alinha a seta com a direção inicial
        transform.rotation = Quaternion.LookRotation(direction);

        // Ignora colisão com quem disparou
        if (ignoreColliders != null && myCol != null)
            foreach (var c in ignoreColliders)
                if (c) Physics.IgnoreCollision(myCol, c, true);

        // Velocidade direta (não força) para sair “já voando”
        rb.velocity = direction * speed;

        Destroy(gameObject, lifeTime);
    }

    void FixedUpdate()
    {
        if (rotateToVelocity && rb.velocity.sqrMagnitude > 0.01f)
            transform.rotation = Quaternion.LookRotation(rb.velocity);
    }

    void OnTriggerEnter(Collider other)
    {
        // Ex.: aplicar dano aqui se tiver HealthBase
        // var h = other.GetComponent<HealthBase>();
        // if (h) h.Damage(damage);

        Destroy(gameObject);
    }
}
