using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using VATRP.Core.Events;

namespace VATRP.Core.Model.Utils
{
    public class RequestState
    {
// This class stores the State of the request.
        private const int BUFFER_SIZE = 1024;
        public byte[] BufferRead;
        public HttpWebRequest request;
        public HttpWebResponse response;
        public Stream streamResponse;
        public string destinationFile;
        public StringBuilder requestData;
        public FileStream fileStream;
        public bool fileDownload;
        public int MaxLogoSize;
        public HttpDownloadHelper downloaderObject;

        public RequestState()
        {
            BufferRead = new byte[BUFFER_SIZE];
            requestData = new StringBuilder("");
            request = null;
            streamResponse = null;
        }
    }

    public class HttpDownloadHelper
    {
        public static ManualResetEvent allDone = new ManualResetEvent(false);
        private const int BUFFER_SIZE = 1024;
        private const int _defaultTimeout = 10 * 1000; // 10 sec timeout
        
        public event EventHandler<HttpDownloadEventArgs> DownloadCompleted;
        public event EventHandler<HttpDownloadEventArgs> ImageSizeReceived;
        private HttpWebRequest webRequest;
        private bool cancelled;
        private bool timeout;
        private string _sourceURI = string.Empty;
        // Abort the request if the timer fires.
        private static void TimeoutCallback(object state, bool timedOut)
        {
            if (timedOut)
            {
                var downloadHelper = state as HttpDownloadHelper;
                if (downloadHelper != null)
                {
                    if (!downloadHelper.cancelled)
                        downloadHelper.timeout = true;
                    downloadHelper.webRequest.Abort();
                }
            }
        }


        public void CancelRequest()
        {
            if (webRequest != null)
            {
                cancelled = true;
                webRequest.Abort();
            }
        }

        public void RequestImageSize(string sourceUri, int maxLogoSize)
        {
            try
            {
                _sourceURI = sourceUri;
                // Create a HttpWebrequest object to the desired URL.
                webRequest = (HttpWebRequest)WebRequest.Create(sourceUri);

                // Create an instance of the RequestState and assign the previous myHttpWebRequest
                // object to it's request field.

                var myRequestState = new RequestState
                {
                    request = webRequest,
                    fileDownload = false,
                    downloaderObject = this,
                    MaxLogoSize = maxLogoSize
                };

                webRequest.Method = "HEAD";
                IAsyncResult result =
                    (IAsyncResult)webRequest.BeginGetResponse(new
                        AsyncCallback(RespCallback), myRequestState);

                ThreadPool.RegisterWaitForSingleObject(result.AsyncWaitHandle,
                    new WaitOrTimerCallback(TimeoutCallback), this,
                    _defaultTimeout, true);

                allDone.WaitOne();

                if (myRequestState.response != null)
                    myRequestState.response.Close();

                Debug.Print("Cancelled: " + this.cancelled + " Timeout: " + timeout);

                if (ImageSizeReceived != null)
                    ImageSizeReceived(this, new HttpDownloadEventArgs(this.timeout, this.cancelled) { Succeeded = !timeout && !cancelled });
            }
            catch (WebException e)
            {
                Console.WriteLine("\nWeb Exception raised!");
                Console.WriteLine("\nMessage:{0}", e.Message);
                Console.WriteLine("\nStatus:{0}", e.Status);
            }
            catch (Exception e)
            {
                Console.WriteLine("\nMain Exception raised!");
                Console.WriteLine("Source :{0} ", e.Source);
                Console.WriteLine("Message :{0} ", e.Message);

                if (ImageSizeReceived != null)
                    ImageSizeReceived(this,
                        new HttpDownloadEventArgs(this.timeout, this.cancelled) { Succeeded = false });
            }
        }

        public void DownloadImage(string sourceUri, string targetFile)
        {
            try
            {
                _sourceURI = sourceUri;
                // Create a HttpWebrequest object to the desired URL.
                webRequest = (HttpWebRequest) WebRequest.Create(sourceUri);

                var myRequestState = new RequestState
                {
                    request = webRequest,
                    fileDownload = true,
                    destinationFile = targetFile,
                    downloaderObject = this
                };

                IAsyncResult result =
                    (IAsyncResult) webRequest.BeginGetResponse(new
                        AsyncCallback(RespCallback), myRequestState);

                ThreadPool.RegisterWaitForSingleObject(result.AsyncWaitHandle,
                    new WaitOrTimerCallback(TimeoutCallback), this,
                    _defaultTimeout, true);

                allDone.WaitOne();

                if (myRequestState.response != null)
                    myRequestState.response.Close();

                Debug.Print("Cancelled: " + this.cancelled + " Timeout: " + timeout);

                if (timeout || cancelled)
                {
                    if (myRequestState.fileDownload && myRequestState.fileStream != null)
                    {
                        myRequestState.fileStream.Close();
                        if (File.Exists(myRequestState.destinationFile))
                            File.Delete(myRequestState.destinationFile);
                    }
                }

                if (DownloadCompleted != null)
                {
                    DownloadCompleted(this,
                        new HttpDownloadEventArgs(this.timeout, this.cancelled)
                        {
                            Succeeded = !timeout && !cancelled,
                            URI = _sourceURI
                        });
                }
            }
            catch (WebException e)
            {
                Console.WriteLine("\nWeb Exception raised!");
                Console.WriteLine("\nMessage:{0}", e.Message);
                Console.WriteLine("\nStatus:{0}", e.Status);
            }
            catch (Exception e)
            {
                Console.WriteLine("\nMain Exception raised!");
                Console.WriteLine("Source :{0} ", e.Source);
                Console.WriteLine("Message :{0} ", e.Message);

                if (DownloadCompleted != null)
                    DownloadCompleted(this,
                        new HttpDownloadEventArgs(this.timeout, this.cancelled) {Succeeded = false, URI = _sourceURI});
            }
        }

        private static void RespCallback(IAsyncResult asynchronousResult)
        {
            try
            {
                RequestState myRequestState = (RequestState)
                    asynchronousResult.AsyncState;
                HttpWebRequest myHttpWebRequest = myRequestState.request;
                myRequestState.response = (HttpWebResponse)
                    myHttpWebRequest.EndGetResponse(asynchronousResult);

                if (!myRequestState.fileDownload)
                {
                    foreach (var hdr in myRequestState.response.Headers)
                    {
                        if (string.Compare(hdr.ToString(), "Content-Length", StringComparison.OrdinalIgnoreCase) ==
                            0)
                        {
                            int size = 0;

                            if (int.TryParse(myRequestState.response.Headers[hdr.ToString()].ToString(), out size))
                            {
                                myRequestState.downloaderObject.cancelled = size > myRequestState.MaxLogoSize;
                            }
                            break;
                        }
                    }
                }
                else
                {
                    Stream responseStream = myRequestState.response.GetResponseStream();

                    myRequestState.streamResponse = responseStream;


                    if (responseStream != null)
                    {
                        if (myRequestState.fileDownload)
                        {
                            myRequestState.fileStream = new FileStream(myRequestState.destinationFile, FileMode.Create,
                                FileAccess.Write);

                        }
                        IAsyncResult asynchronousInputRead =
                            responseStream.BeginRead(myRequestState.BufferRead, 0, BUFFER_SIZE, new
                                AsyncCallback(ReadCallBack), myRequestState);
                    }
                    return;
                }
            }
            catch (WebException e)
            {
                Console.WriteLine("\nRespCallback Exception raised!");
                Console.WriteLine("\nMessage:{0}", e.Message);
                Console.WriteLine("\nStatus:{0}", e.Status);
            }
            allDone.Set();
        }

        private static void ReadCallBack(IAsyncResult asyncResult)
        {
            try
            {
                var myRequestState = (RequestState) asyncResult.AsyncState;
                Stream responseStream = myRequestState.streamResponse;
                int bytesRead = responseStream.EndRead(asyncResult);

                // Read the HTML page and then print it to the console.
                if (bytesRead > 0)
                {
                    if (myRequestState.fileDownload)
                    {
                        myRequestState.fileStream.Write(myRequestState.BufferRead, 0, bytesRead);
                    }
                    else
                    {
                        myRequestState.requestData.Append(Encoding.ASCII.GetString(myRequestState.BufferRead,
                            0, bytesRead));
                    }

                    IAsyncResult asynchronousResult = responseStream.BeginRead(
                        myRequestState.BufferRead, 0, BUFFER_SIZE, new
                            AsyncCallback(ReadCallBack), myRequestState);
                    return;
                }
                else
                {
                    if (myRequestState.fileDownload && myRequestState.fileStream != null)
                    {
                        try
                        {
                            myRequestState.fileStream.Close();
                            myRequestState.fileStream.Dispose();
                            myRequestState.fileStream = null;

                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("File close excepton: " + e.Message);
                        }
                    }

                    if (myRequestState.requestData.Length > 1)
                    {
                        var stringContent = myRequestState.requestData.ToString();
                        Console.WriteLine(stringContent);
                    }

                    responseStream.Close();
                }

            }
            catch (WebException e)
            {
                Console.WriteLine("\nReadCallBack Exception raised!");
                Console.WriteLine("\nMessage:{0}", e.Message);
                Console.WriteLine("\nStatus:{0}", e.Status);
            }
            allDone.Set();

        }
    }
}


//////////////////////////////////////////////////////////

