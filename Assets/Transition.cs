using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class Transition : MonoBehaviour
{
    public float Speed = 10;
    public float t;
    public RectTransform Top;
    public RectTransform Bottom;
    public bool transitioning;
    public string transitionScene;
    public static Transition Instance;
    public float StartDelay = 1f;
    float startDelayT;
    public float Delay = 0.4f;
    float delayT;
    public AudioMixer AudioMixer;

    public AudioSource transitionSound;

    public bool transitionManual;
    public void TransitionButton()
    {
        ChangeScene(transitionScene);
    }

    void Update()
    {
        AudioMixer.SetFloat("GameVolume", -50 + t * 50);
        startDelayT += Time.deltaTime;
        if (startDelayT < StartDelay)
            return;

        if (transitionManual)
        {
            TransitionButton();
            transitionManual = false;
        }
        Instance = this;
        t += (transitioning ? -1 : 1) * Speed * Time.deltaTime;
        t = Mathf.Clamp01(t);

        if (transitioning && t <= 0)
        {
            delayT += Time.deltaTime;
            if (delayT > Delay)
                SceneManager.LoadScene(transitionScene);
        }
        else
            delayT = 0;

        float result = t;
        if (transitioning)
            result = easeInBounce(t);

        Top.localPosition = new Vector3(0, result * 1080, 0);
        Bottom.localPosition = new Vector3(0, -result * 1080, 0);
    }

    float easeInBounce(float x){
        return 1 - easeOutBounce(1 - x);
    }
    const float n1 = 7.5625f;
    const float d1 = 2.75f;
    float easeOutBounce(float x){

        if (x< 1 / d1) {
            return n1 * x * x;
        } else if (x < 2 / d1)
        {
            return n1 * (x -= 1.5f / d1) * x + 0.75f;
        }
        else if (x < 2.5 / d1)
        {
            return n1 * (x -= 2.25f / d1) * x + 0.9375f;
        }
        else
        {
            return n1 * (x -= 2.625f / d1) * x + 0.984375f;
        }
    }

    public static void ChangeScene(string scene)
    {
        if (Instance.transitioning)
            return;

        Instance.transitioning = true;
        Instance.transitionScene = scene;
        Instance.transitionSound.Play();
    }
}
