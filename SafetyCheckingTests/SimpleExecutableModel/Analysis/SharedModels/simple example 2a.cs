﻿// The MIT License (MIT)
// 
// Copyright (c) 2014-2017, Institute for Software & Systems Engineering
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

namespace Tests.SimpleExecutableModel.SharedModels
{
	using System;
	using ISSE.SafetyChecking.DiscreteTimeMarkovChain;
	using ISSE.SafetyChecking.ExecutedModel;
	using ISSE.SafetyChecking.Formula;
	using ISSE.SafetyChecking.Modeling;
	using Shouldly;
	using Utilities;
	using Xunit;
	using Xunit.Abstractions;
	
	
	public class SimpleExample2a : SimpleModelBase
	{
		public override Fault[] Faults { get; } = new Fault[0];
		public override bool[] LocalBools { get; } = { false };
		public override int[] LocalInts { get; } = new int[0];

		public static Formula StateIs0 = new SimpleStateInRangeFormula(0,"StateIs0");
		public static Formula StateIs1 = new SimpleStateInRangeFormula(1, "StateIs1");
		public static Formula StateIs2 = new SimpleStateInRangeFormula(2, "StateIs2");
		public static Formula LocalVarIsTrue = new SimpleLocalVarIsTrue(0, "LIs0");

		private bool L
		{
			get { return LocalBools[0]; }
			set { LocalBools[0] = value; }
		}

		private int Y
		{
			get { return State; }
			set { State = value; }
		}

		public override void SetInitialState()
		{
			State = 0;
		}

		public override void Update()
		{
			L = Choice.Choose(true, false);
			if (L && Y==0)
			{
				L = Choice.Choose(
					new Option<bool>(new Probability(0.6), true),
					new Option<bool>(new Probability(0.4), false));
				if (L)
				{
				}
				else
				{
					Y = Choice.Choose(1, 2);
				}
			}
			else
			{
				L = Choice.Choose(false, true);
			}
		}
	}
}