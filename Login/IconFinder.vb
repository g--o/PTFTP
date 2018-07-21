
Imports System.Runtime.InteropServices

Class IconFinder

    Enum IconSizeEnum
        Small
        Large
        ExtraLarge
        Jumbo
    End Enum

    Public Shared Sub setupImageList()
        imageList.Images.Clear()
        imageList.ImageSize = New Size(256, 256) 'Jumbo should be 256x256
        imageList.ColorDepth = ColorDepth.Depth32Bit

        Dim tmpIcon = IconFinder.GetDirIcon()
        If IsNothing(tmpIcon) Then
            tmpIcon = dirIcon
        End If
        imageList.Images.Add("<dir>", tmpIcon)

        tmpIcon = IconFinder.GetFileIcon("dummy_file")
        If IsNothing(tmpIcon) Then
            tmpIcon = fileIcon
        End If
        imageList.Images.Add("<file>", tmpIcon)

    End Sub

    Public Shared Function GetDirIcon() As Bitmap
        Return GetFileIcon("dummy_folder", True)
    End Function

    Public Shared Function GetFileIcon(ByVal FileName As String, Optional isDir As Boolean = False, Optional ByVal thisSize As IconSizeEnum = IconSizeEnum.Jumbo) As Bitmap
        Dim shinfo As New SHFILEINFOW()
        Dim myIcon As Icon
        Dim flags, res As Integer
        Dim fileAttribute = FILE_ATTRIBUTE_NORMAL

        If isDir Then
            fileAttribute = FILE_ATTRIBUTE_DIRECTORY
        End If

        Select Case thisSize
            Case IconSizeEnum.Small, IconSizeEnum.Large
                If thisSize = IconSizeEnum.Small Then
                    flags = SHGFI_ICON Or SHGFI_SMALLICON
                Else
                    flags = SHGFI_ICON Or SHGFI_LARGEICON
                End If

                flags = flags Or SHGFI_USEFILEATTRIBUTES

                res = SHGetFileInfoW(FileName, fileAttribute, shinfo, Marshal.SizeOf(shinfo), CUInt(flags))
                If res = 0 Then Throw (New System.IO.FileNotFoundException())

                myIcon = CType(Icon.FromHandle(shinfo.hIcon).Clone, Icon)
                DestroyIcon(shinfo.hIcon)

            Case IconSizeEnum.ExtraLarge, IconSizeEnum.Jumbo
                flags = SHGFI_SYSICONINDEX
                flags = flags Or SHGFI_USEFILEATTRIBUTES

                res = SHGetFileInfoW(FileName, fileAttribute, shinfo, Marshal.SizeOf(shinfo), CUInt(flags))
                If res = 0 Then Throw New IO.FileNotFoundException()

                Dim iconIndex As Integer = shinfo.iIcon
                Dim iidImageList As New Guid("46EB5926-582E-4017-9FDF-E8998DAA0950")
                Dim iml As IImageList = Nothing

                Dim size As Integer
                If thisSize = IconSizeEnum.ExtraLarge Then
                    size = CInt(SHIL_EXTRALARGE)
                Else
                    size = CInt(SHIL_JUMBO)
                End If

                SHGetImageList(size, iidImageList, iml)

                Dim hIcon As IntPtr = IntPtr.Zero
                iml.GetIcon(iconIndex, ILD_TRANSPARENT, hIcon)

                myIcon = CType(Icon.FromHandle(hIcon).Clone, Icon)
                DestroyIcon(hIcon)

            Case Else
                Return Nothing
        End Select

        Dim bm As Bitmap = System.Drawing.Icon.FromHandle(myIcon.Handle).ToBitmap

        Return bm

    End Function

    Private Const SHGFI_ICON As Integer = &H100
    Private Const SHGFI_LARGEICON As Integer = &H0
    Private Const SHGFI_SMALLICON As Integer = &H1
    Private Const SHGFI_USEFILEATTRIBUTES As Integer = &H10
    Private Const SHGFI_SYSICONINDEX As Integer = &H4000
    Private Const SHGFI_LINKOVERLAY As Integer = &H8000
    Private Const SHIL_JUMBO As Integer = &H4
    Private Const SHIL_EXTRALARGE As Integer = &H2
    Private Const ILD_TRANSPARENT As Integer = &H1
    Private Const FILE_ATTRIBUTE_NORMAL As UInteger = &H80
    Private Const FILE_ATTRIBUTE_DIRECTORY As UInteger = &H10

    Private Shared dirIcon As Bitmap = Bitmap.FromFile("./Assets/folder.png")
    Private Shared fileIcon As Bitmap = Bitmap.FromFile("./Assets/file.png")
    Public Shared imageList As New ImageList()

    '''
    ''' SHGetImageList is not exported correctly in XP.  See KB316931
    ''' http://support.microsoft.com/default.aspx?scid=kb;EN-US;Q316931
    ''' Apparently (and hopefully) ordinal 727 isn't going to change.
    '''
    <DllImport("shell32.dll", EntryPoint:="#727")>
    Private Shared Function SHGetImageList(ByVal iImageList As Integer, ByRef riid As Guid, ByRef ppv As IImageList) As Integer
    End Function

    <DllImport("shell32.dll", EntryPoint:="SHGetFileInfoW", CallingConvention:=CallingConvention.StdCall)>
    Private Shared Function SHGetFileInfoW(<MarshalAs(UnmanagedType.LPWStr)> ByVal pszPath As String, ByVal dwFileAttributes As UInteger, ByRef psfi As SHFILEINFOW, ByVal cbFileInfo As Integer, ByVal uFlags As UInteger) As Integer
    End Function

    <DllImport("shell32.dll", EntryPoint:="SHGetFileInfoW", CallingConvention:=CallingConvention.StdCall)>
    Private Shared Function SHGetFileInfoW(ByVal pszPath As IntPtr, ByVal dwFileAttributes As UInteger, ByRef psfi As SHFILEINFOW, ByVal cbFileInfo As Integer, ByVal uFlags As UInteger) As Integer
    End Function

    <DllImport("user32.dll", EntryPoint:="DestroyIcon")>
    Private Shared Function DestroyIcon(ByVal hIcon As IntPtr) As <MarshalAs(UnmanagedType.Bool)> Boolean
    End Function

    <StructLayout(LayoutKind.Sequential)>
    Private Structure RECT
        Public left, top, right, bottom As Integer
    End Structure

    <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.[Unicode])>
    Private Structure SHFILEINFOW
        Public hIcon As System.IntPtr
        Public iIcon As Integer
        Public dwAttributes As UInteger
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=260)> Public szDisplayName As String
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=80)> Public szTypeName As String
    End Structure

    <StructLayout(LayoutKind.Sequential)>
    Private Structure IMAGELISTDRAWPARAMS
        Public cbSize As Integer
        Public himl As IntPtr
        Public i As Integer
        Public hdcDst As IntPtr
        Public x As Integer
        Public y As Integer
        Public cx As Integer
        Public cy As Integer
        Public xBitmap As Integer
        Public yBitmap As Integer
        Public rgbBk As Integer
        Public rgbFg As Integer
        Public fStyle As Integer
        Public dwRop As Integer
        Public fState As Integer
        Public Frame As Integer
        Public crEffect As Integer
    End Structure

    <StructLayout(LayoutKind.Sequential)>
    Private Structure IMAGEINFO
        Public hbmImage As IntPtr
        Public hbmMask As IntPtr
        Public Unused1 As Integer
        Public Unused2 As Integer
        Public rcImage As RECT
    End Structure

#Region "Private ImageList COM Interop (XP)"
    <ComImport(), Guid("46EB5926-582E-4017-9FDF-E8998DAA0950")>
    <InterfaceType(ComInterfaceType.InterfaceIsIUnknown)>
    Private Interface IImageList
        <PreserveSig()> Function Add(ByVal hbmImage As IntPtr, ByVal hbmMask As IntPtr, ByRef pi As Integer) As Integer
        <PreserveSig()> Function ReplaceIcon(ByVal i As Integer, ByVal hicon As IntPtr, ByRef pi As Integer) As Integer
        <PreserveSig()> Function SetOverlayImage(ByVal iImage As Integer, ByVal iOverlay As Integer) As Integer
        <PreserveSig()> Function Replace(ByVal i As Integer, ByVal hbmImage As IntPtr, ByVal hbmMask As IntPtr) As Integer
        <PreserveSig()> Function AddMasked(ByVal hbmImage As IntPtr, ByVal crMask As Integer, ByRef pi As Integer) As Integer
        <PreserveSig()> Function Draw(ByRef pimldp As IMAGELISTDRAWPARAMS) As Integer
        <PreserveSig()> Function Remove(ByVal i As Integer) As Integer
        <PreserveSig()> Function GetIcon(ByVal i As Integer, ByVal flags As Integer, ByRef picon As IntPtr) As Integer
        <PreserveSig()> Function GetImageInfo(ByVal i As Integer, ByRef pImageInfo As IMAGEINFO) As Integer
        <PreserveSig()> Function Copy(ByVal iDst As Integer, ByVal punkSrc As IImageList, ByVal iSrc As Integer, ByVal uFlags As Integer) As Integer
        <PreserveSig()> Function Merge(ByVal i1 As Integer, ByVal punk2 As IImageList, ByVal i2 As Integer, ByVal dx As Integer, ByVal dy As Integer, ByRef riid As Guid, ByRef ppv As IntPtr) As Integer
        <PreserveSig()> Function Clone(ByRef riid As Guid, ByRef ppv As IntPtr) As Integer
        <PreserveSig()> Function GetImageRect(ByVal i As Integer, ByRef prc As RECT) As Integer
        <PreserveSig()> Function GetIconSize(ByRef cx As Integer, ByRef cy As Integer) As Integer
        <PreserveSig()> Function SetIconSize(ByVal cx As Integer, ByVal cy As Integer) As Integer
        <PreserveSig()> Function GetImageCount(ByRef pi As Integer) As Integer
        <PreserveSig()> Function SetImageCount(ByVal uNewCount As Integer) As Integer
        <PreserveSig()> Function SetBkColor(ByVal clrBk As Integer, ByRef pclr As Integer) As Integer
        <PreserveSig()> Function GetBkColor(ByRef pclr As Integer) As Integer
        <PreserveSig()> Function BeginDrag(ByVal iTrack As Integer, ByVal dxHotspot As Integer, ByVal dyHotspot As Integer) As Integer
        <PreserveSig()> Function EndDrag() As Integer
        <PreserveSig()> Function DragEnter(ByVal hwndLock As IntPtr, ByVal x As Integer, ByVal y As Integer) As Integer
        <PreserveSig()> Function DragLeave(ByVal hwndLock As IntPtr) As Integer
        <PreserveSig()> Function DragMove(ByVal x As Integer, ByVal y As Integer) As Integer
        <PreserveSig()> Function SetDragCursorImage(ByRef punk As IImageList, ByVal iDrag As Integer, ByVal dxHotspot As Integer, ByVal dyHotspot As Integer) As Integer
        <PreserveSig()> Function DragShowNolock(ByVal fShow As Integer) As Integer
        <PreserveSig()> Function GetDragImage(ByRef ppt As Point, ByRef pptHotspot As Point, ByRef riid As Guid, ByRef ppv As IntPtr) As Integer
        <PreserveSig()> Function GetItemFlags(ByVal i As Integer, ByRef dwFlags As Integer) As Integer
        <PreserveSig()> Function GetOverlayImage(ByVal iOverlay As Integer, ByRef piIndex As Integer) As Integer
    End Interface
#End Region

End Class