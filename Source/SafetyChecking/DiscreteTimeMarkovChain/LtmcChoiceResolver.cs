// The MIT License (MIT)
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

namespace ISSE.SafetyChecking.DiscreteTimeMarkovChain
{
	using System.Collections.Generic;
	using System.Runtime.CompilerServices;
	using AnalysisModelTraverser;
	using ExecutableModel;
	using Modeling;
	using Utilities;
	using System;

	/// <summary>
	///   Represents a stack that is used to resolve nondeterministic choices during state space enumeration.
	/// </summary>
	internal sealed class LtmcChoiceResolver : ChoiceResolver
	{
		/// <summary>
		///   The number of nondeterministic choices that can be stored initially.
		/// </summary>
		private const int InitialCapacity = 64;
		
		/// <summary>
		///   The stack that indicates the chosen values for the current path.
		/// </summary>
		private readonly LtmcChosenValueStack _chosenValues = new LtmcChosenValueStack(InitialCapacity);

		/// <summary>
		///   The stack that stores the number of possible values of all encountered choices along the current path.
		/// </summary>
		private readonly ChoiceStack _valueCount = new ChoiceStack(InitialCapacity);

		/// <summary>
		///   The number of choices that have been encountered for the current path.
		/// </summary>
		private int _choiceIndex = -1;

		/// <summary>
		///   Indicates whether the next path is the first one of the current state.
		/// </summary>
		private bool _firstPath;

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		/// <param name="useForwardOptimization">Use Forward Optimization.</param>
		public LtmcChoiceResolver(bool useForwardOptimization)
				: base(useForwardOptimization)
		{
		}

		/// <summary>
		///   Gets the index of the last choice that has been made.
		/// </summary>
		// ReSharper disable once ConvertToAutoPropertyWithPrivateSetter
		internal override int LastChoiceIndex => _choiceIndex;

		/// <summary>
		///   Prepares the resolver for resolving the choices of the next state.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override void PrepareNextState()
		{
			_firstPath = true;
		}

		private Probability GetProbabilityOfPreviousPath()
		{
			if (_choiceIndex==-1 || _choiceIndex==0)
				return Probability.One;
			return _chosenValues[_choiceIndex - 1].Probability;
		}

		private Probability GetProbabilityUntilIndex(int index)
		{
			if (index == -1)
				return Probability.One;
			return _chosenValues[index].Probability;
		}


		/// <summary>
		///   Prepares the resolver for the next path. Returns <c>true</c> to indicate that all paths have been enumerated.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override bool PrepareNextPath()
		{
			if (_choiceIndex != _valueCount.Count - 1)
				throw new NondeterminismException();

			// Reset the choice counter as each path starts from the beginning
			_choiceIndex = -1;

			// If this is the first path of the state, we definitely have to enumerate it
			if (_firstPath)
			{
				_firstPath = false;
				return true;
			}

			// Let's go through the entire stack to determine what we have to do next
			while (_chosenValues.Count > 0)
			{
				// Remove the value we've chosen last -- we've already chosen it, so we're done with it
				var chosenValue = _chosenValues.Remove();

				// If we have at least one other value to choose, let's do that next
				if (_valueCount.Peek() > chosenValue.OptionIndex + 1)
				{
					var previousProbability = GetProbabilityUntilIndex(_valueCount.Count - 2);
					var valueCount = _valueCount.Peek();

					var newChosenValue =
						new LtmcChosenValue
						{
							OptionIndex = chosenValue.OptionIndex + 1,
							Probability = previousProbability / valueCount //placeholder value (for non deterministic choice)
						};
					_chosenValues.Push(newChosenValue);
					return true;
				}

				// Otherwise, we've chosen all values of the last choice, so we're done with it
				_valueCount.Remove();
			}

			// If we reach this point, we know that we've chosen all values of all choices, so there are no further paths
			return false;
		}

		/// <summary>
		///   Handles a nondeterministic choice that chooses between <paramref name="valueCount" /> values.
		/// </summary>
		/// <param name="valueCount">The number of values that can be chosen.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override int HandleChoice(int valueCount)
		{
			++_choiceIndex;

			// If we have a preselected value that we should choose for the current path, return it
			var chosenValuesMaxIndex = _chosenValues.Count - 1;
			if (_choiceIndex <= chosenValuesMaxIndex)
				return _chosenValues[_choiceIndex].OptionIndex;

			// We haven't encountered this choice before; store the value count and return the first value
			_valueCount.Push(valueCount);
			var newChosenValue =
				new LtmcChosenValue
				{
					OptionIndex = 0,
					Probability = GetProbabilityOfPreviousPath() / valueCount //placeholder value (for non deterministic choice)
				};
			_chosenValues.Push(newChosenValue);

			return 0;
		}
		
		/// <summary>
		///   Handles a probabilistic choice that chooses between two options.
		/// </summary>
		/// <param name="p0">The probability of option 0.</param>
		/// <param name="p1">The probability of option 1.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override int HandleProbabilisticChoice(Probability p0, Probability p1)
		{
			var choice = HandleChoice(2);
			if (choice == 0)
				SetProbabilityOfLastChoice(p0);
			else
				SetProbabilityOfLastChoice(p1);
			return choice;
		}

		/// <summary>
		///   Handles a probabilistic choice that chooses between three options.
		/// </summary>
		/// <param name="p0">The probability of option 0.</param>
		/// <param name="p1">The probability of option 1.</param>
		/// <param name="p2">The probability of option 2.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override int HandleProbabilisticChoice(Probability p0, Probability p1, Probability p2)
		{
			var choice = HandleChoice(3);
			switch (choice)
			{
				case 0:
					SetProbabilityOfLastChoice(p0);
					return choice;
				case 1:
					SetProbabilityOfLastChoice(p1);
					return choice;
				default:
					SetProbabilityOfLastChoice(p2);
					return choice;
			}
		}

		/// <summary>
		///   Handles a probabilistic choice that chooses between different options.
		/// </summary>
		/// <param name="p">Array with probabilities of each option.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override int HandleProbabilisticChoice(params Probability[] p)
		{
			var choice = HandleChoice(p.Length);
			SetProbabilityOfLastChoice(p[choice]);
			return choice;
		}

		/// <summary>
		///   Sets the probability of the taken option of the last taken probabilistic choice.
		/// </summary>
		/// <param name="probability">The probability of the last probabilistic choice.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void SetProbabilityOfLastChoice(Probability probability)
		{
			var probabilitiesOfChosenValuesMaxIndex = _chosenValues.Count - 1;
			// If this part of the path has previously been visited we do not change the value
			// because this value has already been set by a previous call of SetProbabilityOfLastChoice.
			// Only if we explore a new part of the path the probability needs to be written.
			// A new part of the path is explored, iff the previous HandleChoice pushed a new placeholder
			// value onto the three stacks).
			if (_choiceIndex == probabilitiesOfChosenValuesMaxIndex)
			{
				_chosenValues[_choiceIndex] =
					new LtmcChosenValue
					{
						OptionIndex = _chosenValues[_choiceIndex].OptionIndex,
						Probability = GetProbabilityOfPreviousPath() * probability
					};
			}
		}

		/// <summary>
		///   Makes taken choice identified by the <paramref name="choiceIndexToForward" /> deterministic.
		/// </summary>
		/// <param name="choiceIndexToForward">The index of the choice that should be undone.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal override void ForwardUntakenChoicesAtIndex(int choiceIndexToForward)
		{
			// This method is called when it is assumed, that choosing anything different at choiceIndex
			// leads to the same state as the path until the last choice.
			// Thus, we can simply revert this unnecessary choice and add the probability of the unmade alternatives
			// of the reverted choice to the last choice.
			// Note, very small numbers get multiplied and summarized. Maybe type double is too imprecise.

			if (_valueCount[choiceIndexToForward] == 0)
				return; //Nothing to do

			Assert.That(_chosenValues[choiceIndexToForward].OptionIndex==0, "Only first choice can be made deterministic.");

			// We disable a choice by setting the number of values that we have yet to choose to 0, effectively
			// turning the choice into a deterministic selection of the value at index 0

			var oldProbabilityUntilDeterministicChoice = GetProbabilityUntilIndex(choiceIndexToForward-1).Value;
			var oldProbabilityOfDeterministicChoice = GetProbabilityUntilIndex(choiceIndexToForward).Value;
			var differenceProbabilityToAdd = oldProbabilityUntilDeterministicChoice - oldProbabilityOfDeterministicChoice;
			
			var probabilityOfLastChoicePath = GetProbabilityUntilIndex(LastChoiceIndex).Value;
			
			var newValueOfLastChoice = (probabilityOfLastChoicePath + differenceProbabilityToAdd);

			// set the calculated value
			_chosenValues[LastChoiceIndex] =
				new LtmcChosenValue
				{
					OptionIndex = _chosenValues[LastChoiceIndex].OptionIndex,
					Probability = new Probability(newValueOfLastChoice)
				};

			// Set the alternatives to zero.
			_valueCount[choiceIndexToForward] = 0;
		}

		/// <summary>
		///   Sets the choices that should be made during the next step.
		/// </summary>
		/// <param name="choices">The choices that should be made.</param>
		internal override void SetChoices(int[] choices)
		{
			Requires.NotNull(choices, nameof(choices));

			foreach (var choice in choices)
			{
				var newChosenValue =
					new LtmcChosenValue
					{
						Probability = Probability.One,
						OptionIndex = choice
					};
				_chosenValues.Push(newChosenValue);
				_valueCount.Push(0);
			}
		}
		
		/// <summary>
		///	  The probability of the current path
		/// </summary>
		internal Probability CalculateProbabilityOfPath()
		{
			if (_choiceIndex == -1)
				return Probability.One;
			return _chosenValues[_choiceIndex].Probability;
		}

		/// <summary>
		///   Clears all choice information.
		/// </summary>
		internal override void Clear()
		{
			_chosenValues.Clear();
			_valueCount.Clear();
			_choiceIndex = -1;
		}

		/// <summary>
		///   Gets the choices that were made to generate the last transitions.
		/// </summary>
		internal override IEnumerable<int> GetChoices()
		{
			for (var i = 0; i < _chosenValues.Count; ++i)
				yield return _chosenValues[i].OptionIndex;
		}

		/// <summary>
		///   Disposes the object, releasing all managed and unmanaged resources.
		/// </summary>
		/// <param name="disposing">If true, indicates that the object is disposed; otherwise, the object is finalized.</param>
		protected override void OnDisposing(bool disposing)
		{
			if (!disposing)
				return;

			_chosenValues.SafeDispose();
			_valueCount.SafeDispose();
		}
	}
}