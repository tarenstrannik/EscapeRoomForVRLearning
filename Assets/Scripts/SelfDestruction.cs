using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelfDestruction : MonoBehaviour
{
    [SerializeField] private float m_destructionTime = 5f;

    public void SelfDestruct()
    {
        StartCoroutine(ISelfDestruct());
    }
    private IEnumerator ISelfDestruct()
    {
        yield return new WaitForSeconds(m_destructionTime);
        Destroy(gameObject);
    }
}
