using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLevel : MonoBehaviour, ILevel
{
    [SerializeField]
    private int level = 1;
    [SerializeField]
    private int maxLevel = 3;
    [SerializeField]
    private int experience = 0;
    public int[] LevelToRequiredExperience = new int[] { 0, 3, 6, 10 };

    public int Level {
        get => level;
    }
    public int MaxLevel { get => maxLevel;}
    public int Experience { get => experience;
        set
        {
            experience = value;
            if (value < 0)
            {
                experience = 0;
            }
        }
    }

    Animator animator;
    private CapsuleCollider2D playerCollider;
    private BoxCollider2D playerGroundCollider;
    public GameObject LevelDownEffect;
    [SerializeField]
    private GameObject aura;
    private GameObject _aura;

    private void Start()
    {
        animator = GetComponent<Animator>();
        playerCollider = GetComponent<CapsuleCollider2D>();
        playerGroundCollider = GetComponent<BoxCollider2D>();
    }

    public void AddExperience(int point)
    {
        // 最大レベル+1の必要経験値を超えたらそれ以上経験値もレベルも増えない
        if (Experience + point > LevelToRequiredExperience[maxLevel])
        {
            Experience = LevelToRequiredExperience[maxLevel];
            return;
        }
        Experience += point;
        if (level == 1 && Experience >= LevelToRequiredExperience[1]) {
            changeLevel(2);
        }
        if (level == 2 && Experience >= LevelToRequiredExperience[2])
        {
            changeLevel(3);
        }
    }

    public void LoseExperience(int point=3)
    {
        Experience -= point;
        if (experience < 0)
        {
            experience = 0;
            return;
        }
        if (level == 2 && Experience < LevelToRequiredExperience[1])
        {
            changeLevel(1);
        }
        if (level == 3 && Experience < LevelToRequiredExperience[2])
        {
            changeLevel(2);
        }
    }

    private void changeLevel(int nextLevel) {
        level = nextLevel;
        if (level == 1)
        {
            animator.runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>("Player/akim_short_1");
            playerCollider.size = new Vector2(playerCollider.size.x, 1.2f);
            playerCollider.offset = new Vector2(playerCollider.offset.x, -0.1f);
            playerGroundCollider.offset = new Vector2(playerGroundCollider.offset.x, -0.7f);
            GameObject weapon = GameObject.FindWithTag("Weapon");
            weapon.transform.position = new Vector3(weapon.transform.position.x, weapon.transform.position.y + 0.3f, weapon.transform.position.z);
            GameObject explode = Instantiate(LevelDownEffect);
            explode.transform.position = transform.position;
            removeAura();
        }
        if (level == 2)
        {
            animator.runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>("Player/akim_level_2");
            playerCollider.size = new Vector2(playerCollider.size.x, 2.3f);
            playerCollider.offset = new Vector2(playerCollider.offset.x, 0);
            playerGroundCollider.offset = new Vector2(playerGroundCollider.offset.x, -1.1f);
            GameObject weapon = GameObject.FindWithTag("Weapon");
            weapon.transform.position = new Vector3(weapon.transform.position.x, weapon.transform.position.y - 0.3f, weapon.transform.position.z);
            GameObject explode = Instantiate(LevelDownEffect);
            explode.transform.position = transform.position;
            removeAura();
        }
        if (level ==3)
        {
            instantiateAura();
        }
    }

    private void instantiateAura()
    {
        // 子コンポーネントとしてauraを追加
        // xに−0.26, yに-0.62の位置に表示
        _aura = Instantiate(aura, transform.position, Quaternion.identity, transform);
        _aura.transform.localPosition = new Vector3(-0.26f, -0.62f, 0);
        
        // AuraControllerコンポーネントを追加してオーラの表示制御を有効にする
        AuraController auraController = _aura.GetComponent<AuraController>();
        if (auraController == null)
        {
            auraController = _aura.AddComponent<AuraController>();
        }
    }
    private void removeAura()
    {
        if (_aura == null) return;
        Destroy(_aura);
    }
}
