using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using obscureIM.Web.Models;
using obscureIM.Web.Cryptography;
using System.Threading.Tasks;

namespace obscureIM.Web.Hubs
{
    public class ChatHub : Hub
    {
        /// <summary>
        /// Broadcast message to all connected clients.
        /// </summary>
        /// <param name="message"></param>
        public void SendMessage(Message message)
        {
            // The message passed in will have encrypted fields. We 
            // need to decrypt these fields using the right private key.
            var privateKeyString = SessionManager.Instance.sessions[message.PublicKey];

            // Decrypt the message with the private key.
            var plainTextMessage = CryptoProvider.Decrypt(message, privateKeyString);

            // Remove the session from the session dictionary.
            SessionManager.Instance.sessions.Remove(message.PublicKey);

            //-----------------------------------------
            // Initiate a secure broadcast.

            // Use the original public key as a sessionId.
            AskForPublicKeys(message.PublicKey);

            // Set a TTL and store this message against it's ID (the old public key).
            message.Expires = DateTime.Now + TimeSpan.FromMinutes(5);
            SessionManager.Instance.WaitingMessages.Add(message.PublicKey, plainTextMessage);

            //Clients.All.AddNewMessage(message);
        }

        private void AskForPublicKeys(string sessionId)
        {
            // Ask all clients to send up a public key.
            // We'll have to send down the sessionId to ensure we can determine
            // which message to encrypt and send down when the clients reply.
            Clients.All.GetPublicKey(sessionId);
        }


        public Message GetMessageSecurely(KeyValuePair<string, string> sessionIDPublicKeyPair)
        {
            // Fetch the message indicated by the sessionId in the KV pair passed in.
            var plainTextMessage = SessionManager.Instance.WaitingMessages[sessionIDPublicKeyPair.Key];

            // Encrypt the fields on the message using the public key in the KV pair passed in.
            var cypherTextMessage = CryptoProvider.Encrypt(sessionIDPublicKeyPair.Value, plainTextMessage);

            // Remove any expired messages from the WaitingMessages dictionary.
            //RemoveExpiredMessages();

            return cypherTextMessage;
        }

        /// <summary>
        /// Ask all connected clients for their nicks
        /// </summary>
        public void RequestNicks()
        {
            Clients.All.AddNewMessage(new Message() { Sender = "Server", MessageContent = "All nicks requested..." });
            Clients.All.GetNick();
        }

        public void ReleaseNick(string nick)
        {
            Clients.All.AddNewMessage(new Message() { Sender = "Server", MessageContent = nick });
        }

        private void RemoveExpiredMessages()
        {
            var messageDict = SessionManager.Instance.WaitingMessages;

            foreach (var kvPair in messageDict)
            {
                if (kvPair.Value.Expires < DateTime.Now)
                {
                    // Remove the message.
                    SessionManager.Instance.WaitingMessages.Remove(kvPair.Key);
                }
            }
        }

        public string GetPublicKeyForSecureTransfer()
        {
            KeyValuePair<string, string> cryptoKeyPair = CryptoProvider.GenerateKeys();

            // Store the Private Key, using its Public Key as the ID.
            SessionManager.Instance.sessions.Add(cryptoKeyPair.Key, cryptoKeyPair.Value);

            // Return the public key, so the requesting client can send 
            // an encrypted message.
            return cryptoKeyPair.Key;
        }

    }
}