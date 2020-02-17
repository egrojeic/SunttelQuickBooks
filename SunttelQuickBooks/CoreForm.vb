Imports System
Imports System.Net
Imports System.Drawing
Imports System.Collections
Imports System.ComponentModel
Imports System.Windows.Forms
Imports System.Data
Imports System.IO
Imports SunttelQuickBooksDLL
Imports SunttelDll2007


Public Class CoreForm

    Dim QBConection As QuickBooks

    Private Sub Form1_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        On Error Resume Next
        strConeccionDB = "Data Source = tradepoint.galleriafarms.com; Initial Catalog = SUNTTEL_TRADEPOINT; User Id = sa; Password = DELL2008}"
        dtpDesde.Value = "01/01/2000"

        QBConection = New QuickBooks(strConeccionDB, Me.txtAppID.Text, Me.txtAppName.Text, Me.nudMinVer.Value, Me.nudMaxVer.Value)

    End Sub

    Private Sub btnGetData_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnGetData.Click
        lblNumRegsAR.Text = 0
        QBConection.GetAccounts(Me.dtpDesde.Value, Me.dtpHasta.Value)
    End Sub

    Private Sub ButtonPayments_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonPayments.Click
        QBConection.GetPayments(Me.dtpDesde.Value, Me.dtpHasta.Value)
    End Sub

    Private Sub btnGetInvoices_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnGetInvoices.Click
        QBConection.GetInvoices(Me.dtpDesde.Value, Me.dtpHasta.Value)
    End Sub

End Class
