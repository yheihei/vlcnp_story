using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SomeTimesItem : MonoBehaviour
{
    public GameObject Item;
    public int Frequency = 120;
    private int count = 0;

    void FixedUpdate()
    {
        count++;
        if (count == Frequency)
        {
            GameObject item = Instantiate(Item) as GameObject;
            item.transform.position = transform.position;
            count = 0;
        }
    }
}
