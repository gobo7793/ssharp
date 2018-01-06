namespace SafetySharp.CaseStudies.TestingHadoop.Modeling.HadoopModel
{
    /// <summary>
    /// Interface for YARN components which can read its state from hadoop
    /// </summary>
    public interface IYarnReadable
    {
        /// <summary>
        /// Reads the current status from Hadoop
        /// </summary>
        void GetStatus();
    }
}