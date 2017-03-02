﻿using Collectively.Services.Storage.Models.Users;
using Collectively.Services.Storage.Providers.Users;
using Collectively.Services.Storage.Queries;

namespace Collectively.Services.Storage.Modules
{
    public class UserSessionModule : ModuleBase
    {
        public UserSessionModule(IUserProvider userProvider) : base("user-sessions")
        {
            Get("{id}", async args => await Fetch<GetUserSession, UserSession>
                (async x => await userProvider.GetSessionAsync(x.Id)).HandleAsync());
        }
    }
}