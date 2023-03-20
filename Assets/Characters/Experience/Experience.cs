using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Experience : MonoBehaviour
{
    public int Point = 1;


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag.Equals("Player"))
        {
            ILevel level = collision.GetComponent<ILevel>();
            AddExperience(level);
        }
    }

    private void AddExperience(ILevel level)
    {
        level.AddExperience(Point);
        Destroy(gameObject);
    }
}
