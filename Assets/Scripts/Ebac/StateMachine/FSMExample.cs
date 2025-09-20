using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ebac.StateMachine;

[RequireComponent(typeof(CharacterController))]
public class FSMExample : MonoBehaviour
{
    public enum ExampleEnum
    {
        STATE_IDLE,
        STATE_WALK,
        STATE_JUMP
    }

    [Header("Movimento")]
    public float moveSpeed = 4f;
    public float jumpSpeed = 8f;
    public float gravity = 20f;

    [HideInInspector] public CharacterController cc;
    [HideInInspector] public Vector3 velocity; // y e usado para pulo/gravidade

    public StateMachine<ExampleEnum> stateMachine;

    private void Start()
    {
        cc = GetComponent<CharacterController>();

        stateMachine = new StateMachine<ExampleEnum>();
        stateMachine.Init();

        // Registra estados com referencia a este controlador
        stateMachine.RegisterStates(ExampleEnum.STATE_IDLE, new IdleState(this));
        stateMachine.RegisterStates(ExampleEnum.STATE_WALK, new WalkState(this));
        stateMachine.RegisterStates(ExampleEnum.STATE_JUMP, new JumpState(this));

        // Comeca parado
        stateMachine.SwitchState(ExampleEnum.STATE_IDLE);
    }

    private void Update()
    {
        // Atualiza o estado atual
        stateMachine.Update();
    }
}
//
