using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectible_Points : MonoBehaviour
{
    [SerializeField] private int points = 10;
    private void OnTriggerEnter(Collider other)
    {
        if((other.CompareTag("Player")) || (other.CompareTag("Nose")))
        {
            GameManager.Instance.AddPoints(points);

            SaveableObject saveable;
            if (gameObject.TryGetComponent<SaveableObject>(out saveable)) saveable.UnRegisterSaveable();
            Destroy(gameObject);
        }
    }

    
}
