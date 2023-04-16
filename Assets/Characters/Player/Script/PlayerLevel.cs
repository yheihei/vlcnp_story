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
    private GameObject weapon;
    public GameObject LevelDownEffect;

    private void Start()
    {
        animator = GetComponent<Animator>();
        playerCollider = GetComponent<CapsuleCollider2D>();
        playerGroundCollider = GetComponent<BoxCollider2D>();
        weapon = GameObject.FindWithTag("Weapon");
    }

    public void AddExperience(int point)
    {
        if (level == maxLevel) {
            return;
        }
        Experience += point;
        if (level == 1 && Experience >= 3) {
            changeLevel(2);
        }
    }

    public void LoseExperience(int point=3)
    {
        Experience -= point;
        if (level == 1)
        {
            Debug.Log("min level!!");
            return;
        }
        if (level == 2 && Experience < 3)
        {
            changeLevel(1);
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
            weapon.transform.position = new Vector3(weapon.transform.position.x, weapon.transform.position.y + 0.3f, weapon.transform.position.z);
            GameObject explode = Instantiate(LevelDownEffect);
            explode.transform.position = transform.position;
        }
        if (level == 2) {
            animator.runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>("Player/akim_level_2");
            playerCollider.size = new Vector2(playerCollider.size.x, 2.3f);
            playerCollider.offset = new Vector2(playerCollider.offset.x, 0);
            playerGroundCollider.offset = new Vector2(playerGroundCollider.offset.x, -1.1f);
            weapon.transform.position = new Vector3(weapon.transform.position.x, weapon.transform.position.y - 0.3f, weapon.transform.position.z);
            GameObject explode = Instantiate(LevelDownEffect);
            explode.transform.position = transform.position;
        }
    }
}
