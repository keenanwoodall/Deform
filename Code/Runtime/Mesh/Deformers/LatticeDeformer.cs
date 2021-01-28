using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using static Unity.Mathematics.math;

namespace Deform
{
    [Deformer(Name = "Lattice", Description = "Free-form deform a mesh using lattice control points", Type = typeof(LatticeDeformer))]
    [HelpURL("https://github.com/keenanwoodall/Deform/wiki/LatticeDeformer")]
    public class LatticeDeformer : Deformer
    {
        public float3[] Corners
        {
            get => corners;
            set => corners = value;
        }

        [SerializeField] private float3[] corners =
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

        public override DataFlags DataFlags => DataFlags.Vertices;

        public override JobHandle Process(MeshData data, JobHandle dependency = default)
        {
            return new LatticeJob
            {
                corners = new NativeArray<float3>(corners, Allocator.TempJob),
                vertices = data.DynamicNative.VertexBuffer
            }.Schedule(data.Length, DEFAULT_BATCH_COUNT, dependency);
        }

        [BurstCompile(CompileSynchronously = COMPILE_SYNCHRONOUSLY)]
        public struct LatticeJob : IJobParallelFor
        {
            [DeallocateOnJobCompletion, ReadOnly] public NativeArray<float3> corners;
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
                    var min1 = lerp(corners[0][axisIndex], corners[1][axisIndex], sourcePosition[secondaryAxisIndex]);
                    var max1 = lerp(corners[3][axisIndex], corners[2][axisIndex], sourcePosition[secondaryAxisIndex]);

                    var min2 = lerp(corners[0 + 4][axisIndex], corners[1 + 4][axisIndex], sourcePosition[secondaryAxisIndex]);
                    var max2 = lerp(corners[3 + 4][axisIndex], corners[2 + 4][axisIndex], sourcePosition[secondaryAxisIndex]);

                    var min = lerp(min1, min2, sourcePosition[tertiaryAxisIndex]);
                    var max = lerp(max1, max2, sourcePosition[tertiaryAxisIndex]);

                    newPosition[axisIndex] = lerp(min, max, sourcePosition[axisIndex]);
                }

                {
                    int axisIndex = 1;
                    int secondaryAxisIndex = 0;
                    int tertiaryAxisIndex = 2;
                    var min1 = lerp(corners[0][axisIndex], corners[3][axisIndex], sourcePosition[secondaryAxisIndex]);
                    var max1 = lerp(corners[1][axisIndex], corners[2][axisIndex], sourcePosition[secondaryAxisIndex]);

                    var min2 = lerp(corners[0 + 4][axisIndex], corners[3 + 4][axisIndex], sourcePosition[secondaryAxisIndex]);
                    var max2 = lerp(corners[1 + 4][axisIndex], corners[2 + 4][axisIndex], sourcePosition[secondaryAxisIndex]);

                    var min = lerp(min1, min2, sourcePosition[tertiaryAxisIndex]);
                    var max = lerp(max1, max2, sourcePosition[tertiaryAxisIndex]);

                    newPosition[axisIndex] = lerp(min, max, sourcePosition[axisIndex]);
                }

                {
                    int axisIndex = 2;
                    int secondaryAxisIndex = 0;
                    int tertiaryAxisIndex = 1;
                    var min1 = lerp(corners[0][axisIndex], corners[3][axisIndex], sourcePosition[secondaryAxisIndex]);
                    var max1 = lerp(corners[0 + 4][axisIndex], corners[3 + 4][axisIndex], sourcePosition[secondaryAxisIndex]);

                    var min2 = lerp(corners[1][axisIndex], corners[2][axisIndex], sourcePosition[secondaryAxisIndex]);
                    var max2 = lerp(corners[1 + 4][axisIndex], corners[2 + 4][axisIndex], sourcePosition[secondaryAxisIndex]);

                    var min = lerp(min1, min2, sourcePosition[tertiaryAxisIndex]);
                    var max = lerp(max1, max2, sourcePosition[tertiaryAxisIndex]);

                    newPosition[axisIndex] = lerp(min, max, sourcePosition[axisIndex]);
                }

                vertices[index] = newPosition;
            }
        }
    }
}