using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterResources : MonoBehaviour
{
    public GameObject bomb;
    public Transform bombSpawner;
    public Transform trash;

    public GameObject bullet;
    public Transform shootinPoint;
    public Transform bulletParent;

    //Lithra
    public Transform bellPoint;
    public Transform bellStunPoint;

    //Chiback
    public Transform mirrorFireAttackPoint;
    public Transform fireAttackPoint;

    //Steelager
    public Transform explosionPoint;

    //Volch
    public Transform firePoint;
    public GameObject beam;

    //skipler
    public GameObject skiplerDouble;
    public Transform skiplerPoint;

    //Lupen
    public Transform wipPoint;

    //Visvia
    public Transform fartPoint;
    public GameObject fart;

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(wipPoint.position, 1f);

    }

}
