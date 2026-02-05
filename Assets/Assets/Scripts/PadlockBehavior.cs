using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PadlockBehavior : MonoBehaviour
{
    
    public UnityEvent OnUnlock, OnLockFailed;
    public int code = 1234;
    public List<SimpleTransformAnimator> lockDiscs = new List<SimpleTransformAnimator>();
    public List<int> lockDiscValues = new List<int>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ResetLock();
        for (int i = 0; i < lockDiscs.Count; i++)
        {
            lockDiscValues.Add(0);
        }
    }

    public void ResetLock()
    {
        foreach (SimpleTransformAnimator sta in lockDiscs)
        {
            sta.rotX = 0;
            sta.rotY = 0;
            sta.rotZ = 0;   
            
            sta.setRot = true;
            
            sta.PlayAnimation();
        }

        // set all disc values to 0
        for (int i = 0; i < lockDiscValues.Count; i++)
        {
            lockDiscValues[i] = 0;
        }

    }

    public void MovePieceLeft(int discIndex)
    {
        // Limit to: 0, 1, 2, 3...
        if (discIndex < 0 || discIndex >= lockDiscs.Count) return;
        SimpleTransformAnimator sta = lockDiscs[discIndex];

        lockDiscValues[discIndex] -= 1;
        if (lockDiscValues[discIndex] < 0) { lockDiscValues[discIndex] = 9; }

        sta.rotY = lockDiscValues[discIndex] * 36;
        sta.PlayAnimation();
    }

    public void MovePieceRight(int discIndex)
    {
        // Limit to: 0, 1, 2, 3...
        if (discIndex < 0 || discIndex >= lockDiscs.Count) return;
        SimpleTransformAnimator sta = lockDiscs[discIndex];

        lockDiscValues[discIndex] += 1;
        if (lockDiscValues[discIndex] > 9) { lockDiscValues[discIndex] = 0; }

        sta.rotY = lockDiscValues[discIndex] * 36;
        sta.PlayAnimation();
    }

    public void Unlock()
    {
        int attemptedCode = 0;

        // combine the lockDiscValues into a single integer
        for (int i = 0; i < lockDiscValues.Count; i++)
        {
            attemptedCode *= 10;
            attemptedCode += lockDiscValues[i];
        }

        if (attemptedCode == code)
        {
            OnUnlock.Invoke();
        }
        else
        {
            OnLockFailed.Invoke();
        }
    }
}

