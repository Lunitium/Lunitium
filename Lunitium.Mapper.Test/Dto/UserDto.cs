namespace Lunitium.Mapper.Test.Dto;

public record UserDto(uint Id, string Name, int Email = (int)System.DayOfWeek.Friday);