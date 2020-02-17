Imports QBFC13Lib

Public Class QuickBooks

    Dim Var_strConeccionDB As String
    Dim Var_MinVer As String
    Dim Var_MaxVer As String
    Dim Var_AppID As String
    Dim Var_AppName As String

    Dim Var_dsARSummary As dsARSummary
    Dim Var_dsAccountsReceivableQB As dsAccountsReceivableQB
    Dim Var_dsReceivedPayments As dsReceivedPayments
    Dim Var_dsQBInvoices As dsQBInvoices
    Dim Var_dsQBCustomers As dsQBCustomers

    Dim requestMsgSet As IMsgSetRequest

    Public Sub New(ByVal prmstrConeccionDB As String, ByVal prmAppID As String, ByVal prmAppName As String, Optional ByVal prmMinVer As Integer = 0, Optional ByVal prmMaxVer As Integer = 13)
        Var_strConeccionDB = prmstrConeccionDB
        Var_MinVer = prmMinVer
        Var_MaxVer = prmMaxVer
    End Sub

    Public Sub GetInvoices(ByVal prmFechaIni As Date, ByVal prmFechaFin As Date)
        DoInvoiceQuery(prmFechaIni, prmFechaFin)
    End Sub

    Public Sub GetAccounts(ByVal prmFechaIni As Date, ByVal prmFechaFin As Date)
        DoAccountQuery(prmFechaIni, prmFechaFin)
    End Sub

    Public Sub GetPayments(ByVal prmFechaIni As Date, ByVal prmFechaFin As Date)
        DoPaymentsQuery(prmFechaIni, prmFechaFin)
    End Sub


    Private Sub DoAccountQuery(ByVal prmFechaIni As Date, ByVal prmFechaFin As Date)
        Dim sessionBegun As Boolean
        sessionBegun = False
        Dim connectionOpen As Boolean
        connectionOpen = False
        Dim sessionManager As QBSessionManager
        sessionManager = Nothing

        Dim tmpXMLMajorVer As Short = 0
        Dim tmpXMLMinorVer As Short = 0

        tmpXMLMinorVer = Var_MinVer
        tmpXMLMajorVer = Var_MaxVer

        Try
            'Create the session Manager object
            sessionManager = New QBSessionManager

            My.Application.Log.WriteEntry("Iniciando", TraceEventType.Information)
            'Create the message set request object to hold our request

            requestMsgSet = sessionManager.CreateMsgSetRequest("US", tmpXMLMajorVer, tmpXMLMinorVer)
            requestMsgSet.Attributes.OnError = ENRqOnError.roeContinue

            My.Application.Log.WriteEntry("Creando Msg", TraceEventType.Information)

            BuildAccountQueryRq(prmFechaIni, prmFechaFin)

            My.Application.Log.WriteEntry("Abriendo conexion", TraceEventType.Information)
            'Connect to QuickBooks and begin a session
            sessionManager.OpenConnection(Var_AppID, Var_AppName)
            connectionOpen = True
            sessionManager.BeginSession("", ENOpenMode.omDontCare)
            sessionBegun = True

            My.Application.Log.WriteEntry("Conexion abierta", TraceEventType.Information)
            'Send the request and get the response from QuickBooks
            Dim responseMsgSet As IMsgSetResponse
            responseMsgSet = sessionManager.DoRequests(requestMsgSet)

            'End the session and close the connection to QuickBooks
            sessionManager.EndSession()
            sessionBegun = False
            sessionManager.CloseConnection()
            connectionOpen = False

            My.Application.Log.WriteEntry("Records: " & responseMsgSet.ResponseList.Count, TraceEventType.Resume)

            WalkAccountQueryRs(responseMsgSet)

            responseMsgSet.ToXMLString()


        Catch e As Exception
            My.Application.Log.WriteEntry(e.Message, TraceEventType.Error)
            If (sessionBegun) Then
                sessionManager.EndSession()
            End If
            If (connectionOpen) Then
                sessionManager.CloseConnection()
            End If
        End Try
    End Sub

    Private Sub BuildAccountQueryRq(ByVal prmFechaIni As Date, ByVal prmFechaFin As Date)
        On Error GoTo ControlaError
        Dim AccountQueryRq As IAgingReportQuery


        AccountQueryRq = requestMsgSet.AppendAgingReportQueryRq()
        Dim _toDate As Date
        _toDate = Now


        AccountQueryRq.AgingReportType.SetValue(ENAgingReportType.artARAgingDetail)
        AccountQueryRq.DisplayReport.SetValue(False)
        AccountQueryRq.ORReportPeriod.ReportPeriod.FromReportDate.SetValue(prmFechaIni)
        AccountQueryRq.ORReportPeriod.ReportPeriod.ToReportDate.SetValue(prmFechaFin)
        AccountQueryRq.ReportDetailLevelFilter.SetValue(ENReportDetailLevelFilter.rdlfAll)


        Exit Sub
ControlaError:
        My.Application.Log.WriteEntry(Err.Description & " BuildAccount", TraceEventType.Error)

    End Sub


    Private Sub WalkAccountQueryRs(ByRef responseMsgSet As IMsgSetResponse)
        If (responseMsgSet Is Nothing) Then
            Exit Sub
        End If

        Dim responseList As IResponseList
        responseList = responseMsgSet.ResponseList
        If (responseList Is Nothing) Then
            Exit Sub
        End If

        My.Application.Log.WriteEntry("Ingresar info en DataSet" & responseList.Count & " - WalkAccountQueryRs", TraceEventType.Information)

        'if we sent only one request, there is only one response, we'll walk the list for this sample
        For i = 0 To responseList.Count - 1
            Dim response As IResponse
            response = responseList.GetAt(i)
            'check the status code of the response, 0=ok, >0 is warning
            'MessageBox.Show("Ingresar info en DataSet StatusCode:" & response.StatusCode, "WalkAccountQueryRs")
            If (response.StatusCode >= 0) Then
                '//the request-specific response is in the details, make sure we have some
                If (Not IsNothing(response.Detail)) Then
                    '//make sure the response is the type we're expecting
                    'MessageBox.Show("Not IsNothing(response.Detail)", "WalkAccountQueryRs")
                    Dim responseType As ENResponseType
                    responseType = CType(response.Type.GetValue(), ENResponseType)
                    'MessageBox.Show("responseType: " & responseType, "WalkAccountQueryRs")
                    If (responseType = ENResponseType.rtAgingReportQueryRs) Then
                        '//upcast to more specific type here, this is safe because we checked with response.Type check above
                        Dim AccountRet As IReportRet
                        AccountRet = CType(response.Detail, IReportRet)

                        My.Application.Log.WriteEntry("Por ingresar detalle", TraceEventType.Information)
                        WalkReportRet(AccountRet)
                        'MessageBox.Show(AccountRet.l)
                    End If
                End If
            End If
        Next i
    End Sub


    Private Sub WalkReportRet(ByVal ReportRet As IReportRet)
        On Error Resume Next

        If (ReportRet Is Nothing) Then

            Exit Sub

        End If

        'Go through all the elements of IReportRet
        'Get value of ReportTitle
        Dim ReportTitle7 As String
        ReportTitle7 = ReportRet.ReportTitle.GetValue()
        'Get value of ReportSubtitle
        Dim ReportSubtitle8 As String
        ReportSubtitle8 = ReportRet.ReportSubtitle.GetValue()
        'Get value of ReportBasis
        If (Not ReportRet.ReportBasis Is Nothing) Then

            Dim ReportBasis9 As ENReportBasis
            ReportBasis9 = ReportRet.ReportBasis.GetValue()

        End If
        'Get value of NumRows
        Dim NumRows10 As Integer
        NumRows10 = ReportRet.NumRows.GetValue()
        'Get value of NumColumns
        Dim NumColumns11 As Integer
        NumColumns11 = ReportRet.NumColumns.GetValue()
        'Get value of NumColTitleRows
        Dim NumColTitleRows12 As Integer
        NumColTitleRows12 = ReportRet.NumColTitleRows.GetValue()
        If (Not IsNothing(ReportRet.ColDescList)) Then

            Dim i13 As Integer
            For i13 = 0 To ReportRet.ColDescList.Count - 1

                Dim ColDesc As IColDesc
                ColDesc = ReportRet.ColDescList.GetAt(i13)
                If (Not IsNothing(ColDesc)) Then

                    Dim i14 As Integer
                    For i14 = 0 To ColDesc.Count - 1

                        Dim ColTitle As IColTitle
                        ColTitle = ColDesc.GetAt(i14)

                    Next i14
                End If
                'Get value of ColType
                Dim ColType15 As ENColType
                ColType15 = ColDesc.ColType.GetValue()
            Next i13

        End If


        If (Not IsNothing(ReportRet.ReportData)) Then

            If (Not IsNothing(ReportRet.ReportData.ORReportDataList)) Then

                Dim i16 As Integer
                For i16 = 0 To ReportRet.ReportData.ORReportDataList.Count - 1

                    Dim ORReportData17 As IORReportData
                    ORReportData17 = ReportRet.ReportData.ORReportDataList.GetAt(i16)
                    If (Not IsNothing(ORReportData17.DataRow)) Then

                        Dim tmpNuevaFila As dsARSummary.QBARSummaryRow
                        tmpNuevaFila = Me.Var_dsARSummary.QBARSummary.NewQBARSummaryRow

                        Dim i18 As Integer
                        For i18 = 0 To ORReportData17.DataRow.ColDataList.Count - 1

                            Dim ColData As IColData
                            ColData = ORReportData17.DataRow.ColDataList.GetAt(i18)

                            Select Case ColData.colID.GetValue
                                Case 2
                                    tmpNuevaFila.TipoDocto = ColData.value.GetValue()
                                Case 3
                                    tmpNuevaFila.FechaDoc = ColData.value.GetValue
                                Case 6
                                    tmpNuevaFila.Cliente = ColData.value.GetValue
                                Case 8
                                    tmpNuevaFila.Fecha2 = ColData.value.GetValue
                                Case 9
                                    tmpNuevaFila.TipoDeuda = ColData.value.GetValue
                                Case 10
                                    tmpNuevaFila.Col10 = ColData.value.GetValue
                                Case 11
                                    tmpNuevaFila.Valor = ColData.value.GetValue

                            End Select


                        Next i18

                        If tmpNuevaFila.Cliente.Length > 0 And Not IsNothing(tmpNuevaFila.Cliente) Then
                            Me.Var_dsARSummary.QBARSummary.AddQBARSummaryRow(tmpNuevaFila)
                        End If

                    End If
                    If (Not IsNothing(ORReportData17.TextRow)) Then

                    End If
                    If (Not IsNothing(ORReportData17.SubtotalRow)) Then

                        If (Not IsNothing(ORReportData17.SubtotalRow)) Then

                            If (Not IsNothing(ORReportData17.SubtotalRow.RowData)) Then


                            End If
                            If (Not IsNothing(ORReportData17.SubtotalRow.ColDataList)) Then

                                Dim i19 As Integer
                                For i19 = 0 To ORReportData17.SubtotalRow.ColDataList.Count - 1

                                    Dim ColData As IColData
                                    ColData = ORReportData17.SubtotalRow.ColDataList.GetAt(i19)

                                Next i19
                            End If
                        End If
                    End If
                    If (Not IsNothing(ORReportData17.TotalRow)) Then

                        If (Not IsNothing(ORReportData17.TotalRow)) Then

                            If (Not IsNothing(ORReportData17.TotalRow.RowData)) Then


                            End If
                            If (Not IsNothing(ORReportData17.TotalRow.ColDataList)) Then

                                Dim i20 As Integer
                                For i20 = 0 To ORReportData17.TotalRow.ColDataList.Count - 1

                                    Dim ColData As IColData
                                    ColData = ORReportData17.TotalRow.ColDataList.GetAt(i20)

                                Next i20
                            End If
                        End If
                    End If

                Next i16

            End If
        End If

    End Sub

    Private Sub DoPaymentsQuery(ByVal prmFechaIni As Date, ByVal prmFechaFin As Date)
        Dim sessionBegun As Boolean
        sessionBegun = False
        Dim connectionOpen As Boolean
        connectionOpen = False
        Dim sessionManager As QBSessionManager
        sessionManager = Nothing

        Dim tmpXMLMajorVer As Short = 0
        Dim tmpXMLMinorVer As Short = 0

        tmpXMLMinorVer = Var_MinVer
        tmpXMLMajorVer = Var_MaxVer

        Try
            'Create the session Manager object
            sessionManager = New QBSessionManager
            'MessageBox.Show("Iniciando Payments")
            'Create the message set request object to hold our request

            requestMsgSet = sessionManager.CreateMsgSetRequest("US", tmpXMLMajorVer, tmpXMLMinorVer)
            requestMsgSet.Attributes.OnError = ENRqOnError.roeContinue

            'MessageBox.Show("Creando Msg")

            BuildPaymentsQueryRq(prmFechaIni, prmFechaFin)

            'MessageBox.Show("Abriendo conexion")
            'Connect to QuickBooks and begin a session
            sessionManager.OpenConnection(Var_AppID, Var_AppName)
            connectionOpen = True
            sessionManager.BeginSession("", ENOpenMode.omDontCare)
            sessionBegun = True

            'MessageBox.Show("Conexion abierta")
            'Send the request and get the response from QuickBooks
            Dim responseMsgSet As IMsgSetResponse
            responseMsgSet = sessionManager.DoRequests(requestMsgSet)

            'End the session and close the connection to QuickBooks
            sessionManager.EndSession()
            sessionBegun = False
            sessionManager.CloseConnection()
            connectionOpen = False

            My.Application.Log.WriteEntry("Recs: " & responseMsgSet.ResponseList.Count, TraceEventType.Resume)

            'MessageBox.Show("Preparando respuesta")
            WalkReceivePaymentQueryRs(responseMsgSet)

            responseMsgSet.ToXMLString()


            'Dim tmpStream As StringReader
            'tmpStream = New System.IO.StringReader(responseMsgSet.ToXMLString)


            ''Dim tmpXMLTextReader As New System.Xml.XmlTextReader(tmpStream)

            'Dim tmpXMLPath As String = ""
            'tmpXMLPath = System.IO.Path.GetDirectoryName(Application.ExecutablePath) & "\QueryXML.xml"

            'Me.dsQuery.ReadXml(tmpStream, XmlReadMode.IgnoreSchema)


            'Me.dsQuery.WriteXml(tmpXMLPath, XmlWriteMode.WriteSchema)



        Catch e As Exception
            My.Application.Log.WriteEntry(e.Message, TraceEventType.Error)
            If (sessionBegun) Then
                sessionManager.EndSession()
            End If
            If (connectionOpen) Then
                sessionManager.CloseConnection()
            End If
        End Try
    End Sub

    Private Sub BuildPaymentsQueryRq(ByVal prmFechaIni As Date, ByVal prmFechaFin As Date)
        On Error GoTo ControlaError

        Dim ReceivePaymentQueryRq As IReceivePaymentQuery
        ReceivePaymentQueryRq = requestMsgSet.AppendReceivePaymentQueryRq()


        'Dim _toDate As Date
        '_toDate = Now

        'ReceivePaymentQueryRq.IncludeLineItems.SetValue(True)
        ReceivePaymentQueryRq.ORTxnQuery.TxnFilter.ORDateRangeFilter.TxnDateRangeFilter.ORTxnDateRangeFilter.TxnDateFilter.FromTxnDate.SetValue(prmFechaIni)
        ReceivePaymentQueryRq.ORTxnQuery.TxnFilter.ORDateRangeFilter.TxnDateRangeFilter.ORTxnDateRangeFilter.TxnDateFilter.ToTxnDate.SetValue(prmFechaFin)

        'ReceivePaymentQueryRq.ORTxnQuery.TxnFilter.EntityFilter.OREntityFilter.FullNameWithChildren.SetValue(Me.txtCustomer.Text)   temporal entender que hace

        'ReceivePaymentQueryRq.IncludeLineItems.SetValue(True)


        Exit Sub
ControlaError:
        My.Application.Log.WriteEntry(Err.Description & " Build Payments Error", TraceEventType.Error)
    End Sub

    Private Sub WalkAccountRet(ByRef AccountRet As IReportRet)
        On Error GoTo ControlaError

        If (AccountRet Is Nothing) Then
            Exit Sub
        End If

        'Go through all the elements of IAccountRetList
        'Get value of ListID
        Dim ListID4 As String = ""
        'Get value of TimeCreated
        Dim TimeCreated5 As DateTime = "01/01/1900"
        'Get value of TimeModified
        Dim TimeModified6 As DateTime = "01/01/1900"
        'Get value of EditSequence
        Dim EditSequence7 As String = ""
        'Get value of Name
        Dim Name8 As String = ""
        'Get value of FullName
        Dim FullName9 As String = ""
        Dim IsActive10 As Boolean = True
        Dim ListID11 As String = ""
        Dim FullName12 As String = ""
        'Get value of Sublevel
        Dim Sublevel13 As Integer = 0
        'Get value of AccountType
        Dim AccountType14 As ENAccountType
        Dim SpecialAccountType15 As ENSpecialAccountType
        Dim AccountNumber16 As String = ""
        Dim BankNumber17 As String = ""
        Dim Desc18 As String = ""
        Dim Balance19 As Double = 0
        Dim TotalBalance20 As Double = 0
        Dim TaxLineID21 As Integer = 0
        Dim ListID24 As String = ""
        Dim FullName25 As String = ""

        ListID4 = AccountRet.ListID.GetValue()
        TimeCreated5 = AccountRet.TimeCreated.GetValue()
        TimeModified6 = AccountRet.TimeModified.GetValue()
        EditSequence7 = AccountRet.EditSequence.GetValue()
        Name8 = AccountRet.Name.GetValue()
        FullName9 = AccountRet.FullName.GetValue()


        'Get value of IsActive
        If (Not AccountRet.IsActive Is Nothing) Then

            IsActive10 = AccountRet.IsActive.GetValue()
        End If
        If (Not IsNothing(AccountRet.ParentRef)) Then
            'Get value of ListID
            If (Not AccountRet.ParentRef.ListID Is Nothing) Then

                ListID11 = AccountRet.ParentRef.ListID.GetValue()
            End If
            'Get value of FullName
            If (Not AccountRet.ParentRef.FullName Is Nothing) Then

                FullName12 = AccountRet.ParentRef.FullName.GetValue()
            End If
        End If

        Sublevel13 = AccountRet.Sublevel.GetValue()

        AccountType14 = AccountRet.AccountType.GetValue()
        'Get value of SpecialAccountType
        If (Not AccountRet.SpecialAccountType Is Nothing) Then

            SpecialAccountType15 = AccountRet.SpecialAccountType.GetValue()
        End If
        'Get value of AccountNumber
        If (Not AccountRet.AccountNumber Is Nothing) Then

            AccountNumber16 = AccountRet.AccountNumber.GetValue()
        End If
        'Get value of BankNumber
        If (Not AccountRet.BankNumber Is Nothing) Then

            BankNumber17 = AccountRet.BankNumber.GetValue()
        End If
        'Get value of Desc
        If (Not AccountRet.Desc Is Nothing) Then

            Desc18 = AccountRet.Desc.GetValue()
        End If
        'Get value of Balance
        If (Not AccountRet.Balance Is Nothing) Then

            Balance19 = AccountRet.Balance.GetValue()
        End If
        'Get value of TotalBalance
        If (Not AccountRet.TotalBalance Is Nothing) Then

            TotalBalance20 = AccountRet.TotalBalance.GetValue()
        End If
        If (Not IsNothing(AccountRet.TaxLineInfoRet)) Then
            'Get value of TaxLineID

            TaxLineID21 = AccountRet.TaxLineInfoRet.TaxLineID.GetValue()
            'Get value of TaxLineName
            If (Not AccountRet.TaxLineInfoRet.TaxLineName Is Nothing) Then
                Dim TaxLineName22 As String
                TaxLineName22 = AccountRet.TaxLineInfoRet.TaxLineName.GetValue()
            End If
        End If
        'Get value of CashFlowClassification
        If (Not AccountRet.CashFlowClassification Is Nothing) Then
            Dim CashFlowClassification23 As ENCashFlowClassification
            CashFlowClassification23 = AccountRet.CashFlowClassification.GetValue()
        End If
        If (Not IsNothing(AccountRet.CurrencyRef)) Then
            'Get value of ListID
            If (Not AccountRet.CurrencyRef.ListID Is Nothing) Then

                ListID24 = AccountRet.CurrencyRef.ListID.GetValue()
            End If
            'Get value of FullName
            If (Not AccountRet.CurrencyRef.FullName Is Nothing) Then

                FullName25 = AccountRet.CurrencyRef.FullName.GetValue()
            End If
        End If
        If (Not IsNothing(AccountRet.DataExtRetList)) Then
            Dim i26 As Integer
            For i26 = 0 To AccountRet.DataExtRetList.Count - 1
                Dim DataExtRet As IDataExtRet
                DataExtRet = AccountRet.DataExtRetList.GetAt(i26)
                'Get value of OwnerID
                If (Not IsNothing(DataExtRet.OwnerID)) Then
                    Dim OwnerID27 As String
                    OwnerID27 = DataExtRet.OwnerID.GetValue()
                End If
                'Get value of DataExtName
                Dim DataExtName28 As String
                DataExtName28 = DataExtRet.DataExtName.GetValue()
                'Get value of DataExtType
                Dim DataExtType29 As ENDataExtType
                DataExtType29 = DataExtRet.DataExtType.GetValue()
                'Get value of DataExtValue
                Dim DataExtValue30 As String
                DataExtValue30 = DataExtRet.DataExtValue.GetValue()
            Next i26
        End If

        My.Application.Log.WriteEntry("Por ingresar detalle en Dataset" & " WalkAccountRet", TraceEventType.Information)

        Dim tmpNuevaFila As dsAccountsReceivableQB.AccountsReceivableQBRow
        tmpNuevaFila = Me.Var_dsAccountsReceivableQB.AccountsReceivableQB.NewAccountsReceivableQBRow
        With tmpNuevaFila
            .ListID24 = ListID4
            .TimeCreated5 = TimeCreated5
            .TimeModified6 = TimeModified6
            .EditSequence7 = EditSequence7
            .Name8 = Name8
            .FullName9 = FullName9
            .IsActive10 = IIf(IsActive10, 1, 0)
            .ListID11 = ListID11
            .FullName12 = FullName12
            .Sublevel13 = Sublevel13
            .AccountType14 = AccountType14

            .AccountNumber16 = AccountNumber16
            .BankNumber17 = BankNumber17
            .Desc18 = Desc18
            .Balance19 = Balance19
            .TotalBalance20 = TotalBalance20
            .TaxLineID21 = TaxLineID21
            .ListID24 = ListID24
            .FullName25 = FullName25
        End With
        Me.Var_dsAccountsReceivableQB.AccountsReceivableQB.AddAccountsReceivableQBRow(tmpNuevaFila)

        Exit Sub
ControlaError:
        My.Application.Log.WriteEntry(Err.Description & " WalkAccountRet", TraceEventType.Error)
    End Sub

    Private Sub WalkReceivePaymentQueryRs(ByVal responseMsgSet As IMsgSetResponse)

        On Error GoTo ControlaError
        If (responseMsgSet Is Nothing) Then

            Exit Sub

        End If

        Dim responseList As IResponseList
        responseList = responseMsgSet.ResponseList
        If (responseList Is Nothing) Then

            Exit Sub

        End If

        'if we sent only one request, there is only one response, we'll walk the list for this sample
        For j = 0 To responseList.Count - 1

            Dim response As IResponse
            response = responseList.GetAt(j)
            'check the status code of the response, 0=ok, >0 is warning
            If (response.StatusCode >= 0) Then

                ''the request-specific response is in the details, make sure we have some
                If (Not IsNothing(response.Detail)) Then

                    ''make sure the response is the type we're expecting
                    Dim responseType As ENResponseType
                    responseType = CType(response.Type.GetValue(), ENResponseType)

                    'MessageBox.Show("Tipo Rsta:" & responseType.ToString, "WalkReceivePaymentQueryRs")

                    If (responseType = ENResponseType.rtReceivePaymentQueryRs) Then

                        '//upcast to more specific type here, this is safe because we checked with response.Type check above
                        Dim ReceivePaymentRet As IReceivePaymentRetList
                        ReceivePaymentRet = CType(response.Detail, IReceivePaymentRetList)
                        Dim i As Integer = 0
                        For i = 0 To ReceivePaymentRet.Count - 1
                            WalkReceivePaymentRet(ReceivePaymentRet.GetAt(i))
                        Next


                    End If
                End If
            End If
        Next j

        Exit Sub
ControlaError:
        My.Application.Log.WriteEntry(Err.Description & " WalkReceivePaymentQueryRs", TraceEventType.Information)
    End Sub

    Private Sub WalkReceivePaymentRet(ByVal ReceivePaymentRet As IReceivePaymentRet)
        On Error GoTo ControlaError

        If (ReceivePaymentRet Is Nothing) Then

            Exit Sub

        End If

        'MessageBox.Show("Entra", "WalkReceivePaymentRet")

        Dim TxnID8 As String = ""
        Dim TimeCreated9 As DateTime = "01/01/1900"
        Dim TimeModified10 As DateTime = "01/01/1900"
        Dim EditSequence11 As String = ""
        Dim TxnNumber12 As Integer = 0
        Dim ListID13 As String = ""
        Dim FullName14 As String = "" ' Nombre del Cliente
        Dim ListID15 As String = ""
        Dim FullName16 As String = ""
        Dim TxnDate17 As DateTime = "01/01/1900"
        Dim RefNumber18 As String = ""
        Dim TotalAmount19 As Double = 0

        'Go through all the elements of IReceivePaymentRetList
        'Get value of TxnID
        If (Not IsNothing(ReceivePaymentRet.TxnID)) Then
            TxnID8 = ReceivePaymentRet.TxnID.GetValue()
        End If
        'Get value of TimeCreated
        If (Not ReceivePaymentRet.TimeCreated Is Nothing) Then
            TimeCreated9 = ReceivePaymentRet.TimeCreated.GetValue()
        End If
        'Get value of TimeModified
        If (Not ReceivePaymentRet.TimeModified Is Nothing) Then
            TimeModified10 = ReceivePaymentRet.TimeModified.GetValue()
        End If
        'Get value of EditSequence
        If (Not ReceivePaymentRet.EditSequence Is Nothing) Then
            EditSequence11 = ReceivePaymentRet.EditSequence.GetValue()
        End If
        'Get value of TxnNumber
        If (Not ReceivePaymentRet.TxnNumber Is Nothing) Then
            TxnNumber12 = ReceivePaymentRet.TxnNumber.GetValue()
        End If
        If (Not IsNothing(ReceivePaymentRet.CustomerRef)) Then

            'Get value of ListID
            If (Not ReceivePaymentRet.CustomerRef.ListID Is Nothing) Then
                ListID13 = ReceivePaymentRet.CustomerRef.ListID.GetValue()
            End If
            'Get value of FullName
            If (Not ReceivePaymentRet.CustomerRef.FullName Is Nothing) Then
                FullName14 = ReceivePaymentRet.CustomerRef.FullName.GetValue()

            End If
        End If
        If (Not IsNothing(ReceivePaymentRet.ARAccountRef)) Then

            'Get value of ListID
            If (Not ReceivePaymentRet.ARAccountRef.ListID Is Nothing) Then
                ListID15 = ReceivePaymentRet.ARAccountRef.ListID.GetValue()
            End If
            'Get value of FullName
            If (Not ReceivePaymentRet.ARAccountRef.FullName Is Nothing) Then
                FullName16 = ReceivePaymentRet.ARAccountRef.FullName.GetValue()

            End If
        End If
        'Get value of TxnDate
        If (Not ReceivePaymentRet.TxnDate Is Nothing) Then
            TxnDate17 = ReceivePaymentRet.TxnDate.GetValue()
        End If
        'Get value of RefNumber
        If (Not ReceivePaymentRet.RefNumber Is Nothing) Then
            RefNumber18 = ReceivePaymentRet.RefNumber.GetValue()

        End If
        'Get value of TotalAmount
        If (Not ReceivePaymentRet.TotalAmount Is Nothing) Then
            TotalAmount19 = ReceivePaymentRet.TotalAmount.GetValue()

        End If
        If (Not IsNothing(ReceivePaymentRet.CurrencyRef)) Then

            'Get value of ListID
            If (Not ReceivePaymentRet.CurrencyRef.ListID Is Nothing) Then

                Dim ListID20 As String
                ListID20 = ReceivePaymentRet.CurrencyRef.ListID.GetValue()

            End If
            'Get value of FullName
            If (Not ReceivePaymentRet.CurrencyRef.FullName Is Nothing) Then

                Dim FullName21 As String
                FullName21 = ReceivePaymentRet.CurrencyRef.FullName.GetValue()

            End If
        End If
        'Get value of ExchangeRate
        If (Not ReceivePaymentRet.ExchangeRate Is Nothing) Then

            Dim ExchangeRate22 As IQBFloatType
            'ExchangeRate22 = ReceivePaymentRet.ExchangeRate.GetValue()

        End If
        'Get value of TotalAmountInHomeCurrency
        If (Not ReceivePaymentRet.TotalAmountInHomeCurrency Is Nothing) Then

            Dim TotalAmountInHomeCurrency23 As Double
            TotalAmountInHomeCurrency23 = ReceivePaymentRet.TotalAmountInHomeCurrency.GetValue()

        End If
        If (Not IsNothing(ReceivePaymentRet.PaymentMethodRef)) Then

            'Get value of ListID
            If (Not ReceivePaymentRet.PaymentMethodRef.ListID Is Nothing) Then

                Dim ListID24 As String
                ListID24 = ReceivePaymentRet.PaymentMethodRef.ListID.GetValue()

            End If
            'Get value of FullName
            If (Not ReceivePaymentRet.PaymentMethodRef.FullName Is Nothing) Then

                Dim FullName25 As String
                FullName25 = ReceivePaymentRet.PaymentMethodRef.FullName.GetValue()

            End If
        End If
        'Get value of Memo
        If (Not ReceivePaymentRet.Memo Is Nothing) Then

            Dim Memo26 As String
            Memo26 = ReceivePaymentRet.Memo.GetValue()

        End If
        If (Not ReceivePaymentRet.DepositToAccountRef Is Nothing) Then

            'Get value of ListID
            If (Not ReceivePaymentRet.DepositToAccountRef.ListID Is Nothing) Then

                Dim ListID27 As String
                ListID27 = ReceivePaymentRet.DepositToAccountRef.ListID.GetValue()

            End If
            'Get value of FullName
            If (Not ReceivePaymentRet.DepositToAccountRef.FullName Is Nothing) Then

                Dim FullName28 As String
                FullName28 = ReceivePaymentRet.DepositToAccountRef.FullName.GetValue()

            End If
        End If
        If (Not ReceivePaymentRet.CreditCardTxnInfo Is Nothing) Then

            'Get value of CreditCardNumber
            Dim CreditCardNumber29 As String
            CreditCardNumber29 = ReceivePaymentRet.CreditCardTxnInfo.CreditCardTxnInputInfo.CreditCardNumber.GetValue()
            'Get value of ExpirationMonth
            Dim ExpirationMonth30 As Integer
            ExpirationMonth30 = ReceivePaymentRet.CreditCardTxnInfo.CreditCardTxnInputInfo.ExpirationMonth.GetValue()
            'Get value of ExpirationYear
            Dim ExpirationYear31 As Integer
            ExpirationYear31 = ReceivePaymentRet.CreditCardTxnInfo.CreditCardTxnInputInfo.ExpirationYear.GetValue()
            'Get value of NameOnCard
            Dim NameOnCard32 As String
            NameOnCard32 = ReceivePaymentRet.CreditCardTxnInfo.CreditCardTxnInputInfo.NameOnCard.GetValue()
            'Get value of CreditCardAddress
            If (Not ReceivePaymentRet.CreditCardTxnInfo.CreditCardTxnInputInfo.CreditCardAddress Is Nothing) Then

                Dim CreditCardAddress33 As String
                CreditCardAddress33 = ReceivePaymentRet.CreditCardTxnInfo.CreditCardTxnInputInfo.CreditCardAddress.GetValue()

            End If
            'Get value of CreditCardPostalCode
            If (Not ReceivePaymentRet.CreditCardTxnInfo.CreditCardTxnInputInfo.CreditCardPostalCode Is Nothing) Then

                Dim CreditCardPostalCode34 As String
                CreditCardPostalCode34 = ReceivePaymentRet.CreditCardTxnInfo.CreditCardTxnInputInfo.CreditCardPostalCode.GetValue()

            End If
            'Get value of CommercialCardCode
            If (Not ReceivePaymentRet.CreditCardTxnInfo.CreditCardTxnInputInfo.CommercialCardCode Is Nothing) Then

                Dim CommercialCardCode35 As String
                CommercialCardCode35 = ReceivePaymentRet.CreditCardTxnInfo.CreditCardTxnInputInfo.CommercialCardCode.GetValue()

            End If
            'Get value of TransactionMode
            If (Not ReceivePaymentRet.CreditCardTxnInfo.CreditCardTxnInputInfo.TransactionMode Is Nothing) Then

                Dim TransactionMode36 As ENTransactionMode
                TransactionMode36 = ReceivePaymentRet.CreditCardTxnInfo.CreditCardTxnInputInfo.TransactionMode.GetValue()

            End If
            'Get value of CreditCardTxnType
            If (Not ReceivePaymentRet.CreditCardTxnInfo.CreditCardTxnInputInfo.CreditCardTxnType Is Nothing) Then

                Dim CreditCardTxnType37 As ENCreditCardTxnType
                CreditCardTxnType37 = ReceivePaymentRet.CreditCardTxnInfo.CreditCardTxnInputInfo.CreditCardTxnType.GetValue()

            End If
            'Get value of ResultCode
            Dim ResultCode38 As Integer
            ResultCode38 = ReceivePaymentRet.CreditCardTxnInfo.CreditCardTxnResultInfo.ResultCode.GetValue()
            'Get value of ResultMessage
            Dim ResultMessage39 As String
            ResultMessage39 = ReceivePaymentRet.CreditCardTxnInfo.CreditCardTxnResultInfo.ResultMessage.GetValue()
            'Get value of CreditCardTransID
            Dim CreditCardTransID40 As String
            CreditCardTransID40 = ReceivePaymentRet.CreditCardTxnInfo.CreditCardTxnResultInfo.CreditCardTransID.GetValue()
            'Get value of MerchantAccountNumber
            Dim MerchantAccountNumber41 As String
            MerchantAccountNumber41 = ReceivePaymentRet.CreditCardTxnInfo.CreditCardTxnResultInfo.MerchantAccountNumber.GetValue()
            'Get value of AuthorizationCode
            If (Not ReceivePaymentRet.CreditCardTxnInfo.CreditCardTxnResultInfo.AuthorizationCode Is Nothing) Then

                Dim AuthorizationCode42 As String
                AuthorizationCode42 = ReceivePaymentRet.CreditCardTxnInfo.CreditCardTxnResultInfo.AuthorizationCode.GetValue()

            End If
            'Get value of AVSStreet
            If (Not ReceivePaymentRet.CreditCardTxnInfo.CreditCardTxnResultInfo.AVSStreet Is Nothing) Then

                Dim AVSStreet43 As ENAVSStreet
                AVSStreet43 = ReceivePaymentRet.CreditCardTxnInfo.CreditCardTxnResultInfo.AVSStreet.GetValue()

            End If
            'Get value of AVSZip
            If (Not ReceivePaymentRet.CreditCardTxnInfo.CreditCardTxnResultInfo.AVSZip Is Nothing) Then

                Dim AVSZip44 As ENAVSZip
                AVSZip44 = ReceivePaymentRet.CreditCardTxnInfo.CreditCardTxnResultInfo.AVSZip.GetValue()

            End If
            'Get value of CardSecurityCodeMatch
            If (Not ReceivePaymentRet.CreditCardTxnInfo.CreditCardTxnResultInfo.CardSecurityCodeMatch Is Nothing) Then

                Dim CardSecurityCodeMatch45 As ENCardSecurityCodeMatch
                CardSecurityCodeMatch45 = ReceivePaymentRet.CreditCardTxnInfo.CreditCardTxnResultInfo.CardSecurityCodeMatch.GetValue()

            End If
            'Get value of ReconBatchID
            If (Not ReceivePaymentRet.CreditCardTxnInfo.CreditCardTxnResultInfo.ReconBatchID Is Nothing) Then

                Dim ReconBatchID46 As String
                ReconBatchID46 = ReceivePaymentRet.CreditCardTxnInfo.CreditCardTxnResultInfo.ReconBatchID.GetValue()

            End If
            'Get value of PaymentGroupingCode
            If (Not ReceivePaymentRet.CreditCardTxnInfo.CreditCardTxnResultInfo.PaymentGroupingCode Is Nothing) Then

                Dim PaymentGroupingCode47 As Integer
                PaymentGroupingCode47 = ReceivePaymentRet.CreditCardTxnInfo.CreditCardTxnResultInfo.PaymentGroupingCode.GetValue()

            End If
            'Get value of PaymentStatus
            Dim PaymentStatus48 As ENPaymentStatus
            PaymentStatus48 = ReceivePaymentRet.CreditCardTxnInfo.CreditCardTxnResultInfo.PaymentStatus.GetValue()
            'Get value of TxnAuthorizationTime
            Dim TxnAuthorizationTime49 As DateTime
            TxnAuthorizationTime49 = ReceivePaymentRet.CreditCardTxnInfo.CreditCardTxnResultInfo.TxnAuthorizationTime.GetValue()
            'Get value of TxnAuthorizationStamp
            If (Not ReceivePaymentRet.CreditCardTxnInfo.CreditCardTxnResultInfo.TxnAuthorizationStamp Is Nothing) Then

                Dim TxnAuthorizationStamp50 As Integer
                TxnAuthorizationStamp50 = ReceivePaymentRet.CreditCardTxnInfo.CreditCardTxnResultInfo.TxnAuthorizationStamp.GetValue()

            End If
            'Get value of ClientTransID
            If (Not ReceivePaymentRet.CreditCardTxnInfo.CreditCardTxnResultInfo.ClientTransID Is Nothing) Then

                Dim ClientTransID51 As String
                ClientTransID51 = ReceivePaymentRet.CreditCardTxnInfo.CreditCardTxnResultInfo.ClientTransID.GetValue()

            End If
        End If
        'Get value of UnusedPayment
        If (Not ReceivePaymentRet.UnusedPayment Is Nothing) Then

            Dim UnusedPayment52 As Double
            UnusedPayment52 = ReceivePaymentRet.UnusedPayment.GetValue()

        End If
        'Get value of UnusedCredits
        If (Not ReceivePaymentRet.UnusedCredits Is Nothing) Then

            Dim UnusedCredits53 As Double
            UnusedCredits53 = ReceivePaymentRet.UnusedCredits.GetValue()

        End If
        'Get value of ExternalGUID
        If (Not ReceivePaymentRet.ExternalGUID Is Nothing) Then

            Dim ExternalGUID54 As String
            ExternalGUID54 = ReceivePaymentRet.ExternalGUID.GetValue()

        End If
        If (Not ReceivePaymentRet.AppliedToTxnRetList Is Nothing) Then

            Dim i55 As Integer
            For i55 = 0 To ReceivePaymentRet.AppliedToTxnRetList.Count - 1

                Dim AppliedToTxnRet As IAppliedToTxnRet
                AppliedToTxnRet = ReceivePaymentRet.AppliedToTxnRetList.GetAt(i55)
                'Get value of TxnID
                Dim TxnID56 As String
                TxnID56 = AppliedToTxnRet.TxnID.GetValue()
                'Get value of TxnType
                Dim TxnType57 As ENTxnType
                TxnType57 = AppliedToTxnRet.TxnType.GetValue()
                'Get value of TxnDate
                If (Not AppliedToTxnRet.TxnDate Is Nothing) Then

                    Dim TxnDate58 As DateTime
                    TxnDate58 = AppliedToTxnRet.TxnDate.GetValue()

                End If
                'Get value of RefNumber
                If (Not AppliedToTxnRet.RefNumber Is Nothing) Then

                    Dim RefNumber59 As String
                    RefNumber59 = AppliedToTxnRet.RefNumber.GetValue()

                End If
                'Get value of BalanceRemaining
                If (Not AppliedToTxnRet.BalanceRemaining Is Nothing) Then

                    Dim BalanceRemaining60 As Double
                    BalanceRemaining60 = AppliedToTxnRet.BalanceRemaining.GetValue()

                End If
                'Get value of Amount
                If (Not AppliedToTxnRet.Amount Is Nothing) Then

                    Dim Amount61 As Double
                    Amount61 = AppliedToTxnRet.Amount.GetValue()

                End If
                'Get value of DiscountAmount
                If (Not AppliedToTxnRet.DiscountAmount Is Nothing) Then

                    Dim DiscountAmount62 As Double
                    DiscountAmount62 = AppliedToTxnRet.DiscountAmount.GetValue()

                End If
                If (Not AppliedToTxnRet.DiscountAccountRef Is Nothing) Then

                    'Get value of ListID
                    If (Not AppliedToTxnRet.DiscountAccountRef.ListID Is Nothing) Then

                        Dim ListID63 As String
                        ListID63 = AppliedToTxnRet.DiscountAccountRef.ListID.GetValue()

                    End If
                    'Get value of FullName
                    If (Not AppliedToTxnRet.DiscountAccountRef.FullName Is Nothing) Then

                        Dim FullName64 As String
                        FullName64 = AppliedToTxnRet.DiscountAccountRef.FullName.GetValue()

                    End If
                End If
                If (Not AppliedToTxnRet.DiscountClassRef Is Nothing) Then

                    'Get value of ListID
                    If (Not AppliedToTxnRet.DiscountClassRef.ListID Is Nothing) Then

                        Dim ListID65 As String
                        ListID65 = AppliedToTxnRet.DiscountClassRef.ListID.GetValue()

                    End If
                    'Get value of FullName
                    If (Not AppliedToTxnRet.DiscountClassRef.FullName Is Nothing) Then

                        Dim FullName66 As String
                        FullName66 = AppliedToTxnRet.DiscountClassRef.FullName.GetValue()

                    End If
                End If
                If (Not AppliedToTxnRet.LinkedTxnList Is Nothing) Then

                    Dim i67 As Integer
                    For i67 = 0 To AppliedToTxnRet.LinkedTxnList.Count - 1

                        Dim LinkedTxn As ILinkedTxn
                        LinkedTxn = AppliedToTxnRet.LinkedTxnList.GetAt(i67)
                        'Get value of TxnID
                        Dim TxnID68 As String
                        TxnID68 = LinkedTxn.TxnID.GetValue()
                        'Get value of TxnType
                        Dim TxnType69 As ENTxnType
                        TxnType69 = LinkedTxn.TxnType.GetValue()
                        'Get value of TxnDate
                        Dim TxnDate70 As DateTime
                        TxnDate70 = LinkedTxn.TxnDate.GetValue()
                        'Get value of RefNumber
                        If (Not LinkedTxn.RefNumber Is Nothing) Then

                            Dim RefNumber71 As String
                            RefNumber71 = LinkedTxn.RefNumber.GetValue()

                        End If
                        'Get value of LinkType
                        If (Not LinkedTxn.LinkType Is Nothing) Then

                            Dim LinkType72 As ENLinkType
                            LinkType72 = LinkedTxn.LinkType.GetValue()

                        End If
                        'Get value of Amount
                        Dim Amount73 As Double
                        Amount73 = LinkedTxn.Amount.GetValue()
                    Next i67
                End If
            Next i55

        End If
        If (Not ReceivePaymentRet.DataExtRetList Is Nothing) Then

            Dim i74 As Integer
            For i74 = 0 To ReceivePaymentRet.DataExtRetList.Count - 1

                Dim DataExtRet As IDataExtRet
                DataExtRet = ReceivePaymentRet.DataExtRetList.GetAt(i74)
                'Get value of OwnerID
                If (Not DataExtRet.OwnerID Is Nothing) Then

                    Dim OwnerID75 As String
                    OwnerID75 = DataExtRet.OwnerID.GetValue()

                End If
                'Get value of DataExtName
                Dim DataExtName76 As String
                DataExtName76 = DataExtRet.DataExtName.GetValue()
                'Get value of DataExtType
                Dim DataExtType77 As ENDataExtType
                DataExtType77 = DataExtRet.DataExtType.GetValue()
                'Get value of DataExtValue
                Dim DataExtValue78 As String
                DataExtValue78 = DataExtRet.DataExtValue.GetValue()
            Next i74
        End If

        Dim tmpNuevaFila As dsReceivedPayments.QBReceivedPaymentsRow
        tmpNuevaFila = Me.Var_dsReceivedPayments.QBReceivedPayments.NewQBReceivedPaymentsRow

        With tmpNuevaFila
            .Cliente = FullName14
            .Fecha = TxnDate17
            .Valor = TotalAmount19
            .Referencia = RefNumber18

        End With

        Me.Var_dsReceivedPayments.QBReceivedPayments.AddQBReceivedPaymentsRow(tmpNuevaFila)

        Exit Sub

ControlaError:
        'MessageBox.Show(Err.Description, "Error al conformar respuesta")
        Resume Next
    End Sub

    Private Sub DoInvoiceQuery(ByVal prmFechaIni As Date, ByVal prmFechaFin As Date)

        Dim sessionBegun As Boolean
        sessionBegun = False
        Dim connectionOpen As Boolean
        connectionOpen = False
        Dim sessionManager As QBSessionManager
        sessionManager = Nothing

        Try

            'Create the session Manager object
            sessionManager = New QBSessionManager

            'Create the message set request object to hold our request
            Dim requestMsgSet As IMsgSetRequest
            requestMsgSet = sessionManager.CreateMsgSetRequest("US", 13, 0)
            requestMsgSet.Attributes.OnError = ENRqOnError.roeContinue

            BuildInvoiceQueryRq(requestMsgSet, prmFechaIni, prmFechaFin)

            'Connect to QuickBooks and begin a session
            'MessageBox.Show("Abriendo conexion")
            'Connect to QuickBooks and begin a session
            sessionManager.OpenConnection(Var_AppID, Var_AppName)
            connectionOpen = True
            sessionManager.BeginSession("", ENOpenMode.omDontCare)
            sessionBegun = True



            'Send the request and get the response from QuickBooks
            Dim responseMsgSet As IMsgSetResponse
            responseMsgSet = sessionManager.DoRequests(requestMsgSet)

            'End the session and close the connection to QuickBooks
            sessionManager.EndSession()
            sessionBegun = False
            sessionManager.CloseConnection()
            connectionOpen = False

            responseMsgSet.ToXMLString()

            WalkInvoiceQueryRs(responseMsgSet)

        Catch e As Exception

            My.Application.Log.WriteEntry(e.Message, TraceEventType.Error)

            If (sessionBegun) Then

                sessionManager.EndSession()


            End If
            If (connectionOpen) Then

                sessionManager.CloseConnection()


            End If
        End Try
    End Sub

    Private Sub WalkInvoiceQueryRs(ByVal responseMsgSet As IMsgSetResponse)

        If (responseMsgSet Is Nothing) Then

            Exit Sub

        End If

        Dim responseList As IResponseList
        responseList = responseMsgSet.ResponseList
        If (responseList Is Nothing) Then

            Exit Sub

        End If

        'if we sent only one request, there is only one response, we'll walk the list for this sample
        For j = 0 To responseList.Count - 1

            Dim response As IResponse
            response = responseList.GetAt(j)
            'check the status code of the response, 0=ok, >0 is warning
            If (response.StatusCode >= 0) Then

                'the request-specific response is in the details, make sure we have some
                If (Not response.Detail Is Nothing) Then

                    'make sure the response is the type we're expecting
                    Dim responseType As ENResponseType
                    responseType = CType(response.Type.GetValue(), ENResponseType)
                    If (responseType = ENResponseType.rtInvoiceQueryRs) Then

                        '//upcast to more specific type here, this is safe because we checked with response.Type check above
                        Dim InvoiceRet As IInvoiceRetList
                        InvoiceRet = CType(response.Detail, IInvoiceRetList)

                        Dim i As Integer = 0
                        For i = 0 To InvoiceRet.Count - 1
                            WalkInvoiceRet(InvoiceRet.GetAt(i))
                        Next


                    End If
                End If
            End If
        Next j
    End Sub


    Private Sub WalkInvoiceRet(ByVal InvoiceRet As IInvoiceRet)

        If (InvoiceRet Is Nothing) Then

            Exit Sub

        End If

        'Go through all the elements of IInvoiceRetList
        'Get value of TxnID
        Dim TxnID12989 As String
        Dim TimeCreated12990 As DateTime
        Dim TimeModified12991 As DateTime
        Dim EditSequence12992 As String = ""
        Dim TxnNumber12993 As Integer = 0
        Dim ListID12994 As String = ""
        Dim FullName12995 As String = ""
        Dim ListID12996 As String = ""
        Dim FullName12997 As String = ""
        Dim ListID12998 As String = ""
        Dim FullName12999 As String = ""
        Dim ListID13000 As String = ""
        Dim FullName13001 As String = ""

        Dim TxnDate13002 As DateTime 'F Factura
        Dim RefNumber13003 As String = ""
        Dim Subtotal13046 As Double = 0
        Dim AppliedAmount13051 As Double = 0
        Dim TxnType13071 As ENTxnType

        Dim Desc13081 As String = ""

        TxnID12989 = InvoiceRet.TxnID.GetValue()
        'Get value of TimeCreated
        TimeCreated12990 = InvoiceRet.TimeCreated.GetValue()
        'Get value of TimeModified
        TimeModified12991 = InvoiceRet.TimeModified.GetValue()
        'Get value of EditSequence
        EditSequence12992 = InvoiceRet.EditSequence.GetValue()
        'Get value of TxnNumber
        If (Not InvoiceRet.TxnNumber Is Nothing) Then
            TxnNumber12993 = InvoiceRet.TxnNumber.GetValue()
        End If
        'Get value of ListID
        If (Not InvoiceRet.CustomerRef.ListID Is Nothing) Then
            ListID12994 = InvoiceRet.CustomerRef.ListID.GetValue()
        End If
        'Get value of FullName
        If (Not InvoiceRet.CustomerRef.FullName Is Nothing) Then
            FullName12995 = InvoiceRet.CustomerRef.FullName.GetValue()

        End If
        If (Not IsNothing(InvoiceRet.ClassRef)) Then

            'Get value of ListID
            If (Not InvoiceRet.ClassRef.ListID Is Nothing) Then
                ListID12996 = InvoiceRet.ClassRef.ListID.GetValue()

            End If
            'Get value of FullName
            If (Not InvoiceRet.ClassRef.FullName Is Nothing) Then
                FullName12997 = InvoiceRet.ClassRef.FullName.GetValue()

            End If
        End If
        If (Not InvoiceRet.ARAccountRef Is Nothing) Then

            'Get value of ListID
            If (Not InvoiceRet.ARAccountRef.ListID Is Nothing) Then
                ListID12998 = InvoiceRet.ARAccountRef.ListID.GetValue()

            End If
            'Get value of FullName
            If (Not InvoiceRet.ARAccountRef.FullName Is Nothing) Then
                FullName12999 = InvoiceRet.ARAccountRef.FullName.GetValue()

            End If
        End If
        If (Not InvoiceRet.TemplateRef Is Nothing) Then

            'Get value of ListID
            If (Not InvoiceRet.TemplateRef.ListID Is Nothing) Then
                ListID13000 = InvoiceRet.TemplateRef.ListID.GetValue()

            End If
            'Get value of FullName
            If (Not InvoiceRet.TemplateRef.FullName Is Nothing) Then
                FullName13001 = InvoiceRet.TemplateRef.FullName.GetValue()

            End If
        End If
        'Get value of TxnDate

        TxnDate13002 = InvoiceRet.TxnDate.GetValue()
        'Get value of RefNumber
        If (Not InvoiceRet.RefNumber Is Nothing) Then
            RefNumber13003 = InvoiceRet.RefNumber.GetValue()

        End If
        If (Not InvoiceRet.BillAddress Is Nothing) Then

            'Get value of Addr1
            If (Not InvoiceRet.BillAddress.Addr1 Is Nothing) Then

                Dim Addr113004 As String
                Addr113004 = InvoiceRet.BillAddress.Addr1.GetValue()

            End If
            'Get value of Addr2
            If (Not InvoiceRet.BillAddress.Addr2 Is Nothing) Then

                Dim Addr213005 As String
                Addr213005 = InvoiceRet.BillAddress.Addr2.GetValue()

            End If
            'Get value of Addr3
            If (Not InvoiceRet.BillAddress.Addr3 Is Nothing) Then

                Dim Addr313006 As String
                Addr313006 = InvoiceRet.BillAddress.Addr3.GetValue()

            End If
            'Get value of Addr4
            If (Not InvoiceRet.BillAddress.Addr4 Is Nothing) Then

                Dim Addr413007 As String
                Addr413007 = InvoiceRet.BillAddress.Addr4.GetValue()

            End If
            'Get value of Addr5
            If (Not InvoiceRet.BillAddress.Addr5 Is Nothing) Then

                Dim Addr513008 As String
                Addr513008 = InvoiceRet.BillAddress.Addr5.GetValue()

            End If
            'Get value of City
            If (Not InvoiceRet.BillAddress.City Is Nothing) Then

                Dim City13009 As String
                City13009 = InvoiceRet.BillAddress.City.GetValue()

            End If
            'Get value of State
            If (Not InvoiceRet.BillAddress.State Is Nothing) Then

                Dim State13010 As String
                State13010 = InvoiceRet.BillAddress.State.GetValue()

            End If
            'Get value of PostalCode
            If (Not InvoiceRet.BillAddress.PostalCode Is Nothing) Then

                Dim PostalCode13011 As String
                PostalCode13011 = InvoiceRet.BillAddress.PostalCode.GetValue()

            End If
            'Get value of Country
            If (Not InvoiceRet.BillAddress.Country Is Nothing) Then

                Dim Country13012 As String
                Country13012 = InvoiceRet.BillAddress.Country.GetValue()

            End If
            'Get value of Note
            If (Not InvoiceRet.BillAddress.Note Is Nothing) Then

                Dim Note13013 As String
                Note13013 = InvoiceRet.BillAddress.Note.GetValue()

            End If
        End If
        If (Not InvoiceRet.BillAddressBlock Is Nothing) Then

            'Get value of Addr1
            If (Not InvoiceRet.BillAddressBlock.Addr1 Is Nothing) Then

                Dim Addr113014 As String
                Addr113014 = InvoiceRet.BillAddressBlock.Addr1.GetValue()

            End If
            'Get value of Addr2
            If (Not InvoiceRet.BillAddressBlock.Addr2 Is Nothing) Then

                Dim Addr213015 As String
                Addr213015 = InvoiceRet.BillAddressBlock.Addr2.GetValue()

            End If
            'Get value of Addr3
            If (Not InvoiceRet.BillAddressBlock.Addr3 Is Nothing) Then

                Dim Addr313016 As String
                Addr313016 = InvoiceRet.BillAddressBlock.Addr3.GetValue()

            End If
            'Get value of Addr4
            If (Not InvoiceRet.BillAddressBlock.Addr4 Is Nothing) Then

                Dim Addr413017 As String
                Addr413017 = InvoiceRet.BillAddressBlock.Addr4.GetValue()

            End If
            'Get value of Addr5
            If (Not InvoiceRet.BillAddressBlock.Addr5 Is Nothing) Then

                Dim Addr513018 As String
                Addr513018 = InvoiceRet.BillAddressBlock.Addr5.GetValue()

            End If
        End If
        If (Not InvoiceRet.ShipAddress Is Nothing) Then

            'Get value of Addr1
            If (Not InvoiceRet.ShipAddress.Addr1 Is Nothing) Then

                Dim Addr113019 As String
                Addr113019 = InvoiceRet.ShipAddress.Addr1.GetValue()

            End If
            'Get value of Addr2
            If (Not InvoiceRet.ShipAddress.Addr2 Is Nothing) Then

                Dim Addr213020 As String
                Addr213020 = InvoiceRet.ShipAddress.Addr2.GetValue()

            End If
            'Get value of Addr3
            If (Not InvoiceRet.ShipAddress.Addr3 Is Nothing) Then

                Dim Addr313021 As String
                Addr313021 = InvoiceRet.ShipAddress.Addr3.GetValue()

            End If
            'Get value of Addr4
            If (Not InvoiceRet.ShipAddress.Addr4 Is Nothing) Then

                Dim Addr413022 As String
                Addr413022 = InvoiceRet.ShipAddress.Addr4.GetValue()

            End If
            'Get value of Addr5
            If (Not InvoiceRet.ShipAddress.Addr5 Is Nothing) Then

                Dim Addr513023 As String
                Addr513023 = InvoiceRet.ShipAddress.Addr5.GetValue()

            End If
            'Get value of City
            If (Not InvoiceRet.ShipAddress.City Is Nothing) Then

                Dim City13024 As String
                City13024 = InvoiceRet.ShipAddress.City.GetValue()

            End If
            'Get value of State
            If (Not InvoiceRet.ShipAddress.State Is Nothing) Then

                Dim State13025 As String
                State13025 = InvoiceRet.ShipAddress.State.GetValue()

            End If
            'Get value of PostalCode
            If (Not InvoiceRet.ShipAddress.PostalCode Is Nothing) Then

                Dim PostalCode13026 As String
                PostalCode13026 = InvoiceRet.ShipAddress.PostalCode.GetValue()

            End If
            'Get value of Country
            If (Not InvoiceRet.ShipAddress.Country Is Nothing) Then

                Dim Country13027 As String
                Country13027 = InvoiceRet.ShipAddress.Country.GetValue()

            End If
            'Get value of Note
            If (Not InvoiceRet.ShipAddress.Note Is Nothing) Then

                Dim Note13028 As String
                Note13028 = InvoiceRet.ShipAddress.Note.GetValue()

            End If
        End If
        If (Not InvoiceRet.ShipAddressBlock Is Nothing) Then

            'Get value of Addr1
            If (Not InvoiceRet.ShipAddressBlock.Addr1 Is Nothing) Then

                Dim Addr113029 As String
                Addr113029 = InvoiceRet.ShipAddressBlock.Addr1.GetValue()

            End If
            'Get value of Addr2
            If (Not InvoiceRet.ShipAddressBlock.Addr2 Is Nothing) Then

                Dim Addr213030 As String
                Addr213030 = InvoiceRet.ShipAddressBlock.Addr2.GetValue()

            End If
            'Get value of Addr3
            If (Not InvoiceRet.ShipAddressBlock.Addr3 Is Nothing) Then

                Dim Addr313031 As String
                Addr313031 = InvoiceRet.ShipAddressBlock.Addr3.GetValue()

            End If
            'Get value of Addr4
            If (Not InvoiceRet.ShipAddressBlock.Addr4 Is Nothing) Then

                Dim Addr413032 As String
                Addr413032 = InvoiceRet.ShipAddressBlock.Addr4.GetValue()

            End If
            'Get value of Addr5
            If (Not InvoiceRet.ShipAddressBlock.Addr5 Is Nothing) Then

                Dim Addr513033 As String
                Addr513033 = InvoiceRet.ShipAddressBlock.Addr5.GetValue()

            End If
        End If
        'Get value of IsPending
        If (Not InvoiceRet.IsPending Is Nothing) Then

            Dim IsPending13034 As Boolean
            IsPending13034 = InvoiceRet.IsPending.GetValue()

        End If
        'Get value of IsFinanceCharge
        If (Not InvoiceRet.IsFinanceCharge Is Nothing) Then

            Dim IsFinanceCharge13035 As Boolean
            IsFinanceCharge13035 = InvoiceRet.IsFinanceCharge.GetValue()

        End If
        'Get value of PONumber
        If (Not InvoiceRet.PONumber Is Nothing) Then

            Dim PONumber13036 As String
            PONumber13036 = InvoiceRet.PONumber.GetValue()

        End If
        If (Not InvoiceRet.TermsRef Is Nothing) Then

            'Get value of ListID
            If (Not InvoiceRet.TermsRef.ListID Is Nothing) Then

                Dim ListID13037 As String
                ListID13037 = InvoiceRet.TermsRef.ListID.GetValue()

            End If
            'Get value of FullName
            If (Not InvoiceRet.TermsRef.FullName Is Nothing) Then

                Dim FullName13038 As String
                FullName13038 = InvoiceRet.TermsRef.FullName.GetValue()

            End If
        End If
        'Get value of DueDate
        If (Not InvoiceRet.DueDate Is Nothing) Then

            Dim DueDate13039 As DateTime
            DueDate13039 = InvoiceRet.DueDate.GetValue()

        End If
        If (Not InvoiceRet.SalesRepRef Is Nothing) Then

            'Get value of ListID
            If (Not InvoiceRet.SalesRepRef.ListID Is Nothing) Then

                Dim ListID13040 As String
                ListID13040 = InvoiceRet.SalesRepRef.ListID.GetValue()

            End If
            'Get value of FullName
            If (Not InvoiceRet.SalesRepRef.FullName Is Nothing) Then

                Dim FullName13041 As String
                FullName13041 = InvoiceRet.SalesRepRef.FullName.GetValue()

            End If
        End If
        'Get value of FOB
        If (Not InvoiceRet.FOB Is Nothing) Then

            Dim FOB13042 As String
            FOB13042 = InvoiceRet.FOB.GetValue()

        End If
        'Get value of ShipDate
        If (Not InvoiceRet.ShipDate Is Nothing) Then

            Dim ShipDate13043 As DateTime
            ShipDate13043 = InvoiceRet.ShipDate.GetValue()

        End If
        If (Not InvoiceRet.ShipMethodRef Is Nothing) Then

            'Get value of ListID
            If (Not InvoiceRet.ShipMethodRef.ListID Is Nothing) Then

                Dim ListID13044 As String
                ListID13044 = InvoiceRet.ShipMethodRef.ListID.GetValue()

            End If
            'Get value of FullName
            If (Not InvoiceRet.ShipMethodRef.FullName Is Nothing) Then

                Dim FullName13045 As String
                FullName13045 = InvoiceRet.ShipMethodRef.FullName.GetValue()

            End If
        End If
        'Get value of Subtotal
        If (Not InvoiceRet.Subtotal Is Nothing) Then
            Subtotal13046 = InvoiceRet.Subtotal.GetValue()

        End If
        If (Not InvoiceRet.ItemSalesTaxRef Is Nothing) Then

            'Get value of ListID
            If (Not InvoiceRet.ItemSalesTaxRef.ListID Is Nothing) Then

                Dim ListID13047 As String
                ListID13047 = InvoiceRet.ItemSalesTaxRef.ListID.GetValue()

            End If
            'Get value of FullName
            If (Not InvoiceRet.ItemSalesTaxRef.FullName Is Nothing) Then

                Dim FullName13048 As String
                FullName13048 = InvoiceRet.ItemSalesTaxRef.FullName.GetValue()

            End If
        End If
        'Get value of SalesTaxPercentage
        If (Not InvoiceRet.SalesTaxPercentage Is Nothing) Then

            Dim SalesTaxPercentage13049 As Double
            SalesTaxPercentage13049 = InvoiceRet.SalesTaxPercentage.GetValue()

        End If
        'Get value of SalesTaxTotal
        If (Not InvoiceRet.SalesTaxTotal Is Nothing) Then

            Dim SalesTaxTotal13050 As Double
            SalesTaxTotal13050 = InvoiceRet.SalesTaxTotal.GetValue()

        End If
        'Get value of AppliedAmount
        If (Not InvoiceRet.AppliedAmount Is Nothing) Then


            AppliedAmount13051 = InvoiceRet.AppliedAmount.GetValue()

        End If
        'Get value of BalanceRemaining
        If (Not InvoiceRet.BalanceRemaining Is Nothing) Then

            Dim BalanceRemaining13052 As Double
            BalanceRemaining13052 = InvoiceRet.BalanceRemaining.GetValue()

        End If
        If (Not InvoiceRet.CurrencyRef Is Nothing) Then

            'Get value of ListID
            If (Not InvoiceRet.CurrencyRef.ListID Is Nothing) Then

                Dim ListID13053 As String
                ListID13053 = InvoiceRet.CurrencyRef.ListID.GetValue()

            End If
            'Get value of FullName
            If (Not InvoiceRet.CurrencyRef.FullName Is Nothing) Then

                Dim FullName13054 As String
                FullName13054 = InvoiceRet.CurrencyRef.FullName.GetValue()

            End If
        End If
        'Get value of ExchangeRate
        If (Not InvoiceRet.ExchangeRate Is Nothing) Then

            Dim ExchangeRate13055 As IQBFloatType
            'ExchangeRate13055 = InvoiceRet.ExchangeRate.GetValue()

        End If
        'Get value of BalanceRemainingInHomeCurrency
        If (Not InvoiceRet.BalanceRemainingInHomeCurrency Is Nothing) Then

            Dim BalanceRemainingInHomeCurrency13056 As Double
            BalanceRemainingInHomeCurrency13056 = InvoiceRet.BalanceRemainingInHomeCurrency.GetValue()

        End If
        'Get value of Memo
        If (Not InvoiceRet.Memo Is Nothing) Then

            Dim Memo13057 As String
            Memo13057 = InvoiceRet.Memo.GetValue()

        End If
        'Get value of IsPaid
        If (Not InvoiceRet.IsPaid Is Nothing) Then

            Dim IsPaid13058 As Boolean
            IsPaid13058 = InvoiceRet.IsPaid.GetValue()

        End If
        If (Not InvoiceRet.CustomerMsgRef Is Nothing) Then

            'Get value of ListID
            If (Not InvoiceRet.CustomerMsgRef.ListID Is Nothing) Then

                Dim ListID13059 As String
                ListID13059 = InvoiceRet.CustomerMsgRef.ListID.GetValue()

            End If
            'Get value of FullName
            If (Not InvoiceRet.CustomerMsgRef.FullName Is Nothing) Then

                Dim FullName13060 As String
                FullName13060 = InvoiceRet.CustomerMsgRef.FullName.GetValue()

            End If
        End If
        'Get value of IsToBePrinted
        If (Not InvoiceRet.IsToBePrinted Is Nothing) Then

            Dim IsToBePrinted13061 As Boolean
            IsToBePrinted13061 = InvoiceRet.IsToBePrinted.GetValue()

        End If
        'Get value of IsToBeEmailed
        If (Not InvoiceRet.IsToBeEmailed Is Nothing) Then

            Dim IsToBeEmailed13062 As Boolean
            IsToBeEmailed13062 = InvoiceRet.IsToBeEmailed.GetValue()

        End If
        If (Not InvoiceRet.CustomerSalesTaxCodeRef Is Nothing) Then

            'Get value of ListID
            If (Not InvoiceRet.CustomerSalesTaxCodeRef.ListID Is Nothing) Then

                Dim ListID13063 As String
                ListID13063 = InvoiceRet.CustomerSalesTaxCodeRef.ListID.GetValue()

            End If
            'Get value of FullName
            If (Not InvoiceRet.CustomerSalesTaxCodeRef.FullName Is Nothing) Then

                Dim FullName13064 As String
                FullName13064 = InvoiceRet.CustomerSalesTaxCodeRef.FullName.GetValue()

            End If
        End If
        'Get value of SuggestedDiscountAmount
        If (Not InvoiceRet.SuggestedDiscountAmount Is Nothing) Then

            Dim SuggestedDiscountAmount13065 As Double
            SuggestedDiscountAmount13065 = InvoiceRet.SuggestedDiscountAmount.GetValue()

        End If
        'Get value of SuggestedDiscountDate
        If (Not InvoiceRet.SuggestedDiscountDate Is Nothing) Then

            Dim SuggestedDiscountDate13066 As DateTime
            SuggestedDiscountDate13066 = InvoiceRet.SuggestedDiscountDate.GetValue()

        End If
        'Get value of Other
        If (Not InvoiceRet.Other Is Nothing) Then

            Dim Other13067 As String
            Other13067 = InvoiceRet.Other.GetValue()

        End If
        'Get value of ExternalGUID
        If (Not InvoiceRet.ExternalGUID Is Nothing) Then

            Dim ExternalGUID13068 As String
            ExternalGUID13068 = InvoiceRet.ExternalGUID.GetValue()

        End If
        If (Not InvoiceRet.LinkedTxnList Is Nothing) Then

            Dim i13069 As Integer
            For i13069 = 0 To InvoiceRet.LinkedTxnList.Count - 1

                Dim LinkedTxn As ILinkedTxn
                Dim RefNumber13073 As String = ""
                Dim LinkType13074 As ENLinkType

                LinkedTxn = InvoiceRet.LinkedTxnList.GetAt(i13069)
                'Get value of TxnID
                Dim TxnID13070 As String
                TxnID13070 = LinkedTxn.TxnID.GetValue()
                'Get value of TxnType

                TxnType13071 = LinkedTxn.TxnType.GetValue()
                'Get value of TxnDate
                Dim TxnDate13072 As DateTime
                TxnDate13072 = LinkedTxn.TxnDate.GetValue()
                'Get value of RefNumber
                If (Not LinkedTxn.RefNumber Is Nothing) Then

                    RefNumber13073 = LinkedTxn.RefNumber.GetValue()

                End If
                'Get value of LinkType
                If (Not LinkedTxn.LinkType Is Nothing) Then


                    LinkType13074 = LinkedTxn.LinkType.GetValue()

                End If
                'Get value of Amount
                Dim Amount13075 As Double
                Amount13075 = LinkedTxn.Amount.GetValue()

                Dim tmpDescMvto As String = ""
                Select Case TxnType13071
                    Case ENTxnType.ttARRefundCreditCard
                        tmpDescMvto = "INVOICE"
                    Case ENTxnType.ttBill
                        tmpDescMvto = "INVOICE"
                    Case ENTxnType.ttBillPaymentCheck
                        tmpDescMvto = "BILL PAYMENT CHECK"
                    Case ENTxnType.ttBillPaymentCreditCard
                        tmpDescMvto = "BILL PAYMENT CC"
                    Case ENTxnType.ttCharge
                        tmpDescMvto = "CHARGE"
                    Case ENTxnType.ttCheck
                        tmpDescMvto = "CHECK"
                    Case ENTxnType.ttDeposit
                        tmpDescMvto = "DEPOSIT"
                    Case ENTxnType.ttInvoice
                        tmpDescMvto = "INVOICE"
                    Case ENTxnType.ttTransfer
                        tmpDescMvto = "TRANSFER"
                    Case ENTxnType.ttVendorCredit
                        tmpDescMvto = "VENDOR CREDIT"
                    Case ENTxnType.ttReceivePayment
                        tmpDescMvto = "PAYMENT"
                    Case ENTxnType.ttCreditMemo
                        tmpDescMvto = "CREDIT MEMO"
                    Case Else
                        tmpDescMvto = TxnType13071.ToString
                End Select


                Dim NuevaFilaDetalles As dsQBInvoices.QBInvoicesRow
                NuevaFilaDetalles = Me.Var_dsQBInvoices.QBInvoices.NewQBInvoicesRow
                With NuevaFilaDetalles
                    .AppliedAmount = Amount13075
                    .Customer = FullName12995
                    .Fecha = TxnDate13072
                    .RefNumber = RefNumber13073
                    .TipoMvto = tmpDescMvto

                End With
                Me.Var_dsQBInvoices.QBInvoices.AddQBInvoicesRow(NuevaFilaDetalles)

            Next i13069
        End If
        If (Not InvoiceRet.ORInvoiceLineRetList Is Nothing) Then

            Dim i13076 As Integer
            For i13076 = 0 To InvoiceRet.ORInvoiceLineRetList.Count - 1

                Dim ORInvoiceLineRet13077 As IORInvoiceLineRet
                ORInvoiceLineRet13077 = InvoiceRet.ORInvoiceLineRetList.GetAt(i13076)
                If (Not ORInvoiceLineRet13077.InvoiceLineRet Is Nothing) Then

                    If (Not ORInvoiceLineRet13077.InvoiceLineRet Is Nothing) Then

                        'Get value of TxnLineID
                        Dim TxnLineID13078 As String
                        TxnLineID13078 = ORInvoiceLineRet13077.InvoiceLineRet.TxnLineID.GetValue()
                        If (Not ORInvoiceLineRet13077.InvoiceLineRet.ItemRef Is Nothing) Then

                            'Get value of ListID
                            If (Not ORInvoiceLineRet13077.InvoiceLineRet.ItemRef.ListID Is Nothing) Then

                                Dim ListID13079 As String
                                ListID13079 = ORInvoiceLineRet13077.InvoiceLineRet.ItemRef.ListID.GetValue()

                            End If
                            'Get value of FullName
                            If (Not ORInvoiceLineRet13077.InvoiceLineRet.ItemRef.FullName Is Nothing) Then

                                Dim FullName13080 As String
                                FullName13080 = ORInvoiceLineRet13077.InvoiceLineRet.ItemRef.FullName.GetValue()

                            End If
                        End If
                        'Get value of Desc
                        If (Not ORInvoiceLineRet13077.InvoiceLineRet.Desc Is Nothing) Then


                            Desc13081 = ORInvoiceLineRet13077.InvoiceLineRet.Desc.GetValue()

                        End If
                        'Get value of Quantity
                        If (Not ORInvoiceLineRet13077.InvoiceLineRet.Quantity Is Nothing) Then

                            Dim Quantity13082 As Integer
                            Quantity13082 = ORInvoiceLineRet13077.InvoiceLineRet.Quantity.GetValue()

                        End If
                        'Get value of UnitOfMeasure
                        If (Not ORInvoiceLineRet13077.InvoiceLineRet.UnitOfMeasure Is Nothing) Then

                            Dim UnitOfMeasure13083 As String
                            UnitOfMeasure13083 = ORInvoiceLineRet13077.InvoiceLineRet.UnitOfMeasure.GetValue()

                        End If
                        If (Not ORInvoiceLineRet13077.InvoiceLineRet.OverrideUOMSetRef Is Nothing) Then

                            'Get value of ListID
                            If (Not ORInvoiceLineRet13077.InvoiceLineRet.OverrideUOMSetRef.ListID Is Nothing) Then

                                Dim ListID13084 As String
                                ListID13084 = ORInvoiceLineRet13077.InvoiceLineRet.OverrideUOMSetRef.ListID.GetValue()

                            End If
                            'Get value of FullName
                            If (Not ORInvoiceLineRet13077.InvoiceLineRet.OverrideUOMSetRef.FullName Is Nothing) Then

                                Dim FullName13085 As String
                                FullName13085 = ORInvoiceLineRet13077.InvoiceLineRet.OverrideUOMSetRef.FullName.GetValue()

                            End If
                        End If
                        If (Not ORInvoiceLineRet13077.InvoiceLineRet.ORRate Is Nothing) Then

                            If (Not ORInvoiceLineRet13077.InvoiceLineRet.ORRate.Rate Is Nothing) Then

                                'Get value of Rate
                                If (Not ORInvoiceLineRet13077.InvoiceLineRet.ORRate.Rate Is Nothing) Then

                                    Dim Rate13087 As Double
                                    Rate13087 = ORInvoiceLineRet13077.InvoiceLineRet.ORRate.Rate.GetValue()

                                End If
                            End If
                            If (Not ORInvoiceLineRet13077.InvoiceLineRet.ORRate.RatePercent Is Nothing) Then

                                'Get value of RatePercent
                                If (Not ORInvoiceLineRet13077.InvoiceLineRet.ORRate.RatePercent Is Nothing) Then

                                    Dim RatePercent13088 As Double
                                    RatePercent13088 = ORInvoiceLineRet13077.InvoiceLineRet.ORRate.RatePercent.GetValue()

                                End If
                            End If
                        End If
                        If (Not ORInvoiceLineRet13077.InvoiceLineRet.ClassRef Is Nothing) Then

                            'Get value of ListID
                            If (Not ORInvoiceLineRet13077.InvoiceLineRet.ClassRef.ListID Is Nothing) Then

                                Dim ListID13089 As String
                                ListID13089 = ORInvoiceLineRet13077.InvoiceLineRet.ClassRef.ListID.GetValue()

                            End If
                            'Get value of FullName
                            If (Not ORInvoiceLineRet13077.InvoiceLineRet.ClassRef.FullName Is Nothing) Then

                                Dim FullName13090 As String
                                FullName13090 = ORInvoiceLineRet13077.InvoiceLineRet.ClassRef.FullName.GetValue()

                            End If
                        End If
                        'Get value of Amount
                        If (Not ORInvoiceLineRet13077.InvoiceLineRet.Amount Is Nothing) Then

                            Dim Amount13091 As Double
                            Amount13091 = ORInvoiceLineRet13077.InvoiceLineRet.Amount.GetValue()

                        End If
                        If (Not ORInvoiceLineRet13077.InvoiceLineRet.InventorySiteRef Is Nothing) Then

                            'Get value of ListID
                            If (Not ORInvoiceLineRet13077.InvoiceLineRet.InventorySiteRef.ListID Is Nothing) Then

                                Dim ListID13092 As String
                                ListID13092 = ORInvoiceLineRet13077.InvoiceLineRet.InventorySiteRef.ListID.GetValue()

                            End If
                            'Get value of FullName
                            If (Not ORInvoiceLineRet13077.InvoiceLineRet.InventorySiteRef.FullName Is Nothing) Then

                                Dim FullName13093 As String
                                FullName13093 = ORInvoiceLineRet13077.InvoiceLineRet.InventorySiteRef.FullName.GetValue()

                            End If
                        End If
                        If (Not ORInvoiceLineRet13077.InvoiceLineRet.InventorySiteLocationRef Is Nothing) Then

                            'Get value of ListID
                            If (Not ORInvoiceLineRet13077.InvoiceLineRet.InventorySiteLocationRef.ListID Is Nothing) Then

                                Dim ListID13094 As String
                                ListID13094 = ORInvoiceLineRet13077.InvoiceLineRet.InventorySiteLocationRef.ListID.GetValue()

                            End If
                            'Get value of FullName
                            If (Not ORInvoiceLineRet13077.InvoiceLineRet.InventorySiteLocationRef.FullName Is Nothing) Then

                                Dim FullName13095 As String
                                FullName13095 = ORInvoiceLineRet13077.InvoiceLineRet.InventorySiteLocationRef.FullName.GetValue()

                            End If
                        End If
                        If (Not ORInvoiceLineRet13077.InvoiceLineRet.ORSerialLotNumber Is Nothing) Then

                            If (Not ORInvoiceLineRet13077.InvoiceLineRet.ORSerialLotNumber.SerialNumber Is Nothing) Then

                                'Get value of SerialNumber
                                If (Not ORInvoiceLineRet13077.InvoiceLineRet.ORSerialLotNumber.SerialNumber Is Nothing) Then

                                    Dim SerialNumber13097 As String
                                    SerialNumber13097 = ORInvoiceLineRet13077.InvoiceLineRet.ORSerialLotNumber.SerialNumber.GetValue()

                                End If
                            End If
                            If (Not ORInvoiceLineRet13077.InvoiceLineRet.ORSerialLotNumber.LotNumber Is Nothing) Then

                                'Get value of LotNumber
                                If (Not ORInvoiceLineRet13077.InvoiceLineRet.ORSerialLotNumber.LotNumber Is Nothing) Then

                                    Dim LotNumber13098 As String
                                    LotNumber13098 = ORInvoiceLineRet13077.InvoiceLineRet.ORSerialLotNumber.LotNumber.GetValue()

                                End If
                            End If
                        End If
                        'Get value of ServiceDate
                        If (Not ORInvoiceLineRet13077.InvoiceLineRet.ServiceDate Is Nothing) Then

                            Dim ServiceDate13099 As DateTime
                            ServiceDate13099 = ORInvoiceLineRet13077.InvoiceLineRet.ServiceDate.GetValue()

                        End If
                        If (Not ORInvoiceLineRet13077.InvoiceLineRet.SalesTaxCodeRef Is Nothing) Then

                            'Get value of ListID
                            If (Not ORInvoiceLineRet13077.InvoiceLineRet.SalesTaxCodeRef.ListID Is Nothing) Then

                                Dim ListID13100 As String
                                ListID13100 = ORInvoiceLineRet13077.InvoiceLineRet.SalesTaxCodeRef.ListID.GetValue()

                            End If
                            'Get value of FullName
                            If (Not ORInvoiceLineRet13077.InvoiceLineRet.SalesTaxCodeRef.FullName Is Nothing) Then

                                Dim FullName13101 As String
                                FullName13101 = ORInvoiceLineRet13077.InvoiceLineRet.SalesTaxCodeRef.FullName.GetValue()

                            End If
                        End If
                        'Get value of Other1
                        If (Not ORInvoiceLineRet13077.InvoiceLineRet.Other1 Is Nothing) Then

                            Dim Other113102 As String
                            Other113102 = ORInvoiceLineRet13077.InvoiceLineRet.Other1.GetValue()

                        End If
                        'Get value of Other2
                        If (Not ORInvoiceLineRet13077.InvoiceLineRet.Other2 Is Nothing) Then

                            Dim Other213103 As String
                            Other213103 = ORInvoiceLineRet13077.InvoiceLineRet.Other2.GetValue()

                        End If
                        If (Not ORInvoiceLineRet13077.InvoiceLineRet.DataExtRetList Is Nothing) Then

                            Dim i13104 As Integer
                            For i13104 = 0 To ORInvoiceLineRet13077.InvoiceLineRet.DataExtRetList.Count - 1

                                Dim DataExtRet As IDataExtRet
                                DataExtRet = ORInvoiceLineRet13077.InvoiceLineRet.DataExtRetList.GetAt(i13104)
                                'Get value of OwnerID
                                If (Not DataExtRet.OwnerID Is Nothing) Then

                                    Dim OwnerID13105 As String
                                    OwnerID13105 = DataExtRet.OwnerID.GetValue()

                                End If
                                'Get value of DataExtName
                                Dim DataExtName13106 As String
                                DataExtName13106 = DataExtRet.DataExtName.GetValue()
                                'Get value of DataExtType
                                Dim DataExtType13107 As ENDataExtType
                                DataExtType13107 = DataExtRet.DataExtType.GetValue()
                                'Get value of DataExtValue
                                Dim DataExtValue13108 As String
                                DataExtValue13108 = DataExtRet.DataExtValue.GetValue()
                            Next i13104
                        End If
                    End If

                End If

                If (Not ORInvoiceLineRet13077.InvoiceLineGroupRet Is Nothing) Then

                    If (Not ORInvoiceLineRet13077.InvoiceLineGroupRet Is Nothing) Then

                        'Get value of TxnLineID
                        Dim TxnLineID13109 As String
                        TxnLineID13109 = ORInvoiceLineRet13077.InvoiceLineGroupRet.TxnLineID.GetValue()
                        'Get value of ListID
                        If (Not ORInvoiceLineRet13077.InvoiceLineGroupRet.ItemGroupRef.ListID Is Nothing) Then

                            Dim ListID13110 As String
                            ListID13110 = ORInvoiceLineRet13077.InvoiceLineGroupRet.ItemGroupRef.ListID.GetValue()

                        End If
                        'Get value of FullName
                        If (Not ORInvoiceLineRet13077.InvoiceLineGroupRet.ItemGroupRef.FullName Is Nothing) Then

                            Dim FullName13111 As String
                            FullName13111 = ORInvoiceLineRet13077.InvoiceLineGroupRet.ItemGroupRef.FullName.GetValue()

                        End If
                        'Get value of Desc
                        If (Not ORInvoiceLineRet13077.InvoiceLineGroupRet.Desc Is Nothing) Then

                            Dim Desc13112 As String
                            Desc13112 = ORInvoiceLineRet13077.InvoiceLineGroupRet.Desc.GetValue()

                        End If
                        'Get value of Quantity
                        If (Not ORInvoiceLineRet13077.InvoiceLineGroupRet.Quantity Is Nothing) Then

                            Dim Quantity13113 As Integer
                            Quantity13113 = ORInvoiceLineRet13077.InvoiceLineGroupRet.Quantity.GetValue()

                        End If
                        'Get value of UnitOfMeasure
                        If (Not ORInvoiceLineRet13077.InvoiceLineGroupRet.UnitOfMeasure Is Nothing) Then

                            Dim UnitOfMeasure13114 As String
                            UnitOfMeasure13114 = ORInvoiceLineRet13077.InvoiceLineGroupRet.UnitOfMeasure.GetValue()

                        End If
                        If (Not ORInvoiceLineRet13077.InvoiceLineGroupRet.OverrideUOMSetRef Is Nothing) Then

                            'Get value of ListID
                            If (Not ORInvoiceLineRet13077.InvoiceLineGroupRet.OverrideUOMSetRef.ListID Is Nothing) Then

                                Dim ListID13115 As String
                                ListID13115 = ORInvoiceLineRet13077.InvoiceLineGroupRet.OverrideUOMSetRef.ListID.GetValue()

                            End If
                            'Get value of FullName
                            If (Not ORInvoiceLineRet13077.InvoiceLineGroupRet.OverrideUOMSetRef.FullName Is Nothing) Then

                                Dim FullName13116 As String
                                FullName13116 = ORInvoiceLineRet13077.InvoiceLineGroupRet.OverrideUOMSetRef.FullName.GetValue()

                            End If
                        End If
                        'Get value of IsPrintItemsInGroup
                        Dim IsPrintItemsInGroup13117 As Boolean
                        IsPrintItemsInGroup13117 = ORInvoiceLineRet13077.InvoiceLineGroupRet.IsPrintItemsInGroup.GetValue()
                        'Get value of TotalAmount
                        Dim TotalAmount13118 As Double
                        TotalAmount13118 = ORInvoiceLineRet13077.InvoiceLineGroupRet.TotalAmount.GetValue()
                        If (Not ORInvoiceLineRet13077.InvoiceLineGroupRet.InvoiceLineRetList Is Nothing) Then

                            Dim i13119 As Integer
                            For i13119 = 0 To ORInvoiceLineRet13077.InvoiceLineGroupRet.InvoiceLineRetList.Count - 1

                                Dim InvoiceLineRet As IInvoiceLineRet
                                InvoiceLineRet = ORInvoiceLineRet13077.InvoiceLineGroupRet.InvoiceLineRetList.GetAt(i13119)
                                'Get value of TxnLineID
                                Dim TxnLineID13120 As String
                                TxnLineID13120 = InvoiceLineRet.TxnLineID.GetValue()
                                If (Not InvoiceLineRet.ItemRef Is Nothing) Then

                                    'Get value of ListID
                                    If (Not InvoiceLineRet.ItemRef.ListID Is Nothing) Then

                                        Dim ListID13121 As String
                                        ListID13121 = InvoiceLineRet.ItemRef.ListID.GetValue()

                                    End If
                                    'Get value of FullName
                                    If (Not InvoiceLineRet.ItemRef.FullName Is Nothing) Then

                                        Dim FullName13122 As String
                                        FullName13122 = InvoiceLineRet.ItemRef.FullName.GetValue()

                                    End If
                                End If
                                'Get value of Desc
                                If (Not InvoiceLineRet.Desc Is Nothing) Then

                                    Dim Desc13123 As String
                                    Desc13123 = InvoiceLineRet.Desc.GetValue()

                                End If
                                'Get value of Quantity
                                If (Not InvoiceLineRet.Quantity Is Nothing) Then

                                    Dim Quantity13124 As Integer
                                    Quantity13124 = InvoiceLineRet.Quantity.GetValue()

                                End If
                                'Get value of UnitOfMeasure
                                If (Not InvoiceLineRet.UnitOfMeasure Is Nothing) Then

                                    Dim UnitOfMeasure13125 As String
                                    UnitOfMeasure13125 = InvoiceLineRet.UnitOfMeasure.GetValue()

                                End If
                                If (Not InvoiceLineRet.OverrideUOMSetRef Is Nothing) Then

                                    'Get value of ListID
                                    If (Not InvoiceLineRet.OverrideUOMSetRef.ListID Is Nothing) Then

                                        Dim ListID13126 As String
                                        ListID13126 = InvoiceLineRet.OverrideUOMSetRef.ListID.GetValue()

                                    End If
                                    'Get value of FullName
                                    If (Not InvoiceLineRet.OverrideUOMSetRef.FullName Is Nothing) Then

                                        Dim FullName13127 As String
                                        FullName13127 = InvoiceLineRet.OverrideUOMSetRef.FullName.GetValue()

                                    End If
                                End If
                                If (Not InvoiceLineRet.ORRate Is Nothing) Then

                                    If (Not InvoiceLineRet.ORRate.Rate Is Nothing) Then

                                        'Get value of Rate
                                        If (Not InvoiceLineRet.ORRate.Rate Is Nothing) Then

                                            Dim Rate13129 As Double
                                            Rate13129 = InvoiceLineRet.ORRate.Rate.GetValue()

                                        End If
                                    End If
                                    If (Not InvoiceLineRet.ORRate.RatePercent Is Nothing) Then

                                        'Get value of RatePercent
                                        If (Not InvoiceLineRet.ORRate.RatePercent Is Nothing) Then

                                            Dim RatePercent13130 As Double
                                            RatePercent13130 = InvoiceLineRet.ORRate.RatePercent.GetValue()

                                        End If
                                    End If
                                End If
                                If (Not InvoiceLineRet.ClassRef Is Nothing) Then

                                    'Get value of ListID
                                    If (Not InvoiceLineRet.ClassRef.ListID Is Nothing) Then

                                        Dim ListID13131 As String
                                        ListID13131 = InvoiceLineRet.ClassRef.ListID.GetValue()

                                    End If
                                    'Get value of FullName
                                    If (Not InvoiceLineRet.ClassRef.FullName Is Nothing) Then

                                        Dim FullName13132 As String
                                        FullName13132 = InvoiceLineRet.ClassRef.FullName.GetValue()

                                    End If
                                End If
                                'Get value of Amount
                                If (Not InvoiceLineRet.Amount Is Nothing) Then

                                    Dim Amount13133 As Double
                                    Amount13133 = InvoiceLineRet.Amount.GetValue()

                                End If
                                If (Not InvoiceLineRet.InventorySiteRef Is Nothing) Then

                                    'Get value of ListID
                                    If (Not InvoiceLineRet.InventorySiteRef.ListID Is Nothing) Then

                                        Dim ListID13134 As String
                                        ListID13134 = InvoiceLineRet.InventorySiteRef.ListID.GetValue()

                                    End If
                                    'Get value of FullName
                                    If (Not InvoiceLineRet.InventorySiteRef.FullName Is Nothing) Then

                                        Dim FullName13135 As String
                                        FullName13135 = InvoiceLineRet.InventorySiteRef.FullName.GetValue()

                                    End If
                                End If
                                If (Not InvoiceLineRet.InventorySiteLocationRef Is Nothing) Then

                                    'Get value of ListID
                                    If (Not InvoiceLineRet.InventorySiteLocationRef.ListID Is Nothing) Then

                                        Dim ListID13136 As String
                                        ListID13136 = InvoiceLineRet.InventorySiteLocationRef.ListID.GetValue()

                                    End If
                                    'Get value of FullName
                                    If (Not InvoiceLineRet.InventorySiteLocationRef.FullName Is Nothing) Then

                                        Dim FullName13137 As String
                                        FullName13137 = InvoiceLineRet.InventorySiteLocationRef.FullName.GetValue()

                                    End If
                                End If
                                If (Not InvoiceLineRet.ORSerialLotNumber Is Nothing) Then

                                    If (Not InvoiceLineRet.ORSerialLotNumber.SerialNumber Is Nothing) Then

                                        'Get value of SerialNumber
                                        If (Not InvoiceLineRet.ORSerialLotNumber.SerialNumber Is Nothing) Then

                                            Dim SerialNumber13139 As String
                                            SerialNumber13139 = InvoiceLineRet.ORSerialLotNumber.SerialNumber.GetValue()

                                        End If
                                    End If
                                    If (Not InvoiceLineRet.ORSerialLotNumber.LotNumber Is Nothing) Then

                                        'Get value of LotNumber
                                        If (Not InvoiceLineRet.ORSerialLotNumber.LotNumber Is Nothing) Then

                                            Dim LotNumber13140 As String
                                            LotNumber13140 = InvoiceLineRet.ORSerialLotNumber.LotNumber.GetValue()

                                        End If
                                    End If
                                End If
                                'Get value of ServiceDate
                                If (Not InvoiceLineRet.ServiceDate Is Nothing) Then

                                    Dim ServiceDate13141 As DateTime
                                    ServiceDate13141 = InvoiceLineRet.ServiceDate.GetValue()

                                End If
                                If (Not InvoiceLineRet.SalesTaxCodeRef Is Nothing) Then

                                    'Get value of ListID
                                    If (Not InvoiceLineRet.SalesTaxCodeRef.ListID Is Nothing) Then

                                        Dim ListID13142 As String
                                        ListID13142 = InvoiceLineRet.SalesTaxCodeRef.ListID.GetValue()

                                    End If
                                    'Get value of FullName
                                    If (Not InvoiceLineRet.SalesTaxCodeRef.FullName Is Nothing) Then

                                        Dim FullName13143 As String
                                        FullName13143 = InvoiceLineRet.SalesTaxCodeRef.FullName.GetValue()

                                    End If
                                End If
                                'Get value of Other1
                                If (Not InvoiceLineRet.Other1 Is Nothing) Then

                                    Dim Other113144 As String
                                    Other113144 = InvoiceLineRet.Other1.GetValue()

                                End If
                                'Get value of Other2
                                If (Not InvoiceLineRet.Other2 Is Nothing) Then

                                    Dim Other213145 As String
                                    Other213145 = InvoiceLineRet.Other2.GetValue()

                                End If
                                If (Not InvoiceLineRet.DataExtRetList Is Nothing) Then

                                    Dim i13146 As Integer
                                    For i13146 = 0 To InvoiceLineRet.DataExtRetList.Count - 1

                                        Dim DataExtRet As IDataExtRet
                                        DataExtRet = InvoiceLineRet.DataExtRetList.GetAt(i13146)
                                        'Get value of OwnerID
                                        If (Not DataExtRet.OwnerID Is Nothing) Then

                                            Dim OwnerID13147 As String
                                            OwnerID13147 = DataExtRet.OwnerID.GetValue()

                                        End If
                                        'Get value of DataExtName
                                        Dim DataExtName13148 As String
                                        DataExtName13148 = DataExtRet.DataExtName.GetValue()
                                        'Get value of DataExtType
                                        Dim DataExtType13149 As ENDataExtType
                                        DataExtType13149 = DataExtRet.DataExtType.GetValue()
                                        'Get value of DataExtValue
                                        Dim DataExtValue13150 As String
                                        DataExtValue13150 = DataExtRet.DataExtValue.GetValue()
                                    Next i13146
                                End If
                            Next i13119

                        End If
                        If (Not ORInvoiceLineRet13077.InvoiceLineGroupRet.DataExtRetList Is Nothing) Then

                            Dim i13151 As Integer
                            For i13151 = 0 To ORInvoiceLineRet13077.InvoiceLineGroupRet.DataExtRetList.Count - 1

                                Dim DataExtRet As IDataExtRet
                                DataExtRet = ORInvoiceLineRet13077.InvoiceLineGroupRet.DataExtRetList.GetAt(i13151)
                                'Get value of OwnerID
                                If (Not DataExtRet.OwnerID Is Nothing) Then

                                    Dim OwnerID13152 As String
                                    OwnerID13152 = DataExtRet.OwnerID.GetValue()

                                End If
                                'Get value of DataExtName
                                Dim DataExtName13153 As String
                                DataExtName13153 = DataExtRet.DataExtName.GetValue()
                                'Get value of DataExtType
                                Dim DataExtType13154 As ENDataExtType
                                DataExtType13154 = DataExtRet.DataExtType.GetValue()
                                'Get value of DataExtValue
                                Dim DataExtValue13155 As String
                                DataExtValue13155 = DataExtRet.DataExtValue.GetValue()
                            Next i13151
                        End If
                    End If

                End If

            Next i13076

        End If

        If (Not InvoiceRet.DataExtRetList Is Nothing) Then

            Dim i13156 As Integer
            For i13156 = 0 To InvoiceRet.DataExtRetList.Count - 1

                Dim DataExtRet As IDataExtRet
                DataExtRet = InvoiceRet.DataExtRetList.GetAt(i13156)
                'Get value of OwnerID
                If (Not DataExtRet.OwnerID Is Nothing) Then

                    Dim OwnerID13157 As String
                    OwnerID13157 = DataExtRet.OwnerID.GetValue()

                End If
                'Get value of DataExtName
                Dim DataExtName13158 As String
                DataExtName13158 = DataExtRet.DataExtName.GetValue()
                'Get value of DataExtType
                Dim DataExtType13159 As ENDataExtType
                DataExtType13159 = DataExtRet.DataExtType.GetValue()
                'Get value of DataExtValue
                Dim DataExtValue13160 As String
                DataExtValue13160 = DataExtRet.DataExtValue.GetValue()
            Next i13156
        End If



        Dim NuevaFila As dsQBInvoices.QBInvoicesRow
        NuevaFila = Me.Var_dsQBInvoices.QBInvoices.NewQBInvoicesRow
        With NuevaFila
            .AppliedAmount = Subtotal13046
            .Customer = FullName12995
            .Fecha = TxnDate13002
            .RefNumber = RefNumber13003
            .TipoMvto = Desc13081.Replace("RETAINER FEE", "INVOICE")

        End With
        Me.Var_dsQBInvoices.QBInvoices.AddQBInvoicesRow(NuevaFila)

    End Sub


    Private Sub BuildInvoiceQueryRq(ByVal requestMsgSet As IMsgSetRequest, ByVal prmFechaIni As Date, ByVal prmFechaFin As Date)

        Dim InvoiceQueryRq As IInvoiceQuery
        InvoiceQueryRq = requestMsgSet.AppendInvoiceQueryRq()
        Dim ORInvoiceQueryElementType12982 As String
        ORInvoiceQueryElementType12982 = "InvoiceFilter"

        If (ORInvoiceQueryElementType12982 = "TxnIDList") Then

            'Set field value for TxnIDList
            'May create more than one of these if needed
            InvoiceQueryRq.ORInvoiceQuery.TxnIDList.Add("200000-1011023419")


        End If
        If (ORInvoiceQueryElementType12982 = "RefNumberList") Then

            'Set field value for RefNumberList
            'May create more than one of these if needed
            InvoiceQueryRq.ORInvoiceQuery.RefNumberList.Add("ab")

        End If
        If (ORInvoiceQueryElementType12982 = "RefNumberCaseSensitiveList") Then

            'Set field value for RefNumberCaseSensitiveList
            'May create more than one of these if needed
            InvoiceQueryRq.ORInvoiceQuery.RefNumberCaseSensitiveList.Add("ab")

        End If
        If (ORInvoiceQueryElementType12982 = "InvoiceFilter") Then

            'Set field value for MaxReturned
            InvoiceQueryRq.ORInvoiceQuery.InvoiceFilter.MaxReturned.SetValue(6)
            Dim ORDateRangeFilterElementType12983 As String
            ORDateRangeFilterElementType12983 = "TxnDateRangeFilter"

            If (ORDateRangeFilterElementType12983 = "ModifiedDateRangeFilter") Then

                'Set field value for FromModifiedDate
                InvoiceQueryRq.ORInvoiceQuery.InvoiceFilter.ORDateRangeFilter.ModifiedDateRangeFilter.FromModifiedDate.SetValue(DateTime.Parse("12/15/2007 12:15:12"), False)
                'Set field value for ToModifiedDate
                InvoiceQueryRq.ORInvoiceQuery.InvoiceFilter.ORDateRangeFilter.ModifiedDateRangeFilter.ToModifiedDate.SetValue(DateTime.Parse("12/15/2007 12:15:12"), False)

            End If
            If (ORDateRangeFilterElementType12983 = "TxnDateRangeFilter") Then

                Dim ORTxnDateRangeFilterElementType12984 As String
                ORTxnDateRangeFilterElementType12984 = "TxnDateFilter"
                If (ORTxnDateRangeFilterElementType12984 = "TxnDateFilter") Then

                    'Set field value for FromTxnDate
                    InvoiceQueryRq.ORInvoiceQuery.InvoiceFilter.ORDateRangeFilter.TxnDateRangeFilter.ORTxnDateRangeFilter.TxnDateFilter.FromTxnDate.SetValue(DateTime.Parse(prmFechaIni))
                    'Set field value for ToTxnDate
                    InvoiceQueryRq.ORInvoiceQuery.InvoiceFilter.ORDateRangeFilter.TxnDateRangeFilter.ORTxnDateRangeFilter.TxnDateFilter.ToTxnDate.SetValue(DateTime.Parse(prmFechaFin))

                End If
                If (ORTxnDateRangeFilterElementType12984 = "DateMacro") Then

                    'Set field value for DateMacro
                    InvoiceQueryRq.ORInvoiceQuery.InvoiceFilter.ORDateRangeFilter.TxnDateRangeFilter.ORTxnDateRangeFilter.DateMacro.SetValue(ENDateMacro.dmAll)

                End If
            End If
            Dim OREntityFilterElementType12985 As String
            OREntityFilterElementType12985 = "FullNameList"

            If (OREntityFilterElementType12985 = "ListIDList") Then

                'Set field value for ListIDList
                'May create more than one of these if needed
                InvoiceQueryRq.ORInvoiceQuery.InvoiceFilter.EntityFilter.OREntityFilter.ListIDList.Add("200000-1011023419")

            End If
            If (OREntityFilterElementType12985 = "FullNameList") Then

                'Set field value for FullNameList
                'May create more than one of these if needed

                'InvoiceQueryRq.ORInvoiceQuery.InvoiceFilter.EntityFilter.OREntityFilter.FullNameList.Add(Me.txtCustomer.Text)   temporal 

            End If
            If (OREntityFilterElementType12985 = "ListIDWithChildren") Then

                'Set field value for ListIDWithChildren
                InvoiceQueryRq.ORInvoiceQuery.InvoiceFilter.EntityFilter.OREntityFilter.ListIDWithChildren.SetValue("200000-1011023419")

            End If
            If (OREntityFilterElementType12985 = "FullNameWithChildren") Then

                'Set field value for FullNameWithChildren
                InvoiceQueryRq.ORInvoiceQuery.InvoiceFilter.EntityFilter.OREntityFilter.FullNameWithChildren.SetValue("ab")

            End If
            Dim ORAccountFilterElementType12986 As String
            ORAccountFilterElementType12986 = ""

            If (ORAccountFilterElementType12986 = "ListIDList") Then

                'Set field value for ListIDList
                'May create more than one of these if needed
                InvoiceQueryRq.ORInvoiceQuery.InvoiceFilter.AccountFilter.ORAccountFilter.ListIDList.Add("200000-1011023419")

            End If
            If (ORAccountFilterElementType12986 = "FullNameList") Then

                'Set field value for FullNameList
                'May create more than one of these if needed
                InvoiceQueryRq.ORInvoiceQuery.InvoiceFilter.AccountFilter.ORAccountFilter.FullNameList.Add("ab")

            End If
            If (ORAccountFilterElementType12986 = "ListIDWithChildren") Then

                'Set field value for ListIDWithChildren
                InvoiceQueryRq.ORInvoiceQuery.InvoiceFilter.AccountFilter.ORAccountFilter.ListIDWithChildren.SetValue("200000-1011023419")

            End If
            If (ORAccountFilterElementType12986 = "FullNameWithChildren") Then

                'Set field value for FullNameWithChildren
                InvoiceQueryRq.ORInvoiceQuery.InvoiceFilter.AccountFilter.ORAccountFilter.FullNameWithChildren.SetValue("ab")

            End If
            Dim ORRefNumberFilterElementType12987 As String
            ORRefNumberFilterElementType12987 = "" 'RefNumberFilter

            If (ORRefNumberFilterElementType12987 = "RefNumberFilter") Then

                'Set field value for MatchCriterion
                InvoiceQueryRq.ORInvoiceQuery.InvoiceFilter.ORRefNumberFilter.RefNumberFilter.MatchCriterion.SetValue(ENMatchCriterion.mcStartsWith)
                'Set field value for RefNumber
                InvoiceQueryRq.ORInvoiceQuery.InvoiceFilter.ORRefNumberFilter.RefNumberFilter.RefNumber.SetValue("ab")

            End If
            If (ORRefNumberFilterElementType12987 = "RefNumberRangeFilter") Then

                'Set field value for FromRefNumber
                InvoiceQueryRq.ORInvoiceQuery.InvoiceFilter.ORRefNumberFilter.RefNumberRangeFilter.FromRefNumber.SetValue("ab")
                'Set field value for ToRefNumber
                InvoiceQueryRq.ORInvoiceQuery.InvoiceFilter.ORRefNumberFilter.RefNumberRangeFilter.ToRefNumber.SetValue("ab")

            End If
            Dim ORCurrencyFilterElementType12988 As String
            ORCurrencyFilterElementType12988 = "" 'ListIDList

            If (ORCurrencyFilterElementType12988 = "ListIDList") Then

                'Set field value for ListIDList
                'May create more than one of these if needed
                InvoiceQueryRq.ORInvoiceQuery.InvoiceFilter.CurrencyFilter.ORCurrencyFilter.ListIDList.Add("200000-1011023419")

            End If
            If (ORCurrencyFilterElementType12988 = "FullNameList") Then

                'Set field value for FullNameList
                'May create more than one of these if needed
                InvoiceQueryRq.ORInvoiceQuery.InvoiceFilter.CurrencyFilter.ORCurrencyFilter.FullNameList.Add("ab")

            End If
            'Set field value for PaidStatus
            InvoiceQueryRq.ORInvoiceQuery.InvoiceFilter.PaidStatus.SetValue(ENPaidStatus.psAll)
        End If
        'Set field value for IncludeLineItems
        InvoiceQueryRq.IncludeLineItems.SetValue(True)
        'Set field value for IncludeLinkedTxns
        InvoiceQueryRq.IncludeLinkedTxns.SetValue(True)
        'Set field value for IncludeRetElementList
        'May create more than one of these if needed
        'InvoiceQueryRq.IncludeRetElementList.Add("ab")
        'Set field value for OwnerIDList
        'May create more than one of these if needed
        'InvoiceQueryRq.OwnerIDList.Add(System.Guid.NewGuid().ToString())

    End Sub

    Private Sub DoCustomerQuery()

        Dim sessionBegun As Boolean
        sessionBegun = False
        Dim connectionOpen As Boolean
        connectionOpen = False
        Dim sessionManager As QBSessionManager
        sessionManager = Nothing

        Dim tmpXMLMajorVer As Short = 0
        Dim tmpXMLMinorVer As Short = 0

        tmpXMLMinorVer = Var_MaxVer
        tmpXMLMajorVer = Var_MinVer


        Try

            'Create the session Manager object
            sessionManager = New QBSessionManager

            'Create the message set request object to hold our request
            Dim requestMsgSet As IMsgSetRequest
            requestMsgSet = sessionManager.CreateMsgSetRequest("US", tmpXMLMajorVer, tmpXMLMinorVer)
            requestMsgSet.Attributes.OnError = ENRqOnError.roeContinue


            BuildCustomerQueryRq(requestMsgSet)

            'Connect to QuickBooks and begin a session
            sessionManager.OpenConnection(Var_AppID, Var_AppName)
            connectionOpen = True
            sessionManager.BeginSession("", ENOpenMode.omDontCare)
            sessionBegun = True


            'Send the request and get the response from QuickBooks
            Dim responseMsgSet As IMsgSetResponse
            responseMsgSet = sessionManager.DoRequests(requestMsgSet)

            My.Application.Log.WriteEntry("Hecho - DoRequests", TraceEventType.Information)

            'End the session and close the connection to QuickBooks
            sessionManager.EndSession()
            sessionBegun = False
            sessionManager.CloseConnection()
            connectionOpen = False

            WalkCustomerQueryRs(responseMsgSet)

            responseMsgSet.ToXMLString()

        Catch e As Exception

            My.Application.Log.WriteEntry(e.Message, TraceEventType.Error)
            If (sessionBegun) Then

                sessionManager.EndSession()

            End If
            If (connectionOpen) Then

                sessionManager.CloseConnection()

            End If
        End Try
    End Sub

    Private Sub BuildCustomerQueryRq(ByVal requestMsgSet As IMsgSetRequest)

        Dim CustomerQueryRq As ICustomerQuery
        CustomerQueryRq = requestMsgSet.AppendCustomerQueryRq()
        Dim ORCustomerListQueryElementType8308 As String
        ORCustomerListQueryElementType8308 = "CustomerListFilter"

        If (ORCustomerListQueryElementType8308 = "ListIDList") Then

            'Set field value for ListIDList
            'May create more than one of these if needed
            CustomerQueryRq.ORCustomerListQuery.ListIDList.Add("200000-1011023419")

        End If
        If (ORCustomerListQueryElementType8308 = "FullNameList") Then

            'Set field value for FullNameList
            'May create more than one of these if needed
            CustomerQueryRq.ORCustomerListQuery.FullNameList.Add("ab")

        End If
        If (ORCustomerListQueryElementType8308 = "CustomerListFilter") Then

            'Set field value for MaxReturned
            'CustomerQueryRq.ORCustomerListQuery.CustomerListFilter.MaxReturned.SetValue(6)
            'Set field value for ActiveStatus
            'CustomerQueryRq.ORCustomerListQuery.CustomerListFilter.ActiveStatus.SetValue(ENActiveStatus.asActiveOnly)
            ''Set field value for FromModifiedDate
            'CustomerQueryRq.ORCustomerListQuery.CustomerListFilter.FromModifiedDate.SetValue(DateTime.Parse("12/15/2007 12:15:12"), False)
            ''Set field value for ToModifiedDate
            'CustomerQueryRq.ORCustomerListQuery.CustomerListFilter.ToModifiedDate.SetValue(DateTime.Parse("12/15/2007 12:15:12"), False)
            Dim ORNameFilterElementType8309 As String
            ORNameFilterElementType8309 = "NameFilter"
            If (ORNameFilterElementType8309 = "NameFilter") Then

                'Set field value for MatchCriterion
                CustomerQueryRq.ORCustomerListQuery.CustomerListFilter.ORNameFilter.NameFilter.MatchCriterion.SetValue(ENMatchCriterion.mcStartsWith)
                'Set field value for Name
                'CustomerQueryRq.ORCustomerListQuery.CustomerListFilter.ORNameFilter.NameFilter.Name.SetValue(Me.txtCustomer.Text) 'temporal

            End If
            If (ORNameFilterElementType8309 = "NameRangeFilter") Then

                'Set field value for FromName
                CustomerQueryRq.ORCustomerListQuery.CustomerListFilter.ORNameFilter.NameRangeFilter.FromName.SetValue("ab")
                'Set field value for ToName
                CustomerQueryRq.ORCustomerListQuery.CustomerListFilter.ORNameFilter.NameRangeFilter.ToName.SetValue("ab")

            End If
            'Set field value for Operator
            'CustomerQueryRq.ORCustomerListQuery.CustomerListFilter.TotalBalanceFilter.Operator.SetValue(ENOperator.oLessThan)
            'Set field value for Amount
            'CustomerQueryRq.ORCustomerListQuery.CustomerListFilter.TotalBalanceFilter.Amount.SetValue(10.01)
            Dim ORCurrencyFilterElementType8310 As String
            ORCurrencyFilterElementType8310 = ""
            If (ORCurrencyFilterElementType8310 = "ListIDList") Then

                'Set field value for ListIDList
                'May create more than one of these if needed
                CustomerQueryRq.ORCustomerListQuery.CustomerListFilter.CurrencyFilter.ORCurrencyFilter.ListIDList.Add("200000-1011023419")


            End If
            If (ORCurrencyFilterElementType8310 = "FullNameList") Then

                'Set field value for FullNameList
                'May create more than one of these if needed
                CustomerQueryRq.ORCustomerListQuery.CustomerListFilter.CurrencyFilter.ORCurrencyFilter.FullNameList.Add("ab")


            End If
            Dim ORClassFilterElementType8311 As String
            ORClassFilterElementType8311 = ""
            If (ORClassFilterElementType8311 = "ListIDList") Then

                'Set field value for ListIDList
                'May create more than one of these if needed
                CustomerQueryRq.ORCustomerListQuery.CustomerListFilter.ClassFilter.ORClassFilter.ListIDList.Add("200000-1011023419")

            End If
            If (ORClassFilterElementType8311 = "FullNameList") Then

                'Set field value for FullNameList
                'May create more than one of these if needed
                CustomerQueryRq.ORCustomerListQuery.CustomerListFilter.ClassFilter.ORClassFilter.FullNameList.Add("ab")

            End If
            If (ORClassFilterElementType8311 = "ListIDWithChildren") Then

                'Set field value for ListIDWithChildren
                CustomerQueryRq.ORCustomerListQuery.CustomerListFilter.ClassFilter.ORClassFilter.ListIDWithChildren.SetValue("200000-1011023419")

            End If
            If (ORClassFilterElementType8311 = "FullNameWithChildren") Then

                'Set field value for FullNameWithChildren
                CustomerQueryRq.ORCustomerListQuery.CustomerListFilter.ClassFilter.ORClassFilter.FullNameWithChildren.SetValue("ab")

            End If
        End If
        'Set field value for IncludeRetElementList
        'May create more than one of these if needed
        ' CustomerQueryRq.IncludeRetElementList.Add("ab")
        'Set field value for OwnerIDList
        'May create more than one of these if needed
        'CustomerQueryRq.OwnerIDList.Add(System.Guid.NewGuid().ToString())


    End Sub


    Private Sub WalkCustomerQueryRs(ByVal responseMsgSet As IMsgSetResponse)

        If (responseMsgSet Is Nothing) Then

            Exit Sub

        End If

        Dim responseList As IResponseList
        responseList = responseMsgSet.ResponseList
        If (responseList Is Nothing) Then

            Exit Sub

        End If

        'if we sent only one request, there is only one response, we'll walk the list for this sample
        For j = 0 To responseList.Count - 1

            Dim response As IResponse
            response = responseList.GetAt(j)
            'check the status code of the response, 0=ok, >0 is warning
            If (response.StatusCode >= 0) Then

                '//the request-specific response is in the details, make sure we have some
                If (Not response.Detail Is Nothing) Then

                    '//make sure the response is the type we're expecting
                    Dim responseType As ENResponseType
                    responseType = CType(response.Type.GetValue(), ENResponseType)
                    If (responseType = ENResponseType.rtCustomerQueryRs) Then

                        '//upcast to more specific type here, this is safe because we checked with response.Type check above
                        Dim CustomerRet As ICustomerRetList
                        CustomerRet = CType(response.Detail, ICustomerRetList)

                        Dim i As Integer = 0
                        For i = 0 To CustomerRet.Count - 1
                            WalkCustomerRet(CustomerRet.GetAt(i))
                        Next


                    End If
                End If
            End If
        Next j
    End Sub



    Private Sub WalkCustomerRet(ByVal CustomerRet As ICustomerRet)
        On Error Resume Next

        If (CustomerRet Is Nothing) Then

            Exit Sub

        End If

        Dim FullName8320 As String = ""
        Dim FirstName8326 As String = ""
        Dim MiddleName8327 As String = ""
        Dim LastName8328 As String = ""
        Dim City8350 As String = ""
        Dim State8351 As String = ""
        Dim Name8316 As String = ""
        Dim City8335 As String = ""
        Dim State8336 As String = ""
        Dim Addr58334 As String = ""
        Dim Note8339 As String = ""
        Dim Addr58344 As String = ""
        Dim Email8376 As String = ""

        'Go through all the elements of ICustomerRetList
        'Get value of ListID
        Dim ListID8312 As String
        ListID8312 = CustomerRet.ListID.GetValue()
        'Get value of TimeCreated
        Dim TimeCreated8313 As DateTime
        TimeCreated8313 = CustomerRet.TimeCreated.GetValue()
        'Get value of TimeModified
        Dim TimeModified8314 As DateTime
        TimeModified8314 = CustomerRet.TimeModified.GetValue()
        'Get value of EditSequence
        Dim EditSequence8315 As String
        EditSequence8315 = CustomerRet.EditSequence.GetValue()
        'Get value of Name

        Name8316 = CustomerRet.Name.GetValue()
        'Get value of FullName
        Dim FullName8317 As String
        FullName8317 = CustomerRet.FullName.GetValue()
        'Get value of IsActive
        If (Not CustomerRet.IsActive Is Nothing) Then

            Dim IsActive8318 As Boolean
            IsActive8318 = CustomerRet.IsActive.GetValue()

        End If
        If (Not CustomerRet.ClassRef Is Nothing) Then

            'Get value of ListID
            If (Not CustomerRet.ClassRef.ListID Is Nothing) Then

                Dim ListID8319 As String
                ListID8319 = CustomerRet.ClassRef.ListID.GetValue()

            End If
            'Get value of FullName
            If (Not CustomerRet.ClassRef.FullName Is Nothing) Then
                FullName8320 = CustomerRet.ClassRef.FullName.GetValue()

            End If
        End If
        If (Not CustomerRet.ParentRef Is Nothing) Then

            'Get value of ListID
            If (Not CustomerRet.ParentRef.ListID Is Nothing) Then

                Dim ListID8321 As String
                ListID8321 = CustomerRet.ParentRef.ListID.GetValue()

            End If
            'Get value of FullName
            If (Not CustomerRet.ParentRef.FullName Is Nothing) Then

                Dim FullName8322 As String
                FullName8322 = CustomerRet.ParentRef.FullName.GetValue()

            End If
        End If
        'Get value of Sublevel
        Dim Sublevel8323 As Integer
        Sublevel8323 = CustomerRet.Sublevel.GetValue()
        'Get value of CompanyName
        If (Not CustomerRet.CompanyName Is Nothing) Then

            Dim CompanyName8324 As String
            CompanyName8324 = CustomerRet.CompanyName.GetValue()

        End If
        'Get value of Salutation
        If (Not CustomerRet.Salutation Is Nothing) Then

            Dim Salutation8325 As String
            Salutation8325 = CustomerRet.Salutation.GetValue()

        End If
        'Get value of FirstName
        If (Not CustomerRet.FirstName Is Nothing) Then
            FirstName8326 = CustomerRet.FirstName.GetValue()

        End If
        'Get value of MiddleName
        If (Not CustomerRet.MiddleName Is Nothing) Then
            MiddleName8327 = CustomerRet.MiddleName.GetValue()

        End If
        'Get value of LastName
        If (Not CustomerRet.LastName Is Nothing) Then
            LastName8328 = CustomerRet.LastName.GetValue()

        End If
        'Get value of JobTitle
        If (Not CustomerRet.JobTitle Is Nothing) Then

            Dim JobTitle8329 As String
            JobTitle8329 = CustomerRet.JobTitle.GetValue()

        End If
        If (Not CustomerRet.BillAddress Is Nothing) Then

            'Get value of Addr1
            If (Not CustomerRet.BillAddress.Addr1 Is Nothing) Then

                Dim Addr18330 As String
                Addr18330 = CustomerRet.BillAddress.Addr1.GetValue()

            End If
            'Get value of Addr2
            If (Not CustomerRet.BillAddress.Addr2 Is Nothing) Then

                Dim Addr28331 As String
                Addr28331 = CustomerRet.BillAddress.Addr2.GetValue()

            End If
            'Get value of Addr3
            If (Not CustomerRet.BillAddress.Addr3 Is Nothing) Then

                Dim Addr38332 As String
                Addr38332 = CustomerRet.BillAddress.Addr3.GetValue()

            End If
            'Get value of Addr4
            If (Not CustomerRet.BillAddress.Addr4 Is Nothing) Then

                Dim Addr48333 As String
                Addr48333 = CustomerRet.BillAddress.Addr4.GetValue()

            End If
            'Get value of Addr5
            If (Not CustomerRet.BillAddress.Addr5 Is Nothing) Then
                Addr58334 = CustomerRet.BillAddress.Addr5.GetValue()

            End If
            'Get value of City
            If (Not CustomerRet.BillAddress.City Is Nothing) Then
                City8335 = CustomerRet.BillAddress.City.GetValue()
            End If
            'Get value of State
            If (Not CustomerRet.BillAddress.State Is Nothing) Then
                State8336 = CustomerRet.BillAddress.State.GetValue()

            End If
            'Get value of PostalCode
            If (Not CustomerRet.BillAddress.PostalCode Is Nothing) Then

                Dim PostalCode8337 As String
                PostalCode8337 = CustomerRet.BillAddress.PostalCode.GetValue()

            End If
            'Get value of Country
            If (Not CustomerRet.BillAddress.Country Is Nothing) Then

                Dim Country8338 As String
                Country8338 = CustomerRet.BillAddress.Country.GetValue()

            End If
            'Get value of Note
            If (Not CustomerRet.BillAddress.Note Is Nothing) Then
                Note8339 = CustomerRet.BillAddress.Note.GetValue()

            End If
        End If
        If (Not CustomerRet.BillAddressBlock Is Nothing) Then

            'Get value of Addr1
            If (Not CustomerRet.BillAddressBlock.Addr1 Is Nothing) Then

                Dim Addr18340 As String
                Addr18340 = CustomerRet.BillAddressBlock.Addr1.GetValue()

            End If
            'Get value of Addr2
            If (Not CustomerRet.BillAddressBlock.Addr2 Is Nothing) Then

                Dim Addr28341 As String
                Addr28341 = CustomerRet.BillAddressBlock.Addr2.GetValue()

            End If
            'Get value of Addr3
            If (Not CustomerRet.BillAddressBlock.Addr3 Is Nothing) Then

                Dim Addr38342 As String
                Addr38342 = CustomerRet.BillAddressBlock.Addr3.GetValue()

            End If
            'Get value of Addr4
            If (Not CustomerRet.BillAddressBlock.Addr4 Is Nothing) Then

                Dim Addr48343 As String
                Addr48343 = CustomerRet.BillAddressBlock.Addr4.GetValue()

            End If
            'Get value of Addr5
            If (Not CustomerRet.BillAddressBlock.Addr5 Is Nothing) Then


                Addr58344 = CustomerRet.BillAddressBlock.Addr5.GetValue()

            End If
        End If
        If (Not CustomerRet.ShipAddress Is Nothing) Then

            'Get value of Addr1
            If (Not CustomerRet.ShipAddress.Addr1 Is Nothing) Then

                Dim Addr18345 As String
                Addr18345 = CustomerRet.ShipAddress.Addr1.GetValue()

            End If
            'Get value of Addr2
            If (Not CustomerRet.ShipAddress.Addr2 Is Nothing) Then

                Dim Addr28346 As String
                Addr28346 = CustomerRet.ShipAddress.Addr2.GetValue()

            End If
            'Get value of Addr3
            If (Not CustomerRet.ShipAddress.Addr3 Is Nothing) Then

                Dim Addr38347 As String
                Addr38347 = CustomerRet.ShipAddress.Addr3.GetValue()

            End If
            'Get value of Addr4
            If (Not CustomerRet.ShipAddress.Addr4 Is Nothing) Then

                Dim Addr48348 As String
                Addr48348 = CustomerRet.ShipAddress.Addr4.GetValue()

            End If
            'Get value of Addr5
            If (Not CustomerRet.ShipAddress.Addr5 Is Nothing) Then

                Dim Addr58349 As String
                Addr58349 = CustomerRet.ShipAddress.Addr5.GetValue()

            End If
            'Get value of City
            If (Not CustomerRet.ShipAddress.City Is Nothing) Then
                City8350 = CustomerRet.ShipAddress.City.GetValue()

            End If
            'Get value of State
            If (Not CustomerRet.ShipAddress.State Is Nothing) Then
                State8351 = CustomerRet.ShipAddress.State.GetValue()

            End If
            'Get value of PostalCode
            If (Not CustomerRet.ShipAddress.PostalCode Is Nothing) Then

                Dim PostalCode8352 As String
                PostalCode8352 = CustomerRet.ShipAddress.PostalCode.GetValue()

            End If
            'Get value of Country
            If (Not CustomerRet.ShipAddress.Country Is Nothing) Then

                Dim Country8353 As String
                Country8353 = CustomerRet.ShipAddress.Country.GetValue()

            End If
            'Get value of Note
            If (Not CustomerRet.ShipAddress.Note Is Nothing) Then

                Dim Note8354 As String
                Note8354 = CustomerRet.ShipAddress.Note.GetValue()

            End If
        End If
        If (Not CustomerRet.ShipAddressBlock Is Nothing) Then

            'Get value of Addr1
            If (Not CustomerRet.ShipAddressBlock.Addr1 Is Nothing) Then

                Dim Addr18355 As String
                Addr18355 = CustomerRet.ShipAddressBlock.Addr1.GetValue()

            End If
            'Get value of Addr2
            If (Not CustomerRet.ShipAddressBlock.Addr2 Is Nothing) Then

                Dim Addr28356 As String
                Addr28356 = CustomerRet.ShipAddressBlock.Addr2.GetValue()

            End If
            'Get value of Addr3
            If (Not CustomerRet.ShipAddressBlock.Addr3 Is Nothing) Then

                Dim Addr38357 As String
                Addr38357 = CustomerRet.ShipAddressBlock.Addr3.GetValue()

            End If
            'Get value of Addr4
            If (Not CustomerRet.ShipAddressBlock.Addr4 Is Nothing) Then

                Dim Addr48358 As String
                Addr48358 = CustomerRet.ShipAddressBlock.Addr4.GetValue()

            End If
            'Get value of Addr5
            If (Not CustomerRet.ShipAddressBlock.Addr5 Is Nothing) Then

                Dim Addr58359 As String
                Addr58359 = CustomerRet.ShipAddressBlock.Addr5.GetValue()

            End If
        End If
        If (Not CustomerRet.ShipToAddressList Is Nothing) Then

            Dim i8360 As Integer
            For i8360 = 0 To CustomerRet.ShipToAddressList.Count - 1

                Dim ShipToAddress As IShipToAddress
                ShipToAddress = CustomerRet.ShipToAddressList.GetAt(i8360)
                'Get value of Name
                Dim Name8361 As String
                Name8361 = ShipToAddress.Name.GetValue()
                'Get value of Addr1
                If (Not ShipToAddress.Addr1 Is Nothing) Then

                    Dim Addr18362 As String
                    Addr18362 = ShipToAddress.Addr1.GetValue()

                End If
                'Get value of Addr2
                If (Not ShipToAddress.Addr2 Is Nothing) Then

                    Dim Addr28363 As String
                    Addr28363 = ShipToAddress.Addr2.GetValue()

                End If
                'Get value of Addr3
                If (Not ShipToAddress.Addr3 Is Nothing) Then

                    Dim Addr38364 As String
                    Addr38364 = ShipToAddress.Addr3.GetValue()

                End If
                'Get value of Addr4
                If (Not ShipToAddress.Addr4 Is Nothing) Then

                    Dim Addr48365 As String
                    Addr48365 = ShipToAddress.Addr4.GetValue()

                End If
                'Get value of Addr5
                If (Not ShipToAddress.Addr5 Is Nothing) Then

                    Dim Addr58366 As String
                    Addr58366 = ShipToAddress.Addr5.GetValue()

                End If
                'Get value of City
                If (Not ShipToAddress.City Is Nothing) Then

                    Dim City8367 As String
                    City8367 = ShipToAddress.City.GetValue()

                End If
                'Get value of State
                If (Not ShipToAddress.State Is Nothing) Then

                    Dim State8368 As String
                    State8368 = ShipToAddress.State.GetValue()

                End If
                'Get value of PostalCode
                If (Not ShipToAddress.PostalCode Is Nothing) Then

                    Dim PostalCode8369 As String
                    PostalCode8369 = ShipToAddress.PostalCode.GetValue()

                End If
                'Get value of Country
                If (Not ShipToAddress.Country Is Nothing) Then

                    Dim Country8370 As String
                    Country8370 = ShipToAddress.Country.GetValue()

                End If
                'Get value of Note
                If (Not ShipToAddress.Note Is Nothing) Then

                    Dim Note8371 As String
                    Note8371 = ShipToAddress.Note.GetValue()

                End If
                'Get value of DefaultShipTo
                If (Not ShipToAddress.DefaultShipTo Is Nothing) Then

                    Dim DefaultShipTo8372 As Boolean
                    DefaultShipTo8372 = ShipToAddress.DefaultShipTo.GetValue()

                End If
            Next i8360
        End If
        'Get value of Phone
        If (Not CustomerRet.Phone Is Nothing) Then

            Dim Phone8373 As String
            Phone8373 = CustomerRet.Phone.GetValue()

        End If
        'Get value of AltPhone
        If (Not CustomerRet.AltPhone Is Nothing) Then

            Dim AltPhone8374 As String
            AltPhone8374 = CustomerRet.AltPhone.GetValue()

        End If
        'Get value of Fax
        If (Not CustomerRet.Fax Is Nothing) Then

            Dim Fax8375 As String
            Fax8375 = CustomerRet.Fax.GetValue()

        End If
        'Get value of Email
        If (Not CustomerRet.Email Is Nothing) Then
            Email8376 = CustomerRet.Email.GetValue()

        End If
        'Get value of Cc
        If (Not CustomerRet.Cc Is Nothing) Then

            Dim Cc8377 As String
            Cc8377 = CustomerRet.Cc.GetValue()

        End If
        'Get value of Contact
        If (Not CustomerRet.Contact Is Nothing) Then

            Dim Contact8378 As String
            Contact8378 = CustomerRet.Contact.GetValue()

        End If
        'Get value of AltContact
        If (Not CustomerRet.AltContact Is Nothing) Then

            Dim AltContact8379 As String
            AltContact8379 = CustomerRet.AltContact.GetValue()

        End If
        If (Not CustomerRet.AdditionalContactRefList Is Nothing) Then

            Dim i8380 As Integer
            For i8380 = 0 To CustomerRet.AdditionalContactRefList.Count - 1

                Dim QBBaseRef As IQBBaseRef
                QBBaseRef = CustomerRet.AdditionalContactRefList.GetAt(i8380)
                'Get value of ContactName
                Dim ContactName8381 As String
                ContactName8381 = QBBaseRef.ContactName.GetValue()
                'Get value of ContactValue
                Dim ContactValue8382 As String
                ContactValue8382 = QBBaseRef.ContactValue.GetValue()

            Next i8380
        End If
        If (Not CustomerRet.ContactsRetList Is Nothing) Then

            Dim i8383 As Integer
            For i8383 = 0 To CustomerRet.ContactsRetList.Count - 1

                Dim ContactsRet As IContactsRet
                ContactsRet = CustomerRet.ContactsRetList.GetAt(i8383)
                'Get value of ListID
                Dim ListID8384 As String
                ListID8384 = ContactsRet.ListID.GetValue()
                'Get value of TimeCreated
                Dim TimeCreated8385 As DateTime
                TimeCreated8385 = ContactsRet.TimeCreated.GetValue()
                'Get value of TimeModified
                Dim TimeModified8386 As DateTime
                TimeModified8386 = ContactsRet.TimeModified.GetValue()
                'Get value of EditSequence
                Dim EditSequence8387 As String
                EditSequence8387 = ContactsRet.EditSequence.GetValue()
                'Get value of Contact
                If (Not ContactsRet.Contact Is Nothing) Then

                    Dim Contact8388 As String
                    Contact8388 = ContactsRet.Contact.GetValue()

                End If
                'Get value of Salutation
                If (Not ContactsRet.Salutation Is Nothing) Then

                    Dim Salutation8389 As String
                    Salutation8389 = ContactsRet.Salutation.GetValue()

                End If
                'Get value of FirstName
                Dim FirstName8390 As String
                FirstName8390 = ContactsRet.FirstName.GetValue()
                'Get value of MiddleName
                If (Not ContactsRet.MiddleName Is Nothing) Then

                    Dim MiddleName8391 As String
                    MiddleName8391 = ContactsRet.MiddleName.GetValue()

                End If
                'Get value of LastName
                If (Not ContactsRet.LastName Is Nothing) Then

                    Dim LastName8392 As String
                    LastName8392 = ContactsRet.LastName.GetValue()

                End If
                'Get value of JobTitle
                If (Not ContactsRet.JobTitle Is Nothing) Then

                    Dim JobTitle8393 As String
                    JobTitle8393 = ContactsRet.JobTitle.GetValue()

                End If
                If (Not ContactsRet.AdditionalContactRefList Is Nothing) Then

                    Dim i8394 As Integer
                    For i8394 = 0 To ContactsRet.AdditionalContactRefList.Count - 1

                        Dim QBBaseRef As IQBBaseRef
                        QBBaseRef = ContactsRet.AdditionalContactRefList.GetAt(i8394)
                        'Get value of ContactName
                        Dim ContactName8395 As String
                        ContactName8395 = QBBaseRef.ContactName.GetValue()
                        'Get value of ContactValue
                        Dim ContactValue8396 As String
                        ContactValue8396 = QBBaseRef.ContactValue.GetValue()

                    Next i8394
                End If
            Next i8383

        End If
        If (Not CustomerRet.CustomerTypeRef Is Nothing) Then

            'Get value of ListID
            If (Not CustomerRet.CustomerTypeRef.ListID Is Nothing) Then

                Dim ListID8397 As String
                ListID8397 = CustomerRet.CustomerTypeRef.ListID.GetValue()

            End If
            'Get value of FullName
            If (Not CustomerRet.CustomerTypeRef.FullName Is Nothing) Then

                Dim FullName8398 As String
                FullName8398 = CustomerRet.CustomerTypeRef.FullName.GetValue()

            End If
        End If
        If (Not CustomerRet.TermsRef Is Nothing) Then

            'Get value of ListID
            If (Not CustomerRet.TermsRef.ListID Is Nothing) Then

                Dim ListID8399 As String
                ListID8399 = CustomerRet.TermsRef.ListID.GetValue()

            End If
            'Get value of FullName
            If (Not CustomerRet.TermsRef.FullName Is Nothing) Then

                Dim FullName8400 As String
                FullName8400 = CustomerRet.TermsRef.FullName.GetValue()

            End If
        End If
        If (Not CustomerRet.SalesRepRef Is Nothing) Then

            'Get value of ListID
            If (Not CustomerRet.SalesRepRef.ListID Is Nothing) Then

                Dim ListID8401 As String
                ListID8401 = CustomerRet.SalesRepRef.ListID.GetValue()

            End If
            'Get value of FullName
            If (Not CustomerRet.SalesRepRef.FullName Is Nothing) Then

                Dim FullName8402 As String
                FullName8402 = CustomerRet.SalesRepRef.FullName.GetValue()

            End If
        End If
        'Get value of Balance
        If (Not CustomerRet.Balance Is Nothing) Then

            Dim Balance8403 As Double
            Balance8403 = CustomerRet.Balance.GetValue()

        End If
        'Get value of TotalBalance
        If (Not CustomerRet.TotalBalance Is Nothing) Then

            Dim TotalBalance8404 As Double
            TotalBalance8404 = CustomerRet.TotalBalance.GetValue()

        End If
        If (Not CustomerRet.SalesTaxCodeRef Is Nothing) Then

            'Get value of ListID
            If (Not CustomerRet.SalesTaxCodeRef.ListID Is Nothing) Then

                Dim ListID8405 As String
                ListID8405 = CustomerRet.SalesTaxCodeRef.ListID.GetValue()

            End If
            'Get value of FullName
            If (Not CustomerRet.SalesTaxCodeRef.FullName Is Nothing) Then

                Dim FullName8406 As String
                FullName8406 = CustomerRet.SalesTaxCodeRef.FullName.GetValue()

            End If
        End If
        If (Not CustomerRet.ItemSalesTaxRef Is Nothing) Then

            'Get value of ListID
            If (Not CustomerRet.ItemSalesTaxRef.ListID Is Nothing) Then

                Dim ListID8407 As String
                ListID8407 = CustomerRet.ItemSalesTaxRef.ListID.GetValue()

            End If
            'Get value of FullName
            If (Not CustomerRet.ItemSalesTaxRef.FullName Is Nothing) Then

                Dim FullName8408 As String
                FullName8408 = CustomerRet.ItemSalesTaxRef.FullName.GetValue()

            End If
        End If
        'Get value of ResaleNumber
        If (Not CustomerRet.ResaleNumber Is Nothing) Then

            Dim ResaleNumber8409 As String
            ResaleNumber8409 = CustomerRet.ResaleNumber.GetValue()

        End If
        'Get value of AccountNumber
        If (Not CustomerRet.AccountNumber Is Nothing) Then

            Dim AccountNumber8410 As String
            AccountNumber8410 = CustomerRet.AccountNumber.GetValue()

        End If
        'Get value of CreditLimit
        If (Not CustomerRet.CreditLimit Is Nothing) Then

            Dim CreditLimit8411 As Double
            CreditLimit8411 = CustomerRet.CreditLimit.GetValue()

        End If
        If (Not CustomerRet.PreferredPaymentMethodRef Is Nothing) Then

            'Get value of ListID
            If (Not CustomerRet.PreferredPaymentMethodRef.ListID Is Nothing) Then

                Dim ListID8412 As String
                ListID8412 = CustomerRet.PreferredPaymentMethodRef.ListID.GetValue()

            End If
            'Get value of FullName
            If (Not CustomerRet.PreferredPaymentMethodRef.FullName Is Nothing) Then

                Dim FullName8413 As String
                FullName8413 = CustomerRet.PreferredPaymentMethodRef.FullName.GetValue()

            End If
        End If
        If (Not CustomerRet.CreditCardInfo Is Nothing) Then

            'Get value of CreditCardNumber
            If (Not CustomerRet.CreditCardInfo.CreditCardNumber Is Nothing) Then

                Dim CreditCardNumber8414 As String
                CreditCardNumber8414 = CustomerRet.CreditCardInfo.CreditCardNumber.GetValue()

            End If
            'Get value of ExpirationMonth
            If (Not CustomerRet.CreditCardInfo.ExpirationMonth Is Nothing) Then

                Dim ExpirationMonth8415 As Integer
                ExpirationMonth8415 = CustomerRet.CreditCardInfo.ExpirationMonth.GetValue()

            End If
            'Get value of ExpirationYear
            If (Not CustomerRet.CreditCardInfo.ExpirationYear Is Nothing) Then

                Dim ExpirationYear8416 As Integer
                ExpirationYear8416 = CustomerRet.CreditCardInfo.ExpirationYear.GetValue()

            End If
            'Get value of NameOnCard
            If (Not CustomerRet.CreditCardInfo.NameOnCard Is Nothing) Then

                Dim NameOnCard8417 As String
                NameOnCard8417 = CustomerRet.CreditCardInfo.NameOnCard.GetValue()

            End If
            'Get value of CreditCardAddress
            If (Not CustomerRet.CreditCardInfo.CreditCardAddress Is Nothing) Then

                Dim CreditCardAddress8418 As String
                CreditCardAddress8418 = CustomerRet.CreditCardInfo.CreditCardAddress.GetValue()

            End If
            'Get value of CreditCardPostalCode
            If (Not CustomerRet.CreditCardInfo.CreditCardPostalCode Is Nothing) Then

                Dim CreditCardPostalCode8419 As String
                CreditCardPostalCode8419 = CustomerRet.CreditCardInfo.CreditCardPostalCode.GetValue()

            End If
        End If
        'Get value of JobStatus
        If (Not CustomerRet.JobStatus Is Nothing) Then

            Dim JobStatus8420 As ENJobStatus
            JobStatus8420 = CustomerRet.JobStatus.GetValue()

        End If
        'Get value of JobStartDate
        If (Not CustomerRet.JobStartDate Is Nothing) Then

            Dim JobStartDate8421 As DateTime
            JobStartDate8421 = CustomerRet.JobStartDate.GetValue()

        End If
        'Get value of JobProjectedEndDate
        If (Not CustomerRet.JobProjectedEndDate Is Nothing) Then

            Dim JobProjectedEndDate8422 As DateTime
            JobProjectedEndDate8422 = CustomerRet.JobProjectedEndDate.GetValue()

        End If
        'Get value of JobEndDate
        If (Not CustomerRet.JobEndDate Is Nothing) Then

            Dim JobEndDate8423 As DateTime
            JobEndDate8423 = CustomerRet.JobEndDate.GetValue()

        End If
        'Get value of JobDesc
        If (Not CustomerRet.JobDesc Is Nothing) Then

            Dim JobDesc8424 As String
            JobDesc8424 = CustomerRet.JobDesc.GetValue()

        End If
        If (Not CustomerRet.JobTypeRef Is Nothing) Then

            'Get value of ListID
            If (Not CustomerRet.JobTypeRef.ListID Is Nothing) Then

                Dim ListID8425 As String
                ListID8425 = CustomerRet.JobTypeRef.ListID.GetValue()

            End If
            'Get value of FullName
            If (Not CustomerRet.JobTypeRef.FullName Is Nothing) Then

                Dim FullName8426 As String
                FullName8426 = CustomerRet.JobTypeRef.FullName.GetValue()

            End If
        End If
        'Get value of Notes
        If (Not CustomerRet.Notes Is Nothing) Then

            Dim Notes8427 As String
            Notes8427 = CustomerRet.Notes.GetValue()

        End If
        If (Not CustomerRet.AdditionalNotesRetList Is Nothing) Then

            Dim i8428 As Integer
            For i8428 = 0 To CustomerRet.AdditionalNotesRetList.Count - 1

                Dim AdditionalNotesRet As IAdditionalNotesRet
                AdditionalNotesRet = CustomerRet.AdditionalNotesRetList.GetAt(i8428)
                'Get value of NoteID
                Dim NoteID8429 As Integer
                NoteID8429 = AdditionalNotesRet.NoteID.GetValue()
                'Get value of Date
                Dim Date8430 As DateTime
                Date8430 = AdditionalNotesRet.Date.GetValue()
                'Get value of Note
                Dim Note8431 As String
                Note8431 = AdditionalNotesRet.Note.GetValue()

            Next i8428
        End If
        'Get value of PreferredDeliveryMethod
        If (Not CustomerRet.PreferredDeliveryMethod Is Nothing) Then

            Dim PreferredDeliveryMethod8432 As ENPreferredDeliveryMethod
            PreferredDeliveryMethod8432 = CustomerRet.PreferredDeliveryMethod.GetValue()

        End If
        If (Not CustomerRet.PriceLevelRef Is Nothing) Then

            'Get value of ListID
            If (Not CustomerRet.PriceLevelRef.ListID Is Nothing) Then

                Dim ListID8433 As String
                ListID8433 = CustomerRet.PriceLevelRef.ListID.GetValue()

            End If
            'Get value of FullName
            If (Not CustomerRet.PriceLevelRef.FullName Is Nothing) Then

                Dim FullName8434 As String
                FullName8434 = CustomerRet.PriceLevelRef.FullName.GetValue()

            End If
        End If
        'Get value of ExternalGUID
        If (Not CustomerRet.ExternalGUID Is Nothing) Then

            Dim ExternalGUID8435 As String
            ExternalGUID8435 = CustomerRet.ExternalGUID.GetValue()

        End If
        If (Not CustomerRet.CurrencyRef Is Nothing) Then

            'Get value of ListID
            If (Not CustomerRet.CurrencyRef.ListID Is Nothing) Then

                Dim ListID8436 As String
                ListID8436 = CustomerRet.CurrencyRef.ListID.GetValue()

            End If
            'Get value of FullName
            If (Not CustomerRet.CurrencyRef.FullName Is Nothing) Then

                Dim FullName8437 As String
                FullName8437 = CustomerRet.CurrencyRef.FullName.GetValue()

            End If
        End If
        If (Not CustomerRet.DataExtRetList Is Nothing) Then

            Dim i8438 As Integer
            For i8438 = 0 To CustomerRet.DataExtRetList.Count - 1

                Dim DataExtRet As IDataExtRet
                DataExtRet = CustomerRet.DataExtRetList.GetAt(i8438)
                'Get value of OwnerID
                If (Not DataExtRet.OwnerID Is Nothing) Then

                    Dim OwnerID8439 As String
                    OwnerID8439 = DataExtRet.OwnerID.GetValue()

                End If
                'Get value of DataExtName
                Dim DataExtName8440 As String
                DataExtName8440 = DataExtRet.DataExtName.GetValue()
                'Get value of DataExtType
                Dim DataExtType8441 As ENDataExtType
                DataExtType8441 = DataExtRet.DataExtType.GetValue()
                'Get value of DataExtValue
                Dim DataExtValue8442 As String
                DataExtValue8442 = DataExtRet.DataExtValue.GetValue()
            Next i8438
        End If

        Dim tmpNuevaFila As dsQBCustomers.QBCustomersRow
        tmpNuevaFila = Me.Var_dsQBCustomers.QBCustomers.NewQBCustomersRow
        With tmpNuevaFila
            .City8350 = City8335
            .FirstName8326 = FirstName8326
            .FullName8320 = Name8316
            .LastName8328 = LastName8328
            .MiddleName8327 = MiddleName8327
            .State8351 = State8336
            .Address5 = Addr58344
            .Notes = Note8339
            .EMail = Email8376

        End With
        Me.Var_dsQBCustomers.QBCustomers.AddQBCustomersRow(tmpNuevaFila)


    End Sub


End Class
