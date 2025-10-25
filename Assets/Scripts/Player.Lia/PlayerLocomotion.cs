using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerLocomotion : MonoBehaviour
{
    public float speed = 3f;
    public Animator animator;

    CharacterController controller;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 move = new Vector3(horizontal, 0, vertical);
        controller.SimpleMove(move * speed);

        // Atualiza parâmetro do Animator
        float velocity = move.magnitude;
        animator.SetFloat("Speed", velocity);
    }
}
