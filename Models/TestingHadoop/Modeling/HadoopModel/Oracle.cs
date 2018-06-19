#region License
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
using System.Linq;
using SafetySharp.Modeling;

namespace SafetySharp.CaseStudies.TestingHadoop.Modeling.HadoopModel
{
    /// <summary>
    /// Checks and validates the constraints of the model.
    /// Constraints have to be saved as <see cref="Func{TResult}"/>[] where TResult is <see cref="Boolean"/>
    /// </summary>
    public static class Oracle
    {
        #region Properties

        [NonSerializable]
        private static log4net.ILog Logger { get; } =
            log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// The using controller
        /// </summary>
        [NonSerializable]
        private static YarnController Controller => Model.Instance.Controller;


        /// <summary>
        /// Checked SuT constraint count
        /// </summary>
        [NonSerializable]
        public static int SuTConstraintCheckedCount { get; private set; }

        /// <summary>
        /// Failed SuT constraint count
        /// </summary>
        [NonSerializable]
        public static int SuTConstraintFailedCount { get; private set; }

        /// <summary>
        /// Checked test constraint count
        /// </summary>
        [NonSerializable]
        public static int TestConstraintCheckedCount { get; private set; }

        /// <summary>
        /// Failed test constraint count
        /// </summary>
        [NonSerializable]
        public static int TestConstraintFailedCount { get; private set; }

        #endregion

        #region Utilities


        /// <summary>
        /// Resets the constraint counters
        /// </summary>
        public static void ResetCounters()
        {
            SuTConstraintCheckedCount = 0;
            SuTConstraintFailedCount = 0;
            TestConstraintCheckedCount = 0;
            TestConstraintFailedCount = 0;
        }

        /// <summary>
        /// Adds the constraint check to the counters
        /// </summary>
        private static void CountCheck(EConstraintType constraintType, bool isValid)
        {
            switch(constraintType)
            {
                case EConstraintType.Sut:
                    SuTConstraintCheckedCount++;
                    if(!isValid)
                        SuTConstraintFailedCount++;
                    break;
                case EConstraintType.Test:
                    TestConstraintCheckedCount++;
                    if(!isValid)
                        TestConstraintFailedCount++;
                    break;
            }
        }

        #endregion

        #region Checking Constraints

        /// <summary>
        /// Checks the constraints for all YARN components
        /// </summary>
        /// <param name="constraintType">Sets the constraint type to check</param>
        /// <returns>True if constraints are valid</returns>
        public static bool ValidateConstraints(EConstraintType constraintType)
        {
            Logger.Debug("Checking constraints");

            var isAllValid = true;
            if(constraintType == EConstraintType.Test)
            {
                if(!ValidateConstraints("Controller", Controller.TestConstraints, constraintType))
                    isAllValid = false;
            }

            foreach(var node in Controller.ConnectedNodes)
            {
                if(!ValidateConstraints(node, constraintType))
                    isAllValid = false;
            }

            foreach(var app in Controller.Apps)
            {
                if(!ValidateConstraints(app, constraintType))
                    isAllValid = false;

                foreach(var attempt in app.Attempts)
                {

                    if(!ValidateConstraints(attempt, constraintType))
                        isAllValid = false;

                    foreach(var container in attempt.Containers)
                    {
                        if(!ValidateConstraints(container, constraintType))
                            isAllValid = false;
                    }
                }
            }

            return isAllValid;
        }

        /// <summary>
        /// Validate the constraints for the given yarn component
        /// </summary>
        /// <param name="yarnComponent">The yarn component to validate</param>
        /// <param name="constraintType">Sets the constraint type to check</param>
        /// <returns>True if constraints are valid</returns>
        public static bool ValidateConstraints(IYarnReadable yarnComponent, EConstraintType constraintType)
        {
            var constraints = constraintType == EConstraintType.Sut ? yarnComponent.SutConstraints : yarnComponent.TestConstraints;
            var isComponentValid = ValidateConstraints(yarnComponent.GetId(), constraints, constraintType);
            return isComponentValid;
        }

        /// <summary>
        /// Validates the constraints and logs invalid constraints and returns if all constraints are valid
        /// </summary>
        /// <param name="componentId">The component ID or name for logging</param>
        /// <param name="constraints">The constraints to validate</param>
        /// <returns>True if constraints are valid</returns>
        public static bool ValidateConstraints(string componentId, Func<bool>[] constraints, EConstraintType constraintType)
        {
            var isCompontenValid = true;
            for(var i = 0; i < constraints.Length; i++)
            {
                var constraint = constraints[i];
                bool isValid;
                try
                {
                    isValid = constraint();
                }
                catch
                {
                    isValid = false;
                }

                CountCheck(constraintType, isValid);
                if(!isValid)
                {
                    Logger.Error($"YARN component not valid: Constraint {i} in {componentId}");
                    if(isCompontenValid)
                        isCompontenValid = false;
                }
            }

            return isCompontenValid;
        }

        #endregion

        #region Reconfiguration

        /// <summary>
        /// Validates if reconfiguration for the cluster would be possible
        /// </summary>
        /// <returns>True if reconfiguration is possible</returns>
        /// <exception cref="Exception">Throws an exception if reconfiguration is not possible</exception>
        public static bool IsReconfPossible()
        {
            Logger.Debug("Checking if reconfiguration is possible");

            var isReconfPossible = Controller.ConnectedNodes.Any(n => n.State == ENodeState.RUNNING);
            if(!isReconfPossible)
            {
                Logger.Error("No reconfiguration possible!");
                throw new Exception("No reconfiguration possible!");
            }

            return true;
        }

        #endregion
    }
}