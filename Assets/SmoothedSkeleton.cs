using System.Collections.Generic;
using UnityEngine;

public class SmoothedSkeleton : MonoBehaviour
{
    [Header("Skeletons")]
    public Transform sourceRoot;
    public Transform targetRoot;

    [Header("Smoothing")]
    [Tooltip("Higher = faster response")]
    public float rotationLerpSpeed = 10f;

    private Dictionary<string, Transform> sourceBones = new Dictionary<string, Transform>();
    private Dictionary<string, Transform> targetBones = new Dictionary<string, Transform>();

    void Awake()
    {
        CacheBones(sourceRoot, sourceBones);
        CacheBones(targetRoot, targetBones);
    }

    void LateUpdate()
    {
        foreach (var pair in sourceBones)
        {
            if (!targetBones.TryGetValue(pair.Key, out Transform target))
                continue;

            Transform source = pair.Value;

            target.localRotation = Quaternion.Slerp(
                target.localRotation,
                source.localRotation,
                Time.deltaTime * rotationLerpSpeed
            );
        }
    }

    void CacheBones(Transform root, Dictionary<string, Transform> dict)
    {
        foreach (Transform t in root.GetComponentsInChildren<Transform>())
        {
            if (!dict.ContainsKey(t.name))
                dict.Add(t.name, t);
        }
    }
}
