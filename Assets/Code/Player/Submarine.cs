using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Submarine : MonoBehaviour
{
    public float WheelRotation = 0;
    public float RotationPower = 100f;

    public float Thrust = 0;
    public float ThrustPower = 10f;

    public GameObject Camera;
    public float CameraSmooth = 10f;

    public float CameraHeight = 1.4f;

    public float TargetWaterLevel = 4f;
    public float WaterLevel = 3.74f;
    public float TargetDepth = 0;
    public float MaxHeight = 3.8f;
    public float MaxDepth = 3.5f;
    public float DepthSpring = 5f;
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

    public float ShakeSpeed = 10;
    public float ShakeStrength = 10;
    public float ShakeT = 1;

    public GameObject Propellor;
    public float PropellorSpeed = 10;
    public float PropellorSmooth = 1;

    public float DepthDragMult = 2;

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

    Rigidbody rigidBody;
    void Start()
    {
        propSize = Propellor.transform.localScale.x;
        rigidBody = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        var targetPos = transform.position + Vector3.up * CameraHeight;
        Camera.transform.position = Vector3.Lerp(Camera.transform.position, targetPos, Time.deltaTime * CameraSmooth);
    }

    void FixedUpdate()
    {
        Input();

        Rotation();

        DoThrust();

        Depth();

        PropellorAnimation();

        Drag();

        Shake();

        Leak();

        Suffocate();

        if (transform.position.y > MaxHeight)
        {
            var vel = rigidBody.velocity;
            vel.y = Mathf.Clamp(vel.y, -1000, 0);
            rigidBody.velocity = vel;
        }

        ManageOxygen();
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
        ShakeT = depthDamage;

        Health -= leakAmount * Time.deltaTime;

        Oxygen -= HealthOxygenLeak.Evaluate(1 - (Health / 100)) * Time.deltaTime;
        Oxygen = Mathf.Clamp01(Oxygen);

        LeakedWater += HealthWaterLeak.Evaluate(1-(Health / 100)) * Time.deltaTime * currentDepth * DepthLeakMult;
        LeakedWater = Mathf.Clamp01(LeakedWater);
    }

    void Shake()
    {
        var time = Time.time * ShakeSpeed * ShakeT;
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
        var drag = BaseDrag + depth * DepthDragMult;
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
        WheelRotation = -wheel.rot;
        Thrust = Thruster.value;
        TargetDepth = Mathf.Lerp(MaxDepth, 0, DepthSlider.value);
    }

    void Depth()
    {
        float targetY = TargetWaterLevel - TargetDepth;
        float displacement = targetY - transform.position.y;

        float force =
            (displacement * DepthSpring) -
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
        rigidBody.AddForce(transform.forward * ThrustPower * Thrust);
    }

    float lastRotation;
    void Rotation()
    {
        var delta = (WheelRotation - lastRotation);
        lastRotation = WheelRotation;

        if (Mathf.Abs(delta) <= 0)
            return;

        rigidBody.AddTorque(-Vector3.forward * 100 * delta * RotationPower);
    }
}
