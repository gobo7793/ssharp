﻿// The MIT License (MIT)
// 
// Copyright (c) 2014-2015, Institute for Software & Systems Engineering
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

namespace Tests.Diagnostics.PortKinds.Invalid
{
	using SafetySharp.Compiler.Analyzers;
	using SafetySharp.Modeling;

	[Diagnostic(DiagnosticIdentifier.IndexerPort, 31, 28, 4, "Tests.Diagnostics.PortKinds.Invalid.IndexerPort1.this[int]")]
	internal class IndexerPort1 : Component
	{
		private extern int this[int i] { get; }
	}

	[Diagnostic(DiagnosticIdentifier.IndexerPort, 38, 13, 4, "Tests.Diagnostics.PortKinds.Invalid.IIndexerPort1.this[int]")]
	internal interface IIndexerPort1 : IComponent
	{
		[Required]
		int this[int i] { get; }
	}

	[Diagnostic(DiagnosticIdentifier.IndexerPort, 44, 21, 4, "Tests.Diagnostics.PortKinds.Invalid.IndexerPort2.this[int]")]
	internal class IndexerPort2 : Component
	{
		private int this[int i] => 1;
	}

	[Diagnostic(DiagnosticIdentifier.IndexerPort, 51, 13, 4, "Tests.Diagnostics.PortKinds.Invalid.IIndexerPort2.this[int]")]
	internal interface IIndexerPort2 : IComponent
	{
		[Provided]
		int this[int i] { get; }
	}
}