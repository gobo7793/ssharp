﻿// The MIT License (MIT)
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

namespace Tests.Analysis.Invariants.CounterExamples
{
	using SafetySharp.Modeling;
	using SafetySharp.Runtime;
	using Shouldly;

	internal class Stepping : AnalysisTestObject
	{
		protected override void Check()
		{
			const int start = 2;
			const int steps = 40;

			var c = new C { X = start };
			CheckInvariant(c.X != start + steps, c);
			CounterExample.StepCount.ShouldBe(steps + 1);

			var simulator = new Simulator(CounterExample);
			c = (C)simulator.Model.RootComponents[0];

			c.X.ShouldBe(start);

			simulator.FastForward(10);
			c.X.ShouldBe(start + 10);

			simulator.FastForward(600);
			c.X.ShouldBe(start + steps);

			simulator.Rewind(10);
			c.X.ShouldBe(start + steps - 10);

			simulator.Rewind(100);
			c.X.ShouldBe(start);
		}

		private class C : Component
		{
			public int X;

			public override void Update()
			{
				++X;
			}
		}
	}
}