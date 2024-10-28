using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileScript : MonoBehaviour
{
    CarController CC;
    GameObject Target;
    float ProjectileSpeed = 25f;

    private void Start()
    {
        CC = GameObject.Find("Player").GetComponent<CarController>();
        if(CC.EnemiesInRange.Length > 0)
        {
            Target = CC.EnemiesInRange[0].gameObject;
        }
    }
    void Update()
    {
        if(Target != null)
        {
            transform.position = Vector3.MoveTowards(transform.position, Target.transform.position, ProjectileSpeed * Time.deltaTime);
        }
        if(Target == null || Vector3.Distance(transform.position, Target.transform.position) < 0.1f)
        {
            if(Target)
            {
                Target.GetComponent<CarController>().CarHealth -= 1;
            }
            Destroy(this.gameObject);
        }
    }
}
