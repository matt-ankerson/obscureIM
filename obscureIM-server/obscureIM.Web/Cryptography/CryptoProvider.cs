using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using obscureIM.Web.Models;

namespace obscureIM.Web.Cryptography
{
    /// <summary>
    /// Non-static class for cryptographics fucntionality.
    /// </summary>
    public static class CryptoProvider
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
            return new Message() {
                Sender = EncryptString(publicKeyString, message.Sender),
                MessageContent = EncryptString(publicKeyString, message.MessageContent)
            };
        }

        /// <summary>
        /// Decrypt a given Message
        /// </summary>
        /// <param name="cypherMessage">Message object to decrypt</param>
        /// <param name="privateKeyString">Private key to use for decryption, in string format</param>
        /// <returns>Decrypted Message</returns>
        public static Message Decrypt(Message cypherMessage, string privateKeyString)
        {
            return new Message()
            {
                Sender = DecryptString(privateKeyString, cypherMessage.Sender),
                MessageContent = DecryptString(privateKeyString, cypherMessage.MessageContent)
            };
        }

        /// <summary>
        /// Encrypt a given string using a given public key.
        /// </summary>
        /// <param name="publicKeyString">Public key to use in string format.</param>
        /// <param name="plainText">Text to encrypt</param>
        /// <returns>The cyphertext</returns>
        public static string EncryptString(string publicKeyString, string plainText)
        {
            // Convert the string representation into an RSA param.
            var publicKey = convertStringToKey(publicKeyString);

            // We have a public key ... get a new csp and load the key
            var csp = new RSACryptoServiceProvider();
            csp.ImportParameters(publicKey);

            // Encrypt the plaintext.
            // For encryption, use bytes.
            var bytesPlainText = Encoding.Unicode.GetBytes(plainText);

            // Apply pkcs#1.5 padding and encrypt the data
            var bytesCypherText = csp.Encrypt(bytesPlainText, false);

            // Get a string representation of our cypher text.
            var cypherText = Convert.ToBase64String(bytesCypherText);

            return cypherText;
        }

        /// <summary>
        /// Decrypt a given cyphered message using a given private key.
        /// </summary>
        /// <param name="privateKeyString">The private key to use in string format.</param>
        /// <param name="cypherText">The text to decrypt</param>
        /// <returns>Decrypted string</returns>
        public static string DecryptString(string privateKeyString, string cypherText)
        {
            // First, get the bytes back from the base64 string.
            var bytesCypherText = Convert.FromBase64String(cypherText);

            // We want to decrypt, therefore we need a csp and our private key.
            var privateKey = convertStringToKey(privateKeyString);

            var csp = new RSACryptoServiceProvider();
            csp.ImportParameters(privateKey);

            // Decrypt and strip pkcs#1.5 padding.
            var bytesPlainText = csp.Decrypt(bytesCypherText, false);

            // Get the original plaintext
            var plainText = Encoding.Unicode.GetString(bytesPlainText);

            return plainText;
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