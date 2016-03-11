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

        public static void Main(string[] args)
        {
            Console.Title = "obscureIM";
            JoinChat();
        }

        private static void JoinChat()
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
            var connectionManager = new ConnectionManager();

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

        private static void messageLoop(ConnectionManager connectionManager)
        {
            while (true)
            {
                var message = Console.ReadLine();

                // Clear the typed message.
                Console.SetCursorPosition(0, Console.CursorTop - 1);
                clearCurrentConsoleLine();

                if (message == "/quit")
                {
                    break;
                }

                if (message == "/names")
                {
                    connectionManager.RequestNicks();
                }
                else
                {
                    // Don't await this call, fire the message off and continue.
                    connectionManager.SendMessage(message);
                }
            }
        }

        private static void clearCurrentConsoleLine()
        {
            int currentLineCursor = Console.CursorTop;
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, currentLineCursor);
        }
    }
}
