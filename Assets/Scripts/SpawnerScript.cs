using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SpawnerScript : MonoBehaviour
{
    public GameObject EnemyPrefab;
    public List<GameObject> ObstaclePrefab = new List<GameObject>();
    public GameObject Plane;

    Vector3 SpawnPosition;

    public List<GameObject> AllMovingObjects = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        AllMovingObjects.Add(GameObject.Find("Player"));
        print(AllMovingObjects[0]);

        //How many objects do we want to spawn
        for(int i = 250; i > 0; i--)
        {
            SpawnObject();
        }

        StartCoroutine(SpawnEnemies());
    }

    void SpawnObject()
    {
        GameObject NewObject = Instantiate(ObstaclePrefab[Random.Range(0, ObstaclePrefab.Count)]);
        Vector3 RandomPosition = new Vector3(Random.Range(-Plane.transform.localScale.x * 5, Plane.transform.localScale.x * 5), 0, Random.Range(-Plane.transform.localScale.z * 5, Plane.transform.localScale.z * 5));
        NavMeshHit myNavHit;
        if (NavMesh.SamplePosition(RandomPosition, out myNavHit, 100000, -1))
        {
            NewObject.transform.position = myNavHit.position;
        }
    }

    IEnumerator SpawnEnemies()
    {
        yield return new WaitForSeconds(1f);
        while (true)
        {
            GameObject NewEnemy = Instantiate(EnemyPrefab);
            bool LegalSpawn = false;
            Vector3 RandomDirection = Vector3.zero;
            Vector3 RandomPosition = Vector3.zero;

            //If an object fails to spawn 100 times, its spawned at 0, 0 (SO WE ARE NOT RUNNING AN INFINTE LOOP)
            int FailSafe = 0;
            while (LegalSpawn == false && FailSafe < 100)
            {
                RandomPosition = new Vector3(Random.Range(-Plane.transform.localScale.x * 5, Plane.transform.localScale.x * 5), 0, Random.Range(-Plane.transform.localScale.z * 5, Plane.transform.localScale.z * 5));

                FailSafe += 1;
                LegalSpawn = true;

                foreach (GameObject MovingObject in AllMovingObjects)
                {
                    if(MovingObject != null)
                    {
                        float Distance = Vector3.Distance(RandomPosition, MovingObject.transform.position);
                        if (MovingObject.name == "Player" && Distance < 90)
                        {
                            LegalSpawn = false;
                        }
                        else if (Distance < 3)
                        {
                            LegalSpawn = false;
                        }

                        if (LegalSpawn == false)
                        {
                            print("FAILED SPAWN");
                            break;
                        }
                    }
                }
            }
            //Remove all null objects
            AllMovingObjects.RemoveAll(x => !x);

            NavMeshHit myNavHit;
            if (NavMesh.SamplePosition(RandomPosition, out myNavHit, 100000, -1))
            {
                NewEnemy.transform.position = myNavHit.position;
            }

            AllMovingObjects.Add(NewEnemy);
            yield return new WaitForSeconds(1f);
        }
        yield return null;
    }
}
