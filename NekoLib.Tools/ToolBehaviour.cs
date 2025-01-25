using Microsoft.Extensions.Logging;
using NekoLib.Core;
using NekoLib.Extra;

namespace NekoLib.Tools;

public class ToolBehaviour : Behaviour {
    protected readonly ILogger Log;

    public ToolBehaviour() {
        Log = Logging.GetFor(GetType().Name);
    }
}