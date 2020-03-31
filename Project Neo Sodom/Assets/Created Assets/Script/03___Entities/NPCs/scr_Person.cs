using UnityEngine;
using System.Collections.Generic;

public class scr_Person : MonoBehaviour
{
    public float blood = 100;

    //Components
    private Animator comp_animator = null;
    private float comp_animator_disabledTime = 0;

    private CharacterController comp_characterController = null;
    private scr_PersonController comp_personController = null;

    //Transform Information
    private Vector3 position = Vector3.zero;
    private Vector3 position_pre = Vector3.zero;
    private Vector2 rotation = Vector2.zero;
    private Vector2 rotation_pre = Vector2.zero;
    private float state_cover_rotation = 0;

    //Input
    [HideInInspector] public Vector2 moveInput;
    [HideInInspector] public Vector2 lookInput;

    [HideInInspector] public bool key_postureToggle = false;
    [HideInInspector] public bool key_aim = false;
    [HideInInspector] public bool key_aimProper = false;
    [HideInInspector] public bool key_shoot = false;
    [HideInInspector] public bool key_weaponChange = false;
    [HideInInspector] public bool key_sprint = false;
    [HideInInspector] public bool key_reload = false;
    [HideInInspector] public bool key_cameraSideInvert = false;


    private void Start()
    {
        //Model
        model = transform.GetChild(1);
        model_hip = model.GetChild(0).GetChild(4);

        hand_right = model_hip.GetChild(0).GetChild(2).GetChild(0).GetChild(0).GetChild(0).GetChild(1);

        rotation = new Vector2(Random.Range(0, 360), 0);
        rotation_pre = rotation;

        //Bone Register
        bone = transform.GetChild(0);
        bone_hip = bone.GetChild(0).GetChild(4);
        bone_spine = bone_hip.GetChild(0);
        bone_head = bone_spine.GetChild(0).GetChild(0);

        bone_right_shoulder = bone_spine.GetChild(2);
        bone_right_upperArm = bone_right_shoulder.GetChild(0);
        bone_right_lowerArm = bone_right_upperArm.GetChild(0);
        bone_right_hand = bone_right_lowerArm.GetChild(0);

        bone_right_upperLeg = bone_hip.GetChild(2);
        bone_right_lowerLeg = bone_right_upperLeg.GetChild(0);

        bone_left_shoulder = bone_spine.GetChild(1);
        bone_left_upperArm = bone_left_shoulder.GetChild(0);
        bone_left_lowerArm = bone_left_upperArm.GetChild(0);
        bone_left_hand = bone_left_lowerArm.GetChild(0);

        bone_left_upperLeg = bone_hip.GetChild(1);
        bone_left_lowerLeg = bone_left_upperLeg.GetChild(0);

        //Component Set
        comp_animator = bone.gameObject.GetComponent<Animator>();

        comp_characterController = GetComponent<CharacterController>();
        comp_personController = GetComponent<scr_PersonController>();

        //Weapon Set
        for (int i = 0; i < guns.Count; i++)
        {
            if(guns[i] == null) { continue; }
            guns[i] = Instantiate(guns[i]);
            guns[i].GetComponent<scr_Gun>().setGunHolder(gameObject);
        }

        if (guns.Count > 0) { gun = guns[0]==null?null:guns[0].GetComponent<scr_Gun>(); }

        if (gun != null)
        {
            Transform gunObject = gun.transform;
            gunObject.parent = hand_right;
            gunObject.localPosition = new Vector3(0, 0, 0);
            gunObject.localEulerAngles = new Vector3(0, 0, 0);
            for (int i = 0; i < gun.transform.GetChild(0).childCount; i++)
            {
                gun.transform.GetChild(0).GetChild(i).GetComponent<MeshRenderer>().enabled = true;
            }
        }

        //Initialization Set
        God.CAMERA.reset(transform.eulerAngles.y + 90f);
        enableRagdoll(false);
        modelCatchUp(bone_hip, model_hip, 1);
        model_hip.localPosition = bone_hip.localPosition;
    }

    private void Update()
    {
        stateControl();

        InteractiveControl();
        actionControl();

        movementControl();
        rotationControl();

        weaponGunControl();

        animationControl();
        postureControl();
        finishControl();
    }

    private void LateUpdate()
    {
        movementSet();
        postureSet();
    }


    //Control
    #region


    //State
    #region
    private bool state_limp = false;
    private bool state_stand = true;
    private bool state_stand_toggle = true;
    private bool state_move = false;
    private bool state_walk = false;

    private bool state_holding = false;
    private bool state_handing = false;
    private sys_Item holdingObject = null;

    private bool state_cover = false;
    private bool state_cover_aimClear = false;
    private float state_cover_angle = 0;
    private float state_cover_detectDistance = 0.5f;

    private bool state_sprint = false;
    private bool state_sprint_chance = true;

    private bool state_aim = false;
    private bool state_aimProper = false;

    private bool state_armed = false;

    private bool state_shoot_triggered = false;
    private bool state_shoot_moment = false;

    private bool state_firstPerson = false;
    
    public void hold(sys_Item item) { holdingObject = item; if (item == null) { return; } item.setHolder(this); }
    private void stateControl()
    {
        if (Input.GetKeyDown(KeyCode.H)) { state_firstPerson = !state_firstPerson; }
        state_holding = holdingObject != null;
        state_handing = state_holding && key_aim;

        //Movement Related
        state_move = !moveInput.Equals(Vector2.zero);
        state_walk = state_move && !state_sprint;

        //Sprint
        if (state_sprint_chance)
        {
            state_sprint = state_move && key_sprint && !state_limp;
            state_sprint_chance = !state_sprint;
        }
        if (state_sprint)
        {
            if (moveInput.magnitude < 0.5 || state_aim || (key_postureToggle && state_stand) || state_limp)
            {
                state_sprint = false;
                state_sprint_chance = true;
            }
        }

        //Posture - Stand/Crouch
        if (key_postureToggle) { state_stand_toggle = !state_stand_toggle; }
        if (state_sprint) { state_stand_toggle = true; }
        state_stand = state_stand_toggle;

        state_cover = false;
        state_cover_aimClear = false;

        if (!state_stand_toggle)
        {
            state_cover_angle = lookInput.x;
            Vector3 position = transform.position + new Vector3(0, 0.1f, 0);
            Vector3 direction = Util.dirToVec3(state_cover_angle);

            state_cover_aimClear = !Physics.Raycast(position + Vector3.up * 0.5f, direction, state_cover_detectDistance, Util.BlockLayerMask);
            if (Physics.Raycast(position, direction, state_cover_detectDistance, Util.BlockLayerMask))
            {
                state_stand = state_aim && state_cover_aimClear;
                key_aimProper = true;
                state_cover = true;
            }
        }

        //Gun Related
        state_armed = (gun != null);
        state_aim = state_handing || (state_armed && key_aim && (comp_animator_disabledTime < 0) && (!state_cover || state_cover_aimClear)); 
        state_aimProper = state_aim && key_aimProper;
        state_shoot_triggered = state_armed && state_aim && key_shoot;
        state_shoot_moment = gun != null && gun_lastShootTime == 0;
    }
    #endregion


    //UI
    #region
    private void InteractiveControl()
    {
        sys_Interactable closestInteractive = sys_Interactable.getClosestInteractive(bone_spine.position, 0.5f);
        if (closestInteractive != null)
        {
            float distance = (closestInteractive.transform.position - transform.position).magnitude;
            if (distance < 1f && Input.GetKeyDown(KeyCode.E))
            {
                closestInteractive.interact(this);
            }
        }
    }
    #endregion


    //Action
    #region
    private bool[] action_slide = { false, false, false };
    private bool action_slide_started = false;
    private bool action_slide_current = false;
    private bool action_slide_pre = false;

    private bool[] action_reload = { false, false, false };
    private bool action_reload_started = false;
    private bool action_reload_current = false;
    private bool action_reload_pre = false;
    private void actionControl()
    {
        //Slide
        #region
        action_slide_pre = action_slide_current;
        if (key_postureToggle && !action_reload[1] && !state_stand_toggle && speed > speedSprint * 0.75f && God.PLAYER == comp_personController) { action_slide_current = true; }
        if (action_slide_current)
        {
            if (!action_slide_started) { action_slide_started = isThisAnimationPlaying(0, "Slide"); }
            if (action_slide_started) { action_slide_current = isThisAnimationPlaying(0, "Slide"); }
        }
        else
        {
            action_slide_started = false;
        }

        action_slide[0] = !action_slide_pre && action_slide_current;
        action_slide[1] = action_slide_current;
        action_slide[2] = action_slide_pre && !action_slide_current;
        #endregion

        //Reload
        #region
        action_reload_pre = action_reload_current;
        if (key_reload) { action_reload_current = true; }
        if (action_reload_current)
        {
            if (!action_reload_started) { action_reload_started = isThisAnimationPlaying(1, "Reload"); }
            if ( action_reload_started) { action_reload_current = isThisAnimationPlaying(1, "Reload"); }
        }
        else
        {
            action_reload_started = false;
        }

        action_reload[0] = !action_reload_pre && action_reload_current;
        action_reload[1] = action_reload_current;
        action_reload[2] = action_reload_pre && !action_reload_current;
        #endregion
    }
    #endregion


    //Movement
    #region
    [SerializeField] private float speedWalk = 0.6f;
    [SerializeField] private float speedLimp = 0.9f;
    [SerializeField] private float speedCrawl = 0.4f;
    [SerializeField] private float speedSprint = 1.5f;
    [SerializeField] private float speedSlide = 2f;
    [SerializeField] private float speedAim = 0.4f;
    [SerializeField] private float speedAimProper = 0.2f;

    [SerializeField] private Vector2 speedAccel_normal = new Vector2(1f, 1f);
    [SerializeField] private Vector2 speedAccel_sprint = new Vector2(1f, 1f);
    [SerializeField] private Vector2 speedAccel_slide = new Vector2(1f, 1f);
    private Vector2 speedAccel = new Vector2(1f, 1f);

    private float speed_max = 0;

    private Vector2 moveStack = Vector2.zero;
    private Vector2 speed_axis = Vector2.zero;
    private Vector2 speed_axis_rotation = Vector2.zero;
    private Vector2 speed_axis_position = Vector2.zero;
    private float speed_axis_z = 0;

    private float speed_dir;
    private float speed_dir_position = 0;

    private float speed;
    private float speed_position = 0;

    private float speed_position_rotation_mix = 1;

    private void movementControl()
    {
        // ------------------- Step 0 - Input Information ------------------- 
        float moveInput_dir = Vector2.SignedAngle(Vector2.right, moveInput);
        float moveInput_dis = Mathf.Clamp(moveInput.magnitude, 0, 1);
        float angle_towards = lookInput.x;
        float accelerationAngle = angle_towards + moveInput_dir - 90;

        // ------------------- Step 1 - Basic Property Set ------------------- 
        //Max Speed
        speed_max = speedWalk;
        if (state_limp) { speed_max = speedLimp; }
        if (!state_stand) { speed_max = speedCrawl; }
        if (state_sprint) { speed_max = speedSprint; }
        if (state_aim) { speed_max = speedAim; }
        if (state_aimProper) { speed_max = speedAimProper; }

        float speed_maxFinal = speed_max * moveInput_dis;


        // ------------------- Step 1.5 - Action/Acceleration -------------------
        speedAccel = speedAccel_normal;
        if (state_sprint) { speedAccel = speedAccel_sprint; }

        if (action_slide[0]) { speed_axis_position = Util.dirToVec2(speed_dir) * speedSlide; }
        if (action_slide[1])
        {
            speedAccel = speedAccel_slide;
            moveInput_dis = 0;
        }

        // ------------------- Step 2 - Speed x, y Axis Accelerate ------------------- 
        //Rotation-Related Speed
        Vector2 speed_axis_rotation_goal;
        if (state_aim)
        {
            float spinningRadious = 0.15f;
            Vector2 pivot = new Vector2(position.x, position.z) + Util.dirToVec2(rotation_pre.x) * -spinningRadious;
            Vector2 rotationPosition = pivot + Util.dirToVec2(rotation.x) * spinningRadious;
            speed_axis_rotation_goal = rotationPosition - new Vector2(position.x, position.z);
        }
        else
        {
            speed_axis_rotation_goal = Vector2.zero;
        }
        speed_axis_rotation = Util.smoothChange(speed_axis_rotation, speed_axis_rotation_goal, 10, 1);

        //Acceleration
        if (moveInput_dis > 0)
        {
            float beforeSpeed = speed_axis_position.magnitude;
            speed_axis_position += Util.dirToVec2(accelerationAngle) * speedAccel.x * God.gameTime;
            float afterSpeed = speed_axis_position.magnitude;

            if (afterSpeed > speed_maxFinal)
            {
                if (beforeSpeed > speed_maxFinal) { speed_axis_position = Util.dirToVec2(Util.vec2ToDir(speed_axis_position)) * beforeSpeed; }
                if (beforeSpeed <= speed_maxFinal) { speed_axis_position = Util.dirToVec2(Util.vec2ToDir(speed_axis_position)) * speed_maxFinal; }
            }
        }
        //Deceleration
        if (speed_axis_position.magnitude > speed_maxFinal)
        {
            Vector2 speed_before = speed_axis_position;
            speed_axis_position -= Util.dirToVec2(speed_dir_position) * speedAccel.y * God.gameTime;

            if (speed_maxFinal == 0)
            {
                if (speed_before.x * speed_axis_position.x <= 0 && speed_before.y * speed_axis_position.y <= 0)
                {
                    speed_axis_position = Vector2.zero;
                }
            }
            else
            {
                if (speed_axis_position.magnitude < speed_maxFinal)
                {
                    speed_axis_position = Util.dirToVec2(Util.vec2ToDir(speed_axis_position)) * speed_maxFinal;
                }
            }
        }


        // ------------------- Step 3 - Reflect -------------------
        if (comp_characterController.enabled)
        {
            if (blood > 0)
            {
                comp_characterController.Move(new Vector3(speed_axis_position.x * God.gameTime, 0, speed_axis_position.y * God.gameTime));
                if (state_stand_toggle && !state_aim && false) { comp_characterController.Move(new Vector3(speed_axis_rotation.x, 0, speed_axis_rotation.y)); }
            }
            //Gravity
            if (comp_characterController.isGrounded) { speed_axis_z = 0; }
            else { speed_axis_z -= God.gravityAcceleration * God.gameTime; }
            comp_characterController.Move(new Vector3(0, speed_axis_z * God.gameTime, 0));
        }
        else
        {
            Vector3 velocity = comp_personController.getNavAgent().velocity;
            speed_axis_position = new Vector2(velocity.x, velocity.z);
        }


        // ------------------- Step 4 - Finish -------------------
        speed_dir_position = Vector2.SignedAngle(Vector2.right, speed_axis_position);
        speed_position = speed_axis_position.magnitude;


        //Determine position-rotation speed ratio

        Vector3 movedVector = transform.position - position_pre;
        position_pre = transform.position;

        speed_axis = speed_axis_position + (speed_axis_rotation * speed_position_rotation_mix * Util.NORMAL_FRAMERATE);
        speed_dir = Util.vec2ToDir(new Vector2(movedVector.x, movedVector.z));
        speed = (movedVector.magnitude + speed_axis_rotation.magnitude) / God.gameTime;
    }
    #endregion


    //Rotation
    #region
    private void rotationControl()
    {
        rotation.x -= state_cover_rotation;
        rotation_pre = rotation;

        float state_cover_rotation_goal = 0;

        //Aim
        if (state_aim)
        {
            float rotationSpeed = 5;
            if (state_limp) { rotationSpeed = 15; }
            rotation.x = Util.smoothAngleChange(rotation.x, lookInput.x, rotationSpeed, 1f);
        }
        else
        {
            //Movement
            if (state_move)
            {
                float differenceAngle = Util.angleDifference(rotation.x, speed_dir_position);

                float rotationAngle = (120f * Mathf.Sin((Mathf.Abs(differenceAngle)/2)*Mathf.Deg2Rad) + 90f) * ((1 + speed_max) / 1.4f) * God.gameTime;

                if (Mathf.Abs(differenceAngle) < rotationAngle) { rotation.x = speed_dir_position; }
                else { rotation.x += Mathf.Sign(differenceAngle) * rotationAngle; }

            }
            //Cover
            if (!state_stand_toggle)
            {
                RaycastHit hit;
                Vector3 position = transform.position + new Vector3(0, 0.1f, 0);
                Vector3 direction = Util.dirToVec3(rotation.x);

                float coverDetectDistance = state_cover_detectDistance;
                if (Physics.Raycast(position, direction, out hit, coverDetectDistance, Util.BlockLayerMask))
                {
                    float cover_angle = Util.vec2ToDir(new Vector2(-hit.normal.x, -hit.normal.z));

                    Physics.Raycast(position, new Vector3(-hit.normal.x, 0, -hit.normal.z), out hit, coverDetectDistance, Util.BlockLayerMask);
                    float cover_distance = hit.distance;

                    float coverFixAngle = Mathf.Acos(cover_distance / coverDetectDistance) * Mathf.Rad2Deg;
                    float candidate_1 = Util.angleDifference(rotation.x, cover_angle + coverFixAngle + 35);
                    float candidate_2 = Util.angleDifference(rotation.x, cover_angle - coverFixAngle - 30);

                    if (Mathf.Abs(candidate_1) < Mathf.Abs(candidate_2))
                    {
                        state_cover_rotation_goal = candidate_1;
                    }
                    else
                    {
                        state_cover_rotation_goal = candidate_2;
                    }
                }
            }
        }
        state_cover_rotation = Util.smoothAngleChange(state_cover_rotation, state_cover_rotation_goal, 15, 1);
        rotation.x += state_cover_rotation;


        if (gun != null) { rotation.y = Util.vec3ToDir(gun.getAngle()).y; }
    }
    #endregion


    //Weapon
    #region
    [SerializeField] private List<GameObject> guns = new List<GameObject>();
    private scr_Gun gun = null;
    private Vector2 gun_angle = Vector2.zero;
    private Vector3 gun_position = Vector3.zero;

    private int gunIndex = 0;
    private bool gun_doubleHanded = false;
    private bool gun_properlyAimed = false;
    private PostureType gun_aimPosture = PostureType.OneHanded;
    private float gun_aim_time = 0;
    private float gun_aimProper_time = 0;
    private float gun_lastShootTime = 0;
    private void weaponGunControl()
    {
        //Guns Control
        if (key_weaponChange)
        {
            if ( gun != null )
            {
                for (int i = 0; i < gun.transform.GetChild(0).childCount; i++)
                {
                    gun.transform.GetChild(0).GetChild(i).GetComponent<MeshRenderer>().enabled = false;
                }
            }
            int gunNum = guns.Count;
            gunIndex = (gunIndex + gunNum + 1 ) % gunNum;
            gun = guns[gunIndex]==null?null:guns[gunIndex].GetComponent<scr_Gun>();

            if ( gun != null )
            {
                Transform gunObject = gun.transform;
                gunObject.parent = hand_right;
                gunObject.localPosition = new Vector3(0, 0, 0);
                gunObject.localEulerAngles = new Vector3(0, 0, 0);
                for (int i = 0; i < gun.transform.GetChild(0).childCount; i++)
                {
                    gun.transform.GetChild(0).GetChild(i).GetComponent<MeshRenderer>().enabled = true;
                }
            }
        }

        //Gun Control
        if (gun != null)
        {
            Vector3 checkingPivot = new Vector3(transform.position.x, bone_head.position.y, transform.position.z);
            bool frontClear = !Physics.Raycast(checkingPivot, Util.dirToVec3(lookInput.x), 0.55f, Util.BlockLayerMask);

            if (state_aim && frontClear)
            {
                gun_aimPosture = gun.getPosture_aim();
                gun_aim_time += God.gameTime;

                if (state_aimProper)
                {
                    gun_aimPosture = gun.getPosture_aimProper();
                    gun_aimProper_time += God.gameTime;
                }
                else { gun_aimProper_time = 0; }

                gun_properlyAimed = gun_aimPosture == PostureType.DoubleHanded || gun_aimPosture == PostureType.TacticalShot;
                gun_doubleHanded = gun_aimPosture == PostureType.HipShot || gun_aimPosture == PostureType.TacticalShot;
            }
            else
            {
                gun_aim_time = 0;
                gun_properlyAimed = false;
                gun_doubleHanded = gun.getGunType() == GunType.Rifle;
            }

            //Reload Control
            if (action_reload[0]) { gun.reload(gun.getMagazineSize()); }

            //Shoot Control
            if (state_shoot_triggered && frontClear && !action_reload[1]) { gun.pullTrigger(); }
            gun.operate();
            if (gun.isFired()) { gun_lastShootTime = 0; }
            else { gun_lastShootTime += God.gameTime; }
        }
        else
        {
            gun_doubleHanded = false;
            gun_properlyAimed = false;
        }
    }
    #endregion


    //Animation
    #region 
    private float posture_stand = 1;
    private float posture_limp = 0;
    private float posture_armed = 0;
    private float posture_gun_doubleHanded = 0;
    private float posture_gun_aim = 0;
    private float posture_gun_aim_properness = 0;
    private float posture_holding = 0;

    private Vector2 animationMovingSpeed = Vector2.zero;

    private void animationControl()
    {
        if (comp_animator != null)
        {
            //Enable Animator
            if (comp_animator_disabledTime <= 0 && !comp_animator.enabled )
            {
                enableRagdoll(false);
            }
            else
            {
                comp_animator_disabledTime -= God.gameTime;
            }

            //Posture
            if (state_stand) { posture_stand = Util.smoothChange(posture_stand, 1, 15, 1); }
            else { posture_stand = Util.smoothChange(posture_stand, state_holding&&!state_move?0.1f:0, 15, 1); }

            if (state_limp) { posture_limp = Util.smoothChange(posture_limp, 1, 15, 1); }
            else { posture_limp = Util.smoothChange(posture_limp, 0, 15, 1); }

            /*
            if (state_holding) { posture_holding = Util.smoothChange(posture_holding, 1, 15, 1); }
            else { posture_holding = Util.smoothChange(posture_holding, 0, 15, 1); }
            */

            //Movement
            float ratio = 24.5f;
            float relativeAngle = (speed_dir - rotation.x + 90);
            animationMovingSpeed = Util.smoothChange(animationMovingSpeed, Util.dirToVec2(relativeAngle)*speed, 5, 1);


            //Weapon
            if (state_armed || state_holding) { posture_armed = Util.smoothChange(posture_armed, 1, 10, 1); }
            else { posture_armed = Util.smoothChange(posture_armed, 0, 10, 1); }
            
            if (gun_doubleHanded) { posture_gun_doubleHanded = Util.smoothChange(posture_gun_doubleHanded, 1, 10, 1);   }
            else { posture_gun_doubleHanded = Util.smoothChange(posture_gun_doubleHanded, 0, 10, 1); }

            if (gun_aim_time > 0 || state_handing) { posture_gun_aim = Util.smoothChange(posture_gun_aim, state_holding ? 0.6f : 1, 10, 1); }
            else { posture_gun_aim = Util.smoothChange(posture_gun_aim, 0, 10, 1); }

            if (gun_properlyAimed) { posture_gun_aim_properness = Util.smoothChange(posture_gun_aim_properness, 1, 10, 1); }
            else { posture_gun_aim_properness = Util.smoothChange(posture_gun_aim_properness, 0, 10, 1); }

            
            //Reflect Values
            comp_animator.SetFloat("State Stand", posture_stand);
            comp_animator.SetFloat("State Limp", posture_limp);
            comp_animator.SetFloat("State Holding", posture_holding);

            comp_animator.SetFloat("Speed X", animationMovingSpeed.x * ratio);
            comp_animator.SetFloat("Speed Y", animationMovingSpeed.y * ratio);
            comp_animator.SetFloat("Speed", Mathf.Clamp((animationMovingSpeed.magnitude - 0.55f) / 1.2f, 0, 1));

            comp_animator.SetFloat("State Armed", posture_armed);
            comp_animator.SetFloat("State Big Gun", posture_gun_doubleHanded);
            comp_animator.SetFloat("State Aim", posture_gun_aim);
            comp_animator.SetFloat("State Legit Aim", posture_gun_aim_properness);

            comp_animator.SetBool("Slide Play", action_slide[0]);
            comp_animator.SetBool("Reload Play", action_reload[0]);
        }
    }
    #endregion


    //Posture
    #region
    private float ragdollResemble = 1;
    private float ragdollCatchedUp = 0;

    private Transform model = null;
    private Transform model_hip = null;

    private Transform hand_right = null;

    private Transform bone = null;
    private Transform bone_hip = null;
    private Transform bone_spine = null;
    private Transform bone_head = null;

    private Transform bone_right_shoulder = null;
    private Transform bone_right_upperArm = null;
    private Transform bone_right_lowerArm = null;
    private Transform bone_right_hand = null;

    private Transform bone_left_upperLeg = null;
    private Transform bone_left_lowerLeg = null;


    private Transform bone_left_shoulder = null;
    private Transform bone_left_upperArm = null;
    private Transform bone_left_lowerArm = null;
    private Transform bone_left_hand = null;

    private Transform bone_right_upperLeg = null;
    private Transform bone_right_lowerLeg = null;

    public float posture_bending = 0;
    private float posture_lookAround = 0;
    private Vector2 posture_gun_recoil = Vector2.zero;
    private Vector2 posture_gun_recoil_speed = Vector2.zero;

    private void postureControl()
    {
        //Body Bending
        float bendingAngle_goal;
        float lookAroundAngle_goal;
        if (state_aim)
        {
            bendingAngle_goal = lookInput.y;
            lookAroundAngle_goal = Util.angleDifference(rotation.x, lookInput.x);
        }
        else
        {
            bendingAngle_goal = 0;
            lookAroundAngle_goal = 0;
        }

        posture_bending = Util.smoothAngleChange(posture_bending, bendingAngle_goal, 10, 1);
        posture_lookAround = Util.smoothChange(posture_lookAround, lookAroundAngle_goal, 10, 0.5f);

        while (posture_bending > 180) { posture_bending -= 360; }
        while (posture_bending <= -180) { posture_bending += 360; }


        //Recoil
        if (state_shoot_moment)
        {
            posture_gun_recoil_speed = Util.dirToVec2(gun.getRecoil_angle()) * gun.getRecoil_kick();
        }
        else
        {
            //Recoil Speed
            if (posture_gun_recoil_speed != Vector2.zero)
            {
                Vector2 recoilSpeedRecover = Util.dirToVec2(Util.vec2ToDir(posture_gun_recoil_speed)) *
                                             gun.getRecoil_comeback() *
                                             Util.NORMAL_FRAMERATE *
                                             God.gameTime;

                if (posture_gun_recoil_speed.magnitude > recoilSpeedRecover.magnitude)
                {
                    posture_gun_recoil_speed -= recoilSpeedRecover;
                }
                else
                {
                    posture_gun_recoil_speed = Vector2.zero;
                }
            }
            //Recoil
            else
            {
                posture_gun_recoil = Util.smoothChange(posture_gun_recoil, Vector2.zero, 15, 2);
            }
        }
        posture_gun_recoil += posture_gun_recoil_speed * God.gameTime * Util.NORMAL_FRAMERATE;
        if (God.PLAYER == comp_personController) { God.CAMERA.setRecoil(posture_gun_recoil_speed / 5); }

        //Model - Bone Synchronize
        ragdollResemble = 1;
        if (comp_animator_disabledTime <= 0)
        {
            if (ragdollCatchedUp < 1f) { ragdollResemble = 10; }
            ragdollCatchedUp += God.gameTime;
        }
        else
        {
            ragdollCatchedUp = 0;
        }
    }
    #endregion
    

    private void finishControl()
    {
        transform.rotation = Quaternion.Euler(new Vector3(0,-rotation.x + 90, 0));
    }
    #endregion
    

    //Late Update
    private void movementSet()
    {

    }

    private void postureSet()
    {
        //Body Bending
        Vector3 rotateAxis_x = new Vector3(0, 1, 0);
        Vector3 rotateAxis_y = Util.dirToVec3(rotation.x + 90);
        bone_spine.RotateAround(bone_spine.position, rotateAxis_y, posture_bending * 0.5f);
        bone_spine.RotateAround(bone_spine.position, rotateAxis_x,-posture_lookAround);
        if (!(posture_gun_doubleHanded < 0.9f && posture_gun_aim_properness < 0.9f))
        {
            bone_left_shoulder.RotateAround(bone_left_shoulder.position, rotateAxis_y, posture_bending * 0.5f);
        }
        bone_right_shoulder.RotateAround(bone_right_shoulder.position, rotateAxis_y, posture_bending * 0.5f);
        
        //Recoil Apply
        if (gun != null)
        {
            gun_angle = Util.vec3ToDir(gun.getAngle());
            gun_position = gun.getGunPoint().position;
        }
        // Single Handed Handgun
        #region
        if (posture_gun_doubleHanded < 0.9f && posture_gun_aim_properness < 0.9f) 
        {
            float posture_gun_recoil_magnitude = Mathf.Clamp(posture_gun_recoil.magnitude/2,0,20);
            boneRotate(bone_spine, posture_gun_recoil_magnitude, posture_gun_recoil.y / 20);
            boneRotate(bone_right_shoulder, posture_gun_recoil, 0.05f, 0.05f);
            boneRotate(bone_right_upperArm, -posture_gun_recoil_magnitude, 0);
            boneRotate(bone_right_upperArm, posture_gun_recoil, 0.1f,-0.6f);
            boneRotate(bone_right_lowerArm, posture_gun_recoil, 0.9f, 1.1f);
            boneRotate(bone_head,-posture_gun_recoil_magnitude*0.9f,0);
        }
        #endregion

        // Double Handed Handgun
        #region
        if (posture_gun_doubleHanded < 0.9f && posture_gun_aim_properness >= 0.9f) 
        {
            Vector2 recoil = posture_gun_recoil/2;

            float posture_gun_recoil_magnitude = posture_gun_recoil.magnitude/2;
            boneRotate(bone_spine, posture_gun_recoil.x, posture_gun_recoil_magnitude);

            boneRotate(bone_left_shoulder, recoil, 0.01f, 0.01f);
            boneRotate(bone_left_upperArm, recoil, 0.02f,-0.5f);
            boneRotate(bone_left_lowerArm, recoil, 0.95f, 0.95f);

            boneRotate(bone_right_shoulder, recoil, 0.01f, 0.01f);
            boneRotate(bone_right_upperArm, recoil, 0.02f,-0.5f);
            boneRotate(bone_right_lowerArm, recoil, 0.95f, 0.95f);

            boneRotate(bone_head, 0,-posture_gun_recoil_magnitude*0.9f);
        }
        #endregion

        // Rifle Hipshot
        #region
        if (posture_gun_doubleHanded >= 0.9f && posture_gun_aim_properness < 0.9f) 
        {
            Vector2 recoil = posture_gun_recoil / 2;

            float posture_gun_recoil_magnitude = posture_gun_recoil.magnitude / 2;
            boneRotate(bone_spine, posture_gun_recoil.x, posture_gun_recoil_magnitude);

            boneRotate(bone_left_shoulder, recoil, 0.2f, 0.2f);
            boneRotate(bone_left_upperArm, recoil, 0.01f, -0.25f);
            boneRotate(bone_left_lowerArm, recoil, 0.25f, 0.25f);

            boneRotate(bone_right_shoulder, recoil, 0.6f, 0.6f);
            boneRotate(bone_right_upperArm, recoil, 0.01f, -0.25f);
            boneRotate(bone_right_lowerArm, recoil, 0.25f, 0.25f);

            boneRotate(bone_head, 0, -posture_gun_recoil_magnitude * 0.9f);
        }
        #endregion

        // Rifle Properly Aim
        #region
        if (posture_gun_doubleHanded >= 0.9f && posture_gun_aim_properness >= 0.9f) 
        {
            Vector2 recoil = posture_gun_recoil / 3;

            float posture_gun_recoil_magnitude = posture_gun_recoil.magnitude / 2;
            boneRotate(bone_spine, posture_gun_recoil.x, posture_gun_recoil_magnitude);

            boneRotate(bone_left_shoulder, recoil, 0.01f, 0.01f);
            boneRotate(bone_left_upperArm, recoil, 0.02f, -0.5f);
            boneRotate(bone_left_lowerArm, recoil, 0.95f, 0.95f);

            boneRotate(bone_right_shoulder, recoil, 0.01f, 0.01f);
            boneRotate(bone_right_upperArm, recoil, 0.02f, -0.5f);
            boneRotate(bone_right_lowerArm, recoil, 0.95f, 0.95f);

            boneRotate(bone_head, 0, -posture_gun_recoil_magnitude * 0.9f);
        }
        #endregion

        modelCatchUp(bone_hip, model_hip, ragdollResemble);
    }

    private float modelCatchUp(Transform bone, Transform model, float percentage)
    {
        float returnValue = 0;
        
        model.localRotation = Quaternion.RotateTowards(model.localRotation,
                                                       bone.localRotation,
                                                       Quaternion.Angle(model.localRotation, bone.localRotation) / percentage);

        int childCount = (int)Mathf.Min(new float[] { bone.childCount, model.childCount });
        for (int i = 0; i < childCount; i++)
        {
            returnValue += modelCatchUp(bone.GetChild(i), model.GetChild(i), percentage);
        }

        if ( bone == bone_hip)
        {
            model_hip.localPosition = Util.smoothChange(model_hip.localPosition, bone_hip.localPosition, percentage, 1);
        }
        return returnValue + Quaternion.Angle(model.localRotation, bone.localRotation);
    }

    public void damage(Transform damagePart, float damage)
    {

        if (damagePart == getBone("head")) { blood -= damage * 5; }

        if (damagePart == getBone("right upper arm") ||
            damagePart == getBone("right lower arm") ||
            damagePart == getBone("left upper arm") ||
            damagePart == getBone("left lower arm"))
        {
            blood -= damage * 0.5f;
        }

        if (damagePart == getBone("right upper leg") ||
            damagePart == getBone("right lower leg") ||
            damagePart == getBone("left upper leg") ||
            damagePart == getBone("left lower leg"))
        {
            blood -= damage * 0.25f;
            state_limp = true;
        }

        if ( blood < 40) { state_limp = true; }
    }

    //Helper Methods
    #region
    public sys_Item getHoldingObject() { return holdingObject; }
    public void enableRagdoll(bool enable)
    {
        comp_animator.enabled = !enable;
        Rigidbody[] rigidBodies = bone.GetComponentsInChildren<Rigidbody>();
        foreach (Rigidbody rigidBody in rigidBodies)
        {
            rigidBody.isKinematic = !enable;
            rigidBody.useGravity = enable;
        }
        if (enable) { comp_animator_disabledTime = 1000000000; }
    }
    public void enableRagdoll(float time)
    {
        comp_animator.enabled = false;
        Rigidbody[] rigidBodies = bone.GetComponentsInChildren<Rigidbody>();
        foreach (Rigidbody rigidBody in rigidBodies)
        {
            rigidBody.isKinematic = false;
            rigidBody.useGravity = true;
        }
        comp_animator_disabledTime = time;
    }

    public void boneRotate(Transform bone, float angle_x, float angle_y)
    {
        Vector3 rotateAxis_x = new Vector3(0, 1, 0);
        Vector3 rotateAxis_y = Util.dirToVec3(rotation.x + 90);

        bone.RotateAround(bone.position, rotateAxis_x, angle_x);
        bone.RotateAround(bone.position, rotateAxis_y, angle_y);
    }

    public void boneRotate(Transform bone, Vector2 angle, float ratio_x, float ratio_y)
    {
        Vector3 rotateAxis_x = new Vector3(0, 1, 0);
        Vector3 rotateAxis_y = Util.dirToVec3(rotation.x + 90);

        bone.RotateAround(bone.position, rotateAxis_x, angle.x * ratio_x);
        bone.RotateAround(bone.position, rotateAxis_y, angle.y * ratio_y);
    }

    public void boneRotate_z(Transform bone, float angle_z)
    {
        Vector3 rotateAxis = Util.dirToVec3(rotation.x + 90);

        bone.RotateAround(bone.position, rotateAxis, angle_z);
    }

    public bool isThisAnimationPlaying(int layer, string stateName)
    {
        if (comp_animator == null) { return false; }
        if (comp_animator.GetCurrentAnimatorStateInfo(layer).IsName(stateName) &&
            comp_animator.GetCurrentAnimatorStateInfo(layer).normalizedTime < 1.0f)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    #endregion

    //Getter & Setter
    #region
    public float getMaxSpeed() { return speed_max; }
    public Vector2 getAcceleration() { return speedAccel; }
    public Vector2 getAngle() { return rotation; }
    public bool isAimming() { return state_aim; }
    public float isAimming_time() { return gun_aim_time; }
    public bool isAimmingProper() { return state_aimProper; }
    public float isAimmingProper_time() { return gun_aimProper_time; }
    public bool isInFirstPerson() { return state_firstPerson; }
    public bool isShooting() { return state_shoot_moment; }
    public bool isStandingToggle() { return state_stand_toggle; }
    public bool isStandie() { return state_stand; }


    public bool getAction_slide() { return action_slide[1]; }
    public bool getAction_reload() { return action_reload[1]; }


    public Vector2 getWeapon_angle() { return gun_angle; }
    public Vector3 getWeapon_position() { return gun_position; }
    public float getWeapon_lastShootTime() { return gun_lastShootTime; }

    public Vector2 getPosture_recoil() { return posture_gun_recoil; }

    public Transform getBone(string bone)
    {
        if (bone == "hip") { return bone_hip; }
        if (bone == "spine") { return bone_spine; }
        if (bone == "head") { return bone_head; }

        if (bone == "right shoulder") { return bone_right_shoulder; }
        if (bone == "right upper arm") { return bone_right_upperArm; }
        if (bone == "right lower arm") { return bone_right_lowerArm; }
        if (bone == "right hand") { return bone_right_hand; }

        if (bone == "right upper leg") { return bone_right_upperLeg; }
        if (bone == "right lower leg") { return bone_right_lowerLeg; }

        if (bone == "left shoulder") { return bone_left_shoulder; }
        if (bone == "left upper arm") { return bone_left_upperArm; }
        if (bone == "left lower arm") { return bone_left_lowerArm; }
        if (bone == "left hand") { return bone_left_hand; }

        if (bone == "left upper leg") { return bone_left_upperLeg; }
        if (bone == "left lower leg") { return bone_left_lowerLeg; }

        return null;
    }

    public scr_Gun getGun() { return gun; }
    public scr_Gun[] getGuns()
    {
        scr_Gun[] toReturn = new scr_Gun[guns.Count];
        for (int i = 0; i < guns.Count; i++)
        {
            toReturn[i] = guns[i].GetComponent<scr_Gun>();
        }
        return toReturn;
    }
    #endregion
}
