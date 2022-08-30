using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using ICSharpCode.SharpZipLib.Silverlight.Zip;

namespace xTrade
{
    public class UpdateClass
    {
        private readonly int _port = 8000;
        private readonly string _serverName = string.Empty;

        private const int BufferSize = 1024;
        internal event ResponseReceivedEventHandler ResponseReceived;

        static string _dataIn = String.Empty;

        private static int _endSize;
        private MemoryStream _ms;
        private static string _cmd;

        private const int TinfoLength = 50;

        private byte[] _toSend;

        public UpdateClass(string serverName, int portNumber)
        {
            if (String.IsNullOrWhiteSpace(serverName))
            {
                throw new ArgumentNullException("serverName");
            }

            if (portNumber < 0 || portNumber > 65535)
            {
                throw new ArgumentNullException("portNumber");
            }

            _serverName = serverName;
            _port = portNumber;
        }

        public void SendData(string data, byte[] buff)
        {
            if (String.IsNullOrWhiteSpace(data))
            {
                throw new ArgumentNullException("data");
            }


            _toSend = buff;
            _dataIn = data;

            var socketEventArg = new SocketAsyncEventArgs();

            var hostEntry = new DnsEndPoint(_serverName, _port);

            var sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socketEventArg.Completed += SocketEventArgCompleted;
            socketEventArg.RemoteEndPoint = hostEntry;

            socketEventArg.UserToken = sock;

            try
            {
                sock.ConnectAsync(socketEventArg);
            }
            catch (SocketException ex)
            {
                throw new SocketException(ex.ErrorCode);
            }

        }
        #region

        void SocketEventArgCompleted(object sender, SocketAsyncEventArgs e) 
        { 
            switch (e.LastOperation) 
            { 
                case SocketAsyncOperation.Connect: 
                    ProcessConnect(e); 
                    break; 
                case SocketAsyncOperation.Receive: 
                    ProcessReceive(e); 
                    break; 
                case SocketAsyncOperation.Send: 
                    ProcessSend(e); 
                    break; 
                default: 
                    throw new Exception("Invalid operation completed"); 
            } 
        }

        private void ProcessReceive(SocketAsyncEventArgs e)
        {
           
            if (e.SocketError == SocketError.Success) 
            {
                try
                {
                    var xsock = e.UserToken as Socket;

                    int bytesRead = e.BytesTransferred;

                    if (0 == bytesRead) throw new Exception("Ошибка получения данных");

                    if (0 == _endSize)
                    {
                        _endSize = BitConverter.ToInt32(e.Buffer, 0) - TinfoLength;

                        _cmd = Encoding.UTF8.GetString(e.Buffer, 4, TinfoLength);
                        _cmd = Regex.Replace(_cmd, " ", string.Empty);

                        string[] tmpcmd = _cmd.Split("=".ToCharArray(), StringSplitOptions.None);

                        var comparer = StringComparer.OrdinalIgnoreCase;

                        _cmd = (0 == comparer.Compare(tmpcmd[0], "cmd")) ? tmpcmd[1] : string.Empty;

                        _ms.Write(e.Buffer, TinfoLength + 4, bytesRead - TinfoLength - 4);
                    }
                    else
                    {
                        _ms.Write(e.Buffer, 0, bytesRead);
                    }

                    if (_endSize > _ms.Length)
                    {
                        if (xsock != null) xsock.ReceiveAsync(e);
                    }
                    else
                    {
                        var sock = e.UserToken as Socket;
                        if (sock != null)
                        {
                            sock.Shutdown(SocketShutdown.Send);
                            sock.Close();
                        }

                        var comparer = StringComparer.OrdinalIgnoreCase;

                        if (0 == comparer.Compare(_cmd, "getdatafile"))
                        {
                            _ms.Position = 0;

                            var xfileData = new byte[_ms.Length];
                            _ms.Read(xfileData, 0, xfileData.Length);

                            _ms.Position = 0;

                            using (var s = new ZipInputStream(_ms))
                            {
                                ZipEntry theEntry;
                                while ((theEntry = s.GetNextEntry()) != null)
                                {
                                    Console.WriteLine(theEntry.Name);

                                    string directoryName = Path.GetDirectoryName(theEntry.Name);
                                    string fileName = Path.GetFileName(theEntry.Name);

                                    using (var appStorage = IsolatedStorageFile.GetUserStoreForApplication())
                                    {
                                        if (!string.IsNullOrEmpty(directoryName))
                                        {
                                            if (!appStorage.DirectoryExists(directoryName))
                                                appStorage.CreateDirectory(directoryName);
                                        }

                                        if (fileName != String.Empty)
                                        {
                                            if (appStorage.FileExists(theEntry.Name))
                                                appStorage.DeleteFile(theEntry.Name);

                                            using (var file = appStorage.OpenFile(theEntry.Name, FileMode.CreateNew))
                                            {
                                                using (var writer = new StreamWriter(file))
                                                {
                                                    var data = new byte[2048];

                                                    using (var stream = new MemoryStream())
                                                    {
                                                        while (true)
                                                        {
                                                            int size = s.Read(data, 0, data.Length);
                                                            if (size > 0)
                                                            {
                                                                stream.Write(data, 0, size);
                                                            }
                                                            else
                                                            {
                                                                break;
                                                            }
                                                        }

                                                        stream.Position = 0;

                                                        using (var reader = new StreamReader(stream))
                                                        {
                                                            writer.Write(reader.ReadToEnd());
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            Deployment.Current.Dispatcher.BeginInvoke(() =>
                                                                          {
                                                                              var args = new ResponseReceivedEventArgs
                                                                                             {Response = "rcSuccess"};
                                                                              OnResponseReceived(args);
                                                                          });
                        }

                        if (0 == comparer.Compare(_cmd.Substring(0, 7), "recvreq"))
                        {
                            string[] recreqres = _cmd.Split(":".ToCharArray(), StringSplitOptions.None);

                            if (0 == comparer.Compare(recreqres[1], "DataIsCorrupted"))
                            {
                                Deployment.Current.Dispatcher.BeginInvoke(() =>
                                {
                                    MessageBox.Show("Ошибка отправки. Файл поврежден",
                                                    "Ошибка!",
                                                    MessageBoxButton.OK);

                                    var args = new ResponseReceivedEventArgs { Response = "rcFailed" };
                                    OnResponseReceived(args);
                                    args.IsError = true;
                                });
                            }
                            else
                            {
                                string[] posres = recreqres[1].Split("#".ToCharArray(), StringSplitOptions.None);

                                if (0 == comparer.Compare(posres[0], "OrderIsAccepted"))
                                {
                                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                                                                                  {
                                                                                      var args =
                                                                                          new ResponseReceivedEventArgs
                                                                                              {
                                                                                                  Response = "rcSuccess",
                                                                                                  Info = posres[1]
                                                                                              };
                                                                                      OnResponseReceived(args);
                                                                                  });
                                }

                                if (0 == comparer.Compare(posres[0], "OrderNotAccepted"))
                                {
                                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                                                                                  {
                                                                                      MessageBox.Show("Ошибка добавления заказа",
                                                                                      "Ошибка!",
                                                                                      MessageBoxButton.OK);

                                                                                      var args = new ResponseReceivedEventArgs { Response = "rcFailed" };
                                                                                      OnResponseReceived(args);
                                                                                      args.IsError = true;
                                                                                  });
                                }
                            }
                        }
                    }
                }
                catch (Exception exc)
                {
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        MessageBox.Show(exc.Message,
                                        "Ошибка!",
                                        MessageBoxButton.OK);

                        var args = new ResponseReceivedEventArgs { Response = "rcFailed" };
                        OnResponseReceived(args);
                        args.IsError = true;
                    });
                }
            } 
            else 
            {
                Exception exc = new SocketException((int)e.SocketError);

                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    MessageBox.Show(exc.Message, "Ошибка!", MessageBoxButton.OK);

                    var args = new ResponseReceivedEventArgs {Response = "rcFailed"};
                    OnResponseReceived(args);
                    args.IsError = true;
                });
            } 
        }

        private void OnResponseReceived(ResponseReceivedEventArgs e)
        {
            if (ResponseReceived != null)
                ResponseReceived(this,e);
        }
 
        private void ProcessSend(SocketAsyncEventArgs e) 
        { 
            if (e.SocketError == SocketError.Success) 
            {  
                var sock = e.UserToken as Socket;

                var nbuff = new byte[BufferSize];

                e.SetBuffer(nbuff, 0, BufferSize);

                if (sock != null) sock.ReceiveAsync(e);
            } 
            else 
            {
                var args = new ResponseReceivedEventArgs {Response = e.SocketError.ToString(), IsError = true};
                OnResponseReceived(args);
            } 
        }
 
        private void ProcessConnect(SocketAsyncEventArgs e) 
        { 
            if (e.SocketError == SocketError.Success) 
            {
                _ms = new MemoryStream {Position = 0};
                _endSize = 0;

                if (null != _toSend)
                {
                    byte[] buffer = Encoding.UTF8.GetBytes(_dataIn + "<EOF>");

                    var clientData = new byte[buffer.Length + _toSend.Length];

                    buffer.CopyTo(clientData, 0);
                    _toSend.CopyTo(clientData, buffer.Length);

                    e.SetBuffer(clientData, 0, clientData.Length);
                }
                else
                {
                    byte[] buffer = Encoding.UTF8.GetBytes(_dataIn + "<EOF>");
                    e.SetBuffer(buffer, 0, buffer.Length);
                }


                var sock = e.UserToken as Socket;
                if (sock != null) sock.SendAsync(e);
            } 
            else 
            {
                var args = new ResponseReceivedEventArgs {Response = e.SocketError.ToString(), IsError = true};
                OnResponseReceived(args);
            } 
        } 
        #endregion
    }


    public delegate void ResponseReceivedEventHandler(object sender, ResponseReceivedEventArgs e);

    public class ResponseReceivedEventArgs : EventArgs
    {
        public bool IsError { get; set; }
        public string Response { get; set; }
        public string Info { get; set; }
    }
}
