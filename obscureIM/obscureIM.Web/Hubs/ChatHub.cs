using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;

namespace obscureIM.Web.Hubs
{
    public class ChatHub : Hub
    {
        public void Hello(string message)
        {
            Clients.All.AddNewMessage(message);
        }

        //public void Send(string name, string message)
        //{
        //    Clients.All.addNewMessage(name, message);
        //}
    }
}