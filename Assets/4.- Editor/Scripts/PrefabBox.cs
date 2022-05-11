using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PrefabBox : EditorWindow
{
    public List<GameObject> objectList = new List<GameObject>();
    public GameObject singleObject;

    private List<GUILayout> buttons;
    private bool clickInstant;

    private InputAction click;
    
    [MenuItem("Window/PrefabBox")]
    static void Init()
    {
        PrefabBox window = (PrefabBox)EditorWindow.GetWindow(typeof(PrefabBox));
    }

    private void OnGUI()
    {
        EditorGUILayout.BeginHorizontal();

        int cont = 0;
        foreach(GameObject t in objectList.ToArray())
        {
            if(t == null)
            {
                objectList.Remove(t);
            }
            else
            {
                GUILayout.Label(
                    AssetPreview.GetAssetPreview(t),
                    GUILayout.MinHeight(this.position.size.y / 5f),
                    GUILayout.MaxHeight(this.position.size.y / 5f),
                    GUILayout.MinWidth(this.position.size.x / 5f),
                    GUILayout.MaxWidth(this.position.size.x / 5f)
                );
                /*
                {
                    clickInstant = true;

                    DragAndDrop.PrepareStartDrag();
                    DragAndDrop.objectReferences = new GameObject[] { t };
                    DragAndDrop.StartDrag("test");
                }
                */
            }

            BlackMagic();

            cont++;

            if(cont % 5 == 0)
            {
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
            }
        }

        singleObject = (GameObject)EditorGUILayout.ObjectField(
            singleObject,
            typeof(GameObject),
            true,
            GUILayout.MinHeight(this.position.size.y / 5f),
            GUILayout.MaxHeight(this.position.size.y / 5f),
            GUILayout.MinWidth(this.position.size.x / 5f),
            GUILayout.MaxWidth(this.position.size.x / 5f)
        );

        if (singleObject != null)
        {
            if (PrefabUtility.GetCorrespondingObjectFromSource(singleObject) == null && PrefabUtility.GetPrefabObject(singleObject) != null)
            { 
                objectList.Add(singleObject);
            }

            singleObject = null;
        }

        EditorGUILayout.EndHorizontal();
    }

    private void BlackMagic()
    {
        if (Event.current.type == EventType.Layout) return;
        // Without this you cant get mouse up events dont know why, it also prevents unity to handle some inputs so dont handle layout events otherwise you can move the selected object but cant select a different one
        HandleUtility.AddDefaultControl(-1);

        var e = Event.current;

        // Prevents duplicate key and mouse events
        if (e.isMouse || e.isKey)
        {
            if (e.commandName == "Used")
            {
                return;
            }
            e.commandName = "Used";
        }

        if(e.type == EventType.MouseDrag || e.type == EventType.MouseDown)
        {
            Debug.Log("entra");
            Physics.Raycast(e.mousePosition, 

            //DragAndDrop.visualMode = DragAndDropVisualMode.Move;

            //DragAndDrop.StartDrag("test");
            clickInstant = false;
        }

        if (e.type == EventType.MouseUp)
        {
            Debug.Log("Mouse Up");
        }
    }
}
