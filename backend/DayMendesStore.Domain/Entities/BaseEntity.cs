namespace DayMendesStore.Domain.Entities;

using DayMendesStore.Domain.Enums;

public abstract class BaseEntity
{
    public int Id { get; set; }
    public Status Status { get; set; } = Status.Ativo;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}