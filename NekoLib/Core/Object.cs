namespace NekoLib.Core; 

public abstract class Object : IDisposable{
    public string Name = "";
    public Guid Id = Guid.NewGuid();

    public new string ToString() => Name;
    
    public void Dispose() {
        
    }
}