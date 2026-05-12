using Unity.VisualScripting;
using UnityEngine;


public class Test : MonoBehaviour
{
    public bool result;
    public int resultint;
    public void Start()
    {

        if (Numbers(4, Random.Range(0, 10), out resultint))
        {
            print(resultint);
        }
        
    }
    public bool Numbers(int num1, int num2, out int result)
    {
        result = num1 + num2;
        if (num1+num2>10)
        {
            return true;
        }
        else
        {
            return false;
        }
        
    }
}

