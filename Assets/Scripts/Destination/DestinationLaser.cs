using UnityEngine;

public class DestinationLaser : Destination
{
    //Return destination type
    public override DestinationType DestinationType => DestinationType.Laser;

    public override bool isUpdatingContent => true;


    //Shooting range
    [SerializeField, Range(1.5f, 10.5f)]
    protected float targetingRange = 2.5f;

    [SerializeField, Range(1f, 100f)]
    float damagePerSecond = 15f;

    [SerializeField]
    Transform turret = default, laserBeam = default;

    TargetPoint target;

    //Draw the range for debug purpose
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 position = transform.localPosition;
        position.y += 0.01f;
        Gizmos.DrawWireSphere(position, targetingRange);
    }

    //Collider array containing all colliders that overlap the described sphere.
    protected bool AcquireTarget(out TargetPoint target)
    {
        if (TargetPoint.FillBuffer(transform.localPosition, targetingRange))
        {
            target = TargetPoint.RandomBuffered;
            return true;
        }
        target = null;
        return false;
    }

    //Check if we have a target and if it's still in our radius
    protected bool TrackTarget(ref TargetPoint target)
    {
        if (target == null)
        {
            return false;
        }
        Vector3 a = transform.localPosition;
        Vector3 b = target.Position;
        float x = a.x - b.x;
        float z = a.z - b.z;
        float r = targetingRange + 0.125f * target.Enemy.Scale;
        if (x * x + z * z > r * r)
        {
            target = null;
            return false;
        }
        return true;
    }

    //We will need to change the scale of the laser (for shoot at the enemy)
    Vector3 laserBeamScale;

    void Awake()
    {
        laserBeamScale = laserBeam.localScale;
    }

    public override void GameUpdate()
    {
        if (TrackTarget(ref target) || AcquireTarget(out target))
        {
            Shoot();
        }
        else
        {
            //Let's put the laser scale at 0 if ther's no target, so it doesn't remain on the grid
            laserBeam.localScale = Vector3.zero;
        }
    }

    //Shoot at a target
    void Shoot()
    {
        //Rotate the turret and laser beam to face the enemy
        Vector3 point = target.Position;
        turret.LookAt(point);
        laserBeam.localRotation = turret.localRotation;

        //Scale the laserBeam so it aim at the enemy
        float d = Vector3.Distance(turret.position, point);
        laserBeamScale.z = d;
        laserBeam.localScale = laserBeamScale;
        //Put the correct position for laserBeam (halfway between enemy and laserBeam)
        laserBeam.localPosition = turret.localPosition + 0.5f * d * laserBeam.forward;

        //Shoot at the enemy
        target.Enemy.ApplyDamage(damagePerSecond * Time.deltaTime);
    }
}
