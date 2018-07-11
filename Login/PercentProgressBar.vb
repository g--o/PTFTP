﻿' Thanks to IronRazerz from :
' https://social.msdn.microsoft.com/Forums/vstudio/en-US/5d3eee65-730b-488f-a858-a341b8d61714/progressbar-with-percentage-label?forum=vbgeneral

Public Class PercentProgressBar
    Inherits ProgressBar

    Public Sub New()
        Me.SetStyle(ControlStyles.OptimizedDoubleBuffer Or ControlStyles.UserPaint, True)
    End Sub

    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        ProgressBarRenderer.DrawHorizontalBar(e.Graphics, New Rectangle(0, 0, Me.Width, Me.Height))
        Dim ProgressWidth As Integer = CInt((Me.Width / (Me.Maximum - Me.Minimum)) * Me.Value)
        ProgressBarRenderer.DrawHorizontalChunks(e.Graphics, New Rectangle(0, 0, ProgressWidth, Me.Height))
        Dim ProgressPercent As Integer = CInt(((Me.Maximum - Me.Minimum) / 100) * Me.Value)
        TextRenderer.DrawText(e.Graphics, ProgressPercent.ToString & "%", SystemFonts.DefaultFont, New Rectangle(0, 2, Me.Width, Me.Height), Color.Black, TextFormatFlags.HorizontalCenter Or TextFormatFlags.VerticalCenter)
        MyBase.OnPaint(e)
    End Sub
End Class