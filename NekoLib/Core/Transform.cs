using System.Collections;
using System.Numerics;

namespace NekoLib.Core; 

public class Transform : Component, IEnumerable<Transform> {
    public IEnumerator<Transform> GetEnumerator() => new TransformEnumerator(this);

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public Transform? _parent;

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

    //public Vector3 Scale;
    public Vector3 Position => GlobalMatrix.Translation;

    public Quaternion Rotation => Quaternion.CreateFromRotationMatrix(GlobalMatrix);

    private Vector3 _localScale = Vector3.One;
    private Vector3 _localPosition = Vector3.Zero;
    private Quaternion _localRotation = Quaternion.Identity;

    public Vector3 LocalScale {
        get => _localScale;
        set {
            _localScale = value;
            RecalculateMatrix();
        }
    }

    public Vector3 LocalPosition {
        get => _localPosition;
        set {
            _localPosition = value;
            RecalculateMatrix();
        }
    }

    public Quaternion LocalRotation {
        get => _localRotation;
        set {
            _localRotation = value;
            RecalculateMatrix();
        }
    }

    public Matrix4x4 World2LocalMatrix;
    private Matrix4x4 _matrix = Matrix4x4.Identity;
    private Matrix4x4 GlobalMatrix => (_matrix*Parent?.GlobalMatrix) ?? _matrix;

    public int ChildrenCount => _children.Count;

    public Vector3 Up => Vector3.Transform(Vector3.UnitY, Rotation);
    public Vector3 Forward => Vector3.Transform(Vector3.UnitX, Rotation);
    public Vector3 Right => Vector3.Transform(Vector3.UnitZ, Rotation);
    
    public Vector3 Down => -Up;
    public Vector3 Backward => -Forward;
    public Vector3 Left => -Right;

    public Transform GetChild(int index) {
        return _children[index];
    }

    private void RecalculateMatrix() {
        _matrix = Matrix4x4.CreateScale(LocalScale) * Matrix4x4.CreateFromQuaternion(LocalRotation) *
            Matrix4x4.CreateTranslation(LocalPosition);
    }

    public new string ToString() => $"{base.ToString()}:\n  Pos: {Position}\n  Rot: {Rotation.GetEulerAngles()}\n  Scale: {LocalScale}";
}

internal class TransformEnumerator : IEnumerator<Transform> {
    private Transform _transform;
    public TransformEnumerator(Transform transform) {
        _transform = transform;
        
    }
    
    public bool MoveNext() {
        Cursor++;
        return _transform.ChildrenCount >= Cursor;
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