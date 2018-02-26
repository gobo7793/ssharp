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
        /// Indicates if details parsing is required for full informations
        /// </summary>
        bool IsRequireDetailsParsing { get; set; }

        /// <summary>
        /// Reads the current status from Hadoop
        /// </summary>
        void ReadStatus();

        /// <summary>
        /// Sets the status based on the parsed component
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