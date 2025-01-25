using JetBrains.Annotations;

namespace NekoLib.Extra; 

[MeansImplicitUse]
[AttributeUsage(AttributeTargets.Method)]
public class ConTagHandlerAttribute : Attribute {
    public string Tag;

    public ConTagHandlerAttribute(string tag) {
        Tag = tag;
    }
}