using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectDestory : MonoBehaviour
{
    [SerializeField] float deleteTime;
    void Start()
    {
        Destroy(gameObject,deleteTime);
    }
}
