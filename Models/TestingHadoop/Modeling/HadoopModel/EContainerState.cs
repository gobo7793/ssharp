// The MIT License (MIT)
// 
// Copyright (c) 2014-2016, Institute for Software & Systems Engineering
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

namespace SafetySharp.CaseStudies.TestingHadoop.Modeling.HadoopModel
{
    /// <summary>
    /// Status of <see cref="YarnAppContainer"/>
    /// </summary>
    /// <remarks>
    /// State of the container - valid states are:
    ///     NEW, LOCALIZING, LOCALIZATION_FAILED, LOCALIZED, RUNNING,
    ///     EXITED_WITH_SUCCESS, EXITED_WITH_FAILURE, KILLING,
    ///     CONTAINER_CLEANEDUP_AFTER_KILL, CONTAINER_RESOURCES_CLEANINGUP, DONE
    /// TL: COMPLETE
    /// 
    /// via http://hadoop.apache.org/docs/r2.7.1/hadoop-yarn/hadoop-yarn-site/NodeManagerRest.html#Container_API
    /// and http://hadoop.apache.org/docs/r2.7.1/hadoop-yarn/hadoop-yarn-site/TimelineServer.html#GENERIC_DATA_REST_APIS
    /// </remarks>
    public enum EContainerState
    {
        None,
        NEW,
        LOCALIZING,
        LOCALIZATION_FAILED,
        LOCALIZED,
        RUNNING,
        EXITED_WITH_SUCCESS,
        EXITED_WITH_FAILURE,
        KILLING,
        CONTAINER_CLEANEDUP_AFTER_KILL,
        CONTAINER_RESOURCES_CLEANINGUP,
        DONE,
        COMPLETE
    }
}