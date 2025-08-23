using System.Runtime.CompilerServices;

namespace NekoLib.Extra; 

/// <summary>
/// Ref struct that run callback on dispose
/// </summary>
/// <remarks>cannot be used as class member</remarks>
public readonly ref struct AttachMode {
    private readonly Action OnDetach;
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public AttachMode(Action onDetach) {
        OnDetach = onDetach;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Dispose() {
        OnDetach();
    }

    /// <summary>
    /// Alias for Dispose.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Detach() => Dispose();
}