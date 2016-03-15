using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace obscureIM_client
{
    public class Client
    {
        private static ConsoleHelper _consoleHelper;

        public static void Main(string[] args)
        {
            _consoleHelper = new ConsoleHelper(0, 0, 20);
            Console.Title = "obscureIM";
            joinChat();
        }

        private static void joinChat()
        {
            Console.Write("Chat Server FQDN / <enter> for default: ");
            var serverUrl = Console.ReadLine();

            if (serverUrl == "")
            {
                serverUrl = "http://ankerson.nz/signalr";
                //serverUrl = "http://localhost:52435/signalr";
            }

            Console.Write("Nickname: ");
            var nick = Console.ReadLine();

            // Create connection manager
            var connectionManager = new ConnectionManager(_consoleHelper);

            try
            {
                connectionManager.EstablishConnection(serverUrl, nick);
            }
            catch (Exception)
            {
                Console.WriteLine("Unable to join chat at " + serverUrl);
                Console.ReadLine();
                return;
            }

            // Begin sending/accepting messages
            messageLoop(connectionManager);
        }

        /// <summary>
        /// Continuously accept messages from the user
        /// </summary>
        /// <param name="connectionManager"></param>
        private static void messageLoop(ConnectionManager connectionManager)
        {
            while (true)
            {
                var message = Console.ReadLine();

                // Clear the typed message.
                Console.SetCursorPosition(0, Console.CursorTop - 1);
                _consoleHelper.ClearCurrentConsoleLine();

                if (message == "/quit")
                {
                    break;
                }

                if (message == "/names")
                {
                    // Issue a request for all nicknames asynchronously
                    connectionManager.RequestNicks();
                }
                else
                {
                    // Don't await this call, fire the message off and continue.
                    connectionManager.SendMessage(message);
                }
            }
        }
    }
}
