namespace QuickLogger.Domain.Dto;

public class Page
{
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public bool OrderDescending { get; set; } = true;
    public string? OrderByProperty { get; set; }
    public string Filter { get; set; }
}
