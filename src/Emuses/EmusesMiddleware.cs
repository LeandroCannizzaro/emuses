﻿using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Emuses
{
    public class EmusesMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ISession _session;

        private EmusesMiddleware() { }        

        public EmusesMiddleware(RequestDelegate next, ISession session)
        {
            _session = session;
            _next = next;
        }

        public Task Invoke(HttpContext context)
        {
            context.Request.Cookies.TryGetValue("Emuses.SessionId", out var sessionId);
            if (string.IsNullOrEmpty(sessionId))
            {
                _session.Open();
                context.Response.Cookies.Append("Emuses.SessionId", _session.GetSessionId(), new CookieOptions
                {
                    HttpOnly = true
                    // Secure = true
                });
            }
            else
            {
                Console.WriteLine("Update");
            }

            return _next(context);
        }
    }
}