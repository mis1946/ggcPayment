﻿Imports System.Drawing
Imports System.Windows.Forms
Imports ggcAppDriver

Public Class frmPayGC
    Private WithEvents poGiftCert As GiftCerticate
    Private poAppDrvr As GRider
    Private pnLoadx As Integer
    Private poControl As Control
    Private pbCloseForm As Boolean
    Private pnActiveRow As Integer

    WriteOnly Property GiftCert() As GiftCerticate
        Set(ByVal oGiftCert As GiftCerticate)
            poGiftCert = oGiftCert
        End Set
    End Property

    ReadOnly Property CloseForm() As Boolean
        Get
            Return pbCloseForm
        End Get
    End Property

    Private Sub frmPayGC_KeyDown(sender As Object, e As System.Windows.Forms.KeyEventArgs) Handles Me.KeyDown
        Select Case e.KeyCode
            Case Keys.Escape
                pbCloseForm = False
                If Not IsNothing(p_oFormPayCredit) Then showForm(2, False)
                If Not IsNothing(p_oFormCheck) Then showForm(3, False)
                If Not IsNothing(p_oFormGC) Then showForm(4, False)
                If Not IsNothing(p_oFormDelivery) Then showForm(5, False)
                showForm(1, False)
            Case Keys.Return, Keys.Down
                SetNextFocus()
            Case Keys.Up
                SetPreviousFocus()
        End Select
    End Sub

    Private Sub frmPay_Load(sender As System.Object, e As System.EventArgs) Handles MyBase.Load
        setVisible()

        If pnLoadx = 0 Then
            showDetail(True)
            clearFields()
            Call grpEventHandler(Me, GetType(Button), "cmdButton", "Click", AddressOf cmdButton_Click)
            Call grpEventHandler(Me, GetType(TextBox), "txtField", "GotFocus", AddressOf txtField_GotFocus)
            Call grpEventHandler(Me, GetType(TextBox), "txtField", "LostFocus", AddressOf txtField_LostFocus)
            Call grpKeyHandler(Me, GetType(TextBox), "txtField", "KeyDown", AddressOf txtField_KeyDown)

            Dim row As DataRow
            txtField00.AutoCompleteCustomSource.Clear()
            For Each row In poGiftCert.SearchCompany.Rows
                txtField00.AutoCompleteCustomSource.Add(row.Item("sCompnyNm").ToString())
            Next

            txtField00.AutoCompleteSource = AutoCompleteSource.CustomSource
            txtField00.AutoCompleteMode = AutoCompleteMode.SuggestAppend

            'open by source code and number
            If poGiftCert.OpenBySource() Then loadOthers()

            pnLoadx = 1
        End If
    End Sub

    Private Sub cmdButton_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
        Dim loChk As Button
        loChk = CType(sender, System.Windows.Forms.Button)

        Dim lnIndex As Integer
        lnIndex = Val(Mid(loChk.Name, 10))

        Select Case lnIndex
            Case 0
                pbCloseForm = True
                If Not isEntryOk(True) Then GoTo endProc
                If poGiftCert.SaveTransaction() Then
                    Me.Close()
                    Me.Dispose()
                End If
            Case 1, 2, 3
                pbCloseForm = False
                If isEntryOk(False) Then
                    If Not poGiftCert.SaveTransaction() Then
                        Me.Close()
                        Me.Dispose()
                        GoTo endProc
                    End If
                End If

                'Me.Dispose()
                'Me.Close()

                Select Case lnIndex
                    Case 2
                        Dim loCreditCard As New CreditCard(poGiftCert.AppDriver)
                        loCreditCard.SourceCd = poGiftCert.SourceCd
                        loCreditCard.SourceNo = poGiftCert.SourceNo
                        loCreditCard.ShowCreditCard()
                    Case 3
                        Dim loCheck As New CheckPayment(poGiftCert.AppDriver)
                        loCheck.SourceCd = poGiftCert.SourceCd
                        loCheck.SourceNo = poGiftCert.SourceNo
                        loCheck.ShowCheck()
                    Case 1
                        Me.Hide()
                End Select
            Case 4 'GIFT CERT
            Case 5 'ADD CREDIT CARD
                If poGiftCert.AddGiftCert Then
                    Call loadOthers()
                    Call computeChange()
                End If
            Case 6 ' Delete credit Card
                'If poGiftCert.DeleteGC(pnActiveRow) Then
                '    Call loadOthers()
                '    Call computeChange()
                'End If
                If poGiftCert.ItemCount > 1 Then
                    If DataGridView1.RowCount - 1 > 0 Then
                        poGiftCert.DeleteGC(pnActiveRow)
                        loadOthers()
                    Else
                        poGiftCert.DeleteGC(pnActiveRow)
                        poGiftCert.AddGiftCert()
                        loadOthers()
                    End If
                Else
                    poGiftCert.Master(0, "scompnycd") = ""
                    poGiftCert.Master(0, "sReferNox") = ""
                    poGiftCert.Master(0, "dValidity") = ""
                    poGiftCert.Master(0, "sRemarksx") = ""
                    poGiftCert.Master(0, "nAmountxx") = 0

                    txtField00.Text = ""
                    txtField01.Text = ""
                    txtField02.Text = ""
                    txtField03.Text = ""
                    txtField04.Text = 0

                    loadOthers()
                End If
        End Select
endProc:
        Exit Sub
    End Sub

    Private Sub txtField_GotFocus(ByVal sender As Object, ByVal e As System.EventArgs)
        Dim loTxt As TextBox
        loTxt = CType(sender, System.Windows.Forms.TextBox)

        poControl = loTxt

        loTxt.BackColor = Color.Azure
        loTxt.SelectAll()
    End Sub

    Private Sub txtField_LostFocus(ByVal sender As Object, ByVal e As System.EventArgs)
        Dim loTxt As TextBox
        Dim loIndex As Integer
        loTxt = CType(sender, System.Windows.Forms.TextBox)

        loTxt.BackColor = SystemColors.Window

        loIndex = Val(Mid(loTxt.Name, 9))

        If Mid(loTxt.Name, 1, 8) = "txtField" Then
            Select Case loIndex
                Case 0
                    If loTxt.Text <> String.Empty Then poGiftCert.SearchCompany(pnActiveRow, loTxt.Text)
                Case 1
                    poGiftCert.Master(pnActiveRow, "sReferNox") = loTxt.Text
                Case 2
                    poGiftCert.Master(pnActiveRow, "dValidity") = loTxt.Text
                Case 3
                    poGiftCert.Master(pnActiveRow, "sRemarksx") = loTxt.Text
                Case 4
                    poGiftCert.Master(pnActiveRow, "nAmountxx") = loTxt.Text
            End Select
        End If

        poControl = Nothing
    End Sub

    Private Sub txtField_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs)
        If e.KeyCode = Keys.F3 Or e.KeyCode = Keys.Enter Then
            Dim loTxt As TextBox
            loTxt = CType(sender, System.Windows.Forms.TextBox)
            Dim loIndex As Integer
            loIndex = Val(Mid(loTxt.Name, 9))

            If Mid(loTxt.Name, 1, 8) = "txtField" Then
                'Select Case loIndex
                '    Case 0
                '        If e.KeyCode = Keys.F3 Then
                '            Call poGiftCert.SearchCompany(pnActiveRow, loTxt.Text)
                '        Else
                '            If loTxt.Text <> String.Empty Then poGiftCert.SearchCompany(pnActiveRow, loTxt.Text)
                '        End If
                '        loTxt.Tag = loTxt.Text
                'End Select
            End If
        End If
    End Sub

    Private Sub txtField04_KeyPress(sender As Object, e As System.Windows.Forms.KeyPressEventArgs) Handles txtField04.KeyPress
        If Not Char.IsNumber(e.KeyChar) AndAlso Not Char.IsControl(e.KeyChar) AndAlso Not e.KeyChar = "." Then
            e.Handled = True
        End If
    End Sub

    Private Sub poCreditCard_MasterRetrieved(Row As Integer, Index As Integer, Value As Object) Handles poGiftCert.MasterRetrieved
        Select Case Index
            Case 1
                txtField00.Text = Value
            Case 3
                txtField02.Text = Value
            Case 4
                txtField04.Text = FormatNumber(Value, 2)
        End Select
    End Sub

    Private Sub DataGridView1_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles DataGridView1.Click
        If DataGridView1.Rows.Count <= 0 Then Exit Sub
        With DataGridView1
            pnActiveRow = .CurrentCell.RowIndex

            setFieldValue(pnActiveRow)
        End With
    End Sub

    Private Sub showDetail(ByVal lbShow As Boolean)
        Dim lvDetailLoc As New Point(3, 391)
        Dim lvButtonLoc As New Point(3, 391)
        Dim lvMPnelOrgx As New Size(390, 541)
        Dim lvMPnelNewx As New Size(390, 541)
        Dim lvFormOrgxx As New Size(390, 541)
        Dim lvFormNewxx As New Size(390, 541)

        If lbShow Then
            Me.Size = lvFormOrgxx
            pnlDetail.Visible = True
            pnlMain.Size = lvMPnelOrgx
            pnlButtons.Location = lvButtonLoc
        Else
            Me.Size = lvFormNewxx
            pnlDetail.Visible = False
            pnlMain.Size = lvMPnelNewx
            pnlButtons.Location = lvDetailLoc
        End If

        InitGrid()
    End Sub

    Private Sub setVisible()
        Me.Visible = False
        Me.TransparencyKey = Nothing
        Me.Location = New Point(507, 90)

        txtField00.MaxLength = 64
        txtField00.MaxLength = 15
        txtField03.MaxLength = 64
        txtField04.MaxLength = 9

        txtField00.Focus()
        Me.Visible = True
    End Sub

    Private Sub computeChange()
        Dim lnBill As Decimal = CDec(lblBill.Text)

        If p_nGiftCert > 0 And p_nTendered + p_nCheck + p_nCreditCard = 0 Then 'GC payment only
            lblChange.Text = "0.00"
        ElseIf p_nGiftCert > 0 And p_nTendered + p_nCheck + p_nCreditCard > 0 Then 'GC + Others
            lblChange.Text = FormatNumber((p_nTendered + p_nCheck + p_nCreditCard) - (lnBill - p_nGiftCert), 2)
        ElseIf p_nTendered + p_nCheck + p_nCreditCard + p_nGiftCert <> 0 Then
            If p_nGiftCert > 0 Then
                If p_nGiftCert > lnBill Then
                    lblChange.Text = "0.00"
                Else
                    lblChange.Text = FormatNumber((p_nTendered + p_nCheck + p_nCreditCard + p_nGiftCert) - lnBill, 2)
                End If
            Else
                lblChange.Text = FormatNumber((p_nTendered + p_nCheck + p_nCreditCard + p_nGiftCert) - lnBill, 2)
            End If
        Else
            lblChange.Text = "0.00"
        End If
    End Sub

    Private Sub clearFields()
        Dim lnRow As Integer

        lblBill.Text = FormatNumber(p_nSalesAmt + p_nSchargex, 2)
        With poGiftCert
            lnRow = .ItemCount - 1
            txtField00.Text = .Master(lnRow, "sCompnyNm")
            txtField01.Text = .Master(lnRow, "sReferNox")
            txtField02.Text = CDate(.Master(lnRow, "dValidity")).ToShortDateString
            txtField03.Text = .Master(lnRow, "sRemarksx")
            txtField04.Text = FormatNumber(.Master(lnRow, "nAmountxx"), 2)
        End With
        Call computeChange()
    End Sub

    Private Function isEntryOk(ByVal DisplayMsg As Boolean) As Boolean
        Dim lbDeleted As Boolean

        For lnCtr As Integer = 0 To poGiftCert.ItemCount - 1
            If poGiftCert.Master(lnCtr, "sCompnyCd") = "" Then
                If poGiftCert.DeleteGC(lnCtr) Then
                    If Not lbDeleted Then lbDeleted = True
                End If
            End If
        Next lnCtr
        If lbDeleted Then loadOthers()

        If txtField00.Text = String.Empty Then
            If DisplayMsg Then
                MsgBox("Invalid Company detected..." & vbCrLf & _
                        "Please verify your entry then try again...", MsgBoxStyle.Critical, "WARNING")
            End If
            Return False
        End If

        If txtField01.Text = String.Empty Then
            If DisplayMsg Then
                MsgBox("Invalid Reference Number detected..." & vbCrLf & _
                        "Please verify your entry then try again...", MsgBoxStyle.Critical, "WARNING")
            End If
            Return False
        End If

        If CDec(txtField04.Text) = 0.0 Then
            If DisplayMsg Then
                MsgBox("Invalid Amount Paid..." & vbCrLf & _
                        "Please verify your entry then try again...", MsgBoxStyle.Critical, "WARNING")
            End If
            Return False
        End If

        p_nGiftCert = 0.0
        For lnCtr As Integer = 0 To poGiftCert.ItemCount - 1
            p_nGiftCert = p_nGiftCert + poGiftCert.Master(lnCtr, "nAmountxx")
        Next lnCtr

        If DisplayMsg Then
            If CDec(lblBill.Text) > p_nCash + p_nCheck + p_nGiftCert + p_nCreditCard Then
                MsgBox("Invalid Amount Paid..." & vbCrLf & _
                        "Please verify your entry then try again...", MsgBoxStyle.Critical, "WARNING")
                Return False
            End If
        End If

        Return True
    End Function

    Private Sub loadOthers()
        Dim lnCtr As Integer
        Dim lnRow As Integer
        Dim lnTotal As Decimal

        Call InitGrid()
        With DataGridView1
            If poGiftCert.ItemCount > 0 Then
                lnRow = poGiftCert.ItemCount
                .RowCount = lnRow
                For lnCtr = 0 To lnRow - 1
                    .Rows(lnCtr).Cells(0).Value = lnCtr + 1
                    .Rows(lnCtr).Cells(1).Value = poGiftCert.Master(lnCtr, "sCompnyNm")
                    .Rows(lnCtr).Cells(2).Value = poGiftCert.Master(lnCtr, "sReferNox")
                    .Rows(lnCtr).Cells(3).Value = FormatNumber(poGiftCert.Master(lnCtr, "nAmountxx"), 2)
                    lnTotal = lnTotal + poGiftCert.Master(lnCtr, "nAmountxx")
                Next
                p_nGiftCert = lnTotal

                computeChange()

                .ClearSelection()
                .CurrentCell = .Rows(lnRow - 1).Cells(0)
                .Rows(lnRow - 1).Selected = True

                setFieldValue(lnRow - 1)

                If .Rows.Count > 1 Then showDetail(True)

                txtField00.Focus()
            End If
        End With
    End Sub

    Private Sub setFieldValue(ByVal nRow As Integer)
        With DataGridView1
            pnActiveRow = nRow
            txtField00.Text = poGiftCert.Master(nRow, "sCompnyNm")
            txtField01.Text = poGiftCert.Master(nRow, "sReferNox")
            txtField02.Text = CDate(poGiftCert.Master(nRow, "dValidity")).ToShortDateString
            txtField03.Text = poGiftCert.Master(nRow, "sRemarksx")
            txtField04.Text = FormatNumber(poGiftCert.Master(nRow, "nAmountxx"), 2)
            txtField00.Focus()
        End With
    End Sub

    Private Sub InitGrid()
        InitializeDataGrid()
        With DataGridView1
            'Set No of Columns
            .ColumnCount = 4

            'Set Column Headers
            .Columns(0).HeaderText = ""
            .Columns(1).HeaderText = "Company"
            .Columns(2).HeaderText = "Reference No"
            .Columns(3).HeaderText = "Amount"

            'Set Column Sizes
            'Set Column Sizes
            .Columns(0).Width = 30
            .Columns(1).Width = 150
            .Columns(2).Width = 125
            .Columns(3).Width = 59

            .Columns(0).SortMode = DataGridViewColumnSortMode.NotSortable
            .Columns(1).SortMode = DataGridViewColumnSortMode.NotSortable
            .Columns(2).SortMode = DataGridViewColumnSortMode.NotSortable
            .Columns(3).SortMode = DataGridViewColumnSortMode.NotSortable

            .Columns(0).DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight
            .Columns(1).DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft
            .Columns(2).DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft
            .Columns(3).DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight

            .Columns(0).HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter
            .Columns(1).HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter
            .Columns(2).HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter
            .Columns(3).HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter
        End With
    End Sub

    Private Sub InitializeDataGrid()
        With DataGridView1
            ' Initialize basic DataGridView properties.
            .Dock = DockStyle.Fill
            .BackgroundColor = Color.LightGray
            .BorderStyle = BorderStyle.Fixed3D

            ' Set property values appropriate for read-only display and 
            ' limited interactivity. 
            .AllowUserToAddRows = False
            .AllowUserToDeleteRows = False
            .AllowUserToOrderColumns = False
            .ReadOnly = True
            .SelectionMode = DataGridViewSelectionMode.FullRowSelect
            .MultiSelect = False
            .AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None
            .AllowUserToResizeColumns = False
            .ColumnHeadersHeightSizeMode = _
                DataGridViewColumnHeadersHeightSizeMode.DisableResizing
            .AllowUserToResizeRows = False
            .RowHeadersWidthSizeMode = _
                DataGridViewRowHeadersWidthSizeMode.DisableResizing

            ' Set the selection background color for all the cells.
            .DefaultCellStyle.SelectionBackColor = Color.Empty
            .DefaultCellStyle.SelectionForeColor = Color.Black

            ' Set RowHeadersDefaultCellStyle.SelectionBackColor so that its default
            ' value won't override DataGridView.DefaultCellStyle.SelectionBackColor.
            .RowHeadersDefaultCellStyle.SelectionBackColor = Color.Empty 'Color.White

            ' Set the background color for all rows and for alternating rows. 
            ' The value for alternating rows overrides the value for all rows. 
            .RowsDefaultCellStyle.BackColor = Color.WhiteSmoke
            .AlternatingRowsDefaultCellStyle.BackColor = Color.Gainsboro

            ' Set the row and column header styles.
            .ColumnHeadersDefaultCellStyle.ForeColor = Color.White
            .ColumnHeadersDefaultCellStyle.BackColor = Color.Black
            .RowHeadersDefaultCellStyle.BackColor = Color.Black
        End With

        With DataGridView1.ColumnHeadersDefaultCellStyle
            .BackColor = Color.Navy
            .ForeColor = Color.White
            .Font = New Font(DataGridView1.Font, FontStyle.Bold)
        End With
    End Sub

    Protected Overloads Overrides ReadOnly Property CreateParams() As CreateParams
        Get
            Dim cp As CreateParams = MyBase.CreateParams
            cp.ExStyle = cp.ExStyle Or 33554432
            Return cp
        End Get
    End Property

    Private Sub PreventFlicker()
        With Me
            .SetStyle(ControlStyles.OptimizedDoubleBuffer, True)
            .SetStyle(ControlStyles.UserPaint, True)
            .SetStyle(ControlStyles.AllPaintingInWmPaint, True)
            .UpdateStyles()
        End With
    End Sub
End Class