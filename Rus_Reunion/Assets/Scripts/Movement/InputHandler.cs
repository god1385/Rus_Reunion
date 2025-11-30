using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class InputHandler : MonoBehaviour
{
    [SerializeField] private BaseCharacterController characterController;

    private Vector2 moveDirection;
    private InputAction moveAction;
    void Start()
    {
        moveAction = InputSystem.actions.FindAction("Move");
        InputSystem.actions.FindAction("Sprint").performed += context => characterController.ChangeSpeed(true);
        InputSystem.actions.FindAction("Sprint").canceled += context => characterController.ChangeSpeed(false);
    }

    private void FixedUpdate()
    {
        moveDirection = moveAction.ReadValue<Vector2>();
        characterController.MoveCharacter(new Vector3(moveDirection.x, 0, moveDirection.y));
    }
}
