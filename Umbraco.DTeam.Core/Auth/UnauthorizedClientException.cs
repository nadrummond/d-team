using System;

namespace Umbraco.DTeam.Core.Auth
{
    public class UnauthorizedClientException : Exception
    {
        public UnauthorizedClientException(string message) 
            : base(message) { }
    }
}
