# Nebula Injecting

*Fundamental library of [Pygmalions](https://github.com/Pygmalions)' [Nebula Framework](https://github.com/Pygmalions/Nebula.Framework).*

This library provides an implementation of IoC mechanism. 
It supports intrusive injection (attribute), and non-intrusive injection (preset).

## Concepts

### Preset

Preset is a data class, which describes how to inject an object instance.
You can preset the values of fields, properties, and method invocations in presets,
then they will be injected into the instance when the object instance is passed to the **Inject** method 
or be acquired by a container.

### Definition

A definition describes how to acquire and inject an object.
It contains a **Preset** to describe the injection information.

### Injection Styles

- **Preset Injection**: it is the **non-intrusive** style which uses a preset to instruct the container to inject an instance.
- **Passive Injection**: it is the **intrusive** style by marking *[Injection]* attribute on members.

### Object Name

When the required category and optional name is passed to the container, the null name will be considered as "" (empty string).
Then the container will firstly perform the accurate matching;
if it fails, then the container will try to find the definition with the name "*", and use it to acquire an instance.

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

### Use a preset

This code will set the SampleFiled to 1, SampleProperty to 2, and invoke the SampleMethod with a parameter 3.

```c#
var injector = new Preset()
    .SetField("SampleField", Bind.Value(1))
    .SetProperty("SampleProperty", Bind.Value(2))
    .InvokeMethod("SampleMethod", Bind.Array(3));
var instance = new SampleObject();
injector.Inject(instance);
```

### Use a container and a preset

It is quite similar to use a preset directly:

```c#
var container = new Container();
container.Declare<SampleObject>()
    .AsPreset()
    .SetField("SampleField", Bind.Value(1))
    .SetProperty("SampleProperty", Bind.Value(2));
var instance = container.Get<SampleObject>();
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

First, you have declare the objects which is marked as injected in the PassiveSampleObject,
then you can declare and get an PassiveSampleObject:

```c#
var container = new Container();
container.DeclareValue<int>(1, "SampleField");
container.DeclareValue<int>(2, "SampleProperty");
container.DeclareValue<int>(3, "SampleMethod");
container.DeclareValue<int>(4, "SampleConstructor");
container.Declare<PassiveSampleObject>();
var instance = container.Get<PassiveSampleObject>();
```

## Remarks

### Attention

To preset a language built-in value (such as int, bool, float, etc), you **must**
use **BindBuilder** on the declaration for these types don't have any constructors, 
or use **DeclareValue** (or **DeclareArray**, etc) to quickly declare and bind a binder to the declaration.

### Unstable API

**NOT** compatible with versions older than 0.2.0. Concepts are simplified compared than the previous versions.

This library is under rapid development, thus its API may be very unstable, 
**DO NOT** use it in the production environment,
until its version reaches 1.0.0.