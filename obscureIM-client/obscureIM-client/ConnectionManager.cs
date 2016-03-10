using Microsoft.AspNet.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using obscureIM_client.Models;
using obscureIM_client.Cryptography;

namespace obscureIM_client
{
    public class ConnectionManager
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger
            (System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private HubConnection _hubConnection;
        private IHubProxy _chatHubProxy;

        public ConnectionManager()
        {
            log.Info("Creating Connection Manager");
        }

        public async void EstablishConnection(string serverUrl)
        {
            // Can specify parameters to send to the server via a dictionary, ie. channel to join etc.
            _hubConnection = new HubConnection(serverUrl);

            // Register with the ChatHub proxy.
            _chatHubProxy = _hubConnection.CreateHubProxy("ChatHub");
            registerEventHandlers();

            // Wait for the connection to open before proceding.
            log.Info("Starting Connection");
            await _hubConnection.Start();
            log.Info("Started");
        }

        private void registerEventHandlers()
        {
            // Recieving a command to initiate a downward secure message.
            _chatHubProxy.On<string>("GetPublicKey", sessionId => SendPublicKeyForSecureTransfer(sessionId));


            // Recieving a message.
            _chatHubProxy.On<Message>("AddNewMessage", message => Console.WriteLine("{0} {1}: {2}", DateTime.Now.ToShortTimeString(), message.Sender, message.MessageContent));
        }


        public async Task SendMessage(string nick, string message)
        {
            log.Info("Getting public key for transfer");
            // Get a public key from the server.
            string publicKey = "";
            try
            {
                publicKey = await _chatHubProxy.Invoke<string>("GetPublicKeyForSecureTransfer");
            }
            catch (Exception ex)
            {
                var exc = ex;
            }
            
            log.Info("Encrypting message");
            // Encrypt the message data
            var cypherMessage = CryptoProvider.Encrypt(publicKey, new Message() { Sender = nick, MessageContent = message });

            // Send the encrypted message, include the public key so the server can identify the session.
            cypherMessage.PublicKey = publicKey;

            log.Info("Attempting to send message");
            await _chatHubProxy.Invoke<Message>("SendMessage", cypherMessage);
            log.Info("Sent");
        }

        private async void SendPublicKeyForSecureTransfer(string sessionId)
        {
            // We need to generate a public/private key pair.
            KeyValuePair<string, string> cryptoKeyPair = CryptoProvider.GenerateKeys();

            // Store the Private Key, using its Public Key as the ID.
            SessionManager.Instance.sessions[cryptoKeyPair.Key] = cryptoKeyPair.Value;

            // Send the public key up to the server.
            // We need to include the sessionId so that the server knows
            // which message to encrypt and send down.
            log.Info("Sending generated public key, including sessionId");
            var cypherTextMessage = await _chatHubProxy.Invoke<Message>("GetMessageSecurely", new KeyValuePair<string, string>(sessionId, cryptoKeyPair.Key));
            log.Info("Recieved encrypted message");

            var message = CryptoProvider.Decrypt(cypherTextMessage, SessionManager.Instance.sessions[cryptoKeyPair.Key]);
            log.Info("Decrypted message");

            Console.WriteLine("{0} {1}: {2}", DateTime.Now.ToShortTimeString(), message.Sender, message.MessageContent);

            // Remove the session from the session store
            SessionManager.Instance.sessions.Remove(cryptoKeyPair.Key);
        }

    }
}
