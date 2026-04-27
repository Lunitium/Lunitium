namespace Lunitium.Mapper.Test.Dto;

public record UserRecordDto(int Id, string? Name, string Email = "")
{
    public DateTimeOffset CreatedAt { get; init; }
}