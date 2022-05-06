using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.InputSystem;
using UnityEngine.ResourceManagement.AsyncOperations;

public class CubesAddressablesLevelController : MonoBehaviour
{
    public float cubesDistance = 2f;
    public int maxCubes = 5;
    public Material[] materialsList;

    private Queue<GameObject> instantiatedCubes = new Queue<GameObject>();
    private int lastColor = -1;
    private int auxRandom;
    private GameObject auxGameObject;

    public void _InstantiateCube(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            
        }
        else if (context.performed)
        {
            if(instantiatedCubes.Count == maxCubes)
            {
                auxRandom = GetRandomColor(instantiatedCubes.Peek().GetComponent<Renderer>().sharedMaterial);
            }
            else
            {
                auxRandom = GetRandomColor(null);
            }

            switch (auxRandom)
            {
                case 0:
                    Addressables.InstantiateAsync("Cube Red", this.transform.position, Quaternion.identity, this.transform).Completed += OnCubeInstanced;
                    break;
                case 1:
                    Addressables.InstantiateAsync("Cube Green", this.transform.position, Quaternion.identity, this.transform).Completed += OnCubeInstanced;
                    break;
                case 2:
                    Addressables.InstantiateAsync("Cube Blue", this.transform.position, Quaternion.identity, this.transform).Completed += OnCubeInstanced;
                    break;
            }
        }
        else if (context.canceled)
        {
            
        }
    }

    private void OnCubeInstanced(AsyncOperationHandle<GameObject> obj)
    {
        instantiatedCubes.Enqueue(obj.Result);
        if(instantiatedCubes.Count <= maxCubes)
        {
            obj.Result.transform.position = Vector3.left * 2f * (maxCubes - 1) / 2f + Vector3.right * 2f * (instantiatedCubes.Count - 1);
        }
        else
        {
            auxGameObject = instantiatedCubes.Dequeue();
            obj.Result.transform.position = auxGameObject.transform.position;
            Addressables.ReleaseInstance(auxGameObject);
        }
    }

    public void _ChangeColors(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            
        }
        else if (context.performed)
        {
            StartCoroutine(ChangeAllColors());
        }
        else if (context.canceled)
        {
            
        }
    }

    private IEnumerator ChangeAllColors()
    {
        for (int i = 0; i < instantiatedCubes.Count; i++)
        {
            auxGameObject = instantiatedCubes.Dequeue();
            auxGameObject.gameObject.GetComponent<Renderer>().material = materialsList[GetRandomColor(auxGameObject.gameObject.GetComponent<Renderer>().sharedMaterial)];
            instantiatedCubes.Enqueue(auxGameObject);

            yield return new WaitForSeconds(0.2f);
        }
    }

    private int GetRandomColor(Material actualMaterial)
    {
        if(actualMaterial != null && actualMaterial != materialsList[lastColor])
        {
            auxRandom = materialsList.ToList<Material>().FindIndex(mat => materialsList.ToList<Material>().IndexOf(mat) != lastColor && mat != actualMaterial);
        }
        else
        {
            if (lastColor == -1)
            {
                auxRandom = UnityEngine.Random.Range(0, 3);
            }
            else
            {
                auxRandom = UnityEngine.Random.Range(0, 2);

                if (auxRandom >= lastColor)
                {
                    auxRandom++;
                }
            }
        }

        lastColor = auxRandom;
        return auxRandom;
    }

    private string NumberToColor(int value)
    {
        switch(value)
        {
            case 0: return "Red";
            case 1: return "Green";
            case 2: return "Blue";
            default: return "Error";
        }
    }
}