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
        /// S# analysis/DCCA constraints for the oracle
        /// </summary>
        Func<bool>[] Constraints { get; }

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