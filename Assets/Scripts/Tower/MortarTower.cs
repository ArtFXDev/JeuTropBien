using UnityEngine;

public class MortarTower : Tower
{
    //Values
    [SerializeField, Range(0.5f, 2f)]
    float shotsPerSecond = 1f;

    [SerializeField, Range(0.5f, 3f)]
    float shellBlastRadius = 1f;

    [SerializeField, Range(1f, 100f)]
    float shellDamage = 10f;

    [SerializeField]
    Transform mortar = default;

    public override TowerType TowerType => TowerType.Mortar;

    float launchSpeed;

    float launchProgress;

    //---------------------------
    //Functions
    void Awake()
    {
        OnValidate();    
    }

    void OnValidate()
    {
        float x = targetingRange + 0.25001f;
        float y = -mortar.position.y;
        launchSpeed = Mathf.Sqrt(9.81f * (y + Mathf.Sqrt(x*x + y*y) ) );
    }

    public override void GameUpdate()
    {
        launchProgress += shotsPerSecond * Time.deltaTime;
        while(launchProgress >= 1f)
        {
            if(AcquireTarget(out TargetPoint target))
            {
                Launch(target);
                launchProgress -= 1f;
            }
            else
            {
                launchProgress = 0.999f;
            }
        }
    }

    //Launch a shell to the destionation pointed
    public void Launch(TargetPoint target)
    {
        Vector3 launchPoint = mortar.position;
        Vector3 targetPoint = target.Position;
        targetPoint.y = 0f;

        //Drawing the triangle
        Vector2 dir;
        dir.x = targetPoint.x - launchPoint.x;
        dir.y = targetPoint.z - launchPoint.z;

        //Targeting triangle
        float x = dir.magnitude;
        float y = -launchPoint.y;
        dir /= x;

        //Launch angle
        float g = 9.81f; //gravity
        float s = launchSpeed; //speed
        float s2 = s * s;

        float r = s2 * s2 - g * (g * x * x + 2f * y * s2);

        //Check if the range is enough
        if (r < 0f)
        {
            Debug.Assert(r >= 0f, "Launch velocity insufficient for range!");
            return;
        }
        float tanTheta = (s2 + Mathf.Sqrt(r)) / (g * x);
        float cosTheta = Mathf.Cos(Mathf.Atan(tanTheta));
        float sinTheta = cosTheta * tanTheta;

        //Rotate the mortar so it face it's target
        mortar.localRotation = Quaternion.LookRotation(new Vector3(dir.x, tanTheta, dir.y));

        //Launch a shell
        Game.SpawnShell().Initialize(
            launchPoint, targetPoint,
            new Vector3(s * cosTheta * dir.x, s * sinTheta, s * cosTheta * dir.y),
            shellBlastRadius, shellDamage
        );
    }
}
