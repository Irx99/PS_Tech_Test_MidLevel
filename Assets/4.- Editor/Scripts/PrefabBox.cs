#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class PrefabBox : EditorWindow
{
    public int box1DSize = 10;

    private struct Element
    {
        public Element(GameObject prefab, VisualElement box)
        {
            this.prefab = prefab;
            this.box = box;
        }

        public GameObject prefab;
        public VisualElement box;
    }

    private List<Element> prefabList = new List<Element>();
    private GameObject prefabSelected;

    private VisualElement body;
    private VisualElement[] container;

    [MenuItem("Window/PrefabBox")]
    private static void ShowWindow()
    {
        GetWindow<PrefabBox>("MyWindow");
    }

    private void OnEnable()
    {
        BuildVisuals();
    }

    private void BuildVisuals()
    {
        body = new VisualElement();
        body.style.flexGrow = 1f;
        body.style.flexDirection = FlexDirection.Column;

        body.RegisterCallback<DragUpdatedEvent>(evt =>
        {
            DragAndDrop.visualMode = DragAndDropVisualMode.Move;
        });

        body.RegisterCallback<DragExitedEvent>(evt =>
        {
            EndDragInside();
        });

        rootVisualElement.Add(body);

        container = new VisualElement[box1DSize];
        for (int i = 0; i < box1DSize; i++)
        {
            container[i] = new VisualElement();
            container[i].style.flexGrow = 1f / box1DSize;
            container[i].style.flexDirection = FlexDirection.Row;
            body.Add(container[i]);
        }

        for (int i = 0; i < prefabList.Count; i++)
        {
            CreateNewElement(prefabList[i].prefab, i, true);
        }
    }

    private void StartDragInside()
    {
        if (prefabSelected != null)
        {
            DragAndDrop.PrepareStartDrag();
            DragAndDrop.StartDrag("Dragging");
            DragAndDrop.objectReferences = new Object[] { prefabSelected };
        }
    }

    private void EndDragInside()
    {
        {
            foreach(Object prefab in DragAndDrop.objectReferences)
            {
                if(prefab.GetType() == typeof(GameObject))
                {
                    GameObject prefabGO = (GameObject)prefab;
                    
                    // Para que solo admita prefabs
                    // Esta obsoleto pero es funcional, y se me come el tiempo asi que asi se queda
                    if(PrefabUtility.GetPrefabParent(prefabGO) == null && PrefabUtility.GetPrefabObject(prefabGO) != null)
                    {
                        if(prefabGO.scene.name == null)
                        {
                            CreateNewElement(prefabGO, prefabList.Count);
                        }
                    }
                }
            }

            DragAndDrop.objectReferences = null;
        }
    }

    private void CreateNewElement(GameObject prefab, int position, bool reseting = false)
    {
        if (prefabList.Any(pl => pl.prefab == prefab) && !reseting)
            return;

        if (position / 10 > 9)
            return;

        var box = new VisualElement();
        box.style.backgroundImage = AssetPreview.GetAssetPreview(prefab) != null ? AssetPreview.GetAssetPreview(prefab) : PrefabUtility.GetIconForGameObject(prefab);
        box.style.flexGrow = 0.1f;
        box.style.color = Color.red;
        box.tooltip = prefab.name;

        container[position/10].Add(box);

        box.RegisterCallback<MouseDownEvent>(evt => 
        {
            if(evt.button == 0)
            {
                prefabSelected = prefab;
                StartDragInside();
            }

            if(evt.button == 2)
            {
                RemoveElement(prefab);
            }
        });

        box.RegisterCallback<DragUpdatedEvent>(evt =>
        {
            if (DragAndDrop.objectReferences.Length > 0)
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Move;
            }
        });

        if(!reseting)
        {
            prefabList.Add(new Element(prefab, box));
        }
    }

    private void RemoveElement(GameObject prefab)
    {
        Element aux = prefabList.Find(pl => pl.prefab == prefab);
        prefabList.Remove(aux);

        rootVisualElement.Remove(body);

        BuildVisuals();
    }
}
#endif