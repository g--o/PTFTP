<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class PreferencesForm
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(PreferencesForm))
        Me.LangLabel = New System.Windows.Forms.Label()
        Me.LangComboBox = New System.Windows.Forms.ComboBox()
        Me.ConnLabel = New System.Windows.Forms.Label()
        Me.LimitTextBox = New System.Windows.Forms.TextBox()
        Me.ApplyButton = New System.Windows.Forms.Button()
        Me.CancelButton = New System.Windows.Forms.Button()
        Me.changesLabel = New System.Windows.Forms.Label()
        Me.SuspendLayout()
        '
        'LangLabel
        '
        Me.LangLabel.AutoSize = True
        Me.LangLabel.Location = New System.Drawing.Point(40, 35)
        Me.LangLabel.Name = "LangLabel"
        Me.LangLabel.Size = New System.Drawing.Size(55, 13)
        Me.LangLabel.TabIndex = 0
        Me.LangLabel.Text = "Language"
        '
        'LangComboBox
        '
        Me.LangComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.LangComboBox.FormattingEnabled = True
        Me.LangComboBox.Location = New System.Drawing.Point(43, 52)
        Me.LangComboBox.Name = "LangComboBox"
        Me.LangComboBox.Size = New System.Drawing.Size(121, 21)
        Me.LangComboBox.TabIndex = 1
        '
        'ConnLabel
        '
        Me.ConnLabel.AutoSize = True
        Me.ConnLabel.Location = New System.Drawing.Point(40, 85)
        Me.ConnLabel.Name = "ConnLabel"
        Me.ConnLabel.Size = New System.Drawing.Size(85, 13)
        Me.ConnLabel.TabIndex = 2
        Me.ConnLabel.Text = "Connection Limit"
        '
        'LimitTextBox
        '
        Me.LimitTextBox.Enabled = False
        Me.LimitTextBox.Location = New System.Drawing.Point(43, 102)
        Me.LimitTextBox.Name = "LimitTextBox"
        Me.LimitTextBox.Size = New System.Drawing.Size(121, 20)
        Me.LimitTextBox.TabIndex = 3
        Me.LimitTextBox.Text = "<Unlimited>"
        '
        'ApplyButton
        '
        Me.ApplyButton.Enabled = False
        Me.ApplyButton.Location = New System.Drawing.Point(294, 179)
        Me.ApplyButton.Name = "ApplyButton"
        Me.ApplyButton.Size = New System.Drawing.Size(75, 23)
        Me.ApplyButton.TabIndex = 4
        Me.ApplyButton.Text = "Apply"
        Me.ApplyButton.UseVisualStyleBackColor = True
        '
        'CancelButton
        '
        Me.CancelButton.Location = New System.Drawing.Point(213, 179)
        Me.CancelButton.Name = "CancelButton"
        Me.CancelButton.Size = New System.Drawing.Size(75, 23)
        Me.CancelButton.TabIndex = 5
        Me.CancelButton.Text = "Cancel"
        Me.CancelButton.UseVisualStyleBackColor = True
        '
        'changesLabel
        '
        Me.changesLabel.AutoSize = True
        Me.changesLabel.Location = New System.Drawing.Point(170, 55)
        Me.changesLabel.Name = "changesLabel"
        Me.changesLabel.Size = New System.Drawing.Size(176, 13)
        Me.changesLabel.TabIndex = 6
        Me.changesLabel.Text = "Changes will take effect after restart"
        '
        'PreferencesForm
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(381, 214)
        Me.ControlBox = False
        Me.Controls.Add(Me.changesLabel)
        Me.Controls.Add(Me.CancelButton)
        Me.Controls.Add(Me.ApplyButton)
        Me.Controls.Add(Me.LimitTextBox)
        Me.Controls.Add(Me.ConnLabel)
        Me.Controls.Add(Me.LangComboBox)
        Me.Controls.Add(Me.LangLabel)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Name = "PreferencesForm"
        Me.Text = "Preferences"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents LangLabel As Label
    Friend WithEvents LangComboBox As ComboBox
    Friend WithEvents ConnLabel As Label
    Friend WithEvents LimitTextBox As TextBox
    Friend WithEvents ApplyButton As Button
    Friend WithEvents CancelButton As Button
    Friend WithEvents changesLabel As Label
End Class
