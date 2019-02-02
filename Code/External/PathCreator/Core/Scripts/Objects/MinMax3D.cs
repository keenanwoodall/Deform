using UnityEngine;

namespace PathCreation {
    public class MinMax3D {

        public Vector3 Min { get; private set; }
        public Vector3 Max { get; private set; }

        public MinMax3D()
        {
            Min = Vector3.one * float.MaxValue;
            Max = Vector3.one * float.MinValue;
        }

        public void AddValue(Vector3 v)
        {
            Min = new Vector3(Mathf.Min(Min.x, v.x), Mathf.Min(Min.y,v.y), Mathf.Min(Min.z,v.z));
            Max = new Vector3(Mathf.Max(Max.x, v.x), Mathf.Max(Max.y,v.y), Mathf.Max(Max.z,v.z));
        }
    }
}