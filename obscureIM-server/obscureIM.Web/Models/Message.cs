using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace obscureIM.Web.Models
{
    public class Message
    {
        public string Sender { get; set; }
        public string MessageContent { get; set; }
        public string PublicKey { get; set; }
        public DateTime Expires { get; set; }
    }
}