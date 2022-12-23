using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

public class InputSystemTest : MonoBehaviour
{

    public InputAction action1;
    public InputAction action2;

    MyCustomInputAction myCustomInput;

    private void Awake()
    {
        EnhancedTouchSupport.Enable();

        action1.performed += _ => { Debug.Log("Mouse Left");};
       

        action2.performed += _ => { Debug.Log("Mouse Right"); };
       
    }

    private void OnEnable()
    {
        action1.Enable();
        action2.Enable();


        myCustomInput = new MyCustomInputAction();

        myCustomInput.Test1.Move.performed += _ => { Debug.Log("Move(MycustomInput)"); };
        myCustomInput.Test1.Move.Enable();


        //var accelerateAction = new InputAction("Accelerate");
        //accelerateAction.AddCompositeBinding("Axis")
        //    .With("Positive", "<Gamepad>/rightTrigger")
        //    .With("Negative", "<Gamepad>/leftTrigger");
       
    }

    private void OnDisable()
    {
        action1.Disable();
        action2.Disable();
    }
    // Start is called before the first frame update
    void Start()
    {
        //var action = new InputAction(binding: "*/{PrimaryAction}" ,modifiers:"holde") ;
        //action.performed += _ => Fire();
        //action.Enable();
    }

    private void Fire()
    {

    }

    // Update is called once per frame
    void Update()
    {
        //var keyboard = Keyboard.current;
        //if (keyboard == null)
        //    return;

        //if (keyboard.aKey.isPressed)
        //{
        //    Debug.Log($"AKey was pressed!");
        //}


        //var mouse = Mouse.current;

        //if (mouse == null)
        //    return;

        //Vector2 move = mouse.position.ReadValue();

        //Debug.Log($"Mouse postion =={move}");

        foreach (var touch in Touch.activeTouches)
        {
            Debug.Log($"{touch.touchId}:{touch.screenPosition}{touch.phase}");
        }

    }

    public void MoveTest(InputAction.CallbackContext context)
    {
        Vector2 res = context.ReadValue<Vector2>();
        Debug.Log($"res =={res}");
    }
}
