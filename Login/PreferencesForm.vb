Public Class PreferencesForm
    Private Sub CancelButton_Click(sender As Object, e As EventArgs) Handles CancelButton.Click
        Me.Close()
    End Sub

    Private Sub PreferencesForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        LangComboBox.SelectedIndex = LangComboBox.FindStringExact(Login.language)
    End Sub
End Class