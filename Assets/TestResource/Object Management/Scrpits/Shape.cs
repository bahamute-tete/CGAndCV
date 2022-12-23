using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shape : PersistableObject
{
    
    public int ShapeID
    {
        get
        {
            return shapeID;
        }
        set
        {
            if (shapeID == int.MinValue && value != int.MinValue)
                shapeID = value;
            else
                Debug.LogError("Not Allow to Change shapeID");
            
        }
    }
    int shapeID = int.MinValue;
    MeshRenderer meshRenderer;

    Color color;
    static int colorPropertyID = Shader.PropertyToID("_Color");
    static MaterialPropertyBlock sharedPropertyBlock;

    public int MaterialID { get; private set; }

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
    }

    public void SetMaterial(Material material, int materialID)
    {
        meshRenderer.material = material;
        MaterialID = materialID; 
    }

    public void SetColor(Color color)
    {
        this.color = color;
       
        if (sharedPropertyBlock == null)
            sharedPropertyBlock = new MaterialPropertyBlock();

        sharedPropertyBlock.SetColor(colorPropertyID, color);
        meshRenderer.SetPropertyBlock(sharedPropertyBlock);
    }

    public override void Save(GameDataWriter writer)
    {
        base.Save(writer);
        writer.Write(color);
    }

    public override void Load(GameDataReader reader)
    {
        int version = reader.Version;
        base.Load(reader);
        SetColor(reader.Version > 0 ?  reader.ReadColor():Color.white);
    }

}
