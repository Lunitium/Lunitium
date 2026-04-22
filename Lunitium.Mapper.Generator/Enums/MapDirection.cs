namespace Lunitium.Mapper.Generator.Enums;

[Flags]
public enum MapDirection : byte
{
    To = 1 << 0,
    From = 1 << 1,
    Modify = 1 << 2,
    All = To | From | Modify
}