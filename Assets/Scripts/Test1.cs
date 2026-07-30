using UnityEngine;
using Zenject;

public class Test1 : IInitializable
{
    public void Initialize()
    {
        Debug.Log("Hello world");
    }
}
