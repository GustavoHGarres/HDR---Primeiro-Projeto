using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ebac.StateMachine;

// Base comum para estados com acesso ao controlador
public abstract class ExampleStateBase : StateBase
{
    protected readonly FSMExample ctrl;
    protected ExampleStateBase(FSMExample ctrl) { this.ctrl = ctrl; }

    protected float GetInputX() => Input.GetAxisRaw("Vertical");

    // Helper: aplica gravidade e move
    protected void ApplyGravityAndMove(Vector3 vertical)
    {
        // “Apoiado” no chão: mantém y levemente negativo para colar no chão
        if (ctrl.cc.isGrounded && ctrl.velocity.y < 0f)
            ctrl.velocity.y = -2f;

        ctrl.velocity.y -= ctrl.gravity * Time.deltaTime;

        //Vector3 final = horizontal;
        Vector3 final = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        final.y = ctrl.velocity.y;

        ctrl.cc.Move(final * Time.deltaTime);
    }
}

public class IdleState : ExampleStateBase
{
    public IdleState(FSMExample c) : base(c) { }

    public override void OnStateEnter(params object[] objs) { /* opcional */ }

    public override void OnStateStay()
    {
        float x = GetInputX();

        // Transições
        if (Mathf.Abs(x) > 0.01f) {
            ctrl.stateMachine.SwitchState(FSMExample.ExampleEnum.STATE_WALK);
            return;
        }

        if (ctrl.cc.isGrounded && Input.GetButtonDown("Jump")) {
            ctrl.stateMachine.SwitchState(FSMExample.ExampleEnum.STATE_JUMP);
            return;
        }

        // Sem deslocamento horizontal
        ApplyGravityAndMove(Vector3.zero);
    }
}

public class WalkState : ExampleStateBase
{
    public WalkState(FSMExample c) : base(c) { }

    public override void OnStateStay()
    {
        float x = GetInputX();

        // Transições
        if (Mathf.Abs(x) <= 0.01f) {
            ctrl.stateMachine.SwitchState(FSMExample.ExampleEnum.STATE_IDLE);
            return;
        }
        if (ctrl.cc.isGrounded && Input.GetButtonDown("Jump")) {
            ctrl.stateMachine.SwitchState(FSMExample.ExampleEnum.STATE_JUMP);
            return;
        }

        Vector3 move = new Vector3(x, 0f, 0f) * ctrl.moveSpeed;
        ApplyGravityAndMove(move);
    }
}

public class JumpState : ExampleStateBase
{
    public JumpState(FSMExample c) : base(c) { }

    public override void OnStateEnter(params object[] objs)
    {
        if (ctrl.cc.isGrounded)
            ctrl.velocity.y = ctrl.jumpSpeed;
    }

    public override void OnStateStay()
    {
        float x = GetInputX();
        Vector3 move = new Vector3(x, 0f, 0f) * ctrl.moveSpeed;

        ApplyGravityAndMove(move);

        // Ao tocar o chão, decide: Idle ou Walk
        if (ctrl.cc.isGrounded && ctrl.velocity.y <= 0f)
        {
            if (Mathf.Abs(x) > 0.01f)
                ctrl.stateMachine.SwitchState(FSMExample.ExampleEnum.STATE_WALK);
            else
                ctrl.stateMachine.SwitchState(FSMExample.ExampleEnum.STATE_IDLE);
        }
    }
}