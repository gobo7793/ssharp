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
        IHadoopParser Parser { get; set; }

        /// <summary>
        /// Indicates if the data is collected and parsed by the component itself
        /// </summary>
        bool IsSelfMonitoring { get; set; }

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