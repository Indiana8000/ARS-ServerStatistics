Public Class Form1
    Dim oServer As BMC.ARSystem.Server
    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        If Date.Now.Year > 2020 Or Date.Now.Month > 9 Then
            MsgBox("Trial Period Expired! Please contact the Developer.", vbExclamation, "Unregistered Version")
            Application.Exit()
        End If

        oServer = New BMC.ARSystem.Server

        txt_Server.Text = My.Settings.Server
        txt_Port.Text = My.Settings.Port
        txt_Username.Text = My.Settings.Username
        If txt_Server.Text.Length > 0 Then
            txt_Password.Select()
        End If
    End Sub

    Private Sub btn_Connect_Click(sender As Object, e As EventArgs) Handles btn_Connect.Click
        Dim alForms As ArrayList

        My.Settings.Server = txt_Server.Text
        My.Settings.Port = txt_Port.Text
        My.Settings.Username = txt_Username.Text
        My.Settings.Save()

        Try
            oServer.Login(txt_Server.Text, txt_Username.Text, txt_Password.Text, "", "de_DE", "UTF-8", txt_Port.Text)
            alForms = oServer.GetListForm()
            btn_Connect.Enabled = False
            btn_disconnect.Enabled = True
            btn_exec.Enabled = True
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try
    End Sub

    Private Sub btn_disconnect_Click(sender As Object, e As EventArgs) Handles btn_disconnect.Click
        oServer.Logout()
        btn_Connect.Enabled = True
        btn_disconnect.Enabled = False
        btn_exec.Enabled = False
    End Sub

    Private Sub txt_Password_KeyPress(sender As Object, e As KeyPressEventArgs) Handles txt_Password.KeyPress
        If txt_Password.Text.Length > 0 And Asc(e.KeyChar) = 13 Then
            btn_Connect.Select()
            Application.DoEvents()
            btn_Connect_Click(sender, New EventArgs())
        End If
    End Sub

    Private Sub btn_exec_Click(sender As Object, e As EventArgs) Handles btn_exec.Click
        btn_exec.Enabled = False
        Application.DoEvents()
        Dim xColor() As Color = {Color.Red, Color.Blue, Color.Green, Color.Pink, Color.Purple, Color.DarkGray}

        Dim requestFields As BMC.ARSystem.EntryListFieldList = New BMC.ARSystem.EntryListFieldList
        requestFields.Add(New BMC.ARSystem.EntryListField(3)) 'Time
        requestFields.Add(New BMC.ARSystem.EntryListField(913)) 'API Proc Time
        requestFields.Add(New BMC.ARSystem.EntryListField(940)) 'Filter Proc Time
        requestFields.Add(New BMC.ARSystem.EntryListField(953)) 'DB SQL Proc Time
        requestFields.Add(New BMC.ARSystem.EntryListField(951)) 'ARServer Proc Time
        'requestFields.Add(New BMC.ARSystem.EntryListField(958)) 'Network Time
        requestFields.Add(New BMC.ARSystem.EntryListField(905)) 'User Count

        For i = 0 To requestFields.Count - 1
            If Chart1.Series.Count > i Then
                Chart1.Series("Series" & i).Points.Clear()
            Else
                Chart1.Series.Add("Series" & i)
                Chart1.Series("Series" & i).ChartType = DataVisualization.Charting.SeriesChartType.Line
                Chart1.Series("Series" & i).Color = xColor(i)
                Chart1.Series("Series" & i).BorderWidth = 3
            End If
        Next


        Dim row As BMC.ARSystem.EntryFieldValue
        Dim result As BMC.ARSystem.EntryFieldValueList
        Dim last() As Integer = {0, 0, 0, 0, 0, 0, 0, 0, 0}
        Dim xDate As DateTime
        Dim xPos As Integer
        Dim uTime As Double
        uTime = (DateTimePicker1.Value.Date - New Date(1970, 1, 1, 0, 0, 0)).TotalSeconds
        Console.WriteLine("'3' >= " & uTime & " AND '3' < " & (uTime + 86400))
        result = oServer.GetListEntryWithFields("Server Statistics", "'3' >= " & uTime & " AND '3' < " & (uTime + 86400 * 7), requestFields, 0, 1000)


        Chart1.ChartAreas(0).AxisX.Interval = 1
        Chart1.ChartAreas(0).AxisX.LabelStyle.Angle = -90
        'Chart1.ChartAreas(0).AxisY.Maximum = 1000000



        Console.WriteLine("Count: " & result.Count)
        For Each row In result
            For i = 0 To requestFields.Count - 1
                If row.FieldValues.Keys(i) = 3 Then
                    xPos = i
                ElseIf row.FieldValues.Keys(i) = 905 Then
                    Chart1.Series("Series" & i).YAxisType = DataVisualization.Charting.AxisType.Secondary
                End If
            Next

            'xDate = New DateTime(1970, 1, 1, 0, 0, 0)
            'xDate = xDate.AddSeconds(row.FieldValues.Values(xPos))
            For i = 0 To requestFields.Count - 1
                If last(i) > 0 Then
                    If row.FieldValues.Values(i) - last(i) > 0 Then
                        'Chart1.Series("Series" & i).Points.Add(row.FieldValues.Values(i) - last(i))
                        If row.FieldValues.Keys(i) = 905 Then
                            Chart1.Series("Series" & i).Points.AddXY(row.FieldValues.Values(xPos), row.FieldValues.Values(i))
                        Else
                            Chart1.Series("Series" & i).Points.AddXY(row.FieldValues.Values(xPos), row.FieldValues.Values(i) - last(i))
                        End If
                    Else
                        'Chart1.Series("Series" & i).Points.Add(0)
                    End If
                Else

                    If row.FieldValues.Keys(i) = 913 Then
                        Chart1.Series("Series" & i).LegendText = "API"
                    ElseIf row.FieldValues.Keys(i) = 940 Then
                        Chart1.Series("Series" & i).LegendText = "FILTER"
                    ElseIf row.FieldValues.Keys(i) = 953 Then
                        Chart1.Series("Series" & i).LegendText = "SQL"
                    ElseIf row.FieldValues.Keys(i) = 951 Then
                        Chart1.Series("Series" & i).LegendText = "ARServer"
                    ElseIf row.FieldValues.Keys(i) = 958 Then
                        Chart1.Series("Series" & i).LegendText = "Network"
                    ElseIf row.FieldValues.Keys(i) = 905 Then
                        Chart1.Series("Series" & i).LegendText = "User"
                    Else
                        Chart1.Series("Series" & i).LegendText = row.FieldValues.Keys(i)
                    End If




                End If
                If i <> xPos Then last(i) = row.FieldValues.Values(i)
            Next
        Next

        btn_exec.Enabled = True
    End Sub

    Private Sub Chart1_Click(sender As Object, e As EventArgs) Handles Chart1.Click

    End Sub
End Class
