<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class QueueWindow
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()>
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
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(QueueWindow))
        Me.ListBox1 = New System.Windows.Forms.ListBox()
        Me.ProgressBar1 = New PTFTP.PercentProgressBar()
        Me.BackgroundWorker1 = New System.ComponentModel.BackgroundWorker()
        Me.CancelButton = New System.Windows.Forms.Button()
        Me.fileDetailLabel = New System.Windows.Forms.Label()
        Me.SuspendLayout()
        '
        'ListBox1
        '
        Me.ListBox1.FormattingEnabled = True
        Me.ListBox1.Location = New System.Drawing.Point(12, 59)
        Me.ListBox1.Name = "ListBox1"
        Me.ListBox1.Size = New System.Drawing.Size(582, 95)
        Me.ListBox1.TabIndex = 0
        '
        'ProgressBar1
        '
        Me.ProgressBar1.Location = New System.Drawing.Point(12, 15)
        Me.ProgressBar1.Name = "ProgressBar1"
        Me.ProgressBar1.Size = New System.Drawing.Size(585, 33)
        Me.ProgressBar1.TabIndex = 1
        '
        'BackgroundWorker1
        '
        '
        'CancelButton
        '
        Me.CancelButton.Enabled = False
        Me.CancelButton.Location = New System.Drawing.Point(603, 15)
        Me.CancelButton.Name = "CancelButton"
        Me.CancelButton.Size = New System.Drawing.Size(69, 33)
        Me.CancelButton.TabIndex = 4
        Me.CancelButton.Text = "Cancel"
        Me.CancelButton.UseVisualStyleBackColor = True
        '
        'fileDetailLabel
        '
        Me.fileDetailLabel.AutoSize = True
        Me.fileDetailLabel.Location = New System.Drawing.Point(600, 59)
        Me.fileDetailLabel.Name = "fileDetailLabel"
        Me.fileDetailLabel.Size = New System.Drawing.Size(86, 13)
        Me.fileDetailLabel.TabIndex = 5
        Me.fileDetailLabel.Text = "speed: 000MB/s"
        '
        'QueueWindow
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(684, 161)
        Me.ControlBox = False
        Me.Controls.Add(Me.fileDetailLabel)
        Me.Controls.Add(Me.CancelButton)
        Me.Controls.Add(Me.ProgressBar1)
        Me.Controls.Add(Me.ListBox1)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Name = "QueueWindow"
        Me.Text = "Operations Queue"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents ListBox1 As ListBox
    Friend WithEvents ProgressBar1 As PercentProgressBar
    Friend WithEvents BackgroundWorker1 As System.ComponentModel.BackgroundWorker
    Friend WithEvents CancelButton As Button
    Friend WithEvents fileDetailLabel As Label
End Class
