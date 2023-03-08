# PassOn
  

**PassOn** is a mapper generator, that aims to simplify the mapping between objects using convention over configuration. The generated mappers use simple assignments and are emitted in IL and stored in a thread-safe cache.

## Examples
### No configuration, just matching
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
### Attribute aliasing
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

### Using CustomMap Strategy on the source
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


### Using CustomMap Strategy on the target
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
