Public Class ExternalDropHandler

    Public tmpFileExtension = ".ptftpdl9"
    Public tmpFileName = "ptftp" & tmpFileExtension

    Private FSMArr = New List(Of IO.FileSystemWatcher)
    Private callback As Action(Of String)

    Public Sub New(callback As Action(Of String))
        Me.callback = callback

        setupFSMs()

        If Not IO.File.Exists(My.Application.Info.DirectoryPath & "\" & tmpFileName) Then
            IO.File.Create(My.Application.Info.DirectoryPath & "\" & tmpFileName)
            SetAttr(My.Application.Info.DirectoryPath & "\" & tmpFileName, FileAttribute.Hidden + FileAttribute.System)
        End If

    End Sub

    Public Sub setupFSMs()
        Dim Drives() As IO.DriveInfo = IO.DriveInfo.GetDrives 'This retrieves a complete list of the drives used in the pc.

        For i As Integer = 0 To Drives.Length - 1      'Make a loop to add filesystemwatcher to EVERY drive and thier sub folders.
            If Drives(i).DriveType = IO.DriveType.Fixed And Drives(i).IsReady Then
                Dim fsm = New IO.FileSystemWatcher(Drives(i).RootDirectory.FullName, tmpFileName)

                AddHandler fsm.Created, AddressOf Me.OnChanged
                fsm.IncludeSubdirectories = True
                fsm.EnableRaisingEvents = True

                FSMArr.Add(fsm)
            End If
        Next
    End Sub

    Sub TryDelete(location As String)
        Try
            My.Computer.FileSystem.DeleteFile(location)
        Catch ex As Exception
            System.Threading.Thread.Sleep(1000)
            TryDelete(location)
        End Try
    End Sub

    Sub OnChanged(ByVal source As Object, ByVal e As IO.FileSystemEventArgs)
        Dim output = IO.Path.GetDirectoryName(e.FullPath)
        Dim locator = output.Trim("'") & "\" & tmpFileName

        TryDelete(locator)

        Me.callback(output)
    End Sub

End Class
