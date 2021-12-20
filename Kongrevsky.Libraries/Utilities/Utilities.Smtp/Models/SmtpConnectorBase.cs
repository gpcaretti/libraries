namespace Kongrevsky.Utilities.Smtp.Models
{
    #region << Using >>

    using System.Threading.Tasks;

    #endregion

    internal abstract class SmtpConnectorBase
    {
        #region Constants

        public const string EOF = "\r\n";

        #endregion

        #region Properties

        protected string SmtpServerAddress { get; set; }

        protected int Port { get; set; }

        #endregion

        #region Constructors

        protected SmtpConnectorBase(string smtpServerAddress, int port)
        {
            SmtpServerAddress = smtpServerAddress;
            Port = port;
        }

        #endregion

        #region Abstracts

        /// <summary>
        ///     Detects if response is valid by checking it returns one of the passed <paramref name="expectedCodes"/>
        /// </summary>
        public abstract bool CheckResponse(int[] expectedCodes, out string responseData);

        /// <summary>
        ///     Detects if response is valid by checking it returns one of the passed <paramref name="expectedCodes"/>
        /// </summary>
        public abstract Task<(bool IsSuccess, string responseData)> CheckResponseExAsync(params int[] expectedCodes);

        public abstract void SendData(string data);

        public abstract Task SendDataAsync(string data);

        #endregion Abstracts

        #region Virtuals with default implementations

        /// <summary>
        ///     Detects if response is valid by checking it returns one of the passed <paramref name="expectedCodes"/>
        /// </summary>
        /// <param name="expectedCodes">expected condes</param>
        public virtual bool CheckResponse(params int[] expectedCodes)
        {
            return CheckResponse(expectedCodes, out _);
        }

        /// <summary>
        ///     Detects if response is valid by checking it returns the passed <paramref name="expectedCode"/>
        /// </summary>
        /// <param name="expectedCode"></param>
        public virtual bool CheckResponse(int expectedCode, out string responseData)
        {
            return CheckResponse(new int[] { expectedCode }, out responseData);
        }

        /// <summary>
        ///     Detects if response is valid by checking it returns one of the passed <paramref name="expectedCodes"/>
        /// </summary>
        /// <param name="expectedCodes">expected condes</param>
        public virtual async Task<bool> CheckResponseAsync(params int[] expectedCodes)
        {
            return (await CheckResponseExAsync(expectedCodes).ConfigureAwait(false)).IsSuccess;
        }

        #endregion Virtuals with default implementations
    }
}