'This class has been sanitized to include only the 2205A stuff
'Things that still need to be added:
'1. Autoranging function.
'2. Function to set the sample rate based upon channles used and freq of interest.
'3. ETS Mode.
'4. Streaming Mode

Public Class Pico2205A
    ' Constant values
    ' Although the PS2000 uses an 8-bit ADC it is usually possible to oversample (collect multiple readings at each time) by up to 256.
    ' the results are therefore ALWAYS scaled up to 16-bits even if oversampling is not used.

#Region "Constants and Variables"
    'Constants
    Public inputRanges() As Integer = New Integer(12) {10, 20, 50, 100, 200, 500, 1000, 2000, 5000, 10000, 20000, 50000, 100000} ' ranges in mV
    Public Const PS2000_MAX_VALUE = 32767
    Public Const PS2000_LOST_DATA = -32768
    Public _handle As Short
    Dim rangeOK As Boolean
    Dim status As Short

    Public Sub New()
        OpenUnit()
    End Sub
#End Region

#Region "Structures"
    Public Structure TRIGGER_CHANNEL_PROPERTIES
        Dim thresholdMajor As Short
        Dim thresholdMinor As Short
        Dim hysteresis As UShort
        Dim channel As Channel
        Dim thresholdMode As ThresholdMode
    End Structure

    Public Structure TRIGGER_CONDITIONS
        Dim channelA As TriggerState
        Dim channelB As TriggerState
        Dim pulseWidthQualifier As TriggerState
    End Structure

    Public Structure PWQ_CONDITIONS
        Dim channelA As TriggerState
        Dim channelB As TriggerState
    End Structure
#End Region

#Region "Enumerations"
    Public Enum Dc As Short 'input coupling mode
        AC = 0
        DC = 1
    End Enum

    Public Enum Channel As Short
        PS2000_CHANNEL_A = 0
        PS2000_CHANNEL_B = 1
    End Enum

    Public Enum VoltageRange
        PS2000_50MV
        PS2000_100MV
        PS2000_200MV
        PS2000_500MV
        PS2000_1V
        PS2000_2V
        PS2000_5V
        PS2000_10V
        PS2000_20V
        PS2000_MAX_RANGES
    End Enum

    Public Enum TimeUnits As Short
        PS2000_FS
        PS2000_PS
        PS2000_NS
        PS2000_US
        PS2000_MS
        PS2000_S
        PS2000_MAX_TIME_UNITS
    End Enum

    Public Enum PS2000Error
        PS2000_OK
        PS2000_MAX_UNITS_OPENED  ' more than PS2000_MAX_UNITS
        PS2000_MEM_FAIL          ' not enough RAM on host machine
        PS2000_NOT_FOUND         ' cannot find device
        PS2000_FW_FAIL           ' unable to download firmware
        PS2000_NOT_RESPONDING
        PS2000_CONFIG_FAIL       'missing or corrupted configuration settings
        PS2000_OS_NOT_SUPPORTED  'need to use win98SE (or later) or win2k (or later)
        PS2000_PICOPP_TOO_OLD
    End Enum

    Public Enum Info
        PS2000_DRIVER_VERSION
        PS2000_USB_VERSION
        PS2000_HARDWARE_VERSION
        PS2000_VARIANT_INFO
        PS2000_BATCH_AND_SERIAL
        PS2000_CAL_DATE
        PS2000_ERROR_CODE
        PS2000_KERNEL_DRIVER_VERSION
    End Enum

    Public Enum TriggerDirection
        PS2000_RISING
        PS2000_FALLING
        PS2000_MAX_DIRS
    End Enum

    Public Enum OpenProgress
        PS2000_OPEN_PROGRESS_FAIL = -1
        PS2000_OPEN_PROGRESS_PENDING = 0
        PS2000_OPEN_PROGRESS_COMPLETE = 1
    End Enum

    Public Enum EtsMode
        PS2000_ETS_OFF   ' ETS disabled
        PS2000_ETS_FAST  ' Return ready as soon as requested no of interleaves is available
        PS2000_ETS_SLOW  ' Return ready every time a new set of no_of_cycles is collected
        PS2000_ETS_MODES_MAX
    End Enum

    Public Enum ButtonState
        PS2000_NO_PRESS
        PS2000_SHORT_PRESS
        PS2000_LONG_PRESS
    End Enum

    Public Enum SweepType
        PS2000_UP
        PS2000_DOWN
        PS2000_UPDOWN
        PS2000_DOWNUP
        MAX_SWEEP_TYPES
    End Enum

    Public Enum WaveType
        PS2000_SINE
        PS2000_SQUARE
        PS2000_TRIANGLE
        PS2000_RAMPUP
        PS2000_RAMPDOWN
        PS2000_DC_VOLTAGE
        PS2000_GAUSSIAN
        PS2000_SINC
        PS2000_HALF_SINE
    End Enum

    Public Enum ThresholdDirection
        PS2000_ABOVE
        PS2000_BELOW
        PS2000_ADV_RISING
        PS2000_ADV_FALLING
        PS2000_RISING_OR_FALLING
        PS2000_INSIDE = PS2000_ABOVE
        PS2000_OUTSIDE = PS2000_BELOW
        PS2000_ENTER = PS2000_ADV_RISING
        PS2000_EXIT = PS2000_ADV_FALLING
        PS2000_ENTER_OR_EXIT = PS2000_RISING_OR_FALLING
        PS2000_ADV_NONE = PS2000_ADV_RISING
    End Enum

    Public Enum ThresholdMode
        PS2000_LEVEL
        PS2000_WINDOW
    End Enum

    Public Enum TriggerState
        PS2000_CONDITION_DONT_CARE
        PS2000_CONDITION_TRUE
        PS2000_CONDITION_FALSE
        PS2000_CONDITION_MAX
    End Enum

    Public Enum PulseWidthType
        PS2000_PW_TYPE_NONE
        PS2000_PW_TYPE_LESS_THAN
        PS2000_PW_TYPE_GREATER_THAN
        PS2000_PW_TYPE_IN_RANGE
        PS2000_PW_TYPE_OUT_OF_RANGE
    End Enum

    Public Enum SampleRateNs 'ns
        SR0 = 5
        SR1 = 10
        SR2 = 20
        SR3 = 40
        SR4 = 80
        SR5 = 160
        SR6 = 320
        SR7 = 640
        SR8 = 1280
        SR9 = 2560
        SR10 = 5120
        SR11 = 10240
        SR12 = 20480
        SR13 = 40960
        SR14 = 81920
        SR15 = 163840
        SR16 = 327680
        SR17 = 655360
        SR18 = 1310720
    End Enum
#End Region

#Region "Original Declared Functions"
    ' Original Declared Functions 
    Private Declare Function ps2000_open_unit Lib "ps2000.dll" () As Short
    Private Declare Function ps2000_flash_led Lib "ps2000.dll" (ByVal handle As Short) As Short
    Private Declare Sub ps2000_close_unit Lib "ps2000.dll" (ByVal handle As Short)
    Private Declare Function ps2000_get_unit_info Lib "ps2000.dll" (ByVal handle As Short, ByVal str As String, ByVal lth As Short, ByVal line_no As Info) As Short
    Private Declare Function ps2000_set_channel Lib "ps2000.dll" (ByVal handle As Short, ByVal channel As Channel, ByVal enabled As Short, ByVal dc As Short, ByVal range As VoltageRange) As Short
    Private Declare Function ps2000_set_trigger2 Lib "ps2000.dll" (ByVal handle As Short, ByVal source As Channel, ByVal threshold As Short, ByVal direction As ThresholdDirection,
                                                           ByVal delay As Single, ByVal auto_trigger_ms As Short) As Short
    Public Declare Function ps2000_get_timebase Lib "ps2000.dll" (ByVal handle As Short, ByVal timebase As Short, ByVal numSamples As Integer, ByRef timeInterval As Integer, ByRef timeUnits As Short,
                                                           ByVal oversample As Short, ByRef maxSamples As Integer) As Short
    Private Declare Function ps2000_get_timebase2 Lib "ps2000.dll" (ByVal handle As Short, ByVal timebase As Short, ByVal numSamples As Integer, ByRef timeInterval As Double, ByRef timeUnits As Short,
                                                            ByVal oversample As Short, ByRef maxSamples As Integer) As Short
    Private Declare Function ps2000_run_block Lib "ps2000.dll" (ByVal handle As Short, ByVal no_of_values As Integer, ByVal timebase As Short, ByVal oversample As Short, ByRef timeIndisposedMs As Integer) As Short
    Private Declare Function ps2000_ready Lib "ps2000.dll" (ByVal handle As Short) As Short
    Private Declare Function ps2000_get_values Lib "ps2000.dll" (ByVal handle As Short, ByRef buffer_a As Short, ByRef buffer_b As Short, ByRef buffer_c As Short, ByRef buffer_d As Short,
                                                         ByRef overflow As Short, ByVal no_of_values As Integer) As Integer
    Private Declare Function ps2000_get_times_and_values Lib "ps2000.dll" (ByVal handle As Short, ByRef times As Integer, ByRef buffer_a As Short, ByRef buffer_b As Short, ByRef buffer_c As Short,
                                                                   ByRef buffer_d As Short, ByRef overflow As Short, ByVal timeUnits As Short, ByVal numSamples As Integer) As Integer
    Private Declare Function ps2000_stop Lib "ps2000.dll" (ByVal handle As Short) As Short
    Private Declare Function ps2000_run_streaming Lib "ps2000.dll" (ByVal handle As Short, ByVal sample_interval_ms As Short, ByVal maxSamples As Integer, ByVal windowed As Short) As Short
    Private Declare Function ps2000_run_streaming_ns Lib "ps2000.dll" (ByVal handle As Short, ByVal sample_interval As UInteger, ByVal timeUnits As TimeUnits, ByVal maxSamples As UInteger,
                                                               ByVal auto_stop As Short, ByVal noOfSamplesPerAggregate As UInteger, ByVal overview_buffer_size As UInteger) As Short
    Private Declare Function ps2000_get_streaming_values_no_aggregation Lib "ps2000.dll" (ByVal handle As Short, ByRef start_time As Double, ByRef pBuffer_a As Short, ByRef pBuffer_b As Short,
                                                                                  ByRef pBuffer_c As Short, ByRef pBuffer_d As Short, ByRef overflow As Short, ByRef triggerAt As UInteger,
                                                                                  ByRef trigger As Short, ByVal numValues As UInteger) As UInteger
    Private Declare Function ps2000_set_ets Lib "ps2000.dll" (ByVal handle As Short, ByVal mode As Short, ByVal ets_cycles As Short, ByVal ets_interleave As Short) As Integer
    Private Declare Function ps2000_set_sig_gen_built_in Lib "ps2000.dll" (ByVal handle As Short, ByVal offsetVoltage As Integer, ByVal pkToPk As UInteger, ByVal waveType As WaveType,
                                                                   ByVal startFrequency As Single, ByVal stopFrequency As Single, ByVal increment As Single, ByVal dwellTime As Single,
                                                                   ByVal sweepType As SweepType, ByVal sweeps As UInteger) As Short

    ' Wrapper Functions
    Declare Function PollFastStreaming Lib "ps2000Wrap.dll" (ByVal handle As Integer) As Short
    Declare Sub SetBuffer Lib "ps2000Wrap.dll" (ByVal handle As Integer, ByVal channel As Channel, ByRef buffer As Integer, ByVal bufferSize As UInteger)
    Declare Sub SetAggregateBuffer Lib "ps2000Wrap.dll" (ByVal handle As Integer, ByVal channel As Channel, ByRef bufferMax As Integer, ByRef bufferMin As Integer, ByVal bufferSize As UInteger)
    Declare Function FastStreamingReady Lib "ps2000Wrap.dll" (ByVal handle As Short) As Short
    Declare Function GetFastStreamingDetails Lib "ps2000Wrap.dll" (ByVal handle As Short, ByRef overflow As Short, ByRef triggeredAt As UInteger, ByRef triggered As Short, ByRef auto_stop As Short,
                                                                   ByRef appBufferFull As Short, ByRef startIndex As UInteger) As UInteger
    Declare Sub setEnabledChannels Lib "ps2000Wrap.dll" (ByVal handle As Short, ByRef enabledChannels As Short)
    Declare Sub clearFastStreamingParameters Lib "ps2000Wrap.dll" (ByVal handle As Short)
    Declare Function setCollectionInfo Lib "ps2000Wrap.dll" (ByVal handle As Short, ByVal collectionSize As UInteger, ByVal overviewBufferSize As UInteger) As Short
#End Region

#Region "QD Calls"

    'QD calls to declared functions
    Public Function OpenUnit() As Short
        _handle = ps2000_open_unit()
        Return _handle
    End Function

    Public Sub FlashLED()
        ps2000_flash_led(_handle)
    End Sub

    Public Sub CloseUnit()
        ps2000_close_unit(_handle)
    End Sub

    Public Function UnitInfo(ByVal str As String, ByVal lth As Short, ByVal line_no As Info) As String
        ps2000_get_unit_info(_handle, str, lth, line_no)
        Return str
    End Function

    ''' <summary>
    ''' Specifies if a channel is to be enabled, the AC/DC coupling mode and the input range.
    ''' </summary>
    ''' <param name="handle"></param>
    ''' <param name="channel"></param>
    ''' <param name="enabled"></param>
    ''' <param name="dc"></param>
    ''' <param name="range"></param>
    Public Sub SetChannel(ByVal channel As Channel, ByVal enabled As Short, ByVal dc As Short, ByVal range As VoltageRange)
        ps2000_set_channel(_handle, channel, enabled, dc, range)
    End Sub

    ''' <summary>
    ''' This function is used to enable or disable triggering and set its parameters. It has the same behavior As ps2000_set_trigger, except 
    ''' that the delay parameter Is a floating-point value.
    ''' </summary>
    ''' <param name="handle"></param>
    ''' <param name="source"></param>
    ''' <param name="threshold"></param>
    ''' <param name="direction"></param>
    ''' <param name="delay"></param>
    ''' <param name="auto_trigger_ms"></param>
    Public Sub SetTrigger2(ByVal source As Channel, ByVal threshold As Short, ByVal direction As ThresholdDirection,
                                                           ByVal delay As Single, ByVal auto_trigger_ms As Short)
        ps2000_set_trigger2(_handle, source, threshold, direction, delay, auto_trigger_ms)
    End Sub

    ''' <summary>
    ''' This function discovers which timebases are available on the oscilloscope. You should Set up the channels Using ps2000_set_channel 
    ''' And, If required, ETS mode Using ps2000_set_ets first. Then Call this Function With increasing values Of timebase, starting from 0, until 
    ''' you find a timebase With a sampling interval And sample count close enough To your requirements.
    ''' </summary>
    ''' <param name="handle"></param>
    ''' <param name="timebase"></param>
    ''' <param name="numSamples"></param>
    ''' <param name="timeInterval"></param>
    ''' <param name="timeUnits"></param>
    ''' <param name="oversample"></param>
    ''' <param name="maxSamples"></param>
    ''' <returns></returns>
    Public Function GetTimebase(ByVal timebase As Short, ByVal numSamples As Integer, ByRef timeInterval As Integer, ByRef timeUnits As TimeUnits,
                                                           ByVal oversample As Short, ByRef maxSamples As Integer) As Short
        Return ps2000_get_timebase(_handle, timebase, numSamples, timeInterval, CShort(timeUnits), oversample, maxSamples)
    End Function

    Public Function GetTimebase2(ByVal handle As Short, ByVal timebase As Short, ByVal numSamples As Integer, ByRef timeInterval As Double, ByRef timeUnits As Short,
                                                            ByVal oversample As Short, ByRef maxSamples As Integer) As Short
        Return ps2000_get_timebase2(_handle, timebase, numSamples, timeInterval, timeUnits, oversample, maxSamples)
    End Function

    ''' <summary>
    ''' This function tells the oscilloscope to start collecting data in block mode.
    ''' </summary>
    ''' <param name="handle"></param>
    ''' <param name="no_of_values"></param>
    ''' <param name="timebase"></param>
    ''' <param name="oversample"></param>
    ''' <param name="timeIndisposedMs"></param>
    ''' <returns></returns>
    Public Function RunBlock(ByVal no_of_values As Integer, ByVal timebase As Short, ByVal oversample As Short, ByRef timeIndisposedMs As Integer) As Short
        Return ps2000_run_block(_handle, no_of_values, timebase, oversample, timeIndisposedMs)
    End Function

    ''' <summary>
    ''' This function polls the driver to see if the oscilloscope has finished the last data collection operation.
    ''' </summary>
    ''' <param name="handle"></param>
    ''' <returns></returns>
    Public Function Ready() As Short
        Return ps2000_ready(_handle)
    End Function

    ''' <summary>
    ''' This function is used to get values in compatible streaming mode after calling ps2000_run_streaming, Or in block mode after calling ps2000_run_block. 
    ''' Note that If you are Using block mode Or ETS mode And Call this Function before the oscilloscope Is ready, no capture will be available And the driver
    ''' will Not return any samples.
    ''' </summary>
    ''' <param name="handle"></param>
    ''' <param name="buffer_a"></param>
    ''' <param name="buffer_b"></param>
    ''' <param name="buffer_c"></param>
    ''' <param name="buffer_d"></param>
    ''' <param name="overflow"></param>
    ''' <param name="no_of_values"></param>
    Public Sub GetValues(ByVal handle As Short, ByRef buffer_a As Short, ByRef buffer_b As Short, ByRef buffer_c As Short, ByRef buffer_d As Short,
                                                         ByRef overflow As Short, ByVal no_of_values As Integer)
        ps2000_get_values(_handle, buffer_a, buffer_b, buffer_c, buffer_d, overflow, no_of_values)
    End Sub

    ''' <summary>
    ''' This function is used to get values and times in block mode after calling ps2000_run_block. 
    ''' Note that If you are Using block mode Or ETS mode And Call this Function before the oscilloscope Is ready, no capture will
    ''' be available And the driver will Not return any samples.
    ''' </summary>
    ''' <param name="handle"></param>
    ''' <param name="times"></param>
    ''' <param name="buffer_a"></param>
    ''' <param name="buffer_b"></param>
    ''' <param name="buffer_c"></param>
    ''' <param name="buffer_d"></param>
    ''' <param name="overflow"></param>
    ''' <param name="timeUnits"></param>
    ''' <param name="numSamples"></param>
    ''' <returns></returns>
    Public Function GetTimeAndValues(ByRef times As Integer, ByRef buffer_a As Short, ByRef buffer_b As Short, ByRef buffer_c As Short,
                                                                   ByRef buffer_d As Short, ByRef overflow As Short, ByVal timeUnits As TimeUnits, ByVal numSamples As Integer) As Integer
        Return ps2000_get_times_and_values(_handle, times, buffer_a, buffer_b, buffer_c, buffer_d, overflow, timeUnits, numSamples)
    End Function

    Public Sub StopUnit()
        ps2000_stop(_handle)
    End Sub

    ''' <summary>
    ''' This function tells the oscilloscope to start collecting data in compatible streaming mode. If this function Is called when a trigger 
    ''' has been enabled, the trigger settings will be ignored.
    '''  
    ''' For streaming with the PicoScope 2202, 2203, 2204, 2204A, 2205 And 2205A variants, we recommend you use ps2000_run_streaming_ns instead: 
    ''' this will allow much faster data transfer.
    ''' </summary>
    ''' <param name="handle"></param>
    ''' <param name="sample_interval_ms"></param>
    ''' <param name="maxSamples"></param>
    ''' <param name="windowed"></param>
    Public Sub RunSteaming(ByVal handle As Short, ByVal sample_interval_ms As Short, ByVal maxSamples As Integer, ByVal windowed As Short)
        ps2000_run_streaming(_handle, sample_interval_ms, maxSamples, windowed)
    End Sub

    ''' <summary>
    ''' This function tells the oscilloscope to start collecting data in fast streaming mode. It returns immediately without waiting For 
    ''' data To be captured. After calling it, you should next call ps2000_get_streaming_last_values to copy the data to your application's buffer.
    ''' </summary>
    ''' <param name="handle"></param>
    ''' <param name="sample_interval"></param>
    ''' <param name="timeUnits"></param>
    ''' <param name="maxSamples"></param>
    ''' <param name="auto_stop"></param>
    ''' <param name="noOfSamplesPerAggregate"></param>
    ''' <param name="overview_buffer_size"></param>
    Public Sub RunSteamingNsec(ByVal handle As Short, ByVal sample_interval As UInteger, ByVal timeUnits As TimeUnits, ByVal maxSamples As UInteger,
                                                               ByVal auto_stop As Short, ByVal noOfSamplesPerAggregate As UInteger, ByVal overview_buffer_size As UInteger)
        ps2000_run_streaming_ns(_handle, sample_interval, timeUnits, maxSamples, auto_stop, noOfSamplesPerAggregate, overview_buffer_size)
    End Sub

    ''' <summary>
    ''' This function retrieves raw streaming data from the driver's data store after fast streaming has stopped.
    ''' 
    ''' Before calling the Function, capture some data Using fast streaming, Stop streaming Using ps2000_stop, and Then allocate 
    ''' sufficient buffer space To receive the requested data. The function will store the data in your buffer with values in the range 
    ''' PS2000_MIN_VALUE to PS2000_MAX_VALUE. The special value PS2000_LOST_DATA Is stored in the buffer when data could Not be 
    ''' collected because of a buffer overrun. (See Voltage ranges for more details of data values.)
    ''' </summary>
    ''' <param name="handle"></param>
    ''' <param name="start_time"></param>
    ''' <param name="pBuffer_a"></param>
    ''' <param name="pBuffer_b"></param>
    ''' <param name="pBuffer_c"></param>
    ''' <param name="pBuffer_d"></param>
    ''' <param name="overflow"></param>
    ''' <param name="triggerAt"></param>
    ''' <param name="trigger"></param>
    ''' <param name="numValues"></param>
    Public Sub GetStreamingValuesNoAggregation(ByVal handle As Short, ByRef start_time As Double, ByRef pBuffer_a As Short, ByRef pBuffer_b As Short,
                                                                                  ByRef pBuffer_c As Short, ByRef pBuffer_d As Short, ByRef overflow As Short, ByRef triggerAt As UInteger,
                                                                                  ByRef trigger As Short, ByVal numValues As UInteger)
        ps2000_get_streaming_values_no_aggregation(_handle, start_time, pBuffer_a, pBuffer_b, pBuffer_c, pBuffer_d, overflow, triggerAt, trigger, numValues)
    End Sub

    ''' <summary>
    ''' This function is used to enable or disable ETS mode and to set the ETS parameters.
    ''' </summary>
    ''' <param name="handle"></param>
    ''' <param name="mode"></param>
    ''' <param name="ets_cycles"></param>
    ''' <param name="ets_interleave"></param>
    Public Sub SetETS(ByVal handle As Short, ByVal mode As Short, ByVal ets_cycles As Short, ByVal ets_interleave As Short)
        ps2000_set_ets(_handle, mode, ets_cycles, ets_interleave)
    End Sub

    Public Function SigGenOutputOn(ByVal offsetVoltage As Integer, ByVal pkToPk As UInteger, ByVal waveType As WaveType,
                                                                   ByVal startFrequency As Single, ByVal stopFrequency As Single, ByVal increment As Single, ByVal dwellTime As Single,
                                                                   ByVal sweepType As SweepType, ByVal sweeps As UInteger) As Boolean
        Return 0 <> ps2000_set_sig_gen_built_in(_handle, offsetVoltage, pkToPk, waveType, startFrequency, stopFrequency, increment, dwellTime, sweepType, sweeps)
    End Function
#End Region

#Region "Functions"
    ''' <summary>
    ''' Converts from raw ADC values to mV values. The mV value returned depends upon the ADC count, and the voltage range set for the channel.
    ''' </summary>
    ''' <param name="raw"></param>
    ''' <param name="range"></param>
    ''' <param name="maxADCValue"></param>
    ''' <returns>mvVal = value converted into mV</returns>
    Public Function adcToMv(ByVal raw As Integer, ByVal range As Integer, ByVal maxADCValue As Short) As Single

        Dim mVVal As Single        ' Use this variable to force data to be returned as an integer

        mVVal = (CSng(raw) * inputRanges(range)) / maxADCValue

        Return mVVal

    End Function

    ''' <summary>
    ''' Converts from mV into ADC value. The ADC count returned depends upon the mV value, and the voltage range set for the channel.
    ''' </summary>
    ''' <param name="mv"></param>
    ''' <param name="range"></param>
    ''' <param name="maxADCValue"></param>
    ''' <returns>adcCount = value converted into an ADC count</returns>
    Public Function mvToAdc(ByVal mv As Short, ByVal range As Integer, ByVal maxADCValue As Short) As Short
        Dim adcCount As Short
        adcCount = CShort((mv / inputRanges(range)) * maxADCValue)
        Return adcCount
    End Function

    ''' <summary>
    ''' This is just a method to show the timeunits magnitude
    ''' </summary>
    ''' <param name="timeUnits"></param>
    ''' <returns></returns>
    Public Function TimeUnitsToString(ByVal timeUnits As Integer) As String

        Select Case timeUnits
            Case 0
                Return "fs"
            Case 1
                Return "ps"
            Case 2
                Return "ns"
            Case 3
                Return "us"
            Case 4
                Return "ms"
            Case 5
                Return "s"
            Case Else
                Return ""
        End Select
    End Function

    ''' <summary>
    ''' To use this, pass in the desired mode and the handle from your app
    ''' </summary>
    ''' <param name="etsMode"></param>
    ''' <param name="handle"></param>
    Public Sub setETSMode(etsMode As String, cycles As Short, interleave As Short)
        Dim mode As Short
        GetMode(etsMode)
        Call ps2000_set_ets(_handle, mode, cycles, interleave)
    End Sub

    ''' <summary>
    ''' converts the text selection passed to an ETS Mode
    ''' </summary>
    ''' <param name="etsMode"></param>
    ''' <returns></returns>
    Private Function GetMode(etsMode As String) As EtsMode
        Dim mode As EtsMode
        Select Case etsMode
            Case "PS2000_ETS_OFF"
                mode = Pico2205A.EtsMode.PS2000_ETS_OFF
            Case "PS2000_ETS_FAST"
                mode = Pico2205A.EtsMode.PS2000_ETS_FAST
            Case "PS2000_ETS_SLOW"
                mode = Pico2205A.EtsMode.PS2000_ETS_SLOW
        End Select
        Return mode
    End Function

    ''' <summary>
    ''' I removed the "this = that" ones as they never came up with intellisense, only the = part did???
    ''' </summary>
    ''' <param name="Input"></param>
    ''' <returns></returns>
    Public Function FindDirection(Input As String) As ThresholdDirection
        Dim direction As ThresholdDirection
        Select Case Input
            Case "PS2000_ABOVE"
                direction = ThresholdDirection.PS2000_ABOVE
            Case "PS2000_BELOW"
                direction = ThresholdDirection.PS2000_BELOW
            Case "PS2000_ADV_RISING"
                direction = ThresholdDirection.PS2000_ADV_RISING
            Case "PS2000_ADV_FALLING"
                direction = ThresholdDirection.PS2000_ADV_FALLING
            Case "PS2000_RISING_OR_FALLING"
                direction = ThresholdDirection.PS2000_RISING_OR_FALLING
        End Select
    End Function

    ''' <summary>
    ''' Not implemented yet, but a method to test the voltage range and adjust as necessary
    ''' This should be called in whatever is reading the ADC, currently that is in GetBlockData
    ''' Pass in the adc read.
    ''' </summary>
    ''' <returns></returns>
    Private Function CheckRangeAgainstInput(_channel As Channel, coupling As Dc) As Integer
        'read the ADC value and look for an overflow, if so, try the next higher range, repeat
        'read the ADC and look for a minimum acceptible level, if too low, move down to the next lower range
        Return VoltageRange.PS2000_100MV
    End Function

    ''' <summary>
    ''' This function will return the SampleRate calculated from the total buffer size of 16K samples.
    ''' </summary>
    ''' <param name="channels">is the number of channels you are using, i.e. 1 (ChA) or 2(ChA and ChB)</param>
    ''' <param name="cycles">is the number of cycles you wish to read of the signal</param>
    ''' <param name="freq">is the freq of the signal you are reading</param>
    ''' <returns></returns>
    Public Function SetSampleRate(channels As Short, cycles As Short, freq As Short) As SampleRateNs
        Dim period As Double = 1 / freq
        Dim maxSampleRate As Double = (period * channels) / 16000
        'Need select case to find the highest sample rate
        Select Case maxSampleRate
            Case <= 5
                Return SampleRateNs.SR0
            Case <= 10
                Return SampleRateNs.SR1
            Case <= 20
                Return SampleRateNs.SR2
            Case <= 40
                Return SampleRateNs.SR3
            Case <= 80
                Return SampleRateNs.SR4
            Case <= 160
                Return SampleRateNs.SR5
            Case <= 320
                Return SampleRateNs.SR6
            Case <= 640
                Return SampleRateNs.SR7
            Case <= 1280
                Return SampleRateNs.SR8
            Case <= 2560
                Return SampleRateNs.SR9
            Case <= 5120
                Return SampleRateNs.SR10
            Case <= 10240
                Return SampleRateNs.SR11
            Case <= 20480
                Return SampleRateNs.SR12
            Case <= 40960
                Return SampleRateNs.SR13
            Case <= 81920
                Return SampleRateNs.SR14
            Case <= 163840
                Return SampleRateNs.SR15
            Case <= 327680
                Return SampleRateNs.SR16
            Case <= 655360
                Return SampleRateNs.SR17
        End Select
        Return SampleRateNs.SR18
    End Function
#End Region
End Class
