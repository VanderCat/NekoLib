using System.Collections;
using System.Numerics;

namespace NekoLib.Core; 

/// <summary>
/// A transform of the GameObject
/// </summary>
public class Transform : Component, IEnumerable<Transform> {
    public IEnumerator<Transform> GetEnumerator() => new TransformEnumerator(this);

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    
    private Transform? _parent;

    /// <summary>
    /// Parent of this Transform, if it is exist
    ///
    /// <para>
    /// You can set parent just with assignment, should also update children on parent transform
    /// </para> 
    /// </summary>
    public Transform? Parent {
        get => _parent;
        set {
            if (value is not null) {
                value._children.Add(this);
            }
            else {
                _parent?._children.Remove(this);
            }
            _parent = value;
        }
    }

    private List<Transform> _children = new();

    public Vector3 Scale {
        get {
            if (Parent is null)
                return LocalScale;
            return Parent.Scale * LocalScale;
        }
    }

    /// <summary>
    /// Position in the world
    /// </summary>
    public Vector3 Position {
        get {
            if (Parent is not null)
                return Parent.Position + LocalPosition;
            return LocalPosition;
        }
        set {
            if (Parent is null) {
                LocalPosition = value;
                return;
            } 
            var inverseSucceded = Matrix4x4.Invert(Parent.GlobalMatrix, out var invertedGlobalMatrix);
            if (inverseSucceded)
                LocalPosition = Vector3.Transform(value, invertedGlobalMatrix);
            else
                throw new ArithmeticException("Could not set global position, due to illegal matrix???"); //todo: more appropriate message
        }
    }

    /// <summary>
    /// Global Rotation
    /// </summary>
    public Quaternion Rotation {
        get {
            if (Parent is not null)
                return Parent.Rotation * LocalRotation;
            return LocalRotation;
        }
        set {
            if (Parent is null) {
                LocalRotation = value;
                return;
            }
            LocalRotation = value / Parent.Rotation;
        }
    }

    /// <summary>
    /// Local Scale of the GameObject
    /// </summary>
    public Vector3 LocalScale { get; set; } = Vector3.One;

    /// <summary>
    /// Position in Parent's space if any otherwise global position
    /// </summary>
    public Vector3 LocalPosition { get; set; } = Vector3.Zero;

    /// <summary>
    /// Local Rotation of the GameObject
    /// </summary>
    public Quaternion LocalRotation { get; set; } = Quaternion.Identity;

    /// <summary>
    /// A matrix to convert from world to local
    /// </summary>
    public Matrix4x4 World2LocalMatrix {
        get {
            if (Parent is null)
                return Matrix4x4.Identity;
            if (!Matrix4x4.Invert(Parent.GlobalMatrix, out var inverted))
                throw new InvalidOperationException("Could not invert the parent GlobalMatrix!");
            return inverted;
        }
    }

    public Matrix4x4 ModelMatrix {
        get => Matrix4x4.CreateFromQuaternion(LocalRotation) * Matrix4x4.CreateScale(LocalScale) *
               Matrix4x4.CreateTranslation(LocalPosition);
        set {
            Matrix4x4.Decompose(value, out var scale, out var rotation, out var translation);
            LocalScale = scale;
            LocalRotation = rotation;
            LocalPosition = translation;
        }
    }
    public Matrix4x4 GlobalMatrix => (ModelMatrix*Parent?.GlobalMatrix) ?? ModelMatrix;

    /// <summary>
    /// How many children this transform has
    /// </summary>
    public int ChildrenCount => _children.Count;
    
    public Vector3 Up => Vector3.Transform(Vector3.UnitY, Rotation);
    public Vector3 Forward => Vector3.Transform(-Vector3.UnitZ, Rotation);
    public Vector3 Right => Vector3.Transform(Vector3.UnitX, Rotation);
    
    public Vector3 Down => Vector3.Transform(-Vector3.UnitY, Rotation);
    public Vector3 Backward => Vector3.Transform(Vector3.UnitZ, Rotation);
    public Vector3 Left => Vector3.Transform(-Vector3.UnitX, Rotation);
    
    /// <summary>
    /// Get child Transform
    /// </summary>
    /// <param name="index">Index of the child</param>
    /// <returns>Transform of this child</returns>
    public Transform GetChild(int index) {
        return _children[index];
    }

    public new string ToString() => $"{base.ToString()}:\n  Pos: {Position}\n  Rot: {Rotation.GetEulerAngles()}\n  Scale: {LocalScale}";
    
    public void LookAt(Vector3 pos) {
        var lookAt = Matrix4x4.CreateLookAt(Position, pos, Vector3.UnitY);
        if (!Matrix4x4.Invert(lookAt, out lookAt))
            throw new InvalidDataException("Failed to invert look at matrix");
        var scale = LocalScale;
        if (Parent is not null) {
            lookAt *= World2LocalMatrix; //to localspace;
        }
        ModelMatrix = lookAt;
        LocalScale = scale; //restore scale;
    }
}

internal class TransformEnumerator : IEnumerator<Transform> {
    private Transform _transform;
    public TransformEnumerator(Transform transform) {
        _transform = transform;
        
    }
    
    public bool MoveNext() {
        Cursor++;
        return _transform.ChildrenCount > Cursor;
    }

    public void Reset() {
        Cursor = -1;
    }

    public int Cursor = -1;

    public Transform Current => _transform.GetChild(Cursor);

    object IEnumerator.Current => Current;

    public void Dispose() {
        Reset();
    }
}