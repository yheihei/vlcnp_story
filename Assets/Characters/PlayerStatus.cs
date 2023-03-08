using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStatus : MonoBehaviour, IStatus
{
    public int HitPoints = 5;
    public int MaxHitPoints = 5;
    public int AttackPoints = 0;
    public int DefencePoints = 0;

    int IStatus.HitPoints {
        get { return HitPoints; }
        set { HitPoints = value; }
    }
    int IStatus.MaxHitPoints
    {
        get { return MaxHitPoints; }
        set { MaxHitPoints = value; }
    }
    int IStatus.AttackPoints { get => AttackPoints; set => AttackPoints = value; }
    int IStatus.DefencePoints { get => DefencePoints; set => DefencePoints = value; }

    private bool isDamaged = false;
    private int invincibleCount = 0;
    private const int INVINCIBLE_MAX_COUNT = 150;

    [SerializeField]
    private GameObject ParentObj;
    [SerializeField]
    private GameObject DamageObj;
    [SerializeField]
    private GameObject PosObj;
    [SerializeField]
    private Vector3 AdjPos;

    public void Start()
    {
        MaxHitPoints = HitPoints;
        Debug.Log(MaxHitPoints);
    }

    public void AddDamage(IStatus status)
    {
        // ダメージを受けている最中は無敵
        if (isDamaged)
        {
            return;
        }

        // 点滅する
        isDamaged = true;

        // 吹っ飛ばす
        Rigidbody2D playerRigitBody = GetComponent<Rigidbody2D>();
        playerRigitBody.AddForce(Vector2.up * 4, ForceMode2D.Impulse);

        // HPを減らす
        HitPoints = HitPoints - status.AttackPoints;
        ViewDamage(status.AttackPoints);
    }

    public void FixedUpdate()
    {
        // ダメージを受けている最中は点滅
        if (isDamaged) {
            invincibleCount++;
            SpriteRenderer playerSprite = GetComponent<SpriteRenderer>();
            float level = Mathf.Abs(Mathf.Sin(Time.time * 10));
            playerSprite.color = new Color(1f, 1f, 1f, level);

            // 無敵時間終了
            if (invincibleCount > INVINCIBLE_MAX_COUNT)
            {
                invincibleCount = 0;
                isDamaged = false;
                playerSprite.color = new Color(1f, 1f, 1f, 1);
            }
        }
    }

    private void ViewDamage(int _damage)
    {
        GameObject _damageObj = Instantiate(DamageObj, ParentObj.transform);
        _damageObj.GetComponent<Text>().text = _damage.ToString();
        SpriteRenderer playerSprite = GetComponent<SpriteRenderer>();
        _damageObj.transform.position = playerSprite.transform.position;
    }
}
