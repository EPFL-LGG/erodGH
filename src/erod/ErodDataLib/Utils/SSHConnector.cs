using System;
using Renci.SshNet;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace ErodDataLib.Utils
{
	public class SSHConnector
	{
        private readonly string _host;
        private readonly int _port;
        private readonly string _username;
        private readonly string _password;
        public SshClient _sshClient;
        private ConnectionInfo _connectionInfo;

        public SSHConnector(string host, int port, string username, string password)
        {
            _host = host;
            _port = port;
            _username = username;
            _password = password;
            _connectionInfo = new ConnectionInfo(_host, _username,
                    new AuthenticationMethod[]
                    {
                        new PasswordAuthenticationMethod(username, _password),
                    });
        }

        public string Connect()
        {
            try
            {
                _sshClient = new SshClient(_connectionInfo);
                _sshClient.Connect();
                return "Connection Established";
            }
            catch (Exception ex)
            {
                return ex.Message.ToString();
            }
        }

        public string ExecuteCommand(string command)
        {
            try
            {
                var sshCommand = _sshClient.RunCommand(command);

                // Read output stream
                if (sshCommand.Error.Length == 0) return sshCommand.Result;
                else throw new Exception(sshCommand.Error);
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public void Disconnect()
        {
            if (_sshClient != null && _sshClient.IsConnected)
            {
                _sshClient.Disconnect();
                _sshClient.Dispose();
            }
        }

        public bool ReceiveJsonFile(string remoteFilePath, string localFilePath)
        {
            try
            {
                using (var Sftp = new SftpClient(_connectionInfo))
                {
                    Sftp.Connect();

                    // Download the JSON file from the remote server
                    using (var fileStream = new FileStream(localFilePath, FileMode.Create))
                    {
                        Sftp.DownloadFile(remoteFilePath, fileStream);
                    }

                    Sftp.Disconnect();

                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void SendJsonObject(object data, string fileName)
        {
            // Create a temporary file on the server
            var tempFileName = Path.GetTempFileName();

            // Write the JSON to the temporary file
            using (var writer = new StreamWriter(tempFileName))
            {
                var serializedData = JsonConvert.SerializeObject(data);
                writer.Write(serializedData);
            }

            // Transfer the temporary file to the server
            using (var scp = new ScpClient(_connectionInfo))
            {
                scp.Connect();
                scp.Upload(new FileInfo(tempFileName), fileName);
                scp.Disconnect();
            }

            // Delete the temporary file
            File.Delete(tempFileName);
        }

        public static bool ReceiveJsonFileToClient(ConnectionInfo connectionInfo, string remoteFilePath, string localFilePath)
        {
            try
            {
                using (var Sftp = new SftpClient(connectionInfo))
                {
                    Sftp.Connect();

                    // Download the JSON file from the remote server
                    using (var fileStream = new FileStream(localFilePath, FileMode.Create))
                    {
                        Sftp.DownloadFile(remoteFilePath, fileStream);
                    }

                    Sftp.Disconnect();
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static void SendJsonObjectToClient(ConnectionInfo connectionInfo, object data, string fileName)
        {
            // Create a temporary file on the server
            var tempFileName = Path.GetTempFileName();

            // Write the JSON to the temporary file
            using (var writer = new StreamWriter(tempFileName))
            {
                var serializedData = JsonConvert.SerializeObject(data);
                writer.Write(serializedData);
            }

            // Transfer the temporary file to the server
            using (var scp = new ScpClient(connectionInfo))
            {
                scp.Connect();
                scp.Upload(new FileInfo(tempFileName), fileName);
                scp.Disconnect();
            }

            // Delete the temporary file
            File.Delete(tempFileName);
        }
    }
}

