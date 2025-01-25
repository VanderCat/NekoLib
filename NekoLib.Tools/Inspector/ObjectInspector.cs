using System.Reflection;
using ImGuiNET;

namespace NekoLib.Tools; 

[CustomInspector(typeof(object))]
public class ObjectInspector : Inspector {
    public override void Initialize() {
        var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;
        base.Initialize();
        //TODO: support attributes on getters and setters
        Members = TargetType.GetMembers(flags).Where(info => {
            if (Attribute.IsDefined(info, typeof(HideAttribute))) return false;
            var isPublic = info.MemberType switch
            {
                MemberTypes.Field => ((FieldInfo)info).IsPublic,
                MemberTypes.Property => ((PropertyInfo)info).GetAccessors().Any(methodInfo => methodInfo.IsPublic),
                MemberTypes.Event => ((EventInfo) info).GetAddMethod().IsPublic, //TODO: fixme
                MemberTypes.Method => ((MethodInfo)info).IsPublic,
                _ => false
            };
            return (isPublic || Attribute.IsDefined(info, typeof(ShowAttribute)))
                   && !Attribute.IsDefined(info, typeof(HideAttribute));
        }).ToList();
    }

    public List<MemberInfo> Members;

    public virtual void RenderMember(MemberInfo memberInfo) {
        //ImGui.Text(memberInfo.Name);
        switch (memberInfo.MemberType) {
            case MemberTypes.Event: RenderEvent((EventInfo) memberInfo); break;
            case MemberTypes.Property: RenderProperty((PropertyInfo) memberInfo); break;
            case MemberTypes.Field: RenderField((FieldInfo) memberInfo); break;
            case MemberTypes.Method: RenderMethod((MethodInfo) memberInfo); break;
            default: ImGui.Text("Error: Unsupported Member type"); break;
        }
    }

    public virtual void RenderEvent(EventInfo eventInfo) {
        ImGui.Text($"event {eventInfo.EventHandlerType?.FullName} {eventInfo.Name}");
    }
    
    public virtual void RenderProperty(PropertyInfo property) {
        if (Drawer.TryGet(property.PropertyType, out var value))
            value.DrawGui(property, Target);
    }

    public virtual void RenderField(FieldInfo field) {
        if (Drawer.TryGet(field.FieldType, out var value))
            value.DrawGui(field, Target);
    }

    public virtual void RenderMethod(MethodInfo methodInfo) {
        //ImGui.Text("hi");
        if (methodInfo.IsSpecialName) return;
        //ImGui.TextDisabled(MethodInfo.ToString());
    }

    public override void DrawGui() {
        foreach (var member in Members) {
            var ro = member.GetCustomAttribute(typeof(ReadOnlyAttribute)) is not null;
            if (ro) ImGui.BeginDisabled();
            RenderMember(member);
            if (ro) ImGui.End();
        }
    }
}