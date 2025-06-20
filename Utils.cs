using System;
using System.Drawing;
using System.IO;

namespace TrayPasswordGenerator
{
    internal static class Utils
    {
        // 16×16 ключ-иконка, сгенерирована и закодирована в base64
        private const string ICON_BASE64 =
            "AAABAAEAEBAAAAAAIABoBAAAFgAAACgAAAAgAAAAQAAAAAEABAAAAAAAQAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA" +
            "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA" +
            "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA" +
            "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA//8AAP//AAD//wAA//8AAP//" +
            "AAD//wAA//8AAP//AAD//wAA//8AAP//AAD//wAA//8AAP//AAD//wAA//8AAP//AAD//wAA//8AAP//AAD//wAA//8A" +
            "AP//AAD//wAA//8AAP//AAD//wAA//8AAP//AAD//wAA//8AAP//AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA";

        public static Icon LoadIcon()
        {
            byte[] bytes = Convert.FromBase64String(ICON_BASE64);
            using var ms = new MemoryStream(bytes);
            return new Icon(ms);
        }
    }
}
