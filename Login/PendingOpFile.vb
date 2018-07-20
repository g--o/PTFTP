
Public Class PendingOpFile
    Public path As String
    Public name As String
    Public size As Integer
    Public isDirectory As Boolean

    Public Sub New(curPath As String, file As FtpFile)
        Me.path = curPath
        Me.name = file.Name
        Me.size = file.Size
        Me.isDirectory = file.isDirectory
    End Sub

    Public Sub New(curPath As String, fileName As String, Optional isDirectory As Boolean = False, Optional size As Integer = 0)
        Me.path = curPath
        Me.name = fileName
        Me.size = size
        Me.isDirectory = isDirectory
    End Sub

    Public Function GetURI() As String
        Return path + "/" + name
    End Function

End Class
