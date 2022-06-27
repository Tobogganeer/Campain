using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SurfaceFX : MonoBehaviour
{
    public static SurfaceFX instance;
    private void Awake()
    {
        if (instance == null) instance = this;

        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }

        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);

        hitClips.Clear();
        foreach (SurfaceEffectAudioClips aclips in i_hitClips)
        {
            hitClips.Add(aclips.array, aclips.clips);
        }

        leftFootstepClips.Clear();
        rightFootstepClips.Clear();
        foreach (SurfaceFootstepAudioClips aclips in i_footstepClips)
        {
            leftFootstepClips.Add(aclips.array, aclips.leftFootClips);
            rightFootstepClips.Add(aclips.array, aclips.rightFootClips);
        }

        surfaceEffects.Clear();
        foreach (SurfaceEffect effect in i_surfaceEffects)
        {
            surfaceEffects.Add(effect.type, effect);
        }
    }

    //public GameObject audioSourcePrefab;

    private static Dictionary<SurfaceType, string> hitClips = new Dictionary<SurfaceType, string>();
    public SurfaceEffectAudioClips[] i_hitClips;

    private static Dictionary<SurfaceFootstepType, string> leftFootstepClips = new Dictionary<SurfaceFootstepType, string>();
    private static Dictionary<SurfaceFootstepType, string> rightFootstepClips = new Dictionary<SurfaceFootstepType, string>();
    public SurfaceFootstepAudioClips[] i_footstepClips;

    public SurfaceEffect[] i_surfaceEffects;
    private static Dictionary<SurfaceType, SurfaceEffect> surfaceEffects = new Dictionary<SurfaceType, SurfaceEffect>();

    public bool suppressWarnings;

    public static void PlayHitSound(SurfaceType surfaceType, Audio audio)
    {
        if (!hitClips.ContainsKey(surfaceType))
        {
            //Play(nullClip, position, parent, maxDistance, volume, minPitch, maxPitch);
            if (!instance.suppressWarnings)
                Debug.LogWarning("Could not find hit sound for " + surfaceType);
            return;
        }

        AudioManager.Play(audio.SetClip(hitClips[surfaceType]));
    }

    public static void PlayFootstepSound(SurfaceFootstepType footstepType, Vector3 position, Foot foot)
    {
        if (foot == Foot.Left)
        {
            if (!leftFootstepClips.ContainsKey(footstepType))
            {
                //Play(nullClip, position, parent, maxDistance, volume, minPitch, maxPitch);
                if (!instance.suppressWarnings)
                    Debug.LogWarning("Could not find footstep sound for " + footstepType);
                return;
            }

            AudioManager.Play(new Audio(leftFootstepClips[footstepType]).SetPosition(position));
        }
        else if (foot == Foot.Right)
        {
            if (!rightFootstepClips.ContainsKey(footstepType))
            {
                //Play(nullClip, position, parent, maxDistance, volume, minPitch, maxPitch);
                if (!instance.suppressWarnings)
                    Debug.LogWarning("Could not find footstep sound for " + footstepType);
                return;
            }

            AudioManager.Play(new Audio(rightFootstepClips[footstepType]).SetPosition(position));
        }

        
    }

    public static void PlayFootstepSound(SurfaceType surfaceType, Vector3 position, Foot foot)
    {
        PlayFootstepSound(surfaceType.GetFootstepType(), position, foot);
    }

    public static void SpawnFX(SurfaceType surfaceType, Vector3 position, Quaternion rotation, Transform parent = null)
    {
        if (!surfaceEffects.TryGetValue(surfaceType, out SurfaceEffect effect))
        {
            if (!instance.suppressWarnings)
                Debug.LogWarning("Could not spawn surface effect for " + surfaceType);
        }

        Instantiate(effect.gameObject, position, rotation, parent);
    }

    public static void SpawnFX(SurfaceType surfaceType, Vector3 position, Transform parent = null)
    {
        SpawnFX(surfaceType, position, Quaternion.identity, parent);
    }
}

// Add support for each foot having different sounds

[System.Serializable]
public struct SurfaceEffectAudioClips
{
    public string name;
    public SurfaceType array;
    public string clips;
}

[System.Serializable]
public struct SurfaceFootstepAudioClips
{
    public string name;
    public SurfaceFootstepType array;
    public string leftFootClips;
    public string rightFootClips;
}
