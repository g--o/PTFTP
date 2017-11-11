﻿Imports System.Net.FtpWebRequest

Public Class Login

    Public user As User

    'יציאה
    Private Sub Button1_Click_1(sender As Object, e As EventArgs) Handles Button1.Click
        Application.Exit()
    End Sub

    'התחברות
    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        Dim res As String
        'התחברות עצמה
        Try
            user = New User(TextBox1.Text, TextBox2.Text, TextBox3.Text, NumericUpDown1.Value)
            res = user.Auth()
        Catch ex As Exception
            MessageBox.Show("Failed to connect" + Environment.NewLine + ex.Message) ' שגיאת התחברות
            Return 'צא מהקוד
        End Try

        'מחביא הכל
        Me.Hide()
        'מציג דף ראשי
        Dim m = New Main(res)
        m.ShowDialog()
        Me.Show()
    End Sub

    'שמירה
    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        Try
            If (Not TextBox1.Text.Equals("")) Then
                Dim user = New User(TextBox1.Text, TextBox2.Text, TextBox3.Text, NumericUpDown1.Value)
                'מוחקים
                Dim fileName = "Users\" + user.GetPrettyHost() + ".txt"

                If My.Computer.FileSystem.FileExists(fileName) Then
                    My.Computer.FileSystem.DeleteFile(fileName)
                End If
                'כותבים
                My.Computer.FileSystem.WriteAllText(fileName, user.username + Environment.NewLine + user.password + Environment.NewLine + user.port.ToString(), True)
                'מוסיפים לרשימה
                ListBox1.Items.Add(user.GetPrettyHost())
            End If
        Catch ex As Exception
            MessageBox.Show("Couldn't save!" + Environment.NewLine + ex.Message)
        End Try
    End Sub

    'בחירה בפרופיל
    Private Sub ListBox1_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ListBox1.SelectedIndexChanged
        Try
            If ListBox1.SelectedIndex.Equals(-1) Then
                ContextMenuStrip1.Enabled = False
                Return
            Else
                ContextMenuStrip1.Enabled = True
            End If

            Dim host = ListBox1.SelectedItem.ToString()
            Dim fileReader = My.Computer.FileSystem.ReadAllText("Users\" + host + ".txt")
            Dim elements = fileReader.ToString().Split(Environment.NewLine)

            Dim user = New User(host, elements.GetValue(0), elements.GetValue(1), Integer.Parse(elements.GetValue(2)))

            TextBox1.Text = user.GetPrettyHost()
            TextBox2.Text = user.username
            TextBox3.Text = user.password.Substring(1, user.password.Length - 1)

            NumericUpDown1.Value = user.port

        Catch ex As Exception
            MessageBox.Show("Couldn't load!" + Environment.NewLine + ex.Message)
            Return
        End Try
    End Sub

    Private Sub InstallUpdateSyncWithInfo()
        Dim info As System.Deployment.Application.UpdateCheckInfo = Nothing

        If (System.Deployment.Application.ApplicationDeployment.IsNetworkDeployed) Then
            Dim AD As System.Deployment.Application.ApplicationDeployment = System.Deployment.Application.ApplicationDeployment.CurrentDeployment

            Try
                info = AD.CheckForDetailedUpdate()
            Catch dde As System.Deployment.Application.DeploymentDownloadException
                MessageBox.Show("The new version of the application cannot be downloaded at this time. " + ControlChars.Lf & ControlChars.Lf & "Please check your network connection, or try again later. Error: " + dde.Message)
                Return
            Catch ioe As InvalidOperationException
                MessageBox.Show("This application cannot be updated. It is likely not a ClickOnce application. Error: " & ioe.Message)
                Return
            End Try

            If (info.UpdateAvailable) Then
                Dim doUpdate As Boolean = True

                If (Not info.IsUpdateRequired) Then
                    Dim dr As DialogResult = MessageBox.Show("An update is available. Would you like to update the application now?", "Update Available", MessageBoxButtons.OKCancel)
                    If (Not System.Windows.Forms.DialogResult.OK = dr) Then
                        doUpdate = False
                    End If
                Else
                    ' Display a message that the app MUST reboot. Display the minimum required version.
                    MessageBox.Show("This application has detected a mandatory update from your current " &
                        "version to version " & info.MinimumRequiredVersion.ToString() &
                        ". The application will now install the update and restart.",
                        "Update Available", MessageBoxButtons.OK,
                        MessageBoxIcon.Information)
                End If

                If (doUpdate) Then
                    Try
                        AD.Update()
                        MessageBox.Show("The application has been upgraded, and will now restart.")
                        Application.Restart()
                    Catch dde As System.Deployment.Application.DeploymentDownloadException
                        MessageBox.Show("Cannot install the latest version of the application. " & ControlChars.Lf & ControlChars.Lf & "Please check your network connection, or try again later.")
                        Return
                    End Try
                End If
            End If
        End If
    End Sub

    'טעינה של הכל
    Private Sub Login_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        InstallUpdateSyncWithInfo()

        'כבה קליק ימני
        ContextMenuStrip1.Enabled = False
        ' תרשום לי את כל הקבצים
        Dim files = My.Computer.FileSystem.GetFiles("Users")

        For Each file In files ' לכל קובץ ברשימה
            Dim host = file.Substring(0, file.Length - 4) ' מעיפים .txt
            host = host.Substring(host.LastIndexOf("\") + 1, host.Length - host.LastIndexOf("\") - 1) 'מעיפים את המיקום תיקייה
            ListBox1.Items.Add(host) ' תוסיף לרשימה
        Next
    End Sub

    'קליק ימני
    Private Sub ContextMenuStrip1_Opening(sender As Object, e As System.ComponentModel.CancelEventArgs) Handles ContextMenuStrip1.Opening
        If ListBox1.SelectedIndex.Equals(-1) Then
            ContextMenuStrip1.Hide()
        End If
    End Sub

    'מחיקה
    Private Sub DeleteToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles DeleteToolStripMenuItem.Click
        Try
            My.Computer.FileSystem.DeleteFile("Users\" + ListBox1.SelectedItem + ".txt")
            ListBox1.Items.RemoveAt(ListBox1.SelectedIndex)
        Catch
            MessageBox.Show("Failed to delete save.")
        End Try
    End Sub
End Class