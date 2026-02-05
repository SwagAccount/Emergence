using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Spawner : MonoBehaviour
{
    public GameObject Enemy;
    public GameObject Target => Submarine.Instance.gameObject;
    public int MaxCount = 3;
    public AnimationCurve SpawnFrequency;
    public float MaxRange = 40;
    public List<GameObject> Enemies;
    public float DespawnDis = 6;
    public Vector2 SpawnHeightRange;

    public Vector2 SpawnRing = new Vector2(3, 5);

    private float nextSpawn = 1;
    float t;
    private void Update()
    {
        var pos = Target.transform.position;
        pos.y = 0;
        var spawnFreq = SpawnFrequency.Evaluate( 1 - Mathf.Clamp01( pos.magnitude / MaxRange) );
        t += Time.deltaTime * spawnFreq;

        if (t > nextSpawn)
        {
            Spawn();
            nextSpawn++;
        }

        foreach(var enemy in new List<GameObject>(Enemies))
        {
            if (enemy == null)
            {
                Enemies.Remove(enemy);
                continue;
            }

            var dist = Vector3.Distance(new Vector3(Target.transform.position.x, 0, Target.transform.position.z), new Vector3(enemy.transform.position.x, 0, enemy.transform.position.z));
            if (dist < DespawnDis)
                continue;

            Destroy(enemy);
        }
    }
    private void Spawn()
    {
        if (Enemies.Count >= MaxCount)
            return;

        var targetPos = Target.transform.position;

        var dir = new Vector3(Random.Range(-1, 1), 0, Random.Range(-1, 1));
        var pos = targetPos + dir.normalized * Random.Range(SpawnRing.x, SpawnRing.y);
        pos.y = Random.Range(SpawnHeightRange.x, SpawnHeightRange.y);

        var newPos = pos;
        NavMeshHit myNavHit;
        if (NavMesh.SamplePosition(pos, out myNavHit, 5, -1))
        {
            newPos = myNavHit.position;
        }

        pos = pos + (pos - newPos) * 2;

        if (Vector3.Distance(new Vector3(pos.x, 0, pos.z), new Vector3(targetPos.x, 0, targetPos.z)) < SpawnRing.x)
            return;

        var enemy = Instantiate(Enemy, pos, Quaternion.Euler(0, Random.Range(0, 360), 0));

        Enemies.Add(enemy);
    }
}
