using Lunitium.Mapper.Attributes;
using Lunitium.Mapper.Test.Dto;

namespace Lunitium.Mapper.Test;

[Mapping<UserDto>]
public partial class UserModel
{
    public uint Id { get; set; }
    public required string Name { get; set; }
}