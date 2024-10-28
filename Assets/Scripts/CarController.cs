using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class CarController : MonoBehaviour
{
    GameObject PlayerCar;

    public Collider[] EnemiesInRange;
    public GameObject ProjectilePrefab, EnemyPrefab;
    bool ShootProjectiles = false;

    GameObject CameraObject;
    Vector3 BaseCameraPosition;

    Rigidbody CarRB;

    Collider HitBox;
    GameObject WarnIndicator;

    GameObject HealthObject;
    TextMeshPro HealthText;

    public List<Collider> IgnoredColliders = new List<Collider>();

    public float CarSpeed = 30f;
    public float TopSpeed = 20f;
    public float TurningSlowdownMultiplier = 1f;
    public float TurnSpeed = 150f;
    public float CarHealth = 10;

    bool Turning = false;

    float VisualTurning = 0;

    float VisualTurningRate = 90f;
    float MaxVisualTurn = 35f;

    string CurrentDirection, LastDirection;
    float TurningSpeedMultiplier = 0f;

    GameObject TurningPoint;

    public bool EnemyCar = true;
    public bool DoesntMove = false;

    Coroutine ProjectileNumerator;

    // Start is called before the first frame update
    void Start()
    {
        TurningPoint = transform.Find("VehicleTurningPoint").gameObject;

        CameraObject = Camera.main.gameObject;
        BaseCameraPosition = CameraObject.transform.position;

        CarRB = this.GetComponent<Rigidbody>();
        HitBox = transform.GetComponent<Collider>();

        PlayerCar = GameObject.Find("Player");

        if(EnemyCar == true)
        {
            HealthObject = this.transform.Find("EnemyHealthAmount").gameObject;
            HealthText = HealthObject.GetComponent<TextMeshPro>();
            WarnIndicator = transform.Find("WarnIndicator").gameObject;
        }
        else if(EnemyCar == false)
        {
            ProjectileNumerator = StartCoroutine(ShootProjectile());
        }
    }

    // Update is called once per frame
    void Update()
    {
        //If a car falls of the map It'll eventually die
        if(transform.position.y < -20f)
        {
            DestroyCar(gameObject);
        }

        if (Input.GetKeyDown(KeyCode.E) == true && EnemyCar == false)
        {
            Instantiate(EnemyPrefab);
        }

        if (EnemyCar == false)
        {
            if (Input.GetKey(KeyCode.LeftArrow) == true)
            {
                this.transform.rotation = Quaternion.Euler(this.transform.eulerAngles.x, this.transform.eulerAngles.y - TurnSpeed * Time.deltaTime, this.transform.eulerAngles.z);
                VisualTurning -= VisualTurningRate * Time.deltaTime;
                Turning = true;

                CurrentDirection = "Left";
            }
            else if (Input.GetKey(KeyCode.RightArrow) == true)
            {
                this.transform.rotation = Quaternion.Euler(this.transform.eulerAngles.x, this.transform.eulerAngles.y + TurnSpeed * Time.deltaTime, this.transform.eulerAngles.z);
                VisualTurning += VisualTurningRate * Time.deltaTime;
                Turning = true;

                CurrentDirection = "Right";
            }
            else
            {
                Turning = false;
            }
        }
        else if(EnemyCar == true && Vector3.Distance(this.transform.position, PlayerCar.transform.position) > 7f)
        {
            Vector3 EnemyDirection = Vector3.RotateTowards(transform.forward, PlayerCar.transform.position - transform.position, TurnSpeed * Time.deltaTime, 0.0f);
            transform.rotation = Quaternion.LookRotation(EnemyDirection);
        }

        //If an enemy is 50m away from the player, they will go through any object in the "obstacles" layer
        //print(Vector3.Distance(this.transform.position, PlayerCar.transform.position));
        if(EnemyCar == true && Vector3.Distance(this.transform.position, PlayerCar.transform.position) > 50f)
        {
            RaycastHit HitObject;
            bool Hit = Physics.Raycast(this.transform.position, this.transform.forward, out HitObject, 5, 1 << LayerMask.NameToLayer("Obstacle"));
            if(Hit)
            {
                Collider HitCollider = HitObject.transform.GetComponent<Collider>();
                Physics.IgnoreCollision(HitBox, HitCollider);
                if(IgnoredColliders.Contains(HitCollider) == false)
                {
                    IgnoredColliders.Add(HitCollider);
                }
            }
        }
        else
        {
            foreach (Collider IgnoredCollider in IgnoredColliders)
            {
                if(IgnoredCollider != null)
                {
                    Physics.IgnoreCollision(HitBox, IgnoredCollider, false);
                }
            }
            IgnoredColliders = new List<Collider>();
        }

        //Doesnt move is for testing purposes
        if (DoesntMove == false)
        {
            if (Turning == false)
            {
                CarRB.velocity += (this.transform.forward * Time.deltaTime * CarSpeed);
            }
            else
            {
                CarRB.velocity += (this.transform.forward * Time.deltaTime * (CarSpeed * TurningSlowdownMultiplier));
            }
        }

        //This checks if an enemy hit a player "deathbox" with the front of their car, if they did, the player dies
        if(EnemyCar == true)
        {
            /*
            RaycastHit HitObject;
            bool Hit = Physics.SphereCast(HurtBox.transform.position, 0.5f ,this.transform.forward, out HitObject, 0.25f, 1 << LayerMask.NameToLayer("Player"));
            if(Hit && HitObject.collider.isTrigger == true)
            {
                //DestoryCar(PlayerCar);
                print("PLAYER DIES");
            }
            */
        }
        //If the player car hits an enemy with their hurt box the enemy takes a bunch of damage
        if(EnemyCar == false)
        {
            /*
            RaycastHit HitObject;
            bool Hit = Physics.BoxCast(HurtBox.transform.position, new Vector3(0.65f, 0.65f, 0.65f), this.transform.forward, out HitObject, 0.05f, 1 << LayerMask.NameToLayer("Enemy"));
            if(Hit)
            {
                print("HIT ENEMY");
                DestoryCar(HitObject.transform.gameObject);
                //HitObject.transform.GetComponent<CarController>().CarHealth -= 5f;
            }
            */
        }


        if (Mathf.Abs(CarRB.velocity.x) + Mathf.Abs(CarRB.velocity.z) > TopSpeed)
        {
            float Diff = 2 - ((Mathf.Abs(CarRB.velocity.x) + Mathf.Abs(CarRB.velocity.z)) / TopSpeed);
            CarRB.velocity = CarRB.velocity * Diff;
        }

        VisuallyTurning();
        if(EnemyCar == true)
        {
            AdjustHealth();
            WarnPlayer();
        }
        if(EnemyCar == false)
        {
            EnemiesInRange = Physics.OverlapSphere(this.transform.position, 15f, 1 << LayerMask.NameToLayer("Enemy"));
            if (EnemiesInRange.Length > 0)
            {
                ShootProjectiles = true;
            }
            else
            {
                ShootProjectiles = false;
            }
        }

        if(CarHealth <= 0f)
        {
            DestroyCar(this.gameObject);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (EnemyCar == false)
        {
            if (collision.gameObject.layer == LayerMask.NameToLayer("Enemy"))
            {
                Transform EnemyHurtBox = collision.transform.Find("VehicleTurningPoint").Find("Vehicle").Find("HurtBox");
                float RequiredDistance = 1.3f;

                if(Vector3.Distance(EnemyHurtBox.position, this.transform.position) > RequiredDistance)
                {
                    DestroyCar(collision.gameObject);
                    //collision.transform.GetComponent<CarController>().CarHealth -= 5f;
                }
                else
                {
                    DestroyCar(this.gameObject);
                }
            }
        }
    }

    public void DestroyCar(GameObject ObjectToDestroy)
    {
        CarController DestroyCC = ObjectToDestroy.GetComponent<CarController>();

        print(ObjectToDestroy.name);
        if(ObjectToDestroy.name != "Player")
        {
            Destroy(ObjectToDestroy);
        }
        else
        {
            PlayerDied(ObjectToDestroy);
        }


        if (EnemyCar == true)
        {
            //Give points and do some other stuff
        }
        else
        {
            //Destroy the player and other stuff
        }
    }

    void PlayerDied(GameObject PlayerCar)
    {
        PlayerCar.transform.localScale = Vector3.zero;
        PlayerCar.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
        StopCoroutine(ProjectileNumerator);
        StartCoroutine(RestartScene());

    }

    IEnumerator RestartScene()
    {
        yield return new WaitForSeconds(2f);
        print("SceneRestart");
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    IEnumerator ShootProjectile()
    {
        while(true)
        {
            yield return new WaitUntil(() => ShootProjectiles == true);
            GameObject NewProjectile = Instantiate(ProjectilePrefab);
            NewProjectile.transform.position = this.transform.position + Vector3.up * 4f;

            yield return new WaitForSeconds(0.25f);
        }
    }

    void AdjustHealth()
    {
        HealthText.text = CarHealth.ToString();
        Vector3 RotateTowardsCamera = Vector3.RotateTowards(HealthObject.transform.forward, CameraObject.transform.position - HealthObject.transform.position, 50f, 0.0f);
        HealthObject.transform.rotation = Quaternion.LookRotation(RotateTowardsCamera);
    }

    private void FixedUpdate()
    {
        if(EnemyCar == false)
        {
            CameraFollowPlayer();
        }
    }

    void VisuallyTurning()
    {
        //VisualTurning = VisualTurning * TurningSpeedMultiplier;
        if (Mathf.Abs(VisualTurning) > MaxVisualTurn)
        {
            VisualTurning = MaxVisualTurn * VisualTurning / Mathf.Abs(VisualTurning); //* (Time.deltaTime * 1);
        }

        if (Turning == false && Mathf.Abs(VisualTurning) > 0.1f)
        {
            VisualTurning -= VisualTurning / Mathf.Abs(VisualTurning) * Time.deltaTime * (VisualTurningRate / 1.5f);
        }
        else if(Turning == false && Mathf.Abs(VisualTurning) <= 0.1f)
        {
            TurningSpeedMultiplier = 0.1f;
            VisualTurning = 0;
        }

        if(LastDirection == CurrentDirection && TurningSpeedMultiplier < 1f)
        {
            TurningSpeedMultiplier += Time.deltaTime * 1f;
        }
        else if(LastDirection != CurrentDirection && Mathf.Abs(VisualTurning) < 1f)
        {
            LastDirection = CurrentDirection;
            TurningSpeedMultiplier = 0.1f;
        }
        else if(TurningSpeedMultiplier > 1f)
        {
            TurningSpeedMultiplier = 1f;
        }
        TurningPoint.transform.localRotation = Quaternion.Euler(TurningPoint.transform.localEulerAngles.x, VisualTurning * TurningSpeedMultiplier, TurningPoint.transform.localEulerAngles.z);
    }

    void CameraFollowPlayer()
    {
        CameraObject.transform.position = Vector3.Lerp(CameraObject.transform.position,
            new Vector3(this.transform.position.x + BaseCameraPosition.x, this.transform.position.y + BaseCameraPosition.y, this.transform.position.z + BaseCameraPosition.z),
            0.15f);
    }

    void WarnPlayer()
    {
        Vector3 CenterPoint = CameraObject.transform.position + (CameraObject.transform.forward * 30f) + new Vector3(CameraObject.transform.forward.x * 8f, 0, CameraObject.transform.forward.z * 8f);
        Vector3 EnemyDirection = (transform.position - PlayerCar.transform.position).normalized;
        float EnemyDistance = Vector3.Distance(PlayerCar.transform.position, this.transform.position);

        if(EnemyDistance > 30f)
        {
            WarnIndicator.SetActive(true);

            Vector3 WeirdCameraAngleNormalized = (CameraObject.transform.position - WarnIndicator.transform.position).normalized;
            float ScreenResolutionSplit;
            ScreenResolutionSplit = (float)Screen.width / (float)Screen.height;

            //We should multiply the Z direction by the difference in screen width to height (((float)Screen.width / (float)Screen.height)), but the tilted camera angle makes it
            //so that whats actually left or right is in a corner direction

            WarnIndicator.transform.position = CenterPoint + (EnemyDirection * (15f));
            //WarnIndicator.transform.position = CenterPoint + (EnemyDirection * (15f + 10f * (ScreenResolutionSplit * (WeirdCameraAngleNormalized.x * (WeirdCameraAngleNormalized.z / WeirdCameraAngleNormalized.z)))));
            //print((CameraObject.transform.position - WarnIndicator.transform.position).normalized);

            //Camera rotation: 0.5375;
            //Vector3 ShitNormalized = (CenterPoint - WarnIndicator.transform.position).normalized;
            //print((this.transform.position - CameraObject.transform.position).normalized);
            //print(this.transform.name);
            //WarnIndicator.transform.position = Vector3.right * WarnIndicator.transform.position.x * (Screen.width / Screen.height);
            //print((float)Screen.width / (float)Screen.height);

            WarnIndicator.transform.localScale = Vector3.one / (EnemyDistance / 20f);
        }
        else
        {
            WarnIndicator.SetActive(false);
        }
    }
}
