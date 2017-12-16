﻿using System;

namespace Emuses
{
    public class Session : ISession
    {
        private string _sessionId;
        private int _minutes;
        private DateTime _expireDateTime;
        private string _version;

        public Session()
        {
        }

        public Session(string sessionId, int minutes)
        {
            _sessionId = sessionId;
            _minutes = minutes;
            _expireDateTime = DateTime.Now.AddMinutes(minutes);
            _version = GenerateVersion();
        }
        
        public Session Open(int minutes)
        {
            return new Session(Guid.NewGuid().ToString(), minutes);
        }

        public Session Update()
        {
            if (GetExpiredDate() < DateTime.Now)
                throw new SessionExpiredException();

            _expireDateTime = DateTime.Now.AddMinutes(_minutes);
            _version = GenerateVersion();
            return this;
        }

        public Session Restore(string sessionId, string version, DateTime expiredDateTime, int minutes)
        {
            _sessionId = sessionId;
            _version = version;
            _expireDateTime = expiredDateTime;
            _minutes = minutes;

            return this;
        }

        public Session Close()
        {
            _expireDateTime = DateTime.Now;
            _version = string.Empty;

            return this;
        }

        public bool IsValid()
        {
            return GetExpiredDate() > DateTime.Now;
        }

        public string GetSessionId()
        {
            return _sessionId;
        }

        public int GetMinutes()
        {
            return _minutes;
        }

        public DateTime GetExpiredDate()
        {
            return _expireDateTime;
        }

        public string GetVersion()
        {
            return _version;
        }

        private static string GenerateVersion()
        {
            return Guid.NewGuid().ToString();
        }
    }
}