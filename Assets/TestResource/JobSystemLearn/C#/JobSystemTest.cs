using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using UnityEngine.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using static Unity.Mathematics.math;

public class JobSystemTest : MonoBehaviour
{

    
    struct VelocityJob : IJob
    {
        [ReadOnly]
        public NativeArray<float3> velocity;
        public NativeArray<float3> position;

        public float deltaTime;

        public void Execute()
        {
            for (int i = 0; i < position.Length; i++)
            {
                position[i] = position[i] + velocity[i] * deltaTime;
            }
        }
    }


    private void Update()
    {
        var position = new NativeArray<float3>(500, Allocator.TempJob);
        var velocity = new NativeArray<float3>(500, Allocator.TempJob);

        for (int i = 0; i < velocity.Length; i++)
        {
            velocity[i] = new Vector3(0.10f, 0);
        }

        var job = new VelocityJob()
        {
            deltaTime = Time.deltaTime,
            position =  position,
            velocity = velocity
        };

        JobHandle handle = job.Schedule();

        handle.Complete();

        Debug.Log(job.position[20]);

        position.Dispose();
        velocity.Dispose();
    }


}
