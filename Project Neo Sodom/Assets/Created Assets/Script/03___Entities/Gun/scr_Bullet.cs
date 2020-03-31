using UnityEngine;
using System.Collections;

public class scr_Bullet : MonoBehaviour
{
    private GameObject mom;
    private float damage = 0;
    private float speedPerSecond = 0;
    private Vector2 direction;
    private Vector3 movement;
    private bool destroy = false;

    private float limitDistance = 100;
    private float movedDistance = 0;

    [SerializeField] private GameObject effect_hit_flesh_BloodBurst = null;
    [SerializeField] private GameObject effect_hit_flesh_BloodMist = null;
    [SerializeField] private GameObject effect_trail = null;

    void Start()
    {
        transform.localScale = new Vector3(1, 1, 0);
        effect_trail = Instantiate(effect_trail);
        effect_trail.transform.position = transform.position;
    }

    void Update()
    {
        if (destroy)
        {
            Destroy(gameObject);
        }
        else
        {
            Vector3 position = transform.position;

            float limitDistance_step = speedPerSecond * God.gameTime;
            float movedDistance_step = 0;
            while (limitDistance_step > movedDistance_step)
            {
                // Basic Variable Configure
                bool hit = false;
                bool abort = false;

                // Raycast
                RaycastHit hitPoint;
                hit = Physics.Raycast(position, movement, out hitPoint, limitDistance_step, Util.BulletLayerMask);

                // Movement
                if (hit)
                {
                    //Damage
                    scr_PersonController pc = Util.findTopComponent<scr_PersonController>(hitPoint.transform);
                    if (pc != null)
                    {
                        pc.getPerson().blood -= damage;
                        pc.getPerson().damage(hitPoint.transform, damage);
                        if (pc.getPerson().blood > 0) { pc.getPerson().enableRagdoll(0.1f); }
                        else { pc.getPerson().enableRagdoll(10000); }

                        float ragdollRatio = 5;
                        if (pc.getPerson() == God.PLAYER) { ragdollRatio = 0.5f; }
                        hitPoint.transform.GetComponent<Rigidbody>().AddForce(movement * damage * ragdollRatio);
                    }

                    //Effects
                    if (Util.findTopComponent<scr_Material>(hitPoint.transform) )
                    {
                        TextureMaterial material = Util.findTopComponent<scr_Material>(hitPoint.transform).material;

                        if (material == TextureMaterial.Flesh)
                        {
                            //Blood Burst
                            GameObject blood = Instantiate(effect_hit_flesh_BloodBurst);
                            blood.transform.position = hitPoint.point + hitPoint.normal*0.01f;
                            blood.transform.rotation = Quaternion.LookRotation(hitPoint.normal +
                                                                                new Vector3(Random.Range(-0.5f, 0.5f),
                                                                                            Random.Range(-0.5f, 0.5f),
                                                                                            Random.Range(-0.5f, 0.5f)));
                            blood.transform.parent = hitPoint.transform;


                            //Blood Mist
                            GameObject mist = Instantiate(effect_hit_flesh_BloodMist);
                            mist.transform.position = hitPoint.point;
                        }
                    }

                    abort = true;
                    destroy = true;
                    movedDistance_step += hitPoint.distance;
                }
                else
                {
                    movedDistance_step += limitDistance_step;
                }
                position = transform.position + movement * movedDistance_step;
                movedDistance += movedDistance_step;

                // Abort Decision
                if (hit)
                {
                    if (abort) { break; }
                    else { movedDistance_step += 0.01f; }
                }
            }
            transform.position = position;
            transform.localScale = new Vector3(1, 1, movedDistance_step);

            if (movedDistance > limitDistance) { destroy = true; }

            effect_trail.transform.position = transform.position;
        }
    }

    public GameObject getMom() { return mom; }
    public void setMom(GameObject mom) { this.mom = mom; }

    public float getDamage() { return damage; }
    public void setDamage(float damage) { this.damage = damage; }

    public float getSpeed() { return speedPerSecond; }
    public void setSpeed(float speed) { speedPerSecond = speed; }

    public Vector2 getDirecction() { return direction; }
    public void setDirection(Vector3 direction)
    {
        this.direction.x = -Util.vec2ToDir(new Vector2(direction.x, direction.z)) + 90;
        this.direction.y = -Util.vec2ToDir(new Vector2(new Vector2(direction.x, direction.z).magnitude, direction.y));
        transform.eulerAngles = new Vector3(this.direction.y, this.direction.x, 0);

        movement = direction / direction.magnitude;
    }
}
