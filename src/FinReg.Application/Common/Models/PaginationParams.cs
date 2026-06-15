namespace FinReg.Application.Common.Models;

public sealed record PaginationParams
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}
