Imports System.IO
Imports System.Diagnostics

Public Class Form1
    Private getFiles() As String

    Private PSI As New ProcessStartInfo()
    Private P As New Process

    Private Const cmdPush As String = "/c adb push ""{0}"" {1} "
    Private Const cmdChmod As String = " && adb shell chmod {0} {1}/{2}"

    Private Sub Form1_DragDrop(ByVal sender As Object, ByVal e As System.Windows.Forms.DragEventArgs) Handles Me.DragDrop
        getFiles = CType(e.Data.GetData(DataFormats.FileDrop), String())

        Me.Cursor = Cursors.WaitCursor

        Select Case ComboBox3.SelectedIndex
            Case 0
                adbpush()
            Case 1
                install()
            Case 2
                install("-r ")
        End Select

        Me.Cursor = Cursors.Default
    End Sub

    Private Sub Form1_DragEnter(ByVal sender As Object, ByVal e As System.Windows.Forms.DragEventArgs) Handles Me.DragEnter
        If e.Data.GetDataPresent(DataFormats.FileDrop) = False Then
            Return
        End If

        Dim FilePath() As String = CType(e.Data.GetData(DataFormats.FileDrop), String())

        For idx As Integer = 0 To FilePath.Length - 1
            If Not File.Exists(FilePath(idx)) Then
                Return
            End If

            If ComboBox3.SelectedIndex <> 0 Then
                If String.Compare(Path.GetExtension(FilePath(idx)), ".apk", True) <> 0 Then
                    Return
                End If
            End If
        Next idx

        e.Effect = DragDropEffects.Copy
    End Sub

    Private Sub SetPushDirCMB()
        With ComboBox1.Items
            .Clear()
            .Add("/data/local/tmp")
            .Add("/data/local")
            .Add("/data/root")

            .Add("/system/app")
            .Add("/system/etc")
            .Add("/system/lib")
            .Add("/system/framework")
            .Add("/system/xbin")
            .Add("/system/vendor")
            .Add("/system/usr")
            .Add("/system/lib/hw")
            .Add("/system/lib/egl")
        End With
        ComboBox1.SelectedIndex = 0
    End Sub

    Private Sub SetAnyCMB()
        With ComboBox2.Items
            .Clear()
            .Add("0777")
            .Add("0666")
            .Add("0755")
            .Add("0644")
        End With

        With ComboBox3.Items
            .Clear()
            .Add("ファイル転送")
            .Add("インストール")
            .Add("アップデート")
        End With

        ComboBox2.SelectedIndex = 0
        ComboBox3.SelectedIndex = 0
    End Sub

    Private Sub Form1_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        Dim ArgCmd As String = Microsoft.VisualBasic.Command()

        SetPushDirCMB()
        SetAnyCMB()

        InitPSI()

        runCommand("/c adb start-server", False)

        If ArgCmd.Length > 0 Then
            ArgCmd = ArgCmd.Replace("""", "")
            If File.Exists(ArgCmd) Then
                getFiles = {ArgCmd}
                adbpush()
            End If
        End If
    End Sub

    Private Sub InitPSI()
        With PSI
            .FileName = System.Environment.GetEnvironmentVariable("ComSpec")
            .RedirectStandardInput = False
            .RedirectStandardOutput = True
            .UseShellExecute = False
            .CreateNoWindow = True
        End With
    End Sub

    Private Sub adbpush()
        If ComboBox1.Text = "" Then
            MessageBox.Show("push先どこ？")
            Return
        End If

        Dim cmd As String
        For idx As Integer = 0 To getFiles.Length - 1
            cmd = String.Format(cmdPush, getFiles(idx), ComboBox1.Text)
            If ComboBox2.Text <> "" Then
                cmd &= String.Format(cmdChmod, ComboBox2.Text, ComboBox1.Text, Path.GetFileName(getFiles(idx)))
            End If

            TextBox2.Text &= cmd & vbNewLine

            If runCommand(cmd) Then
                TextBox1.Text &= Path.GetFileName(getFiles(idx)) & "を転送しました。" & vbNewLine
            End If
        Next
    End Sub

    Private Sub install(Optional ByVal m As String = "")
        Dim cmd As String
        For idx As Integer = 0 To getFiles.Length - 1
            cmd = "/c adb install " + m + """" + getFiles(idx) + """"
            runCommand(cmd)
        Next
    End Sub

    Private Function runCommand(Cmd As String, Optional log As Boolean = True) As Boolean
        PSI.Arguments = Cmd

        P = Process.Start(PSI)
        With P
            If log Then
                TextBox1.Text &= .StandardOutput.ReadToEnd
            End If

            If CheckBox1.Checked Then
                .WaitForExit(10000)
            Else
                .WaitForExit()
            End If

            If .HasExited = False Then
                If .Responding Then
                    .Close()
                Else
                    .Kill()
                End If

                TextBox1.Text &= "タイムアウトしました。"
                Return False
            End If
        End With

        Return True
    End Function

    Private Sub ComboBox3_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ComboBox3.SelectedIndexChanged
        Dim b As Boolean = (ComboBox3.SelectedIndex = 0)
        ComboBox1.Enabled = b
        ComboBox2.Enabled = b
    End Sub

    Private Sub TextBox1_DoubleClick(sender As Object, e As System.EventArgs) Handles TextBox1.DoubleClick
        TextBox1.Text = ""
    End Sub

    Private Sub TextBox2_DoubleClick(sender As Object, e As System.EventArgs) Handles TextBox2.DoubleClick
        TextBox2.Text = ""
    End Sub
End Class
