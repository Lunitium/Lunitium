namespace Lunitium.Mapper.Enums;

[Flags]
public enum MapDirection : byte
{
    ToDto = 1 << 0,
    FromDto = 1 << 1,
    Modify = 1 << 2,
    All = ToDto | FromDto | Modify
}