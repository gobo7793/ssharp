﻿#region License
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
#endregion

using System;
using SafetySharp.CaseStudies.TestingHadoop.Modeling.BenchModel;
using SafetySharp.CaseStudies.TestingHadoop.Modeling.HadoopModel;
using SafetySharp.Modeling;

namespace SafetySharp.CaseStudies.TestingHadoop.Modeling
{
    /// <summary>
    /// Helper class to save general settings for the <see cref="Model"/>
    /// </summary>
    public static class ModelSettings
    {
        #region EHostMode

        /// <summary>
        /// Enum for possible host modes for this model
        /// </summary>
        public enum EHostMode
        {
            /// <summary>
            /// Classical mode that the cluster is started inside a docker-machine envoiroment,
            /// provided by hadoop-benchmark. The docker-machines are realized by virtualbox as VMs.
            /// One machine is for the consul, one for the controller and benchmark containers and
            /// each compute has its own VM.
            /// </summary>
            DockerMachine,

            /// <summary>
            /// Alternative mode that the cluster containers are directly started on the cluster pc
            /// without using docker-machine. The hadoop containers (one for controller and for each
            /// compute) are inside a docker swarm so it's possible to run the cluster on multiple hosts.
            /// </summary>
            Multihost
        }

        #endregion

        #region General

        /// <summary>
        /// Prefix for compute nodes
        /// </summary>
        public const string NodeNamePrefix = "compute-";

        /// <summary>
        /// Minimum step time for execution
        /// </summary>
        public static TimeSpan MinStepTime { get; set; } = new TimeSpan(0, 0, 20);

        /// <summary>
        /// Probability to activate faults, from 0.0 (never, default), up to 1.0 (always)
        /// </summary>
        [Range(0.0, 1.0, OverflowBehavior.Clamp)]
        public static double FaultActivationProbability { get; set; } = 0.0;

        /// <summary>
        /// Probability to repair faults, from 0.0 (never), up to 1.0 (always in step after activation, default)
        /// </summary>
        [Range(0.0, 1.0, OverflowBehavior.Clamp)]
        public static double FaultRepairProbability { get; set; } = 1.0;

        private static int _RandomBaseSeed = Environment.TickCount;

        /// <summary>
        /// Base seed for model random generators, default <see cref="Environment.TickCount"/>.
        /// </summary>
        /// <remarks>
        /// Static generators which will use the seed have to be added here.
        /// </remarks>
        public static int RandomBaseSeed
        {
            get { return _RandomBaseSeed; }
            set
            {
                if(value != _RandomBaseSeed)
                {
                    _RandomBaseSeed = value;

                    NodeFaultAttribute.RandomGen = new Random(_RandomBaseSeed);
                }
            }
        }


        #endregion

        #region SSH

        /// <summary>
        /// Hostnames for supported Hadoop cluster hosts, controller/docker-machine host always the first
        /// </summary>
        public static string[] SshHosts => new[] { "swtse143.informatik.uni-augsburg.de", "swtse144.informatik.uni-augsburg.de" };

        /// <summary>
        /// Usernames for supported Hadoop cluster hosts, controller/docker-machine host always the first
        /// </summary>
        public static string[] SshUsernames => new[] { "hadoop", "hadoop" };

        /// <summary>
        /// Full file path to the private key files to login, controller/docker-machine host always the first
        /// </summary>
        public static string[] SshPrivateKeyFiles => new[] { @"%USERPROFILE%\.ssh\id_rsa", @"%USERPROFILE%\.ssh\id_rsa" };

        #endregion

        #region Host mode

        /// <summary>
        /// The host mode of the Hadoop cluster on the cluster pc
        /// </summary>
        public static EHostMode HostMode { get; set; } = EHostMode.DockerMachine;

        /// <summary>
        /// Command for benchmark startup script on controller/docker-machine host
        /// </summary>
        /// <remarks>
        /// Generic options for all benchmark related commands can be inserted here.
        /// </remarks>
        public const string BenchmarkStartupScript = "~/hadoop-benchmark/bench.sh -q -t";

        /// <summary>
        /// The Hadoop setup script for use on classic singlehost
        /// mode with docker-machine VMs for the cluster
        /// </summary>
        /// <remarks>
        /// Generic options for all cluster related commands can be inserted here.
        /// </remarks>
        public const string HadoopSetupScriptDockerMachine = "~/hadoop-benchmark/setup.sh -q";

        /// <summary>
        /// The Hadoop setup script for use on multihost mode execution docker container
        /// directly on pc without docker-machine and in swarm mode.
        /// </summary>
        /// <remarks>
        /// Generic options for all cluster related commands can be inserted here.
        /// Must be on the same location on all hosts.
        /// </remarks>
        public const string HadoopSetupScriptMultihost = "~/hadoop-benchmark/multihost.sh -q";

        /// <summary>
        /// Command for Hadoop setup script that will be used in the model
        /// </summary>
        public static string HadoopSetupScript =>
            HostMode == EHostMode.DockerMachine ? HadoopSetupScriptDockerMachine : HadoopSetupScriptMultihost;

        /// <summary>
        /// Hadoop controller resource manager url for REST API on <see cref="SshHosts"/>
        /// on use with docker-machine cluster
        /// </summary>
        public const string ControllerRestRmUrlDockerMachine = "http://controller:8088";

        /// <summary>
        /// Hadoop controller resource manager url for REST API on <see cref="SshHosts"/>
        /// on use with multihost docker cluster
        /// </summary>
        public const string ControllerRestRmUrlMultihost = "http://localhost:8088";

        /// <summary>
        /// Hadoop controller resource manager url for REST API on <see cref="SshHosts"/>
        /// that will be used in the model
        /// </summary>
        public static string ControllerRestRmUrl =>
            HostMode == EHostMode.DockerMachine ? ControllerRestRmUrlDockerMachine : ControllerRestRmUrlMultihost;

        /// <summary>
        /// Hadoop controller timeline server url for REST API on <see cref="SshHosts"/>
        /// on use with docker-machine cluster
        /// </summary>
        public const string ControllerRestTlsUrlDockerMachine = "http://controller:8188";

        /// <summary>
        /// Hadoop controller timeline server url for REST API on <see cref="SshHosts"/>
        /// on use with multihost docker cluster
        /// </summary>
        public const string ControllerRestTlsUrlMultihost = "http://localhost:8188";

        /// <summary>
        /// Hadoop controller timeline server url for REST API on <see cref="SshHosts"/>
        /// that will be used in the model
        /// </summary>
        public static string ControllerRestTlsUrl =>
            HostMode == EHostMode.DockerMachine ? ControllerRestTlsUrlDockerMachine : ControllerRestTlsUrlMultihost;

        /// <summary>
        /// Hadoop compute node base http url (url without port number) for using on multihost cluster
        /// </summary>
        public const string NodeHttpUrlMultihostBase = "http://localhost";

        /// <summary>
        /// Base count for nodes on controller host.
        /// In multihost mode the node count on other hosts is the half of the count on controller host.
        /// </summary>
        public static int NodeBaseCount { get; set; } = 4;

        private static int _HostsCount = 1;

        /// <summary>
        /// The host count for multihost mode.
        /// Can only be set if <see cref="HostMode"/> is on Multihost, else the host count is always 1.
        /// </summary>
        public static int HostsCount
        {
            get { return _HostsCount; }
            set
            {
                if(HostMode == EHostMode.Multihost)
                    _HostsCount = value;
                else
                    _HostsCount = 1;
            }
        }

        #endregion

        #region Benchmarks

        private static bool _IsPrecreateBenchInputs;

        /// <summary>
        /// Base directory for precreated input data for benchmarks
        /// </summary>
        public const string PrecreateBenchInputsBaseDir = "InputData";

        /// <summary>
        /// Indicates if the precreated benchmark input data will be fully recreated, default false.
        /// For effects the value must be set before <see cref="IsPrecreateBenchInputs"/>.
        /// </summary>
        public static bool IsPrecreateBenchInputsRecreate { get; set; } = false;

        /// <summary>
        /// Indicates if the benchmark input data is precreated before model execution, default false.
        /// Precreated data will be saved into <see cref="PrecreateBenchInputsBaseDir"/> and
        /// used as input data for benchmarks. For full recreation of the input data
        /// <see cref="IsPrecreateBenchInputsRecreate"/> must be set before.
        /// </summary>
        public static bool IsPrecreateBenchInputs
        {
            get { return _IsPrecreateBenchInputs; }
            set
            {
                if(value)
                    BenchmarkController.PrecreateInputData(IsPrecreateBenchInputsRecreate);
                _IsPrecreateBenchInputs = value;
            }
        }

        #endregion

        #region Char array lengths

        public const int AppIdLength = 30;
        public const int AppNameLength = 0x7F;
        public const int AppAttemptIdLength = 36;
        public const int ContainerIdLength = 38;

        public const int HostNameLength = 10;
        public const int NodeIdLength = 16;
        public const int HttpUrlLength = 24;

        public const int ClientIdLength = 8;

        public const int TrackingUrlLength = 0x7F;
        public const int DiagnosticsLength = 0xFF;

        #endregion
    }
}