Public Class FtpFile

    Public Flags As String
    Public Owner As String
    Public Group As String
    Public Size As Integer
    Public isDirectory As Boolean
    Public CreateDate As DateTime
    Public Name As String
    Public imageIndex As Integer

    Public Sub New(ByVal strRecord As String, Optional type As String = "unix")
        If type = "unix" Then
            Dim strProcess As String = strRecord.Trim
            Dim intIndex As Integer = 0

            Me.imageIndex = 0

            'start parsing
            Me.Flags = strProcess.Substring(0, 9)
            Me.isDirectory = strProcess.Substring(0, 1) = "d"

            'skip first part
            strProcess = strProcess.Substring(11).Trim
            strProcess = strProcess.Substring(strProcess.IndexOf(" ")).Trim

            Me.Owner = strProcess.Substring(0, strProcess.IndexOf(" "))

            strProcess = strProcess.Substring(strProcess.IndexOf(" ")).Trim
            Me.Group = strProcess.Substring(0, strProcess.IndexOf(" "))

            strProcess = strProcess.Substring(strProcess.IndexOf(" ")).Trim
            Integer.TryParse(strProcess.Substring(0, strProcess.IndexOf(" ")), Me.Size)

            strProcess = strProcess.Substring(strProcess.IndexOf(" ")).Trim
            DateTime.TryParse(strProcess.Substring(0, 12), Me.CreateDate)

            Me.Name = strProcess.Substring(12).Trim

            Dim linkArrowIndex = Me.Name.LastIndexOf(" ->")
            If linkArrowIndex <> -1 Then
                Me.Name = Me.Name.Substring(0, linkArrowIndex)
                Me.isDirectory = True
            End If
        End If
    End Sub

End Class


