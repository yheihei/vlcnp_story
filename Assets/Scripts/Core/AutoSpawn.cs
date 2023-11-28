using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VLCNP.Control;
using VLCNP.Core;

public class AutoSpawn : MonoBehaviour, IStoppable
{
    [SerializeField] GameObject spawnObject;
    [SerializeField] float intervalSecond = 3f;
    [SerializeField] GameObject spawnEffect;
    // playerとの距離がこれ以上ならスポーンしない
    [SerializeField] float spawnMaxRange = 8f;
    // playerとの距離がこれ以下ならスポーンしない
    [SerializeField] float spawnMinRange = 2f;

    private float timeSinceLastSpawn = Mathf.Infinity;
    private GameObject player;
    PartyCongroller partyCongroller;

    bool isStopped = false;
    public bool IsStopped { get => isStopped; set => isStopped = value; }

    // PartyTagのオブジェクトからPartyCongrollerを取得して、OnChangeCharacterのイベントを設定
    void OnEnable()
    {
        partyCongroller = GameObject.FindGameObjectWithTag("Party")?.GetComponent<PartyCongroller>();
        if (partyCongroller == null) return;
        partyCongroller.OnChangeCharacter += SetPlayer;
    }

    void OnDisable()
    {
        partyCongroller = GameObject.FindGameObjectWithTag("Party")?.GetComponent<PartyCongroller>();
        if (partyCongroller == null) return;
        partyCongroller.OnChangeCharacter -= SetPlayer;
    }

    void Awake()
    {
        SetPlayer(GameObject.FindGameObjectWithTag("Player"));
    }

    void SetPlayer(GameObject player)
    {
        this.player = player;
    }

    void FixedUpdate()
    {
        if (isStopped) return;
        timeSinceLastSpawn += Time.deltaTime;
        // Playerとの距離が、スポーンする範囲内ならスポーンする
        if (!CanSpawnRange()) return;
        if (timeSinceLastSpawn > intervalSecond)
        {
            Spawn();
            timeSinceLastSpawn = 0;
        }
    }

    bool CanSpawnRange()
    {
        if (player == null) return false;
        if (Vector2.Distance(player.transform.position, transform.position) > spawnMaxRange) return false;
        if (Vector2.Distance(player.transform.position, transform.position) < spawnMinRange) return false;
        return true;
    }

    private void Spawn()
    {
        Instantiate(spawnObject, transform.position, Quaternion.identity);
        if (spawnEffect != null)
        {
            GameObject effect = Instantiate(spawnEffect, transform.position, Quaternion.identity);
            Destroy(effect, 1f);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, spawnMinRange);
        Gizmos.DrawWireSphere(transform.position, spawnMaxRange);
    }
}
