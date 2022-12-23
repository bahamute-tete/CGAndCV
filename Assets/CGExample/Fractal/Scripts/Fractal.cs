using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Burst;
using Unity.Jobs;
using static Unity.Mathematics.math;
using float4x4 = Unity.Mathematics.double4x4;
using quaternion = Unity.Mathematics.quaternion;
using Random = UnityEngine.Random;
using Unity.Mathematics;

public class Fractal : MonoBehaviour
{
    [SerializeField] Mesh mesh, leafMesh;
    [SerializeField] Material material;
    [SerializeField] Gradient gradientA, gradientB;
    [SerializeField] Color leafColorA, leafColorB;
    [SerializeField, Range(0, 90f)] float maxSagAngleA = 15f, maxSagAngleB = 25f;
    [SerializeField, Range(0, 90f)] float spinVelocityA = 20f, spinVelocityB = 25f;
    [SerializeField, Range(0f, 1f)] float reverseSpinChance = 0.25f;
    [SerializeField, Range(3, 8)] int depth = 4;

    static Vector3[] dir = { up(), right(), left(), forward(),back() };

    static Quaternion[] rotation = { quaternion.identity, quaternion.RotateZ(-0.5f*PI), quaternion.RotateZ(0.5f * PI), quaternion.RotateX(0.5f * PI), quaternion.RotateX(-0.5f * PI) };

    
    

    #region OldCode
    // Start is called before the first frame update
    //void Start()
    //{
    //    name = "Fractal" + depth;
    //    if (depth <= 1)
    //    { return; }

    //    Fractal childA = CreatChild(Vector3.up,Quaternion.identity);
    //    Fractal childB = CreatChild(Vector3.right,Quaternion.Euler(0,0,-90f));
    //    Fractal childC = CreatChild(Vector3.left, Quaternion.Euler(0, 0, 90f));
    //    Fractal childD = CreatChild(Vector3.forward, Quaternion.Euler(90f, 0, 0));
    //    Fractal childE = CreatChild(Vector3.back, Quaternion.Euler(-90f, 0, 0));
    //    childA.transform.SetParent(transform, false);
    //    childB.transform.SetParent(transform, false);
    //    childC.transform.SetParent(transform, false);
    //    childD.transform.SetParent(transform, false);
    //    childE.transform.SetParent(transform, false);
    //}

    //Fractal CreatChild(Vector3 dir,Quaternion rotation) {
    //    Fractal child = Instantiate(this);
    //    child.depth = depth - 1;
    //    child.transform.localPosition = dir * 0.75f;
    //    child.transform.localRotation = rotation;
    //    child.transform.localScale = 0.5f * Vector3.one;
    //    return child;
    //}

    // Update is called once per frame
    //void Update()
    //{
    //    transform.Rotate(0f, 22.5f * Time.deltaTime, 0f);
    //}

    //struct FractalPart
    //{
    //    public Vector3 dir, worldPosition;
    //    public Quaternion rotation, worldRotation;
    //    public Transform transform;
    //}

    //FractalPart CreatePart(int levelIndex,int childIndex,float scale)
    //{
    //    var go = new GameObject("Fractal Part L" + levelIndex + " C" + childIndex);
    //    go.transform.localScale = scale * Vector3.one;
    //    go.transform.SetParent(transform, false);
    //    go.AddComponent<MeshFilter>().mesh = mesh;
    //    go.AddComponent<MeshRenderer>().material = material;


    //    return new FractalPart
    //    {
    //        dir = dir[childIndex],
    //        rotation = rotation[childIndex],
    //        transform = go.transform
    //    };
    //}
    #endregion
    struct FractalPart
    {
        public Vector3  dir, worldPosition;
       
        public Quaternion rotation, worldRotation;
        //public Transform transform;
        public float spinAngle;
        public float maxSagAngel;
        public float spinVelocity;
    }

    // FractalPart[][] parts;
    NativeArray<FractalPart>[] parts;



    FractalPart CreatePart(int childIndex)
    {
        return new FractalPart
        {
            dir = dir[childIndex],
            rotation = rotation[childIndex],
            maxSagAngel = radians(Random.Range(maxSagAngleA, maxSagAngleB)),
            spinVelocity =(Random.value<reverseSpinChance)?-1f:1f * radians(Random.Range(spinVelocityA, spinVelocityB))

        };
    }

    // Matrix4x4[][] matrices;
    NativeArray<Matrix4x4>[] matrices;
    ComputeBuffer[] matricesBuffers;


    [BurstCompile(FloatPrecision.Standard,FloatMode.Fast,CompileSynchronously = true)]
    struct UpdateFractalLevelJop : IJobFor
    {
        public float spinAngleDelta;
        public float scale;
        public float deltaTime;

        [ReadOnly]
        public NativeArray<FractalPart> parents;
        public NativeArray<FractalPart> parts;

        [WriteOnly]
        public NativeArray<Matrix4x4> matrices;

        public void Execute(int i)
        {
            FractalPart parent = parents[i / 5];
            FractalPart part = parts[i];
           

            //part.spinAngle += spinAngleDelta;
            part.spinAngle += part.spinVelocity*deltaTime;

            Vector3 upAxis = mul(mul(parent.worldRotation, part.rotation), up());
            Vector3 sagAxis = cross(up(), upAxis);
            float sagMagnitude = length(sagAxis);
            quaternion baseRotation;

            if (sagMagnitude > 0f)
            {
                sagAxis /= sagMagnitude;
                //quaternion sagRotation = quaternion.AxisAngle(sagAxis,0.25f*PI);
                quaternion sagRotation = quaternion.AxisAngle(sagAxis, part.maxSagAngel* sagMagnitude);
                baseRotation = mul(sagRotation, parent.worldRotation);
            }
            else
            {
                baseRotation = parent.worldRotation;
            }
           
           
            part.worldRotation =mul( baseRotation , mul( part.rotation , quaternion.RotateY(part.spinAngle)));

            Vector3 temp = mul(part.worldRotation, float3(0, 1.5f * scale, 0));
            //Vector3 temp = mul(parent.worldRotation, (1.5f*scale*part.dir));
            part.worldPosition = parent.worldPosition + temp;

            parts[i] = part;

            matrices[i] = Matrix4x4.TRS(part.worldPosition, part.worldRotation, float3(scale));

            
        }
    }


    static readonly int
        matricesID = Shader.PropertyToID("_Matrices"),
        colorAID = Shader.PropertyToID("_ColorA"),
        colorBID = Shader.PropertyToID("_ColorB"),
        sequenceNumbersID = Shader.PropertyToID("_SequenceNumbers");

    Vector4[] sequenceNumbers;
    static MaterialPropertyBlock propertyBlock;

    private void OnEnable ()
    {
        parts = new NativeArray<FractalPart>[depth];
        matrices = new NativeArray<Matrix4x4>[depth];
        matricesBuffers = new ComputeBuffer[depth];
        sequenceNumbers = new Vector4[depth];
        int stride = 16 * 4;
        
        for (int i = 0,length =1; i < parts.Length; i++,length*=5)
        {
            parts[i] = new NativeArray<FractalPart>(length,Allocator.Persistent);
            matrices[i] = new NativeArray<Matrix4x4>(length, Allocator.Persistent);
            matricesBuffers[i] = new ComputeBuffer(length, stride);

            //1,2 =Color  3,4 = smoothness
            sequenceNumbers[i] = new Vector4(Random.value,Random.value, Random.value, Random.value);
        }

        //float scale = 1;
        //parts[0][0] = CreatePart(0,0,scale);
        parts[0][0] = CreatePart(0);
      

        for (int li = 1; li < parts.Length; li++)
        {
           
            //scale *= 0.5f;
            NativeArray<FractalPart> levelParts = parts[li];
            for (int fpi = 0; fpi < levelParts.Length; fpi+=5)
            {
                for (int ci = 0; ci < 5; ci++)
                {
                    //levelParts[fpi+ci] = CreatePart(li,ci,scale);
                    levelParts[fpi + ci] = CreatePart( ci);
                }
                
            }
        }

        if (propertyBlock == null)
        {
            propertyBlock = new MaterialPropertyBlock();
        }

    }


   

    private void OnDisable()
    {
        for (int i = 0; i < matricesBuffers.Length; i++)
        {
            matricesBuffers[i].Release();
            parts[i].Dispose();
            matrices[i].Dispose();
        }
        parts = null;
        matrices = null;
        matricesBuffers = null;
        sequenceNumbers = null;
    }


    private void OnValidate()
    {
        if (parts != null && enabled)
        {
            OnDisable();
            OnEnable();
        }
       
    }

    private void Update()
    {
        //Quaternion deltaRotation = Quaternion.Euler(0, 22.5f * Time.deltaTime, 0);
        //float spinAngleDelta = 0.125f*PI * Time.deltaTime*0;
        FractalPart rootPart = parts[0][0];
        float deltaTime = Time.deltaTime;
        //rootPart.rotation *= deltaRotation;
        //rootPart.spinAngle += spinAngleDelta;
        rootPart.spinAngle += rootPart.spinVelocity* deltaTime;
        //Debug.Log(" rootPart.spinAngle " + rootPart.spinAngle);
        rootPart.worldRotation =mul(transform.rotation,( rootPart.rotation*quaternion.RotateY(rootPart.spinAngle)));
        
        rootPart.worldPosition = transform.position;
        parts[0][0] = rootPart;

        float objectiveScale = transform.lossyScale.x;
        matrices[0][0] = Matrix4x4.TRS(rootPart.worldPosition, rootPart.worldRotation, float3(objectiveScale));

        float scale = objectiveScale;

        JobHandle jobHandle = default;
        for (int li = 1; li < parts.Length; li++)
        {
            scale *= 0.5f;

            jobHandle = new UpdateFractalLevelJop
            {
                //spinAngleDelta = spinAngleDelta,
                deltaTime = deltaTime,
                scale = scale,
                parents = parts[li - 1],
                parts = parts[li],
                matrices = matrices[li]
            }.ScheduleParallel(parts[li].Length, 1,jobHandle);

           
        }
        jobHandle.Complete();


        var bound = new Bounds(rootPart.worldPosition, 3 *objectiveScale* Vector3.one);

        int leafIndex = matricesBuffers.Length - 1;

        for (int i = 0; i < matricesBuffers.Length; i++)
        {
            ComputeBuffer buffer = matricesBuffers[i];
            // Color lerpColor = gradient.Evaluate (i / (matricesBuffers.Length - 1f));

            Color colorA, colorB;
            Mesh instanceMesh;
            if (i == leafIndex)
            {
                colorA = leafColorA;
                colorB = leafColorB;
                instanceMesh = leafMesh;
            }
            else
            {
                float gradientInterpolator = i / (matricesBuffers.Length - 2f);
                colorA = gradientA.Evaluate(gradientInterpolator);
                colorB = gradientB.Evaluate(gradientInterpolator);
                instanceMesh = mesh;
            }


           
            propertyBlock.SetColor(colorAID, colorA);
            propertyBlock.SetColor(colorBID, colorB);

            buffer.SetData(matrices[i]);
            propertyBlock.SetBuffer(matricesID, buffer);
            propertyBlock.SetVector(sequenceNumbersID, sequenceNumbers[i]);
            material.SetBuffer(matricesID, buffer);
            Graphics.DrawMeshInstancedProcedural(instanceMesh, 0, material, bound, buffer.count,propertyBlock);
        }

       
    }
}
