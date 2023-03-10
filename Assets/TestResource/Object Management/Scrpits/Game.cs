

using System.Collections.Generic;

using UnityEngine;

public class Game : PersistableObject
{
    //public PersistableObject prefab;
    public ShapeFactory shapeFactory;
    public PersistentStorage storage;


    public KeyCode createKey = KeyCode.C;
    public KeyCode newGameKey = KeyCode.N;
    public KeyCode saveKey = KeyCode.S;
    public KeyCode loadKey = KeyCode.L;

    List<Shape> shapes;
    string savePath;
    const int saveVersion = 1;

    private void Awake()
    {
        shapes = new List<Shape>();
        
    }

    public override void Save(GameDataWriter writer)
    {
       
        writer.Write(shapes.Count);
        for (int i = 0; i < shapes.Count; i++)
        {
            writer.Write(shapes[i].ShapeID);
            writer.Write(shapes[i].MaterialID);
            shapes[i].Save(writer);
        }
    }

    public override void Load(GameDataReader reader)
    {
        int version = -reader.ReadInt();
        if (version > saveVersion)
        {
            Debug.LogError("Unsupported future save version" + version);
            return;
        }
        int count = version<=0? -version:reader.ReadInt();
        for (int i = 0; i < count; i++)
        {
            int shapeID = version > 0 ? reader.ReadInt() : 0;
            int materialID = version > 0 ? reader.ReadInt() : 0;
            Shape instance = shapeFactory.Get(shapeID,materialID);
            instance.Load(reader);
            shapes.Add(instance);
        }

       
    }


    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(createKey))
        {
            CreateShape();
        }
        else if (Input.GetKeyDown(newGameKey))
        {
            BeginNewGame();
        }
        else if (Input.GetKeyDown(saveKey))
        {
            storage.Save(this,saveVersion);
        }
        else if (Input.GetKeyDown(loadKey))
        {
            BeginNewGame();
            storage.Load(this);  
        }
    }

    private void BeginNewGame()
    {
        for (int i = 0; i < shapes.Count; i++)
        {
            Destroy(shapes[i].gameObject);
        }
        shapes.Clear();
    }

    private void CreateShape()
    {
        Shape  instance= shapeFactory.GetRandom();
        Transform t = instance.transform;
        t.localPosition = Random.insideUnitSphere * 5f;
        t.localRotation = Random.rotation;
        t.localScale = Vector3.one * Random.Range(0.1f, 1f);
        instance.SetColor(Random.ColorHSV(
                                            hueMin:0f, hueMax:1f, 
                                            saturationMin:0.5f,saturationMax:1f,
                                            valueMin: 0.25f,valueMax: 1f,
                                            alphaMin: 1f,alphaMax: 1f));
        shapes.Add(instance);
    }
}
