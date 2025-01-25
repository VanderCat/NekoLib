using JetBrains.Annotations;

namespace NekoLib.Tools;

[MeansImplicitUse]
public class CustomDrawerAttribute(Type drawerType) : Attribute {
    public Type DrawerType = drawerType;
}