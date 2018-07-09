Imports System.IO
Imports System.Net

Public Class User
    Private bufferSize As Integer = 4096

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

    'IP to show
    Function GetPrettyHost()
        Return host.Substring(6, host.Length - 6)
    End Function

    Function GetCwd()
        Return Me.host + cwd
    End Function

    Function GetPrettyCwd()
        Dim newCwd As String = cwd
        Return newCwd.Trim().Replace(" ", "-").Replace("/", "_")

    End Function

    'Get dir content
    Function SetDirectory(name As String)
        cwd += "/" + name
        Return Me.GetRequest(Me.GetCwd(), System.Net.WebRequestMethods.Ftp.ListDirectoryDetails)
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

        read = srcStream.Read(buffer, 0, buffer.Length)
        While read > 0
            destStream.Write(buffer, 0, read)
            If callback IsNot Nothing And size <> 0 Then
                totalRead += read
                callback(totalRead / size * 100)
            End If
            read = srcStream.Read(buffer, 0, buffer.Length)
        End While
    End Function

    'Close the connection
    Function Close()
        Me.response.Close()
    End Function

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

    'Upload to path
    Function UploadInto(localPath As String, remotePath As String)
        Dim file() As Byte
        Try
            file = System.IO.File.ReadAllBytes(localPath)
        Catch ex As Exception
            MessageBox.Show("Sorry, file is used right now. Please try again in a few seconds.")
            Return False
        End Try

        Return Me.StreamRequest(remotePath, System.Net.WebRequestMethods.Ftp.UploadFile, file)
    End Function

    'Upload to current dir
    Function UploadFile(fileInfo As FileInfo, Optional callback As Action(Of Integer) = Nothing)
        Dim path = fileInfo.FullName
        Dim name = FileInfo.Name
        Dim size = FileInfo.Length

        Dim uri = Me.GetCwd() + "/" + name
        Dim req = CreateRequest(uri, System.Net.WebRequestMethods.Ftp.UploadFile)
        req.UseBinary = True

        Using fileStream As Stream = File.OpenRead(path)
            Using ftpStream As Stream = req.GetRequestStream()
                CopyStreams(fileStream, ftpStream, size, callback)
                ftpStream.Close()
                fileStream.Close()
            End Using
        End Using
    End Function

    'Download
    Function DownloadFile(fileName As String, target As String, Optional size As Integer = 0, Optional callback As Action(Of Integer) = Nothing)
        Dim req = Me.CreateRequest(Me.GetCwd() + "/" + fileName, System.Net.WebRequestMethods.Ftp.DownloadFile)
        req.UseBinary = True
        ' read stream and write to file
        Using FtpResponse As FtpWebResponse = CType(req.GetResponse, FtpWebResponse)
            Using ftpStream As IO.Stream = FtpResponse.GetResponseStream
                Using fileStream As New IO.FileStream(target, FileMode.Create)
                    CopyStreams(ftpStream, fileStream, size, callback)
                    ftpStream.Close()
                    fileStream.Flush()
                    fileStream.Close()
                End Using
            End Using
        End Using
    End Function

    'Delete file
    Function DeleteFile(name As String)
        Return Me.GetRequest(Me.GetCwd() + "/" + name, System.Net.WebRequestMethods.Ftp.DeleteFile)
    End Function

    Function MoveFile(name As String, dest As String)
        Dim srcPath = GetCwd() + "/"
        Dim req As FtpWebRequest = CreateRequest(srcPath + name, WebRequestMethods.Ftp.Rename)
        req.RenameTo = dest + "/" + name

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

    Public host As String
    Public username As String
    Public password As String
    Public port As Integer
    Public cwd As String

    Private request As System.Net.FtpWebRequest
    Private response As System.Net.WebResponse
End Class

