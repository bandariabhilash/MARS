using ServiceApis.IRepository;
using DataAccess.Db;
using ServiceApis.ISecurity;

namespace ServiceApis.Repository
{
    public class AuthRepository : IAuthRepository
    {
        private readonly FBContext _context;
        private readonly ISecurityClass _securityClass;

        public AuthRepository(FBContext context,ISecurityClass securityClass)
        {
            _context = context;
            _securityClass = securityClass;
        }
        public FbUserMaster GetUser(string username)
        {
            return _context.FbUserMasters.Where(x => x.Email.ToUpper() == username.ToUpper()).FirstOrDefault();
        }
        public bool CheckUser(string username)
        {
            return _context.FbUserMasters.Any(x => x.Email == username);
        }
        public bool CheckPassword(string username, string password)
        {
            return _context.FbUserMasters.Any(x => x.Email == username && x.Password == password);
        }
        public void UpdateUser(FbUserMaster user)
        {
            _context.FbUserMasters.Update(user);
            _context.SaveChanges();
        }

        public void UpdateRefreshToken(UserRefreshtoken refreshToken)
        {
            _context.UserRefreshtokens.Add(refreshToken);
            _context.SaveChanges();
        }

        public UserRefreshtoken getWhuserRefreshtoken(string refreshtoken)
        {
            return _context.UserRefreshtokens.Where(x => x.Refreshtoken == refreshtoken).FirstOrDefault();
        }

      

       

       
        
  
    }

}

