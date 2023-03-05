using UnityEngine;
using System.Collections;

public class Explode : MonoBehaviour
{
    public float deleteTime = 0.5f;
    // Use this for initialization
    void Start()
	{
        Destroy(this.gameObject, deleteTime);
    }

	// Update is called once per frame
	void Update()
	{
			
	}
}

