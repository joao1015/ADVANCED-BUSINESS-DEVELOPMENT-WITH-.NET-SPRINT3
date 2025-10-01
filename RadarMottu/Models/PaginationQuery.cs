namespace RadarMottuAPI.Models;

public record PaginationQuery(int Page = 1, int PageSize = 10)
{
    public int Skip => (Page < 1 ? 0 : (Page - 1) * PageSize);
}
