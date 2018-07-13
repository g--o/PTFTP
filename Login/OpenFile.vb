Public Class OpenFile

    Private FSM As IO.FileSystemWatcher
    Private remotePath As String
    Private localFileName As String
    Private localPath As String
    Private name As String
    Private user As User
    Private isOpen = False
    Private isUpdating = False

    Public Shared Function GetPath(name As String, user As User) As String
        Return user.GetCwd() + "/" + name
    End Function

    Public Sub New(name As String, user As User)
        Me.user = user
        Me.name = name

        Me.remotePath = GetPath(Me.name, Me.user)
        Me.localFileName = user.GetPrettyCwd() + "__" + Me.name
        Me.localPath = "tmp\" + localFileName
    End Sub

    Public Function Open()
        If isOpen Then
            Process.Start(Me.localPath)
            Return False
        End If

        isOpen = True

        If IO.File.Exists(localPath) Then
            Return False
        End If

        Me.user.DownloadFile(Me.remotePath, localPath)
        FSM = New IO.FileSystemWatcher("tmp", Me.localFileName)
        FSM.EnableRaisingEvents = True
        FSM.NotifyFilter = IO.NotifyFilters.LastWrite
        AddHandler FSM.Changed, AddressOf OnSave
        Process.Start(Me.localPath)

        Return True
    End Function

    Public Function OnSave(ByVal source As Object, ByVal e As IO.FileSystemEventArgs)
        If isUpdating Then
            Return False
        End If

        isUpdating = True
        If e.ChangeType = IO.WatcherChangeTypes.Changed Then
            Threading.Thread.Sleep(1000)
            Dim fileInfo = New IO.FileInfo(Me.localPath)

            ''' Queued way:
            'Dim opFile = New PendingOpFile(fileInfo.DirectoryName, fileInfo.Name, fileInfo.Length)
            'Main.queueWindow.EnqueueOperation(New FtpOperation(opFile, Me.remotePath, FTP_OPERATION_TYPE.UPLOAD))
            'Main.queueWindow.TriggerUpdate()

            ''' Seperate thread:
            Me.user.UploadInto(Me.localPath, Me.remotePath)
            Main.queueWindow.Invoke(CType(Sub()
                                              Main.queueWindow.ListBox1.Items.Insert(0, "<Edited> " + Me.remotePath)
                                          End Sub, MethodInvoker))
        End If

        isUpdating = False
    End Function

End Class
