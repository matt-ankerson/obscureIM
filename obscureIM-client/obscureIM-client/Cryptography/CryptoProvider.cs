using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using obscureIM_client.Models;

namespace obscureIM_client.Cryptography
{
    public class CryptoProvider
    {
        /// <summary>
        /// Generate public and private keys, save in this instance.
        /// </summary>
        public static KeyValuePair<string, string> GenerateKeys()
        {
            // Create a new CSP with a new 2048 bit key pair.
            var csp = new RSACryptoServiceProvider(2048);

            // Extract the public key
            var publicKey = csp.ExportParameters(false);

            // Extract the private key
            var privateKey = csp.ExportParameters(true);

            // Convert the public key into a string representation
            var publicKeyString = convertKeyToString(publicKey);

            // Convert the private key into a string representation
            var privateKeyString = convertKeyToString(privateKey);

            return new KeyValuePair<string, string>(publicKeyString, privateKeyString);
        }

        /// <summary>
        /// Encrypt a Message
        /// </summary>
        /// <param name="publicKeyString">Public key to use for the encryption.</param>
        /// <param name="message">Message object to encrypt.</param>
        /// <returns>Encrypted Message</returns>
        public static Message Encrypt(string publicKeyString, Message message)
        {
            // Convert the string representation into an RSA param.
            var publicKey = convertStringToKey(publicKeyString);

            // We have a public key ... get a new csp and load the key
            var csp = new RSACryptoServiceProvider();
            csp.ImportParameters(publicKey);

            // Encrypt both the sender and messageContent fields in the message object.
            var plainTextSender = message.Sender;
            var plainTextMessageContent = message.MessageContent;

            // For encryption, use bytes.
            var bytesPlainTextSender = Encoding.Unicode.GetBytes(plainTextSender);
            var bytesPlainTextMessageContent = Encoding.Unicode.GetBytes(plainTextMessageContent);

            // Apply pkcs#1.5 padding and encrypt the data
            var bytesCypherTextSender = csp.Encrypt(bytesPlainTextSender, false);
            var bytesCypherTextMessageContent = csp.Encrypt(bytesPlainTextMessageContent, false);

            // Get string representations of our cypher text.
            var cypherTextSender = Convert.ToBase64String(bytesCypherTextSender);
            var cypherTextMessageContent = Convert.ToBase64String(bytesCypherTextMessageContent);

            // Return a new message with the fields encrypted.
            return new Message() { Sender = cypherTextSender, MessageContent = cypherTextMessageContent };
        }

        public static Message Decrypt(Message cypherMessage, string privateKeyString)
        {
            var cypherTextSender = cypherMessage.Sender;
            var cypherTextMessageContent = cypherMessage.MessageContent;

            // First, get the bytes back from the base64 strings.
            var bytesCypherTextSender = Convert.FromBase64String(cypherTextSender);
            var bytesCypherTextMessageContent = Convert.FromBase64String(cypherTextMessageContent);

            // We want to decrypt, therefore we need a csp and our private key.
            var privateKey = convertStringToKey(privateKeyString);

            var csp = new RSACryptoServiceProvider();
            csp.ImportParameters(privateKey);

            // Decrypt and strip pkcs#1.5 padding.
            var bytesPlainTextSender = csp.Decrypt(bytesCypherTextSender, false);
            var bytesPlainTextMessageContent = csp.Decrypt(bytesCypherTextMessageContent, false);

            // Get the original plaintext
            var plainTextSender = System.Text.Encoding.Unicode.GetString(bytesPlainTextSender);
            var plainTextMessageContent = System.Text.Encoding.Unicode.GetString(bytesPlainTextMessageContent);

            // Return a decrypted message
            return new Message() { Sender = plainTextSender, MessageContent = plainTextMessageContent };
        }

        private static string convertKeyToString(RSAParameters key)
        {
            // We need a buffer
            var sw = new System.IO.StringWriter();
            // We need a serialiser
            var xs = new System.Xml.Serialization.XmlSerializer(typeof(RSAParameters));
            // Serialise the key into the stream
            xs.Serialize(sw, key);

            // Get the string from the stream
            return sw.ToString();
        }

        private static RSAParameters convertStringToKey(string keyString)
        {
            // Get a stream from the string
            var sr = new System.IO.StringReader(keyString);
            // We need a deserialiser
            var xs = new System.Xml.Serialization.XmlSerializer(typeof(RSAParameters));
            // Get the object back from the stream
            return (RSAParameters)xs.Deserialize(sr);
        }
    }
}
