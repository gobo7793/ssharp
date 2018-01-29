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
using System.Globalization;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using SafetySharp.CaseStudies.TestingHadoop.Modeling.HadoopModel;

namespace SafetySharp.CaseStudies.TestingHadoop.Modeling.Driver
{
    public static class DriverUtilities
    {
        #region Fields

        private static readonly Regex _ParseNodeRegex = new Regex(@"(https?:\/\/)?([^\:]+)(:\d*)?");
        private static readonly Regex _DigitRegex = new Regex(@"\d+");
        private static readonly Regex _DigitIdRegex = new Regex(@"\d+(?:\d|_)*\d+");

        private static readonly CultureInfo _DefaultDtCulture = new CultureInfo("en-US");

        #endregion

        #region Parsing

        /// <summary>
        /// Parses the <see cref="YarnNode"/> or returns null if node not found
        /// </summary>
        /// <param name="node">The node id or http-url</param>
        /// <param name="model">The model</param>
        /// <returns>The parsed <see cref="YarnNode"/></returns>
        public static YarnNode ParseNode(string node, Model model)
        {
            var nodeName = _ParseNodeRegex.Match(node).Groups[2].Value;
            if(!model.Nodes.ContainsKey(nodeName))
                return null;
            return model.Nodes[nodeName];
        }

        /// <summary>
        /// Parses the <see cref="EAppState"/> or returns the default value <see cref="EAppState.None"/>
        /// </summary>
        /// <param name="state">The state to parse</param>
        /// <returns>The parsed <see cref="EAppState"/></returns>
        public static EAppState ParseAppState(string state)
        {
            EAppState parsedState;
            Enum.TryParse(state, true, out parsedState);
            return parsedState;
        }

        /// <summary>
        /// Parses the <see cref="EContainerState"/> or returns the default value <see cref="EContainerState.None"/>
        /// </summary>
        /// <param name="state">The state to parse</param>
        /// <returns>The parsed <see cref="EContainerState"/></returns>
        public static EContainerState ParseContainerState(string state)
        {
            EContainerState parsedState;
            Enum.TryParse(state, true, out parsedState);
            return parsedState;
        }

        /// <summary>
        /// Parses the <see cref="ENodeState"/> or returns the default value <see cref="ENodeState.None"/>
        /// </summary>
        /// <param name="state">The state to parse</param>
        /// <returns>The parsed <see cref="ENodeState"/></returns>
        public static ENodeState ParseNodeState(string state)
        {
            ENodeState parsedState;
            Enum.TryParse(state, true, out parsedState);
            return parsedState;
        }

        /// <summary>
        /// Parses the <see cref="EFinalStatus"/> or returns the default value <see cref="EFinalStatus.None"/>
        /// </summary>
        /// <param name="finalStatus">The state to parse</param>
        /// <returns>The parsed <see cref="EFinalStatus"/></returns>
        public static EFinalStatus ParseFinalStatus(string finalStatus)
        {
            EFinalStatus parsedState;
            Enum.TryParse(finalStatus, true, out parsedState);
            return parsedState;
        }

        /// <summary>
        /// Parses the integer or returns the default value 0
        /// </summary>
        /// <param name="value">The value to parse</param>
        /// <returns>The parsed integer</returns>
        public static int ParseInt(string value)
        {
            int val;
            Int32.TryParse(value, out val);
            return val;
        }

        /// <summary>
        /// Parses an integer with leading or trailing text or returns the default value 0
        /// </summary>
        /// <param name="value">The value to parse</param>
        /// <returns>The progress</returns>
        public static int ParseIntText(string value)
        {
            return ParseInt(_DigitRegex.Match(value).Value);
        }

        /// <summary>
        /// Parses the timestamp with the given format to <see cref="DateTime"/>
        /// or returns the default value <see cref="DateTime.MinValue"/>
        /// </summary>
        /// <param name="javaMillis">The value to parse</param>
        /// <returns>The parsed <see cref="DateTime"/></returns>
        public static DateTime ParseJavaTimestamp(long javaMillis)
        {
            if(javaMillis == 0)
                return DateTime.MinValue;
            var javaTimeUtc = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(javaMillis);
            return javaTimeUtc.ToLocalTime();
        }

        /// <summary>
        /// Parses the timestamp with the given format to <see cref="DateTime"/>
        /// or returns the default value <see cref="DateTime.MinValue"/>
        /// </summary>
        /// <param name="javaMillis">The value to parse</param>
        /// <returns>The parsed <see cref="DateTime"/></returns>
        public static DateTime ParseJavaTimestamp(string javaMillis)
        {
            long javaMillisL;
            if(!Int64.TryParse(javaMillis, out javaMillisL))
                return DateTime.MinValue;
            return ParseJavaTimestamp(javaMillisL);
        }

        /// <summary>
        /// Parses the timestamp with the given format to <see cref="DateTime"/>
        /// or returns the default value <see cref="DateTime.MinValue"/>
        /// </summary>
        /// <param name="value">The value to parse</param>
        /// <param name="format">The time format for parsing</param>
        /// <param name="culture">The <see cref="CultureInfo"/> for parsing, default en-US</param>
        /// <returns>The parsed <see cref="DateTime"/></returns>
        public static DateTime ParseJavaTimestamp(string value, string format, CultureInfo culture = null)
        {
            culture = culture ?? _DefaultDtCulture;
            DateTime time;
            DateTime.TryParseExact(value, format, culture, DateTimeStyles.AssumeUniversal, out time);
            return time;
        }

        /// <summary>
        /// Concatinates the states to a comma seperated string
        /// </summary>
        /// <param name="states">The states</param>
        /// <returns>The comma seperated string</returns>
        public static string ConcatStates(EAppState states)
        {
            var appStates = String.Empty; // default return appStates by hadoop
            if(states != EAppState.None)
                appStates = states.ToString().Replace(" ", "");

            return appStates;
        }

        #endregion

        #region Converting

        /// <summary>
        /// Supported converting types
        /// </summary>
        public enum EConvertType
        {
            App,
            Attempt,
            Container
        }

        /// <summary>
        /// Get the numeric part of the ids, like
        /// "application_1516703400520_0003" -> "1516703400520_0003"
        /// </summary>
        /// <param name="fullId">the full id</param>
        /// <returns>The numeric part</returns>
        public static string GetNumericIdPart(string fullId)
        {
            return _DigitIdRegex.Match(fullId).Value;
        }

        /// <summary>
        /// Converts the given ID type to the target type.
        /// If the input id contains not enough data, the first attempt or container will be returned.
        /// </summary>
        /// <param name="inputId">The id to convert</param>
        /// <param name="targetType">The target type</param>
        /// <returns>The converted id</returns>
        /// <exception cref="ArgumentException">If the inputId is no valid source type id</exception>
        public static string ConvertId(string inputId, EConvertType targetType)
        {
            if(inputId.StartsWith("application"))
                return ConvertAppId(inputId, targetType);
            if(inputId.StartsWith("appattempt"))
                return ConvertAttemptId(inputId, targetType);
            if(inputId.StartsWith("container"))
                return ConvertContainerId(inputId, targetType);
            throw new ArgumentException($"{inputId} cannot be convert to {targetType}: source type not supported");
        }

        private static string ConvertAppId(string inputId, EConvertType targetType)
        {
            var matches = _DigitRegex.Matches(inputId);
            if(matches.Count != 2)
                throw new ArgumentException($"{inputId} cannot be convert to {targetType}: unvalid application id format");

            switch(targetType)
            {
                case EConvertType.Attempt:
                    return $"appattempt_{matches[0].Value}_{matches[1].Value}_000001";

                case EConvertType.Container:
                    return $"container_{matches[0].Value}_{matches[1].Value}_01_000001";

                default:
                    return inputId;
            }
        }

        private static string ConvertAttemptId(string inputId, EConvertType targetType)
        {
            var matches = _DigitRegex.Matches(inputId);
            if(matches.Count != 3)
                throw new ArgumentException($"{inputId} cannot be convert to {targetType}: unvalid appattempt id format");

            var attemptInt = ParseInt(matches[2].Value);

            switch(targetType)
            {
                case EConvertType.App:
                    return $"application_{matches[0].Value}_{matches[1].Value}";

                case EConvertType.Container:
                    return $"container_{matches[0].Value}_{matches[1].Value}_{attemptInt:D2}_000001";

                default:
                    return inputId;
            }
        }

        private static string ConvertContainerId(string inputId, EConvertType targetType)
        {
            var matches = _DigitRegex.Matches(inputId);
            if(matches.Count != 4)
                throw new ArgumentException($"{inputId} cannot be convert to {targetType}: unvalid container id format");

            var attemptInt = ParseInt(matches[2].Value);

            switch(targetType)
            {
                case EConvertType.App:
                    return $"application_{matches[0].Value}_{matches[1].Value}";

                case EConvertType.Attempt:
                    return $"appattempt_{matches[0].Value}_{matches[1].Value}_{attemptInt:D6}";

                default:
                    return inputId;
            }
        }

        /// <summary>
        /// Builds the full application id from an attempt id
        /// </summary>
        /// <param name="attemptId">The full attempt id</param>
        /// <returns>The full application id</returns>
        public static string BuildAppIdFromAttempt(string attemptId)
        {
            var id = $"application_{GetNumericIdPart(attemptId)}";
            return id.Substring(0, id.Length - 7);
        }

        /// <summary>
        /// Builds the full attempt id from an app id
        /// </summary>
        /// <param name="appId">The app id</param>
        /// <param name="shortAttemptId">Short attempt id</param>
        /// <returns>The full attempt id</returns>
        public static string BuildAttemptIdFromApp(string appId, int shortAttemptId)
        {
            return $"appattempt_{GetNumericIdPart(appId)}_{shortAttemptId:D6}";
        }

        /// <summary>
        /// Builds the basic container id from the full attempt id
        /// like "appattempt_1516703400520_0010_000001" -> "container_1516703400520_0010_01"_000001
        /// </summary>
        /// <param name="attemptId"></param>
        /// <returns>The base container id</returns>
        public static string BuildBaseContainerIdFromAttempt(string attemptId)
        {
            var numAttempt = GetNumericIdPart(attemptId);
            var conBaseNum = numAttempt.Remove(numAttempt.Length - 6, 4);
            return $"container_{conBaseNum}";
        }

        /// <summary>
        /// Builds the full attempt id from a container id
        /// </summary>
        /// <param name="containerId">The container id</param>
        /// <returns>The full attempt id</returns>
        public static string BuildAttemptIdFromContainer(string containerId)
        {
            var numContainerParts = GetNumericIdPart(containerId).Split('_');
            var appPart = $"{numContainerParts[0]}_{numContainerParts[1]}";
            var attemptPart = ParseInt(numContainerParts[2]);
            return BuildAttemptIdFromApp(appPart, attemptPart);
        }

        #endregion
    }

    #region Json

    /// <summary>
    /// Utility class to convert java epoch (ms since 1970-01-01 00:00:00) to <see cref="DateTime"/>
    /// </summary>
    /// <remarks>By mpen, https://stackoverflow.com/a/19972214 </remarks>
    public class JsonJavaEpochConverter : DateTimeConverterBase
    {
        private static readonly DateTime _Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteRawValue(((DateTime)value - _Epoch).TotalMilliseconds.ToString());
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return DriverUtilities.ParseJavaTimestamp((long)reader.Value);
        }
    }

    #endregion
}