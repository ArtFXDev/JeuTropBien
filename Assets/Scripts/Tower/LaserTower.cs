using UnityEngine;

public class LaserTower : Tower {

    [SerializeField, Range(1f, 100f)]
    float damagePerSecond = 10f;

    [SerializeField]
    Transform turret = default, laserBeam = default;

    TargetPoint target;

    //We will need to change the scale of the laser (for shoot at the enemy)
    Vector3 laserBeamScale;

    void Awake()
    {
        laserBeamScale = laserBeam.localScale;
    }

    //Return tower type
    public override TowerType TowerType => TowerType.Laser;

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