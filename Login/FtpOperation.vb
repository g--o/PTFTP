Imports PTFTP.My.Resources

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
            s = GlobalStrings.download
        ElseIf type = FTP_OPERATION_TYPE.UPLOAD Then
            s = GlobalStrings.upload
        ElseIf type = FTP_OPERATION_TYPE.MOVE Then
            s = GlobalStrings.move
            extras += " -> " + destPath
        ElseIf type = FTP_OPERATION_TYPE.RENAME Then
            s = GlobalStrings.rename
            extras += " -> " + destPath
        Else
            s = GlobalStrings.invalid_op
        End If

        s += ": " + file.name

        s += extras

        Return s
    End Function
End Class
