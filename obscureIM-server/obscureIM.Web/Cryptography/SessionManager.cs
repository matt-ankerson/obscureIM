using System;
using System.Collections.Generic;
using obscureIM.Web.Models;

namespace obscureIM.Web.Cryptography
{
    public sealed class SessionManager
    {
        private static volatile SessionManager instance;
        private static object syncRoot = new Object();

        // Dictionary to store crypto sessions.
        public Dictionary<string, string> sessions { get; set; }
        public Dictionary<string, Message> WaitingMessages { get; set; }

        private SessionManager()
        {
            sessions = new Dictionary<string, string>();
            WaitingMessages = new Dictionary<string, Message>();
        }

        public static SessionManager Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new SessionManager();
                    }
                }

                return instance;
            }
        }
    }
}
