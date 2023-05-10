using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DamageUI2D : MonoBehaviour
{
    [SerializeField]
    private float deleteTime = 1.5f;
    [SerializeField]
    private float moveRange = 2f;

    private float timeCount;

    void Start()
    {
        timeCount = 0.0f;
        Destroy(gameObject, deleteTime);
    }

    void Update()
    {
        timeCount += Time.deltaTime;
        // 徐々にy座標を上げる
        transform.position += new Vector3(0, moveRange / deleteTime * Time.deltaTime, 0);
    }
}