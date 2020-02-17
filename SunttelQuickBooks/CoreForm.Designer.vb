<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class CoreForm
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.PictureBox1 = New System.Windows.Forms.PictureBox()
        Me.Panel1 = New System.Windows.Forms.Panel()
        Me.lblLastTimeRan = New System.Windows.Forms.Label()
        Me.lblTiempoRestanteNuevoCargue = New System.Windows.Forms.Label()
        Me.lblStatus = New System.Windows.Forms.Label()
        Me.btnGetInvoices = New System.Windows.Forms.Button()
        Me.txtCustomer = New System.Windows.Forms.TextBox()
        Me.Label8 = New System.Windows.Forms.Label()
        Me.lblNumRegsAR = New System.Windows.Forms.Label()
        Me.ButtonPayments = New System.Windows.Forms.Button()
        Me.dtpHasta = New System.Windows.Forms.DateTimePicker()
        Me.Label7 = New System.Windows.Forms.Label()
        Me.dtpDesde = New System.Windows.Forms.DateTimePicker()
        Me.Label6 = New System.Windows.Forms.Label()
        Me.lblNumRecs = New System.Windows.Forms.Label()
        Me.nudMaxVer = New System.Windows.Forms.NumericUpDown()
        Me.Label5 = New System.Windows.Forms.Label()
        Me.nudMinVer = New System.Windows.Forms.NumericUpDown()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.btnGetData = New System.Windows.Forms.Button()
        Me.txtAppName = New System.Windows.Forms.TextBox()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.txtAppID = New System.Windows.Forms.TextBox()
        Me.Label9 = New System.Windows.Forms.Label()
        CType(Me.PictureBox1, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.Panel1.SuspendLayout()
        CType(Me.nudMaxVer, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.nudMinVer, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'Label1
        '
        Me.Label1.BackColor = System.Drawing.Color.MidnightBlue
        Me.Label1.Dock = System.Windows.Forms.DockStyle.Top
        Me.Label1.Font = New System.Drawing.Font("Microsoft Sans Serif", 12.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label1.ForeColor = System.Drawing.Color.White
        Me.Label1.Location = New System.Drawing.Point(0, 0)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(936, 26)
        Me.Label1.TabIndex = 7
        Me.Label1.Text = "QuickBooks Interface"
        Me.Label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'PictureBox1
        '
        Me.PictureBox1.BackColor = System.Drawing.Color.White
        Me.PictureBox1.BackgroundImage = Global.SunttelQuickBooks.My.Resources.Resources.LogoSmartSoft
        Me.PictureBox1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom
        Me.PictureBox1.Dock = System.Windows.Forms.DockStyle.Top
        Me.PictureBox1.Location = New System.Drawing.Point(0, 26)
        Me.PictureBox1.Name = "PictureBox1"
        Me.PictureBox1.Size = New System.Drawing.Size(936, 86)
        Me.PictureBox1.TabIndex = 8
        Me.PictureBox1.TabStop = False
        '
        'Panel1
        '
        Me.Panel1.BackColor = System.Drawing.Color.Silver
        Me.Panel1.Controls.Add(Me.lblLastTimeRan)
        Me.Panel1.Controls.Add(Me.lblTiempoRestanteNuevoCargue)
        Me.Panel1.Controls.Add(Me.lblStatus)
        Me.Panel1.Controls.Add(Me.btnGetInvoices)
        Me.Panel1.Controls.Add(Me.txtCustomer)
        Me.Panel1.Controls.Add(Me.Label8)
        Me.Panel1.Controls.Add(Me.lblNumRegsAR)
        Me.Panel1.Controls.Add(Me.ButtonPayments)
        Me.Panel1.Controls.Add(Me.dtpHasta)
        Me.Panel1.Controls.Add(Me.Label7)
        Me.Panel1.Controls.Add(Me.dtpDesde)
        Me.Panel1.Controls.Add(Me.Label6)
        Me.Panel1.Controls.Add(Me.lblNumRecs)
        Me.Panel1.Controls.Add(Me.nudMaxVer)
        Me.Panel1.Controls.Add(Me.Label5)
        Me.Panel1.Controls.Add(Me.nudMinVer)
        Me.Panel1.Controls.Add(Me.Label4)
        Me.Panel1.Controls.Add(Me.btnGetData)
        Me.Panel1.Controls.Add(Me.txtAppName)
        Me.Panel1.Controls.Add(Me.Label2)
        Me.Panel1.Controls.Add(Me.txtAppID)
        Me.Panel1.Controls.Add(Me.Label9)
        Me.Panel1.Dock = System.Windows.Forms.DockStyle.Top
        Me.Panel1.Location = New System.Drawing.Point(0, 112)
        Me.Panel1.Name = "Panel1"
        Me.Panel1.Size = New System.Drawing.Size(936, 150)
        Me.Panel1.TabIndex = 9
        '
        'lblLastTimeRan
        '
        Me.lblLastTimeRan.AutoSize = True
        Me.lblLastTimeRan.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblLastTimeRan.Location = New System.Drawing.Point(506, 120)
        Me.lblLastTimeRan.Name = "lblLastTimeRan"
        Me.lblLastTimeRan.Size = New System.Drawing.Size(93, 13)
        Me.lblLastTimeRan.TabIndex = 54
        Me.lblLastTimeRan.Text = "Last Time Ran:"
        '
        'lblTiempoRestanteNuevoCargue
        '
        Me.lblTiempoRestanteNuevoCargue.AutoSize = True
        Me.lblTiempoRestanteNuevoCargue.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblTiempoRestanteNuevoCargue.Location = New System.Drawing.Point(189, 120)
        Me.lblTiempoRestanteNuevoCargue.Name = "lblTiempoRestanteNuevoCargue"
        Me.lblTiempoRestanteNuevoCargue.Size = New System.Drawing.Size(236, 13)
        Me.lblTiempoRestanteNuevoCargue.TabIndex = 53
        Me.lblTiempoRestanteNuevoCargue.Text = "Remaining Time for Next Upload (Mins): "
        '
        'lblStatus
        '
        Me.lblStatus.AutoSize = True
        Me.lblStatus.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblStatus.Location = New System.Drawing.Point(18, 120)
        Me.lblStatus.Name = "lblStatus"
        Me.lblStatus.Size = New System.Drawing.Size(91, 13)
        Me.lblStatus.TabIndex = 52
        Me.lblStatus.Text = "Status: Stoped"
        '
        'btnGetInvoices
        '
        Me.btnGetInvoices.Location = New System.Drawing.Point(806, 35)
        Me.btnGetInvoices.Name = "btnGetInvoices"
        Me.btnGetInvoices.Size = New System.Drawing.Size(118, 23)
        Me.btnGetInvoices.TabIndex = 46
        Me.btnGetInvoices.Text = "Get Data Invoices"
        Me.btnGetInvoices.UseVisualStyleBackColor = True
        '
        'txtCustomer
        '
        Me.txtCustomer.Location = New System.Drawing.Point(77, 85)
        Me.txtCustomer.Name = "txtCustomer"
        Me.txtCustomer.Size = New System.Drawing.Size(217, 20)
        Me.txtCustomer.TabIndex = 45
        Me.txtCustomer.Text = "13MOD/FOR5512 VICTOR R LUGO"
        '
        'Label8
        '
        Me.Label8.AutoSize = True
        Me.Label8.Location = New System.Drawing.Point(20, 88)
        Me.Label8.Name = "Label8"
        Me.Label8.Size = New System.Drawing.Size(51, 13)
        Me.Label8.TabIndex = 44
        Me.Label8.Text = "Customer"
        '
        'lblNumRegsAR
        '
        Me.lblNumRegsAR.AutoSize = True
        Me.lblNumRegsAR.Location = New System.Drawing.Point(549, 63)
        Me.lblNumRegsAR.Name = "lblNumRegsAR"
        Me.lblNumRegsAR.Size = New System.Drawing.Size(59, 13)
        Me.lblNumRegsAR.TabIndex = 43
        Me.lblNumRegsAR.Text = "Records: 0"
        '
        'ButtonPayments
        '
        Me.ButtonPayments.Location = New System.Drawing.Point(806, 6)
        Me.ButtonPayments.Name = "ButtonPayments"
        Me.ButtonPayments.Size = New System.Drawing.Size(118, 23)
        Me.ButtonPayments.TabIndex = 42
        Me.ButtonPayments.Text = "Get Data Payments"
        Me.ButtonPayments.UseVisualStyleBackColor = True
        '
        'dtpHasta
        '
        Me.dtpHasta.Location = New System.Drawing.Point(326, 59)
        Me.dtpHasta.Name = "dtpHasta"
        Me.dtpHasta.Size = New System.Drawing.Size(200, 20)
        Me.dtpHasta.TabIndex = 41
        '
        'Label7
        '
        Me.Label7.AutoSize = True
        Me.Label7.Location = New System.Drawing.Point(280, 63)
        Me.Label7.Name = "Label7"
        Me.Label7.Size = New System.Drawing.Size(35, 13)
        Me.Label7.TabIndex = 40
        Me.Label7.Text = "Hasta"
        '
        'dtpDesde
        '
        Me.dtpDesde.Location = New System.Drawing.Point(65, 59)
        Me.dtpDesde.Name = "dtpDesde"
        Me.dtpDesde.Size = New System.Drawing.Size(200, 20)
        Me.dtpDesde.TabIndex = 39
        '
        'Label6
        '
        Me.Label6.AutoSize = True
        Me.Label6.Location = New System.Drawing.Point(19, 63)
        Me.Label6.Name = "Label6"
        Me.Label6.Size = New System.Drawing.Size(38, 13)
        Me.Label6.TabIndex = 38
        Me.Label6.Text = "Desde"
        '
        'lblNumRecs
        '
        Me.lblNumRecs.AutoSize = True
        Me.lblNumRecs.Location = New System.Drawing.Point(381, 35)
        Me.lblNumRecs.Name = "lblNumRecs"
        Me.lblNumRecs.Size = New System.Drawing.Size(59, 13)
        Me.lblNumRecs.TabIndex = 37
        Me.lblNumRecs.Text = "Records: 0"
        '
        'nudMaxVer
        '
        Me.nudMaxVer.Location = New System.Drawing.Point(282, 33)
        Me.nudMaxVer.Name = "nudMaxVer"
        Me.nudMaxVer.Size = New System.Drawing.Size(74, 20)
        Me.nudMaxVer.TabIndex = 36
        Me.nudMaxVer.Value = New Decimal(New Integer() {13, 0, 0, 0})
        '
        'Label5
        '
        Me.Label5.AutoSize = True
        Me.Label5.Location = New System.Drawing.Point(196, 35)
        Me.Label5.Name = "Label5"
        Me.Label5.Size = New System.Drawing.Size(64, 13)
        Me.Label5.TabIndex = 35
        Me.Label5.Text = "QB Max Ver"
        '
        'nudMinVer
        '
        Me.nudMinVer.Location = New System.Drawing.Point(104, 33)
        Me.nudMinVer.Name = "nudMinVer"
        Me.nudMinVer.Size = New System.Drawing.Size(74, 20)
        Me.nudMinVer.TabIndex = 34
        '
        'Label4
        '
        Me.Label4.AutoSize = True
        Me.Label4.Location = New System.Drawing.Point(18, 35)
        Me.Label4.Name = "Label4"
        Me.Label4.Size = New System.Drawing.Size(70, 13)
        Me.Label4.TabIndex = 33
        Me.Label4.Text = "QB Minor Ver"
        '
        'btnGetData
        '
        Me.btnGetData.Location = New System.Drawing.Point(806, 64)
        Me.btnGetData.Name = "btnGetData"
        Me.btnGetData.Size = New System.Drawing.Size(118, 23)
        Me.btnGetData.TabIndex = 32
        Me.btnGetData.Text = "Get Accounts"
        Me.btnGetData.UseVisualStyleBackColor = True
        '
        'txtAppName
        '
        Me.txtAppName.Location = New System.Drawing.Point(315, 6)
        Me.txtAppName.Name = "txtAppName"
        Me.txtAppName.Size = New System.Drawing.Size(170, 20)
        Me.txtAppName.TabIndex = 29
        Me.txtAppName.Text = "Marrero"
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(252, 9)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(57, 13)
        Me.Label2.TabIndex = 28
        Me.Label2.Text = "App Name"
        '
        'txtAppID
        '
        Me.txtAppID.Location = New System.Drawing.Point(62, 6)
        Me.txtAppID.Name = "txtAppID"
        Me.txtAppID.Size = New System.Drawing.Size(170, 20)
        Me.txtAppID.TabIndex = 27
        '
        'Label9
        '
        Me.Label9.AutoSize = True
        Me.Label9.Location = New System.Drawing.Point(16, 9)
        Me.Label9.Name = "Label9"
        Me.Label9.Size = New System.Drawing.Size(40, 13)
        Me.Label9.TabIndex = 26
        Me.Label9.Text = "App ID"
        '
        'CoreForm
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(936, 468)
        Me.Controls.Add(Me.Panel1)
        Me.Controls.Add(Me.PictureBox1)
        Me.Controls.Add(Me.Label1)
        Me.Name = "CoreForm"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "QuickBooks Interface"
        CType(Me.PictureBox1, System.ComponentModel.ISupportInitialize).EndInit()
        Me.Panel1.ResumeLayout(False)
        Me.Panel1.PerformLayout()
        CType(Me.nudMaxVer, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.nudMinVer, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents Label1 As Label
    Friend WithEvents PictureBox1 As PictureBox
    Friend WithEvents Panel1 As Panel
    Friend WithEvents lblLastTimeRan As Label
    Friend WithEvents lblTiempoRestanteNuevoCargue As Label
    Friend WithEvents lblStatus As Label
    Friend WithEvents btnGetInvoices As Button
    Friend WithEvents txtCustomer As TextBox
    Friend WithEvents Label8 As Label
    Friend WithEvents lblNumRegsAR As Label
    Friend WithEvents ButtonPayments As Button
    Friend WithEvents dtpHasta As DateTimePicker
    Friend WithEvents Label7 As Label
    Friend WithEvents dtpDesde As DateTimePicker
    Friend WithEvents Label6 As Label
    Friend WithEvents lblNumRecs As Label
    Friend WithEvents nudMaxVer As NumericUpDown
    Friend WithEvents Label5 As Label
    Friend WithEvents nudMinVer As NumericUpDown
    Friend WithEvents Label4 As Label
    Friend WithEvents btnGetData As Button
    Friend WithEvents txtAppName As TextBox
    Friend WithEvents Label2 As Label
    Friend WithEvents txtAppID As TextBox
    Friend WithEvents Label9 As Label
End Class
