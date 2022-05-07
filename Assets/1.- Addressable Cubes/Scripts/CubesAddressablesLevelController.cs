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
    public AssetReferenceT<Material> redMaterial, greenMaterial, blueMaterial;

    private Queue<GameObject> instantiatedCubes = new Queue<GameObject>();

    private int auxRandom;
    private GameObject auxGameObject;

    enum MaterialColors { UNIDENTIFIED_MATERIAL = -1, RED_MATERIAL = 0, GREEN_MATERIAL, BLUE_MATERIAL };
    private MaterialColors lastMaterial = MaterialColors.UNIDENTIFIED_MATERIAL;

    private AsyncOperationHandle handleRedMaterial, handleGreenMaterial, handleBlueMaterial;
    List<MaterialColors> materialOptions = new List<MaterialColors>();

    private IEnumerator Start()
    {
        handleRedMaterial = redMaterial.LoadAssetAsync<Material>();
        yield return handleRedMaterial;

        handleGreenMaterial = greenMaterial.LoadAssetAsync<Material>();
        yield return handleGreenMaterial;

        handleBlueMaterial = blueMaterial.LoadAssetAsync<Material>();
        yield return handleBlueMaterial;
    }

    private void OnDestroy()
    {
        while(instantiatedCubes.Count > 0)
        {
            Addressables.ReleaseInstance(instantiatedCubes.Dequeue());
        }

        Addressables.Release(handleRedMaterial);
        Addressables.Release(handleGreenMaterial);
        Addressables.Release(handleBlueMaterial);
    }

    public void _InstantiateCube(InputAction.CallbackContext context)
    {
        if (context.started)
            return;
        else if (context.performed)
            InstantiateCube();
        else if (context.canceled)
            return;
    }

    private void InstantiateCube()
    {
        if (instantiatedCubes.Count == maxCubes)
        {
            auxRandom = (int)GetValidMaterial(SharedMaterialToMaterialColors((instantiatedCubes.Peek().GetComponent<Renderer>().sharedMaterial)));
        }
        else
        {
            auxRandom = (int)GetValidMaterial(MaterialColors.UNIDENTIFIED_MATERIAL);
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

    private void OnCubeInstanced(AsyncOperationHandle<GameObject> obj)
    {
        instantiatedCubes.Enqueue(obj.Result);
        if(instantiatedCubes.Count <= maxCubes)
        {
            obj.Result.transform.localPosition = Vector3.left * 2f * (maxCubes - 1) / 2f + Vector3.right * 2f * (instantiatedCubes.Count - 1);
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
            return;
        else if (context.performed)
            ChangeAllColors();
        else if (context.canceled)
            return;
    }

    private void ChangeAllColors()
    {
        for (int i = 0; i < instantiatedCubes.Count; i++)
        {
            auxGameObject = instantiatedCubes.Dequeue();
            auxGameObject.gameObject.GetComponent<Renderer>().material = MaterialColorsToRealMaterial(GetValidMaterial(SharedMaterialToMaterialColors(auxGameObject.gameObject.GetComponent<Renderer>().sharedMaterial)));
            instantiatedCubes.Enqueue(auxGameObject);
        }        
    }

    private MaterialColors SharedMaterialToMaterialColors(Material sharedMaterial)
    {
        if(sharedMaterial == (Material)handleRedMaterial.Result)
        {
            return MaterialColors.RED_MATERIAL;
        }
        else if(sharedMaterial == (Material)handleGreenMaterial.Result)
        {
            return MaterialColors.GREEN_MATERIAL;
        }
        else if(sharedMaterial == (Material)handleBlueMaterial.Result)
        {
            return MaterialColors.BLUE_MATERIAL;
        }

        return MaterialColors.UNIDENTIFIED_MATERIAL;
    }

    private MaterialColors GetValidMaterial(MaterialColors actualMaterial)
    {
        materialOptions.Clear();

        if(actualMaterial != MaterialColors.RED_MATERIAL && lastMaterial != MaterialColors.RED_MATERIAL)
        {
            materialOptions.Add(MaterialColors.RED_MATERIAL);
        }

        if (actualMaterial != MaterialColors.GREEN_MATERIAL && lastMaterial != MaterialColors.GREEN_MATERIAL)
        {
            materialOptions.Add(MaterialColors.GREEN_MATERIAL);
        }

        if (actualMaterial != MaterialColors.BLUE_MATERIAL && lastMaterial != MaterialColors.BLUE_MATERIAL)
        {
            materialOptions.Add(MaterialColors.BLUE_MATERIAL);
        }

        lastMaterial = materialOptions[UnityEngine.Random.Range(0, materialOptions.Count)];

        return lastMaterial;
    }

    private Material MaterialColorsToRealMaterial(MaterialColors materialColor)
    {
        switch(materialColor)
        {
            case MaterialColors.RED_MATERIAL:
                return (Material)handleRedMaterial.Result;
            case MaterialColors.GREEN_MATERIAL:
                return (Material)handleGreenMaterial.Result;
            case MaterialColors.BLUE_MATERIAL:
                return (Material)handleBlueMaterial.Result;
            default:
                Debug.LogError("ERROR, No se puede determinar a que color se asocia el valor pasado: " + materialColor + ", se devuelve rojo por defecto");
                return (Material)handleRedMaterial.Result;
        }
    }

    /*
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
    */
}