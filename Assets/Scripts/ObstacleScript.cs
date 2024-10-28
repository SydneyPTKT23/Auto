using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleScript : MonoBehaviour
{
    public bool DestroyOnCollision = true;
    public enum ObstacleType { Basic, ExplosiveBarrel };
    public ObstacleType TypeOfObstacle = ObstacleType.Basic;
    public bool WorksOnce = true;

    bool HitObject = false;


    private void OnCollisionEnter(Collision collision)
    {
        if (HitObject == false)
        {
            if (collision.gameObject.layer == LayerMask.NameToLayer("Player") || collision.gameObject.layer == LayerMask.NameToLayer("Enemy"))
            {
                if(WorksOnce == true)
                {
                    HitObject = true;
                }
                ObstacleEffects(collision.gameObject);
            }
        }
    }

    void ObstacleEffects(GameObject HitObject)
    {
        switch(TypeOfObstacle)
        {
            //Basic obstacles just destroy whatever car hit the obstacle and potentially the obstacle itself depending on if its destroyed on collision
            case ObstacleType.Basic:
                CarController CC = HitObject.gameObject.GetComponent<CarController>();
                CC.DestroyCar(HitObject);
                //DestoryCar(gameObject);
                if (DestroyOnCollision == true)
                {
                    Destroy(this.gameObject);
                }
                break;

            case ObstacleType.ExplosiveBarrel:
                //Barrels explode after 5 seconds with collision destroying every car around it
                StartCoroutine(BarrelTimer());
                break;
        }
    }

    IEnumerator BarrelTimer()
    {
        float BarrelExplosionRadius = 15f;

        yield return new WaitForSeconds(2.5f);

        RaycastHit[] HitObject = Physics.SphereCastAll(this.transform.position, BarrelExplosionRadius, Vector3.up, BarrelExplosionRadius / 2f, (1 << LayerMask.NameToLayer("Player")) + (1 << LayerMask.NameToLayer("Enemy")));
        foreach(RaycastHit Object in HitObject)
        {
            GameObject TempObject = Object.collider.gameObject;
            TempObject.GetComponent<CarController>().DestroyCar(TempObject);
        }
        print(HitObject.Length);
        Destroy(this.gameObject);
    }
}
