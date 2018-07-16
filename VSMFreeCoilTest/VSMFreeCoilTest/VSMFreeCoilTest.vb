Imports VSMFreeCoilTest.Pico2205A
Imports System.Threading
Imports System.IO

Public Class VSMFreeCoilTest
    Dim caption As String
    Dim infoStr As String
    Dim message As String
    Dim pico As New Pico2205A
    Dim range As VoltageRange = VoltageRange.PS2000_200MV
    Dim status As Short
    Dim timeUnits As TimeUnits

    ' Arrays for data collection
    Dim times() As Integer
    Dim valuesChA() As Short
    Dim valuesChB() As Short

    Private Sub btnExit_Click(sender As Object, e As EventArgs) Handles btnExit.Click
        End
    End Sub

    Private Sub btnTest_Click(sender As Object, e As EventArgs) Handles btnTest.Click
        ' First setup the channel
        pico.SetChannel(Channel.PS2000_CHANNEL_A, CShort(1), CShort(1), range)
        'status = ps2000_set_channel(ps2000Handle, Channel.PS2000_CHANNEL_B, CShort(0), CShort(1), 0) ' Channel B: not enabled
        pico.SetChannel(Channel.PS2000_CHANNEL_B, CShort(0), CShort(1), 0)
        Dim etsMode As String = Pico2205A.EtsMode.PS2000_ETS_OFF
        pico.setETSMode(etsMode, 0, 0)

        'Find the maximum number of samples the time interval (in TimeUnits)
        'the most suitable time units and the maximum oversample at the current timebase


        'The maximum samples for the 2205A would be just over 16,000 samples with one channel enabled. Buffer memory is divided between
        'active channels.
        Dim timeBaseStatus As Short = 0
        Dim timeBase As Short = 4
        Dim numSamples As Integer = 5000
        Dim timeInterval As Integer 'in nSec's
        Dim timeUnits As Short
        Dim overSample As Short = 1
        Dim maxSamples As Integer = 0

        Do Until timeBaseStatus = 1
            timeBaseStatus = pico.GetTimebase(timeBase, numSamples, timeInterval, timeUnits, overSample, maxSamples)
            If timeBaseStatus = 0 Then
                timeBase = timeBase + CShort(1)
            End If
        Loop

        Dim TimeUnitsString = pico.TimeUnitsToString(timeBase)  'this just tells you what the time interval unit is in the returned data


        ' Now setup the triggering
        DoSetTriggering()



        ' Capture block of data from the device using a trigger.
        Dim timeIndisposedMs As Integer = 0
        Dim isReady As Short = 0

        'Turn on the signal generator, 1kHz Squarewave, 1V, 0 offset
        DoEnableSigGen()

        ' Start device collecting data then...
        status = pico.RunBlock(numSamples, timeBase, overSample, timeIndisposedMs)

        If status = 0 Then
            Call ErrorHandler("ps2000_run_block")
        End If
        'wait for completion
        While isReady = 0
            isReady = pico.Ready()
            Thread.Sleep(5)
        End While

        If isReady = 1 Then
            ' Set up buffers to collect time and channel data
            ReDim times(numSamples - 1)
            ReDim valuesChA(numSamples - 1)
            ReDim valuesChB(numSamples - 1)
            Dim overflow As Short
            Dim numSamplesCollected As Integer = 0
            ' Retrieve the number samples collected, time and channel data
            numSamplesCollected = pico.GetTimeAndValues(times(0), valuesChA(0), Nothing, Nothing, Nothing, overflow, timeUnits, numSamples)
            If numSamplesCollected = 0 Then
                Call ErrorHandler("ps2000_get_times_and_values")
            End If

            Dim valuesChAMv() As Single
            ReDim valuesChAMv(numSamplesCollected - 1)

            tbData.AppendText(String.Concat("Time (", pico.TimeUnitsToString(timeUnits), ")", vbTab, vbTab, "Ch. A"))
            tbData.AppendText(vbNewLine)

            For index = 0 To numSamplesCollected - 1
                valuesChAMv(index) = pico.adcToMv(valuesChA(index), range, PS2000_MAX_VALUE) ' Use the voltage range specified in the call to ps2000_set_channel
                tbData.AppendText(String.Concat(times(index).ToString, ",", vbTab, valuesChAMv(index).ToString))
                'put this line into a csv file
                DoWriteCsvFile(String.Concat(index, ",", times(index).ToString, ",", valuesChAMv(index).ToString))
                tbData.AppendText(vbNewLine)
            Next
        Else
            MessageBox.Show("Data collection cancelled.", "Data Collection", MessageBoxButtons.OK, MessageBoxIcon.Information)
        End If
        DoDisableSigGen()
        isReady = 0
        pico.StopUnit()
    End Sub

    Private Sub DoSetTriggering()
        Dim threshold As Short = 100
        Dim direction As Pico2205A.ThresholdDirection = 0
        Dim delay As Single = 0
        Dim autoTrig As Short = 0
        pico.SetTrigger2(Channel.PS2000_CHANNEL_A, threshold, direction, delay, autoTrig)
    End Sub

    Private Sub DoEnableSigGen()
        Dim offsetVoltage As Integer = 0
        Dim peakToPeak As UInteger = 1000000
        Dim waveType As WaveType = WaveType.PS2000_SQUARE
        Dim startFrequency As Single = 1000 ' Hertz
        Dim stopFrequency As Single = 1000 ' Hertz
        Dim increment As Single = 0.0   'should make this a setting too
        Dim dwellTime As Single = 0.0   'should make this a setting too
        Dim sweepType As SweepType = SweepType.PS2000_UP  'SweepType.PS2000_UP
        Dim numSweeps As UInteger = 0
        'status = ps2000_set_sig_gen_built_in(ps2000Handle, offsetVoltage, peakToPeak, WaveType, startFrequency, stopFrequency, increment, dwellTime, SweepType, numSweeps)
        Dim outcome As Short = pico.SigGenOutputOn(offsetVoltage, peakToPeak, waveType, startFrequency, stopFrequency, increment, dwellTime, sweepType, numSweeps)
        If outcome = 0 Then
            Call ErrorHandler("ps2000_set_sig_gen_built_in")
        End If
        'Thread.Sleep(1000)
    End Sub

    Private Sub DoDisableSigGen()
        Dim offsetVoltage As Integer = 0
        Dim peakToPeak As UInteger = 0
        Dim waveType As WaveType = WaveType.PS2000_SQUARE
        Dim startFrequency As Single = 0 ' Hertz
        Dim stopFrequency As Single = 0 ' Hertz
        Dim increment As Single = 0.0   'should make this a setting too
        Dim dwellTime As Single = 0.0   'should make this a setting too
        Dim sweepType As SweepType = SweepType.PS2000_UP  'SweepType.PS2000_UP
        Dim numSweeps As UInteger = 0
        'status = ps2000_set_sig_gen_built_in(ps2000Handle, offsetVoltage, peakToPeak, WaveType, startFrequency, stopFrequency, increment, dwellTime, SweepType, numSweeps)
        Dim outcome As Short = pico.SigGenOutputOn(offsetVoltage, peakToPeak, waveType, startFrequency, stopFrequency, increment, dwellTime, sweepType, numSweeps)
        If outcome = 0 Then
            Call ErrorHandler("ps2000_set_sig_gen_built_in")
        End If
    End Sub

    Private Sub ErrorHandler(ByVal functionName As String)

        infoStr = "                    "
        pico.UnitInfo(infoStr, CShort(infoStr.Length), Info.PS2000_ERROR_CODE)

        message = String.Concat("Error: ", functionName, " was not called correctly.")
        caption = "Error Calling Function"

        MessageBox.Show(message, caption, MessageBoxButtons.OK, MessageBoxIcon.Error)

    End Sub

    ''' <summary>
    ''' This sub creates a file, named using time/date, writes the current line of data,
    ''' </summary>
    ''' <param name="input"></param>
    Private Sub DoWriteCsvFile(input As String)
        Try
            If Not File.Exists("..\scopeData.csv") Then
                Using writer As StreamWriter = File.CreateText("..\scopeData.csv")
                    writer.WriteLine(String.Concat("Sample", ",", "Time (", pico.TimeUnitsToString(timeUnits), ")", ",", "Ch. A"))
                    writer.WriteLine(input)
                End Using
            Else
                Using writer As StreamWriter = File.AppendText("..\scopeData.csv")
                    writer.WriteLine(input, vbCrLf)
                End Using
            End If
        Catch ex As Exception
            MessageBox.Show(ex.Message)
        End Try
    End Sub

End Class
