using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.PlasticSCM.Editor.WebApi;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Submarine : MonoBehaviour
{
    public float WheelRotation = 0;
    public float RotationPower = 100f;

    public float Thrust = 0;
    public float ThrustPower = 2f;
    public float ThrustBoostPower = 4f;

    public float SpeedDamage = 1;
    public float SpeedDamageStart = 2.3f;
    public float SpeedShake = 1f;

    public GameObject Camera;
    public float CameraSmooth = 10f;

    public Vector3 cameraOffset = new Vector3(0, 0.8f, 0);

    public float TargetWaterLevel = 4f;
    public float WaterLevel = 3.74f;
    public float TargetDepth = 0;
    public float MaxHeight = 3.8f;
    public float MaxDepth = 3.5f;
    public AnimationCurve DepthSpring;
    public float DepthDamp = 6f;
    public float DepthDepthSlower = 1f;

    public Wheel wheel;
    public Slider Thruster;
    public Slider DepthSlider;
    public RectTransform OxygenIndicator;
    public float OxygenIndicatorAngle = 43f;
    public GameObject Shaker;
    public Image SuffocateImage;
    public WaterUI WaterUI;
    public Text PickupText;
    public Button BoostButton;
    public Image BoostLight;

    public float PickupDistance = 1;

    public List<TugPoint> Tugs;

    [Serializable]
    public class TugPoint
    {
        public LineRenderer Line;
        public Joint Joint;
    }

    public float TugMaxDistance = 0.5f;
    public float TugBreakForce = 4f;
    public float TugSpring = 10;

    public float ShakeSpeed = 10;
    public float ShakeStrength = 10;
    public float ShakeT = 1;

    public GameObject Propellor;
    public float PropellorSpeed = 10;
    public float PropellorSmooth = 1;

    public float DepthDragMult = 2;
    public float MaxDepthDragMultDepth = 7;

    public float BaseDrag = 5;

    public float Oxygen = 1;
    public float OxygenRegenSpeed = 1f;
    public float OxygenUseSpeed = 0.05f;

    public float DepthLeakDepth = 3.8f;
    public float MaxDepthLeakDepth = 5;
    public float DepthLeakDamage = 20f;


    public float SuffocateSpeed = 0.5f;
    public float SuffocateRecover = 1f;
    public float SuffocateAmount;

    public float DrownLevel = 0.8f;

    private float propSize;

    public float Health = 100f;
    public AnimationCurve HealthWaterLeak;
    public AnimationCurve HealthOxygenLeak;

    public float WaterLossRate = 0.2f;
    public float DepthLeakMult = 1f;

    private float LeakedWater;

    public List<BoltHole> BoltHoles = new();

    public static Submarine Instance;

    private bool boost;

    public Rigidbody rigidBody;
    void Start()
    {
        MissionManager.Instance?.SpawnActiveMissionItems();
        Instance = this;
        lastBoltHealth = Health;
        propSize = Propellor.transform.localScale.x;
        rigidBody = GetComponent<Rigidbody>();
        BoostButton.onClick.AddListener(() => { boost = !boost; });
    }

    private void Update()
    {
        if (Instance == null)
            Instance = this;
        var targetPos = transform.position + cameraOffset;
        Camera.transform.position = Vector3.Lerp(Camera.transform.position, targetPos, Time.deltaTime * CameraSmooth);
        ManagePickup();
        TugVisuals();

        if ( Mathf.Abs( transform.eulerAngles.x ) > 0.1f || Mathf.Abs(transform.eulerAngles.z) > 0.1f)
        {
            rigidBody.isKinematic = true;
            rigidBody.isKinematic = false;
            transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);
        }
    }

    void FixedUpdate()
    {
        ShakeT = 0;
        Input();

        Rotation();

        DoThrust();

        Depth();

        PropellorAnimation();

        Drag();

        Shake();

        Leak();

        Suffocate();

        Bolts();

        if (transform.position.y > MaxHeight)
        {
            var vel = rigidBody.velocity;
            vel.y = Mathf.Clamp(vel.y, -1000, 0);
            rigidBody.velocity = vel;
        }

        ManageOxygen();

        DoSpeedDamage();
    }

    void DoSpeedDamage()
    {
        var vel = rigidBody.velocity;
        vel.y = 0;
        var relativeSpeed = vel.magnitude * depth;
        boostShake = Mathf.Lerp(boostShake, relativeSpeed < SpeedDamageStart ? 0 : SpeedShake, Time.deltaTime * 10);

        if (relativeSpeed < SpeedDamageStart)
            return;

        Health -= SpeedDamage * Time.deltaTime;
    }

    void TugVisuals()
    {
        foreach (var tug in Tugs)
        {
            tug.Line.enabled = tug.Joint != null;
            if (!tug.Line.enabled)
                continue;

            tug.Line.SetPositions(new Vector3[]{Vector3.zero, tug.Line.transform.InverseTransformPoint(tug.Joint.connectedBody.transform.position)});
        }
    }

    void ManagePickup()
    {
        if (Pickup.AllPickups == null || Pickup.AllPickups.Count == 0)
            return;

        if (!Tugs.Any(x => x.Joint == null))
        {
            PickupText.text = "";
            return;
        }


        Pickup closest = null;
        float closestDist = PickupDistance * 2;

        Vector3 pos = transform.position;

        foreach (var pickup in Pickup.AllPickups)
        {
            if (pickup == null)
                continue;

            if (pickup.joint != null)
                continue;

            float dist = Vector3.Distance(transform.position, pickup.transform.position);

            if (dist < closestDist)
            {
                closestDist = dist;
                closest = pickup;
            }
        }

        if (closest == null)
        {
            PickupText.text = "";
            return;
        }

        PickupText.text = $"Press E to Pickup {closest.Name}";

        if (UnityEngine.Input.GetKeyDown(KeyCode.E))
        {
            PickupItem(closest);  
        }
    }

    public float lastBoltHealth;
    void Bolts()
    {
        if (lastBoltHealth - Health < 10)
            return;

        for (int i = 0; i + 1 < (lastBoltHealth - Health) / 10; i++)
        {
            var validBoltHoles = BoltHoles.Where(x => x.Bolt != null).ToList();

            if (validBoltHoles.Count > 0)
                validBoltHoles[UnityEngine.Random.Range(0, validBoltHoles.Count())].FreeBolt();
        }

        lastBoltHealth = Health;
    }

    void Suffocate()
    {
        var suffocating = Oxygen <= 0.01f || LeakedWater > DrownLevel;

        SuffocateAmount += (suffocating ? SuffocateSpeed : -SuffocateRecover) * Time.deltaTime;
        SuffocateAmount = Mathf.Clamp01(SuffocateAmount);

        SuffocateImage.color = Color.black.WithAlpha(SuffocateAmount);
    }

    void Leak()
    {
        WaterUI.Level = LeakedWater;
        if (transform.position.y >= WaterLevel)
        {
            LeakedWater -= WaterLossRate * Time.deltaTime;
            LeakedWater = Mathf.Clamp01(LeakedWater);
            return;
        }

        float currentDepth = depth;
        
        float depthDamage = Mathf.InverseLerp(DepthLeakDepth, MaxDepthLeakDepth, currentDepth);

        float leakAmount = depthDamage * DepthLeakDamage;
        ShakeT += depthDamage;

        Health -= leakAmount * Time.deltaTime;

        Oxygen -= HealthOxygenLeak.Evaluate(1 - (Health / 100)) * Time.deltaTime;
        Oxygen = Mathf.Clamp01(Oxygen);

        LeakedWater += HealthWaterLeak.Evaluate(1-(Health / 100)) * Time.deltaTime * currentDepth * DepthLeakMult;
        LeakedWater = Mathf.Clamp01(LeakedWater);
    }
    float boostShake;
    void Shake()
    {
        var time = Time.time * ShakeSpeed;
        var shakePos = (new Vector3(Mathf.PerlinNoise1D(time + 100), Mathf.PerlinNoise1D(time + 100), 0.5f) - Vector3.one/2) * ShakeStrength;
        Shaker.transform.localPosition = Vector3.Lerp(Vector3.zero, shakePos, ShakeT);
    }

    void ManageOxygen()
    {
        var underWater = transform.position.y < WaterLevel;

        Oxygen += (underWater ? -OxygenUseSpeed : OxygenRegenSpeed) * Time.deltaTime;
        Oxygen = Mathf.Clamp01(Oxygen);

        var rot = Mathf.Lerp(-OxygenIndicatorAngle, OxygenIndicatorAngle, Oxygen);
        OxygenIndicator.localEulerAngles = new Vector3(0, 0, rot);
    }

    void Drag()
    {
        var drag = BaseDrag + Mathf.Clamp( depth, 0, MaxDepthDragMultDepth) * DepthDragMult;
        rigidBody.drag = drag;
        rigidBody.angularDrag = drag;
    }

    private float depth => Mathf.Clamp(-(transform.position.y - TargetWaterLevel), 0, 1000);

    float propT;
    float propThrust;
    void PropellorAnimation()
    {
        propThrust = Mathf.Lerp(propThrust, Thrust, Time.deltaTime * PropellorSmooth);
        propT += Time.deltaTime * propThrust * PropellorSpeed;
        var size = (Mathf.Cos(propT) + 1) / 2;
        var scale = Propellor.transform.localScale;
        scale.x = size * propSize;
        Propellor.transform.localScale = scale;
    }

    void Input()
    {
        BoostLight.enabled = boost;
        WheelRotation = -wheel.rot;
        Thrust = Thruster.value;
        TargetDepth = Mathf.Lerp(MaxDepth, 0, DepthSlider.value);
    }

    void Depth()
    {
        float targetY = TargetWaterLevel - TargetDepth;
        float displacement = targetY - transform.position.y;

        float force =
            (displacement * DepthSpring.Evaluate(depth)) -
            (rigidBody.velocity.y * DepthDamp);

        if (transform.position.y > MaxHeight)
        {
            force = Mathf.Min(force, 0f);
        }

        force /= depth * DepthDepthSlower;

        rigidBody.AddForce(Vector3.up * force, ForceMode.Acceleration);
    }

    void DoThrust()
    {
        var power = boost ? ThrustBoostPower : ThrustPower;
        rigidBody.AddForce(transform.forward * power * Thrust);

        ShakeT += boostShake;
    }

    float lastRotation;
    void Rotation()
    {
        var delta = (WheelRotation - lastRotation);
        lastRotation = WheelRotation;

        if (Mathf.Abs(delta) <= 0)
        {
            return;
        }

        rigidBody.AddTorque(-Vector3.forward * 100 * delta * RotationPower);
    }

    public void PickupItem(Pickup target)
    {
        if (target == null)
            return;

        TugPoint closestTug = null;
        float closestDist = float.MaxValue;

        foreach (var tug in Tugs)
        {
            if (tug.Joint != null)
                continue;

            float dist = Vector3.Distance(tug.Line.transform.position, target.transform.position);

            if (dist < closestDist)
            {
                closestDist = dist;
                closestTug = tug;
            }
        }

        if (closestTug == null)
            return;
        
        var joint = closestTug.Line.AddComponent<SpringJoint>();
        closestTug.Joint = joint;
        target.joint = joint;
        joint.autoConfigureConnectedAnchor = false;
        joint.connectedAnchor = Vector3.zero;
        joint.spring = TugSpring;
        joint.breakForce = TugBreakForce;
        joint.maxDistance = TugMaxDistance;
        joint.connectedBody = target.GetComponent<Rigidbody>();
        joint.enableCollision = true;
    }
}
