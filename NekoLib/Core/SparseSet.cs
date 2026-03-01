using System.Collections;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace NekoLib.Core;

/// <summary>
/// Unordered fast set
/// </summary>
/// <typeparam name="T">what to contain</typeparam>
public class SparseSet<T> : ICollection<T>, IReadOnlyCollection<T> {
    private T[] _data = [];
    private uint[] _id = [];
    private uint[] _dataIndex = [];
    private int _size = 0;
    private int _capacity = 0;
    private uint _version = 0;
    public int Count => _size;
    public bool IsSynchronized => false;
    public object SyncRoot => this;
    public bool IsReadOnly => false;
    
    
    public IEnumerator<T> GetEnumerator() {
        // for (int i = 0; i < Count; i++)
        //     yield return _data[i];
        return new Enumerator(this);
    }
    
    IEnumerator IEnumerable.GetEnumerator() {
        return GetEnumerator();
    }

    public void Add(T item) {
        _version++;
        var size = _size;
        var data = _data;

        if (size < _capacity) {
            data[size] = item;
            _size = size + 1;
        }
        else
            PushWithResize(item);
    }
    
    [MethodImpl(MethodImplOptions.NoInlining)]
    private void PushWithResize(T item) {
        Debug.Assert(_size == _capacity);
        Grow(_size + 1);
        _data[_size] = item;
        _size++;
    }
    
    /// <summary>
    /// Ensures that the capacity of this SparseSet is at least the specified <paramref name="capacity"/>.
    /// If the current capacity of the SparseSet is less than specified <paramref name="capacity"/>,
    /// the capacity is increased by continuously twice current capacity until it is at least the specified <paramref name="capacity"/>.
    /// </summary>
    /// <param name="capacity">The minimum capacity to ensure.</param>
    /// <returns>The new capacity of this SparseSet.</returns>
    public int EnsureCapacity(int capacity) {
#if NET8_0_OR_GREATER
        ArgumentOutOfRangeException.ThrowIfNegative(capacity);
#else
        if (capacity < 1)
            throw new ArgumentOutOfRangeException(nameof(capacity), capacity, "The capacity can't be negative");
#endif
        if (_data.Length < capacity)
            Grow(capacity);
        return _capacity;
    }

#if NETSTANDARD
    private const int MaxLength = int.MaxValue;
#else
     private int MaxLength = Array.MaxLength;
#endif
    
    private void Grow(int capacity) {
        Debug.Assert(_data.Length < capacity);

        _capacity = _data.Length == 0 ? 4 : 2 * _data.Length;
        
        if ((uint)_capacity > MaxLength) _capacity = MaxLength;
        
        if (_capacity < capacity) _capacity = capacity;

        Array.Resize(ref _data, _capacity);
        var prevId = _id.Length;
        Array.Resize(ref _id, _capacity);
        Array.Resize(ref _dataIndex, _capacity);
        for (; prevId < _id.Length; prevId++)
            _id[prevId] = _dataIndex[prevId] = (uint)prevId;
    }


    public void Clear() {
        for (var i = 0; i < _size; i++) {
            _data[i] = default!;
        }
        _size = 0;
    }
    public bool Contains(T item) {
        for (var i = 0; i < _size; i++) {
            var a = _data[i];
            if (a is null)
                continue;
            return a.Equals(item);
        }
        return false;
    }
    public void CopyTo(T[] array, int arrayIndex = 0) {
        for (var i = 0; i < _size; i++)
            array[arrayIndex++] = _data[i];
    }
    public void CopyTo(Array array, int index = 0) {
        throw new NotImplementedException();
    }
    public bool Remove(T item) {
        var i = IndexOf(item);
        if (i < 0) return false;
        RemoveAt(i);
        return true;
    }
    public void RemoveAt(int index) {
// #if NET8_0_OR_GREATER
//         ArgumentOutOfRangeException.ThrowIfNegative(index);
//         ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, _size);
// #else
//         if (index < 0 || index >= _size)
//             throw new ArgumentOutOfRangeException(nameof(index), index, "Specified index is incorrect");
// #endif
        _version++;
        var physicalIndex = (int)_dataIndex[index];
        if (physicalIndex != _size - 1)
            if (!Swap(physicalIndex, _size - 1))
                throw new Exception("Unknown error");
        
        _data[_size - 1] = default!;
        _size--;
    }

    public bool Swap(int a, int b) {
#if NET8_0_OR_GREATER
        ArgumentOutOfRangeException.ThrowIfNegative(a);
        ArgumentOutOfRangeException.ThrowIfNegative(b);
#else
        if (a < 0)
            throw new ArgumentOutOfRangeException(nameof(a), a, "Specified index is incorrect");
        if (b < 0)
            throw new ArgumentOutOfRangeException(nameof(b), b, "Specified index is incorrect");
#endif
        _version++;
        if (a >= _capacity || b >= _capacity)
            return false;
        (_data[b], _data[a]) = (_data[a], _data[b]);
        (_id[b], _id[a]) = (_id[a], _id[b]);
        _dataIndex[_id[a]] = (uint)a;
        _dataIndex[_id[b]] = (uint)b;
        return true;
    }
    
    public int IndexOf(T item) {
        for (var i = 0; i < _size; i++) {
            var a = _data[i];
            if (a is null)
                continue;
            if (!a.Equals(item))
                continue;
            return (int)_id[i];
        }
        return -1;
    }
    
    public void Insert(int index, T item) {
#if NET8_0_OR_GREATER
        ArgumentOutOfRangeException.ThrowIfNegative(index);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(index, _size);
#else
        if (index < 0 || index >= _size)
            throw new ArgumentOutOfRangeException(nameof(index), index, "Specified index is incorrect");
#endif
        Add(item);
        for (int i = _size - 1; i > index; i--) {
            Swap(i-1, i);
        }
    }
    
    public T this[int index] {
        get => _data[_dataIndex[index]];
        set => _data[_dataIndex[index]] = value;
    }
    
    private struct Enumerator(SparseSet<T> parent) : IEnumerator<T> {
        public T Current => parent._data[_cursor];
        object? IEnumerator.Current => Current;
        private uint _version = parent._version;

        private int _cursor = -1;
        public bool MoveNext() {
            if (_version != parent._version)
                throw new InvalidOperationException("Collection was modified.");
            _cursor++;
            return _cursor < parent.Count;
        }
        public void Reset() {
            _cursor = parent.Count;
        }
        
        public void Dispose() {
            
        }
    }
}