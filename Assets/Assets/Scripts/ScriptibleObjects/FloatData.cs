using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "FloatData", menuName = "ScriptableObjects/FloatData", order = 1)]
public class FloatData : ScriptableObject
{
    
    public float value;
    
    public void setFloatValue(float newValue)
    {
        value = newValue;
        
    }

    public void Sliderupdate(Slider slider)
    {
        value = slider.value;
    }
    
}
