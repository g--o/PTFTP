Public Enum FTP_OPERATION_TYPE
    UPLOAD
    DOWNLOAD
End Enum

Public Class FtpOperation
    Public type As FTP_OPERATION_TYPE
    Public fileName As String
    Public destPath As String
    Public size As Integer

    Public Sub New(fileName As String, destPath As String, type As FTP_OPERATION_TYPE, Optional size As Integer = 0)
        Me.type = type
        Me.fileName = fileName
        Me.destPath = destPath
        Me.size = size
    End Sub

    Public Function ToString() As String
        Dim s = ""

        If type = FTP_OPERATION_TYPE.DOWNLOAD Then
            s = "Download"
        Else
            s = "Upload"
        End If

        s += ": " + fileName

        Return s
    End Function
End Class
