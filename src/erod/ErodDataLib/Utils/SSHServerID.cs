using System;
using GH_IO.Serialization;
using Grasshopper.Kernel.Types;

namespace ErodDataLib.Utils
{
	public struct SSHServerID : IGH_Goo
    {
        public string Host;
        public int Port;
        public string Username;
        public string Password;
        public string RunFolder;
        public string CondaEnvironment;

        public SSHServerID(string host, int port, string username, string password, string runFolder, string condaEnvironment)
		{
            Host = host;
            Port = port;
            Username = username;
            Password = password;
            RunFolder = runFolder;
            CondaEnvironment = condaEnvironment;
		}

        #region GH_Methods
        public bool IsValid => true;

        public string IsValidWhyNot => "";

        public string TypeName => ToString();

        public string TypeDescription => "RemoteServer [" + Host + "]";

        public IGH_Goo Duplicate()
        {
            return (IGH_Goo)this.MemberwiseClone();
        }

        public IGH_GooProxy EmitProxy()
        {
            return null;
        }

        public bool CastFrom(object source)
        {
            return false;
        }

        public bool CastTo<T>(out T target)
        {
            target = default(T);
            return false;
        }

        public object ScriptVariable()
        {
            return null;
        }

        public bool Write(GH_IWriter writer)
        {
            return false;
        }

        public bool Read(GH_IReader reader)
        {
            return false;
        }
        #endregion
    }
}

