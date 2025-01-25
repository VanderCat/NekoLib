namespace NekoLib.Tools; 

public class ToolsIconAttribute : Attribute {
    public string Icon;
    public ToolsIconAttribute(string iconEndpoint) {
        Icon = iconEndpoint;
    }
}