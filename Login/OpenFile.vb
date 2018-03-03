Public Class OpenFile

    Private FSM As IO.FileSystemWatcher
    Private remotePath As String
    Private localPath As String
    Private name As String
    Private user As User

    Public Sub New(name As String, user As User)
        Me.user = user
        Me.name = name

        Me.remotePath = Me.user.GetCwd() + "/" + Me.name
        Me.localPath = "tmp\" + user.GetPrettyCwd() + "__" + Me.name
    End Sub

    Public Function Open()
        Me.user.DownloadFile(Me.name, localPath)
        FSM = New IO.FileSystemWatcher("tmp", Me.user.GetPrettyCwd() + "__" + Me.name)
        FSM.EnableRaisingEvents = True
        AddHandler FSM.Changed, AddressOf OnSave
        Process.Start(Me.localPath)
    End Function

    Public Function OnSave(ByVal source As Object, ByVal e As IO.FileSystemEventArgs)
        If e.ChangeType = IO.WatcherChangeTypes.Changed Then
            Me.user.UploadInto(Me.localPath, Me.remotePath)
        End If
    End Function

End Class
