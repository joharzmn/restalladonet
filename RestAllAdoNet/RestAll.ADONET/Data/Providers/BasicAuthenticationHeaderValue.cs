// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
// Modified for Intuit's Oauth2 implementation

using System.Net.Http.Headers;
using System.Text;

namespace RESTAll.Data.Providers
{
    /// <summary>
    /// Formatter for Basic Authentication header
    /// </summary>
    public class BasicAuthenticationHeaderValue : AuthenticationHeaderValue
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="username">Username</param>
        /// <param name="password">Password</param>
        public BasicAuthenticationHeaderValue(string username, string password)
            : base("Basic", EncodeCredential(username, password))
        { }

        /// <summary>
        /// Encode Credential
        /// </summary>
        /// <param name="username">Username</param>
        /// <param name="password">Password</param>
        private static string EncodeCredential(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username)) throw new ArgumentNullException(nameof(username));
            if (string.IsNullOrWhiteSpace(password)) throw new ArgumentNullException(nameof(password));

            Encoding encoding = Encoding.ASCII;
            string credential = $"{username}:{password}";

            return Convert.ToBase64String(encoding.GetBytes(credential));
        }
    }
}