# PassOn
  

**PassOn** is a mapper generator, that aims to simplify the mapping between objects using convention over configuration. The generated mappers use simple assignments and are emitted in IL and stored in a thread-safe cache.

## Examples
### Mapping without configuration, just matching
The mapper will try to assign the values from a `source` to a `target` base on: the property **name** and the **type** (it needs to be assignable). Anything else will be ignored.

<table>
<tr>
<th> Source </th><th> Target </th>
</tr>
<tr>
<td>
   <pre lang="csharp">
class Source
{
  public Guid Id { get; set; }
  public string? Text { get; set; }
  public datetime? Date { get; set; }
}
   </pre>
</td>
<td>
<pre lang="csharp">
class Target
{
  public Guid Id { get; set; }
  public string? Text { get; set; }
  public string? Message { get; set; }
}
</pre>
</td>
</tr>
</table>

You can call the mapper like so:
```csharp
var target = Pass.On<Source, Target>(src);
```
Or using the extensions
```csharp
var target = src.Map<Source, Target>();
```
### Mapping with Attribute aliasing
The mapper will try to assign the values from a `source` to a `target` based on the `Aliases` decorating the property (*if the property has one or more aliases, they overrule the name in the matching*). Anything not decorated will behave like the *No configuration, just matching* approach.

<table>
<tr>
<th> Source </th><th> Target </th>
</tr>
<tr>
<td>
   <pre lang="csharp">
class Source
{ 
    [MapStrategy("Id", "Identifier")]
    public Guid Oid { get; set; }
    public string? Text { get; set; }
    public string? Message { get; set; }
}
   </pre>
</td>
<td>
<pre lang="csharp">
class Target
{
    public Guid Id { get; set; }  
    [MapStrategy("Message")]
    public string? Text { get; set; }
    [MapStrategy("Text")]
    public string? Message { get; set; }
}
</pre>
</td>
</tr>
</table>

You can call the mapper like so:
```csharp
var target = Pass.On<Source, Target>(src);
```
Or using the extensions
```csharp
var target = src.Map<Source, Target>();
```

### Mapping using CustomMap Strategy on the source
The mapper will try to assign the values from a `source` to a `target`. The properties decorated with that strategy will have their mapping based on the mapping function in the body. The name is given by the "map" + *property's name* or the value of `mapper` property on the attribute. This function is a sort of a **`getter`**, meaning it takes no arguments and the return needs to be assignable to the *target* property.
<table>
<tr>
<th> Source </th><th> Target </th>
</tr>
<tr>
<td>
<pre lang="csharp">
class  Source
{ 
    [MapStrategy(Strategy.CustomMap)]
    public  Guid  Id { get; set; }   
    public string? Text { get; set; }
    public string MapId()
    {
        return  $"{Id.ToString()} + !!";
    }
} 
</pre>
</td>
<td>
<pre lang="csharp">
class Target
{
  public string Id { get; set; }
  public string? Text { get; set; }
}
</pre>
</td>
</tr>
</table>

You can call the mapper like so:
```csharp
var target = Pass.On<Source, Target>(src);
```
Or using the extensions
```csharp
var target = src.Map<Source, Target>();
```


### Mapping with using CustomMap Strategy on the target
The mapper will try to assign the values from a `source` to a `target`. The properties decorated with that strategy will have their mapping based on the mapping function in the body. The name is given by the "map" + *property's name* or the value of `mapper` property on the attribute. This function is a **`setter`** meaning it doesn't return any values and has only one parameter that needs to be assignable to the source property.
<table>
<tr>
<th> Source </th><th> Target </th>
</tr>
<tr>
<td>
<pre lang="csharp">
class  Source
{
    public  Guid  Id { get; set; }        
    public string? Text { get; set; }
} 
</pre>
</td>
<td>
<pre lang="csharp">
class Target
{
    [MapStrategy(Strategy.CustomMap, "Id", Mapper = "ReceiveId")]
    public Guid Oid { get; set; }
    public string? Text { get; set; }
    public void ReceiveId(Guid id)
    {
        this.Oid = $"{Id.ToString()} + !!";
    }
}
</pre>
</td>
</tr>
</table>

You can call the mapper like so:
```csharp
var target = Pass.On<Source, Target>(src);
```
Or using the extensions
```csharp
var target = src.Map<Source, Target>();
```

## About the framework

## The MapStrategyAttribute

When decorating a property, it will change the behaviour of the mapper for that property.
When decorating with it you can:
```csharp
// Pass just the strategy
[MapStrategy(Strategy.Shallow)]
public ComplexObject SomeProperty { get; set; }
// Pass just the strategy and the aliases
[MapStrategy(Strategy.CustomMap, "alias_1", "alias_2")]
public ComplexObject SomeOtherProperty { get; set; }
// Pass just the aliases
[MapStrategy("alias_1", "alias_2", "alias_3")]
public ComplexObject AnotherProperty { get; set; }
// or specify all of it 
[MapStrategy(Type = Strategy.Deep, Aliases = new[] { "alias_1", "alias_2" })]
public ComplexObject YetAnotherProperty { get; set; }
[MapStrategy(Type = Strategy.CustomMap, Alias = "alias_1", Mapper = "MapFunc")]
public ComplexObject AndAnotherProperty { get; set; }
```

| Option | Type | Description |
| -------------- | -------------- | ----------- |
| Type | `PassOn.Strategy` (enum) | Sets the behavior of that mapping. More [here](#map-Strategies)|
| Aliases | `System.String[]` | Can be used on the source and/or the destination. The property name is not used in the match if there is at least one alias.|
| Alias | `System.String` | Same as `Aliases` but only one|
|Mapper| `System.String` | Is the name of the mapping function for this property. If not specified, the name is `"Map" + PropertyName`|


## Map Strategies
They set the behavior when generating the mapper. The default behavior is `Strategy.Deep`.

### Strategy.Shallow
As the name implies, it will be a simple value copy or a reference passing when possible.    
This is similar to:
```csharp
// age is an int
target.age = source.age; 
target.ComplexObject = source.ComplexObject; 
```
### Strategy.Deep
All properties that have a reference value will call the `On<T>()` function. Their values will be mapped following the same rules and that function will be added to the cache.    
This is similar to:
```csharp
// age is an int
target.age = source.age; 
target.ComplexObject = Pass.On(source.ComplexObject); 
```
### Strategy.Ignore
The property wont be added to the mapper.

### Strategy.CustomMap
This strategy has two different applications.    
_When decorating a **source**'s property_, it will be used as a getter function, instead of the actual property getter.    
This function's signature needs to be similar to `System.Func<T>`, meaning: 

- is public,
- not static,
- returns an type that is assignable to the target's property,
- has no arguments

This is similar to:
```csharp
// age is an int
target.age = source.MapAge(); 
target.ComplexObject = source.MapComplexObject(); 
```
_When decorating a **target**'s property_, it will be used as a setter function, instead of the actual operator `=`.    
This function's signature needs to be similar to `System.Action<T>`, meaning: 

- is public
- not static
- has no return (`void`),
- has _only_ one argument, and that argument is assignable to the target's property, or is of type `object`.    
This is similar to:
```csharp
target.MapAge(source.age); 
target.MapComplexObject(source.ComplexObject); 
```
## The life-cycle Interceptors
Each mapper has lifecycle which you can monitor and manipulate. 
In order to intercept them, your function needs to obey some constraints:
- Be decorated with the `BeforeMappingAttribute` or `AfterMappingAttribute`, the name is not important,
- Have a maximum of 2 arguments,
- Can return `void` or the **target**'s `Type`, or something that can be cast to the target's type,
- if has two arguments, one needs to assignable to the source and the other to the type,
- if the argument is an object, it needs to be decorated with `SourceAttribute` or `TargetAttribute`.
### Examples
Monitoring the map:
```csharp
[BeforeMapping]
public void Log(Source src) 
{
	Log.Debug($"Parsing {src.Id}");
}
```
Manipulating the map:
```csharp
[AfterMapping]
public void Manipulate(Source src, Target target) 
{	
	target.NotMappedProperty = DateTime.Now();
}
```
Throwing the merging away:
```csharp
[BeforeMapping]
public void Validate(Source src, Target target) 
{	
	if(src.Id != target.Id) {
		throw new IdsDontMatchException(src.Id, target.Id);
	}
}
```
Throwing the target away:
```csharp
[BeforeMapping]
public Target Validate(Source src, Target target) 
{	
	if(target.Id != Guid.Empty) {
		return new Target();
	}
	return target;
}
```
### Before Mapping
This happens before the mapping starts. You can think of it as:
If you create an `Action` like method, you can think of it as:
```csharp
[BeforeMapping]
public void Log(Source src) 
{
	// do something here
}
```
And it would generate:
```csharp
target.Log(source);

target.age = source.age;
target.ComplexObject = source.ComplexObject;

return target
```
Or  if you create an `Func` like method, you can think of it as:
```csharp
[BeforeMapping]
public object Modify(Source src, [Target] object tgt) 
{
	// do something here
	return tgt;
}
```
And it would generate:
```csharp
// this signature could be something like
target = (Target) source.Modify(source, target);

target.age = source.age;
target.ComplexObject = source.ComplexObject;

return target
```
### After Mapping
This happens after the mapping starts. 
If you create an `Action` like method, you can think of it as:
```csharp
[AfterMapping]
public void Log(Source src) 
{
	// do something here
}
```
And it would generate:
```csharp
target.age = source.age;
target.ComplexObject = source.ComplexObject;

target.Log(source);

return target
```
Or  if you create an `Func` like method, you can think of it as:
```csharp
[AfterMapping]
public object Modify(Source src, [Target] object tgt) 
{
	// do something here
	return tgt;
}
```
And it would generate:
```csharp
// this signature could be something like
target.age = source.age;
target.ComplexObject = source.ComplexObject;

target = (Target) source.Modify(source, target);

return target
```