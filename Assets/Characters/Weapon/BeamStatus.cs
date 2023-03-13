using UnityEngine;
using System.Collections;

public class BeamStatus : MonoBehaviour, IStatus
{
    public int HitPoints= 5;
    public int MaxHitPoints = 5;
    public int AttackPoints = 0;
    public int DefencePoints = 0;

    int IStatus.HitPoints { get => HitPoints; set => HitPoints = value; }
    int IStatus.MaxHitPoints
    {
        get { return MaxHitPoints; }
        set { MaxHitPoints = value; }
    }
    int IStatus.AttackPoints { get => AttackPoints; set => AttackPoints = value; }
    int IStatus.DefencePoints { get => DefencePoints; set => DefencePoints = value; }

    public void AddDamage(IStatus status)
    {
        
    }
}

