using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Configuration;

namespace FarmerBrothers.Data
{
    public class ServerDBContext: DbContext
    {
        public ServerDBContext(string sNameOrStringConnection)
        {
            new DbContext(GetConnection(sNameOrStringConnection));
        }

        public static string GetConnection(string nameStrg)
        {
            bool bFlag = false;
            var connection = Desencriptar(ConfigurationManager.ConnectionStrings[nameStrg].ConnectionString, out bFlag);
            if (bFlag)
                return connection;
            else
                return "";
        }
    }
}
