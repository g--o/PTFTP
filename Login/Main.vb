Imports System.Collections.Specialized
Imports System.IO

Public Class Main
    Private fileList As String
    Private dirIcon = "./dir.png"
    Private imgScale = 15
    Private imgProportion = 1
    Private selectedList As ArrayList
    Private user = Login.user

    Public Sub New(res As String)

        MyBase.New()
        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        fileList = res

    End Sub

    Private Sub ListView1_ItemActivate(sender As ListView, e As EventArgs) _
     Handles ListView1.ItemActivate

        Dim name = sender.SelectedItems.Item(0).Text
        fileList = Login.user.SetDirectory(name)
        RequestEnded("")
    End Sub

    Private Sub Reload()
        fileList = Login.user.SetDirectory(".")

        Dim il = New ImageList()
        il.Images.Add(Bitmap.FromFile(dirIcon))

        If (ListView1.FindForm IsNot Nothing) Then
            Dim width = Math.Sqrt(Math.Pow(ListView1.FindForm.Size().Height(), 2) + Math.Pow(ListView1.FindForm.Size().Width(), 2)) / imgScale
            il.ImageSize = New Size(width, imgProportion * width)
        End If

        ListView1.LargeImageList = il
        ListView1.Items.Clear()

        'My.Computer.FileSystem.WriteAllText("tmp.txt", fileList, True)
        Dim arr = fileList.Split(Environment.NewLine)

        For Each file As String In arr 'נפריד את הקבצים לרשימה
            If Not arr.First.Equals(file) Then
                file = file.Substring(1, file.Length - 1)
            End If
            If file.Equals("") Then
                file = ".."
            End If

            ListView1.Items.Add(file, file, 0)
        Next
    End Sub

    'טעינה של החלון
    Private Sub Main_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        setupFSMs()

        If Not IO.File.Exists(My.Application.Info.DirectoryPath & "\\tmp_donot_delete.antf2") Then
            IO.File.Create(My.Application.Info.DirectoryPath & "\\tmp_donot_delete.antf2")
            SetAttr(My.Application.Info.DirectoryPath & "\\tmp_donot_delete.antf2", FileAttribute.Hidden + FileAttribute.System)
        End If

        ContextMenuStrip1.Enabled = False
        Reload()
    End Sub

    Public Sub setupFSMs()
        Dim Drives() As IO.DriveInfo = IO.DriveInfo.GetDrives 'This retrieves a complete list of the drives used in the pc.

        For i As Integer = 0 To Drives.Length - 1      'Make a loop to add filesystemwatcher to EVERY drive and thier sub folders.

            If Drives(i).DriveType = IO.DriveType.Fixed Then
                If Drives(i).IsReady Then
                    Dim FSM As IO.FileSystemWatcher = New IO.FileSystemWatcher(Drives(i).RootDirectory.FullName, "*.antf2") 'antf2 is my ext use your own if you wish but careful to change it sith the rest of the code.

                    AddHandler FSM.Created, AddressOf OnChanged
                    FSM.IncludeSubdirectories = True
                    FSM.EnableRaisingEvents = True
                End If
            End If
        Next
    End Sub

    Sub OnChanged(ByVal source As Object, ByVal e As IO.FileSystemEventArgs)
        Dim output = IO.Path.GetDirectoryName(e.FullPath)

        Dim files = New ArrayList(selectedList)
        selectedList.Clear()

        DownloadTo(files, output)

        Dim locator = output.Trim("'") & "\tmp_donot_delete.antf2"
        If File.Exists(locator) Then
            My.Computer.FileSystem.DeleteFile(locator)
        End If
    End Sub

    Private Sub Main_ResizeEnd(sender As Object, e As EventArgs) _
     Handles MyBase.ResizeEnd
        Reload()
    End Sub

    Private Sub ExitToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ExitToolStripMenuItem.Click
        Login.user.Close() 'סגור תחיבור
        Me.Close()
    End Sub

    Private Sub AboutToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles AboutToolStripMenuItem.Click
        Dim ab = New AboutBox1()
        ab.ShowDialog()
    End Sub

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

    Private Sub RenameToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles RenameToolStripMenuItem.Click

    End Sub

    Private Sub DeleteToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles DeleteToolStripMenuItem.Click
        For Each item As ListViewItem In ListView1.SelectedItems
            Dim resp = Login.user.DeleteFile(item.Text)
            RequestEnded(resp)
        Next
    End Sub

    Private Sub ContextMenuStrip1_Opening(sender As Object, e As System.ComponentModel.CancelEventArgs) Handles ContextMenuStrip1.Opening
        If ListView1.SelectedItems.Count() = 0 Then
            ContextMenuStrip1.Hide()
        End If
    End Sub

    'פונקציות גרירה
    Private Sub ListView1_DragEnter(sender As Object, e As DragEventArgs) Handles ListView1.DragEnter
        If e.Data.GetDataPresent(DataFormats.FileDrop) Then
            e.Effect = DragDropEffects.Copy
        Else
            e.Effect = DragDropEffects.None
        End If
    End Sub

    Private Sub ListView1_DragDrop(sender As Object, e As DragEventArgs) Handles ListView1.DragDrop
        Dim files = e.Data.GetData(DataFormats.FileDrop)

        For Each file In files
            Dim fileInfo = New FileInfo(file)

            If String.IsNullOrWhiteSpace(fileInfo.Extension) Then Exit Sub
            Dim resp = ""
            If sender Is ListView1 Then
                resp = Login.user.UploadFile(fileInfo)
            End If
            RequestEnded(resp)
        Next
    End Sub

    Private Sub DownloadTo(files As ArrayList, path As String)
        For Each fileName In files
            user.DownloadFile(fileName, path + "/" + fileName)
        Next
    End Sub

    Private Sub ListView1_OnItemDrag(ByVal sender As Object, ByVal m As System.Windows.Forms.ItemDragEventArgs) Handles ListView1.ItemDrag
        If ListView1.SelectedItems.Count = 0 Then Return

        Dim fileas As String() = New [String](0) {}
        fileas(0) = My.Application.Info.DirectoryPath & "\tmp_donot_delete.antf2"
        Dim dta = New DataObject(DataFormats.FileDrop, fileas)
        dta.SetData(DataFormats.StringFormat, fileas)

        ' prepare download array
        selectedList = New ArrayList()
        For Each item As ListViewItem In ListView1.SelectedItems
            selectedList.Add(item.Text)
        Next

        DoDragDrop(dta, DragDropEffects.Move)
    End Sub

    Private Sub RequestEnded(resp)
        If Not String.IsNullOrEmpty(resp) Then
            MessageBox.Show(resp)
        End If

        Reload()
    End Sub
End Class
