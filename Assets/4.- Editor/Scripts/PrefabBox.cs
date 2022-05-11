using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class PrefabBox : EditorWindow
{
    private GameObject prefab;

 

    [MenuItem("Window/MyWindow")]
    private static void ShowWindow()
    {
        GetWindow<PrefabBox>("MyWindow");
    }

    private void OnEnable()
    {
        var box = new VisualElement();
        box.style.backgroundColor = Color.gray;
        box.style.flexGrow = 1f;

        box.RegisterCallback<MouseDownEvent>(evt =>
        {
            DragAndDrop.PrepareStartDrag();
            DragAndDrop.StartDrag("Dragging");
            if(prefab != null)
            {
                DragAndDrop.objectReferences = new Object[] { prefab };
            }
        });

        box.RegisterCallback<DragUpdatedEvent>(evt =>
        {
            DragAndDrop.visualMode = DragAndDropVisualMode.Move;
        });

        box.RegisterCallback<DragExitedEvent>(evt =>
        {
            prefab = (GameObject)DragAndDrop.objectReferences[0];
        });

        rootVisualElement.Add(box);
    }
}
