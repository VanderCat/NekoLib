# NekoLib's Console Subsystem
Allows you to implement source/quake like console scripting,
configuration, etc

does not depend on NekoLib, so you can use it freely.

you can find Gui for this console in NekoLib.Tools

to create custom command use this
```csharp
public static class Commands {
    [ConCommand("console_name")] // a handle to run command
    [ConDescription("Description text to show in help")] //(optional)
    [ConTag("cheat")] //(optional) use this do mark a command cheat so it could not be run without sv_cheats true
    public static void SampleCommand(string arg1, int arg2) {} // it also supports optional args and params
    
    [ConVariable("sample_convar")]
    public static int SampleConvar {get; set;} // must be convertible (IConverible) to string if using set accessor; most default types are
}
```

it will be automatically registered as you run `Console.Init()`. Also it will
automatically reregister all commands/convars on hotreload

use `Console.Submit()` to run commandline buffer.

worth to note it will also run command in trycatch block and print an error, so it will not crash.