Imports System.Globalization
Imports System.Threading
Imports PTFTP.My.Resources

Public Class Login

    Public user As User
    Public langList As New List(Of Globalization.CultureInfo)
    Public language As CultureInfo = Application.CurrentCulture

    Private userPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\PTFTP\"
    Private profilesPath = userPath
    Private settingsPath = userPath + "\ptftp.settings"
    Private extension = ".ptftp"

    Public Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        UpdateFormLang()

        ' Add supported langs
        langList.Add(CultureInfo.GetCultureInfo("en"))
        langList.Add(CultureInfo.GetCultureInfo("he"))

        ' Load settings
        LoadSettings()

    End Sub

    Public Sub SetLanguage(culInfo As CultureInfo)
        If langList.Contains(culInfo) Then
            Me.language = culInfo

            CultureInfo.DefaultThreadCurrentCulture = culInfo
            CultureInfo.DefaultThreadCurrentUICulture = culInfo
            Thread.CurrentThread.CurrentCulture = culInfo
            Thread.CurrentThread.CurrentUICulture = culInfo
            UpdateFormLang()
        End If
    End Sub

    Public Sub SaveSettings()
        My.Computer.FileSystem.WriteAllText(settingsPath, Me.language.TwoLetterISOLanguageName, False)
    End Sub

    Public Sub LoadSettings()
        If My.Computer.FileSystem.FileExists(settingsPath) Then
            Dim settingsString = My.Computer.FileSystem.ReadAllText(settingsPath)

            Dim langStr = settingsString
            Dim lang = New CultureInfo(langStr)
            SetLanguage(lang)
        Else
            SaveSettings()
        End If
    End Sub

    Private Sub UpdateFormLang()
        Me.Text = "PTFTP " + GlobalStrings.login
        Me.HostLabel.Text = GlobalStrings.host
        Me.PortLabel.Text = GlobalStrings.port
        Me.UsrLabel.Text = GlobalStrings.username
        Me.PassLabel.Text = GlobalStrings.password
        Me.ConnectButton.Text = GlobalStrings.connect
        Me.SaveButton.Text = GlobalStrings.save
        Me.ExitButton.Text = GlobalStrings.exitText
        Me.DeleteToolStripMenuItem.Text = GlobalStrings.delete
    End Sub

    Private Sub ListBox1_DoubleClick(sender As ListBox, e As EventArgs) Handles ListBox1.DoubleClick
        If ListBox1.SelectedItems.Count > 0 Then
            Login()
        End If
    End Sub

    'exit
    Private Sub ExitButton_Click(sender As Object, e As EventArgs) Handles ExitButton.Click
        Application.Exit()
    End Sub

    'actual login

    Private Sub Login()
        Dim res As String

        Try
            user = New User(TextBox1.Text, TextBox2.Text, TextBox3.Text, NumericUpDown1.Value)
            res = user.Auth()
        Catch ex As Exception
            MessageBox.Show(GlobalStrings.err_cant_connect + Environment.NewLine + ex.Message) ' login error
            Return
        End Try

        'hides everything
        Me.Hide()

        'show main form
        Dim m = New Main(res)
        m.ShowDialog()
        Me.Show()
    End Sub

    'login button
    Private Sub ConnectButton_Click(sender As Object, e As EventArgs) Handles ConnectButton.Click
        Login()
    End Sub

    'save
    Private Sub SaveButton_Click(sender As Object, e As EventArgs) Handles SaveButton.Click
        Try
            If (Not TextBox1.Text.Equals("")) Then
                Dim user = New User(TextBox1.Text, TextBox2.Text, TextBox3.Text, NumericUpDown1.Value)
                'delete
                Dim fileName = profilesPath + user.GetPrettyHost() + extension

                If My.Computer.FileSystem.FileExists(fileName) Then
                    My.Computer.FileSystem.DeleteFile(fileName)
                End If
                'writing to file
                My.Computer.FileSystem.WriteAllText(fileName, user.username + Environment.NewLine + user.password + Environment.NewLine + user.port.ToString(), True)
                'adding to list
                ListBox1.Items.Add(user.GetPrettyHost())
            End If
        Catch ex As Exception
            MessageBox.Show(GlobalStrings.err_cant_save + Environment.NewLine + ex.Message)
        End Try
    End Sub

    'profile selection
    Private Sub ListBox1_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ListBox1.SelectedIndexChanged
        Try
            If ListBox1.SelectedIndex.Equals(-1) Then
                ContextMenuStrip1.Enabled = False
                Return
            Else
                ContextMenuStrip1.Enabled = True
            End If

            Dim host = ListBox1.SelectedItem.ToString()
            Dim fileReader = My.Computer.FileSystem.ReadAllText(profilesPath + host + extension)
            Dim elements = fileReader.ToString().Split(Environment.NewLine)

            Dim user = New User(host, elements.GetValue(0), elements.GetValue(1), Integer.Parse(elements.GetValue(2)))

            TextBox1.Text = user.GetPrettyHost()
            TextBox2.Text = user.username
            TextBox3.Text = user.password.Substring(1, user.password.Length - 1)

            NumericUpDown1.Value = user.port

        Catch ex As Exception
            MessageBox.Show(GlobalStrings.err_cant_load + Environment.NewLine + ex.Message)
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

    'form load
    Private Sub Login_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        'Updates: @TODO: enable again once server is up again
        'InstallUpdateSyncWithInfo()

        'disable right click
        ContextMenuStrip1.Enabled = False

        If Not My.Computer.FileSystem.DirectoryExists(profilesPath) Then
            My.Computer.FileSystem.CreateDirectory(profilesPath)
        End If

        'load translation


        'list all profile files
        Dim files = My.Computer.FileSystem.GetFiles(profilesPath)

        For Each file In files
            If Not file.EndsWith(extension) Then
                Continue For
            End If

            Dim host = file.Substring(0, file.Length - extension.Length) ' get rid of extension
            host = host.Substring(host.LastIndexOf("\") + 1, host.Length - host.LastIndexOf("\") - 1) 'strip directory path
            ListBox1.Items.Add(host) 'add to list
        Next
    End Sub

    'right click
    Private Sub ContextMenuStrip1_Opening(sender As Object, e As System.ComponentModel.CancelEventArgs) Handles ContextMenuStrip1.Opening
        If ListBox1.SelectedIndex.Equals(-1) Then
            ContextMenuStrip1.Hide()
        End If
    End Sub

    'deletion
    Private Sub DeleteToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles DeleteToolStripMenuItem.Click
        Try
            My.Computer.FileSystem.DeleteFile(profilesPath + ListBox1.SelectedItem + extension)
            ListBox1.Items.RemoveAt(ListBox1.SelectedIndex)
        Catch
            MessageBox.Show(GlobalStrings.err_cant_delete)
        End Try
    End Sub
End Class
