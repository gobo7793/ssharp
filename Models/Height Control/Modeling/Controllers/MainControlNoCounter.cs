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

namespace SafetySharp.CaseStudies.HeightControl.Modeling.Controllers
{
	public sealed class MainControlNoCounter : MainControl
	{
		/// <summary>
		///   Invoked when the given number of vehicles enters the main-control area.
		/// </summary>
		public override void VehiclesEntering(int vehicleCount)
		{
			Timer.Start();
		}

		/// <summary>
		///   Updates the internal state of the component.
		/// </summary>
		public override void Update()
		{
			base.Update();

			var onlyRightTriggered = !LeftDetector.IsVehicleDetected && RightDetector.IsVehicleDetected;

			// We assume the worst case: If the vehicle was not seen on the right lane, it is assumed to be on the left lane
			if (Timer.IsActive && PositionDetector.IsVehicleDetected && !onlyRightTriggered)
				CloseTunnel();

			if (Timer.IsActive && PositionDetector.IsVehicleDetected && onlyRightTriggered)
				ActivateEndControl();
		}
	}
}