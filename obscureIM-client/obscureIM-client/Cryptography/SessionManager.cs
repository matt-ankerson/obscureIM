using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace obscureIM_client.Cryptography
{
    public sealed class SessionManager
    {
        private static volatile SessionManager instance;
        private static object syncRoot = new Object();

        // Dictionary to store crypto sessions.
        public Dictionary<string, string> sessions { get; set; }

        private SessionManager()
        {
            sessions = new Dictionary<string, string>();
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
