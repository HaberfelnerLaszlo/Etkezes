using Etkezes_Models.ViewModels;

using libzkfpcsharp;

using Microsoft.Extensions.Logging;

using SourceAFIS;

using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection.Metadata;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

using ZkTecoFingerPrint;
namespace FingerPrintService
{
    public class FPServiceWindows : IDisposable, IFPService
    {
        IntPtr mDevHandle = IntPtr.Zero;
        IntPtr mDBHandle = IntPtr.Zero;
        //string mDevName = "";
        int devCount = 0;
        public string ErrorInfo = "", SuccessInfo = "";
        bool bIsTimeToDie = false;
        bool IsRegister = false;
        bool bIdentify = true;
        byte[]? FPBuffer;
        int RegisterCount = 0;
        const int REGISTER_FINGER_COUNT = 3;
        const int DEVICE_OPEN = 100;
        const int SUCCESS_DB_ADD = 201;
        private readonly ILogger<FPServiceWindows> _logger;
        byte[][] RegTmps = new byte[3][];
        byte[] RegTmp = new byte[2048];
        byte[] CapTmp = new byte[2048];
        int cbCapTmp = 2048;
        int cbRegTmp = 0;
        int iFid = 1;
        Thread? captureThread = null;
        int ProcessId = 1;
        private int mfpWidth = 0;
        private int mfpHeight = 0;
        public event EventHandler<FPMessageChangedEventArgs>? MessageChanged;
        public event EventHandler<FPRegisteredEventArgs>? FingerPrintRegistered;
        public event EventHandler<FPSuccessIdentificationEventArgs>? SuccessIdentification;
        public FPServiceWindows(ILogger<FPServiceWindows> logger)
        {
            _logger = logger;
            bool flowControl = DeviceInit();
            if (!flowControl)
            {
                return;
            }
        }

        public bool DeviceInit()
        {
            int ret = zkfperrdef.ZKFP_ERR_OK;
            if ((ret = zkfp2.Init()) == zkfperrdef.ZKFP_ERR_OK)
            //if ((ret = ZkfpLinux.ZKFPM_Init()) == zkfperrdef.ZKFP_ERR_OK)
            {
                devCount = zkfp2.GetDeviceCount();
                //devCount = ZkfpLinux.ZKFPM_GetDeviceCount(); 
                
                if (devCount > 0)
                {
                _logger.LogInformation("Number of devices connected: {DevCount}", devCount);
                    if (devCount > 1)
                    {
                        ErrorInfo = devCount + " darab eszköz csatlakoztatva!";
                        MessageChanged?.Invoke(this, new FPMessageChangedEventArgs(ErrorInfo, true));
                        return false;
                    }
                    SuccessInfo = "Eszköz csatlakoztatva, kész a használatra!";  
                    _logger.LogInformation(SuccessInfo);
                    MessageChanged?.Invoke(this, new FPMessageChangedEventArgs(SuccessInfo, false, DEVICE_OPEN));
                }
            }
            else
            {
                //ZkfpLinux.ZKFPM_Terminate();
                zkfp2.Terminate();
                ErrorInfo = $"Nincs csatlakoztatott eszköz! devCount: {devCount}";
                _logger.LogError(ErrorInfo);
                MessageChanged?.Invoke(this, new FPMessageChangedEventArgs(ErrorInfo, true));
                return false;
            }

            return true;
        }

        /// <summary>
        /// Service destructor to ensure that the device handle is properly closed when the service is disposed of. This prevents resource leaks and ensures that the fingerprint device is released correctly.
        /// </summary>
        ~FPServiceWindows()
        {
            if (mDevHandle != IntPtr.Zero)
            {
                _logger.LogInformation("Destructor called, closing device handle...");
                //ZkfpLinux.ZKFPM_CloseDevice(mDevHandle);
                zkfp2.CloseDevice(mDevHandle);
                mDevHandle = IntPtr.Zero;
            }
        }
        public async Task Open()
        {
            if (devCount < 1)
            {
                _logger.LogWarning("No devices connected, attempting to reinitialize...");
                bool flowControl = DeviceInit();
                if (!flowControl)
                {
                    return;
                }
            }

            int ret = zkfp.ZKFP_ERR_OK;
            if (IntPtr.Zero == mDevHandle )
 {
            Console.WriteLine("Opening device...");
                if (IntPtr.Zero == (mDevHandle = zkfp2.OpenDevice(0)))
            {
                ErrorInfo = "Eszköz megnyitása sikertelen!";
                _logger?.LogError(ErrorInfo+mDevHandle.ToString());
                MessageChanged?.Invoke(this, new FPMessageChangedEventArgs(ErrorInfo, true));
                return;
            }
            }
            if (IntPtr.Zero == (mDBHandle = zkfp2.DBInit()))
            {
                ErrorInfo = "DB inicializálása sikertelen!"; 
                _logger?.LogError(ErrorInfo);
                MessageChanged?.Invoke(this, new FPMessageChangedEventArgs(ErrorInfo, true));
                zkfp2.CloseDevice(mDevHandle);
                mDevHandle = IntPtr.Zero;
                return;
            }
            RegisterCount = 0;
            cbRegTmp = 0;
            //iFid = 1;
            for (int i = 0; i < 3; i++)
            {
                RegTmps[i] = new byte[2048];
            }
            byte[] paramValue = new byte[4];
            int size = 4;
            zkfp2.GetParameters(mDevHandle, 1, paramValue, ref size);
            //mfpHeight=ByteIntConverter.ToInt(paramValue, bigEndian: false);
            zkfp2.ByteArray2Int(paramValue, ref mfpHeight);
            size = 4;
            zkfp2.GetParameters(mDevHandle, 2, paramValue, ref size);
            zkfp2.ByteArray2Int(paramValue,ref mfpWidth);
            FPBuffer = new byte[mfpWidth * mfpHeight];

            captureThread = new Thread(new ThreadStart(DoCapture));
            captureThread.IsBackground = true;
            captureThread.Start();
            bIsTimeToDie = false;

        }
        private void DoCapture()
        {
            Console.WriteLine("Starting fingerprint capture thread...");
            while (!bIsTimeToDie)
            {
                cbCapTmp = 2048;
                int ret = zkfp2.AcquireFingerprint(mDevHandle, FPBuffer!, CapTmp, ref cbCapTmp);
                if (ret == zkfp.ZKFP_ERR_OK)
                {
                    // Process the captured fingerprint data (FPBuffer) as needed
                    Console.WriteLine("Fingerprint captured successfully, processing...");
                    //Register();
                    ProcessFingerprintData();
                }
                Thread.Sleep(200);
            }
            Console.WriteLine("Exiting fingerprint capture thread...");
        }
        private void ProcessFingerprintData()
        {
            // Implement any additional processing of the fingerprint data here
            // For example, you could convert the raw data to an image or extract features
            Console.WriteLine("Processing fingerprint data...");
            switch (ProcessId)
            {
                case 0:
                    {
                        // Registration mode processing
                        Console.WriteLine("Processing in registration mode...");
                        if (cbRegTmp <= 0)
                        {
                            ErrorInfo = "Kérem, először regisztrálja az ujjlenyomatát!";
                            MessageChanged?.Invoke(this, new FPMessageChangedEventArgs(ErrorInfo, true, -1));
                            cbRegTmp = RegTmp.Length;
                            Console.WriteLine("First registration, skipping identification check..." + $"RegTmp hossz: {cbRegTmp}");
                            return;
                        }
                        int ret = zkfp.ZKFP_ERR_OK;
                        int fid = 0, score = 0;
                        ret = zkfp2.DBIdentify(mDBHandle, CapTmp, ref fid, ref score);
                        //fid = (int)fidUint;
                        //score = (int)scoreUint;
                        if (zkfp.ZKFP_ERR_OK == ret)
                        {
                            ErrorInfo = "Ujjlenyomat már regisztrálva van, fid= " + fid + "!";
                            MessageChanged?.Invoke(this, new FPMessageChangedEventArgs(ErrorInfo, true, ret));
                            return;
                        }
                        //int APICALL ZKFPM_DBMatch (HANDLE hDBCache, unsigned char* fpTemplate1, unsigned int cbfpTemplate1, unsigned char* fpTemplate2, unsigned int cbfpTemplate2);
                        if (RegisterCount > 0 && zkfp2.DBMatch(mDBHandle, CapTmp, RegTmps[RegisterCount - 1]) <= 0)
                        {
                            SuccessInfo = "Kérem, érintse meg ugyanazt az ujjlenyomatot 3 alkalommal a regisztrációhoz";
                            MessageChanged?.Invoke(this, new FPMessageChangedEventArgs(SuccessInfo, false));
                            return;
                        }

                        Array.Copy(CapTmp, RegTmps[RegisterCount], cbCapTmp);
                        String strBase64 = zkfp2.BlobToBase64(CapTmp, cbCapTmp);
                        byte[] blob = zkfp2.Base64ToBlob(strBase64);
                        RegisterCount++;
                        if (RegisterCount >= REGISTER_FINGER_COUNT)
                        {
                            RegisterCount = 0;
                            if (zkfp.ZKFP_ERR_OK == (ret = zkfp2.DBMerge(mDBHandle, RegTmps[0], RegTmps[1], RegTmps[2], RegTmp, ref cbRegTmp)) &&
                                   zkfp.ZKFP_ERR_OK == (ret = zkfp2.DBAdd(mDBHandle, iFid, RegTmp)))
                            {
                                string fingerTmpBase64 = zkfp2.BlobToBase64(RegTmp,cbRegTmp);
                                FingerPrintRegistered?.Invoke(this, new FPRegisteredEventArgs(iFid, fingerTmpBase64));
                                SuccessInfo = "Sikeres regisztráció";
                                MessageChanged?.Invoke(this, new FPMessageChangedEventArgs(SuccessInfo, false));
                                iFid = (iFid >= 1000) ? iFid - 999 : iFid + 1;
                            }
                            else
                            {
                                ErrorInfo = "Sikertelen regisztráció, hibakód=" + ret;
                                MessageChanged?.Invoke(this, new FPMessageChangedEventArgs(ErrorInfo, true, ret));
                            }
                            IsRegister = false;
                            ProcessId = 1;
                            return;
                        }
                        else
                        {
                            SuccessInfo = "Sikeres regisztráció, érintse meg a szennert még " + (REGISTER_FINGER_COUNT - RegisterCount) + " ujjlenyomat szükséges";
                            MessageChanged?.Invoke(this, new FPMessageChangedEventArgs(SuccessInfo, false));
                        }
                        break;
                    }
                case 1:
                    {
                        // Identification mode processing
                        Console.WriteLine("Processing in identification mode...");
                        int ret = zkfp.ZKFP_ERR_OK;
                        int fid = 0, score = 0;
                        ret = zkfp2.DBIdentify(mDBHandle, CapTmp, ref fid, ref score);
                        if (zkfp.ZKFP_ERR_OK == ret)
                        {
                            SuccessInfo = "Sikeres azonosítás, fid= " + fid + ",score=" + score + "!";
                            SuccessIdentification?.Invoke(this, new FPSuccessIdentificationEventArgs(fid, score));
                            return;
                        }
                        else
                        {
                            ErrorInfo = "Sikertelen azonosítás, hibakód= " + ret;
                            MessageChanged?.Invoke(this, new FPMessageChangedEventArgs(ErrorInfo, true, ret));
                            return;
                        }
                    }
                case 2:
                    {
                        // Matching mode processing
                        Console.WriteLine("Processing in matching mode...");
                        int fid = 0;
                        int ret = zkfp2.DBMatch(mDBHandle, CapTmp, RegTmp);
                        if (0 < ret)
                        {
                            SuccessInfo = "Sikeres ujjlenyomat egyeztetés, score=" + ret + "!";
                            MessageChanged?.Invoke(this, new FPMessageChangedEventArgs(SuccessInfo, false, fid));
                            return;
                        }
                        else
                        {
                            ErrorInfo = "Sikertelen ujjlenyomat egyeztetés, hibakód= " + ret;
                            MessageChanged?.Invoke(this, new FPMessageChangedEventArgs(ErrorInfo, true, ret));
                            return;
                        }
                    }

                default:
                    break;
            }
        }
        //private void Register()
        //{
        //    //MemoryStream ms = new MemoryStream();
        //    //BitmapFormat.GetBitmap(FPBuffer, mfpWidth, mfpHeight, ref ms);
        //    //Bitmap bmp = new Bitmap(ms);
        //    if (IsRegister)
        //    {
        //        Console.WriteLine("Registering fingerprint...");
        //        int ret = zkfp.ZKFP_ERR_OK;
        //        int fid = 0, score = 0;
        //        if (cbRegTmp <= 0)
        //        {
        //            ErrorInfo = "Kérem, először regisztrálja az ujjlenyomatát!";
        //            MessageChanged?.Invoke(this, new FPMessageChangedEventArgs(ErrorInfo, true, -1));
        //            cbRegTmp=RegTmp.Length;
        //            Console.WriteLine("First registration, skipping identification check..."+$"RegTmp hossz: {cbRegTmp}");
        //            //IsRegister = true;
        //            return;
        //        }
        //        ret = ZkfpLinux.ZKFPM_DBIdentify(mDBHandle, CapTmp, (uint)CapTmp.Length, out uint fidUint, out uint scoreUint);
        //        fid = (int)fidUint;
        //        score = (int)scoreUint;
        //        if (zkfp.ZKFP_ERR_OK == ret)
        //        {
        //            ErrorInfo = "Ujjlenyomat már regisztrálva van, fid= " + fid + "!";
        //            MessageChanged?.Invoke(this, new FPMessageChangedEventArgs(ErrorInfo, true, ret));
        //            return;
        //        }
        //        //int APICALL ZKFPM_DBMatch (HANDLE hDBCache, unsigned char* fpTemplate1, unsigned int cbfpTemplate1, unsigned char* fpTemplate2, unsigned int cbfpTemplate2);
        //        if (RegisterCount > 0 && ZkfpLinux.ZKFPM_DBMatch(mDBHandle, CapTmp, CapTmp.Length, RegTmps[RegisterCount - 1], RegTmps[RegisterCount - 1].Length, ref fid) <= 0)
        //        {
        //            SuccessInfo = "Kérem, érintse meg ugyanazt az ujjlenyomatot 3 alkalommal a regisztrációhoz";
        //            MessageChanged?.Invoke(this, new FPMessageChangedEventArgs(SuccessInfo, false));
        //            return;
        //        }

        //        Array.Copy(CapTmp, RegTmps[RegisterCount], cbCapTmp);
        //        String strBase64 = Base64Converter.ToBase64(CapTmp, false, false);
        //        byte[] blob = Base64Converter.ToBytes(strBase64).Bytes;
        //        RegisterCount++;
        //        if (RegisterCount >= REGISTER_FINGER_COUNT)
        //        {
        //            RegisterCount = 0;
        //            if (zkfp.ZKFP_ERR_OK == (ret = ZkfpLinux.ZKFPM_DBMerge(mDBHandle, RegTmps[0], RegTmps[1], RegTmps[2], RegTmp, ref cbRegTmp)) &&
        //                   zkfp.ZKFP_ERR_OK == (ret = ZkfpLinux.ZKFPM_DBAdd(mDBHandle, iFid, RegTmp,RegTmp.Length)))
        //            {
        //                string fingerTmpBase64 = Base64Converter.ToBase64(RegTmp, false, false);
        //                FingerPrintRegistered?.Invoke(this, new FPRegisteredEventArgs(iFid, fingerTmpBase64));
        //                SuccessInfo = "Sikeres regisztráció";
        //                MessageChanged?.Invoke(this, new FPMessageChangedEventArgs(SuccessInfo, false));
        //                iFid = (iFid >= 1000) ? iFid - 999 : iFid + 1;
        //            }
        //            else
        //            {
        //                ErrorInfo = "Sikertelen regisztráció, hibakód=" + ret;
        //                MessageChanged?.Invoke(this, new FPMessageChangedEventArgs(ErrorInfo, true, ret));
        //            }
        //            IsRegister = false;
        //            return;
        //        }
        //        else
        //        {
        //            SuccessInfo = "Sikeres regisztráció, érintse meg a szennert még " + (REGISTER_FINGER_COUNT - RegisterCount) + " ujjlenyomat szükséges";
        //            MessageChanged?.Invoke(this, new FPMessageChangedEventArgs(SuccessInfo, false));
        //        }
        //    }
        //    else
        //    {
        //        //if (cbRegTmp <= 0)
        //        //{
        //        //    ErrorInfo = "Kérem, először regisztrálja az ujjlenyomatát!";
        //        //    MessageChanged?.Invoke(this, new FPWinMessageChangedEventArgs(ErrorInfo, true, -1));
        //        //    //IsRegister = true;
        //        //    return;
        //        //}
        //        Console.WriteLine("Identifying fingerprint...");
        //        int fid = 0, score = 0;
        //        if (bIdentify)
        //        {
        //            int ret = zkfp.ZKFP_ERR_OK;
                     
        //            ret = ZkfpLinux.ZKFPM_DBIdentify(mDBHandle, CapTmp, (uint)CapTmp.Length, out uint fidUint, out uint scoreUint);
        //             fid = (int)fidUint;
        //             score = (int)scoreUint;
        //            if (zkfp.ZKFP_ERR_OK == ret)
        //            {
        //                SuccessInfo = "Sikeres azonosítás, fid= " + fid + ",score=" + score + "!";
        //                SuccessIdentification?.Invoke(this, new FPSuccessIdentificationEventArgs(fid, score));
        //                //MessageChanged?.Invoke(this, new FPWinMessageChangedEventArgs(SuccessInfo, false));
        //                return;
        //            }
        //            else
        //            {
        //                ErrorInfo = "Sikertelen azonosítás, hibakód= " + ret;
        //                MessageChanged?.Invoke(this, new FPMessageChangedEventArgs(ErrorInfo, true, ret));
        //                return;
        //            }
        //        }
        //        else
        //        {
        //            int ret = ZkfpLinux.ZKFPM_DBMatch(mDBHandle, CapTmp, CapTmp.Length, RegTmp, RegTmp.Length,ref fid);
        //            if (0 < ret)
        //            {
        //                SuccessInfo = "Sikeres ujjlenyomat egyeztetés, score=" + ret + "!";
        //                MessageChanged?.Invoke(this, new FPMessageChangedEventArgs(SuccessInfo, false));
        //                return;
        //            }
        //            else
        //            {
        //                ErrorInfo = "Sikertelen ujjlenyomat egyeztetés, hibakód= " + ret;
        //                MessageChanged?.Invoke(this, new FPMessageChangedEventArgs(ErrorInfo, true, ret));
        //                return;
        //            }
        //        }
        //    }
        //}
        public async Task NewFingerprintAsync(int index)
        {
            if (index < 1 || index > 3)
            {
                ErrorInfo = "Érvénytelen index, csak 1-3 között lehet!";
                MessageChanged?.Invoke(this, new FPMessageChangedEventArgs(ErrorInfo, true, -1));
                return;
            }
            else if (index == 2)
            {
                iFid += 1000;
            }
            IsRegister = true;
            ProcessId = 0;
            cbRegTmp = 2048;
            if (mDevHandle == IntPtr.Zero)
            {
                await Open();
            }
        }
        public bool ClearDb()
        {
            try
            {
                if (mDBHandle != IntPtr.Zero)
                {
                    zkfp2.DBFree(mDBHandle);
                    mDBHandle = IntPtr.Zero;
                }
                mDBHandle = zkfp2.DBInit();
                SuccessInfo = "Sikeres adatbázis törlés";
                _logger.LogInformation(SuccessInfo);
                MessageChanged?.Invoke(this, new FPMessageChangedEventArgs(SuccessInfo, false));
                return true;
            }
            catch (Exception ex)
            {
                ErrorInfo = $"Hiba történt az adatbázis törlése során: {ex.Message}";
                _logger.LogError(ErrorInfo, ex);
                MessageChanged?.Invoke(this, new FPMessageChangedEventArgs(ErrorInfo, true));
                return false;
            }
        }
        public async Task<bool> AddFingerprintAsync(string fingerPrintBase64, int fId)
        {
            try
            {
                if (mDevHandle == IntPtr.Zero)
                {
                    await Open();
                }
                byte[] blob = zkfp2.Base64ToBlob(fingerPrintBase64);
                int fid = 0, score = 0;
                int ret = zkfp2.DBIdentify(mDBHandle, blob, ref fid, ref score);
                if (zkfp.ZKFP_ERR_OK == ret)
                {
                    ErrorInfo = "Ujjlenyomat már regisztrálva van, fid= " + fid + "!";
                    _logger.LogWarning(ErrorInfo, fid);
                    MessageChanged?.Invoke(this, new FPMessageChangedEventArgs(ErrorInfo, true, ret));
                    return true;
                }
                ret = zkfp2.DBAdd(mDBHandle, fId, blob);
                if (ret == zkfp.ZKFP_ERR_OK)
                {
                    FingerPrintRegistered?.Invoke(this, new FPRegisteredEventArgs(fId, fingerPrintBase64));
                    SuccessInfo = $"Sikeres adatbázis regisztráció FpId: {fId}";
                    _logger.LogInformation(SuccessInfo, fId);
                    //MessageChanged?.Invoke(this, new FPWinMessageChangedEventArgs(SuccessInfo, false, SUCCESS_DB_ADD));
                    if (fId < 1000)
                    {
                        iFid = fId + 1;
                    }
                    return true;
                }
                else
                {
                    ErrorInfo = "Sikertelen adatbázis regisztráció, hibakód=" + ret;
                    MessageChanged?.Invoke(this, new FPMessageChangedEventArgs(ErrorInfo, true, ret));
                    return false;
                }
            }
            catch (Exception ex)
            {
                ErrorInfo = $"Hiba történt az adatbázis regisztráció során: {ex.Message}";
                MessageChanged?.Invoke(this, new FPMessageChangedEventArgs(ErrorInfo, true));
                return false;
            }
        }
        public int Matching(List<FingerPrintData> fingerPrints)
        {
            int ret = 0, actualret=0;
            int iFid = 0;

            foreach (var fp in fingerPrints)
            {
                RegTmp = zkfp2.Base64ToBlob(fp.FingerTemplate1);
                ret = zkfp2.DBMatch(mDBHandle, CapTmp, RegTmp);
                if (0 < ret)
                {
                    if (actualret < ret)
                    {
                        actualret = ret;
                        iFid=fp.FpId;
                    }
                    //SuccessInfo = "Sikeres ujjlenyomat egyeztetés, score=" + ret + "!";
                    //MessageChanged?.Invoke(this, new FPMessageChangedEventArgs(SuccessInfo, false, ret));
                    //return fp.FpId;
                }
                else if (!string.IsNullOrWhiteSpace(fp.FingerTemplate2))
                {
                    RegTmp = zkfp2.Base64ToBlob(fp.FingerTemplate2);
                    ret = zkfp2.DBMatch(mDBHandle, CapTmp, RegTmp);
                    if (0 < ret)
                    {
                        if (actualret < ret)
                        {
                            actualret = ret;
                            iFid=fp.FpId;
                        }
                        //SuccessInfo = "Sikeres ujjlenyomat egyeztetés, score=" + ret + "!";
                        //MessageChanged?.Invoke(this, new FPMessageChangedEventArgs(SuccessInfo, false, ret));
                        //return fp.FpId;
                    }
                }
            }
            return iFid;
        }
        public bool DeviceConnected()
        {
            //_logger.LogInformation($"Checking device connection status... Device handle: {mDevHandle}");
            return mDevHandle != IntPtr.Zero;

        }
        public void SwitchIdentifyMode(bool identify)
        {
            if (identify)
            {
                ProcessId = 1;
            }
            else
            {
                ProcessId = 2;
            }
            bIdentify = identify;
        }
        public void Close()
        {
            bIsTimeToDie = true;
            if (captureThread != null && captureThread.IsAlive)
            {
                captureThread.Join();
            }
            if (mDBHandle != IntPtr.Zero)
            {
                zkfp2.DBFree(mDBHandle);
                mDBHandle = IntPtr.Zero;
            }
            if (mDevHandle != IntPtr.Zero)
            {
                zkfp2.CloseDevice(mDevHandle);
                mDevHandle = IntPtr.Zero;
            }
        }
        public void Dispose() => Close();

    }
}