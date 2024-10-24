using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace ServiceApis.ISecurity
{
    public interface ISecurityClass
    {
        string EncryptPwd(string encryptString);
        string DecryptPwd(string cipherText);
        string EncryptbyId(int userId);
        int DecryptbyId(string encryptedUserId);

    }
}
