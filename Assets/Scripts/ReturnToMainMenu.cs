using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class ReturnToMainMenu : MonoBehaviour
{
    private void Update()
    {
        if(Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            _GoToMainMenu();
        }
    }

    public void _GoToMainMenu()
    {
        SceneManager.LoadScene("3. UI");
    }
}
