namespace Lunitium.Mapper.Enums;

[Flags]
public enum MapAction : byte
{
    To = 1 << 0,
    From = 1 << 1,
    Modify = 1 << 2,
    All = To | From | Modify
}