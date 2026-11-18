using CefSharp;
using CefSharp.Handler;
using CefSharp.WinForms;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Windows.Forms;

namespace EcgAnalyseStart
{
    public class ExtChromiumBrowser : ChromiumWebBrowser
    {
        public ExtChromiumBrowser() : base(address: null)
        {
            this.LifeSpanHandler = new CefLifeSpanHandler();
            this.DownloadHandler = new DownloadHandler();
            this.MenuHandler = new ContextMenuHandler();
            this.KeyboardHandler = new CustomKeyboardHandler(); // 设置键盘处理程序
            this.RequestHandler = new CustomRequestHandler();
        }

        public ExtChromiumBrowser(string url) : base(url)
        {
            this.LifeSpanHandler = new CefLifeSpanHandler();
        }

        public event EventHandler<NewWindowEventArgs> StartNewWindow;
        public event Action<IFrame, IRequest> RedirectToNewWindow;

        public void OnNewWindow(NewWindowEventArgs e)
        {
            StartNewWindow?.Invoke(this, e);
        }

        public void OnWindowRedirecting(IFrame currentFrame, IRequest request)
        {
            RedirectToNewWindow?.Invoke(currentFrame, request);
        }
    }

    public class CefLifeSpanHandler : CefSharp.ILifeSpanHandler
    {
        public CefLifeSpanHandler()
        {

        }

        public bool DoClose(IWebBrowser browserControl, CefSharp.IBrowser browser)
        {
            if (browser.IsDisposed || browser.IsPopup)
            {
                return false;
            }

            return true;
        }

        public void OnAfterCreated(IWebBrowser browserControl, IBrowser browser)
        {

        }

        public void OnBeforeClose(IWebBrowser browserControl, IBrowser browser)
        {
        }

        public bool OnBeforePopup(IWebBrowser browserControl, IBrowser browser, IFrame frame, string targetUrl, string targetFrameName, WindowOpenDisposition targetDisposition, bool userGesture,
            IPopupFeatures popupFeatures, IWindowInfo windowInfo, IBrowserSettings browserSettings, ref bool noJavascriptAccess, out IWebBrowser newBrowser)
        {
            ExtChromiumBrowser chromiumWebBrowser = (ExtChromiumBrowser)browserControl;

            chromiumWebBrowser.Invoke(new Action(() =>
            {
                NewWindowEventArgs e = new NewWindowEventArgs(windowInfo, browser, targetUrl);
                chromiumWebBrowser.OnNewWindow(e);
            }));

            newBrowser = null;
            return false;
        }
    }

    public class DownloadHandler : IDownloadHandler
    {
        public bool CanDownload(IWebBrowser chromiumWebBrowser, IBrowser browser, string url, string requestMethod)
        {
            //throw new NotImplementedException();
            return true;
        }

        public void OnBeforeDownload(IWebBrowser chromiumWebBrowser, IBrowser browser, DownloadItem downloadItem, IBeforeDownloadCallback callback)
        {
            if (!callback.IsDisposed)
            {
                using (callback)
                {
                    //callback.Continue(@"C:Users" + System.Security.Principal.WindowsIdentity.GetCurrent().Name + @"Downloads" + downloadItem.SuggestedFileName,
                    //    showDialog: true);
                    callback.Continue(downloadItem.SuggestedFileName, showDialog: true);
                }
            }
        }

        public void OnDownloadUpdated(IWebBrowser chromiumWebBrowser, IBrowser browser, DownloadItem downloadItem, IDownloadItemCallback callback)
        {
            if (downloadItem.IsComplete || downloadItem.IsCancelled)
            {
                if (browser.IsPopup && !browser.HasDocument)
                {
                    browser.GetHost().CloseBrowser(true);
                }
            }
        }
    }

    public class ContextMenuHandler : IContextMenuHandler
    {
        public void OnBeforeContextMenu(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IContextMenuParams parameters, IMenuModel model)
        {
            model.Clear(); //屏蔽右键菜单
        }

        // 其他未重写的方法留空
        public void OnContextMenuDismissed(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame) { }
        public bool OnContextMenuCommand(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IContextMenuParams parameters, CefMenuCommand commandId, CefEventFlags eventFlags)
        {
            return false;
        }

        public bool RunContextMenu(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IContextMenuParams parameters, IMenuModel model, IRunContextMenuCallback callback)
        {
            return false;
        }
    }

    public class CustomKeyboardHandler : IKeyboardHandler
    {
        public bool OnPreKeyEvent(IWebBrowser chromiumWebBrowser, IBrowser browser, KeyType type, int windowsKeyCode, int nativeKeyCode, CefEventFlags modifiers, bool isSystemKey, ref bool isKeyboardShortcut)
        {
            if (type == KeyType.RawKeyDown && (modifiers & CefEventFlags.ControlDown) != 0 && windowsKeyCode == (int)Keys.J)
            {
                browser.GetHost().ShowDevTools(); // 按下Ctrl + J时显示开发者工具
                return true;
            }

            return false;
        }

        // 其他未实现的方法
        public bool OnKeyEvent(IWebBrowser chromiumWebBrowser, IBrowser browser, KeyType type, int windowsKeyCode, int nativeKeyCode, CefEventFlags modifiers, bool isSystemKey)
        {
            return false;
        }
    }

    public class CustomRequestHandler : RequestHandler
    {
        protected override IResourceRequestHandler GetResourceRequestHandler(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, bool isNavigation, bool isDownload, string requestInitiator, ref bool disableDefaultHandling)
        {
            //if (!string.IsNullOrEmpty(request.ReferrerUrl) && isNavigation)
            if (!string.IsNullOrEmpty(request.ReferrerUrl) && request.Url.EndsWith("/login"))
            {
                // 处理超时重定向逻辑
                ExtChromiumBrowser extBrowser = (ExtChromiumBrowser)chromiumWebBrowser;
                extBrowser.Invoke(new Action(() =>
                {
                    extBrowser.OnWindowRedirecting(frame, request);
                }));
            }

            if (request.ResourceType == ResourceType.Script || request.ResourceType == ResourceType.Image
                || request.ResourceType == ResourceType.Stylesheet || request.ResourceType == ResourceType.FontResource)
            {
                return new CustomResourceRequestHandler();
            }

            return base.GetResourceRequestHandler(chromiumWebBrowser, browser, frame, request, isNavigation, isDownload, requestInitiator, ref disableDefaultHandling);
        }

        protected override bool OnBeforeBrowse(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, bool userGesture, bool isRedirect)
        {
            if (isRedirect)
            {
                // 处理超时重定向逻辑
                ExtChromiumBrowser extBrowser = (ExtChromiumBrowser)chromiumWebBrowser;
                extBrowser.Invoke(new Action(() =>
                {
                    extBrowser.OnWindowRedirecting(frame, request);
                }));
            }
            return base.OnBeforeBrowse(chromiumWebBrowser, browser, frame, request, userGesture, isRedirect);
        }

        public CefReturnValue OnBeforeResourceLoad(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, IRequestCallback callback)
        {

            return CefReturnValue.Continue;
        }
    }

    public class CustomResourceRequestHandler : ResourceRequestHandler
    {
        private static Dictionary<string, byte[]> _jsFiles = new Dictionary<string, byte[]>(); //内存缓存

        //protected override CefReturnValue OnBeforeResourceLoad(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, IRequestCallback callback)
        //{
        //    if (!request.IsReadOnly && request.ResourceType == ResourceType.Script)
        //    {        
        //        request.SetHeaderByName("Cache-Control", "public, max-age=31536000", true);
        //        request.SetHeaderByName("If-None-Match", "", true);
        //    }

        //    return base.OnBeforeResourceLoad(chromiumWebBrowser, browser, frame, request, callback);
        //}

        protected override IResourceHandler GetResourceHandler(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request)
        {
            if (request.ResourceType == ResourceType.Script || request.ResourceType == ResourceType.Image
                || request.ResourceType == ResourceType.Stylesheet || request.ResourceType == ResourceType.FontResource)
            {
                Uri reqUri = new Uri(request.Url);
                string fullName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "holter-bp\\public", reqUri.AbsolutePath.Replace('/', '\\').TrimStart('/', '\\'));
                FileInfo file = new FileInfo(fullName);
                if (file.Exists)
                {
                    byte[] fileData = null;
                    if (_jsFiles.ContainsKey(file.Name))
                    {
                        fileData = _jsFiles[file.Name];
                    }
                    else
                    {
                        fileData = File.ReadAllBytes(fullName);
                    }

                    MemoryStream stream = new MemoryStream(fileData);
                    NameValueCollection headers = new NameValueCollection
                    {
                        //{ "Cache-Control", "no-cache" },
                        //{ "Pragma", "no-cache" }
                        { "Cache-Control", "public, max-age=31536000" }
                    };

                    string mimeType = "";
                    if (request.ResourceType == ResourceType.Script)
                    {
                        mimeType = "application/javascript";
                    }
                    else if (request.ResourceType == ResourceType.Stylesheet)
                    {
                        mimeType = "text/css";
                    }
                    else if (request.ResourceType == ResourceType.FontResource)
                    {
                        mimeType = "font/" + file.Extension.TrimStart('.');
                    }
                    else if (request.ResourceType == ResourceType.Image)
                    {
                        mimeType = "image/" + file.Extension.TrimStart('.');
                    }
                    return new CustomResourceHandler(mimeType, stream, headers);
                }
            }
            return base.GetResourceHandler(chromiumWebBrowser, browser, frame, request);
        }
    }

    public class CustomResourceHandler : ResourceHandler
    {
        public CustomResourceHandler(string mimeType, Stream resourceStream, NameValueCollection headers)
        {
            MimeType = mimeType;
            Stream = resourceStream;
            Headers.Add(headers);

            // 返回 200 状态码
            StatusCode = 200;
            StatusText = "OK";
        }

        public override CefReturnValue ProcessRequestAsync(IRequest request, ICallback callback)
        {

            return base.ProcessRequestAsync(request, callback);
        }
    }


    public class NewWindowEventArgs : EventArgs
    {
        private IWindowInfo _windowInfo;
        public IWindowInfo WindowInfo
        {
            get { return _windowInfo; }
            set { value = _windowInfo; }
        }
        public string Url { get; set; }
        public IBrowser Browser { get; set; }

        public NewWindowEventArgs(IWindowInfo windowInfo, IBrowser browser, string url)
        {
            _windowInfo = windowInfo;
            this.Url = url;
            this.Browser = browser;
        }
    }
}
