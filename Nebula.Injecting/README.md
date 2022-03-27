# Nebula Injecting

*Fundamental library of [Pygmalions](https://github.com/Pygmalions)' [Nebula Framework](https://github.com/Pygmalions/Nebula.Framework).*

This library provides an implementation of IoC mechanism. 
It supports intrusive injection (attribute), and non-intrusive injection (preset).
The **Source** mechanism allows you to merge presets defined in data sources (SQL, XML, JSON, etc)
into the containers.

## Concepts

### Preset

Preset is a data class, which describes how to inject an object instance.
You can preset the values of fields, properties, and method invocations in presets,
then they will be injected into the instance when the object instance is acquired
by a builder or a container.

### Builder

Builder is a data class which describes how to create an object instance.
Its like a **Func<object?>**, but the procedure of binding constructor arguments
and binding class can be separately customized.

### Container

Container is a set of presets and sources. 
A preset can describe an object with an optional name, which can be acquired by users
using **Get** method, or be used to inject another object.

### Source

Compared to builder, source is a more complex instance creator,
which following a customized rule to generate instance.
You need to inherit and override **OnInstall**, **OnUninstall**, and **Get**
to make your customized source.

Source has to declare a resource before anyone want to get it from the container.
A declaration instructs the container to find and use the specific source which provides
this object.

When source is uninstalling from the container, all its declarations will
be revoked.

It is used to introduce outsides data source into the container. 
For example, you can make a source which reads XML files,
and declares objects which are described in the XML files in the container.
When your source is invoked for getting those objects,
it can generate instances according to the construction and injection
information in the XML files.

You can declare an object at **any time**. 
It is common but not must to make declarations during the installing.

### Name of Objects

All objects in the container have an optional name.
When trying to get an object, you can also specific the name of the object to get.

There are two specific names: "" (Empty), and "*".

No matter what the specific name is, any requirement will firstly match the preset
entries which has complete the same name, such "" -> "", * -> *, "aName" -> "aName".

When the container can not find a entry with the same name,
then, the container will try to find the entry with the name "*",
and use it; if it fails, then it will return a null, unless the name of the requirement is "",
which will let the container to use any existing entry in the same type category.

So, declare or preset an object with the name "*", if you want to make it a generator;
require an object with the name "", if you only care about the type and don't care about which object to use;

And, declare or preset an object with the name "", if you want to make it the default instance
for requirement which has the name "".

### Injection Styles

- **Preset Injection**: it is the **non-intrusive** style which uses a preset to instruct the container to inject an instance.
- **Passive Injection**: it is the **intrusive** style by marking *[Injection]* attribute on members.

## How to use

### Sample Object for Injection

This is the object class which we will use to show how to use this library:

```c#
public class SampleObject
{
    public int SampleField;
    
    public int SampleProperty { get; set; }

    public int MethodValue;
    
    public void SampleMethod(int value)
    {
        MethodValue = value;
    }

    public int ConstructorValue;
    
    public SampleObject()
    {
        SampleField = -1;
        SampleProperty = -1;
        MethodValue = -1;
        ConstructorValue = -1;
    }
    
    public SampleObject(int value)
    {
        SampleField = -1;
        SampleProperty = -1;
        MethodValue = -1;
        ConstructorValue = value;
    }
}
```

### Use a preset and a builder separately

This code will set the SampleFiled to 1, SampleProperty to 2, and invoke the SampleMethod with a parameter 3.

```c#
var instance = new Preset(typeof(SampleObject))
            .PresetField("SampleField", () => 1)
            .PresetProperty("SampleProperty", () => 2)
            .InvokeMethod("SampleMethod", () => new object?[] { 3 })
            .SetBuilder()
            .AsBuilder()?.Build() as SampleObject;
```

Invoke the **BindArguments** method of a builder will enable the constructor injection:

```c#
var instance = new Preset(typeof(SampleObject))
            .PresetField("SampleField", () => 1)
            .PresetProperty("SampleProperty", () => 2)
            .InvokeMethod("SampleMethod", () => new object?[] { 3 })
            .SetBuilder()
            .AsBuilder()?
            .BindArguments(() => new object?[]{4})
            .Build() as SampleObject;
```

### Use a container in preset injection

It is quite similar to use a preset directly:

```c#
var container = new Container();
var instance = container.Preset<SampleObject>()
            .PresetField("SampleField", () => 1)
            .PresetProperty("SampleProperty", () => 2)
            .InvokeMethod("SampleMethod", () => new object?[] { 3 })
            .SetBuilder()
            .AsBuilder()?
            .BindArguments(() => new object?[] {4})
            .Build() as SampleObject;
```

Or, you can get it from the container, rather than directly invoke the builder.
But actually it the same to invoking the builder.

```c#
var container = new Container();
container.Preset<SampleObject>()
    .PresetField("SampleField", () => 1)
    .PresetProperty("SampleProperty", () => 2)
    .InvokeMethod("SampleMethod", () => new object?[] { 3 })
    .SetBuilder()
    .AsBuilder()?
    .BindArguments(() => new object?[] {4});
var instance = container.Get<SampleObject>() as SampleObject;
```

### Sample Object for Passive Injection

Compared to SampleObject, this class has *[Injection]* on members.

```c#
public class PassiveSampleObject
{
    [Injection("SampleField")]
    public int SampleField;
    
    [Injection("SampleProperty")]
    public int SampleProperty { get; set; }

    public int MethodValue;
    
    [Injection]
    public void SampleMethod([Injection("SampleMethod")] int value)
    {
        MethodValue = value;
    }

    public int ConstructorValue;
    
    public PassiveSampleObject()
    {
        SampleField = -1;
        SampleProperty = -1;
        MethodValue = -1;
        ConstructorValue = -1;
    }
    
    [Injection]
    public PassiveSampleObject([Injection("SampleConstructor")] int value)
    {
        SampleField = -1;
        SampleProperty = -1;
        MethodValue = -1;
        ConstructorValue = value;
    }
}
```

### Use a container for passive injection

Invoke the builder:

```c#
var container = new Container();
container.Preset<int>("SampleField").SetBuilder().AsBuilder()?
    .BindInstance(1);
container.Preset<int>("SampleProperty").SetBuilder().AsBuilder()?
    .BindInstance(2);
container.Preset<int>("SampleMethod").SetBuilder().AsBuilder()?
    .BindInstance(3);
container.Preset<int>("SampleConstructor").SetBuilder().AsBuilder()?
    .BindInstance(4);
var instance = container.Preset<PassiveSampleObject>()
    .SetBuilder()
    .AsBuilder()?
    .Build() as PassiveSampleObject;
```

or use the **Get**:

```c#
var container = new Container();
container.Preset<int>("SampleField").SetBuilder().AsBuilder()?
    .BindInstance(1);
container.Preset<int>("SampleProperty").SetBuilder().AsBuilder()?
    .BindInstance(2);
container.Preset<int>("SampleMethod").SetBuilder().AsBuilder()?
    .BindInstance(3);
container.Preset<int>("SampleConstructor").SetBuilder().AsBuilder()?
    .BindInstance(4);
container.Preset<PassiveSampleObject>().SetBuilder();
var instance = container.Get<PassiveSampleObject>() as PassiveSampleObject;
```

## Remarks

### Attention

There are two things you should be aware of:
1. Container is **necessary** in passive injection.
2. To preset a language built-in value (such as int, bool, float, etc), you **must**
   use BindInstance for these types don't have any constructors.

### Unstable API

This library is under rapid development, thus its API may be very unstable, 
**DO NOT** use it in the production environment,
until its version reaches 1.0.0.