using CefSharp;
using System;

namespace EcgAnalyseStart
{
    class CookieVisitor : ICookieVisitor
    {
        public Action<Cookie> SendCookie;
        public bool Visit(Cookie cookie, int count, int total, ref bool deleteCookie)
        {
            deleteCookie = false;
            if (SendCookie != null)
            {
                SendCookie(cookie);
            }
            return true;
        }
        public void Dispose()
        {

        }
    }
}
