using SafetySharp.CaseStudies.TestingHadoop.Modeling.Driver;

namespace SafetySharp.CaseStudies.TestingHadoop.Modeling.HadoopModel
{
    /// <summary>
    /// Interface for YARN components which can read its state from hadoop
    /// </summary>
    public interface IYarnReadable
    {
        /// <summary>
        /// Parser to use for monitoring
        /// </summary>
        IHadoopParser Parser { get; set; }

        /// <summary>
        /// Reads the current status from Hadoop
        /// </summary>
        void ReadStatus();

        /// <summary>
        /// Returns the current status as comma seperated string
        /// </summary>
        /// <returns>The status as string</returns>
        string StatusAsString();
    }
}