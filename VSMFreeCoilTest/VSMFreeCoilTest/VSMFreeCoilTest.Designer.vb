<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class VSMFreeCoilTest
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.btnExit = New System.Windows.Forms.Button()
        Me.btnTest = New System.Windows.Forms.Button()
        Me.tbPeakAmp = New System.Windows.Forms.TextBox()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.tbData = New System.Windows.Forms.TextBox()
        Me.lblTestResult = New System.Windows.Forms.Label()
        Me.SuspendLayout()
        '
        'btnExit
        '
        Me.btnExit.Location = New System.Drawing.Point(203, 18)
        Me.btnExit.Margin = New System.Windows.Forms.Padding(5, 5, 5, 5)
        Me.btnExit.Name = "btnExit"
        Me.btnExit.Size = New System.Drawing.Size(125, 35)
        Me.btnExit.TabIndex = 0
        Me.btnExit.Text = "Exit"
        Me.btnExit.UseVisualStyleBackColor = True
        '
        'btnTest
        '
        Me.btnTest.Location = New System.Drawing.Point(20, 18)
        Me.btnTest.Margin = New System.Windows.Forms.Padding(5, 5, 5, 5)
        Me.btnTest.Name = "btnTest"
        Me.btnTest.Size = New System.Drawing.Size(125, 35)
        Me.btnTest.TabIndex = 1
        Me.btnTest.Text = "Test"
        Me.btnTest.UseVisualStyleBackColor = True
        '
        'tbPeakAmp
        '
        Me.tbPeakAmp.Location = New System.Drawing.Point(175, 89)
        Me.tbPeakAmp.Margin = New System.Windows.Forms.Padding(5, 5, 5, 5)
        Me.tbPeakAmp.Name = "tbPeakAmp"
        Me.tbPeakAmp.Size = New System.Drawing.Size(164, 26)
        Me.tbPeakAmp.TabIndex = 2
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(20, 100)
        Me.Label1.Margin = New System.Windows.Forms.Padding(5, 0, 5, 0)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(144, 20)
        Me.Label1.TabIndex = 3
        Me.Label1.Text = "Peak Amplitude: "
        '
        'tbData
        '
        Me.tbData.Location = New System.Drawing.Point(33, 180)
        Me.tbData.Margin = New System.Windows.Forms.Padding(5, 5, 5, 5)
        Me.tbData.Multiline = True
        Me.tbData.Name = "tbData"
        Me.tbData.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
        Me.tbData.Size = New System.Drawing.Size(364, 350)
        Me.tbData.TabIndex = 4
        '
        'lblTestResult
        '
        Me.lblTestResult.AutoSize = True
        Me.lblTestResult.Location = New System.Drawing.Point(170, 142)
        Me.lblTestResult.Margin = New System.Windows.Forms.Padding(5, 0, 5, 0)
        Me.lblTestResult.Name = "lblTestResult"
        Me.lblTestResult.Size = New System.Drawing.Size(101, 20)
        Me.lblTestResult.TabIndex = 5
        Me.lblTestResult.Text = "Test Result"
        '
        'VSMFreeCoilTest
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(10.0!, 20.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(439, 557)
        Me.Controls.Add(Me.lblTestResult)
        Me.Controls.Add(Me.tbData)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.tbPeakAmp)
        Me.Controls.Add(Me.btnTest)
        Me.Controls.Add(Me.btnExit)
        Me.Font = New System.Drawing.Font("Microsoft Sans Serif", 12.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Margin = New System.Windows.Forms.Padding(5, 5, 5, 5)
        Me.Name = "VSMFreeCoilTest"
        Me.Text = "VSM Free Coil Test"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents btnExit As Button
    Friend WithEvents btnTest As Button
    Friend WithEvents tbPeakAmp As TextBox
    Friend WithEvents Label1 As Label
    Friend WithEvents tbData As TextBox
    Friend WithEvents lblTestResult As Label
End Class
