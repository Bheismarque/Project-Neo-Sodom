using UnityEngine;
using System.Collections.Generic;

public class Eft_BloodBullet_script : MonoBehaviour
{
    private void Update()
    {
        if (transform.childCount == 0 ) { Destroy(gameObject); }
    }
    /*
    private Transform target;
    private Vector3 targetPosition;
    private Quaternion targetRotation;
    private int childCount = 0;
    private List<ParticleSystem> particleSystems = null;
    private float emission_ratioOverTime = 50;
    private float emission_startSpeed = 0.5f;

    void Start()
    {
        target = new GameObject("Particle Target").transform;
        target.parent = transform.parent;
        target.localPosition = transform.localPosition;
        target.localRotation = transform.localRotation;
        transform.parent = null;

        childCount = transform.childCount;
        particleSystems = new List<ParticleSystem>();
        for (int i = 0; i < childCount; i++)
        {
            particleSystems.Add(transform.GetChild(i).GetComponent<ParticleSystem>());
            ParticleSystem.CollisionModule collision = particleSystems[i].collision;
            collision.enabled = false;
        }
    }

    void Update()
    {
        for (int i = 0; i < childCount; i++)
        {
            ParticleSystem.MainModule main = particleSystems[i].main;
            main.startSpeed = emission_startSpeed + Random.Range(0, emission_startSpeed/2);

            ParticleSystem.EmissionModule emission = particleSystems[i].emission;
            emission.rateOverTime = emission_ratioOverTime;
            
                ParticleSystem.CollisionModule collision = particleSystems[i].collision;
                collision.enabled = true;
        }
        emission_startSpeed = Mathf.Clamp(emission_startSpeed - (1f / 3) * God.gameTime, 0.05f, emission_startSpeed);
        if (emission_ratioOverTime > 15) { emission_ratioOverTime = Mathf.Clamp(emission_ratioOverTime - (50f / 15) * God.gameTime, 5, emission_ratioOverTime); }
        if (emission_ratioOverTime <= 15) { emission_ratioOverTime = Util.smoothChange(emission_ratioOverTime, 0, 30, 1); }

        transform.position = targetPosition;
        transform.rotation = targetRotation;
        
        if (emission_startSpeed <= 0.1f && emission_ratioOverTime <= 0.1) { Destroy(gameObject); }
    }

    void LateUpdate()
    {
        targetPosition = target.position;
        targetRotation = target.rotation;
    }*/
}
