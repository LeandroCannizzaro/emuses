﻿using System;
using Emuses.Example.Core.Entities;
using Emuses.Example.Core.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Emuses.Example.Controllers
{
    public class HomeController : Controller
    {
        private readonly ISession _session;
        private readonly IEmusesSessionRepository _emusesSessionRepository;

        public HomeController(ISession session, IEmusesSessionRepository emusesSessionRepository)
        {
            _session = session;
            _emusesSessionRepository = emusesSessionRepository;
        }

        public IActionResult Index()
        {
            return View();
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public IActionResult Open()
        {
            var entity = _emusesSessionRepository.Create(new EmusesSession(_session.Open(1)));

            Response.Cookies.Append("Emuses.Example.SessionId", entity.SessionId, new CookieOptions
            {
                Expires = DateTimeOffset.Now.AddDays(1), //Expires = entity.ExpireDateTime,
                HttpOnly = true /*,
                Secure = true*/
            });

            ViewData["session"] = $"Created session. Id: {entity.SessionId}, expired date: {entity.ExpireDateTime}";
            return View("Index");
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public IActionResult Update()
        {
            Request.Cookies.TryGetValue("Emuses.Example.SessionId", out string sessionId);
            if (!string.IsNullOrEmpty(sessionId))
            {
                try
                {
                    var entity = _emusesSessionRepository.GetBySessionId(sessionId);

                    var session = _session.Restore(entity.SessionId, entity.Version, entity.ExpireDateTime, entity.Minutes);
                    session.Update();

                    entity = _emusesSessionRepository.Update(new EmusesSession(session));
                    ViewData["session"] = $"Updated session. Id: {entity.SessionId}, expired date: {entity.ExpireDateTime}";
                }
                catch (SessionNotFoundException)
                {
                    ViewData["session"] = $"Session not found: {sessionId}";
                }
                catch (SessionExpiredException)
                {
                    ViewData["session"] = $"Session expired: {sessionId}";
                }
            }

            return View("Index");
        }

        public IActionResult Close()
        {
            Request.Cookies.TryGetValue("Emuses.Example.SessionId", out string sessionId);
            if (!string.IsNullOrEmpty(sessionId))
            {
                try
                {
                    var entity = _emusesSessionRepository.GetBySessionId(sessionId);
                    _emusesSessionRepository.Delete(entity.EmusesSessionId);

                    ViewData["session"] = $"Deleted session. Id: {entity.SessionId}";
                }
                catch (SessionNotFoundException)
                {
                    ViewData["session"] = $"Session not found: {sessionId}";
                    return View("Index");
                }
            }

            Response.Cookies.Delete("Emuses.Example.SessionId");
            return View("Index");
        }

        public IActionResult IsValid()
        {
            Request.Cookies.TryGetValue("Emuses.Example.SessionId", out string sessionId);
            if (!string.IsNullOrEmpty(sessionId))
            {
                try
                {
                    var entity = _emusesSessionRepository.GetBySessionId(sessionId);

                    var session = _session.Restore(entity.SessionId, entity.Version, entity.ExpireDateTime, entity.Minutes);
                    if (session.IsValid())
                        ViewData["session"] = $"Session is valid. Id: {entity.SessionId}, expired date: {entity.ExpireDateTime}";
                    else
                        ViewData["session"] = $"Session is invalid. Id: {entity.SessionId}, expired date: {entity.ExpireDateTime}";
                }
                catch (SessionNotFoundException)
                {
                    ViewData["session"] = $"Session not found: {sessionId}";
                    return View("Index");
                }
            }
            else
                ViewData["session"] = "Session is invalid.";

            return View("Index");
        }
    }
}