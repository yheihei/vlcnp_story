using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStatus : MonoBehaviour, IStatus
{
    public int HitPoints = 2;
    public int AttackPoints = 2;
    public int DefencePoints = 0;

    int IStatus.HitPoints { get => HitPoints; set => HitPoints = value; }
    int IStatus.AttackPoints { get => AttackPoints; set => AttackPoints = value; }
    int IStatus.DefencePoints { get => DefencePoints; set => DefencePoints = value; }

    private Damage damage;


    public void Start()
    {
        damage = GetComponent<Damage>();
    }

    public void AddDamage(IStatus status)
    {
        HitPoints = HitPoints - status.AttackPoints;
        damage.ViewDamage(status.AttackPoints);
        if (HitPoints <= 0) {
            Destroy(this.gameObject);
        }
    }
}
