Imports System.Collections.Specialized
Imports System.IO
Imports System.Text.RegularExpressions
Imports System.Threading

Public Class Main
    Private files As Hashtable = New Hashtable
    Private openFiles As Hashtable = New Hashtable
    Private dirIcon As Bitmap = Bitmap.FromFile("./folder.png")
    Private fileIcon As Bitmap = Bitmap.FromFile("./file.png")
    Private imgScale As Integer = 15
    Private imgProportion As Integer = 1

    Private renameOriginalLabel As String = ""
    Private Shared opFileQueue As New Queue
    Private externalDropHandler As ExternalDropHandler
    Private user As User = Login.user

    Public Shared queueWindow As QueueWindow

    Public Sub New(res As String)

        MyBase.New()
        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.

        If System.IO.Directory.Exists("tmp") Then
            System.IO.Directory.Delete("tmp", True)
        End If

        externalDropHandler = New ExternalDropHandler(AddressOf Me.DownloadCallback)
    End Sub

    '============== miscellenious

    Public Shared Sub VisitWebsite()
        Process.Start("http://ptftp.net")
    End Sub

    Private Sub AddOp(dest As String, type As FTP_OPERATION_TYPE)
        Dim q = Queue.Synchronized(opFileQueue)

        SyncLock q.SyncRoot
            While q.Count > 0
                Dim pendingFile As PendingOpFile = q.Dequeue()
                queueWindow.EnqueueOperation(New FtpOperation(pendingFile, dest + "/" + pendingFile.name, type))
            End While
            q.Clear()
        End SyncLock

        queueWindow.TriggerUpdate()
    End Sub

    Private Sub DownloadCallback(path As String)
        AddOp(path, FTP_OPERATION_TYPE.DOWNLOAD)
    End Sub

    Private Sub PrepareOpFileQueue()
        ' prepare op file queue
        Dim q As Queue = Queue.Synchronized(opFileQueue)
        SyncLock q.SyncRoot
            For Each item As ListViewItem In ListView1.SelectedItems
                Dim f As FtpFile = files(item.Text)
                q.Enqueue(New PendingOpFile(user.GetCwd(), f))
            Next
        End SyncLock
    End Sub

    Private Sub RequestEnded(resp)
        If Not String.IsNullOrEmpty(resp) Then
            MessageBox.Show(resp)
        End If

        Reload()
    End Sub

    Private Sub listViewRename()
        If Not ListView1.FocusedItem Is Nothing Then
            ListView1.LabelEdit = True
            ListView1.FocusedItem.BeginEdit()
        End If
    End Sub

    Private Sub listViewDelete()
        For Each item As ListViewItem In ListView1.SelectedItems
            Dim resp = Login.user.DeleteFile(user.GetCwd() + "/" + item.Text)
            RequestEnded(resp)
        Next
    End Sub

    Private Sub Reload()
        Dim filesStrList As String
        files.Clear()
        filesStrList = Login.user.SetDirectory(".")

        Dim il = New ImageList()
        il.Images.Add(dirIcon)
        il.Images.Add(fileIcon)

        If (ListView1.FindForm IsNot Nothing) Then
            Dim width = Math.Sqrt(Math.Pow(ListView1.FindForm.Size().Height(), 2) + Math.Pow(ListView1.FindForm.Size().Width(), 2)) / imgScale
            il.ImageSize = New Size(width, imgProportion * width)
        End If

        ListView1.LargeImageList = il
        ListView1.Items.Clear()

        Dim arr = filesStrList.Split(Environment.NewLine)

        For Each file As String In arr 'Seperate to a list
            ' skip "."
            If Not arr.First.Equals(file) Then
                file = file.Substring(1, file.Length - 1)
            End If

            If file.Equals("") Then
                ' include ".."
                ListView1.Items.Add("..", "..", 0)
            Else
                Dim parsedFile = New FtpFile(file)

                ' update image index
                Dim imageIndex = 0
                If Not parsedFile.isDirectory Then
                    imageIndex = 1
                End If

                parsedFile.imageIndex = imageIndex

                ' Add file
                files(parsedFile.Name) = parsedFile
                ListView1.Items.Add(parsedFile.Name, parsedFile.Name, imageIndex)
            End If
        Next
    End Sub

    Public Sub TriggerUpdate()
        Reload()
    End Sub

    Public Shared Function FilenameIsOK(ByVal fileName As String) As Boolean
        If fileName = "" Then
            Return False
        End If

        Dim file As String = Path.GetFileName(fileName)
        Dim directory As String = Path.GetDirectoryName(fileName)

        Return Not (file.Intersect(Path.GetInvalidFileNameChars()).Any() _
                OrElse
                directory.Intersect(Path.GetInvalidPathChars()).Any())
    End Function

    '============== gui

    Private Sub ListView1_ItemActivate(sender As ListView, e As EventArgs) _
     Handles ListView1.ItemActivate

        Dim item = sender.SelectedItems.Item(0)
        Dim name = item.Text

        If item.ImageIndex = 0 Then
            ' directory
            Login.user.SetDirectory(name)
            RequestEnded("")
        Else
            Dim path = OpenFile.GetPath(name, user)
            Dim file = Nothing

            If openFiles.ContainsKey(path) Then
                file = openFiles(path)
            Else
                file = New OpenFile(name, user)
                openFiles(path) = file
            End If

            file.Open()
        End If

        ContextMenuStrip1.Enabled = False
    End Sub

    'Form loading
    Private Sub Main_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ContextMenuStrip1.Enabled = False
        System.IO.Directory.CreateDirectory("tmp")
        Reload()
        queueWindow = New QueueWindow(Me)
        queueWindow.Show()
        queueWindow.Location = New Point(Me.Location.X, Me.Location.Y + Me.Height)
    End Sub

    Private Sub Main_ResizeEnd(sender As Object, e As EventArgs) _
     Handles MyBase.ResizeEnd
        Reload()
    End Sub

    Private Sub ExitToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ExitToolStripMenuItem.Click
        Login.user.Close() 'close connection
        Me.Close()
    End Sub

    Private Sub AboutToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles AboutToolStripMenuItem.Click
        Dim ab = New AboutBox1()
        ab.ShowDialog()
    End Sub

    '============ list view stuff

    Private Sub ListView1_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ListView1.SelectedIndexChanged
        If ListView1.SelectedItems.Count() = 0 Then
            ContextMenuStrip1.Enabled = False
            Return
        Else
            ContextMenuStrip1.Enabled = True
        End If
    End Sub

    Private Sub IconsToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles IconsToolStripMenuItem.Click
        ListToolStripMenuItem.Checked = False
        ListView1.View = View.LargeIcon
    End Sub

    Private Sub ListToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ListToolStripMenuItem.Click
        IconsToolStripMenuItem.Checked = False
        ListView1.View = View.List
    End Sub

    Private Sub ListView_BeforeLabelEdit(sender As Object, e As System.Windows.Forms.LabelEditEventArgs) Handles ListView1.BeforeLabelEdit
        renameOriginalLabel = ListView1.FocusedItem.Text
    End Sub

    Private Sub ListView_AfterLabelEdit(sender As Object, e As System.Windows.Forms.LabelEditEventArgs) Handles ListView1.AfterLabelEdit
        ListView1.LabelEdit = False

        If e.Label Is Nothing Then
            Return
        End If

        If e.Label = renameOriginalLabel Or Not FilenameIsOK(e.Label) Then
            e.CancelEdit = True
            Return
        End If

        Dim opFile = New PendingOpFile(user.GetCwd(), renameOriginalLabel)
        queueWindow.EnqueueOperation(New FtpOperation(opFile, e.Label, FTP_OPERATION_TYPE.RENAME))
        queueWindow.TriggerUpdate()
    End Sub


    Private Sub ListView_KeyDown(sender As Object, e As KeyEventArgs) Handles ListView1.KeyDown
        If e.KeyCode = Keys.F2 Then
            listViewRename()
        ElseIf e.KeyCode = Keys.F5 Then
            Reload()
        ElseIf e.KeyCode = Keys.Delete Then
            listViewDelete()
        End If

    End Sub

    '=============== context menu

    Private Sub RenameToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles RenameToolStripMenuItem.Click
        listViewRename()
    End Sub

    Private Sub DeleteToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles DeleteToolStripMenuItem.Click
        listViewDelete()
    End Sub

    Private Sub ContextMenuStrip1_Opening(sender As Object, e As System.ComponentModel.CancelEventArgs) Handles ContextMenuStrip1.Opening
        If ListView1.SelectedItems.Count() = 0 Then
            ContextMenuStrip1.Hide()
        End If
    End Sub

    '=============== drag n drop
    Private Sub ListView1_DragEnter(sender As Object, e As DragEventArgs) Handles ListView1.DragEnter
        If e.Data.GetDataPresent(DataFormats.FileDrop) Then
            e.Effect = DragDropEffects.Copy
        Else
            e.Effect = DragDropEffects.None
        End If
    End Sub

    Private Sub ListView1_DragDrop(sender As Object, e As DragEventArgs) Handles ListView1.DragDrop
        Dim files = e.Data.GetData(DataFormats.FileDrop)

        If files.Length.Equals(1) Then
            Dim fileInfo = New FileInfo(files(0))

            If fileInfo.Name = externalDropHandler.tmpFileName Then
                'Get target from mouse position
                Dim p As Point = ListView1.PointToClient(MousePosition)
                Dim item As ListViewItem = ListView1.GetItemAt(p.X, p.Y)

                If item IsNot Nothing Then
                    AddOp(item.Text, FTP_OPERATION_TYPE.MOVE)
                End If

                Exit Sub
            End If
        End If

        For Each file In files
            Dim fileInfo = New FileInfo(file)

            If String.IsNullOrWhiteSpace(fileInfo.Extension) Then Exit Sub
            Dim resp = ""
            If sender Is ListView1 Then
                Dim opFile = New PendingOpFile(fileInfo.DirectoryName, fileInfo.Name, fileInfo.Length)
                queueWindow.EnqueueOperation(New FtpOperation(opFile, user.GetCwd() + "/" + fileInfo.Name, FTP_OPERATION_TYPE.UPLOAD))
            End If
        Next

        queueWindow.TriggerUpdate()
    End Sub

    Private Sub ListView1_OnItemDrag(ByVal sender As Object, ByVal m As System.Windows.Forms.ItemDragEventArgs) Handles ListView1.ItemDrag
        If ListView1.SelectedItems.Count = 0 Then Return

        Dim fileas As String() = New [String](0) {}
        fileas(0) = My.Application.Info.DirectoryPath & "\" & externalDropHandler.tmpFileName
        Dim dta = New DataObject(DataFormats.FileDrop, fileas)
        dta.SetData(DataFormats.StringFormat, fileas)

        PrepareOpFileQueue()

        DoDragDrop(dta, DragDropEffects.Copy)

    End Sub

    '=============== closing stuff

    Private Sub Main_Closing(sender As Object, e As System.ComponentModel.CancelEventArgs) Handles MyBase.Closing
        System.IO.Directory.Delete("tmp", True)
        queueWindow.Close()
    End Sub

    Private Sub SiteToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles SiteToolStripMenuItem.Click
        Main.VisitWebsite()
    End Sub

    Private Sub PreferencesToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles PreferencesToolStripMenuItem.Click
        Dim preferences = New PreferencesForm()
        preferences.Show()
    End Sub
End Class
