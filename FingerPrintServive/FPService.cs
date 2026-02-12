using libzkfpcsharp;
using System.Drawing;
using System.Text.RegularExpressions;
using System.IO;
namespace FingerPrintService
{
    public class FPService
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

        byte[][] RegTmps = new byte[3][];
        byte[] RegTmp = new byte[2048];
        byte[] CapTmp = new byte[2048];
        int cbCapTmp = 2048;
        int cbRegTmp = 0;
        int iFid = 1;
        Thread? captureThread = null;

        private int mfpWidth = 0;
        private int mfpHeight = 0;
        public event EventHandler<FPMessageChangedEventArgs>? MessageChanged;      
        public event EventHandler<FPRegisteredEventArgs>? FingerPrintRegistered;
        public FPService()
        {
            bool flowControl = DeviceInit();
            if (!flowControl)
            {
                return;
            }
        }

        private bool DeviceInit()
        {
            int ret = zkfperrdef.ZKFP_ERR_OK;
            if ((ret = zkfp2.Init()) == zkfperrdef.ZKFP_ERR_OK)
            {
                devCount = zkfp2.GetDeviceCount();
                if (devCount > 0)
                {
                    if (devCount > 1)
                    {
                        ErrorInfo = devCount + " darab eszköz csatlakoztatva!";
                        MessageChanged?.Invoke(this, new FPMessageChangedEventArgs(ErrorInfo, true));
                        return false;
                    }
                    SuccessInfo = "Eszköz csatlakoztatva, kész a használatra!";
                    MessageChanged?.Invoke(this, new FPMessageChangedEventArgs(SuccessInfo, false));
                }
            }
            else
            {
                zkfp2.Terminate();
                ErrorInfo = "Nincs csatlakoztatott eszköz!";
                MessageChanged?.Invoke(this, new FPMessageChangedEventArgs(ErrorInfo, true));
                return false;
            }

            return true;
        }

        /// <summary>
        /// Service destructor to ensure that the device handle is properly closed when the service is disposed of. This prevents resource leaks and ensures that the fingerprint device is released correctly.
        /// </summary>
        ~FPService()
        {
            if (mDevHandle != IntPtr.Zero)
            {
                zkfp2.CloseDevice(mDevHandle);
                mDevHandle = IntPtr.Zero;
            }
        }
        public void Open()
        {
            if (mDevHandle == IntPtr.Zero)
            {
                bool flowControl = DeviceInit();
                if (!flowControl)
                {
                    return;
                }
            }

            int ret = zkfp.ZKFP_ERR_OK;
            if (IntPtr.Zero == (mDevHandle = zkfp2.OpenDevice(0)))
            {
                ErrorInfo = "Eszköz megnyitása sikertelen!";
                MessageChanged?.Invoke(this, new FPMessageChangedEventArgs(ErrorInfo, true));
                return;
            }
            if (IntPtr.Zero == (mDBHandle = zkfp2.DBInit()))
            {
                ErrorInfo = "DB inicializálása sikertelen!";
                MessageChanged?.Invoke(this, new FPMessageChangedEventArgs(ErrorInfo, true));
                zkfp2.CloseDevice(mDevHandle);
                mDevHandle = IntPtr.Zero;
                return;
            }
            RegisterCount = 0;
            cbRegTmp = 0;
            iFid = 1;
            for (int i = 0; i < 3; i++)
            {
                RegTmps[i] = new byte[2048];
            }
            byte[] paramValue = new byte[4];
            int size = 4;
            zkfp2.GetParameters(mDevHandle, 1, paramValue, ref size);
            zkfp2.ByteArray2Int(paramValue, ref mfpWidth);

            size = 4;
            zkfp2.GetParameters(mDevHandle, 2, paramValue, ref size);
            zkfp2.ByteArray2Int(paramValue, ref mfpHeight);

            FPBuffer = new byte[mfpWidth * mfpHeight];

            captureThread = new Thread(new ThreadStart(DoCapture));
            captureThread.IsBackground = true;
            captureThread.Start();
            bIsTimeToDie = false;

        }
        private void DoCapture()
        {
            while (!bIsTimeToDie)
            {
                cbCapTmp = 2048;
                int ret = zkfp2.AcquireFingerprint(mDevHandle, FPBuffer, CapTmp, ref cbCapTmp);
                if (ret == zkfp.ZKFP_ERR_OK)
                {
                    // Process the captured fingerprint data (FPBuffer) as needed
                    Register();
                }
                //else
                //{
                //    ErrorInfo = "Ujjlenyomat rögzítése sikertelen, hibakód= " + ret;
                //    MessageChanged?.Invoke(this, new FPMessageChangedEventArgs(ErrorInfo, true));
                //}
                Thread.Sleep(200);
            }
        }
        private void Register()
        {
            //MemoryStream ms = new MemoryStream();
            //BitmapFormat.GetBitmap(FPBuffer, mfpWidth, mfpHeight, ref ms);
            //Bitmap bmp = new Bitmap(ms);
            if (IsRegister)
            {
                int ret = zkfp.ZKFP_ERR_OK;
                int fid = 0, score = 0;
                ret = zkfp2.DBIdentify(mDBHandle, CapTmp, ref fid, ref score);
                if (zkfp.ZKFP_ERR_OK == ret)
                {
                    ErrorInfo = "Ujjlenyomat már regisztrálva van, fid= " + fid + "!";
                    MessageChanged?.Invoke(this, new FPMessageChangedEventArgs(ErrorInfo, true));
                    return;
                }
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
                        string fingerTmpBase64 = zkfp2.BlobToBase64(RegTmp, cbRegTmp); 
                        FingerPrintRegistered?.Invoke(this, new FPRegisteredEventArgs(iFid, fingerTmpBase64));
                        iFid++;
                        SuccessInfo = "Sikeres regisztráció";
                        MessageChanged?.Invoke(this, new FPMessageChangedEventArgs(SuccessInfo, false));
                    }
                    else
                    {
                        ErrorInfo = "Sikertelen regisztráció, hibakód=" + ret;                                    
                        MessageChanged?.Invoke(this, new FPMessageChangedEventArgs(ErrorInfo, true));
                    }
                    IsRegister = false;
                    return;
                }
                else
                {
                    SuccessInfo = "Sikeres regisztráció, érintse meg a szennert még " + (REGISTER_FINGER_COUNT - RegisterCount) + " ujjlenyomat szükséges";
                    MessageChanged?.Invoke(this, new FPMessageChangedEventArgs(SuccessInfo, false));
                }
            }
            else
            {
                if (cbRegTmp <= 0)
                {
                    ErrorInfo = "Kérem, először regisztrálja az ujjlenyomatát!";
                    MessageChanged?.Invoke(this, new FPMessageChangedEventArgs(ErrorInfo, true));
                    IsRegister = true;
                    return;
                }
                if (bIdentify)
                {
                    int ret = zkfp.ZKFP_ERR_OK;
                    int fid = 0, score = 0;
                    ret = zkfp2.DBIdentify(mDBHandle, CapTmp, ref fid, ref score);
                    if (zkfp.ZKFP_ERR_OK == ret)
                    {
                        SuccessInfo = "Sikeres azonosítás, fid= " + fid + ",score=" + score + "!";
                        MessageChanged?.Invoke(this, new FPMessageChangedEventArgs(SuccessInfo, false));
                        return;
                    }
                    else
                    {
                        ErrorInfo = "Sikertelen azonosítás, hibakód= " + ret;
                        MessageChanged?.Invoke(this, new FPMessageChangedEventArgs(ErrorInfo, true));
                        return;
                    }
                }
                else
                {
                    int ret = zkfp2.DBMatch(mDBHandle, CapTmp, RegTmp);
                    if (0 < ret)
                    {
                        SuccessInfo = "Sikeres ujjlenyomat egyeztetés, score=" + ret + "!";                    
                        MessageChanged?.Invoke(this, new FPMessageChangedEventArgs(SuccessInfo, false));
                        return;
                    }
                    else
                    {
                        ErrorInfo = "Sikertelen ujjlenyomat egyeztetés, hibakód= " + ret;                         
                        MessageChanged?.Invoke(this, new FPMessageChangedEventArgs(ErrorInfo, true));
                        return;
                    }
                }
            }
        }
        public void NewFingerprint()
        {
            IsRegister = true;
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
    }
    public class FPMessageChangedEventArgs : EventArgs
    {
        public string Message { get; private set; }
        public bool IsError { get; private set; }
        public FPMessageChangedEventArgs(string message, bool isError)
        {
            Message = message;
            IsError = isError;
        }
    }
    public class FPRegisteredEventArgs : EventArgs 
    {
        public int Fid { get; private set; } 
        public string FingerPrint { get; private set; }
        public FPRegisteredEventArgs(int fid, string fingerPrint)
        {
            Fid = fid;
            FingerPrint = fingerPrint;
        }
    }
}