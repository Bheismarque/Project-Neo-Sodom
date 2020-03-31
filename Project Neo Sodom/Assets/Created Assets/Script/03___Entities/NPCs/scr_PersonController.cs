using UnityEngine;
using UnityEngine.AI;

using System.Collections.Generic;

public class scr_PersonController : MonoBehaviour
{
    [SerializeField] bool isPlayer = false;
    private scr_Person person = null;

    private float time_alive = 0;


    //Helper Methods
    #region
    public void setAsPlayer(bool isPlayer)
    {
        if (isPlayer)
        {
            if (God.PLAYER != null) { God.PLAYER.setAsPlayer(false); }
            God.PLAYER = this;
            this.isPlayer = true;
        }
        else
        {
            this.isPlayer = false;
        }
    }

    public scr_Person getPerson() { return person; }
    #endregion

    //Common Logic
    #region
    private void Start()
    {
        person = GetComponent<scr_Person>();
        setAsPlayer(isPlayer);

        if (isPlayer) { Player_Start(); }
        if (!isPlayer) { AI_Start(); }

        God.NPCs.Add(this);
    }

    private void Update()
    {
        if (person != null)
        {
            if (isPlayer) { Player_Update(); }
            if (!isPlayer) { AI_Update(); }
        }
        time_alive += God.gameTime;
    }
    #endregion

    //Player
    #region
    private void Player_Start()
    {

    }
    private void Player_Update()
    {
        // Dead
        if (person.blood <= 0)
        {
            person.moveInput = new Vector2(0, 0);
            person.key_postureToggle = false;
            person.key_sprint = false;

            person.key_shoot = false;
            person.key_aim = false;
            person.key_reload = false;
            person.key_weaponChange = false;
            person.key_cameraSideInvert = false;
            return;
        }
        
        //Move Input
        person.moveInput = new Vector2(God.input.move_hor, God.input.move_ver);
        person.key_postureToggle = Input.GetButtonDown("B") || Input.GetKeyDown(KeyCode.C) || Input.GetKeyDown(KeyCode.LeftControl);
        person.key_sprint = (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetButtonDown("LS"));

        //Look Input
        if (God.CAMERA != null) { person.lookInput = God.CAMERA.getAngle(); }
        if (God.CAMERA != null) { person.key_cameraSideInvert = Input.GetButtonDown("LB") || Input.GetKeyDown(KeyCode.F); }

        //Gun Input
        person.key_shoot = God.input.mouse_left[1] || God.input.rightTriggered[1];
        person.key_aim = (God.input.mouse_right[1] || God.input.leftTriggered[1]);
        person.key_reload = Input.GetKeyDown(KeyCode.R) || Input.GetButtonDown("X");
        person.key_weaponChange = Input.GetKeyDown(KeyCode.Q) || Input.GetButtonDown("Y");

        if (person.key_aim)
        {
            bool toggle = true;
            if (toggle && !person.key_aimProper && (Input.mouseScrollDelta.y > 0.1f || Input.GetButtonDown("RS")))
            {
                person.key_aimProper = true; toggle = false;
            }
            if (toggle && person.key_aimProper && (Input.mouseScrollDelta.y < -0.1f || Input.GetButtonDown("RS")))
            {
                person.key_aimProper = false; toggle = false;
            }
        }
        else
        {
            person.key_aimProper = false;
        }
    }
    #endregion

    //AI
    #region
    /*=======================================================
     *            ___                  _______________
     *           /%%%\                |&&&&&&&&&&&&&&&|
     *          /%%%%%\                     |&&&|
     *         /%%/ \%%\                    |&&&|
     *        /%%/   \%%\       ___         |&&&|
     *       /%%/     \%%\     |&&&|        |&&&|
     *      /%%%-------%%%\    |&&&|        |&&&|
     *     /%%%%%%%%%%%%%%%\                |&&&|
     *    /&&/           \%%\          _____|&&&|_____
     *   |&&&|           |&&&|        |&&&&&&&&&&&&&&&|
     *
     *========================================================
    */
    //Component
    private Rigidbody rigidBody = null;
    private NavMeshAgent navAgent = null;

    //Faction
    public enum Faction { None, Player, Enemy };
    private Faction AI_faction = Faction.None;

    //Stats
    Vector3 currentPosition = Vector3.zero;

    float visibility_distance = 50f;
    Vector2 visibility_sightRange = new Vector2(160, 90);


    //Navigation
    private Vector3 destinationLocation = Vector3.zero;
    private Vector3 destinationDistance = Vector3.zero;


    //Target
    private scr_PersonController target = null;
    private Vector3 target_location = Vector3.zero;
    private Vector3 target_vector = Vector3.zero;
    private Vector2 target_direction = Vector2.zero;
    private bool target_visiblity = false;

    private float targetSearchTimer = 0;
    private float targetSearchTimerValue = 0;
    private Vector2 targetSearchTimerValue_normal = new Vector2(1f,2f);


    //Cover
    private Vector3 coverLocation = Vector3.zero;
    private float coverSearchTimer = 0;
    private float coverSearchTimerValue = 0;
    private Vector2 coverSearchTimerValue_normal = new Vector2(8f, 10f);
    private bool coverExposed = false;

    //Shooting
    private float shootingTimer_aim = 0;
    private float shootingTimer_aimWait = 0;
    private float shootingTimer_shoot = 0;

    private float shootingTimer_aimValue = 0;
    private float shootingTimer_aimWaitValue = 0;
    private float shootingTimer_shootValue = 0;

    private Vector2 shootingTimer_aimValue_range = new Vector2(5, 10);
    private Vector2 shootingTimer_aimWaitValue_range = new Vector2(1f, 1.5f);
    private Vector2 shootingTimer_shootValue_range = new Vector2(2, 3);

    private void AI_Start()
    {
        //Component Setting
        rigidBody = gameObject.AddComponent<Rigidbody>();
        rigidBody.isKinematic = true;
        gameObject.GetComponent<CharacterController>().enabled = false;

        //Navigation Setting
        navAgent = gameObject.AddComponent<NavMeshAgent>();
        navAgent.agentTypeID = 0;
        navAgent.radius = 0.1f;
        navAgent.height = 0.7f;
        destinationLocation = transform.position;

        //Gun Setting
        foreach (scr_Gun gun in person.getGuns())
        {
            gun.setFireType(FireType.FullAuto);
        }
        shootingTimer_aimValue = shootingTimer_aimValue_range.y;
        shootingTimer_aimWaitValue = shootingTimer_aimWaitValue_range.y + 3;
        shootingTimer_shootValue = shootingTimer_shootValue_range.y;

        //Etc...
        person.lookInput = new Vector2(0, 0f);
    }

    private List<Vector3> testLines = new List<Vector3>();

    private void AI_Update()
    {
        // Dead
        if (person.blood <= 0)
        {
            person.moveInput = new Vector2(0, 0);
            person.key_postureToggle = false;
            person.key_sprint = false;

            person.key_shoot = false;
            person.key_aim = false;
            person.key_reload = false;
            person.key_weaponChange = false;

            navAgent.enabled = false;
            return;
        }

        //Basic Information Control
        currentPosition = person.getBone("head").position;

        //Battle Control
        battleControl();
    }

    //Combat
    #region
        
    //Battle Control
    #region
    private void battleControl()
    {
        //Target Control
        #region
        {
            //--------------- Step 1 : Target Search ---------------
            targetSearchTimer += God.gameTime;
            if (targetSearchTimer > targetSearchTimerValue)
            {
                scr_PersonController tempTarget = targetSearch(God.NPCs, person.lookInput, visibility_sightRange);
                if (tempTarget != null) { target = tempTarget; }

                targetSearchTimer = 0;
                targetSearchTimerValue = Random.Range(targetSearchTimerValue_normal.x, targetSearchTimerValue_normal.y);
            }
            target = God.PLAYER;

            //--------------- Step 2 : Target Information Setting ---------------
            if (target != null)
            {
                target_vector = target.getPerson().getBone("head").position - currentPosition;
                target_direction = Util.vec3ToDir(target_vector);
                target_visiblity = !Physics.Raycast(currentPosition, target_vector, visibility_distance, Util.VisibleLayerMask);

                if (target_visiblity)
                {
                    target_location = target.getPerson().getBone("spine").position;
                }
            }
            else
            {
                target_location = Vector3.zero;
                target_location = Vector3.zero;
                target_vector = Vector3.zero;
                target_direction = Vector2.zero;
                target_visiblity = false;
            }
        }
        #endregion

        //Cover Control
        #region
        coverSearchTimer += God.gameTime;
        RaycastHit hit;
        if (coverLocation != Vector3.zero)
        {
            coverExposed = !Physics.Raycast(coverLocation, target_location - coverLocation, out hit, visibility_distance, Util.VisibleLayerMask);
        }
        else
        {
            coverExposed = false;
        }

        if (coverSearchTimer > coverSearchTimerValue || coverExposed)
        {
            if (target != null)
            {
                coverLocation = coverSearch("Defend", target_location, 90, 5);
            }
            coverSearchTimer = 0;

            float v1 = coverSearchTimerValue_normal.x;
            float v2 = coverSearchTimerValue_normal.y;

            coverSearchTimerValue = Random.Range(v1, v2);
        }
        #endregion

        //Posture Control
        #region
        if (coverLocation != Vector3.zero)
        {
            if (destinationDistance.magnitude < 1.5f)
            {
                person.key_postureToggle = false;
                if (person.isStandingToggle()) { person.key_postureToggle = true; }
            }
            else
            {
                person.key_postureToggle = false;
                if (!person.isStandingToggle()) { person.key_postureToggle = true; }
            }
        }
        else
        {
            person.key_postureToggle = false;
            if (!person.isStandingToggle()) { person.key_postureToggle = true; }
        }
        #endregion

        //Movement Control
        #region
        navAgent.speed = person.getMaxSpeed();
        navAgent.acceleration = person.getAcceleration().x;

        if (target != null)
        {
            if (coverLocation != Vector3.zero)
            {
                destinationLocation = coverLocation;
            }
        }
        else
        {
            destinationLocation = transform.position;
        }

        navAgent.SetDestination(destinationLocation);
        destinationDistance = destinationLocation - currentPosition;

        if (navAgent.velocity.magnitude > 0.1) { person.moveInput = Vector2.right; }
        person.key_sprint = person.isStandingToggle();
        #endregion

        //Fire
        #region
        if (target_location != Vector3.zero)
        {
            scr_Gun own_gun = person.getGun();
            if (own_gun != null)
            {
                //-------------- Aim --------------
                Vector2 angle_gun = person.getWeapon_angle();
                Vector2 angle_person = person.getAngle();
                Vector2 angle_target = Util.vec3ToDir(target_location - person.getWeapon_position());

                Vector2 lookInputGoal;
                lookInputGoal.x = angle_gun.x + Util.angleDifference(angle_gun.x, angle_target.x) + Util.angleDifference(angle_gun.x, angle_person.x);// + Random.Range(-10, 10);
                lookInputGoal.y = angle_gun.y + Util.angleDifference(angle_gun.y, angle_target.y) + Util.angleDifference(angle_gun.y, angle_person.y);// + Random.Range(-10, 10);

                person.lookInput.x = Util.smoothAngleChange(person.lookInput.x, lookInputGoal.x, 10, 1);
                person.lookInput.y = Util.smoothAngleChange(person.lookInput.y, lookInputGoal.y, 10, 1);


                //-------------- Shoot --------------
                shootingTimer_aim += God.gameTime;

                if (target_visiblity) { shootingTimer_aim = shootingTimer_aimValue + 1; }
                if (shootingTimer_aim > shootingTimer_aimValue)
                {
                    person.key_aim = true;

                    shootingTimer_aimWait += God.gameTime;
                    if (shootingTimer_aimWait > shootingTimer_aimWaitValue)
                    {
                        if (person.getPosture_recoil().magnitude <= Random.Range(0, 10))
                        {
                            person.key_shoot = true;
                        }
                        else
                        {
                            person.key_shoot = false;
                        }

                        shootingTimer_shoot += God.gameTime;
                        if (shootingTimer_shoot > shootingTimer_shootValue)
                        {
                            shootingTimer_aim = 0;
                            shootingTimer_aimWait = 0;
                            shootingTimer_shoot = 0;

                            shootingTimer_aimValue = Random.Range(shootingTimer_aimValue_range.x, shootingTimer_aimValue_range.y);
                            shootingTimer_aimWaitValue = Random.Range(shootingTimer_aimWaitValue_range.x, shootingTimer_aimWaitValue_range.y);
                            shootingTimer_shootValue = Random.Range(shootingTimer_shootValue_range.x, shootingTimer_shootValue_range.y);
                        }
                    }
                }
                else
                {
                    person.key_aim = false;
                    person.key_shoot = false;
                }


                //-------------- Reload --------------
                person.key_reload = (own_gun.getBulletsInMagazine() == 0);
                if (person.getAction_reload())
                {
                    person.key_aim = false;
                    person.key_shoot = false;
                }

            }
        }
        #endregion
    }
    #endregion

    //Target Search
    #region
    private class NPCInfo
    {
        public scr_PersonController npc = null;
        public float distance;

        public NPCInfo(scr_PersonController npc, float distance)
        {
            this.npc = npc;
            this.distance = distance;
        }
    }

    private List<scr_PersonController> targetListCompose()
    {
        List<scr_PersonController> targetList = new List<scr_PersonController>();

        foreach (scr_PersonController targetCandidate in God.NPCs)
        {
        }

        return targetList;
    }
    private scr_PersonController targetSearch(List<scr_PersonController> enemyList, Vector2 searchAngle, Vector2 searchAngleRange)
    {
        //---------------- Step 0 : Information Set ----------------
        Vector3 curLocation = person.getBone("head").position;
        List<NPCInfo> targetCandidates = new List<NPCInfo>();

        //---------------- Step 1 : Pick the Candidates ----------------
        foreach (scr_PersonController pc in enemyList)
        {
            Vector3 checkLocation = pc.person.getBone("head").position;
            Vector3 checkVector = checkLocation - curLocation;

            //Self Check
            if (pc == this) { continue; }

            //Health Check
            if (pc.getPerson().blood <= 0) { continue; }

            //Distance Check
            if ((checkVector.x * checkVector.x + checkVector.y * checkVector.y) > visibility_distance * visibility_distance) { continue; }

            //Angle Check
            Vector2 angle = Util.vec3ToDir(checkVector);
            if (Mathf.Abs(Util.angleDifference(searchAngle.x, angle.x)) > searchAngleRange.x / 2) { continue; }
            if (Mathf.Abs(Util.angleDifference(searchAngle.y, angle.y)) > searchAngleRange.y / 2) { continue; }

            //Visibility Check
            RaycastHit hit;
            if (Physics.Raycast(curLocation, checkVector, out hit, visibility_distance, Util.VisibleLayerMask))
            {
                if (Util.findTopComponent<scr_PersonController>(hit.transform) != pc) { continue; }
            }

            //Register
            targetCandidates.Add(new NPCInfo(pc, checkVector.magnitude));
        }

        //---------------- Step 2 : Pick the Target ----------------
        scr_PersonController closestNPC = null;
        float closestDistance = visibility_distance;
        foreach (NPCInfo npc in targetCandidates)
        {
            if (npc.distance <= closestDistance)
            {
                closestNPC = npc.npc;
                closestDistance = npc.distance;
            }
        }
        
        //---------------- Step 3 : Return the Target ----------------
        return closestNPC;
    }
    #endregion

    //Cover Search
    #region
    private class CoverInfo
    {
        public Vector3 cover;
        public float normalAngle;
        public float distanceFromMe;
        public float distanceFromEnemy;

        public CoverInfo(Vector3 cover, float normalAngle, float distanceFromMe, float distanceFromEnemy)
        {
            this.cover = cover;
            this.normalAngle = normalAngle;
            this.distanceFromMe = distanceFromMe;
            this.distanceFromEnemy = distanceFromEnemy;
        }
    }

    private Vector3 coverSearch(string searchMode, Vector3 targetLocation, float coverTightness, float referenceDistance)
    {
        testLines = new List<Vector3>();

        float characterRadius = 0.15f;
        float coverSearchMaxDistance = 50f;
        //float minimumDistance = 1f;

        Vector3 curLocation = person.transform.position + Vector3.up * 0.25f;
        Vector3 targetToMe = curLocation - targetLocation;
        float angleTargetToMe = Util.vec2ToDir(new Vector2(targetToMe.x, targetToMe.z));
        float distanceTargetToMe = targetToMe.magnitude;

        List<CoverInfo> coverCandidates = new List<CoverInfo>();

        //---------------- Step 3 : Pick Cover Candidates ----------------
        int rayCount = 36;
        float incrediment = 360 / rayCount;
        for (int i = 0; i < rayCount; i++)
        {
            Vector3 curRayVector = Util.dirToVec3(incrediment * i + Random.Range(-incrediment / 4, incrediment / 4));

            RaycastHit[] hits_1 = Physics.RaycastAll(curLocation, curRayVector, coverSearchMaxDistance, Util.BlockLayerMask);
            RaycastHit[] hits_2 = Physics.RaycastAll(curLocation + curRayVector * coverSearchMaxDistance, -curRayVector, coverSearchMaxDistance, Util.BlockLayerMask);

            RaycastHit[] hits = new RaycastHit[hits_1.Length + hits_2.Length];
            int index = 0;
            for (int ii = 0; ii < hits_1.Length; ii++) { hits[index] = hits_1[ii]; index++; }
            for (int ii = 0; ii < hits_2.Length; ii++) { hits[index] = hits_2[ii]; index++; }

            foreach (RaycastHit hit in hits)
            {

                float coverNormalAngle = Util.vec2ToDir(new Vector2(hit.normal.x, hit.normal.z));
                Vector2 targetToCoverVector = new Vector2((hit.transform.position - targetLocation).x, (hit.transform.position - targetLocation).z);

                float targetToCoverDistance = targetToCoverVector.magnitude;
                float meToCoverDistance = (hit.transform.position - curLocation).magnitude;

                if (Mathf.Abs(Util.angleDifference(coverNormalAngle, Util.vec2ToDir(targetToCoverVector))) < coverTightness)
                {
                    coverCandidates.Add(new CoverInfo(hit.point + new Vector3(hit.normal.x, 0, hit.normal.z) * characterRadius,
                                                      coverNormalAngle,
                                                      meToCoverDistance,
                                                      targetToCoverDistance));
                }
            }
        }

        //---------------- Step 3 : Pick the Cover ----------------
        CoverInfo[] covers = coverCandidates.ToArray();
        coverMergeSort(covers);

        Vector3 defaultReturnValue = Vector3.zero;
        foreach (CoverInfo coverCandidate in covers)
        {
            defaultReturnValue = covers[covers.Length - 1].cover;

            if (searchMode.Equals("Closest"))
            {
                return coverCandidate.cover;
            }
            else if (searchMode.Equals("Assault"))
            {
                if (coverCandidate.distanceFromEnemy < distanceTargetToMe * 0.75f)
                {
                    return coverCandidate.cover;
                }
            }
            else if (searchMode.Equals("Defend"))
            {
                if (Mathf.Abs(coverCandidate.distanceFromEnemy - referenceDistance) < 2f)
                {
                    return coverCandidate.cover;
                }
            }
            else if (searchMode.Equals("FallBack"))
            {
                if (coverCandidate.distanceFromEnemy > distanceTargetToMe * 1.25f)
                {
                    return coverCandidate.cover;
                }
            }
            else
            {
                int distance = int.Parse(searchMode);
                if (Mathf.Abs(coverCandidate.distanceFromEnemy - distance) < 1f)
                {
                    return coverCandidate.cover;
                }
            }
        }

        return defaultReturnValue;
    }

    private void coverMergeSort(CoverInfo[] covers)
    {
        //Base Case
        if (covers.Length <= 1) { return; }

        //Array Divide
        int cover_1_length = covers.Length / 2;
        int cover_2_length = covers.Length - (covers.Length / 2);

        CoverInfo[] covers_1 = new CoverInfo[cover_1_length];
        CoverInfo[] covers_2 = new CoverInfo[cover_2_length];

        for (int i = 0; i < cover_1_length; i++) { covers_1[i] = covers[i]; }
        for (int i = 0; i < cover_2_length; i++) { covers_2[i] = covers[i + cover_1_length]; }

        //Recursion
        coverMergeSort(covers_1);
        coverMergeSort(covers_2);

        //Sort
        int pointer = 0;
        int pointer_1 = 0;
        int pointer_2 = 0;

        while (pointer_1 < cover_1_length && pointer_2 < cover_2_length)
        {
            if (covers_1[pointer_1].distanceFromMe <= covers_2[pointer_2].distanceFromMe)
            {
                covers[pointer] = covers_1[pointer_1];
                pointer_1++;
            }
            else
            {
                covers[pointer] = covers_2[pointer_2];
                pointer_2++;
            }
            pointer++;
        }

        while (pointer_1 < cover_1_length) { covers[pointer] = covers_1[pointer_1]; pointer_1++; pointer++; }
        while (pointer_2 < cover_2_length) { covers[pointer] = covers_2[pointer_2]; pointer_2++; pointer++; }
    }
    #endregion
    #endregion

    public NavMeshAgent getNavAgent() { return navAgent; }
    #endregion
}