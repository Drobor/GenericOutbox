using System.Text;
using GenericOutbox.DataAccess.Entities;
using GenericOutbox.ManagementUi.Models;

namespace GenericOutbox.ManagementUi;

internal static class FilterExtensions
{
    public static IQueryable<OutboxEntity> ApplyFilter(this IQueryable<OutboxEntity> query, OutboxEntityFilter filter)
    {
        if (filter?.Ids?.Any() == true)
            query = query.Where(x => filter.Ids.Contains(x.Id));

        if (filter?.Status.HasValue ?? false)
            query = query.Where(x => x.Status == filter.Status);

        if (filter?.ActionContains?.Any() == true)
            query = query.Where(x => x.Action.Contains(filter.ActionContains));

        if (filter?.Skip.HasValue ?? false)
            query = query.Skip(filter.Skip.Value);

        if (filter?.Take.HasValue ?? false)
            query = query.Take(filter.Take.Value);

        return query;
    }
}