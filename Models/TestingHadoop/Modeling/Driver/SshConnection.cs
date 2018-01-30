// The MIT License (MIT)
// 
// Copyright (c) 2014-2018, Institute for Software & Systems Engineering
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Text;
using System.Threading.Tasks;
using Renci.SshNet;

namespace SafetySharp.CaseStudies.TestingHadoop.Modeling.Driver
{
    /// <summary>
    /// Represents a ssh connection
    /// </summary>
    public class SshConnection : IDisposable
    {
        #region Properties

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
        /// Indicates if the connection is currently executing something
        /// </summary>
        public bool InUse { get; private set; }

        #endregion

        #region Main Methods

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
        /// Releases all ressources
        /// </summary>
        public void Dispose()
        {
            Disconnect();
            Client?.Dispose();
            Stream?.Dispose();
            PrivateKeyFile?.Dispose();
        }

        #endregion

        #region Connection Methods

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

        #endregion

        #region Executing Methods

        /// <summary>
        /// Runs the given command on the host and returns the output immediately
        /// </summary>
        /// <param name="command">Command to run</param>
        /// <param name="consoleOut">True to show the output directly on the own shell</param>
        /// <returns>The command output</returns>
        public string RunIm(string command, bool consoleOut = false)
        {
            InUse = true;

            var exitStr = GetExitString();
            var sendStr = $"{command}; echo '{exitStr}'";

            Out($"Executing: {sendStr}", consoleOut);
            Stream.WriteLine(sendStr);
            var answer = Read(exitStr, consoleOut);

            InUse = false;
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
            InUse = true;

            var cmd = Client.CreateCommand(command);
            Out($"Executing: {cmd.CommandText}", consoleOut);
            cmd.Execute();
            Out(cmd.Result, consoleOut);

            InUse = false;
            return cmd.Result;
        }

        /// <summary>
        /// Runs the given command async and waits NOT for the output
        /// </summary>
        /// <param name="consoleOut">True to show the cmd on the own shell</param>
        /// <param name="command">Command to run</param>
        public void RunAsync(string command, bool consoleOut = false)
        {
            InUse = true;

            //var cmd = Client.CreateCommand(command);
            //Out($"Executing: {cmd.CommandText}", consoleOut);
            //cmd.BeginExecute();

            Task.Run(() => RunIm(command, consoleOut));
            //exec.Wait();

            InUse = false;
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
                Out(line, consoleOut);

                if(count <= 1) count++;
            } while(!line.ToLower().Contains(exitStr) || count <= 1);

            return result.ToString();
        }

        /// <summary>
        /// Writes the given line to log file and shows on console
        /// </summary>
        /// <param name="line">The line to write</param>
        /// <param name="consoleOut">True to show the output directly on the own shell</param>
        ///// <param name="callingMember">The calling member for log</param>
        private void Out(string line, bool consoleOut = false/*, [CallerMemberName] string callingMember = ""*/)
        {
            if(consoleOut)
                Console.WriteLine(line);
            // TODO: Log to file
        }

        /// <summary>
        /// Gets the exit string for the given command
        /// </summary>
        /// <returns>the exit string</returns>
        private string GetExitString()
        {
            return $"[cmd-end]id-{_RandomGen.Next(0xffff):x4}";
        }

        #endregion
    }
}
