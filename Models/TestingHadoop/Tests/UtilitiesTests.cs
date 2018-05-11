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

using System;
using NUnit.Framework;
using SafetySharp.CaseStudies.TestingHadoop.Modeling;
using SafetySharp.CaseStudies.TestingHadoop.Modeling.Driver;
using SafetySharp.CaseStudies.TestingHadoop.Modeling.Driver.Parser;
using SafetySharp.CaseStudies.TestingHadoop.Modeling.HadoopModel;

namespace SafetySharp.CaseStudies.TestingHadoop.Tests
{
    [TestFixture]
    public class UtilitiesTests
    {
        [Test]
        [TestCase("Wed Jan 10 19:42:01 +0000 2018", CmdParser.HadoopDateFormat, ExpectedResult = "2018-01-10T20:42:01.0000000+01:00")]
        public string TestParseTimestamp(string date, string format)
        {
            return DriverUtilities.ParseJavaTimestamp(date, format).ToString("o");
        }

        [Test]
        [TestCase("1512187108523", ExpectedResult = "2017-12-02T04:58:28.5230000+01:00")]
        [TestCase("0", ExpectedResult = "0001-01-01T00:00:00.0000000")]
        public string TestParseTimestamp(string millis)
        {
            return DriverUtilities.ParseJavaTimestamp(millis).ToString("o");
        }

        [Test]
        [TestCase("application_1517215519416_0010", EConvertType.Attempt, ExpectedResult = "appattempt_1517215519416_0010")]
        [TestCase("application_1517215519416_0012", EConvertType.Container, ExpectedResult = "container_1517215519416_0012")]
        [TestCase("appattempt_1517215519416_0007_000001", EConvertType.App, ExpectedResult = "application_1517215519416_0007")]
        [TestCase("appattempt_1517215519416_0020_000003", EConvertType.Container, ExpectedResult = "container_1517215519416_0020_03")]
        [TestCase("container_1517215519416_0002_01_000007", EConvertType.App, ExpectedResult = "application_1517215519416_0002")]
        [TestCase("container_1517215519416_0006_02_000017", EConvertType.Attempt, ExpectedResult = "appattempt_1517215519416_0006_000002")]
        public string TestConvert(string input, EConvertType targetType)
        {
            return DriverUtilities.ConvertId(input, targetType);
        }

        [Test]
        [TestCase("application_1517215519416_0010", 2, EConvertType.Attempt, ExpectedResult = "appattempt_1517215519416_0010_000002")]
        [TestCase("application_1517215519416_0012", 4, EConvertType.Container, ExpectedResult = "container_1517215519416_0012_04")]
        [TestCase("appattempt_1517215519416_0007_000001", 5, EConvertType.App, ExpectedResult = "application_1517215519416_0007")]
        [TestCase("appattempt_1517215519416_0020_000003", 3, EConvertType.Container, ExpectedResult = "container_1517215519416_0020_03_000003")]
        [TestCase("container_1517215519416_0002_01_000007", 6, EConvertType.App, ExpectedResult = "application_1517215519416_0002")]
        [TestCase("container_1517215519416_0006_02_000017", 1, EConvertType.Attempt, ExpectedResult = "appattempt_1517215519416_0006_000002")]
        public string TestConvert(string input, int shortId, EConvertType targetType)
        {
            return DriverUtilities.ConvertId(input, shortId, targetType);
        }

        [Test]
        [TestCase(1, 4, ExpectedResult = 4)]
        [TestCase(2, 4, ExpectedResult = 6)]
        [TestCase(1, 2, ExpectedResult = 2)]
        [TestCase(2, 2, ExpectedResult = 3)]
        [TestCase(3, 4, ExpectedResult = 8)]
        [TestCase(4, 6, ExpectedResult = 15)]
        public int TestModelGetFullNodeCount(int hostsCount, int nodeBaseCount)
        {
            Model.HostsCount = hostsCount;
            Model.NodeBaseCount = nodeBaseCount;
            return ModelUtilities.GetFullNodeCount();
        }

        [Test]
        [TestCase(1, 4, ExpectedResult = 4)]
        [TestCase(2, 4, ExpectedResult = 6)]
        [TestCase(1, 2, ExpectedResult = 2)]
        [TestCase(2, 2, ExpectedResult = 3)]
        [TestCase(3, 4, ExpectedResult = 8)]
        [TestCase(4, 6, ExpectedResult = 15)]
        public int TestDriverGetFullNodeCount(int hostsCount, int nodeBaseCount)
        {
            return DriverUtilities.GetFullNodeCount(hostsCount, nodeBaseCount);
        }

        [Test]
        [TestCase(1, 4, 1, ExpectedResult = 1)]
        [TestCase(1, 4, 3, ExpectedResult = 1)]
        [TestCase(2, 4, 1, ExpectedResult = 1)]
        [TestCase(2, 4, 4, ExpectedResult = 1)]
        [TestCase(2, 4, 5, ExpectedResult = 2)]
        [TestCase(1, 2, 2, ExpectedResult = 1)]
        [TestCase(2, 2, 1, ExpectedResult = 1)]
        [TestCase(2, 2, 3, ExpectedResult = 2)]
        [TestCase(3, 4, 4, ExpectedResult = 1)]
        [TestCase(3, 4, 6, ExpectedResult = 2)]
        [TestCase(3, 4, 7, ExpectedResult = 3)]
        [TestCase(3, 4, 8, ExpectedResult = 3)]
        [TestCase(4, 6, 6, ExpectedResult = 1)]
        [TestCase(4, 6, 7, ExpectedResult = 2)]
        [TestCase(4, 6, 9, ExpectedResult = 2)]
        [TestCase(4, 6, 10, ExpectedResult = 3)]
        [TestCase(4, 6, 13, ExpectedResult = 4)]
        [TestCase(4, 6, 16, ExpectedException = typeof(ArgumentOutOfRangeException))]
        public int TestGetHostId(int hostsCount, int nodeBaseCount, int nodeId)
        {
            return DriverUtilities.GetHostId(nodeId, hostsCount, nodeBaseCount);
        }
    }
}