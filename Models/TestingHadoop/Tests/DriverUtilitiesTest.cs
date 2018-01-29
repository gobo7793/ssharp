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

using NUnit.Framework;
using SafetySharp.CaseStudies.TestingHadoop.Modeling.Driver;

namespace SafetySharp.CaseStudies.TestingHadoop.Tests
{
    [TestFixture]
    public class DriverUtilitiesTest
    {
        [Test]
        [TestCase("Wed Jan 10 19:42:01 +0000 2018", CmdLineParser.HadoopDateFormat, Result = "2018-01-10T20:42:01.0000000+01:00")]
        //[TestCase("1512187108523", null, Result = "2017-12-02T04:58:28.5230000+01:00")]
        //[TestCase("0", null, Result = "0001-01-01T00:00:00.0000000")]
        public string TestParseTimestamp(string date, string format)
        {
            return DriverUtilities.ParseJavaTimestamp(date, format).ToString("o");
        }

        [Test]
        [TestCase("application_1517215519416_0010", DriverUtilities.EConvertType.Attempt, Result = "appattempt_1517215519416_0010_000001")]
        [TestCase("application_1517215519416_0012", DriverUtilities.EConvertType.Container, Result = "container_1517215519416_0012_01_000001")]
        [TestCase("appattempt_1517215519416_0007_000001", DriverUtilities.EConvertType.App, Result = "application_1517215519416_0007")]
        [TestCase("appattempt_1517215519416_0020_000003", DriverUtilities.EConvertType.Container, Result = "container_1517215519416_0020_03_000001")]
        [TestCase("container_1517215519416_0002_01_000007", DriverUtilities.EConvertType.App, Result = "application_1517215519416_0002")]
        [TestCase("container_1517215519416_0006_02_000017", DriverUtilities.EConvertType.Attempt, Result = "appattempt_1517215519416_0006_000002")]
        public string TestConvert(string input, DriverUtilities.EConvertType targetType)
        {
            return DriverUtilities.ConvertId(input, targetType);
        }
    }
}