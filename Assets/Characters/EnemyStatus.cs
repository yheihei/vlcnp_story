using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStatus : MonoBehaviour, IStatus
{
    public int hitPoints = -1;
    public int attackPoints = 2;
    public int defencePoints = 0;

    int IStatus.hitPoints { get => hitPoints; set => hitPoints = value; }
    int IStatus.attackPoints { get => attackPoints; set => attackPoints = value; }
    int IStatus.defencePoints { get => defencePoints; set => defencePoints = value; }

    public void addDamage(IStatus status)
    {
        this.hitPoints = this.hitPoints - status.attackPoints;
        Debug.Log(hitPoints);
    }
}
