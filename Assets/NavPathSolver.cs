using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class NavPathSolver : MonoBehaviour
{
    public float agentRadius = 0.5f;
    public float repathDistance = 1.0f;
    public float cornerReachDistance = 0.5f;
    public int areaMask = NavMesh.AllAreas;

    private NavMeshPath path;
    private int currentCornerIndex;
    private Vector3 lastTargetPos;

    float[] Levels =
    {
        -1.82f,
        0.82f,
        1.82f,
        2.82f,
        3.82f,
        4.82f,
        5.82f,
        6.82f,
        7.82f
    };

    public bool HasPath => path != null && path.corners.Length > 0;

    void Awake()
    {
        path = new NavMeshPath();
    }
    private float currentLevel;
    private float Level()
    {
        float level = Levels[0];
        for (int i = 0; i < Levels.Count(); i++)
        {
            if (transform.position.y < Levels[i])
                break;
            level = Levels[i];
        }
        return level;
    }

    float lastLevel;
    private void FixedUpdate()
    {
        currentLevel = Level();
        if (currentLevel != lastLevel && targetGenerated)
            SetTarget(lastTargetPos);
        lastLevel = currentLevel;
    }

    private bool targetGenerated;
    public void SetTarget(Vector3 targetPosition)
    {
        targetGenerated = true;
        if ((targetPosition - lastTargetPos).sqrMagnitude < repathDistance * repathDistance)
            return;

        lastTargetPos = targetPosition;
        RecalculatePath(targetPosition);
    }

    void RecalculatePath(Vector3 target)
    {
        currentCornerIndex = 0;

        var start = transform.position;
        start.y = currentLevel;
        target.y = currentLevel;

        NavMesh.CalculatePath(
            start,
            target,
            areaMask,
            path
        );
    }

    public bool TryGetNextPoint(out Vector3 nextPoint)
    {
        nextPoint = Vector3.zero;

        if (!HasPath || currentCornerIndex >= path.corners.Length)
            return false;

        Vector3 corner = path.corners[currentCornerIndex];
        bool isLastCorner = currentCornerIndex == path.corners.Length - 1;

        float reachDist = isLastCorner
            ? cornerReachDistance * 2f
            : cornerReachDistance;
        var disDir = transform.position - corner;
        disDir.y = 0;
        float sqrDist = disDir.sqrMagnitude;

        if (sqrDist < reachDist * reachDist)
        {
            if (!isLastCorner)
                currentCornerIndex++;

            if (currentCornerIndex >= path.corners.Length)
                return false;

            corner = path.corners[currentCornerIndex];
            isLastCorner = currentCornerIndex == path.corners.Length - 1;
        } 

        if (!isLastCorner)
        {
            Vector3 dir = (corner - transform.position);
            dir.y = 0;
            dir = dir.normalized;
            nextPoint = corner - dir * agentRadius;
        }
        else
        {
            nextPoint = corner;
        }

        return true;
    }
}
