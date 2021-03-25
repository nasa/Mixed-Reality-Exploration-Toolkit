// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    [SerializeField] GameObject prefab;

    public Queue<GameObject> Pool { get; private set; }

    private void Awake()
    {
        Pool = new Queue<GameObject>();
    }

    public void CreateInstances(int amount)
    {
        GameObject instance = Instantiate(prefab);
        instance.gameObject.SetActive(false);
        ReturnToQueue(instance);
    }

    public void ReturnToQueue(GameObject instance)
    {
        Pool.Enqueue(instance);
    }

    public GameObject Get()
    {
        if(Pool.Count == 0)
        {
            CreateInstances(1);
        }

        GameObject instance = Pool.Dequeue();

        if (instance)
        {
            instance.gameObject.SetActive(true);
            MethodDelayer.DelayMethodByPredicateAsync(() => ReturnToQueue(instance), () => instance.activeSelf == false);
        }
        return instance;
    }


}
