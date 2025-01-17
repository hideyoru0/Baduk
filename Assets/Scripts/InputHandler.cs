using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputHandler : MonoBehaviour
{
    [SerializeField] Camera mainCamera;
    [SerializeField] GameManager gameManager;

    public void OnClick(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            HandleClick();
        }
    }

    void HandleClick()
    {
        Vector2 inputPosition;
        if (Mouse.current != null && Mouse.current.leftButton.isPressed)
        {
            inputPosition = Mouse.current.position.ReadValue();
        }
        else if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
        {
            inputPosition = Touchscreen.current.primaryTouch.position.ReadValue();
        }
        else
        {
            return;
        }

        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(new Vector3(inputPosition.x, inputPosition.y, 0));

        RaycastHit2D hit = Physics2D.Raycast(worldPosition, Vector2.zero);
        if (hit.collider != null)
        {
            BoardCell cell = hit.collider.GetComponent<BoardCell>();
            if (cell != null)
            {
                gameManager.OnCellClicked(cell);
            }
        }
    }
}
