using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[DefaultExecutionOrder(-105)] // Input system is -100
public class PlayerInputs : MonoBehaviour
{
    /*
    public bool controllerViewPow = false;
    public float pow = 2;
    public bool controllerViewSelfMult = false;
    public float selfMult = 0.2f;
    */

    private Inputs inputs;
    private static Inputs.GameplayActions actions;

    public static Vector2 Movement { get; private set; }
    public static bool Fire => actions.Fire.WasPressedThisFrame();
    public static bool FireHeld { get; private set; }
    public static bool ADS { get; private set; }
    public static bool Reload => actions.Reload.WasPressedThisFrame();
    public static bool Crouch { get; private set; }
    public static bool Sprint { get; private set; }
    public static float Lean { get; private set; }
    public static bool AttachBarrel => actions.AttachBarrel.WasPressedThisFrame();
    public static bool AttachSight => actions.AttachSight.WasPressedThisFrame();
    public static bool AttachMisc => actions.AttachMisc.WasPressedThisFrame();
    public static bool SwapWeapons => actions.SwapWeapons.WasPressedThisFrame();
    public static bool Jump => actions.Jump.WasPressedThisFrame();
    public static Vector2 Look { get; private set; }
    public static bool Interact => actions.Interact.WasPressedThisFrame();


    private void Awake()
    {
        inputs = new Inputs();
        actions = inputs.Gameplay;

        actions.Movement.performed += ctx => Movement = ctx.ReadValue<Vector2>();
        actions.Movement.canceled += ctx => Movement = Vector2.zero;

        actions.Fire.started += ctx => FireHeld = true;
        actions.Fire.canceled += ctx => FireHeld = false;

        actions.ADS.started += ctx => ADS = true;
        actions.ADS.canceled += ctx => ADS = false;

        actions.Crouch.started += ctx => Crouch = true;
        actions.Crouch.canceled += ctx => Crouch = false;

        actions.Sprint.started += ctx => Sprint = true;
        actions.Sprint.canceled += ctx => Sprint = false;

        actions.Lean.performed += ctx => Lean = ctx.ReadValue<float>();
        actions.Lean.canceled += ctx => Lean = 0f;

        actions.Look.performed += ctx => EvaluateLookVector(ctx.ReadValue<Vector2>());
        actions.Look.canceled += ctx => Look = Vector2.zero;

        //actions.Interact.performed += ctx => Interact = true;
    }

    private void OnEnable()
    {
        inputs.Gameplay.Enable();
    }

    private void OnDisable()
    {
        inputs.Gameplay.Disable();
    }

    private void EvaluateLookVector(Vector2 value)
    {
        //Look = value;
        /*
        if (controllerViewPow || controllerViewSelfMult)
        {
            value.x = EvaluateComp(value.x);
            value.y = EvaluateComp(value.y);
        }
        */

        Look = value;
    }

    /*
    private float EvaluateComp(float comp)
    {
        float sign = Mathf.Sign(comp);
        comp = Mathf.Abs(comp);

        if (controllerViewPow)
            comp = Mathf.Pow(comp, pow);
        if (controllerViewSelfMult)
            comp *= selfMult * comp;

        return comp * sign;
    }
    */
}
