Public Enum FTP_OPERATION_TYPE
    UPLOAD
    DOWNLOAD
    MOVE
    RENAME
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
        Dim extras = ""

        If type = FTP_OPERATION_TYPE.DOWNLOAD Then
            s = "Download"
        ElseIf type = FTP_OPERATION_TYPE.UPLOAD Then
            s = "Upload"
        ElseIf type = FTP_OPERATION_TYPE.MOVE Then
            s = "Move"
            extras += " -> " + destPath
        ElseIf type = FTP_OPERATION_TYPE.RENAME Then
            s = "Rename"
            extras += " -> " + destPath
        Else
            s = "Invalid Operation"
        End If

        s += ": " + fileName

        s += extras

        Return s
    End Function
End Class
