Public Class ExternalDropHandler

    Private Shared FSM As IO.FileSystemWatcher
    Private callback As Action(Of String)

    Public Sub New(callback As Action(Of String))
        Me.callback = callback

        setupFSMs()

        If Not IO.File.Exists(My.Application.Info.DirectoryPath & "\\tmp_donot_delete.antf2") Then
            IO.File.Create(My.Application.Info.DirectoryPath & "\\tmp_donot_delete.antf2")
            SetAttr(My.Application.Info.DirectoryPath & "\\tmp_donot_delete.antf2", FileAttribute.Hidden + FileAttribute.System)
        End If

    End Sub

    Public Sub setupFSMs()
        Dim Drives() As IO.DriveInfo = IO.DriveInfo.GetDrives 'This retrieves a complete list of the drives used in the pc.

        For i As Integer = 0 To Drives.Length - 1      'Make a loop to add filesystemwatcher to EVERY drive and thier sub folders.

            If Drives(i).DriveType = IO.DriveType.Fixed Then
                If Drives(i).IsReady Then
                    If FSM Is Nothing Then
                        FSM = New IO.FileSystemWatcher(Drives(i).RootDirectory.FullName, "*.antf2") 'antf2 is some unique ext
                    End If

                    AddHandler FSM.Created, AddressOf OnChanged
                    FSM.IncludeSubdirectories = True
                    FSM.EnableRaisingEvents = True
                End If
            End If
        Next
    End Sub

    Sub OnChanged(ByVal source As Object, ByVal e As IO.FileSystemEventArgs)
        Dim output = IO.Path.GetDirectoryName(e.FullPath)

        Dim locator = output.Trim("'") & "\tmp_donot_delete.antf2"

        Try
            While System.IO.File.Exists(locator)
                My.Computer.FileSystem.DeleteFile(locator)
                System.Threading.Thread.Sleep(1000)
            End While
        Catch ex As Exception
        End Try

        Me.callback(output)
    End Sub

End Class
