using Lunitium.Mapper.Attributes;
using Lunitium.Mapper.Enums;
using Lunitium.Mapper.Test.Dto;

namespace Lunitium.Mapper.Test.Models;

[Mapping<UserRecordDto>(Direction = MapDirection.To)]
[Mapping<UserClassDto>(Direction = MapDirection.To)]
public partial class UserModel
{
    public uint Id { get; set; }
    public required string? Name { get; set; }
}