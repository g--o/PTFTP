Imports System.IO
Imports System.Threading
Imports PTFTP.My.Resources

Public Class QueueWindow
    Public Shared operationQueue As New Queue
    Private user = Login.user
    Private main As Main
    Private opProgress As Action(Of Integer) = AddressOf Me.OperationProgress
    Public Shared isCancelled = False

    Public Sub New(mainForm As Main)

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        Me.Text = GlobalStrings.op_queue
        Me.CancelButton.Text = GlobalStrings.cancel

        If Login.language.TextInfo.IsRightToLeft() Then
            Me.ListBox1.RightToLeft = RightToLeft.Yes
        End If


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
        Dim opString = fileOp.ToString()

        isCancelled = False
        Me.Invoke(CType(Sub()
                            CancelButton.Enabled = True
                            ListBox1.SetSelected(ListBox1.FindString(opString), True)
                        End Sub, MethodInvoker))

        If fileOp.type = FTP_OPERATION_TYPE.DOWNLOAD Then
            user.DownloadFile(fileOp.file.GetURI(), fileOp.destPath, fileOp.file.size, Me.opProgress)
        ElseIf fileOp.type = FTP_OPERATION_TYPE.UPLOAD Then
            Dim windowsURI = fileOp.file.path + "\" + fileOp.file.name
            user.UploadFile(windowsURI, fileOp.destPath, fileOp.file.size, Me.opProgress)
        ElseIf fileOp.type = FTP_OPERATION_TYPE.MOVE Then
            user.MoveFile(fileOp.file.GetURI(), fileOp.destPath)
        ElseIf fileOp.type = FTP_OPERATION_TYPE.RENAME Then
            user.RenameFile(fileOp.file.GetURI(), fileOp.destPath)
        Else
            Console.WriteLine(GlobalStrings.err_bad_op)
        End If

        Dim result = GlobalStrings.finished

        If isCancelled Then
            result = GlobalStrings.cancelled
            isCancelled = False
        End If

        Me.Invoke(CType(Sub()
                            ListBox1.Items.Remove(opString)
                            ListBox1.Items.Insert(0, "<" + result + "> " + opString)
                            CancelButton.Enabled = isCancelled
                            main.TriggerUpdate()
                        End Sub, MethodInvoker))
    End Sub

    Private Sub OperationProgress(percentage As Integer)
        BackgroundWorker1.ReportProgress(percentage)
    End Sub

    Private Function GetUnitsString(amount) As String
        Dim units = "KB"
        Dim speed = amount / 1000.0
        If (speed - 1000.0 > 0.001) Then
            speed /= 1000.0
            units = "MB"
        End If

        Return Int(speed).ToString() + " " + units
    End Function

    Private Sub BackgroundWorker1_ProgressChanged(sender As Object, e As System.ComponentModel.ProgressChangedEventArgs) Handles BackgroundWorker1.ProgressChanged
        Dim spacing = Environment.NewLine
        Me.Invoke(CType(Sub()
                            ProgressBar1.Value = e.ProgressPercentage

                            Dim percentage = e.ProgressPercentage.ToString() + " %"
                            Dim speed = GlobalStrings.speed + ": " + GetUnitsString(user.opSpeed) + "/s"
                            Dim size = GlobalStrings.size + ": " + GetUnitsString(user.opSize)

                            fileDetailLabel.Text = speed + spacing + size
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
                ErrorAndQuit(GlobalStrings.err_bad_queue_count)
            End If
        End SyncLock

        If Not IsNothing(op) Then
            HandleOperation(op)
        End If

    End Sub

    Private Sub BackgroundWorker1_RunWorkerCompleted(sender As Object, e As System.ComponentModel.RunWorkerCompletedEventArgs) Handles BackgroundWorker1.RunWorkerCompleted
        If e.Cancelled Then
            Me.Invoke(CType(Sub()
                                fileDetailLabel.Text = ""
                                ProgressBar1.Value = 100
                            End Sub, MethodInvoker))
        Else
            Me.TriggerUpdate()
        End If
    End Sub

    Private Sub QueueWindow_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        fileDetailLabel.Text = ""
        BackgroundWorker1.WorkerReportsProgress = True
        ProgressBar1.Value = 0
    End Sub

    Private Sub CancelButton_Click(sender As Object, e As EventArgs) Handles CancelButton.Click
        CancelButton.Enabled = False
        isCancelled = True
    End Sub
End Class
