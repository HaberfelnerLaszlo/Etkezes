using System;
using System.IO;
using System.Text;

namespace FingerPrintService
{
    /// <summary>
    /// Base64 ⇄ byte[] konverter, Data URI és URL-safe támogatással.
    /// Teljesen keresztplatformos (.NET 6/7/8).
    /// </summary>
    public static class Base64Converter
    {
        /// <summary>
        /// Sima vagy Data URI Base64-ből nyers bájtokra konvertál.
        /// Visszaadja a bájtokat és – ha volt Data URI – a MIME típust.
        /// </summary>
        public static (byte[] Bytes, string? Mime) ToBytes(string base64OrDataUri)
        {
            if (string.IsNullOrWhiteSpace(base64OrDataUri))
                throw new ArgumentException("Üres Base64 bemenet.", nameof(base64OrDataUri));

            string? mime = null;
            string raw = base64OrDataUri.Trim();

            // Data URI támogatás: data:<mime>;base64,<adat>
            const string prefix = "data:";
            if (raw.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                int comma = raw.IndexOf(',');
                if (comma <= 0)
                    throw new FormatException("Hibás Data URI (hiányzó vessző).");

                string header = raw.Substring(0, comma);
                raw = raw.Substring(comma + 1);

                // "data:image/png;base64"
                var parts = header.Split(';');
                if (parts.Length > 0 && parts[0].StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    mime = parts[0].Substring(prefix.Length);
            }

            // URL-safe normalizálás (- → +, _ → /) és padding pótlás (=)
            raw = raw.Replace('-', '+').Replace('_', '/');
            int mod4 = raw.Length % 4;
            if (mod4 != 0) raw = raw.PadRight(raw.Length + (4 - mod4), '=');

            byte[] bytes = Convert.FromBase64String(raw);
            return (bytes, mime);
        }

        /// <summary>
        /// Ugyanaz, mint a ToBytes, de kivétel helyett false-t ad vissza és null-okkal tér vissza hiba esetén.
        /// </summary>
        public static bool TryToBytes(string base64OrDataUri, out byte[]? bytes, out string? mime)
        {
            try
            {
                (bytes, mime) = ToBytes(base64OrDataUri);
                return true;
            }
            catch
            {
                bytes = null;
                mime = null;
                return false;
            }
        }

        /// <summary>
        /// Nyers bájtokból Base64 stringet készít.
        /// Opcionálisan URL-safe módot is használhat.
        /// </summary>
        public static string ToBase64(byte[] data, bool urlSafe = false, bool noPadding = false)
        {
            if (data is null) throw new ArgumentNullException(nameof(data));

            string b64 = Convert.ToBase64String(data);

            if (urlSafe)
            {
                b64 = b64.Replace('+', '-').Replace('/', '_');
            }

            if (noPadding)
            {
                b64 = b64.TrimEnd('=');
            }

            return b64;
        }

        /// <summary>
        /// Base64 Data URI előállítása (pl. image/png).
        /// </summary>
        public static string ToDataUri(byte[] data, string mime, bool urlSafe = false, bool noPadding = false)
        {
            if (string.IsNullOrWhiteSpace(mime)) throw new ArgumentException("Üres MIME.", nameof(mime));
            var b64 = ToBase64(data, urlSafe, noPadding);
            return $"data:{mime};base64,{b64}";
        }

        /// <summary>
        /// Base64 → memóriafolyam (read-only).
        /// </summary>
        public static MemoryStream ToStream(string base64OrDataUri)
        {
            var (bytes, _) = ToBytes(base64OrDataUri);
            return new MemoryStream(bytes, writable: false);
        }

        /// <summary>
        /// Memóriafolyam → Base64 (a stream elejétől a végéig olvas).
        /// </summary>
        public static string FromStream(Stream stream, bool urlSafe = false, bool noPadding = false)
        {
            if (stream is null) throw new ArgumentNullException(nameof(stream));
            if (stream.CanSeek) stream.Position = 0;
            using var ms = new MemoryStream();
            stream.CopyTo(ms);
            return ToBase64(ms.ToArray(), urlSafe, noPadding);
        }

        /// <summary>
        /// Szöveg (UTF-8) ⇄ Base64 segéd (gyors teszteléshez).
        /// </summary>
        public static string TextToBase64(string text, bool urlSafe = false, bool noPadding = false)
        {
            if (text is null) throw new ArgumentNullException(nameof(text));
            return ToBase64(Encoding.UTF8.GetBytes(text), urlSafe, noPadding);
        }

        public static string Base64ToText(string base64OrDataUri)
        {
            var (bytes, _) = ToBytes(base64OrDataUri);
            return Encoding.UTF8.GetString(bytes);
        }
    }
}
