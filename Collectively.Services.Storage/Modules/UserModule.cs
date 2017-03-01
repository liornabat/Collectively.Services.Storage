﻿using Collectively.Services.Storage.Providers.Users;
using Collectively.Services.Storage.Queries;


namespace Collectively.Services.Storage.Modules
{
    public class UserModule : ModuleBase
    {
        public UserModule(IUserProvider userProvider) : base("users")
        {
            Get("", async args => await FetchCollection<BrowseUsers, UserDto>
                (async x => await userProvider.BrowseAsync(x)).HandleAsync());

            Get("{id}", async args => await Fetch<GetUser, UserDto>
                (async x => await userProvider.GetAsync(x.Id)).HandleAsync());

            Get("{name}/account", async args => await Fetch<GetUserByName, UserDto>
                (async x => await userProvider.GetByNameAsync(x.Name)).HandleAsync());

            Get("{name}/available", async args => await Fetch<GetNameAvailability, AvailableResourceDto>
                (async x => await userProvider.IsAvailableAsync(x.Name)).HandleAsync());
        }
    }
}