Imports System.Collections.Specialized
Imports System.IO
Imports System.Text.RegularExpressions
Imports System.Threading
Imports PTFTP.My.Resources

Public Class Main
    Private files As Hashtable = New Hashtable
    Private openFiles As Hashtable = New Hashtable
    Private imgScale As Integer = 15
    Private imgProportion As Integer = 1

    Private renameOriginalLabel As String = ""
    Private Shared opFileQueue As New Queue
    Private externalDropHandler As ExternalDropHandler
    Private user As User = Login.user

    Public Shared sizedImageList As New ImageList()
    Public Shared queueWindow As QueueWindow

    Public Sub New(res As String)

        MyBase.New()
        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.

        Me.FileToolStripMenuItem.Text = GlobalStrings.file
        Me.ExitToolStripMenuItem.Text = GlobalStrings.exitText

        Me.EditToolStripMenuItem.Text = GlobalStrings.edit
        Me.PreferencesToolStripMenuItem.Text = GlobalStrings.preferences

        Me.ViewToolStripMenuItem.Text = GlobalStrings.view
        Me.IconsToolStripMenuItem.Text = GlobalStrings.icons
        Me.ListToolStripMenuItem.Text = GlobalStrings.list

        Me.HelpToolStripMenuItem.Text = GlobalStrings.help
        Me.SiteToolStripMenuItem.Text = GlobalStrings.website
        Me.AboutToolStripMenuItem.Text = GlobalStrings.about

        Me.RenameToolStripMenuItem.Text = GlobalStrings.renameAction
        Me.DeleteToolStripMenuItem.Text = GlobalStrings.delete

        Me.RefreshToolStripButton.Text = GlobalStrings.refresh
        Me.NewFolderToolStripButton.Text = GlobalStrings.new_folder

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
        If opFileQueue.Count > 0 Then
            AddOp(path, FTP_OPERATION_TYPE.DOWNLOAD)
        End If
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

        'build message with selected file list
        Dim separator = "- "
        Dim msg = GlobalStrings.ask_should_delete
        For Each item As ListViewItem In ListView1.SelectedItems
            msg += Environment.NewLine + separator + item.Text
        Next

        'ask to confirm deletion
        Dim confirm As Integer = MessageBox.Show(msg, GlobalStrings.delete, MessageBoxButtons.YesNo)
        If confirm = DialogResult.No Then
            Exit Sub
        End If

        'confirmed: delete
        For Each item As ListViewItem In ListView1.SelectedItems
            Dim resp = ""
            Dim file As FtpFile = files(item.Text)

            If (file.isDirectory) Then
                resp = Login.user.DeleteDirectory(user.GetCwd() + "/" + item.Text)
            Else
                resp = Login.user.DeleteFile(user.GetCwd() + "/" + item.Text)
            End If

            RequestEnded(resp)
        Next
    End Sub

    Private Sub Reload()
        files.Clear()
        Dim filesStrList As String = Login.user.GetDirectory(user.GetCwd())

        ListView1.Items.Clear()

        Dim filesArr = FtpFile.ParseStringArray(filesStrList.Split(Environment.NewLine))
        For Each parsedFile As FtpFile In filesArr
            files(parsedFile.Name) = parsedFile
            ListView1.Items.Add(parsedFile.Name, parsedFile.Name, parsedFile.imageIndex)
        Next

        CwdToolStripLabel.Text = user.cwd

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

        If sender.SelectedItems.Count = 0 Then
            Exit Sub
        End If

        Dim item = sender.SelectedItems.Item(0)
        Dim name = item.Text

        Dim remoteFile As FtpFile = files(name)

        If remoteFile.isDirectory Then
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

    Public Shared Sub updateSingleImage(key As String)
        sizedImageList.Images.Add(key, IconFinder.imageList.Images.Item(key))
    End Sub

    Public Shared Sub updateSizedImageList()
        sizedImageList.Images.Clear()
        For Each key In IconFinder.imageList.Images.Keys()
            updateSingleImage(key)
        Next
    End Sub

    Private Sub setSizedImageList()
        sizedImageList.ColorDepth = ColorDepth.Depth32Bit

        If (ListView1.FindForm IsNot Nothing) Then
            Dim width = Math.Sqrt(Math.Pow(ListView1.FindForm.Size().Height(), 2) + Math.Pow(ListView1.FindForm.Size().Width(), 2)) / imgScale
            sizedImageList.ImageSize = New Size(width, imgProportion * width)
        End If

        updateSizedImageList()
        ListView1.LargeImageList = sizedImageList
    End Sub

    'Form loading
    Private Sub Main_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        System.IO.Directory.CreateDirectory("tmp")
        ContextMenuStrip1.Enabled = False
        IconFinder.setupImageList()
        setSizedImageList()
        Reload()

        queueWindow = New QueueWindow(Me)
        queueWindow.Show()
        queueWindow.Location = New Point(Me.Location.X, Me.Location.Y + Me.Height)
    End Sub

    Private Sub Main_ResizeEnd(sender As Object, e As EventArgs) Handles MyBase.ResizeEnd
        setSizedImageList()
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
        '@TODO: set small image list
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

            Dim resp = ""
            If sender Is ListView1 Then
                Dim isDir = Directory.Exists(fileInfo.FullName)
                Dim length = 0

                If Not isDir Then
                    length = fileInfo.Length
                End If

                Dim opFile = New PendingOpFile(fileInfo.DirectoryName, fileInfo.Name, isDir, length)
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

    Private Sub RefreshToolStripButton_Click(sender As Object, e As EventArgs) Handles RefreshToolStripButton.Click
        Reload()
    End Sub

    Private Sub NewFolderToolStripButton_Click(sender As Object, e As EventArgs) Handles NewFolderToolStripButton.Click
        user.CreateDirectory(user.GetNewDirUri(user.GetCwd()))
        Reload()
    End Sub

End Class
