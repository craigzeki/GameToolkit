using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropSpawnMarker : MonoBehaviour
{
    [SerializeField] private GameObject spawnMarkerPrefab;
    private GameObject spawnMarker;
    private void Start()
    {
        if (spawnMarkerPrefab == null) return;

        Vector3 pos;

        pos = transform.position;
        pos.y = 0.015f;

        spawnMarker = Instantiate(spawnMarkerPrefab, pos, Quaternion.identity);
    }

    private void OnDestroy()
    {
        if(spawnMarker != null)
        {
            Destroy(spawnMarker);
        }
    }
}
