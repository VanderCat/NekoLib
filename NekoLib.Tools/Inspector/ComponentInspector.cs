using System.Reflection;
using NekoLib.Core;

namespace NekoLib.Tools;

[CustomInspector(typeof(Component))]
public class ComponentInspector : ObjectInspector {
    public override void Initialize() {
        base.Initialize();
        
        Members.RemoveAll(Filter);
        if (Target is not Component target) 
            throw new ArgumentException($"The target of type {Target.GetType()} is not assignable to type {typeof(Component)}");
        if (target is Behaviour) _isBehaviour = true;
    }

    private bool _isBehaviour;

    private static bool Filter(MemberInfo info) {
        if (info.DeclaringType is null)
            return false;
        return info.DeclaringType.IsAssignableFrom(typeof(Component)) || 
               info.DeclaringType.IsAssignableFrom(typeof(Behaviour));
    }
}