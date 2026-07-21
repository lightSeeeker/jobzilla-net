using System;
using System.Collections.Generic;
using System.Text;

namespace jobzilla_net.Application.Constants
{
    public static class CommonText
    {
        public const string Local = "Local";
        public const string Development = "Development";
        public const string Staging = "Staging";
        public const string Production = "Production";

        public const string ASPNETCORE_ENVIRONMENT = "ASPNETCORE_ENVIRONMENT";
        public const string DOTNET_ENVIRONMENT = "DOTNET_ENVIRONMENT";

        /// <summary>
        /// Specifies the target environment name to use as a fallback if the 
        /// ASPNETCORE_ENVIRONMENT environment variable is not set.
        /// </summary>
        public const string CurrentEnv = Local;
    }
}
