
Imports System.Globalization
Imports PTFTP.My.Resources

Public Class PreferencesForm

    Public Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.

        Me.Text = GlobalStrings.preferences
        Me.ApplyButton.Text = GlobalStrings.apply
        Me.CancelButton.Text = GlobalStrings.cancel
        Me.LimitTextBox.Text = "<" + GlobalStrings.unlimited + ">"
        Me.changesLabel.Text = GlobalStrings.changes_after_reset
        Me.ConnLabel.Text = GlobalStrings.conn_limit
        Me.LangLabel.Text = GlobalStrings.language
    End Sub

    Private Sub CancelButton_Click(sender As Object, e As EventArgs) Handles CancelButton.Click
        Me.Close()
    End Sub

    Private Sub PreferencesForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        For Each lang In Login.langList
            Dim item = Me.LangComboBox.Items.Add(lang.DisplayName)

            If lang.TwoLetterISOLanguageName = Login.language.TwoLetterISOLanguageName Then
                Me.LangComboBox.SelectedIndex = item
            End If
        Next

        Me.ApplyButton.Enabled = False
    End Sub

    Private Sub LangComboBox_SelectedIndexChanged(sender As Object, e As EventArgs) Handles LangComboBox.SelectedIndexChanged
        Me.ApplyButton.Enabled = True
    End Sub

    Private Sub LimitTextBox_TextChanged(sender As Object, e As EventArgs) Handles LimitTextBox.TextChanged

    End Sub

    Private Sub ApplyButton_Click(sender As Object, e As EventArgs) Handles ApplyButton.Click
        For Each lang In Login.langList
            If lang.DisplayName = LangComboBox.SelectedItem Then
                Login.SetLanguage(lang)
                Exit For
            End If
        Next

        'Update settings file
        Login.SaveSettings()

        Me.Close()
    End Sub
End Class