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
        If Not BackgroundWorker1.IsBusy Then
            BackgroundWorker1.RunWorkerAsync()
        End If
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

    Private Sub ErrorAndQuit(msg As String)
        Me.Invoke(CType(Sub()
                            MessageBox.Show(msg)
                        End Sub, MethodInvoker))
        Application.Exit()
    End Sub

    Private Sub HandleOperation(fileOp As FtpOperation)
        If fileOp.type = FTP_OPERATION_TYPE.DOWNLOAD Then
            user.DownloadFile(fileOp.fileName, fileOp.destPath + "/" + fileOp.fileName, fileOp.size, Me.opProgress)
        ElseIf fileOp.type = FTP_OPERATION_TYPE.UPLOAD Then
            Dim fi = New FileInfo(fileOp.destPath)
            If fi.Length <> fileOp.size Then
                ErrorAndQuit("File length mismatch!")
            End If
            user.UploadFile(fi, Me.opProgress)
        Else
            Console.WriteLine("Bad operation?!")
        End If

        Me.Invoke(CType(Sub()
                            Dim opString = fileOp.ToString()
                            ListBox1.Items.Remove(opString)
                            ListBox1.Items.Insert(0, "<Finished> " + opString)
                            main.TriggerUpdate()
                        End Sub, MethodInvoker))
    End Sub

    Private Sub OperationProgress(percentage As Integer)
        BackgroundWorker1.ReportProgress(percentage)
    End Sub

    Private Sub BackgroundWorker1_ProgressChanged(sender As Object, e As System.ComponentModel.ProgressChangedEventArgs) Handles BackgroundWorker1.ProgressChanged
        Me.Invoke(CType(Sub()
                            ProgressBar1.Value = e.ProgressPercentage
                            Label1.Text = e.ProgressPercentage.ToString() + " %"
                        End Sub, MethodInvoker))
    End Sub

    Private Sub BackgroundWorker1_DoWork(sender As Object, e As System.ComponentModel.DoWorkEventArgs) Handles BackgroundWorker1.DoWork
        Dim q = Queue.Synchronized(QueueWindow.operationQueue)
        Dim op = Nothing

        SyncLock q.SyncRoot
            If q.Count = 0 Then
                e.Cancel = True
                Return
            ElseIf q.Count > 0 Then
                ' get file operation
                op = q.Dequeue()
            Else
                ErrorAndQuit("Negative queue count!")
            End If
        End SyncLock

        If Not IsNothing(op) Then
            HandleOperation(op)
        End If

    End Sub

    Private Sub BackgroundWorker1_RunWorkerCompleted(sender As Object, e As System.ComponentModel.RunWorkerCompletedEventArgs) Handles BackgroundWorker1.RunWorkerCompleted
        If e.Cancelled Then
            Me.Invoke(CType(Sub()
                                Label1.Text = "Done"
                            End Sub, MethodInvoker))
        Else
            Me.TriggerUpdate()
        End If
    End Sub

    Private Sub QueueWindow_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        BackgroundWorker1.WorkerReportsProgress = True
    End Sub
End Class
