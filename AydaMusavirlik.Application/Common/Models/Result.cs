namespace AydaMusavirlik.Application.Common.Models;

/// <summary>
/// Generic Result wrapper
/// </summary>
public class Result
{
    public bool Succeeded { get; init; }
    public string[] Errors { get; init; } = Array.Empty<string>();

    protected Result(bool succeeded, IEnumerable<string> errors)
    {
        Succeeded = succeeded;
        Errors = errors.ToArray();
    }

    public static Result Success() => new(true, Array.Empty<string>());

    public static Result Failure(IEnumerable<string> errors) => new(false, errors);

    public static Result Failure(string error) => new(false, new[] { error });
}

/// <summary>
/// Generic Result with data
/// </summary>
public class Result<T> : Result
{
    public T? Data { get; init; }

    protected Result(bool succeeded, T? data, IEnumerable<string> errors) : base(succeeded, errors)
    {
        Data = data;
    }

    public static Result<T> Success(T data) => new(true, data, Array.Empty<string>());

    public static new Result<T> Failure(IEnumerable<string> errors) => new(false, default, errors);

    public static new Result<T> Failure(string error) => new(false, default, new[] { error });
}

/// <summary>
/// Sayfalama sonucu
/// </summary>
public class PaginatedList<T>
{
    public List<T> Items { get; }
    public int PageNumber { get; }
    public int TotalPages { get; }
    public int TotalCount { get; }

    public PaginatedList(List<T> items, int count, int pageNumber, int pageSize)
    {
        PageNumber = pageNumber;
        TotalPages = (int)Math.Ceiling(count / (double)pageSize);
        TotalCount = count;
        Items = items;
    }

    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;

    public static PaginatedList<T> Create(IQueryable<T> source, int pageNumber, int pageSize)
    {
        var count = source.Count();
        var items = source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
        return new PaginatedList<T>(items, count, pageNumber, pageSize);
    }
}