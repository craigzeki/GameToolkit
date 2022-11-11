using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

public static class FindMissingScripts
{
    [MenuItem("Zekster's Lab/Find Missing Scripts in Project")]
    static void FindMissingScriptsInProjectMenuItem()
    {
        string[] prefabPaths = AssetDatabase.GetAllAssetPaths().Where(path => path.EndsWith(".prefab", System.StringComparison.OrdinalIgnoreCase)).ToArray();
        foreach(string path in prefabPaths)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            foreach(Component component in prefab.GetComponentsInChildren<Component>())
            {
                if(component == null)
                {
                    Debug.Log("Prefab found with missing script " + path, prefab);
                    break;
                }
            }
        }
    }

    [MenuItem("Zekster's Lab/Remove All Missing Scripts in Project")]
    static void RemoveAllMissingScriptsInProjectMenuItem()
    {
        string[] prefabPaths = AssetDatabase.GetAllAssetPaths().Where(path => path.EndsWith(".prefab", System.StringComparison.OrdinalIgnoreCase)).ToArray();
        foreach (string path in prefabPaths)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            foreach (Component component in prefab.GetComponentsInChildren<Component>())
            {
                
                if (component == null)
                {
                    Debug.Log("Diabled missing script in prefab " + path, prefab);
                    GameObjectUtility.RemoveMonoBehavioursWithMissingScript(component.gameObject);
                    break;
                }
            }
        }
    }


    [MenuItem("Zekster's Lab/Find Missing Scripts in Scene")]
    static void FindMissingScriptsInSceneMenuItem()
    {
        foreach(GameObject gameObject in GameObject.FindObjectsOfType<GameObject>(true))
        {
            foreach(Component component in gameObject.GetComponentsInChildren<Component>())
            {
                if(component == null)
                {
                    Debug.Log("GameObject found with missing scripyt " + gameObject.name, gameObject);
                    break;
                }
            }
        }
    }
}
