Imports System.IO
Imports System.Net

Public Class User
    'יצירה של חיבור משתמש
    Sub New(ByVal h As String, ByVal u As String, ByVal p As String, ByVal po As Integer)

        ' וולאק שלא יהיה קצר
        If h.Length < 5 Then
            Throw New Exception("Invalid IP!")
        End If

        ' אם אין קידומת תוסיף
        If Not h.Substring(0, 5).Equals("ftp://") Then
            h = "ftp://" + h
        End If

        host = h
        username = u
        password = p
        port = po
    End Sub

    'תביא אייפי להציג למשתמש
    Function GetPrettyHost()
        Return host.Substring(6, host.Length - 6)
    End Function

    Function GetCwd()
        Return Me.host + cwd
    End Function

    'השג את תוכן התיקיה
    Function SetDirectory(name As String)
        cwd += "/" + name
        Return Me.GetRequest(Me.GetCwd(), System.Net.WebRequestMethods.Ftp.ListDirectory)
    End Function

    Function CreateRequest(uri As String, method As String)
        Dim req = System.Net.FtpWebRequest.Create(uri)
        req.Method = method
        req.Credentials = Me.request.Credentials
        Return req
    End Function

    'שלח בקשה
    Function GetRequest(uri As String, method As String)
        Dim req = CreateRequest(uri, method)

        Dim resp = req.GetResponse()
        Dim s As Stream = resp.GetResponseStream()
        Dim res = New System.IO.StreamReader(s).ReadToEnd()

        resp.Close()

        Return res
    End Function

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

    'סגור את החיבור
    Function Close()
        Me.response.Close()
    End Function

    'התחבר עם הפרטים שהזנת
    Function Auth()
        request = System.Net.FtpWebRequest.Create(Me.host)
        request.Method = System.Net.WebRequestMethods.Ftp.ListDirectory
        request.Credentials = New System.Net.NetworkCredential(Me.username, Me.password)

        request.KeepAlive = True
        request.UseBinary = True
        request.ConnectionGroupName = "MyGroupName"
        request.ServicePoint.ConnectionLimit = 2 'רגיל

        response = request.GetResponse()
        Return New System.IO.StreamReader(response.GetResponseStream()).ReadToEnd()
    End Function

    Function UploadFile(fileInfo As FileInfo)
        Dim path = fileInfo.FullName
        Dim name = fileInfo.Name

        Dim file() As Byte = System.IO.File.ReadAllBytes(path)
        Dim uri = Me.GetCwd() + "/" + name

        Return Me.StreamRequest(uri, System.Net.WebRequestMethods.Ftp.UploadFile, file)
    End Function

    Function DownloadFile(fileName As String, target As String)
        Dim req = Me.CreateRequest(Me.GetCwd() + "/" + fileName, System.Net.WebRequestMethods.Ftp.DownloadFile)
        req.UseBinary = True
        ' read stream and write to file
        Using FtpResponse As FtpWebResponse = CType(req.GetResponse, FtpWebResponse)
            Using ResponseStream As IO.Stream = FtpResponse.GetResponseStream

                Using fs As New IO.FileStream(target, FileMode.Create)
                    Dim buffer(2047) As Byte
                    Dim read As Integer = 0
                    Do
                        read = ResponseStream.Read(buffer, 0, buffer.Length)
                        fs.Write(buffer, 0, read)
                    Loop Until read = 0
                    ResponseStream.Close()
                    fs.Flush()
                    fs.Close()
                End Using
                ResponseStream.Close()

            End Using
        End Using
    End Function

    Function DeleteFile(name As String)
        Return Me.GetRequest(Me.GetCwd() + "/" + name, System.Net.WebRequestMethods.Ftp.DeleteFile)
    End Function

    Public host As String
    Public username As String
    Public password As String
    Public port As Integer
    Public cwd As String

    Private request As System.Net.FtpWebRequest
    Private response As System.Net.WebResponse
End Class

