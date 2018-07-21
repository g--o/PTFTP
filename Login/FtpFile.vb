Public Class FtpFile

    Public Flags As String
    Public Owner As String
    Public Group As String
    Public Size As Integer
    Public isDirectory As Boolean
    Public CreateDate As DateTime
    Public Name As String
    Public imageIndex = 0

    Public Sub New(ByVal strRecord As String, Optional type As String = "unix")
        If type = "unix" Then
            Dim strProcess As String = strRecord.Trim
            Dim intIndex As Integer = 0

            Me.imageIndex = 0

            'start parsing
            Me.Flags = strProcess.Substring(0, 9)
            Me.isDirectory = (Flags.Substring(0, 1) = "d")

            'skip first part
            strProcess = strProcess.Substring(11).Trim
            strProcess = strProcess.Substring(strProcess.IndexOf(" ")).Trim

            'owner
            Me.Owner = strProcess.Substring(0, strProcess.IndexOf(" "))

            'group
            strProcess = strProcess.Substring(strProcess.IndexOf(" ")).Trim
            Me.Group = strProcess.Substring(0, strProcess.IndexOf(" "))

            'size
            strProcess = strProcess.Substring(strProcess.IndexOf(" ")).Trim
            Integer.TryParse(strProcess.Substring(0, strProcess.IndexOf(" ")), Me.Size)

            'date
            strProcess = strProcess.Substring(strProcess.IndexOf(" ")).Trim
            DateTime.TryParse(strProcess.Substring(0, 12), Me.CreateDate)

            'name
            Me.Name = strProcess.Substring(12).Trim

            'symlinks
            Dim linkArrowIndex = Me.Name.LastIndexOf(" ->")
            If linkArrowIndex <> -1 Then
                Me.Name = Me.Name.Substring(0, linkArrowIndex)
                Me.isDirectory = True
            End If

            'set corresponding image
            Dim ext = "<dir>"
            If Not Me.isDirectory Then 'handle file icon
                If Name.Contains(".") Then
                    ext = Name.Substring(Name.LastIndexOf("."))
                Else
                    ext = "dummy_file"
                End If
                If (Not IconFinder.imageList.Images.ContainsKey(ext)) Then
                    Dim image = IconFinder.GetFileIcon(ext)
                    IconFinder.imageList.Images.Add(ext, image)
                    Main.updateSizedImageList()
                End If
            End If

            Me.imageIndex = Main.sizedImageList.Images.IndexOfKey(ext)
            If Me.imageIndex < 0 Then
                Me.imageIndex = 0
            End If
        End If

    End Sub

    Public Shared Function ParseStringArray(arr() As String)
        Dim outputList As New List(Of FtpFile)

        For Each file As String In arr 'Seperate to a list
            ' skip "."
            If Not arr.First.Equals(file) Then
                file = file.Substring(1, file.Length - 1)
            End If

            Dim parsedFile
            If file.Equals("") Then
                ' include ".."
                parsedFile = New FtpFile("d--------- 1 - - 0 Jan 01 00:00 ..")
            Else
                parsedFile = New FtpFile(file)
            End If

            ' Add file
            outputList.Add(parsedFile)
        Next

        Return outputList
    End Function

End Class


