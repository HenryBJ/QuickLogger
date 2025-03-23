namespace QuickLogger.Domain.Dto;

public class AppEdit
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public bool? Active { get; set; }
    public bool? RegisterInfo { get; set; }
    public bool? RegisterWarning { get; set; }
    public bool? RegisterError { get; set; }
    public bool? RegisterCritical { get; set; }
}
