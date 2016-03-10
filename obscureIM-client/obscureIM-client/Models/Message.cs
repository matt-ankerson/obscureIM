using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace obscureIM_client.Models
{
    public class Message
    {
        public string Sender { get; set; }
        public string MessageContent { get; set; }
        public string PublicKey { get; set; }
    }
}
