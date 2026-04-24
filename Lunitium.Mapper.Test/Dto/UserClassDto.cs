using Lunitium.Mapper.Attributes;

namespace Lunitium.Mapper.Test.Dto;

public class UserClassDto
{
    [MapperConstructor]
    public UserClassDto(int id, string name, string email = "")
    {
        Id = id;
        Name = name;
        Email = email;
    }

    public UserClassDto()
    {
    }

    public int Id { get; }
    public string Name { get; }
    public string Email { get; }
}