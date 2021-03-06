﻿using System;
using Emuses.Exceptions;

namespace Emuses
{
    public class Session
    {
        private const int DefaultSessionTimeout = 60;

        private string _sessionId;
        private string _version;
        private int _sessionTimeout;
        private DateTime _expirationData;
        private readonly ISessionStorage _storage;

        private Session()
        {
        }

        public Session(ISessionStorage storage) : this(DefaultSessionTimeout, storage)
        {
        }

        public Session(int sessionTimeout, ISessionStorage storage)
        {
            _sessionTimeout = sessionTimeout;
            _version = GenerateVersion();
            _expirationData = DateTime.Now.AddMinutes(sessionTimeout);
            _storage = storage;
        }

        public Session(string sessionId, string version, int sessionTimeout, DateTime expirationData, ISessionStorage storage)
        {
            _sessionId = sessionId;
            _version = version;
            _sessionTimeout = sessionTimeout;
            _expirationData = expirationData;
            _storage = storage;
        }
        
        public Session Open()
        {
            _sessionId = Guid.NewGuid().ToString();
            _expirationData = DateTime.Now.AddMinutes(_sessionTimeout);

            _storage.Create(this);
            return this;
        }

        public Session Update(string sessionId)
        {
            Restore(sessionId);

            if (GetExpirationDate() < DateTime.Now)
                throw new SessionExpiredException();

            _expirationData = DateTime.Now.AddMinutes(_sessionTimeout);
            _version = GenerateVersion();

            _storage.Update(this);
            return this;
        }

        public void SetSessionTimeout(int sessionTimeout)
        {
            _sessionTimeout = sessionTimeout;
        }

        public Session Close()
        {
            _expirationData = DateTime.Now;
            _version = string.Empty;

            _storage.Delete(_sessionId);
            return this;
        }

        public bool IsValid()
        {
            return GetExpirationDate() > DateTime.Now;
        }

        public string GetSessionId()
        {
            return _sessionId;
        }

        public int GetSessionTimeout()
        {
            return _sessionTimeout;
        }

        public DateTime GetExpirationDate()
        {
            return _expirationData;
        }

        public string GetVersion()
        {
            return _version;
        }
        
        private static string GenerateVersion()
        {
            return Guid.NewGuid().ToString();
        }

        private void Restore(string sessionId)
        {
            var session = _storage.GetBySessionId(sessionId);
            _sessionId = session.GetSessionId();
            _version = session.GetVersion();
            _sessionTimeout = session.GetSessionTimeout();
            _expirationData = session.GetExpirationDate();
        }
    }
}
