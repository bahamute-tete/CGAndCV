using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ControlTest : MonoBehaviour
{
    public InputAction action;
    public InputActionMap PlayerInput;
    float value;
    public float speed = 1f;

    public InputAction moveAction;

    public InputAction myAction;
    private void Awake()
    {

            myAction.AddCompositeBinding("Axis") // Or just "Axis"
            .With("Positive", "<Gamepad>/rightTrigger")
            .With("Negative", "<Gamepad>/leftTrigger");


        moveAction = new InputAction("move", binding: "<Gamepad>/rightStick");
        moveAction.AddCompositeBinding("2DVector(mode=2)")//or Dpad   // To set mode (2=analog, 1=digital, 0=digitalNormalized):
                        .With("Up", "<Keyboard>/w")
                        .With("Down", "<Keyboard>/s")
                        .With("Left", "<Keyboard>/a")
                        .With("Right", "<Keyboard>/d");
        //action = new InputAction("Move",InputActionType.Value);
        //action.AddCompositeBinding("2D")
        //    .With("left", "<Keyboard>/a")
        //    .With("Right", "<Keyboard>/d")
        //    .With("Up", "<Keyboard>/w")
        //    .With("Down", "<Keyboard>/s");

        PlayerInput = new InputActionMap("Player");
        PlayerInput.AddAction("look", InputActionType.Value, "<Gamepad>/leftStick");
    }
    // Start is called before the first frame update
    void Start()
    {
        //action.performed += MoveObj;
        action.Enable();
    }

    private void MoveObj(InputAction.CallbackContext ctx)
    {
        Vector2 vector2 = ctx.ReadValue<Vector2>();
        transform.position += new Vector3(vector2.x,0,vector2.y) * speed*Time.deltaTime;

    }

    // Update is called once per frame
    void Update()
    {

        
        Vector2 vector2 = action.ReadValue<Vector2>();
        transform.position += new Vector3(vector2.x, 0, vector2.y) * speed * Time.deltaTime;

        
    }
}
