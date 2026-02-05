using UnityEngine;

public class SimpleTransformAnimator : MonoBehaviour
{
    [Header("Position Offset (Relative)")]
    public float moveX = 0f;
    public float moveY = 0f;
    public float moveZ = 0f;
    public bool animatePosition = true;
    public bool setPos = false;

    [Header("Rotation Offset (Relative)")]
    public float rotX = 0f;
    public float rotY = 0f;
    public float rotZ = 0f;
    public bool animateRotation = true;
    public bool setRot = false;

    [Header("Settings")]
    [Min(0f)] public float duration = 1f;
    public bool useLocalSpace = true;

    [Header("Animation Curve")]
    public AnimationCurve curve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    private Coroutine routine;

    /// <summary>
    /// Call this from a UnityEvent.
    /// </summary>
    public void PlayAnimation()
    {
        if (routine != null)
            StopCoroutine(routine);

        routine = StartCoroutine(Animate());
    }

    private System.Collections.IEnumerator Animate()
    {
        Debug.Log("Play Anim");
        Vector3 startPos = useLocalSpace ? transform.localPosition : transform.position;
        Quaternion startRot = useLocalSpace ? transform.localRotation : transform.rotation;

        Vector3 targetPos = setPos ? 
        new Vector3(moveX, moveY, moveZ) : 
        startPos + new Vector3(moveX, moveY, moveZ);

        Quaternion targetRot = setRot ?
        Quaternion.Euler(rotX, rotY, rotZ) :
        startRot * Quaternion.Euler(rotX, rotY, rotZ);

        if (duration <= 0f)
        {
            if (animatePosition)
            {
                if (useLocalSpace) transform.localPosition = targetPos;
                else transform.position = targetPos;
            }

            if (animateRotation)
            {
                if (useLocalSpace) transform.localRotation = targetRot;
                else transform.rotation = targetRot;
            }

            routine = null;
            yield break;
        }

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / duration);
            float c = curve.Evaluate(p);

            if (animatePosition)
            {
                Vector3 pos = Vector3.LerpUnclamped(startPos, targetPos, c);
                if (useLocalSpace) transform.localPosition = pos;
                else transform.position = pos;
            }

            if (animateRotation)
            {
                Quaternion rot = Quaternion.LerpUnclamped(startRot, targetRot, c);
                if (useLocalSpace) transform.localRotation = rot;
                else transform.rotation = rot;
            }

            yield return null;
        }

        // Snap to perfect final values
        if (animatePosition)
        {
            if (useLocalSpace) transform.localPosition = targetPos;
            else transform.position = targetPos;
        }

        if (animateRotation)
        {
            if (useLocalSpace) transform.localRotation = targetRot;
            else transform.rotation = targetRot;
        }

        routine = null;
    }
}
