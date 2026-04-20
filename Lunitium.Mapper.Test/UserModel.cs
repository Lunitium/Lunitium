using Lunitium.Mapper.Attributes;
using Lunitium.Mapper.Enums;
using Lunitium.Mapper.Test.Dto;

namespace Lunitium.Mapper.Test;

[Mapping<UserDto>(Direction = MapDirection.ToDto)]
public partial class UserModel
{
    public uint Id { get; set; }
    public required string Name { get; set; }
}