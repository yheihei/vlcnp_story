using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DamageUI2D : MonoBehaviour
{
    [SerializeField]
    private float deleteTime = 1.5f;
    [SerializeField]
    private float moveRange = 50.0f;

    private float timeCount;
    private Text nowText;

    void Start()
    {
        timeCount = 0.0f;
        Destroy(this.gameObject, deleteTime);
        nowText = this.gameObject.GetComponent<Text>();
    }

    void Update()
    {
        timeCount += Time.deltaTime;
        this.gameObject.transform.localPosition += new Vector3(0, moveRange / deleteTime * Time.deltaTime, 0);
    }
}