﻿
using Coolector.Common.Nancy;

namespace Coolector.Services.Storage.Modules
{
    public abstract class ModuleBase : ApiModuleBase
    {
        protected ModuleBase(string modulePath = "") : base(modulePath)
        {
        }
    }
}