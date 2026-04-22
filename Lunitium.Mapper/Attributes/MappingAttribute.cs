using Lunitium.Mapper.Enums;

namespace Lunitium.Mapper.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class MappingAttribute<TType> : Attribute where TType : class
{
    public MapDirection Direction { get; set; } = MapDirection.All;
}