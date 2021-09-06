using System.Net.Sockets;
using System.Text;
using System.Net;
using System.Threading;
using System.Security.Cryptography;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System;
using System.IO;

namespace ChessWPF
{
    public class NetworkClient
    {
        // Some constants 
        const int RSA_KEY_SIZE = 2048;
        const int PORT = 5000;
        const int BUFFER_SIZE = 1024;
        const string MESSAGE_SEPERATOR = "\n\n";
        readonly IPAddress ServerIp = IPAddress.Parse("127.0.0.1");

        private MainWindow callBackWindow;
        private string MessageBuffer;
        private bool HasKey;
        private byte[] AesKey;

        TcpClient Client;
        NetworkStream ClientStream;

        public NetworkClient(MainWindow callBackWindow)
        {
            HasKey = false;
            this.callBackWindow = callBackWindow;
            Thread _th = new Thread(new ThreadStart(run));
            _th.IsBackground = true;
            _th.Start();
        }

        /// <summary>
        /// Running in a seperate thread, always reads messages
        /// and passes them to main window
        /// </summary>
        private void run()
        {
            bool Connected = false;
            bool AlertedWindowConnectionFailed = false;
            Client = new TcpClient();

            while (!Connected)
            {
                try
                {
                    Client.Connect(ServerIp, PORT);
                    ClientStream = Client.GetStream();
                    Connected = true;
                }
                catch
                {
                    if (!AlertedWindowConnectionFailed)
                    {
                        callBackWindow.OnConnectionFailed();
                        AlertedWindowConnectionFailed = true;
                    }
                }
            }

            // If connected after disconnection, update UI
            if (AlertedWindowConnectionFailed)
                callBackWindow.OnReconnection();

            // Establish aes key
            HasKey = false;
            EstablishSecret();
            HasKey = true;

            int just_recieved = 0;

            while (true)
            {
                byte[] buffer = new byte[BUFFER_SIZE];
                try
                {
                    just_recieved = ClientStream.Read(buffer, 0, BUFFER_SIZE);
                    string Message = Encoding.UTF8.GetString(buffer, 0, just_recieved);
                    GotMessage(Message);
                }
                catch
                {
                    // Show message and try to reconnect
                    callBackWindow.OnConnectionFailed();
                    Connected = false;
                    while (!Connected)
                    {
                        try
                        {
                            Client = new TcpClient();
                            Client.Connect(ServerIp, PORT);
                            ClientStream = Client.GetStream();
                            Connected = true;
                            callBackWindow.OnReconnectionAfterConnected();
                        }
                        catch
                        {

                        }
                    }

                    // Establish aes key
                    HasKey = false;
                    EstablishSecret();
                    HasKey = true;
                }
            }
        }

        private void GotMessage(string Message)
        {
            // Buffer messages
            MessageBuffer += Message;
            string[] Messages = Regex.Split(MessageBuffer, MESSAGE_SEPERATOR);
            for (int i = 0; i < Messages.Length - 1; i++)
            {
                // First decrypt data
                string result = Decrypt(Messages[i]);
                Trace.WriteLine(result);
                callBackWindow.OnRecievedMessage(result);
            }

            MessageBuffer = "";
            MessageBuffer += Messages[Messages.Length - 1];
        }

        public void SendMsg(string mess)
        {
            while (!HasKey)
            {
                Thread.Sleep(10);
                // Rare instance where client tries to send something
                // Before key exchange - Very rare, client should just wait
                // For key, the application might freeze for a very small time
            }

            mess = Encrypt(mess);
            byte[] msg = Encoding.ASCII.GetBytes(mess);
            try
            {
                ClientStream.Write(msg, 0, msg.Length);
            }
            catch
            {
                // Exception will only rise when the server disconnected, and 
                // One of the window is trying to write to the socket.
                // Ignore the exception because the Network-Thread
                // Is Working to reconnect already
            }
        }


        // <----------------------------------------->
        // <----------------------------------------->
        // Encryption Utilities Used by NetworkClient
        // <----------------------------------------->
        // <----------------------------------------->

        private void EstablishSecret()
        {
            // Send public rsa key
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(RSA_KEY_SIZE);
            string exported_key = rsa.ToXmlString(false);
            byte[] msg = Encoding.ASCII.GetBytes(exported_key);
            ClientStream.Write(msg, 0, msg.Length);

            // Wait for aes key, encrypted with RSA
            byte[] buffer = new byte[1024];
            int just_recieved = ClientStream.Read(buffer, 0, BUFFER_SIZE);

            // Take from the buffer the bytes that the socket actally recievied
            byte[] actual_buffer = new byte[just_recieved];
            Array.Copy(buffer, actual_buffer, just_recieved);

            // The info is an aes-key
            byte[] decrypted = rsa.Decrypt(actual_buffer, true);

            // Use the aes-key
            AesKey = decrypted;
        }

        private string Encrypt(string mess)
        {
            AesCryptoServiceProvider Key = new AesCryptoServiceProvider();
            Key.Key = AesKey;
            Key.Padding = PaddingMode.Zeros;
            Key.Mode = CipherMode.CBC;

            string iv = Convert.ToBase64String(Key.IV);
            byte[] message_bytes = Encoding.UTF8.GetBytes(mess);

            // The encryption itself
            using (MemoryStream stream = new MemoryStream())
            using (ICryptoTransform encryptor = Key.CreateEncryptor())
            using (CryptoStream encrypt = new CryptoStream(stream, encryptor, CryptoStreamMode.Write))
            {
                encrypt.Write(message_bytes, 0, message_bytes.Length);
                encrypt.FlushFinalBlock();
                message_bytes = stream.ToArray();
            }

            string message = Convert.ToBase64String(message_bytes);

            return iv + message + MESSAGE_SEPERATOR;
        }

        private string Decrypt(string mess)
        {
            string iv = mess.Substring(0, 24);
            string cipher_text = mess.Substring(24, mess.Length - 24);

            byte[] iv_bytes = Convert.FromBase64String(iv);
            byte[] cipher_text_bytes = Convert.FromBase64String(cipher_text);

            using (AesCryptoServiceProvider Key = new AesCryptoServiceProvider())
            {
                Key.Key = AesKey;
                Key.Mode = CipherMode.CBC;
                Key.IV = iv_bytes;
                Key.Padding = PaddingMode.Zeros;

                // The decryption its self
                using (MemoryStream stream = new MemoryStream())
                using (ICryptoTransform decryptor = Key.CreateDecryptor())
                using (CryptoStream decrypt = new CryptoStream(stream, decryptor, CryptoStreamMode.Write))
                {
                    decrypt.Write(cipher_text_bytes, 0, cipher_text_bytes.Length);
                    decrypt.FlushFinalBlock();
                    return Encoding.ASCII.GetString(stream.ToArray()).Trim('\0');
                }
            }
        }
    }
}
