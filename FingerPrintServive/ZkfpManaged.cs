using System;

namespace FingerPrintService
{
    public sealed class ZkfpManaged : IDisposable
    {
        private IntPtr _device = IntPtr.Zero;
        private IntPtr _db = IntPtr.Zero;
        private bool _initialized = false;

        public void Init()
        {
            if (_initialized) return;
            ZkfpNative.Initialize();
            int rc = ZkfpNative.Init();
            if (rc != 0) throw new InvalidOperationException($"ZKFPM_Init failed: {rc}");
            _initialized = true;
        }

        public void Terminate()
        {
            if (!_initialized) return;
            ZkfpNative.Terminate();
            _initialized = false;
        }

        public int GetDeviceCount() => ZkfpNative.GetDeviceCount();

        public void OpenDevice(int index = 0)
        {
            if (!_initialized) throw new InvalidOperationException("Not initialized");
            _device = ZkfpNative.OpenDevice(index);
            if (_device == IntPtr.Zero) throw new InvalidOperationException("OpenDevice returned null handle");
        }

        public void CloseDevice()
        {
            if (_device == IntPtr.Zero) return;
            ZkfpNative.CloseDevice(_device);
            _device = IntPtr.Zero;
        }

        public byte[] CaptureTemplate()
        {
            if (_device == IntPtr.Zero) throw new InvalidOperationException("Device not open");
            // Allocate typical buffers; adjust sizes to SDK docs.
            byte[] img = new byte[4096];
            byte[] tpl = new byte[1024];
            int tplLen = (int)tpl.Length;
            int rc = ZkfpNative.AcquireFingerprint(_device, img, tpl, ref tplLen);
            if (rc != 0) throw new InvalidOperationException($"AcquireFingerprint failed: {rc}");
            var result = new byte[tplLen];
            Array.Copy(tpl, result, tplLen);
            return result;
        }

        public void DBInit()
        {
            _db = ZkfpNative.DBInit();
            if (_db == IntPtr.Zero) throw new InvalidOperationException("DBInit failed");
        }

        public void DBFree()
        {
            if (_db == IntPtr.Zero) return;
            ZkfpNative.DBFree(_db);
            _db = IntPtr.Zero;
        }

        public void DBAdd(int fid, byte[] tpl)
        {
            if (_db == IntPtr.Zero) throw new InvalidOperationException("DB not initialized");
            int rc = ZkfpNative.DBAdd(_db, fid, tpl);
            if (rc != 0) throw new InvalidOperationException($"DBAdd failed: {rc}");
        }

        public (int fid, int score) DBIdentify(byte[] tpl)
        {
            if (_db == IntPtr.Zero) throw new InvalidOperationException("DB not initialized");
            int fid = 0;
            int score = 0;
            int rc = ZkfpNative.DBIdentify(_db, tpl, ref fid, ref score);
            if (rc != 0) throw new InvalidOperationException($"DBIdentify failed: {rc}");
            return (fid, score);
        }

        public void Dispose()
        {
            CloseDevice();
            DBFree();
            Terminate();
            GC.SuppressFinalize(this);
        }
    }
}