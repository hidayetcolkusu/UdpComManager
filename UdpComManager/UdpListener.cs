using PackageManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace UdpComManager
{
    public class UdpListener
    {
        #region Events

        public delegate void NetworkPackageReceived(NetworkPackage package);
        public event NetworkPackageReceived PackageReceived;

        public delegate void NetworkBytesReceived(byte[] bytes);
        public event NetworkBytesReceived BytesPackageReceived;

        public delegate void NetworkTextReceived(string text);
        public event NetworkTextReceived TextReceived;

        #endregion

        #region Properties

        public string IpAddresses { get; set; }
        public int Port { get; set; }

        private UdpClient udp;
        private IAsyncResult ar_ = null;
        private NetworkPackageGenerator _packageGenerator;

        #endregion


        public UdpListener(NetworkPackageGenerator packageGenerator, string ipAddress, int port)
        {
            _packageGenerator = packageGenerator;
            IpAddresses = ipAddress;
            Port = port; 
        }

        public void Connect()
        {
            udp = new UdpClient(Port);
            StartReceive();
        }

        public void Disconnect()
        {
            udp = null;
        }

        public void Send(string message, string ipAddress, int port)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(message);
            SendBytes(bytes, ipAddress, port);
        }

        public void Send(byte[] bytes, string ipAddress, int port)
        {
            SendBytes(bytes, ipAddress, port);
        }

        public void Send(NetworkPackage package, string ipAddress, int port)
        {
            byte[] bytes = package.GenerateByteArray();
            SendBytes(bytes, ipAddress, port);
        }
         

        private void StartReceive()
        {
            ar_ = udp.BeginReceive(Receive, new object());
        }

        private void Receive(IAsyncResult ar)
        {
            IPEndPoint ip = new IPEndPoint(IPAddress.Parse(IpAddresses), Port);
            byte[] bytes = udp.EndReceive(ar, ref ip);
            Receive(bytes);
            StartReceive();
        }

        private void Receive(byte[] bytes)
        {
            ReceivePackage(bytes);
            ReceiveBytes(bytes);
            ReceiveText(bytes);
        }

        private void ReceivePackage(byte[] bytes)
        {
            try
            {
                NetworkPackage networkPackage = _packageGenerator.Generate(bytes);
                PackageReceived?.Invoke(networkPackage);
            }
            catch  
            {

            }
        }

        private void ReceiveBytes(byte[] bytes)
        {
            try
            {
                BytesPackageReceived?.Invoke(bytes);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void ReceiveText(byte[] bytes)
        {
            try
            {
                TextReceived?.Invoke(Encoding.ASCII.GetString(bytes));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        } 

        private void SendBytes(byte[] bytes, string ipAddress, int port)
        {
            UdpClient client = new UdpClient();
            IPEndPoint ip = new IPEndPoint(IPAddress.Parse(ipAddress), port);
            client.Send(bytes, bytes.Length, ip);
            client.Close();
        }

    }
}
