using System.Threading;
using System.IO.Pipes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using System.Linq;
using System;
using System.Diagnostics;

namespace RGN.MyEditor
{
    public class WindowsNamedPipes : MonoBehaviour
    {
        private Thread serverReadThread;
        private Thread clientWriteThread;
        private object readLock;
        private object writeLock;
        private Queue<string> readQueue;
        private Queue<string> writeQueue;

        //private Mutex mutex = new Mutex(true, "MyAppMutex");

        private void Awake()
        {

            // Check if the existing instance is running
            if (!IsGameInstanceRunning(Application.productName))
            {
                // Listen for incoming IPC messages
                StartServerThread();

                // Process the command line arguments
                if (Environment.GetCommandLineArgs().Contains("unitydl"))
                {
                    // Process the new argument
                    UnityEngine.Debug.Log(Environment.GetCommandLineArgs()[1]);
                }
                else
                {
                    // No new argument, start the application as normal
                    UnityEngine.Debug.Log("Starting application");
                }
            }
            else
            {
                // The existing instance is running
                // Send the new argument as an IPC message
                StartClientThread();
                ClientThread_Write();
            }

        }

        private bool IsGameInstanceRunning(string processName)
        {
            Process[] processArray = Process.GetProcesses();
            Process process = processArray.FirstOrDefault(x => x.ProcessName.Contains(processName));
            if (process != null)
                return true;
            else
                return false;
        }

        private void StartServerThread()
        {
            serverReadThread = new Thread(ServerThread_Read);
            serverReadThread.Start();
            UnityEngine.Debug.Log("Starting server thread");
        }

        private void ServerThread_Read()
        {
            NamedPipeServerStream namedPipeServerStream = new NamedPipeServerStream("ServerRead_ClientWrite", PipeDirection.In);

            namedPipeServerStream.WaitForConnection();

            try
            {
                StreamString streamString = new StreamString(namedPipeServerStream);
                while (true)
                {
                    string message = streamString.ReadString();
                    UnityEngine.Debug.Log("Message : " + message);

                    lock (readLock)
                    {
                        readQueue.Enqueue(message);
                    }

                    Thread.Sleep(10);
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.Log("ERROR : " + e);
            }

            namedPipeServerStream.Close();
        }

        private void StartClientThread()
        {
            clientWriteThread = new Thread(ClientThread_Write);
            clientWriteThread.Start();
        }

        private void ClientThread_Write()
        {
            NamedPipeClientStream namedPipeClientStream = new NamedPipeClientStream(".", "ServerRead_ClientWrite", PipeDirection.Out);

            while (!namedPipeClientStream.IsConnected)
            {
                UnityEngine.Debug.Log("Connecting to server...");
                try
                {
                    namedPipeClientStream.Connect();
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.Log("Connection failed : " + e);
                }
                Thread.Sleep(1000);
            }

            UnityEngine.Debug.Log("Connected to server!");

            try
            {
                StreamString streamString = new StreamString(namedPipeClientStream);
                streamString.WriteString("Hello from the other side!!");

                while (true)
                {
                    string messageQueue = null;

                    lock (writeLock)
                    {
                        if (writeQueue.Count > 0)
                        {
                            messageQueue = writeQueue.Dequeue();
                        }
                    }

                    if (messageQueue != null)
                    {
                        streamString.WriteString("Hello from the other side!!");
                    }

                    Thread.Sleep(10);
                }

            }
            catch (Exception e)
            {
                UnityEngine.Debug.Log("ERROR : " + e);
            }

            namedPipeClientStream.Close();

            Application.Quit();
        }

        private void ReadMessage()
        {
            lock (readLock)
            {
                if (readQueue.Count > 0)
                {
                    string message = readQueue.Dequeue();
                    UnityEngine.Debug.Log(message); // TODO Do something with message (Action)
                }
            }
        }

        private void OnDestroy()
        {
            if (serverReadThread != null)
                serverReadThread.Abort();
            if (clientWriteThread != null)
                clientWriteThread.Abort();
        }
    }

    public class StreamString
    {
        private Stream ioStream;
        private UnicodeEncoding streamEncoding;

        public StreamString(Stream ioStream)
        {
            this.ioStream = ioStream;
            streamEncoding = new UnicodeEncoding();
        }

        public string ReadString()
        {
            int len = 0;

            len = ioStream.ReadByte() * 256;
            len += ioStream.ReadByte();
            byte[] inBuffer = new byte[len];
            ioStream.Read(inBuffer, 0, len);

            return streamEncoding.GetString(inBuffer);
        }

        public int WriteString(string outString)
        {
            byte[] outBuffer = streamEncoding.GetBytes(outString);
            int len = outBuffer.Length;
            if (len > UInt16.MaxValue)
            {
                len = (int)UInt16.MaxValue;
            }
            ioStream.WriteByte((byte)(len / 256));
            ioStream.WriteByte((byte)(len & 255));
            ioStream.Write(outBuffer, 0, len);
            ioStream.Flush();

            return outBuffer.Length + 2;
        }
    }
}
