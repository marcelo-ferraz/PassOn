# PassOn

**PassOn** is set for no setup and out of the box usage.     
Simply stating, you can pass the values of `_oneInstance.To<AnotherType>()` or (`Pass.On<AnotherType>(_oneInstance)`), designed as a _Convention over configuration_ library, it matches the names and types. 
- Little or no configuration,
- Fast execution (not reflection or expression tree based value passing),
- Out of the box solution

##  Basic use (Simplest scenarios)
In the simplest scenario, where you have properties with the same name and assignable: 

### How to clone
```csharp
OneDTO dto = Pass.On<OneDTO>(instance);
```
Or using the extensions,
```csharp
OneDTO dto = instance.To<OneDTO>();
```

###  How to merge
```csharp
CompositeDto dto = Pass.On<CompositeDto>(instance1);
dto = Pass.Onto(dto, instance2);
```
Or using the extensions,
```csharp
var dto = new CompositeDto();
dto = instance1.To(dto);
dto = instance2.To(dto);
```
Nothing needs to be added to either source or destination types.

## Attributes or Decorators

With the `Clone` attribute, you can set a couple of specific behaviours to the passage:

| Option | Type | Description |
|---------|---------|---------|
| InspectionType | `PassOn.Inspection` (enum) | It changes on how the values will be passed, meaning: <ul><li>Shallow</li><li>Deep</li><li>Ignore</li></ul>
| Aliases | `System.String` | Any values added to this, either on source or destination will be taken into consideration when trying to matching the names. Always the first to match will be used.