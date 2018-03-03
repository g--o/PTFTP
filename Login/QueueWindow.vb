Imports System.IO
Imports System.Threading

Public Class QueueWindow
    Public Shared operationQueue As New Queue
    Private user = Login.user
    Private main As Main
    Private opProgress As Action(Of Integer) = AddressOf Me.OperationProgress

    Public Sub New(mainForm As Main)

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        Me.main = mainForm
    End Sub

    Public Sub TriggerUpdate()
        Dim q = Queue.Synchronized(QueueWindow.operationQueue)

        SyncLock q.SyncRoot
            While q.Count > 0
                ' get file operation & handle
                HandleOperation(q.Dequeue())
            End While
        End SyncLock
    End Sub

    Public Sub EnqueueOperation(fileOp As FtpOperation)
        Dim q = Queue.Synchronized(QueueWindow.operationQueue)
        SyncLock q.SyncRoot
            q.Enqueue(fileOp)
        End SyncLock

        Me.Invoke(CType(Sub()
                            ListBox1.Items.Insert(0, fileOp.ToString())
                        End Sub, MethodInvoker))
    End Sub

    Private Sub HandleOperation(fileOp As FtpOperation)
        If fileOp.type = FTP_OPERATION_TYPE.DOWNLOAD Then
            user.DownloadFile(fileOp.fileName, fileOp.destPath + "/" + fileOp.fileName, fileOp.size, Me.opProgress)
        ElseIf fileOp.type = FTP_OPERATION_TYPE.UPLOAD Then
            Dim fi = New FileInfo(fileOp.destPath)
            If fi.Length <> fileOp.size Then
                Me.Invoke(CType(Sub()
                                    MessageBox.Show("File length mismatch!")
                                End Sub, MethodInvoker))
                Application.Exit()
            End If
            UploadTask(fi)
        Else
            Console.WriteLine("Bad operation?!")
        End If

        Me.Invoke(CType(Sub()
                            Dim opString = fileOp.ToString()
                            ListBox1.Items.Remove(opString)
                            ListBox1.Items.Insert(0, "<Finished> " + opString)
                        End Sub, MethodInvoker))
    End Sub

    Private Sub CreateUploadTask(fileInfo As FileInfo)
        Dim trd = New Thread(Sub() UploadTask(fileInfo))
        trd.IsBackground = True
        trd.Start()
    End Sub

    Private Sub UploadTask(fileInfo As FileInfo)
        Me.Invoke(CType(Sub()
                            user.UploadFile(fileInfo, Me.opProgress)
                            main.TriggerUpdate()
                        End Sub, MethodInvoker))
    End Sub

    Private Sub OperationProgress(percentage As Integer)

        Me.Invoke(CType(Sub()
                            ProgressBar1.Value = percentage
                            Label1.Text = percentage.ToString() + "%"
                        End Sub, MethodInvoker))
    End Sub

End Class