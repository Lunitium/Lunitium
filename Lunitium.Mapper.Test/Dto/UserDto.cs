namespace Lunitium.Mapper.Test.Dto;

public record UserDto(int Id, string Name, string Email = "")
{
    public DateTimeOffset CreatedAt { get; set; }
}