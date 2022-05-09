using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    public Animator UIAnimator;

    public List<Button> topButtons, bottomButtons;

    private bool isOnTop = true;

    private void Start()
    {
        if (isOnTop)
        {
            foreach (Button button in topButtons)
            {
                button.interactable = true;
            }
            topButtons[0].Select();

            foreach (Button button in bottomButtons)
            {
                button.interactable = false;
            }
        }
        else
        {
            foreach (Button button in topButtons)
            {
                button.interactable = false;
            }

            foreach (Button button in bottomButtons)
            {
                button.interactable = true;
            }
            bottomButtons[0].Select();
        }
    }

    public void _ChangeOptions(InputAction.CallbackContext context)
    {
        if (context.started)
            return;
        else if (context.performed)
            TriggerChangeOptions();
        else if (context.canceled)
            return;
    }

    private void TriggerChangeOptions()
    {
        UIAnimator.SetTrigger("changeOptions");
    }

    public void _TransitionBegins()
    {
        foreach(Button button in topButtons)
        {
            button.interactable = false;
        }

        foreach (Button button in bottomButtons)
        {
            button.interactable = false;
        }

        isOnTop = !isOnTop;
    }

    public void _TransitionEnds()
    {
        if (isOnTop)
        {
            foreach (Button button in topButtons)
            {
                button.interactable = true;
            }

            topButtons[0].Select();
        }
        else
        {
            foreach (Button button in bottomButtons)
            {
                button.interactable = true;
            }
            bottomButtons[0].Select();
        }
    }
}
