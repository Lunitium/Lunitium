using Lunitium.Mapper.Attributes;
using Lunitium.Mapper.Enums;
using Lunitium.Mapper.Test.Dto;

namespace Lunitium.Mapper.Test.Models;

[Mapping<UserRecordDto>]
[Mapping<UserClassDto>(Action = MapAction.To | MapAction.From)]
public partial class UserModel
{
    public uint Id { get; set; }
    public required string? Name { get; set; }
}