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
using System.Text.RegularExpressions;
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

        private static log4net.ILog Logger { get; } =
            log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static log4net.ILog SshOutLogger { get; } = log4net.LogManager.GetLogger("SshOutLogger");


        /// <summary>
        /// Random generator
        /// </summary>
        private Random RandomGen { get; }

        /// <summary>
        /// The SSH client itself
        /// </summary>
        private SshClient Client { get; set; }

        /// <summary>
        /// The shell stream of the client
        /// </summary>
        private ShellStream Stream { get; set; }

        /// <summary>
        /// The private key file
        /// </summary>
        private PrivateKeyFile PrivateKeyFile { get; }

        /// <summary>
        /// Application id regex
        /// </summary>
        public Regex AppIdRegex { get; }

        /// <summary>
        /// The host to connect to
        /// </summary>
        public string Host { get; }

        /// <summary>
        /// The username on the host
        /// </summary>
        public string Username { get; }

        /// <summary>
        /// Indicates if the connection is currently executing something
        /// </summary>
        public bool InUse { get; private set; }

        /// <summary>
        /// Name of the connection
        /// </summary>
        public string ConnectionName { get; }

        /// <summary>
        /// Connection id to identify during runtime
        /// </summary>
        private string ConnId { get; }

        #endregion

        #region Main Methods

        /// <summary>
        /// Create a ssh connection instance for the given host and given username
        /// </summary>
        /// <param name="host">Host to connect</param>
        /// <param name="username">Username to connect</param>
        /// <param name="connectionName">Name of the connection</param>
        public SshConnection(string host, string username, string connectionName = "")
        {
            RandomGen = new Random();
            ConnectionName = connectionName;
            ConnId = Convert.ToBase64String(Guid.NewGuid().ToByteArray()).Substring(0, 10);

            Host = host;
            Username = username;
        }

        /// <summary>
        /// Connect via SSH to the given host using the given username and private key file
        /// </summary>
        /// <param name="host">Host to connect</param>
        /// <param name="username">Username to connect</param>
        /// <param name="privKeyFile">The path to the private key file to connect</param>
        /// <param name="connectionName">Name of the connection</param>
        public SshConnection(string host, string username, string privKeyFile, string connectionName = "")
            : this(host, username, new PrivateKeyFile(Environment.ExpandEnvironmentVariables(privKeyFile)), connectionName)
        {

        }

        /// <summary>
        /// Connect via SSH to the given host using the given username and private key file
        /// </summary>
        /// <param name="host">Host to connect</param>
        /// <param name="username">Username to connect</param>
        /// <param name="privKeyFile">The path to the private key file to connect</param>
        /// <param name="appIdRegexPattern">Pattern for returning application ids via <see cref="RunAttachedTillAppId(string)"/></param>
        /// <param name="connectionName">Name of the connection</param>
        public SshConnection(string host, string username, string privKeyFile, string appIdRegexPattern, string connectionName = "")
            : this(host, username, new PrivateKeyFile(Environment.ExpandEnvironmentVariables(privKeyFile)), appIdRegexPattern,
                connectionName)
        {

        }

        /// <summary>
        /// Connect via SSH to the given host using the given username and private key file
        /// </summary>
        /// <param name="host">Host to connect</param>
        /// <param name="username">Username to connect</param>
        /// <param name="privateKeyFile">Private key file instance to connect</param>
        /// <param name="connectionName">Name of the connection</param>
        public SshConnection(string host, string username, PrivateKeyFile privateKeyFile, string connectionName = "")
            : this(host, username, connectionName)
        {
            PrivateKeyFile = privateKeyFile;
            Connect();
        }

        /// <summary>
        /// Connect via SSH to the given host using the given username and private key file
        /// </summary>
        /// <param name="host">Host to connect</param>
        /// <param name="username">Username to connect</param>
        /// <param name="privateKeyFile">Private key file instance to connect</param>
        /// <param name="appIdRegexPattern">Pattern for returning application ids via <see cref="RunAttachedTillAppId(string)"/></param>
        /// <param name="connectionName">Name of the connection</param>
        public SshConnection(string host, string username, PrivateKeyFile privateKeyFile, string appIdRegexPattern, string connectionName = "")
            : this(host, username, privateKeyFile, connectionName)
        {
            AppIdRegex = new Regex(appIdRegexPattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
            //AppIdRegex = new Regex(@"Submitted application (application_\d+_\d+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
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
            ReadForAppId("Last login");
            Logger.Info($"SSH connected to {Username}@{Host} ({ConnectionName}/{ConnId})");
        }

        /// <summary>
        /// Disconnects the SSH connection to the host
        /// </summary>
        public void Disconnect()
        {
            Stream.Close();
            Client.Disconnect();
            Logger.Info($"SSH disconnected from {Username}@{Host} ({ConnectionName}/{ConnId})");
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
        /// Runs the given command on the host and keeps attached on the output
        /// till the application id is parsed and return the id.
        /// If no application id found all output will be returned.
        /// The application whait is only available, if <see cref="AppIdRegex"/> is set.
        /// </summary>
        /// <param name="command">Command to run</param>
        /// <returns>The application id or command output if no id found</returns>
        public string RunAttachedTillAppId(string command)
        {
            InUse = true;

            var exitStr = GetWaitingExitString();
            var sendStr = $"{command}; echo '{exitStr}'";

            Logger.Debug($"[{ConnId}] Executing:\n{sendStr}");
            SshOutLogger.Debug($"[{ConnId}] --> {sendStr}");
            Stream.WriteLine(sendStr);

            var id = ReadForAppId(exitStr);

            InUse = false;
            Task.Run(() => ReadForAppId(exitStr, true)); // get unneeded stuff to make clear for next use

            return id;
        }

        /// <summary>
        /// Runs the given command and returns the output.
        /// </summary>
        /// <param name="command">Command to run</param>
        /// <returns>The command output</returns>
        public string Run(string command)
        {
            InUse = true;

            var res = DoRunCommand(command);

            InUse = false;

            return res;
        }

        /// <summary>
        /// Runs the given command async and ignores all output
        /// </summary>
        /// <param name="command">Command to run</param>
        public void RunAsync(string command)
        {
            InUse = true;

            Task.Run(() => DoRunCommand(command));

            InUse = false;
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Executes the given command on connected host
        /// </summary>
        /// <param name="command">Command to execute</param>
        /// <returns>The output of the command</returns>
        private string DoRunCommand(string command)
        {
            Logger.Debug($"[{ConnId}] Executing: {command}");
            SshOutLogger.Debug($"[{ConnId}] --> {command}");
            var cmd = Client.RunCommand(command);
            var output = cmd.ExitStatus == 0 ? cmd.Result : cmd.Error;
            SshOutLogger.Debug($"[{ConnId}] {output.Trim()}");
            return output;
        }

        /// <summary>
        /// Reads the output till the exit string or any application id
        /// </summary>
        /// <param name="exitStr">The exit string that indicates the end of the output</param>
        /// <param name="isAsync">True if the output should read async</param>
        /// <returns>The application id or the full output</returns>
        private string ReadForAppId(string exitStr, bool isAsync = false)
        {
            StringBuilder result = new StringBuilder();
            string line;
            byte count = 0;
            do
            {
                if(isAsync)
                    InUse = true;

                line = Stream.ReadLine();

                result.AppendLine(line);
                SshOutLogger.Debug($"[{ConnId}] {line.Trim()}");

                if(!isAsync && AppIdRegex != null)
                {
                    var idMatch = AppIdRegex.Match(line);
                    if(idMatch.Groups[1].Success)
                    {
                        //Task.Run(() => ReadForAppId(exitStr));
                        return idMatch.Groups[1].Value;
                    }
                }

                if(count <= 1) count++;
            } while(!line.Contains(exitStr) || count <= 1);

            if(isAsync)
                InUse = false;

            var resultStr = result.ToString();
            return resultStr;
        }

        /// <summary>
        /// Generates an exit string
        /// </summary>
        /// <returns>The exit string</returns>
        private string GetWaitingExitString()
        {
            return $"[cmd-end]id-{RandomGen.Next(0xffff):x4}";
        }

        #endregion
    }
}
