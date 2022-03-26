# Nebula Translating

*Fundamental library of [Pygmalions](https://github.com/Pygmalions)' [Nebula Framework](https://github.com/Pygmalions/Nebula.Framework).*

This library provides a mechanism to allow converting objects between non-related types. 
This is designed NOT for customized type converting, 
but for customized non-intrusive serialization and deserialization.

## Concepts

### Translator

Translator is the object which can **construct** a new object of the required type according to the information of the given object
and the **customized** rule. And that is the difference between the translating and converting.

Compared to ISerializable, Translators are designed for serialization and deserialization in a **non-intrusive** style
with flexible **protocol** support.
For example, you can create a translator which can translate a string into your customized
data structure and transport it among the Internet.

### Protocol
 
There may be multiple rules to do the translating,
so register and translate methods have the optional parameter **protocol**.
If the protocol parameter is "" (empty string),
then the registry will firstly to find the translator which is register with "",
then secondly it will try to use a random translator to translate.

### Translator Registry

A translator registry is a translators set,
which allows you to customize which translators to use in different scenarios.

The static class **Translators** is a static facade of a singleton registry instance.
In most situations, you can directly register translators into it,
and use it to translate objects.

## How to Use

### Implement a translator

A simple translator which translate a integer into a string is as follows.
(Though it is meaningless, it is displayed to demonstrate the translator interface.)

```c#
[Translator]
public class SampleTranslator : ITranslator<int, string>
{
    public string Translate(int original)
    {
        return original.ToString();
    }
}
```

The **TranslatorAttribute** will let the static facade **Translators**
auto discover and register it.

### Register it

Register a translator in a registry:
```c#
var translators = new TranslatorRegistry();
translators.RegisterTranslator<int, string>(new SampleTranslator());
```

### Translate

Use translators in a registry to translate a integer:
```c#
var result = translators.Translate<string>(3);
```

## Remarks

This library is under rapid development, thus its API may be very unstable,
**DO NOT** use it in the production environment,
until its version reaches 1.0.0.