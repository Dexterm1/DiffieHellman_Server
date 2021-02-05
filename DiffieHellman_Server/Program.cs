using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.IO;

namespace DiffieHellman_Server
{
    class Program
    {
        public static List<TcpClient> clients = new List<TcpClient>();
        public static string privKey = "webquiz";
        public static string pubServerKey = "pcandxlr";
        public static byte[] pubClientKey = new byte[256];
        public static string combinedKey = pubServerKey + privKey;
        public static string newComboKey = "";

        static void Main(string[] args)
        {
            IPAddress ip = IPAddress.Parse("127.0.0.1");
            int port = 13356;
            TcpListener listener = new TcpListener(ip, port);
            listener.Start();

            AcceptClients(listener);

            bool isRunning = true;
            while (isRunning)
            {
                // Send a Message
                Console.WriteLine("Write message: ");
                string plainText = Console.ReadLine();
                string cipherText = EncryptByte(plainText, newComboKey);
                byte[] buffer = Encoding.UTF8.GetBytes(cipherText);

                foreach (TcpClient client in clients)
                {
                    client.GetStream().Write(buffer, 0, buffer.Length);
                }
            }
        }

        public static async void AcceptClients(TcpListener listener)
        {
            bool isRunning = true;
            while (isRunning)
            {
                TcpClient client = await listener.AcceptTcpClientAsync();
                clients.Add(client);

                byte[] keyBuffer = Encoding.UTF8.GetBytes(combinedKey);

                NetworkStream stream = client.GetStream();
                ReceiveMessages(stream);

                // Send server key
                stream.Write(keyBuffer, 0, keyBuffer.Length);
            }
        }

        public static async void ReceiveMessages(NetworkStream stream)
        {
            byte[] buffer = new byte[256];
            bool isRunning = true;

            while (isRunning)
            {
                int read = await stream.ReadAsync(buffer, 0, buffer.Length);

                string text = Encoding.UTF8.GetString(buffer, 0, read);

                if (!text.EndsWith("pcandxlr"))
                {
                    Console.WriteLine("Encrypted: " + text);
                    string decryptText = DecryptByte(text, newComboKey);
                    Console.WriteLine("Decrypted: " + decryptText);
                }
                else
                {
                    newComboKey = text + privKey;
                    Console.WriteLine("SharedKey: " + newComboKey);
                }
            }
        }

        static string EncryptByte(string plainText, string key)
        {
            char[] chars = new char[plainText.Length];

            for(int i = 0; i < plainText.Length; i++)
            {
                if (plainText[i] == ' ')
                {
                    chars[i] = ' ';
                } 
                else
                {
                    int j = plainText[i] - 97;
                    chars[i] = key[j];
                }
            }

            return new string(chars);
        }

        static string DecryptByte(string cipherText, string key)
        {
            char[] chars = new char[cipherText.Length];

            for (int i = 0; i < cipherText.Length; i++)
            {
                if (cipherText[i] == ' ')
                {
                    chars[i] = ' ';
                }
                else
                {
                    int j = key.IndexOf(cipherText[i]) + 97;
                    chars[i] = (char)j;
                }
            }

            return new string(chars);
        }
    }
}
