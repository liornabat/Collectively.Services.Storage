﻿using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Collectively.Common.Extensions;
using Collectively.Common.Mongo;
using Collectively.Services.Storage.ServiceClients.Queries;
using Collectively.Common.Types;
using Collectively.Services.Storage.Models.Remarks;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Collectively.Services.Storage.Repositories.Queries
{
    public static class RemarkQueries
    {
        private static readonly int NegativeVotesThreshold = -2;

        public static IMongoCollection<Remark> Remarks(this IMongoDatabase database)
            => database.GetCollection<Remark>();

        public static async Task<Remark> GetByIdAsync(this IMongoCollection<Remark> remarks, Guid id)
        {
            if (id == Guid.Empty)
                return null;

            return await remarks.AsQueryable().FirstOrDefaultAsync(x => x.Id == id);
        }

        public static async Task<PagedResult<Remark>> QueryAsync(this IMongoCollection<Remark> remarks,
            BrowseRemarks query)
        {
            if (!IsLocationProvided(query) && query.AuthorId.Empty() && !query.Latest)
                return PagedResult<Remark>.Empty;

            if (query.Page <= 0)
            {
                query.Page = 1;
            }
            if (query.Results <= 0)
            {
                query.Results = 10;
            }

            var filterBuilder = new FilterDefinitionBuilder<Remark>();
            var filter = FilterDefinition<Remark>.Empty;
            if (IsLocationProvided(query))
            {
                var maxDistance = query.Radius > 0 ? (double?) query.Radius/1000/6378.1 : null;
                filter = filterBuilder.NearSphere(x => x.Location,
                        query.Longitude, query.Latitude, maxDistance);
            }
            if (query.Latest)
            {
                filter = filterBuilder.Where(x => x.Id != Guid.Empty);
            }
            if (query.AuthorId.NotEmpty())
            {
                filter = filter & filterBuilder.Where(x => x.Author.UserId == query.AuthorId);
                
                if (query.OnlyLiked)
                {
                    filter = filter & filterBuilder.Where(x => x.Votes.Any(v => v.UserId == query.AuthorId && v.Positive));
                }
                else if (query.OnlyDisliked)
                {
                    filter = filter & filterBuilder.Where(x => x.Votes.Any(v => v.UserId == query.AuthorId && !v.Positive));
                }
            }
            if (query.ResolverId.NotEmpty())
            {
                filter = filter & filterBuilder.Where(x => x.State.State == "resolved" && x.State.User.UserId == query.ResolverId);
            }
            if (!query.Description.Empty())
            {
                filter = filter & filterBuilder.Where(x => x.Description.Contains(query.Description));
            }
            if (query.Categories?.Any() == true)
            {
                filter = filter & filterBuilder.Where(x => query.Categories.Contains(x.Category.Name));
            }
            if (query.Tags?.Any() == true)
            {
                filter = filter & filterBuilder.Where(x => x.Tags.Any(y => query.Tags.Contains(y)));
            }
            if (query.State.NotEmpty() && query.State != "all")
            {
                if (query.State == "resolved")
                {
                    filter = filter & filterBuilder.Where(x => x.State.State == "resolved");
                }
                else
                {
                    filter = filter & filterBuilder.Where(x => x.State.State != "resolved");
                }
            }
            if (!query.Disliked)
            {
                filter = filter & filterBuilder.Where(x => x.Rating > NegativeVotesThreshold);
            }

            var filteredRemarks = remarks.Find(filter);
            var totalCount = await filteredRemarks.CountAsync();
            var totalPages = (int) totalCount / query.Results + 1;
            var findResult = filteredRemarks
                .Skip(query.Results * (query.Page - 1))
                .Limit(query.Results);

            findResult = SortRemarks(query, findResult);

            var result = await findResult.ToListAsync();
            return PagedResult<Remark>.Create(result, query.Page, query.Results, totalPages, totalCount);
        }

        private static IFindFluent<Remark,Remark>  SortRemarks(BrowseRemarks query, 
            IFindFluent<Remark,Remark> findResult)
        {
            if(query.OrderBy.Empty())
            {
                return findResult;
            }
            if(query.SortOrder.Empty())
            {
                query.SortOrder = "ascending";
            }

            query.OrderBy = query.OrderBy.ToLowerInvariant();
            query.SortOrder = query.SortOrder.ToLowerInvariant();

            switch(query.OrderBy)
            {
                case "userid": return SortRemarks(query, findResult, x => x.Author.UserId);
                case "createdat": return SortRemarks(query, findResult, x => x.CreatedAt);
            }

            return findResult;
        }

        private static IFindFluent<Remark,Remark>  SortRemarks(BrowseRemarks query,
            IFindFluent<Remark,Remark> findResult, Expression<Func<Remark, object>> sortBy)
        {
            switch(query.SortOrder)
            {
                case "ascending": return findResult.SortBy(sortBy);
                case "descending": return findResult.SortByDescending(sortBy);
            }

            return findResult;
        }      

        private static bool IsLocationProvided(BrowseRemarks query)
            => (Math.Abs(query.Latitude) <= 0.0000000001 
                || Math.Abs(query.Longitude) <= 0.0000000001) == false;
    }
}