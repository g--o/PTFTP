Public Enum FTP_OPERATION_TYPE
    UPLOAD
    DOWNLOAD
    MOVE
    RENAME
End Enum

Public Class FtpOperation
    Public type As FTP_OPERATION_TYPE
    Public file As PendingOpFile
    Public destPath As String

    Public Sub New(file As PendingOpFile, destPath As String, type As FTP_OPERATION_TYPE)
        Me.type = type
        Me.file = file
        Me.destPath = destPath
    End Sub

    Public Overrides Function ToString() As String
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

        s += ": " + file.name

        s += extras

        Return s
    End Function
End Class
