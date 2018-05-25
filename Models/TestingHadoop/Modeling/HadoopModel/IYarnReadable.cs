using System;
using System.Collections.Generic;
using SafetySharp.CaseStudies.TestingHadoop.Modeling.Driver;

namespace SafetySharp.CaseStudies.TestingHadoop.Modeling.HadoopModel
{
    /// <summary>
    /// Interface for YARN components which can read its state from hadoop
    /// </summary>
    public interface IYarnReadable
    {
        /// <summary>
        /// Parser to monitoring data from cluster
        /// </summary>
        IHadoopParser Parser { get; }

        /// <summary>
        /// Indicates if the data is collected and parsed by the component itself
        /// </summary>
        bool IsSelfMonitoring { get; set; }

        /// <summary>
        /// S# constraints for the oracle based on requirement for the SuT
        /// </summary>
        Func<bool>[] SutConstraints { get; }

        /// <summary>
        /// Constraints to check the requirements of the test suite itself
        /// </summary>
        Func<bool>[] TestConstraints { get; }

        /// <summary>
        /// Returns the ID of the component
        /// </summary>
        string GetId();

        /// <summary>
        /// Monitors the current status from Hadoop
        /// </summary>
        void MonitorStatus();

        /// <summary>
        /// Sets the status based on the given <see cref="IParsedComponent"/>
        /// </summary>
        /// <param name="parsed">The parsed component</param>
        void SetStatus(IParsedComponent parsed);

        /// <summary>
        /// Returns the current status as comma seperated string
        /// </summary>
        /// <returns>The status as string</returns>
        string StatusAsString();
    }
}