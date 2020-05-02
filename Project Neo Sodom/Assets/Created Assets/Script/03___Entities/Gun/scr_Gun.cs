using UnityEngine;
using System.Collections.Generic;

public enum GunType { Handgun, Rifle }
public enum GunCategory { Pistol, Revolver, SubMachinegun, AssaultRifle, Shotgun }
public enum FireType { SemiAuto, FullAuto, Safety }
public enum PostureType { OneHanded, DoubleHanded, HipShot, TacticalShot}
public class scr_Gun : MonoBehaviour
{
    private GameObject gunHolder = null;

    [SerializeField] private GunType gunType = GunType.Handgun;
    [SerializeField] private GunCategory gunCategory = GunCategory.Pistol;
    [SerializeField] private PostureType posture_aim = PostureType.OneHanded;
    [SerializeField] private PostureType posture_aimProper = PostureType.OneHanded;

    [SerializeField] private FireType fireType = FireType.FullAuto; 
    private bool fireType_isModified = false;
    private FireType fireType_modified = FireType.FullAuto;

    [SerializeField] private Vector2 damage = new Vector2(0,0);
    [SerializeField] private GameObject bulletKind = null;
    [SerializeField] private Vector2 bulletSpeed = Vector2.zero;
    [SerializeField] private float bulletNum = 1;
    [SerializeField] private float bulletSpread = 0;

    [SerializeField] private int magazineSize = 8;
    [SerializeField] private int bulletConsumePerShot = 1;
    [SerializeField] private int bulletsInMagazineNum = 0;
    private int bulletsInMagazine = 0;


    private float recoil_angle = 90;
    [SerializeField] private float recoil_kick = 100;
    [SerializeField] private float recoil_comback = 30;
    [SerializeField] private float recoil_angle_center = 90;
    [SerializeField] private float recoil_angle_random = 60;

    private float coolTime = 0f;
    [SerializeField] private float coolTime_val = (1f/15);

    private bool fired = false;

    private bool triggered = false;
    private bool triggered_pre = false;

    private bool trigger_semiAuto = false;
    private bool trigger_fullAuto = false;
    private bool trigger_safety = false;

    private Transform gunPoint = null;
    private Vector3 angle = Vector3.zero;

    private void Start()
    {
        gunPoint = transform.GetChild(1);
        bulletsInMagazine = bulletsInMagazineNum;
    }

    public void pullTrigger()
    {
        triggered = true;
    }

    public void reload(int bulletRefill)
    {
        bulletsInMagazine += bulletRefill;
        if (bulletsInMagazine > magazineSize) { bulletsInMagazine = magazineSize; }
    }

    public void operate()
    {
        fired = false;

        if ( coolTime > 0 ) { coolTime -= God.gameTime; }
        if ( coolTime < 0 ) { coolTime = 0; }

        trigger_semiAuto = !triggered_pre && triggered;
        trigger_fullAuto = triggered;
        trigger_safety = false;

        angleCalculation();
        shoot();

        triggered_pre = triggered;
        triggered = false;
    }

    public void shoot()
    {

        bool trigger = false;

        FireType fireType_cur;
        if (fireType_isModified) { fireType_cur = fireType_modified; }
        else { fireType_cur = fireType; }

        if (fireType_cur == FireType.SemiAuto) { trigger = trigger_semiAuto; }
        if (fireType_cur == FireType.FullAuto) { trigger = trigger_fullAuto; }
        if (fireType_cur == FireType.Safety) { trigger = trigger_safety; }


        if (trigger && coolTime <= 0 && bulletsInMagazine >= bulletConsumePerShot)
        {
            bulletsInMagazine -= bulletConsumePerShot;

            for (int i = 0; i < bulletNum; i++)
            {
                if (i > 0) { angleCalculation(); }

                scr_Bullet bullet = Instantiate(bulletKind).GetComponent<scr_Bullet>();
                if (bullet != null)
                {
                    bullet.setDamage(Random.Range(damage.x, damage.y));
                    bullet.setMom(gunHolder);
                    bullet.transform.position = gunPoint.transform.position;
                    bullet.setDirection(angle);
                    bullet.setSpeed(Random.Range(bulletSpeed.x, bulletSpeed.y));
                }
            }
            
            recoil_angle = recoil_angle_center + Random.Range(-recoil_angle_random / 2, recoil_angle_random / 2);
            coolTime = coolTime_val;
            fired = true;
        }
    }

    private void angleCalculation()
    {
        if (gunPoint == null) { return; }
        //Saves the Original Angle
        Vector3 originalPosition = gunPoint.localPosition;

        //Gun Center Angle-Length Calculation
        gunPoint.localPosition = new Vector3(originalPosition.x,0,0);
        angle = gunPoint.position - transform.position;
        
        //Spread Calculation
        float spreadAngle = Mathf.Tan(Random.Range(0, bulletSpread / 2) * Mathf.Deg2Rad) * angle.magnitude;
        Vector2 spread = Util.dirToVec2(Random.Range(0, 360)) * spreadAngle;

        gunPoint.localPosition = new Vector3(originalPosition.x, spread.x, spread.y);
        angle = gunPoint.position - transform.position;
        
        //Revert to the Orignal Angle
        gunPoint.localPosition = originalPosition;
    }

    public PostureType getPosture_aim() { return posture_aim; }
    public void setPosture_aim(PostureType posture_aim) { this.posture_aim = posture_aim; }

    public PostureType getPosture_aimProper() { return posture_aimProper; }
    public void setPosture_aimProper(PostureType posture_aimProper) { this.posture_aimProper = posture_aimProper; }

    public FireType getFireType() { return fireType; }
    public void setFireType(FireType fireType) { this.fireType_modified = fireType; fireType_isModified = true; }

    public GunType getGunType () { return gunType; }
    public void setGunType(GunType gunType) { this.gunType = gunType; }

    public GunCategory getGunCategory() { return gunCategory; }
    public void setGunCategory(GunCategory gunCategory) { this.gunCategory = gunCategory; }

    public GameObject getGunHolder() { return gunHolder; }
    public void setGunHolder( GameObject gunHolder ) { this.gunHolder = gunHolder; }

    public float getRecoil_kick() { return recoil_kick; }
    public void setRecoil_kick(float recoil_kick) { this.recoil_kick = recoil_kick; }

    public float getRecoil_comeback() { return recoil_comback; }
    public void setRecoil_comeback(float recoil_comback) { this.recoil_comback = recoil_comback; }

    public float getRecoil_angle_center() { return recoil_angle_center; }
    public void setRecoil_angle_center(float recoil_angle_center) { this.recoil_angle_center = recoil_angle_center; }

    public float getRecoil_angle_random() { return recoil_angle_random; }
    public void setRecoil_angle_random(float recoil_angle_random ) { this.recoil_angle_random = recoil_angle_random; }
    
    public int getMagazineSize() { return magazineSize; }

    public int getBulletsInMagazine() { return bulletsInMagazine; }

    public float getCoolTime() { return coolTime; }

    public Transform getGunPoint() { return gunPoint.transform; } 

    public Vector3 getAngle() { angleCalculation(); return angle; }

    public float getRecoil_angle() { return recoil_angle; }

    public bool isFired() { return fired; }
}
