using System;
using UnityEngine;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace Deform
{
    [Deformer(Name = "Lattice", Description = "Free form deform a mesh", Type = typeof(LatticeDeformer))]
    [HelpURL("https://github.com/keenanwoodall/Deform/wiki/LatticeDeformer")]
    public class LatticeDeformer : Deformer
    {
        public float3[] Corners
        {
            get => corners;
            set => corners = value;
        }

        [SerializeField] private float3[] corners = new[]
        {
            0.5f * new float3(-1, -1, -1),
            0.5f * new float3(-1, 1, -1),
            0.5f * new float3(1, 1, -1),
            0.5f * new float3(1, -1, -1),

            0.5f * new float3(-1, -1, 1),
            0.5f * new float3(-1, 1, 1),
            0.5f * new float3(1, 1, 1),
            0.5f * new float3(1, -1, 1),
        };

        private NativeArray<float3> cornersNative;
        
        public override DataFlags DataFlags => DataFlags.Vertices;

        public override JobHandle Process(MeshData data, JobHandle dependency = default(JobHandle))
        {
            if (cornersNative.Length != corners.Length)
            {
                if (cornersNative.IsCreated)
                {
                    cornersNative.Dispose();
                }
                cornersNative = new NativeArray<float3>(corners, Allocator.Persistent);
            }

            for (int i = 0; i < corners.Length; i++)
            {
                cornersNative[i] = corners[i];
            }
            
            return new LatticeJob
            {
                corners = cornersNative,
                vertices = data.DynamicNative.VertexBuffer
            }.Schedule(data.Length, DEFAULT_BATCH_COUNT, dependency);
        }

        [BurstCompile(CompileSynchronously = COMPILE_SYNCHRONOUSLY)]
        public struct LatticeJob : IJobParallelFor
        {
            [ReadOnly] public NativeArray<float3> corners;
            public NativeArray<float3> vertices;

            public void Execute(int index)
            {
                // Convert from [-0.5,0.5] space to [0,1]
                var sourcePosition = vertices[index] + float3(0.5f, 0.5f, 0.5f);

                var newPosition = vertices[index];

                {
                    int axisIndex = 0;
                    int secondaryAxisIndex = 1;
                    int tertiaryAxisIndex = 2;
                    var min1 = Mathf.Lerp(corners[0][axisIndex], corners[1][axisIndex], sourcePosition[secondaryAxisIndex]);
                    var max1 = Mathf.Lerp(corners[3][axisIndex], corners[2][axisIndex], sourcePosition[secondaryAxisIndex]);

                    var min2 = Mathf.Lerp(corners[0 + 4][axisIndex], corners[1 + 4][axisIndex], sourcePosition[secondaryAxisIndex]);
                    var max2 = Mathf.Lerp(corners[3 + 4][axisIndex], corners[2 + 4][axisIndex], sourcePosition[secondaryAxisIndex]);

                    var min = Mathf.Lerp(min1, min2, sourcePosition[tertiaryAxisIndex]);
                    var max = Mathf.Lerp(max1, max2, sourcePosition[tertiaryAxisIndex]);

                    newPosition[axisIndex] = Mathf.Lerp(min, max, sourcePosition[axisIndex]);
                }

                {
                    int axisIndex = 1;
                    int secondaryAxisIndex = 0;
                    int tertiaryAxisIndex = 2;
                    var min1 = Mathf.Lerp(corners[0][axisIndex], corners[3][axisIndex], sourcePosition[secondaryAxisIndex]);
                    var max1 = Mathf.Lerp(corners[1][axisIndex], corners[2][axisIndex], sourcePosition[secondaryAxisIndex]);

                    var min2 = Mathf.Lerp(corners[0 + 4][axisIndex], corners[3 + 4][axisIndex], sourcePosition[secondaryAxisIndex]);
                    var max2 = Mathf.Lerp(corners[1 + 4][axisIndex], corners[2 + 4][axisIndex], sourcePosition[secondaryAxisIndex]);

                    var min = Mathf.Lerp(min1, min2, sourcePosition[tertiaryAxisIndex]);
                    var max = Mathf.Lerp(max1, max2, sourcePosition[tertiaryAxisIndex]);

                    newPosition[axisIndex] = Mathf.Lerp(min, max, sourcePosition[axisIndex]);
                }

                {
                    int axisIndex = 2;
                    int secondaryAxisIndex = 0;
                    int tertiaryAxisIndex = 1;
                    var min1 = Mathf.Lerp(corners[0][axisIndex], corners[3][axisIndex], sourcePosition[secondaryAxisIndex]);
                    var max1 = Mathf.Lerp(corners[0 + 4][axisIndex], corners[3 + 4][axisIndex], sourcePosition[secondaryAxisIndex]);

                    var min2 = Mathf.Lerp(corners[1][axisIndex], corners[2][axisIndex], sourcePosition[secondaryAxisIndex]);
                    var max2 = Mathf.Lerp(corners[1 + 4][axisIndex], corners[2 + 4][axisIndex], sourcePosition[secondaryAxisIndex]);

                    var min = Mathf.Lerp(min1, min2, sourcePosition[tertiaryAxisIndex]);
                    var max = Mathf.Lerp(max1, max2, sourcePosition[tertiaryAxisIndex]);

                    newPosition[axisIndex] = Mathf.Lerp(min, max, sourcePosition[axisIndex]);
                }

                vertices[index] = newPosition;
            }
        }

        private void OnDisable()
        {
            if(cornersNative.IsCreated)
            {
                cornersNative.Dispose();
            }
        }
    }
}