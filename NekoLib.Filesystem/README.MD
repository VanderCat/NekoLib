# NekoLib's Filesystem

uhh idk it pretty much speaks for itself, allows you to mount content from folders,
or any archive, but that you need to implement yourself, there is however an assembly
embedded file stuff implemented so you can use that idk

implement `IFilesystem` and `IFile`, then use `Files.Mount(new YourClass())`
now Files is your best friend, please block and report default `File` type.

default implementation does not assume any sandboxing here be dragons