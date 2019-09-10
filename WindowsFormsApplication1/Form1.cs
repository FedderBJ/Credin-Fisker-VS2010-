using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MOXA_CSharp_MXIO;
using System.Threading;
using System.Management;
using System.ServiceProcess;
using System.Security.Permissions;
using System.Security.Principal;


namespace WindowsFormsApplication1
{
    /* Objekter der skal bruges i Navision
     * 
     * Tabel 95103, til opsætning af stamdata. Felter tilføjes
     * Tabel 52010, til Pallekøen. 5 indløb, 1 vikler, 1 printer
     * Form  50173, Visualisering af Kø-data.
     * Codeu 50058 og 50057. Funktions komponent og programkomponent.
     * 
     * Programflow Indløb
     * 
     * SEND DO{1} når data er scannet og tilføj pallen i køen.
     * MODTAG DI{1}. Der kommer en "Gennemløbs palle". Kun wrappes.
     * 
     * Programflow udløb
     * 
     * MODTAG DI{2} Klar til at sende label til printer. Send data til printer og
     * SEND DO{2} Label er sendt til printer.
     * MODTAG {3} Label er påsat og pallen er færdig.
     * 
     */ 

    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        public const UInt16 DI_DIRECTION_DI_MODE = 0;
        public const UInt16 DI_DIRECTION_COUNT_MODE = 1;
        public const UInt16 DO_DIRECTION_DO_MODE = 0;
        public const UInt16 DO_DIRECTION_PULSE_MODE = 1;

        UInt16[] wGetDIMode = new UInt16[8];        //Get DI Direction Mode
        UInt16[] wSetDI_DIMode = new UInt16[4];     //Set DI Direction DI Mode
        UInt16[] wSetDI_CounterMode = new UInt16[4];//Set DI Direction Counter Mode
        public const UInt16   wDI_DI_MODE = 0;      //Set DI Direction DI Mode Value

        public const UInt16 TRIGGER_TYPE_HI_2_LO = 0x0000; 
        public const UInt16 TRIGGER_TYPE_LO_2_HI = 0x0003;
        public const UInt16 TRIGGER_TYPE_BOTH = 2;

        public static byte GlobalChannel = 0;

        byte bytCount = 4;
        byte bytStartChannel = 0;

        int ret;
        Int32[] hConnection = new Int32[1];

        string Password = "";
        string ioLogicIP = "0.0.0.0";
        string EidosPrinterIp = "0.0.0.0";
        string EidosPrinterPort = "";
        ushort ioLogicPortNo = 502;
        uint   ioLogicTimeout = 0;
        byte   DO_DATA_SEND_TO_FISKER = 0;
        byte   DI_DUMMY_PALLET = 0;
        byte   DI_READY_FOR_LABEL = 0;
        byte   DO_LABEL_SEND_TO_PRINTER = 0;
        byte   DI_PALLET_FINISHED = 0;
        byte   DO_DUMMY_PALLET = 0;
        int    DI_SignalFilter = 0;
        int    DO_SignalFilter = 0;
        string MyString = "";
        string NavServiceServer = "";
        string NavServiceServiceName = "";
        string NavServiceUserName = "";
        string NavServicePassword = "";
        bool IsNavLocalService = false;
        //------------- Format variable ------------
        string s_port = "";
        string s_timeout = "";
        string s_DOnewSSCC = "";
        string s_DIwrap = "";
        string s_DIReady = "";
        string s_DOlabel = "";
        string s_DIfin = "";
        string s_DIst = "";
        string s_DOst = "";
        int    NewPalletPosIs = 0;
        int    i_DoDummyPallet = 0;
        string s_SSCCPath = "";
        string s_SSCCLabelName = "";
        bool b_DeleteFile = false;
        
        //------------- Format variable ------------
  

        private void RunTimer_Tick(object sender, EventArgs e)
        {
            Int32 dwShiftValue = 0x0000;
            int i = 0;
            UInt32[] dwGetDIValue = new UInt32[1];
            if (cbService.Checked)
              CheckService(NavServiceServer, NavServiceUserName, NavServicePassword, NavServiceServiceName, false);
            BalanceWS.BalanceWS CBPAutomation = new BalanceWS.BalanceWS();
            CBPAutomation.UseDefaultCredentials = true;
            try
            {
                bool OK = false;
                OK = CBPAutomation.WSNewPalletExist(NewPalletPosIs);
                if (OK)
                {
                    ret = CheckConnection(false);
                    if (ret == MXIO_CS.EIO_SOCKET_DISCONNECT)
                        CreateIoHandle();
                    ret = MOXA_CSharp_MXIO.MXIO_CS.E1K_DO_Writes(hConnection[0], DO_DATA_SEND_TO_FISKER, 1, TRIGGER_TYPE_LO_2_HI);
                    MXEIO_Error.CheckErr(ret, "E1K_DO_Writes");
                    if (ret == MXIO_CS.MXIO_OK)
                        tbCommunication.AppendText(string.Format("New Pallet E1K_DO_Writes Set Ch{0} DO Direction DO Mode value = ON success.{1}", DO_DATA_SEND_TO_FISKER, "\r\n"));
                    else
                        tbCommunication.AppendText(string.Format("New Pallet E1K_DO_Write Ch{0} ON returned ERROR: {1} Return value: {2}{3}", DO_DATA_SEND_TO_FISKER, MXEIO_Error.CheckErr(ret, "E1K_DO_Writes"), ret.ToString(), "\r\n"));
                    GlobalChannel = DO_DATA_SEND_TO_FISKER;
                    Thread.Sleep(2000);
                    ResetChanel_Tick(sender, e);
                }

                if (OK)
                {
                    try
                    {
                        CBPAutomation.WSRenameNewPallet(NewPalletPosIs);
                    }
                    catch (Exception ex)
                    {
                        tbCommunication.AppendText(string.Format("Rename New pallet failed: {0}{1}", MessageBox.Show(ex.Message), "\r\n"));
                    } 
                }
            }
            catch (Exception ex)
            {
                tbCommunication.AppendText(string.Format("New pallet check failed: {0}{1}", MessageBox.Show(ex.Message), "\r\n"));
            }

            ret = CheckConnection(false);
            if (ret == MXIO_CS.EIO_SOCKET_DISCONNECT)
                CreateIoHandle();

            ret = MXIO_CS.E1K_DI_Reads(hConnection[0], bytStartChannel, bytCount, dwGetDIValue);
            MXEIO_Error.CheckErr(ret, "E1K_DI_Reads");
            
            if (ret == MXIO_CS.MXIO_OK)
            {
                for (i = 0, dwShiftValue = 0; i < bytCount; i++, dwShiftValue++)
                {
                    MyString = (((dwGetDIValue[0] & (1 << dwShiftValue)) == 0) ? "OFF" : "ON");
                    if (MyString.Equals("ON"))
                    {
                        // se hvilken Ch det drejer sig om og udfør handling.
                        //tbCommunication.AppendText(string.Format("DI value: ch[{0}] = {1}{2}", i + bytStartChannel, ((dwGetDIValue[0] & (1 << dwShiftValue)) == 0) ? "OFF" : "ON", "\r\n"));
                        if ((dwShiftValue) == DI_DUMMY_PALLET)
                        {
                            try
                            {
                                CBPAutomation.WSAddPalletToQue(string.Format("W {0}", DateTime.Now.ToShortDateString() + ' ' + DateTime.Now.ToShortTimeString()));
                                tbCommunication.AppendText(string.Format("Add Wrap-only pallet OK: {0}",  "\r\n"));
                            }
                            catch (Exception ex)
                            {
                                tbCommunication.AppendText(string.Format("Add Wrap-only pallet failed: {0}{1}", MessageBox.Show(ex.Message), "\r\n"));
                            }
                        }


                        if ((dwShiftValue) == DI_READY_FOR_LABEL)
                        {
                            try
                            {
                                if (CBPAutomation.WSIsDummyPallet())
                                {
                                    ret = CheckConnection(false);
                                    if (ret == MXIO_CS.EIO_SOCKET_DISCONNECT)
                                        CreateIoHandle();
                                    
                                    ret = MOXA_CSharp_MXIO.MXIO_CS.E1K_DO_Writes(hConnection[0], DO_DUMMY_PALLET, 1, TRIGGER_TYPE_LO_2_HI);
                                    MXEIO_Error.CheckErr(ret, "E1K_DO_Writes");
                                    if (ret == MXIO_CS.MXIO_OK)
                                        tbCommunication.AppendText(string.Format("Dummy Pallet DataE1K_DO_Writes Set Ch{0} DO Direction DO Mode value = ON success.{1}", DO_LABEL_SEND_TO_PRINTER, "\r\n"));
                                    else
                                        tbCommunication.AppendText(string.Format("Dummy Pallet E1K_DO_Write returned ERROR: {0} Return value: {1}{2}", MXEIO_Error.CheckErr(ret, "E1K_DO_Writes"), ret.ToString(), "\r\n"));
                                    Thread.Sleep(2000);
                                    GlobalChannel = DO_DUMMY_PALLET;
                                    ResetChanel_Tick(sender, e);
                                }

                                else if (CBPAutomation.WSCreateSSCCDataFile())
                                {
                                    if (System.IO.File.Exists(s_SSCCPath + s_SSCCLabelName))
                                        System.IO.File.Copy(s_SSCCPath + s_SSCCLabelName, EidosPrinterIp + EidosPrinterPort, true);
                                            
                                    if (b_DeleteFile)
                                    {
                                        System.IO.File.Delete(s_SSCCPath + s_SSCCLabelName);
                                    }

                                    ret = CheckConnection(false);
                                    if (ret == MXIO_CS.EIO_SOCKET_DISCONNECT)
                                        CreateIoHandle();
        
                                    ret = MOXA_CSharp_MXIO.MXIO_CS.E1K_DO_Writes(hConnection[0], DO_LABEL_SEND_TO_PRINTER, 1, TRIGGER_TYPE_LO_2_HI);
                                    MXEIO_Error.CheckErr(ret, "E1K_DO_Writes");
                                    if (ret == MXIO_CS.MXIO_OK)
                                        tbCommunication.AppendText(string.Format("Create SSCC DataE1K_DO_Writes Set Ch{0} DO Direction DO Mode value = ON success.{1}", DO_LABEL_SEND_TO_PRINTER, "\r\n"));
                                    else
                                        tbCommunication.AppendText(string.Format("Create SSCC E1K_DO_Write returned ERROR: {0} Return value: {1}{2}", MXEIO_Error.CheckErr(ret, "E1K_DO_Writes"), ret.ToString(), "\r\n"));
                                    Thread.Sleep(2000);
                                    GlobalChannel = DO_LABEL_SEND_TO_PRINTER;
                                    //RunTimer.Stop();
                                    //ResetChanel.Start();
                                    ResetChanel_Tick(sender, e);
                                }
                            }
                            catch (Exception ex)
                            {
                                tbCommunication.AppendText(string.Format("Send SSCC Data to printer failed: {0}{1}", MessageBox.Show(ex.Message), "\r\n"));
                            }
                        }

                        if ((dwShiftValue) == DI_PALLET_FINISHED)
                        {
                            try
                            {
                                CBPAutomation.WSRemovePallet();
                                tbCommunication.AppendText(string.Format("Remove pallet success: {0}", "\r\n"));
                                Thread.Sleep(2000);
                            }
                            catch (Exception ex)
                            {
                                tbCommunication.AppendText(string.Format("Remove pallet failed: {0}{1}", MessageBox.Show(ex.Message), "\r\n"));
                            }
                        }
                    }
                }
            }
        }
        private void btnAutoRun_Click(object sender, EventArgs e)
        {
            if (RunTimer.Enabled)
            {
                RunTimer.Stop();
                btnAutoRun.Text = "Run Auto";
            }
            else
            {
                RunTimer.Start();
                btnAutoRun.Text = "Stop Auto";
            }

        }
        private int CreateIoHandle()
        {
            ret = MXIO_CS.MXEIO_E1K_Connect(System.Text.Encoding.UTF8.GetBytes(ioLogicIP), ioLogicPortNo, ioLogicTimeout, hConnection, System.Text.Encoding.UTF8.GetBytes(Password));
            MXEIO_Error.CheckErr(ret, "MXEIO_E1K_Connect");
            if (ret == MXIO_CS.MXIO_OK)
                tbCommunication.AppendText("MXEIO_E1K_Connect Success.\r\n");
            else
                tbCommunication.AppendText("MXEIO_E1K_Connect FAILED!!!.\r\n");

            return ret; 
        }
        private int CheckConnection(bool ShowDialog)
        {
            //--------------------------------------------------------------------------
            //Check Connection
            byte[] bytCheckStatus = new byte[1];
            ret = MXIO_CS.MXEIO_CheckConnection(hConnection[0], ioLogicTimeout, bytCheckStatus);
            MXEIO_Error.CheckErr(ret, "MXEIO_CheckConnection");
            if (ret == MXIO_CS.MXIO_OK)
            {
                switch (bytCheckStatus[0])
                {
                    case MXIO_CS.CHECK_CONNECTION_OK:
                        if (ShowDialog)
                            tbCommunication.AppendText(string.Format("MXEIO_CheckConnection: Check connection ok => {0}{1}", bytCheckStatus[0], "\r\n"));
                        break;
                    case MXIO_CS.CHECK_CONNECTION_FAIL:
                        if (ShowDialog)
                            tbCommunication.AppendText(string.Format("MXEIO_CheckConnection: Check connection fail => {0}{1}", bytCheckStatus[0], "\r\n"));
                        break;
                    case MXIO_CS.CHECK_CONNECTION_TIME_OUT:
                        if (ShowDialog)
                            tbCommunication.AppendText(string.Format("MXEIO_CheckConnection: Check connection time out => {0}{1}", bytCheckStatus[0], "\r\n"));
                        break;
                    default:
                        if (ShowDialog)
                            tbCommunication.AppendText(string.Format("MXEIO_CheckConnection: Check connection status unknown => {0}{1}", bytCheckStatus[0], "\r\n"));
                        break;
                }
            }
            return ret;
            //--------------------------------------------------------------------------
        }
        private void CheckService(string ServerName, string DomainUserName, string DomainPassword, string ServiceName, bool ShowDialog)
        {
            if (IsNavLocalService)
            {
                try
                {
                    ServiceController controller = new ServiceController(NavServiceServiceName, "NAV60TEST");
                   
                    if (controller.Status.Equals(ServiceControllerStatus.Stopped))
                    {
                        controller.Start();
                        controller.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(10.0));
                        tbCommunication.AppendText(string.Format("Service Started\r\n"));
                    }
                }
                catch (Exception ex)
                {
                    tbCommunication.AppendText(string.Format("{0}{1}",ex.Message, "\r\n"));
                }
            }
            else
            {
                string ns = @"root\cimv2";
                ConnectionOptions op = new ConnectionOptions();
                op.Username = DomainUserName;
                op.Password = DomainPassword;
                ManagementScope scope = new ManagementScope(string.Format(@"\\{0}\{1}", ServerName, ns), op);
                scope.Connect();
                ManagementPath path = new ManagementPath("Win32_Service");
                ObjectQuery query = new ObjectQuery(string.Format("SELECT * FROM Win32_Service WHERE Name = '{0}'", ServiceName));
                ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, query);

                foreach (ManagementObject service in searcher.Get())
                {

                    if (service.GetPropertyValue("State").ToString().ToLower().Equals("stopped"))
                    {
                        if (ShowDialog)
                        {
                            tbCommunication.AppendText(string.Format("The Service {0} on Server: {1} was stopped. Trying to Start it...{2}", ServiceName, ServerName, "\r\n"));
                        }
                        service.InvokeMethod("StartService", null);
                    }
                }
            }
        }
        private void btnInit_Click(object sender, EventArgs e)
        {
            //--------------------------------------------------------------------------
            ret = MXIO_CS.MXIO_GetDllVersion();
            tbCommunication.AppendText(string.Format("MXIO_GetDllVersion:{0}.{1}.{2}.{3}{4}", (ret >> 12) & 0xF, (ret >> 8) & 0xF, (ret >> 4) & 0xF, (ret) & 0xF, "\r\n"));

            ret = MXIO_CS.MXIO_GetDllBuildDate();
            tbCommunication.AppendText(string.Format("MXIO_GetDllBuildDate:{0:x}/{1:x}/{2:x}{3}", (ret >> 16), (ret >> 8) & 0xFF, (ret) & 0xFF, "\r\n"));
            //--------------------------------------------------------------------------
            ret = MXIO_CS.MXEIO_Init();
            tbCommunication.AppendText(string.Format("MXEIO_Init return {0}{1}", ret, "\r\n"));
            //--------------------------------------------------------------------------
            //Connect to ioLogik device                
            tbCommunication.AppendText(string.Format("MXEIO_E1K_Connect IP={0}, Port={1}, Timeout={2}, Password={3}{4}", ioLogicIP, ioLogicPortNo, ioLogicTimeout, Password, "\r\n"));
            //Create Connection
            CreateIoHandle();
            //Check Connection
            CheckConnection(true);
            //Check the if the webservice for Navision is running
            if (cbService.Checked)
                CheckService(NavServiceServer, NavServiceUserName, NavServicePassword, NavServiceServiceName, true);
            //Get firmware Version
            byte[] bytRevision = new byte[4];
            ret = MXIO_CS.MXIO_ReadFirmwareRevision(hConnection[0], bytRevision);
            MXEIO_Error.CheckErr(ret, "MXIO_ReadFirmwareRevision");
            if (ret == MXIO_CS.MXIO_OK)
                tbCommunication.AppendText(string.Format("MXIO_ReadFirmwareRevision:V{0}.{1}, Release:{2}, build:{3}{4}", bytRevision[0], bytRevision[1], bytRevision[2], bytRevision[3], "\r\n"));
            //--------------------------------------------------------------------------
            //Get firmware Release Date
            UInt16[] wGetFirmwareDate = new UInt16[2];
            ret = MXIO_CS.MXIO_ReadFirmwareDate(hConnection[0], wGetFirmwareDate);
            MXEIO_Error.CheckErr(ret, "MXIO_ReadFirmwareDate");
            if (ret == MXIO_CS.MXIO_OK)
                tbCommunication.AppendText(string.Format("MXIO_ReadFirmwareDate:{0:x}/{1:x}/{2:x}{3}", wGetFirmwareDate[1], (wGetFirmwareDate[0] >> 8) & 0xFF, (wGetFirmwareDate[0]) & 0xFF, "\r\n"));
            //--------------------------------------------------------------------------
            //Get Module Type
            UInt16[] wModuleType = new UInt16[1];
            ret = MXIO_CS.MXIO_GetModuleType(hConnection[0], 0, wModuleType);
            MXEIO_Error.CheckErr(ret, "MXIO_GetModuleType");
            if (ret == MXIO_CS.MXIO_OK)
                tbCommunication.AppendText(string.Format("MXIO_GetModuleType: Module Type = {0:x}{1}", wModuleType[0], "\r\n"));
            //--------------------------------------------------------------------------

            byte safebytCount = 8;
            byte safebytStartChannel = 0;

            //Set Power On value = OFF
            UInt32 dwSetDOPowerOnValue = 0;
            ret = MXIO_CS.E1K_DO_SetPowerOnValues(hConnection[0], safebytStartChannel, safebytCount, dwSetDOPowerOnValue);
            MXEIO_Error.CheckErr(ret, "E1K_DO_GetPowerOnValues");
            if (ret == MXIO_CS.MXIO_OK)
                tbCommunication.AppendText(string.Format("E1K_DO_SetPowerOnValues Set Ch{0}~ch{1} DO Direction DO Mode Power On value = OFF success.{2}", bytStartChannel, safebytCount + bytStartChannel - 1, "\r\n"));

            //Get Ch{0}~ch{1} DO Direction DO Mode value
            UInt32[] dwGetDOValue = new UInt32[1];
            ret = MXIO_CS.E1K_DO_Reads(hConnection[0], safebytStartChannel, safebytCount, dwGetDOValue);
            MXEIO_Error.CheckErr(ret, "E1K_DO_Reads");
            if (ret == MXIO_CS.MXIO_OK)
            {
                tbCommunication.AppendText(string.Format("E1K_DO_Reads Get Ch{0}~ch{1} DO Direction DO Mode value success.{2}", bytStartChannel, bytCount + bytStartChannel - 1, "\r\n"));
                for (int i = 0, dwShiftValue = 0; i < safebytCount; i++, dwShiftValue++)
                    tbCommunication.AppendText(string.Format("DO value: ch[{0}] = {1}{2}", i + safebytStartChannel, ((dwGetDOValue[0] & (1 << dwShiftValue)) == 0) ? "OFF" : "ON", "\r\n"));
            }

            UInt16[] wDoSafeValue = new UInt16[8] { 0, 0, 0, 0, 0, 0, 0, 0 };

            ret = MXIO_CS.E1K_DO_SetSafeValues_W(hConnection[0], safebytStartChannel, safebytCount, wDoSafeValue);
            MXEIO_Error.CheckErr(ret, "E1K_DO_SetSafeValues_W");
            if (ret == MXIO_CS.MXIO_OK)
                tbCommunication.AppendText(string.Format("E1K_DO_SetSafeValues_W ch{0}~ch{1} success.{2}", safebytStartChannel, safebytStartChannel + safebytCount - 1, "\r\n"));
            //Get Ch XX DO Direction DO Mode safe values
            UInt16[] wGetDOSafeValue = new UInt16[8];
            ret = MXIO_CS.E1K_DO_GetSafeValues_W(hConnection[0], safebytStartChannel, safebytCount, wGetDOSafeValue);
            MXEIO_Error.CheckErr(ret, "E1K_DO_GetSafeValues_W");
            if (ret == MXIO_CS.MXIO_OK)
            {
                tbCommunication.AppendText(string.Format("E1K_DO_GetSafeValues_W Get Ch{0}~ch{1} DO Direction DO Mode DO Safe Value success.", safebytStartChannel, safebytStartChannel + safebytCount - 1, "\r\n"));
                for (int i = 0; i < safebytCount; i++)
                {
                    switch (wGetDOSafeValue[i])
                    {
                        case 0:
                            tbCommunication.AppendText(string.Format("DO Safe value: ch[{0}] = {1}{2}", i, "OFF", "\r\n"));
                            break;
                        case 1:
                            tbCommunication.AppendText(string.Format("DO Safe value: ch[{0}] = {1}{2}", i, "ON", "\r\n"));
                            break;
                        case 2:
                            tbCommunication.AppendText(string.Format("DO Safe value: ch[{0}] = {1}{2}", i, "Hold Last", "\r\n"));
                            break;
                    }
                }
            }
                //  ---------------------------------------  DI setup -------------------------------------------------

                //Set Ch0~ch3 DI Direction DI Mode
                for (int i = 0; i < bytCount; i++)
                    wSetDI_DIMode[i] = DI_DIRECTION_DI_MODE;

                ret = MXIO_CS.E1K_DI_SetModes(hConnection[0], bytStartChannel, bytCount, wSetDI_DIMode);
                MXEIO_Error.CheckErr(ret, "E1K_DI_SetModes");
                if (ret == MXIO_CS.MXIO_OK)
                {
                    tbCommunication.AppendText(string.Format("E1K_DI_SetModes Set Ch{0} ~ Ch{1} DI Direction DI Mode Succcess.{2}", bytStartChannel, bytCount - 1, "\r\n"));
                }
                //Get Ch0~ch3 DI Direction Mode
                ret = MXIO_CS.E1K_DI_GetModes(hConnection[0], bytStartChannel, bytCount, wGetDIMode);
                MXEIO_Error.CheckErr(ret, "E1K_DI_GetModes");
                if (ret == MXIO_CS.MXIO_OK)
                {
                    tbCommunication.AppendText(string.Format("E1K_DI_GetModes Get Ch{0}~ch{1} DI Direction Mode success.", bytStartChannel, bytCount + bytStartChannel - 1));
                    tbCommunication.AppendText("\r\n");
                    for (int i = 0; i < bytCount; i++)
                    {
                        tbCommunication.AppendText(string.Format("ch{0}={1}", i + bytStartChannel, (wGetDIMode[i] == wDI_DI_MODE) ? "DI_MODE" : "COUNT_MODE"));
                        tbCommunication.AppendText("\r\n");
                    }
                }
                //*******************
                // Set/Get DI filter
                //*******************
                //Set Ch0~ch3 DI Direction Filter
                UInt16[] wFilter = new UInt16[8];
                for (int i = 0; i < safebytCount; i++)
                    wFilter[i] = (UInt16)(DI_SignalFilter);
                ret = MXIO_CS.E1K_DI_SetFilters(hConnection[0], safebytStartChannel, safebytCount, wFilter);
                MXEIO_Error.CheckErr(ret, "E1K_DI_SetFilters");
                if (ret == MXIO_CS.MXIO_OK)
                {
                    tbCommunication.AppendText(string.Format("E1K_DI_SetFilters Set Ch{0}~ch{1} DI Direction Filter to {2} return {3}{4}", safebytStartChannel, safebytCount + safebytStartChannel - 1, DI_SignalFilter, ret, "\r\n"));
                }

                //Get Ch0~ch3 DI Direction Filter
                ret = MXIO_CS.E1K_DI_GetFilters(hConnection[0], safebytStartChannel, safebytCount, wFilter);
                MXEIO_Error.CheckErr(ret, "E1K_DI_GetFilters");
                if (ret == MXIO_CS.MXIO_OK)
                {
                    tbCommunication.AppendText(string.Format("E1K_DI_GetFilters Get Ch{0}~ch{1} DI Direction Filter return {2}", bytStartChannel, bytCount + bytStartChannel - 1, ret));
                    tbCommunication.AppendText("\r\n");
                    for (int i = 0; i < safebytCount; i++)
                    {
                        tbCommunication.AppendText(string.Format("DI Filter Value: ch[{0}] = {1}{2}", i, wFilter[i], "\r\n"));
                    }
                }
            
        }
        private void WaitTimer_Tick(object sender, EventArgs e)
        {
            tbCommunication.AppendText(string.Format("WaitTimer active for: {0}{1}", WaitTimer.Interval.ToString(), "\r\n"));
            WaitTimer.Stop();
        }
        private void ResetChanel_Tick(object sender, EventArgs e)
        {
            ret = CheckConnection(false);
            if (ret == MXIO_CS.EIO_SOCKET_DISCONNECT)
                CreateIoHandle();

            ret = MOXA_CSharp_MXIO.MXIO_CS.E1K_DO_Writes(hConnection[0], GlobalChannel, 1, TRIGGER_TYPE_HI_2_LO);
            MXEIO_Error.CheckErr(ret, "E1K_DO_Writes");
            if (ret == MXIO_CS.MXIO_OK)
                tbCommunication.AppendText(string.Format("Reset E1K_DO_Writes Set Ch{0} DO Direction DO Mode value = OFF success.{1}", GlobalChannel, "\r\n"));
            else
                tbCommunication.AppendText(string.Format("Reset E1K_DO_Write Ch{0} OFF returned ERROR: {1} Return value: {2}{3}", GlobalChannel, MXEIO_Error.CheckErr(ret, "E1K_DO_Writes"), ret.ToString(), "\r\n"));

            tbCommunication.AppendText(string.Format("ResetChanelTimer active for: {0}{1}", ResetChanel.Interval.ToString(), "\r\n"));
            GlobalChannel = 0;
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            /*
             * Try / Catch til initialisering af variabler gennem WebService til Navision
             * 
             */
            BalanceWS.BalanceWS CBPAutomation = new BalanceWS.BalanceWS();
            CBPAutomation.UseDefaultCredentials = true;
            tbCommunication.AppendText("Sending Param request to Navision\r\n");
            try
            {
                CBPAutomation.WSReturnParam(ref ioLogicIP, ref s_port, ref s_timeout, ref s_DOnewSSCC, ref s_DIwrap, ref s_DIReady, ref s_DOlabel, ref s_DIfin, ref s_DOst, ref s_DIst, ref EidosPrinterIp, ref EidosPrinterPort, ref NewPalletPosIs, ref s_SSCCPath, ref s_SSCCLabelName, ref b_DeleteFile, ref i_DoDummyPallet);
                ioLogicPortNo = ushort.Parse(s_port);
                ioLogicTimeout = ushort.Parse(s_timeout);
                DO_DATA_SEND_TO_FISKER = byte.Parse(s_DOnewSSCC);
                DI_DUMMY_PALLET = byte.Parse(s_DIwrap);
                DO_LABEL_SEND_TO_PRINTER = byte.Parse(s_DOlabel);
                DI_READY_FOR_LABEL = byte.Parse(s_DIReady);
                DI_PALLET_FINISHED = byte.Parse(s_DIfin);
                DO_DUMMY_PALLET =  byte.Parse(i_DoDummyPallet.ToString());
                DI_SignalFilter = int.Parse(s_DIst);
                DO_SignalFilter = int.Parse(s_DOst);

                tbCommunication.AppendText(string.Format("ioLogic IP from Nav: {0}{1}", ioLogicIP, "\r\n"));
                tbCommunication.AppendText(string.Format("ioLogic Port from Nav: {0}{1}", ioLogicPortNo, "\r\n"));
                tbCommunication.AppendText(string.Format("Eidos Printer IP from Nav: {0}{1}", EidosPrinterIp, "\r\n"));
                tbCommunication.AppendText(string.Format("Eidos Port from Nav: {0}{1}", EidosPrinterPort, "\r\n"));
                tbCommunication.AppendText(string.Format("SSCC Label path from Nav: {0}{1}", s_SSCCPath, "\r\n"));
                tbCommunication.AppendText(string.Format("SSCC Label Name from Nav: {0}{1}", s_SSCCLabelName, "\r\n"));

                tbCommunication.AppendText(string.Format("DoNewPallet from Nav: {0}{1}", DO_DATA_SEND_TO_FISKER, "\r\n"));
                tbCommunication.AppendText(string.Format("DiLabelRequest from Nav: {0}{1}", DI_READY_FOR_LABEL, "\r\n"));
                tbCommunication.AppendText(string.Format("DoLabelDataSend from Nav: {0}{1}", DO_LABEL_SEND_TO_PRINTER, "\r\n"));
                tbCommunication.AppendText(string.Format("DiDummyPallet from Nav: {0}{1}", DI_DUMMY_PALLET, "\r\n"));
                tbCommunication.AppendText(string.Format("DoDummyPallet from Nav: {0}{1}", DO_DUMMY_PALLET, "\r\n"));
                tbCommunication.AppendText(string.Format("DiPalletEND from Nav: {0}{1}", DI_PALLET_FINISHED, "\r\n"));

                CBPAutomation.WSReturnServiceParam(ref NavServiceServer, ref NavServiceUserName, ref NavServicePassword, ref NavServiceServiceName, ref IsNavLocalService);
                tbCommunication.AppendText(string.Format("Nav Webservice Server from Nav: {0}{1}", NavServiceServer, "\r\n"));
                tbCommunication.AppendText(string.Format("User for starting Service from Nav: {0}{1}", NavServiceUserName, "\r\n"));
                tbCommunication.AppendText(string.Format("Password for starting Service from Nav: {0}{1}", NavServicePassword, "\r\n"));
                tbCommunication.AppendText(string.Format("Service to Start from Nav: {0}{1}", NavServiceServiceName, "\r\n"));
                tbCommunication.AppendText(string.Format("Service is running local from Nav: {0}{1}", IsNavLocalService.ToString(), "\r\n"));

                btnInit_Click(sender, e);
                btnAutoRun_Click(sender, e);
            }
            catch (Exception ex)
            {
                tbCommunication.AppendText(string.Format("WebService faild with message: {0}{1}", ex.Message.ToString(), "\r\n"));
            }
        }
        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            ret = MOXA_CSharp_MXIO.MXIO_CS.MXEIO_Disconnect(hConnection[0]);
            tbCommunication.AppendText("Closing the connection: " + MXEIO_Error.CheckErr(ret, "MXEIO_Disconnect") + " " + ret.ToString());
            tbCommunication.AppendText("\r\n");

            MOXA_CSharp_MXIO.MXIO_CS.MXEIO_Exit();
            tbCommunication.AppendText("EXIT Communication: OK");
            tbCommunication.AppendText("\r\n");

        }
    }
}
