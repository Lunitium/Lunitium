namespace Lunitium.Mapper.Test.Dto;

public class UserClassDto(int id, string name, string email = "")
{
    public int Id { get; } = id;
    public string Name { get; } = name;
    public string Email { get; } = email;
}