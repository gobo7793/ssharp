# The MIT License (MIT)
# 
# Copyright (c) 2014-2016, Institute for Software & Systems Engineering
# 
# Permission is hereby granted, free of charge, to any person obtaining a copy
# of this software and associated documentation files (the "Software"), to deal
# in the Software without restriction, including without limitation the rights
# to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
# copies of the Software, and to permit persons to whom the Software is
# furnished to do so, subject to the following conditions:
# 
# The above copyright notice and this permission notice shall be included in
# all copies or substantial portions of the Software.
# 
# THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
# IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
# FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
# AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
# LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
# OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
# THE SOFTWARE.
#
# Sources:
#    * https://www.nunit.org/index.php?p=guiCommandLine&r=2.4
#    * https://www.nunit.org/index.php?p=nunit-console&r=2.4
#    * https://msdn.microsoft.com/en-us/powershell/scripting/getting-started/cookbooks/managing-current-location
#    * https://msdn.microsoft.com/en-us/powershell/reference/5.1/microsoft.powershell.management/start-process
#    * https://www.safaribooksonline.com/library/view/windows-powershell-cookbook/9780596528492/ch11s02.html
#    * http://www.get-blog.com/?p=82

# Note: You must run the following command first
#  Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
# To Undo
#  Set-ExecutionPolicy -ExecutionPolicy Restricted -Scope CurrentUser

# It is easy to get the method names in an assembly by extracting them from TestResult.xml which is generated by nunit.exe


New-Variable -Force -Name tests -Option AllScope -Value @()

function AddTest($testName, $testAssembly, $testMethod, $testCategory ="")
{
    $newTest = New-Object System.Object
    $newTest | Add-Member -type NoteProperty -name TestName -Value $testName
    $newTest | Add-Member -type NoteProperty -name TestAssembly -Value $testAssembly
    $newTest | Add-Member -type NoteProperty -name TestMethod -Value $testMethod
    $newTest | Add-Member -type NoteProperty -name TestCategory -Value $testCategory
    $tests += $newTest
}

###############################################
# Pressure Tank
###############################################

#AddTest -Testname "PressureTank_Probability_HazardIsDepleted" -TestAssembly "SafetySharp.CaseStudies.PressureTank.dll" -TestMethod "SafetySharp.CaseStudies.PressureTank.Analysis.HazardProbabilityTests.CalculateHazardIsDepleted"



###############################################
# Height Control
###############################################

# Original-Original-Original
AddTest -Testname "HeightControl_Original-Original-Original_Probability_HazardCollision" -TestAssembly "SafetySharp.CaseStudies.HeightControl.dll" -TestMethod "SafetySharp.CaseStudies.HeightControl.Analysis.HazardProbabilityTests.Original-Original-Original" -TestCategory "CollisionProbability"
AddTest -Testname "HeightControl_Original-Original-Original_Probability_HazardFalseAlarm" -TestAssembly "SafetySharp.CaseStudies.HeightControl.dll" -TestMethod "SafetySharp.CaseStudies.HeightControl.Analysis.HazardProbabilityTests.Original-Original-Original" -TestCategory "FalseAlarmProbability"
AddTest -Testname "HeightControl_Original-Original-Original_DCCA_HazardCollision" -TestAssembly "SafetySharp.CaseStudies.HeightControl.dll" -TestMethod "SafetySharp.CaseStudies.HeightControl.Analysis.ModelCheckingTests.Original-Original-Original" -TestCategory "CollisionDCCA"
AddTest -Testname "HeightControl_Original-Original-Original_DCCA_HazardFalseAlarm" -TestAssembly "SafetySharp.CaseStudies.HeightControl.dll" -TestMethod "SafetySharp.CaseStudies.HeightControl.Analysis.ModelCheckingTests.Original-Original-Original" -TestCategory "FalseAlarmDCCA"

## OverheadDetectors-Tolerant-LightBarrier
#AddTest -Testname "HeightControl_OverheadDetectors-Tolerant-LightBarrier_Probability_HazardCollision" -TestAssembly "SafetySharp.CaseStudies.HeightControl.dll" -TestMethod "SafetySharp.CaseStudies.HeightControl.Analysis.HazardProbabilityTests.OverheadDetectors-Tolerant-LightBarrier" -TestCategory "CollisionProbability"
#AddTest -Testname "HeightControl_OverheadDetectors-Tolerant-LightBarrier_Probability_HazardFalseAlarm" -TestAssembly "SafetySharp.CaseStudies.HeightControl.dll" -TestMethod "SafetySharp.CaseStudies.HeightControl.Analysis.HazardProbabilityTests.OverheadDetectors-Tolerant-LightBarrier" -TestCategory "FalseAlarmProbability"
#AddTest -Testname "HeightControl_OverheadDetectors-Tolerant-LightBarrier_DCCA_HazardCollision" -TestAssembly "SafetySharp.CaseStudies.HeightControl.dll" -TestMethod "SafetySharp.CaseStudies.HeightControl.Analysis.ModelCheckingTests.OverheadDetectors-Tolerant-LightBarrier" -TestCategory "CollisionDCCA"
#AddTest -Testname "HeightControl_OverheadDetectors-Tolerant-LightBarrier_DCCA_HazardFalseAlarm" -TestAssembly "SafetySharp.CaseStudies.HeightControl.dll" -TestMethod "SafetySharp.CaseStudies.HeightControl.Analysis.ModelCheckingTests.OverheadDetectors-Tolerant-LightBarrier" -TestCategory "FalseAlarmDCCA"
#
## OverheadDetectors-Tolerant-Original
#AddTest -Testname "HeightControl_OverheadDetectors-Tolerant-Original_Probability_HazardCollision" -TestAssembly "SafetySharp.CaseStudies.HeightControl.dll" -TestMethod "SafetySharp.CaseStudies.HeightControl.Analysis.HazardProbabilityTests.OverheadDetectors-Tolerant-Original" -TestCategory "CollisionProbability"
#AddTest -Testname "HeightControl_OverheadDetectors-Tolerant-Original_Probability_HazardFalseAlarm" -TestAssembly "SafetySharp.CaseStudies.HeightControl.dll" -TestMethod "SafetySharp.CaseStudies.HeightControl.Analysis.HazardProbabilityTests.OverheadDetectors-Tolerant-Original" -TestCategory "FalseAlarmProbability"
#AddTest -Testname "HeightControl_OverheadDetectors-Tolerant-Original_DCCA_HazardCollision" -TestAssembly "SafetySharp.CaseStudies.HeightControl.dll" -TestMethod "SafetySharp.CaseStudies.HeightControl.Analysis.ModelCheckingTests.OverheadDetectors-Tolerant-Original" -TestCategory "CollisionDCCA"
#AddTest -Testname "HeightControl_OverheadDetectors-Tolerant-Original_DCCA_HazardFalseAlarm" -TestAssembly "SafetySharp.CaseStudies.HeightControl.dll" -TestMethod "SafetySharp.CaseStudies.HeightControl.Analysis.ModelCheckingTests.OverheadDetectors-Tolerant-Original" -TestCategory "FalseAlarmDCCA"
#
## OverheadDetectors-Original-LightBarrier
#AddTest -Testname "HeightControl_OverheadDetectors-Original-LightBarrier_Probability_HazardCollision" -TestAssembly "SafetySharp.CaseStudies.HeightControl.dll" -TestMethod "SafetySharp.CaseStudies.HeightControl.Analysis.HazardProbabilityTests.OverheadDetectors-Original-LightBarrier" -TestCategory "CollisionProbability"
#AddTest -Testname "HeightControl_OverheadDetectors-Original-LightBarrier_Probability_HazardFalseAlarm" -TestAssembly "SafetySharp.CaseStudies.HeightControl.dll" -TestMethod "SafetySharp.CaseStudies.HeightControl.Analysis.HazardProbabilityTests.OverheadDetectors-Original-LightBarrier" -TestCategory "FalseAlarmProbability"
#AddTest -Testname "HeightControl_OverheadDetectors-Original-LightBarrier_DCCA_HazardCollision" -TestAssembly "SafetySharp.CaseStudies.HeightControl.dll" -TestMethod "SafetySharp.CaseStudies.HeightControl.Analysis.ModelCheckingTests.OverheadDetectors-Original-LightBarrier" -TestCategory "CollisionDCCA"
#AddTest -Testname "HeightControl_OverheadDetectors-Original-LightBarrier_DCCA_HazardFalseAlarm" -TestAssembly "SafetySharp.CaseStudies.HeightControl.dll" -TestMethod "SafetySharp.CaseStudies.HeightControl.Analysis.ModelCheckingTests.OverheadDetectors-Original-LightBarrier" -TestCategory "FalseAlarmDCCA"
#
## OverheadDetectors-Original-Original
#AddTest -Testname "HeightControl_OverheadDetectors-Original-Original_Probability_HazardCollision" -TestAssembly "SafetySharp.CaseStudies.HeightControl.dll" -TestMethod "SafetySharp.CaseStudies.HeightControl.Analysis.HazardProbabilityTests.OverheadDetectors-Original-Original" -TestCategory "CollisionProbability"
#AddTest -Testname "HeightControl_OverheadDetectors-Original-Original_Probability_HazardFalseAlarm" -TestAssembly "SafetySharp.CaseStudies.HeightControl.dll" -TestMethod "SafetySharp.CaseStudies.HeightControl.Analysis.HazardProbabilityTests.OverheadDetectors-Original-Original" -TestCategory "FalseAlarmProbability"
#AddTest -Testname "HeightControl_OverheadDetectors-Original-Original_DCCA_HazardCollision" -TestAssembly "SafetySharp.CaseStudies.HeightControl.dll" -TestMethod "SafetySharp.CaseStudies.HeightControl.Analysis.ModelCheckingTests.OverheadDetectors-Original-Original" -TestCategory "CollisionDCCA"
#AddTest -Testname "HeightControl_OverheadDetectors-Original-Original_DCCA_HazardFalseAlarm" -TestAssembly "SafetySharp.CaseStudies.HeightControl.dll" -TestMethod "SafetySharp.CaseStudies.HeightControl.Analysis.ModelCheckingTests.OverheadDetectors-Original-Original" -TestCategory "FalseAlarmDCCA"
#
## Original-NoCounterTolerant-LightBarrier
#AddTest -Testname "HeightControl_Original-NoCounterTolerant-LightBarrier_Probability_HazardCollision" -TestAssembly "SafetySharp.CaseStudies.HeightControl.dll" -TestMethod "SafetySharp.CaseStudies.HeightControl.Analysis.HazardProbabilityTests.Original-NoCounterTolerant-LightBarrier" -TestCategory "CollisionProbability"
#AddTest -Testname "HeightControl_Original-NoCounterTolerant-LightBarrier_Probability_HazardFalseAlarm" -TestAssembly "SafetySharp.CaseStudies.HeightControl.dll" -TestMethod "SafetySharp.CaseStudies.HeightControl.Analysis.HazardProbabilityTests.Original-NoCounterTolerant-LightBarrier" -TestCategory "FalseAlarmProbability"
#AddTest -Testname "HeightControl_Original-NoCounterTolerant-LightBarrier_DCCA_HazardCollision" -TestAssembly "SafetySharp.CaseStudies.HeightControl.dll" -TestMethod "SafetySharp.CaseStudies.HeightControl.Analysis.ModelCheckingTests.Original-NoCounterTolerant-LightBarrier" -TestCategory "CollisionDCCA"
#AddTest -Testname "HeightControl_Original-NoCounterTolerant-LightBarrier_DCCA_HazardFalseAlarm" -TestAssembly "SafetySharp.CaseStudies.HeightControl.dll" -TestMethod "SafetySharp.CaseStudies.HeightControl.Analysis.ModelCheckingTests.Original-NoCounterTolerant-LightBarrier" -TestCategory "FalseAlarmDCCA"
#
## Original-NoCounterTolerant-Original
#AddTest -Testname "HeightControl_Original-NoCounterTolerant-Original_Probability_HazardCollision" -TestAssembly "SafetySharp.CaseStudies.HeightControl.dll" -TestMethod "SafetySharp.CaseStudies.HeightControl.Analysis.HazardProbabilityTests.Original-NoCounterTolerant-Original" -TestCategory "CollisionProbability"
#AddTest -Testname "HeightControl_Original-NoCounterTolerant-Original_Probability_HazardFalseAlarm" -TestAssembly "SafetySharp.CaseStudies.HeightControl.dll" -TestMethod "SafetySharp.CaseStudies.HeightControl.Analysis.HazardProbabilityTests.Original-NoCounterTolerant-Original" -TestCategory "FalseAlarmProbability"
#AddTest -Testname "HeightControl_Original-NoCounterTolerant-Original_DCCA_HazardCollision" -TestAssembly "SafetySharp.CaseStudies.HeightControl.dll" -TestMethod "SafetySharp.CaseStudies.HeightControl.Analysis.ModelCheckingTests.Original-NoCounterTolerant-Original" -TestCategory "CollisionDCCA"
#AddTest -Testname "HeightControl_Original-NoCounterTolerant-Original_DCCA_HazardFalseAlarm" -TestAssembly "SafetySharp.CaseStudies.HeightControl.dll" -TestMethod "SafetySharp.CaseStudies.HeightControl.Analysis.ModelCheckingTests.Original-NoCounterTolerant-Original" -TestCategory "FalseAlarmDCCA"
#
## Original-NoCounter-LightBarrier
#AddTest -Testname "HeightControl_Original-NoCounter-LightBarrier_Probability_HazardCollision" -TestAssembly "SafetySharp.CaseStudies.HeightControl.dll" -TestMethod "SafetySharp.CaseStudies.HeightControl.Analysis.HazardProbabilityTests.Original-NoCounter-LightBarrier" -TestCategory "CollisionProbability"
#AddTest -Testname "HeightControl_Original-NoCounter-LightBarrier_Probability_HazardFalseAlarm" -TestAssembly "SafetySharp.CaseStudies.HeightControl.dll" -TestMethod "SafetySharp.CaseStudies.HeightControl.Analysis.HazardProbabilityTests.Original-NoCounter-LightBarrier" -TestCategory "FalseAlarmProbability"
#AddTest -Testname "HeightControl_Original-NoCounter-LightBarrier_DCCA_HazardCollision" -TestAssembly "SafetySharp.CaseStudies.HeightControl.dll" -TestMethod "SafetySharp.CaseStudies.HeightControl.Analysis.ModelCheckingTests.Original-NoCounter-LightBarrier" -TestCategory "CollisionDCCA"
#AddTest -Testname "HeightControl_Original-NoCounter-LightBarrier_DCCA_HazardFalseAlarm" -TestAssembly "SafetySharp.CaseStudies.HeightControl.dll" -TestMethod "SafetySharp.CaseStudies.HeightControl.Analysis.ModelCheckingTests.Original-NoCounter-LightBarrier" -TestCategory "FalseAlarmDCCA"
#
## Original-NoCounter-Original
#AddTest -Testname "HeightControl_Original-NoCounter-Original_Probability_HazardCollision" -TestAssembly "SafetySharp.CaseStudies.HeightControl.dll" -TestMethod "SafetySharp.CaseStudies.HeightControl.Analysis.HazardProbabilityTests.Original-NoCounter-Original" -TestCategory "CollisionProbability"
#AddTest -Testname "HeightControl_Original-NoCounter-Original_Probability_HazardFalseAlarm" -TestAssembly "SafetySharp.CaseStudies.HeightControl.dll" -TestMethod "SafetySharp.CaseStudies.HeightControl.Analysis.HazardProbabilityTests.Original-NoCounter-Original" -TestCategory "FalseAlarmProbability"
#AddTest -Testname "HeightControl_Original-NoCounter-Original_DCCA_HazardCollision" -TestAssembly "SafetySharp.CaseStudies.HeightControl.dll" -TestMethod "SafetySharp.CaseStudies.HeightControl.Analysis.ModelCheckingTests.Original-NoCounter-Original" -TestCategory "CollisionDCCA"
#AddTest -Testname "HeightControl_Original-NoCounter-Original_DCCA_HazardFalseAlarm" -TestAssembly "SafetySharp.CaseStudies.HeightControl.dll" -TestMethod "SafetySharp.CaseStudies.HeightControl.Analysis.ModelCheckingTests.Original-NoCounter-Original" -TestCategory "FalseAlarmDCCA"
#
## Original-Tolerant-LightBarrier
#AddTest -Testname "HeightControl_Original-Tolerant-LightBarrier_Probability_HazardCollision" -TestAssembly "SafetySharp.CaseStudies.HeightControl.dll" -TestMethod "SafetySharp.CaseStudies.HeightControl.Analysis.HazardProbabilityTests.Original-Tolerant-LightBarrier" -TestCategory "CollisionProbability"
#AddTest -Testname "HeightControl_Original-Tolerant-LightBarrier_Probability_HazardFalseAlarm" -TestAssembly "SafetySharp.CaseStudies.HeightControl.dll" -TestMethod "SafetySharp.CaseStudies.HeightControl.Analysis.HazardProbabilityTests.Original-Tolerant-LightBarrier" -TestCategory "FalseAlarmProbability"
#AddTest -Testname "HeightControl_Original-Tolerant-LightBarrier_DCCA_HazardCollision" -TestAssembly "SafetySharp.CaseStudies.HeightControl.dll" -TestMethod "SafetySharp.CaseStudies.HeightControl.Analysis.ModelCheckingTests.Original-Tolerant-LightBarrier" -TestCategory "CollisionDCCA"
#AddTest -Testname "HeightControl_Original-Tolerant-LightBarrier_DCCA_HazardFalseAlarm" -TestAssembly "SafetySharp.CaseStudies.HeightControl.dll" -TestMethod "SafetySharp.CaseStudies.HeightControl.Analysis.ModelCheckingTests.Original-Tolerant-LightBarrier" -TestCategory "FalseAlarmDCCA"
#
## Original-Tolerant-Original
#AddTest -Testname "HeightControl_Original-Tolerant-Original_Probability_HazardCollision" -TestAssembly "SafetySharp.CaseStudies.HeightControl.dll" -TestMethod "SafetySharp.CaseStudies.HeightControl.Analysis.HazardProbabilityTests.Original-Tolerant-Original" -TestCategory "CollisionProbability"
#AddTest -Testname "HeightControl_Original-Tolerant-Original_Probability_HazardFalseAlarm" -TestAssembly "SafetySharp.CaseStudies.HeightControl.dll" -TestMethod "SafetySharp.CaseStudies.HeightControl.Analysis.HazardProbabilityTests.Original-Tolerant-Original" -TestCategory "FalseAlarmProbability"
#AddTest -Testname "HeightControl_Original-Tolerant-Original_DCCA_HazardCollision" -TestAssembly "SafetySharp.CaseStudies.HeightControl.dll" -TestMethod "SafetySharp.CaseStudies.HeightControl.Analysis.ModelCheckingTests.Original-Tolerant-Original" -TestCategory "CollisionDCCA"
#AddTest -Testname "HeightControl_Original-Tolerant-Original_DCCA_HazardFalseAlarm" -TestAssembly "SafetySharp.CaseStudies.HeightControl.dll" -TestMethod "SafetySharp.CaseStudies.HeightControl.Analysis.ModelCheckingTests.Original-Tolerant-Original" -TestCategory "FalseAlarmDCCA"
#
## Original-Original-LightBarrier
#AddTest -Testname "HeightControl_Original-Original-LightBarrier_Probability_HazardCollision" -TestAssembly "SafetySharp.CaseStudies.HeightControl.dll" -TestMethod "SafetySharp.CaseStudies.HeightControl.Analysis.HazardProbabilityTests.Original-Original-LightBarrier" -TestCategory "CollisionProbability"
#AddTest -Testname "HeightControl_Original-Original-LightBarrier_Probability_HazardFalseAlarm" -TestAssembly "SafetySharp.CaseStudies.HeightControl.dll" -TestMethod "SafetySharp.CaseStudies.HeightControl.Analysis.HazardProbabilityTests.Original-Original-LightBarrier" -TestCategory "FalseAlarmProbability"
#AddTest -Testname "HeightControl_Original-Original-LightBarrier_DCCA_HazardCollision" -TestAssembly "SafetySharp.CaseStudies.HeightControl.dll" -TestMethod "SafetySharp.CaseStudies.HeightControl.Analysis.ModelCheckingTests.Original-Original-LightBarrier" -TestCategory "CollisionDCCA"
#AddTest -Testname "HeightControl_Original-Original-LightBarrier_DCCA_HazardFalseAlarm" -TestAssembly "SafetySharp.CaseStudies.HeightControl.dll" -TestMethod "SafetySharp.CaseStudies.HeightControl.Analysis.ModelCheckingTests.Original-Original-LightBarrier" -TestCategory "FalseAlarmDCCA"
