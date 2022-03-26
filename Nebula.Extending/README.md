# Nebula Extending

*Fundamental library of [Pygmalions](https://github.com/Pygmalions)' [Nebula Framework](https://github.com/Pygmalions/Nebula.Framework).*

This library provides the assemblies filter mechanism which are mostly used for
in-assembly auto discovery functions.

Checking all types in all loaded assemblies is very time-consuming.
Especially there are many assemblies provided by the language or the third parties which don't
have any possibility to contain types which should be auto discovered.

By using this library, the auto discovery functions can ignore most assemblies which are not used as **plugins**,
which means that they don't have types to discover.


## Concepts

### Plugin Assembly

A plugin, or a plugin assembly, is the plugin which contains types that should enable in-assembly auto discovery on them.

By mark an assembly with the attribute **PluginAssembly**, it will be auto scanned by **PluginRegister.Scanner**,
and thus it will not be ignored by auto discovery functions which are using Nebula.Extending to filter assemblies.

### Transited Plugins

An assembly may also use other assemblies as plugins. 
If it is used as a plugin by another assembly, then those plugin assemblies will also be used.

### Conditional Plugins

By setting the properties **ApplyOnReleaseMode** and **ApplyOnDebugMode**,
the plugin assemblies can only be used in debug mode, or relase mode.

## How to Use

### Make An Assembly Discoverable

All libraries in Nebula.Framework use this library to filter assemblies.
To auto discovery will always enable on the entrance assembly (which contains the program entrance).
Otherwise, mark the assembly with the **PluginAssembly** attribute in any *.cs* file outsides the namespace.

```c#
[assembly: PluginAssembly]
```

### Filter Assemblies

The **PluginRegistry.Scanner** provides a lazy global singleton instance to use. 
It will scan plugin assemblies in loaded assemblies when its instance is firstly visited.
Then the result will be cached, but can be manually updated by **Update()** method.

```c#
foreach (var (assemblyName, assembly) in PluginRegistry.Scanner.Plugins)
{
    foreach (var type in assembly.GetTypes)
    {
        // Do type discovery here.
    }
}
```

## Remarks

This library is under rapid development, thus its API may be very unstable, 
**DO NOT** use it in the production environment,
until its version reaches 1.0.0.