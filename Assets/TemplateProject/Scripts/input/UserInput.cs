using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem; // CHANGED: nodig voor InputAction.CallbackContext, gebruikt door de named handler hieronder

public class UserInput : MonoBehaviour
{
    public static UserInput instance;
    [HideInInspector] public Controls controls;
    [HideInInspector] public Vector2 moveInput;

    private void Awake()
    {
        // CHANGED: singleton-check herschreven. De originele code maakte "controls = new Controls()"
        // aan in de "else"-tak, vlak voordat het GameObject werd vernietigd (dode code, want het object
        // wordt toch weggegooid), en overschreef die daarna sowieso opnieuw onderaan, ongeacht welke tak
        // gekozen werd. Zo lekte er bij een dubbele instance kortstondig een ongebruikt Controls-object.
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return; // CHANGED: hier stoppen, geen zin om controls op te zetten voor een object dat vernietigd wordt
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        controls = new Controls();

        // CHANGED: inline lambda vervangen door een named method (OnMove), zodat deze hieronder
        // netjes uitgeschreven kan worden in OnDestroy — dit voorkomt een memory/event leak.
        controls.Movement.Move.performed += OnMove;
    }

    private void OnMove(InputAction.CallbackContext ctx)
    {
        moveInput = ctx.ReadValue<Vector2>();
    }

    private void OnEnable()
    {
        controls.Enable();
    }

    private void OnDisable()
    {
        controls.Disable();
    }

    // CHANGED: toegevoegd — zonder dit werden de event-subscriptie en het onbeheerde
    // Controls/InputActionAsset object nooit vrijgegeven.
    private void OnDestroy()
    {
        if (controls == null) return;
        controls.Movement.Move.performed -= OnMove;
        controls.Dispose();
    }
}
