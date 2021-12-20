namespace Kongrevsky.Utilities.Smtp.Models
 {
    #region << Using >>

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading.Tasks;

    #endregion

    internal class SmtpConnectorWithoutSsl : SmtpConnectorBase
    {
        #region Properties

        private Socket _socket = null;

        #endregion

        private const int MAX_ATTEMPTS_COUNT = 100;

        #region Constructors

        public SmtpConnectorWithoutSsl(string smtpServerAddress, int port) : base(smtpServerAddress, port)
        {
            var hostEntry = Dns.GetHostEntry(smtpServerAddress);
            var endPoint = new IPEndPoint(hostEntry.AddressList[0], port);
            try
            {
                this._socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                //try to connect and test the rsponse for code 220 = success
                this._socket.Connect(endPoint);
            }
            catch
            {
                if (this._socket != null) this._socket.Dispose();
                throw;
            }
        }

        #endregion

        ~SmtpConnectorWithoutSsl()
        {
            try
            {
                if (this._socket != null)
                {
                    this._socket.Close();
                    this._socket.Dispose();
                    this._socket = null;
                }
            }
            catch (Exception)
            {
                ;
            }
        }


        /// <summary>
        ///     Check the response from the server returns one of the passed <paramref name="expectedCodes"/>
        /// </summary>
        /// <param name="expectedCodes"></param>
        /// <param name="responseData"></param>
        /// <returns></returns>
        public override bool CheckResponse(int[] expectedCodes, out string responseData)
        {
            responseData = null;

            if (_socket == null)
            {
                responseData = "socket not available (null)";
                return false;
            }

            var currentAttemptIndex = 1;
            while (_socket.Available == 0)
            {
                System.Threading.Thread.Sleep(100);
                if (currentAttemptIndex++ > MAX_ATTEMPTS_COUNT)
                {
                    responseData = "not available data to read from the socket";
                    return false;
                }
            }
            byte[] responseArray = new byte[2048];
            _socket.Receive(responseArray, 0, _socket.Available, SocketFlags.None);
            responseData = Encoding.UTF8.GetString(responseArray);

            int responseCode = Convert.ToInt32(responseData.Substring(0, 3));
            return expectedCodes.Contains(responseCode);
        }

        /// <summary>
        ///     Check the response from the server returns one of the passed <paramref name="expectedCodes"/>
        /// </summary>
        /// <param name="expectedCodes"></param>
        /// <param name="responseData"></param>
        public async override Task<(bool IsSuccess, string responseData)> CheckResponseExAsync(params int[] expectedCodes)
        {
            if (_socket == null) {
                return (false, "socket not available (null)");
            }

            var currentAttemptIndex = 1;
            while (_socket.Available == 0)
            {
                System.Threading.Thread.Sleep(100);
                if (currentAttemptIndex++ > MAX_ATTEMPTS_COUNT)
                {
                    return (false, "not available data to read from the socket");
                }
            }

            List<ArraySegment<byte>> buffer = new List<ArraySegment<byte>> { new ArraySegment<byte>(new byte[_socket.ReceiveBufferSize]) };
            int receivedBytes = await _socket.ReceiveAsync(buffer, SocketFlags.None).ConfigureAwait(false);

            if (receivedBytes == 0)
            {
                return (false, "0 bytes returned from the socket");
            }

            byte[] responseArray = new byte[receivedBytes];
            Array.Copy(buffer.First().Array, 0, responseArray, 0, receivedBytes);
            var responseData = Encoding.UTF8.GetString(responseArray);

            int responseCode = Convert.ToInt32(responseData.Substring(0, 3));
            return (expectedCodes.Contains(responseCode), responseData);
        }

        public override void SendData(string data)
        {
            var dataArray = Encoding.UTF8.GetBytes(data);
            this._socket.Send(dataArray, 0, dataArray.Length, SocketFlags.None);
        }

        public override async Task SendDataAsync(string data)
        {
            if (_socket == null) {
                return;
            }
            ArraySegment<byte> dataArray = new ArraySegment<byte>(Encoding.UTF8.GetBytes(data));
            await _socket.SendAsync(dataArray, SocketFlags.None).ConfigureAwait(false);
        }
    }
}