﻿using System;
using System.Threading.Tasks;
using Coolector.Common.Types;
using Coolector.Dto.Remarks;
using Coolector.Services.Storage.Queries;
using Coolector.Services.Storage.Repositories;
using Coolector.Services.Storage.Settings;

namespace Coolector.Services.Storage.Providers
{
    public class RemarkProvider : IRemarkProvider
    {
        private readonly IRemarkRepository _remarkRepository;
        private readonly IRemarkCategoryRepository _remarkCategoryRepository;
        private readonly IProviderClient _providerClient;
        private readonly ProviderSettings _providerSettings;

        public RemarkProvider(IRemarkRepository remarkRepository,
            IRemarkCategoryRepository remarkCategoryRepository,
            IProviderClient providerClient,
            ProviderSettings providerSettings)
        {
            _remarkRepository = remarkRepository;
            _remarkCategoryRepository = remarkCategoryRepository;
            _providerClient = providerClient;
            _providerSettings = providerSettings;
        }

        public async Task<Maybe<RemarkDto>> GetAsync(Guid id)
            => await _providerClient.GetUsingStorageAsync(_providerSettings.RemarksApiUrl, $"remarks/{id}",
                async () => await _remarkRepository.GetByIdAsync(id),
                async remark => await _remarkRepository.AddAsync(remark));

        public async Task<Maybe<PagedResult<RemarkDto>>> BrowseAsync(BrowseRemarks query)
            => await _providerClient.GetCollectionUsingStorageAsync(_providerSettings.RemarksApiUrl, "remarks",
                async () => await _remarkRepository.BrowseAsync(query),
                async remarks => await _remarkRepository.AddManyAsync(remarks.Items));

        public async Task<Maybe<PagedResult<RemarkCategoryDto>>> BrowseCategoriesAsync(BrowseRemarkCategories query)
            => await _providerClient.GetCollectionUsingStorageAsync(_providerSettings.RemarksApiUrl,
                "remarks/categories",
                async () => await _remarkCategoryRepository.BrowseAsync(query),
                async remarks => await _remarkCategoryRepository.AddManyAsync(remarks.Items));
    }
}