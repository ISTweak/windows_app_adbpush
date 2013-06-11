Imports System.IO

Public Delegate Sub dlgAddCmd(m As String)
Public Delegate Sub dlgAddLog(m As String)

Public Class adb
    Private Const cmdPush As String = "push ""{0}"" {1} "
    Private Const cmdChmod As String = "shell chmod {0} {1}/{2}"
    Private Const cmdInstall As String = "install {0}""{1}"""

    Private PS As New Process

    Private addCmd As dlgAddCmd
    Private addLog As dlgAddLog

    Public Sub New(addc As dlgAddCmd, addl As dlgAddLog)
        addCmd = addc
        addLog = addl
    End Sub

    Public Sub Start()
        PS = New Process
        Using PS
            addCmd("adb.exe start-server")

            With PS
                .StartInfo.UseShellExecute = False
                .StartInfo.RedirectStandardInput = False
                .StartInfo.RedirectStandardOutput = True
                .StartInfo.RedirectStandardError = True
                .StartInfo.CreateNoWindow = True
                .StartInfo.FileName = "adb.exe"
                .StartInfo.Arguments = "start-server"

                .Start()
                For i As Integer = 1 To 2
                    addLog(.StandardOutput.ReadLine)
                Next i
                .Close()
                .Dispose()
            End With
        End Using
    End Sub

    Private Function PSStart(cmd As String) As Integer
        Dim ext As Integer = 0

        With PS
            .StartInfo.Arguments = cmd
            .Start()

            addCmd(cmd)
            addLog(.StandardError.ReadToEnd)
            addLog(.StandardOutput.ReadToEnd)

            .WaitForExit()
            ext = .ExitCode
            .Close()
        End With

        Return ext
    End Function

    Public Sub Push(files() As String, ppath As String, Optional chmod As String = "")
        PS = New Process
        Using PS
            With PS
                .StartInfo.UseShellExecute = False
                .StartInfo.RedirectStandardInput = True
                .StartInfo.RedirectStandardOutput = True
                .StartInfo.RedirectStandardError = True
                .StartInfo.CreateNoWindow = True
                .StartInfo.FileName = "adb"

                For i As Integer = 0 To files.Length - 1
                    If PSStart(String.Format(cmdPush, files(i), ppath)) = 0 And chmod.Length > 0 Then
                        PSStart(String.Format(cmdChmod, chmod, ppath, Path.GetFileName(files(i))))
                    End If
                Next i
                .Dispose()
            End With
        End Using
    End Sub

    Public Sub Install(files() As String, m As String)
        PS = New Process
        Using PS
            With PS
                .StartInfo.UseShellExecute = False
                .StartInfo.RedirectStandardInput = True
                .StartInfo.RedirectStandardOutput = True
                .StartInfo.RedirectStandardError = True
                .StartInfo.CreateNoWindow = True
                .StartInfo.FileName = "adb"

                For i As Integer = 0 To files.Length - 1
                    PSStart(String.Format(cmdInstall, m, files(i)))
                Next i
                .Dispose()
            End With
        End Using
    End Sub

    Public Function SerchProcess() As Boolean
        Dim ps As System.Diagnostics.Process() = System.Diagnostics.Process.GetProcessesByName("adb")
        If ps.Count > 0 Then
            Return True
        End If
        Return False
    End Function
End Class
