using UnityEngine;

[CreateAssetMenu(fileName = "BoolData", menuName = "ScriptableObjects/BoolData", order = 1)]
public class BoolData : ScriptableObject
{
    public bool value;

    public void SetBoolActive()
    {
        value = true;
        
    }
    
    public void SetBoolInactive()
    {
        value = false;
        
    }
    
    public void ButtonTooggle()
    {
        value = !value;
        
    }
    
    
}