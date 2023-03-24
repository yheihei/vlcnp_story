using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fungus;

public class OnKeyMessage : MonoBehaviour
{
    public Flowchart Flowchart;
    public string BlockName;
    private bool isChat = false;
    private bool isCollision = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.tag.Equals("Player"))
        {
            return;
        }
        isCollision = true;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.tag.Equals("Player")) {
            return;
        }
        isCollision = false;
    }

    private void Update()
    {
        if (Input.GetKey("up") && !isChat)
        {
            isChat = true;
        }
    }

    private void FixedUpdate()
    {
        if (!isCollision) {
            return;
        }
        if (!isChat) {
            return;
        }
        if (Flowchart.HasExecutingBlocks())
        {
            return;
        }
        StartCoroutine(Talk());
    }

    IEnumerator Talk() {
        Debug.Log("開始");
        Flowchart.ExecuteBlock(BlockName);
        yield return new WaitUntil(() => Flowchart.GetExecutingBlocks().Count == 0);
        isChat = false;
        Debug.Log("終了");
    }
}
