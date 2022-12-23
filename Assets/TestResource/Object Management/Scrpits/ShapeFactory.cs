using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class ShapeFactory : ScriptableObject
{
    [SerializeField] Shape[] prefabs;
    [SerializeField] Material[] materials;

    public Shape Get(int shapeID=0,int materialID=0)
    {
       Shape  instance = Instantiate(prefabs[shapeID]);
        instance.ShapeID = shapeID;

        instance.SetMaterial(materials[materialID], materialID);
        return instance;
    }

    public Shape GetRandom()
    {
        return Get(
                    Random.Range(0, prefabs.Length),
                    Random.Range(0, materials.Length));
    }
}
