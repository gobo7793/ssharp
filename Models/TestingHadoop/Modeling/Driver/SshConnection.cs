﻿using System;
using System.Text;
using Renci.SshNet;

namespace SafetySharp.CaseStudies.TestingHadoop.Modeling.Driver
{
    /// <summary>
    /// Represents a ssh connection
    /// </summary>
    public class SshConnection : IDisposable
    {

        /// <summary>
        /// Random generator
        /// </summary>
        private readonly Random _RandomGen = new Random();

        /// <summary>
        /// The SSH client itself
        /// </summary>
        public SshClient Client { get; private set; }

        /// <summary>
        /// The shell stream of the client
        /// </summary>
        public ShellStream Stream { get; private set; }

        /// <summary>
        /// The host to connect to
        /// </summary>
        public string Host { get; }

        /// <summary>
        /// The username on the host
        /// </summary>
        public string Username { get; }

        /// <summary>
        /// The private key file
        /// </summary>
        private PrivateKeyFile PrivateKeyFile { get; }

        /// <summary>
        /// Create a ssh connection instance for the given host and given username
        /// </summary>
        /// <param name="host">Host to connect</param>
        /// <param name="username">Username to connect</param>
        public SshConnection(string host, string username)
        {
            Host = host;
            Username = username;
        }

        /// <summary>
        /// Connect via SSH to the given host using the given username and private key file
        /// </summary>
        /// <param name="host">Host to connect</param>
        /// <param name="username">Username to connect</param>
        /// <param name="privKeyFile">The path to the private key file to connect</param>
        public SshConnection(string host, string username, string privKeyFile)
            : this(host, username, new PrivateKeyFile(privKeyFile))
        {

        }

        /// <summary>
        /// Connect via SSH to the given host using the given username and private key file
        /// </summary>
        /// <param name="host">Host to connect</param>
        /// <param name="username">Username to connect</param>
        /// <param name="privateKeyFile">Private key file instance to connect</param>
        public SshConnection(string host, string username, PrivateKeyFile privateKeyFile)
            : this(host, username)
        {
            PrivateKeyFile = privateKeyFile;
            Connect();
        }

        /// <summary>
        /// Deconstructor
        /// </summary>
        ~SshConnection()
        {
            Dispose();
        }

        /// <summary>
        /// Connects to the Host via SSH using the given password
        /// </summary>
        /// <param name="password">Password</param>
        public void Connect(string password)
        {
            Client = new SshClient(Host, Username, password);
            DoConnect();
        }

        /// <summary>
        /// Connects to the Host via SSH using the <see cref="PrivateKeyFile"/>
        /// </summary>
        /// <exception cref="InvalidOperationException">If no <see cref="PrivateKeyFile"/> given</exception>
        public void Connect()
        {
            if(PrivateKeyFile == null)
                throw new InvalidOperationException($"Cannot connect to {Username}@{Host}: Private key file missing.");
            Client = new SshClient(Host, Username, PrivateKeyFile);
            DoConnect();
        }

        /// <summary>
        /// Establish the connection
        /// </summary>
        private void DoConnect()
        {
            Client.Connect();
            Stream = Client.CreateShellStream("ssharpShell", 120, 24, 800, 600, 2048);
            Read("Last login");
        }

        /// <summary>
        /// Disconnects the SSH connection to the host
        /// </summary>
        public void Disconnect()
        {
            Stream.Close();
            Client.Disconnect();
        }

        /// <summary>
        /// Disconnects and reconnects to the host via SSH.
        /// Note: Not available for password based connection.
        /// </summary>
        public void Reconnect()
        {
            Disconnect();
            Connect();
        }

        /// <summary>
        /// Runs the given command on the host and returns the output immediately
        /// </summary>
        /// <param name="command">Command to run</param>
        /// <param name="consoleOut">True to show the output directly on the own shell</param>
        /// <returns>The command output</returns>
        public string RunIm(string command, bool consoleOut = false)
        {
            var exitStr = GetExitString();
            var sendStr = $"{command}; echo '{exitStr}'";

            Stream.WriteLine(sendStr);
            var answer = Read(exitStr, consoleOut);
            return answer;
        }

        /// <summary>
        /// Runs the given command and returns the full output when the command finished
        /// </summary>
        /// <param name="command">Command to run</param>
        /// <param name="consoleOut">True to show the output directly on the own shell</param>
        /// <returns>The command output</returns>
        public string Run(string command, bool consoleOut = false)
        {
            var cmd = Client.CreateCommand(command);
            cmd.Execute();
            if (consoleOut) Console.WriteLine(cmd.Result);
            return cmd.Result;
        }

        /// <summary>
        /// Reads the output of the shell till the output contains the given exit string
        /// </summary>
        /// <param name="exitStr">The output exit string</param>
        /// <param name="consoleOut">True to show the output directly on the own shell</param>
        /// <returns>The output</returns>
        public string Read(string exitStr, bool consoleOut = false)
        {
            exitStr = exitStr.ToLower();
            StringBuilder result = new StringBuilder();

            string line;
            byte count = 0;
            do
            {
                line = Stream.ReadLine();

                result.AppendLine(line);
                if (consoleOut)
                    Console.WriteLine(line);

                if (count <= 1) count++;
            } while (!line.ToLower().Contains(exitStr) || count <= 1);

            return result.ToString();
        }

        /// <summary>
        /// Gets the exit string for the given command
        /// </summary>
        /// <returns>the exit string</returns>
        private string GetExitString()
        {
            return $"[cmd-end]id-{_RandomGen.Next(0xffff):x4}";
        }

        /// <summary>
        /// Releases all ressources
        /// </summary>
        public void Dispose()
        {
            Disconnect();
            Client?.Dispose();
            Stream?.Dispose();
            PrivateKeyFile?.Dispose();
        }
    }
}