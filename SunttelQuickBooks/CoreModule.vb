Imports System.Data.SqlClient


Module CoreModule

    Public strConeccionDB As String = ""


    Public Sub LlenaDataSetGenerico(ByVal prmDataTable As DataTable, ByVal prmTablaConsulta As String, Optional ByVal prmStrSQL As String = "", Optional ByVal StringConeccion As String = "", Optional ByVal FlagOrderByNombre As Boolean = False)
        Dim cnn As SqlConnection
        Dim Adaptador As Object
        Dim TablaDataSet As String = prmDataTable.TableName
        Dim tempStrConeccion As String
        On Error GoTo ControlaError

        If StringConeccion.Length > 0 Then
            tempStrConeccion = StringConeccion
        Else
            tempStrConeccion = strConeccionDB
        End If


        cnn = New SqlConnection(tempStrConeccion)
        Dim Comando As New SqlCommand

        Adaptador = New SqlDataAdapter(Comando)


        With Adaptador.SelectCommand
            .Connection = cnn
            If prmStrSQL.Length = 0 Then
                If FlagOrderByNombre Then
                    .CommandText = "SELECT  ID, RTRIM(Nombre) AS Nombre  FROM " & prmTablaConsulta & " ORDER BY Nombre ASC"
                Else
                    .CommandText = "SELECT ID, RTRIM(Nombre) AS Nombre FROM " & prmTablaConsulta
                End If

            Else
                .CommandText = prmStrSQL
            End If

        End With
        Adaptador.TableMappings.Add(prmTablaConsulta, TablaDataSet)
        Adaptador.Fill(prmDataTable)
        Exit Sub
ControlaError:
        Dim strError As String = Err.Description


    End Sub

End Module
