using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStatus : MonoBehaviour, IStatus
{
    public int HitPoints = 2;
    public int MaxHitPoints = 2;
    public int AttackPoints = 2;
    public int DefencePoints = 0;

    int IStatus.HitPoints { get => HitPoints; set => HitPoints = value; }
    int IStatus.MaxHitPoints
    {
        get { return MaxHitPoints; }
        set { MaxHitPoints = value; }
    }
    int IStatus.AttackPoints { get => AttackPoints; set => AttackPoints = value; }
    int IStatus.DefencePoints { get => DefencePoints; set => DefencePoints = value; }

    private Damage damage;
    public GameObject ExplodeObject;


    public void Start()
    {
        damage = GetComponent<Damage>();
    }

    public void AddDamage(IStatus status)
    {
        HitPoints = HitPoints - status.AttackPoints;
        damage.ViewDamage(status.AttackPoints);
        if (HitPoints <= 0) {
            GameObject explode = Instantiate(ExplodeObject);
            //Vector3 position = new Vector3(transform.position.x, transform.position.y, transform.position.z);
            explode.transform.position = transform.position;
            Destroy(this.gameObject);
        }
    }
}
