using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class UIController : MonoBehaviour
{
    public GameObject onGameMenu, onEditorMenu;
    void Awake()
    {
        onEditorMenu.SetActive(false);
        onGameMenu.SetActive(false);

#if UNITY_EDITOR
        onEditorMenu.SetActive(true);
#else
        onGameMenu.SetActive(true);
#endif
    }
}
