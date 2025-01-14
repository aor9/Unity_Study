using System.Numerics;
using UnityEngine;
using Matrix4x4 = UnityEngine.Matrix4x4;
using Vector3 = UnityEngine.Vector3;

public abstract class Transformation : MonoBehaviour
{
    public Vector3 Apply(Vector3 point)
    {
        return Matrix.MultiplyPoint(point);
    }
    
    public abstract Matrix4x4 Matrix { get; }
}
