using UnityEngine;

[CreateAssetMenu(fileName = "surface", menuName = "Surface", order = 1)]
public class Surface : ScriptableObject
{
    public GameObject ImpactEffect;
    public SoundEvent ImpactSound;

    public static void DoImpact(GameObject gameObject, Vector3 position, float volume = 1, bool forcePlay = true)
    {
        if (volume <= 0)
            return;

        SurfaceDefinition surfaceDefinition = null;
        if (!gameObject.TryGetComponent(out surfaceDefinition))
        {
            gameObject.transform.root.TryGetComponent(out surfaceDefinition);
        }

        Surface surface;

        if (surfaceDefinition?.Surface != null)
            surface = surfaceDefinition.Surface;
        else
            surface = Resources.Load<Surface>("DefaultSurface");

        if (surface.ImpactEffect != null)
            Instantiate(surface.ImpactEffect, position, Quaternion.identity);

        if (surface.ImpactSound != null)
            surface.ImpactSound.Play(position, volumeMult: volume, forcePlay: forcePlay);
    }
}