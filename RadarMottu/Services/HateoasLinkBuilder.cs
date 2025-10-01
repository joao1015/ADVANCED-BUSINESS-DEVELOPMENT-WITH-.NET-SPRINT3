using RadarMottuAPI.Models;

namespace RadarMottuAPI.Services;

public class HateoasLinkBuilder
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HateoasLinkBuilder(IHttpContextAccessor? httpContextAccessor = null)
    {
        _httpContextAccessor = httpContextAccessor ?? new HttpContextAccessor();
    }

    public IEnumerable<Link> GetLinks(string resource, int page, int pageSize, int totalItems)
    {
        var links = new List<Link>();
        var baseUrl = $"{_httpContextAccessor.HttpContext?.Request.Scheme}://{_httpContextAccessor.HttpContext?.Request.Host}/api/{resource}";

        links.Add(new Link("self", $"{baseUrl}?page={page}&pageSize={pageSize}", "GET"));

        if (page > 1)
            links.Add(new Link("prev", $"{baseUrl}?page={page - 1}&pageSize={pageSize}", "GET"));

        if (page * pageSize < totalItems)
            links.Add(new Link("next", $"{baseUrl}?page={page + 1}&pageSize={pageSize}", "GET"));

        return links;
    }
}
