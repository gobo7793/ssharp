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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISSE.SafetyChecking.MarkovDecisionProcess
{
	internal class LtmdpStepGraphToContinuationDistributionMapper
	{
		public LtmdpContinuationDistributionMapperOld LtmdpContinuationDistributionMapper { get; } =
			new LtmdpContinuationDistributionMapperOld();

		private LtmdpStepGraph CurrentGraph { get; set; }

		public void Clear()
		{
			LtmdpContinuationDistributionMapper.Clear();
		}

		private void DeriveChoice(int cidOfChoice)
		{
			var choice = CurrentGraph.GetChoiceOfCid(cidOfChoice);
			if (choice.IsChoiceTypeUnsplitOrFinal)
				return;
			if (choice.IsChoiceTypeDeterministic ||
				choice.IsChoiceTypeNondeterministic)
			{
				LtmdpContinuationDistributionMapper.NonDeterministicSplit(cidOfChoice,choice.From,choice.To);
			}
			else if (choice.IsChoiceTypeProbabilitstic)
			{
				LtmdpContinuationDistributionMapper.ProbabilisticSplit(cidOfChoice, choice.From, choice.To);
			}

			for (var i = choice.From; i <= choice.To; i++)
			{
				DeriveChoice(i);
			}
		}

		public void Derive(LtmdpStepGraph graph)
		{
			CurrentGraph = graph;
			DeriveChoice(0);
		}
	}
}
