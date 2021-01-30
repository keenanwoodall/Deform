using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using static Unity.Mathematics.math;
using float3 = Unity.Mathematics.float3;

namespace Deform
{
    [Deformer(Name = "Lattice", Description = "Free-form deform a mesh using lattice control points", Type = typeof(LatticeDeformer))]
    [HelpURL("https://github.com/keenanwoodall/Deform/wiki/LatticeDeformer")]
    public class LatticeDeformer : Deformer
    {
        public Transform Target
        {
            get
            {
                if (target == null)
                    target = transform;
                return target;
            }
            set { target = value; }
        }

        [SerializeField, HideInInspector] private Transform target;
        
        public float3[] Corners
        {
            get => corners;
            set => corners = value;
        }

        public Vector3Int Resolution
        {
            get => resolution;
            set => resolution = value;
        }

        [SerializeField] private float3[] corners;
        [SerializeField] private Vector3Int resolution = new Vector3Int(2, 2, 2);


        protected virtual void Reset()
        {
            GenerateCorners(resolution);
        }

        public void GenerateCorners(Vector3Int newResolution)
        {
            resolution = newResolution;
            corners = new float3[resolution.x * resolution.y * resolution.z];

            for (int z = 0; z < resolution.z; z++)
            {
                for (int y = 0; y < resolution.y; y++)
                {
                    for (int x = 0; x < resolution.x; x++)
                    {
                        int index = GetIndex(x, y, z);

                        // TODO - Clean up these calculations
                        corners[index] = 0.5f * new float3(Mathf.Lerp(-1, 1, x / (float) (resolution.x - 1)),
                            Mathf.Lerp(-1, 1, y / (float) (resolution.y - 1)), Mathf.Lerp(-1, 1, z / (float) (resolution.z - 1)));
                    }
                }
            }
        }

        public int GetIndex(int x, int y, int z)
        {
            return x + y * resolution.x + z * (resolution.x * resolution.y);
        }

        public override DataFlags DataFlags => DataFlags.Vertices;

        public override JobHandle Process(MeshData data, JobHandle dependency = default)
        {
            var meshToAxis = DeformerUtils.GetMeshToAxisSpace (Target, data.Target.GetTransform ());

            return new LatticeJob
            {
                corners = new NativeArray<float3>(corners, Allocator.TempJob),
                resolution = new int3(resolution.x, resolution.y, resolution.z),
                meshToTarget = meshToAxis,
                targetToMesh = meshToAxis.inverse,
                vertices = data.DynamicNative.VertexBuffer
            }.Schedule(data.Length, DEFAULT_BATCH_COUNT, dependency);
        }

        [BurstCompile(CompileSynchronously = COMPILE_SYNCHRONOUSLY)]
        public struct LatticeJob : IJobParallelFor
        {
            [DeallocateOnJobCompletion, ReadOnly] public NativeArray<float3> corners;
            [ReadOnly] public int3 resolution;
            [ReadOnly] public float4x4 meshToTarget;
            [ReadOnly] public float4x4 targetToMesh;
            public NativeArray<float3> vertices;

            // TODO -  Rewrite these calculations to be simpler now that the maths is figured out
            public void Execute(int index)
            {
                // Convert from [-0.5,0.5] space to [0,1]
                var sourcePosition = transform(meshToTarget, vertices[index]) + float3(0.5f, 0.5f, 0.5f);
                
                // Determine the negative corner of the lattice cell containing the source position
                var negativeCorner = new int3((int) (sourcePosition.x * (resolution.x - 1)), (int) (sourcePosition.y * (resolution.y - 1)), (int) (sourcePosition.z * (resolution.z - 1)));

                // Clamp the corner to an acceptable range in case the source vertex is outside or on the lattice bounds
                negativeCorner = max(negativeCorner, new int3(0, 0, 0));
                negativeCorner = min(negativeCorner, resolution - new int3(2, 2, 2));

                int index0 = (negativeCorner.x + 0) + (negativeCorner.y + 0) * resolution.x + (negativeCorner.z + 0) * (resolution.x * resolution.y);
                int index1 = (negativeCorner.x + 1) + (negativeCorner.y + 0) * resolution.x + (negativeCorner.z + 0) * (resolution.x * resolution.y);
                int index2 = (negativeCorner.x + 0) + (negativeCorner.y + 1) * resolution.x + (negativeCorner.z + 0) * (resolution.x * resolution.y);
                int index3 = (negativeCorner.x + 1) + (negativeCorner.y + 1) * resolution.x + (negativeCorner.z + 0) * (resolution.x * resolution.y);
                int index4 = (negativeCorner.x + 0) + (negativeCorner.y + 0) * resolution.x + (negativeCorner.z + 1) * (resolution.x * resolution.y);
                int index5 = (negativeCorner.x + 1) + (negativeCorner.y + 0) * resolution.x + (negativeCorner.z + 1) * (resolution.x * resolution.y);
                int index6 = (negativeCorner.x + 0) + (negativeCorner.y + 1) * resolution.x + (negativeCorner.z + 1) * (resolution.x * resolution.y);
                int index7 = (negativeCorner.x + 1) + (negativeCorner.y + 1) * resolution.x + (negativeCorner.z + 1) * (resolution.x * resolution.y);

                var localizedSourcePosition = (sourcePosition) * (resolution - new int3(1, 1, 1)) - negativeCorner;

                var newPosition = float3.zero;

                {
                    int axisIndex = 0;
                    int secondaryAxisIndex = 1;
                    int tertiaryAxisIndex = 2;
                    var min1 = lerp(corners[index0][axisIndex], corners[index2][axisIndex], localizedSourcePosition[secondaryAxisIndex]);
                    var max1 = lerp(corners[index1][axisIndex], corners[index3][axisIndex], localizedSourcePosition[secondaryAxisIndex]);

                    var min2 = lerp(corners[index4][axisIndex], corners[index6][axisIndex], localizedSourcePosition[secondaryAxisIndex]);
                    var max2 = lerp(corners[index5][axisIndex], corners[index7][axisIndex], localizedSourcePosition[secondaryAxisIndex]);

                    var min = lerp(min1, min2, localizedSourcePosition[tertiaryAxisIndex]);
                    var max = lerp(max1, max2, localizedSourcePosition[tertiaryAxisIndex]);

                    newPosition[axisIndex] = lerp(min, max, localizedSourcePosition[axisIndex]);
                }

                {
                    int axisIndex = 1;
                    int secondaryAxisIndex = 0;
                    int tertiaryAxisIndex = 2;
                    var min1 = lerp(corners[index0][axisIndex], corners[index1][axisIndex], localizedSourcePosition[secondaryAxisIndex]);
                    var max1 = lerp(corners[index2][axisIndex], corners[index3][axisIndex], localizedSourcePosition[secondaryAxisIndex]);

                    var min2 = lerp(corners[index4][axisIndex], corners[index5][axisIndex], localizedSourcePosition[secondaryAxisIndex]);
                    var max2 = lerp(corners[index6][axisIndex], corners[index7][axisIndex], localizedSourcePosition[secondaryAxisIndex]);

                    var min = lerp(min1, min2, localizedSourcePosition[tertiaryAxisIndex]);
                    var max = lerp(max1, max2, localizedSourcePosition[tertiaryAxisIndex]);

                    newPosition[axisIndex] = lerp(min, max, localizedSourcePosition[axisIndex]);
                }

                {
                    int axisIndex = 2;
                    int secondaryAxisIndex = 0;
                    int tertiaryAxisIndex = 1;
                    var min1 = lerp(corners[index0][axisIndex], corners[index1][axisIndex], localizedSourcePosition[secondaryAxisIndex]);
                    var max1 = lerp(corners[index4][axisIndex], corners[index5][axisIndex], localizedSourcePosition[secondaryAxisIndex]);

                    var min2 = lerp(corners[index2][axisIndex], corners[index3][axisIndex], localizedSourcePosition[secondaryAxisIndex]);
                    var max2 = lerp(corners[index6][axisIndex], corners[index7][axisIndex], localizedSourcePosition[secondaryAxisIndex]);

                    var min = lerp(min1, min2, localizedSourcePosition[tertiaryAxisIndex]);
                    var max = lerp(max1, max2, localizedSourcePosition[tertiaryAxisIndex]);

                    newPosition[axisIndex] = lerp(min, max, localizedSourcePosition[axisIndex]);
                }

                vertices[index] = transform(targetToMesh,newPosition);
            }
        }
    }
}