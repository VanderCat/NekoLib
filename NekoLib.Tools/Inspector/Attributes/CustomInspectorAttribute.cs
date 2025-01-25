using JetBrains.Annotations;

namespace NekoLib.Tools; 

[MeansImplicitUse]
public class CustomInspectorAttribute : Attribute {
    public Type InspectorType;

    public CustomInspectorAttribute(Type inspectorType) {
        InspectorType = inspectorType;
    }
}