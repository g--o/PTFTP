Imports System.IO
Imports System.Net
Imports PTFTP.My.Resources

Public Class User
    Private bufferSize As Integer = 4096

    Public Shared defaultNewDirName = "new_folder_"

    Public Shared opSpeed = 0
    Public Shared opSize = 0
    Public Shared opFileName = ""

    'create new user
    Sub New(ByVal h As String, ByVal u As String, ByVal p As String, ByVal po As Integer)

        'Check IP length
        If h.Length < 5 Then
            Throw New Exception("Invalid IP!")
        End If

        'If there's no prefix add one
        If Not h.Substring(0, 5).Equals("ftp://") Then
            h = "ftp://" + h
        End If

        host = h
        username = u
        password = p
        port = po
    End Sub

    'Connect with creds
    Function Auth()
        request = System.Net.FtpWebRequest.Create(Me.host)
        request.Method = System.Net.WebRequestMethods.Ftp.ListDirectory
        request.Credentials = New System.Net.NetworkCredential(Me.username, Me.password)

        request.KeepAlive = True
        request.UseBinary = True
        request.ConnectionGroupName = "PTFTP"
        request.ServicePoint.ConnectionLimit = 2 'normal

        response = request.GetResponse()
        Return New System.IO.StreamReader(response.GetResponseStream()).ReadToEnd()
    End Function

    'Close the connection
    Function Close()
        Me.response.Close()
    End Function

    'IP to show
    Function GetPrettyHost()
        Return host.Substring(6, host.Length - 6)
    End Function

    'Current directory URI
    Function GetCwd() As String
        Return Me.host + cwd
    End Function

    'File format for cwd
    Function GetPrettyCwd()
        Dim newCwd As String = cwd
        Return newCwd.Trim().Replace(" ", "-").Replace("/", "_")
    End Function

    'Generate new dir name
    Function GetNewDirUri(uri As String)
        Dim i = 0
        Dim dirPath = ""

        Do
            i += 1
            dirPath = uri + "/" + defaultNewDirName + i.ToString()
        Loop While Me.IsDirExists(dirPath)

        Return dirPath
    End Function

    'Send a request
    Function IsDirExists(uri As String)
        uri = uri + "/"
        Dim req = CreateRequest(uri, System.Net.WebRequestMethods.Ftp.ListDirectory)

        Try
            Dim resp = req.GetResponse()
            Dim s As Stream = resp.GetResponseStream()
            Dim res = New System.IO.StreamReader(s).ReadToEnd()
            resp.Close()
        Catch ex As WebException
            Dim response As FtpWebResponse = DirectCast(ex.Response, FtpWebResponse)
            If response.StatusCode = FtpStatusCode.ActionNotTakenFileUnavailable Then
                Return False
            Else
                Throw New ApplicationException("Unable to determine if FTP directory exists.")
            End If
        End Try

        Return True
    End Function

    'Get dir content
    Function GetDirectory(name As String)
        Return Me.GetRequest(name, System.Net.WebRequestMethods.Ftp.ListDirectoryDetails)
    End Function

    'Set + get dir content
    Function SetDirectory(name As String)
        If name = ".." And cwd.Contains("/") Then
            cwd = cwd.Substring(0, cwd.LastIndexOf("/"))
        Else
            cwd += "/" + name
        End If

        Return GetDirectory(Me.GetCwd())
    End Function

    'Make directory
    Function CreateDirectory(uri As String)
        Return Me.GetRequest(uri, System.Net.WebRequestMethods.Ftp.MakeDirectory)
    End Function

    'Upload FILE to path
    Function UploadInto(localPath As String, remotePath As String)
        Dim file() As Byte
        Try
            file = System.IO.File.ReadAllBytes(localPath)
        Catch ex As Exception
            MessageBox.Show(GlobalStrings.err_file_in_use)
            Return False
        End Try

        Return Me.StreamRequest(remotePath, System.Net.WebRequestMethods.Ftp.UploadFile, file)
    End Function

    'Upload
    Function Upload(path As String, destUri As String, isDirectory As Boolean, Optional size As Integer = 0, Optional callback As Action(Of Integer) = Nothing)
        If isDirectory Then
            UploadDirectory(path, destUri, size, callback)
        Else
            UploadFile(path, destUri, size, callback)
        End If
    End Function

    'Upload dir
    Function UploadDirectory(dirPath As String, destUri As String, Optional size As Integer = 0, Optional callback As Action(Of Integer) = Nothing)

        If Not Me.IsDirExists(destUri) Then
            Me.CreateDirectory(destUri)
        End If

        If Not Directory.Exists(dirPath) Then
            QueueWindow.isCancelled = True
            Return False
        End If

        Dim dirInfo = New DirectoryInfo(dirPath)
        Dim fileList = dirInfo.GetFiles()

        ' Upload each file
        For Each file As FileInfo In fileList
            opFileName = file.Name
            Me.UploadFile(file.FullName, destUri + "/" + file.Name, file.Length, callback)
            opFileName = ""
        Next

        ' Upload each dir (recursively)
        Dim dirList = dirInfo.GetDirectories()
        For Each dir As DirectoryInfo In dirList
            If dir.Name = ".." Then
                Continue For
            End If
            Me.UploadDirectory(dir.FullName, destUri + "/" + dir.Name, 0, callback)
        Next

    End Function

    'Upload to current dir
    Function UploadFile(filePath As String, destUri As String, Optional size As Integer = 0, Optional callback As Action(Of Integer) = Nothing)
        Dim req = CreateRequest(destUri, System.Net.WebRequestMethods.Ftp.UploadFile)
        req.UseBinary = True

        Using fileStream As Stream = File.OpenRead(filePath)
            Using ftpStream As Stream = req.GetRequestStream()
                CopyStreams(fileStream, ftpStream, size, callback)
                ftpStream.Close()
                fileStream.Close()
            End Using
        End Using
    End Function

    'Download
    Function Download(uri As String, dest As String, isDirectory As Boolean, Optional size As Integer = 0, Optional callback As Action(Of Integer) = Nothing)
        If isDirectory Then
            Me.DownloadDirectory(uri, dest, size, callback)
        Else
            Me.DownloadFile(uri, dest, size, callback)
        End If
    End Function

    'Download dir
    Function DownloadDirectory(uri As String, dest As String, Optional size As Integer = 0, Optional callback As Action(Of Integer) = Nothing)

        If Not Directory.Exists(dest) Then
            Directory.CreateDirectory(dest)
        End If

        Dim dirString As String = GetDirectory(uri)
        Dim fileList = FtpFile.ParseStringArray(dirString.Split(Environment.NewLine))

        ' Download each file/dir recursively
        For Each file As FtpFile In fileList
            If file.Name = ".." Then
                Continue For
            End If

            If file.isDirectory Then
                Me.DownloadDirectory(uri + "/" + file.Name, dest + "/" + file.Name, file.Size, callback)
            Else
                opFileName = file.Name
                Me.DownloadFile(uri + "/" + file.Name, dest + "/" + file.Name, file.Size, callback)
                opFileName = ""
            End If
        Next

    End Function

    'Download file
    Function DownloadFile(uri As String, dest As String, Optional size As Integer = 0, Optional callback As Action(Of Integer) = Nothing)
        Dim req = Me.CreateRequest(uri, System.Net.WebRequestMethods.Ftp.DownloadFile)
        req.UseBinary = True
        ' read stream and write to file
        Using FtpResponse As FtpWebResponse = CType(req.GetResponse, FtpWebResponse)
            Using ftpStream As IO.Stream = FtpResponse.GetResponseStream
                Using fileStream As New IO.FileStream(dest, FileMode.Create)
                    CopyStreams(ftpStream, fileStream, size, callback)
                    If QueueWindow.isCancelled Then
                        req.Abort()
                    Else
                        ftpStream.Close()
                        fileStream.Flush()
                        fileStream.Close()
                    End If
                End Using
            End Using
        End Using
    End Function

    'Delete file
    Function DeleteFile(uri As String)
        Return Me.GetRequest(uri, System.Net.WebRequestMethods.Ftp.DeleteFile)
    End Function

    'Delete dir
    Function DeleteDirectory(uri As String)

        Dim dirString As String = GetDirectory(uri)
        Dim fileList = FtpFile.ParseStringArray(dirString.Split(Environment.NewLine))

        ' Download each file/dir recursively
        For Each file As FtpFile In fileList
            If file.Name = ".." Then
                Continue For
            End If

            If file.isDirectory Then
                Me.DeleteDirectory(uri + "/" + file.Name)
            Else
                opFileName = file.Name
                Me.DeleteFile(uri + "/" + file.Name)
                opFileName = ""
            End If
        Next

        Return Me.GetRequest(uri, System.Net.WebRequestMethods.Ftp.RemoveDirectory)
    End Function

    'Rename file/dir
    Function RenameFile(uri As String, dest As String)
        Dim req As FtpWebRequest = CreateRequest(uri, WebRequestMethods.Ftp.Rename)
        req.RenameTo = dest

        Dim res = ""
        Try
            Dim resp = req.GetResponse()
            Dim s As Stream = resp.GetResponseStream()
            res = New System.IO.StreamReader(s).ReadToEnd()
            resp.Close()
            Return res
        Catch ex As Exception
            MessageBox.Show(ex.Message)
        End Try

    End Function

    Function MoveFile(uri As String, dest As String)
        Return RenameFile(uri, dest)
    End Function


    'Create ftp web request
    Function CreateRequest(uri As String, method As String)
        Dim req = System.Net.FtpWebRequest.Create(uri)
        req.Method = method
        req.Credentials = Me.request.Credentials
        Return req
    End Function

    'Send a request
    Function GetRequest(uri As String, method As String)
        Dim req = CreateRequest(uri, method)

        Try
            Dim resp = req.GetResponse()
            Dim s As Stream = resp.GetResponseStream()
            Dim res = New System.IO.StreamReader(s).ReadToEnd()
            resp.Close()
            Return res
        Catch e As Exception
            MessageBox.Show(e.Message)
        End Try

        Return ""
    End Function

    'Send stream request
    Function StreamRequest(uri As String, method As String, data As Byte())
        Dim req = CreateRequest(uri, method)

        Dim strz As Stream = req.GetRequestStream()
        strz.Write(data, 0, data.Length)
        strz.Close()
        strz.Dispose()

        Dim resp = req.GetResponse()
        Dim s As Stream = resp.GetResponseStream()
        Dim res = New System.IO.StreamReader(s).ReadToEnd()
        resp.Close()

        Return res
    End Function

    'Copy two streams
    Function CopyStreams(ByRef srcStream As Stream, ByRef destStream As Stream, Optional size As Integer = 0, Optional callback As Action(Of Integer) = Nothing)
        Dim read As Integer = 0
        Dim totalRead As Double = read
        Dim buffer(bufferSize) As Byte

        Dim stopWatch = New Stopwatch()
        Dim totalTime = 0
        Dim readInMeasure = 0
        Dim percentage = 0
        Dim newPercentage = 0

        opSpeed = 0
        opSize = size

        read = srcStream.Read(buffer, 0, buffer.Length)
        While read > 0 And Not QueueWindow.isCancelled
            stopWatch.Start()

            destStream.Write(buffer, 0, read)

            If callback IsNot Nothing And size <> 0 Then
                totalRead += read

                newPercentage = Int(totalRead / size * 100)
                If newPercentage > percentage Then
                    percentage = Math.Min(newPercentage, 100)
                    callback(percentage)
                End If
            End If

            read = srcStream.Read(buffer, 0, buffer.Length)
            stopWatch.Stop()

            If stopWatch.ElapsedMilliseconds > 1000 Then
                opSpeed = (totalRead - readInMeasure) / (stopWatch.ElapsedMilliseconds / 1000.0)
                readInMeasure = totalRead
                stopWatch.Reset()
            End If
        End While

        opSpeed = 0
    End Function

    Public host As String
    Public username As String
    Public password As String
    Public port As Integer
    Public cwd = ""

    Private request As System.Net.FtpWebRequest
    Private response As System.Net.WebResponse
End Class

