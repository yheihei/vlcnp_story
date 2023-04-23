using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Experience : MonoBehaviour
{
    public int Point = 1;
    private bool isHit = false;


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isHit)
        {
            return;
        }
        if (collision.tag.Equals("Player"))
        {
            isHit = true;
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
