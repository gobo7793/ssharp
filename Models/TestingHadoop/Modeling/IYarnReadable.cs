using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SafetySharp.CaseStudies.TestingHadoop
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