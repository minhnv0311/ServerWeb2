using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using System.Web.UI;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Infrastructure;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OAuth;
using VSDCompany.Models;
using VSDCompany.Providers;
using VSDCompany.Results;

//class LoginAccessViewModel
//{
//    public string UserName { get; set; }
//    public string AccessToken { get; set; }
//}
//public class LoginViewModel
//{
//    public string UserName { get; set; }
//    public string Password { get; set; }
    
//}



namespace VSDCompany.Controllers
{


   


    [Authorize]
    [RoutePrefix("api/Account")]
    public class AccountController : ApiController
    {

        //[AllowAnonymous]
        //[HttpPost, Route("/Token123")]
        //public IHttpActionResult Token(LoginViewModel login)
        //{
        //    ClaimsIdentity identity = new ClaimsIdentity("ActiveDirectory");
        //    //if (!_loginProvider.ValidateCredentials(login.UserName, login.Password, out identity))
        //    //{
        //    //    return BadRequest("Incorrect user or password");
        //    //}

        //    var ticket = new AuthenticationTicket(identity, new AuthenticationProperties());
        //    var currentUtc = new SystemClock().UtcNow;
        //    ticket.Properties.IssuedUtc = currentUtc;
        //    ticket.Properties.ExpiresUtc = currentUtc.Add(TimeSpan.FromMinutes(30));

        //    return Ok(new LoginAccessViewModel
        //    {
        //        UserName = login.UserName,
        //        AccessToken = Startup.OAuthOptions.AccessTokenFormat.Protect(ticket)
        //    });
        //}
        //https://stackoverflow.com/questions/20482375/use-active-directory-with-web-api-for-spa

        private const string LocalLoginProvider = "Local";
        private ApplicationUserManager _userManager;

        public AccountController()
        {
        }

        public AccountController(ApplicationUserManager userManager,
            ISecureDataFormat<AuthenticationTicket> accessTokenFormat)
        {
            UserManager = userManager;
            AccessTokenFormat = accessTokenFormat;
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? Request.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        public ISecureDataFormat<AuthenticationTicket> AccessTokenFormat { get; private set; }

        // GET api/Account/UserInfo
        [HostAuthentication(DefaultAuthenticationTypes.ExternalBearer)]
        [Route("UserInfo")]
        public UserInfoViewModel GetUserInfo()
        {
            ExternalLoginData externalLogin = ExternalLoginData.FromIdentity(User.Identity as ClaimsIdentity);

            return new UserInfoViewModel
            {
                Email = User.Identity.GetUserName(),
                HasRegistered = externalLogin == null,
                LoginProvider = externalLogin != null ? externalLogin.LoginProvider : null
            };
        }

        // POST api/Account/Logout
        [Route("Logout")]
        public IHttpActionResult Logout()
        {
            Authentication.SignOut(CookieAuthenticationDefaults.AuthenticationType);
            return Ok();
        }

        // GET api/Account/ManageInfo?returnUrl=%2F&generateState=true
        [Route("ManageInfo")]
        public async Task<ManageInfoViewModel> GetManageInfo(string returnUrl, bool generateState = false)
        {
            IdentityUser user = await UserManager.FindByIdAsync(User.Identity.GetUserId());

            if (user == null)
            {
                return null;
            }

            List<UserLoginInfoViewModel> logins = new List<UserLoginInfoViewModel>();

            foreach (IdentityUserLogin linkedAccount in user.Logins)
            {
                logins.Add(new UserLoginInfoViewModel
                {
                    LoginProvider = linkedAccount.LoginProvider,
                    ProviderKey = linkedAccount.ProviderKey
                });
            }

            if (user.PasswordHash != null)
            {
                logins.Add(new UserLoginInfoViewModel
                {
                    LoginProvider = LocalLoginProvider,
                    ProviderKey = user.UserName,
                });
            }

            return new ManageInfoViewModel
            {
                LocalLoginProvider = LocalLoginProvider,
                Email = user.UserName,
                Logins = logins,
                ExternalLoginProviders = GetExternalLogins(returnUrl, generateState)
            };
        }

        // POST api/Account/ChangePassword
        [Route("ChangePassword")]
        public async Task<IHttpActionResult> ChangePassword(ChangePasswordBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            IdentityResult result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId(), model.OldPassword,
                model.NewPassword);

            if (!result.Succeeded)
            {
                return GetErrorResult(result);
            }

            return Ok();
        }

        // POST api/Account/SetPassword
        [Route("SetPassword")]
        public async Task<IHttpActionResult> SetPassword(String Id, SetPasswordBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            String s = User.Identity.GetUserId();
            IdentityResult result = await UserManager.AddPasswordAsync(Id, model.NewPassword);

            if (!result.Succeeded)
            {
                return GetErrorResult(result);
            }

            return Ok();
        }

        // POST api/Account/ResetPassword
        //[Route("ResetPassword")]
        //public async Task<IHttpActionResult> ResetPassword(String Id, SetPasswordBindingModel model)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }

        //    IdentityResult result = await UserManager.AddPasswordAsync(User.Identity.GetUserId(), model.NewPassword);

        //    if (!result.Succeeded)
        //    {
        //        return GetErrorResult(result);
        //    }

        //    return Ok();
        //}
        [Route("ResetPassword")]
        public async Task<IHttpActionResult> ResetPassword(string id, SetPasswordBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            ApplicationDbContext context = new ApplicationDbContext();
            UserStore<ApplicationUser> store = new UserStore<ApplicationUser>(context);
            UserManager<ApplicationUser> UserManager = new UserManager<ApplicationUser>(store);
            String userId = id;
            String newPassword = model.NewPassword;
            String hashedNewPassword = UserManager.PasswordHasher.HashPassword(newPassword);
            ApplicationUser cUser = await store.FindByIdAsync(userId);
            await store.SetPasswordHashAsync(cUser, hashedNewPassword);
            await store.UpdateAsync(cUser);
            return Ok();
        }
        // POST api/Account/AddExternalLogin
        [Route("AddExternalLogin")]
        public async Task<IHttpActionResult> AddExternalLogin(AddExternalLoginBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Authentication.SignOut(DefaultAuthenticationTypes.ExternalCookie);

            AuthenticationTicket ticket = AccessTokenFormat.Unprotect(model.ExternalAccessToken);

            if (ticket == null || ticket.Identity == null || (ticket.Properties != null
                && ticket.Properties.ExpiresUtc.HasValue
                && ticket.Properties.ExpiresUtc.Value < DateTimeOffset.UtcNow))
            {
                return BadRequest("External login failure.");
            }

            ExternalLoginData externalData = ExternalLoginData.FromIdentity(ticket.Identity);

            if (externalData == null)
            {
                return BadRequest("The external login is already associated with an account.");
            }

            IdentityResult result = await UserManager.AddLoginAsync(User.Identity.GetUserId(),
                new UserLoginInfo(externalData.LoginProvider, externalData.ProviderKey));

            if (!result.Succeeded)
            {
                return GetErrorResult(result);
            }

            return Ok();
        }

        // POST api/Account/RemoveLogin
        [Route("RemoveLogin")]
        public async Task<IHttpActionResult> RemoveLogin(RemoveLoginBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            IdentityResult result;

            if (model.LoginProvider == LocalLoginProvider)
            {
                result = await UserManager.RemovePasswordAsync(User.Identity.GetUserId());
            }
            else
            {
                result = await UserManager.RemoveLoginAsync(User.Identity.GetUserId(),
                    new UserLoginInfo(model.LoginProvider, model.ProviderKey));
            }

            if (!result.Succeeded)
            {
                return GetErrorResult(result);
            }

            return Ok();
        }

        // GET api/Account/ExternalLogin
        [OverrideAuthentication]
        [HostAuthentication(DefaultAuthenticationTypes.ExternalCookie)]
        [AllowAnonymous]
        [Route("ExternalLogin", Name = "ExternalLogin")]
        public async Task<IHttpActionResult> GetExternalLogin(string provider, string error = null)
        {
            if (error != null)
            {
                return Redirect(Url.Content("~/") + "#error=" + Uri.EscapeDataString(error));
            }

            if (!User.Identity.IsAuthenticated)
            {
                return new ChallengeResult(provider, this);
            }

            ExternalLoginData externalLogin = ExternalLoginData.FromIdentity(User.Identity as ClaimsIdentity);

            if (externalLogin == null)
            {
                return InternalServerError();
            }

            if (externalLogin.LoginProvider != provider)
            {
                Authentication.SignOut(DefaultAuthenticationTypes.ExternalCookie);
                return new ChallengeResult(provider, this);
            }

            ApplicationUser user = await UserManager.FindAsync(new UserLoginInfo(externalLogin.LoginProvider,
                externalLogin.ProviderKey));

            bool hasRegistered = user != null;

            if (hasRegistered)
            {
                Authentication.SignOut(DefaultAuthenticationTypes.ExternalCookie);

                ClaimsIdentity oAuthIdentity = await user.GenerateUserIdentityAsync(UserManager,
                   OAuthDefaults.AuthenticationType);
                ClaimsIdentity cookieIdentity = await user.GenerateUserIdentityAsync(UserManager,
                    CookieAuthenticationDefaults.AuthenticationType);

                AuthenticationProperties properties = ApplicationOAuthProvider.CreateProperties(user.UserName);
                Authentication.SignIn(properties, oAuthIdentity, cookieIdentity);
            }
            else
            {
                IEnumerable<Claim> claims = externalLogin.GetClaims();
                ClaimsIdentity identity = new ClaimsIdentity(claims, OAuthDefaults.AuthenticationType);
                Authentication.SignIn(identity);
            }

            return Ok();
        }

        // GET api/Account/ExternalLogins?returnUrl=%2F&generateState=true
        [AllowAnonymous]
        [Route("ExternalLogins")]
        public IEnumerable<ExternalLoginViewModel> GetExternalLogins(string returnUrl, bool generateState = false)
        {
            IEnumerable<AuthenticationDescription> descriptions = Authentication.GetExternalAuthenticationTypes();
            List<ExternalLoginViewModel> logins = new List<ExternalLoginViewModel>();

            string state;

            if (generateState)
            {
                const int strengthInBits = 256;
                state = RandomOAuthStateGenerator.Generate(strengthInBits);
            }
            else
            {
                state = null;
            }

            foreach (AuthenticationDescription description in descriptions)
            {
                ExternalLoginViewModel login = new ExternalLoginViewModel
                {
                    Name = description.Caption,
                    Url = Url.Route("ExternalLogin", new
                    {
                        provider = description.AuthenticationType,
                        response_type = "token",
                        client_id = Startup.PublicClientId,
                        redirect_uri = new Uri(Request.RequestUri, returnUrl).AbsoluteUri,
                        state = state
                    }),
                    State = state
                };
                logins.Add(login);
            }

            return logins;
        }

        // POST api/Account/Register
        [AllowAnonymous]
        [Route("Register")]
        public async Task<IHttpActionResult> Register(RegisterBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = new ApplicationUser() { UserName = model.Email, Email = model.Email };
            user.UserName = model.UserName;
            user.Email = model.Email;
            user.PhoneNumber = model.PhoneNumber;

            IdentityResult result = await UserManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                return GetErrorResult(result);
            }

            return Ok();
        }

        // POST api/Account/RegisterExternal
        [OverrideAuthentication]
        [HostAuthentication(DefaultAuthenticationTypes.ExternalBearer)]
        [Route("RegisterExternal")]
        public async Task<IHttpActionResult> RegisterExternal(RegisterExternalBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var info = await Authentication.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return InternalServerError();
            }

            var user = new ApplicationUser() { UserName = model.Email, Email = model.Email };

            IdentityResult result = await UserManager.CreateAsync(user);
            if (!result.Succeeded)
            {
                return GetErrorResult(result);
            }

            result = await UserManager.AddLoginAsync(user.Id, info.Login);
            if (!result.Succeeded)
            {
                return GetErrorResult(result);
            }
            return Ok();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && _userManager != null)
            {
                _userManager.Dispose();
                _userManager = null;
            }

            base.Dispose(disposing);
        }

        #region Helpers

        private IAuthenticationManager Authentication
        {
            get { return Request.GetOwinContext().Authentication; }
        }

        private IHttpActionResult GetErrorResult(IdentityResult result)
        {
            if (result == null)
            {
                return InternalServerError();
            }

            if (!result.Succeeded)
            {
                if (result.Errors != null)
                {
                    foreach (string error in result.Errors)
                    {
                        ModelState.AddModelError("", error);
                    }
                }

                if (ModelState.IsValid)
                {
                    // No ModelState errors are available to send, so just return an empty BadRequest.
                    return BadRequest();
                }

                return BadRequest(ModelState);
            }

            return null;
        }

        private class ExternalLoginData
        {
            public string LoginProvider { get; set; }
            public string ProviderKey { get; set; }
            public string UserName { get; set; }

            public IList<Claim> GetClaims()
            {
                IList<Claim> claims = new List<Claim>();
                claims.Add(new Claim(ClaimTypes.NameIdentifier, ProviderKey, null, LoginProvider));

                if (UserName != null)
                {
                    claims.Add(new Claim(ClaimTypes.Name, UserName, null, LoginProvider));
                }

                return claims;
            }

            public static ExternalLoginData FromIdentity(ClaimsIdentity identity)
            {
                if (identity == null)
                {
                    return null;
                }

                Claim providerKeyClaim = identity.FindFirst(ClaimTypes.NameIdentifier);

                if (providerKeyClaim == null || String.IsNullOrEmpty(providerKeyClaim.Issuer)
                    || String.IsNullOrEmpty(providerKeyClaim.Value))
                {
                    return null;
                }

                if (providerKeyClaim.Issuer == ClaimsIdentity.DefaultIssuer)
                {
                    return null;
                }

                return new ExternalLoginData
                {
                    LoginProvider = providerKeyClaim.Issuer,
                    ProviderKey = providerKeyClaim.Value,
                    UserName = identity.FindFirstValue(ClaimTypes.Name)
                };
            }
        }

        private static class RandomOAuthStateGenerator
        {
            private static RandomNumberGenerator _random = new RNGCryptoServiceProvider();

            public static string Generate(int strengthInBits)
            {
                const int bitsPerByte = 8;

                if (strengthInBits % bitsPerByte != 0)
                {
                    throw new ArgumentException("strengthInBits must be evenly divisible by 8.", "strengthInBits");
                }

                int strengthInBytes = strengthInBits / bitsPerByte;

                byte[] data = new byte[strengthInBytes];
                _random.GetBytes(data);
                return HttpServerUtility.UrlTokenEncode(data);
            }
        }

        #endregion

        private Entities db = new Entities();
        [HttpPost]
        [Route("create")]
        public IHttpActionResult Actionser(VSDCompany.Models.UserProfile user)
        {
            user.Mobile = Commons.ConvertMobile(user.Mobile);
            dynamic objValidate = Validate(user);
            if (objValidate.response_data.Count > 0)
                return Ok(objValidate);



            var objUser = new ApplicationUser();
            objUser.UserName = user.UserName;
            objUser.Email = user.Email;
            objUser.PhoneNumber = user.Mobile;

            if (string.IsNullOrEmpty(user.Id))
            {
                IdentityResult resultUser = UserManager.Create(objUser, "aBc@123456");
                if (resultUser.Succeeded)
                {
                    user.Id = objUser.Id;
                    if (string.IsNullOrEmpty(user.Avatar))
                        user.Avatar = "/Uploads/Images/user_no_image.png";

                    db.UserProfiles.AddOrUpdate(user);
                    db.SaveChanges();
                    var result = new
                    {
                        response_code = "00",
                        response_data = user
                    };
                    return Ok(result);
                }
                return BadRequest();
            }
            else
            {
                db.UserProfiles.AddOrUpdate(user);
                db.SaveChanges();
                var result = new
                {
                    response_code = "00",
                    response_data = user
                };
                return Ok(result);
            }


        }

        [HttpGet]
        [Route("createAll")]
        public IHttpActionResult CreateUser()
        {
            try
            {
                var DV = db.Organizations.Where(x => x.Type == "DONVI").ToList();
                foreach (var dv in DV)
                {
                    var objUser = new ApplicationUser();
                    objUser.UserName = dv.FCode;
                    objUser.Email = dv.FCode + ".hagiang@moet.edu.vn";
                    var user = new UserProfile();
                    var gu = new Group_User();
                    IdentityResult resultUser = UserManager.Create(objUser, "aBc@123456");
                    if (resultUser.Succeeded)
                    {
                        user.Id = objUser.Id;
                        user.UserName = dv.FCode;
                        user.FullName = dv.FName;
                        user.DonVi = dv.FCode;
                        user.Provin = dv.Provin;
                        user.District = dv.Disctrict;
                        user.Ward = dv.Ward;
                        user.Address = dv.Address;
                        user.IsAdmin = false;
                        user.FIndex = 1;
                        user.FInUse = true;
                        user.FLevel = 1;
                        if (string.IsNullOrEmpty(user.Avatar))
                            user.Avatar = "/Uploads/Images/user_no_image.png";
                        db.UserProfiles.AddOrUpdate(user);

                        gu.FCode = Commons.GenerateID(db, "GU").FName;
                        gu.UserName = dv.FCode;
                        gu.CodeGroup = "USERS";
                        gu.FCreateTime = DateTime.Now;
                        gu.FUserCreate = "admin";
                        db.Group_User.AddOrUpdate(gu);

                        db.SaveChanges();
                    }
                }
                return Ok("OK");
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

        private dynamic Validate(VSDCompany.Models.UserProfile user)
        {

            List<dynamic> validate = new List<dynamic>();
            var item = db.UserProfiles.Where(x => x.UserName == user.UserName && x.Id != user.Id).SingleOrDefault();
            if (item != null)
            {
                validate.Add(new
                {
                    fieldName = "UserName",
                    errorMsg = "Tên đăng nhập đã tồn tại !",
                    isError = true
                });
            }

            if (string.IsNullOrEmpty(user.Email))
            {
                validate.Add(new
                {
                    fieldName = "Email",
                    errorMsg = "Email không được bỏ trống !",
                    isError = true
                });

            }
            item = db.UserProfiles.Where(x => x.Email == user.Email && x.Id != user.Id).SingleOrDefault();
            if (item != null)
            {
                validate.Add(new
                {
                    fieldName = "Email",
                    errorMsg = "Email đã tồn tại !",
                    isError = true
                });
            }
            Regex ValidEmailRegex = Commons.CreateValidEmailRegex();
            if (!ValidEmailRegex.IsMatch(user.Email))
            {
                validate.Add(new
                {
                    fieldName = "Email",
                    errorMsg = "Email không đúng định dạng !",
                    isError = true
                });
            }

            if (string.IsNullOrEmpty(user.Mobile))
            {
                validate.Add(new
                {
                    fieldName = "Mobile",
                    errorMsg = "Số điện thoại không được bỏ trống !",
                    isError = true
                });

            }
            item = db.UserProfiles.Where(x => x.Mobile == user.Mobile && x.Id != user.Id).SingleOrDefault();
            if (item != null)
            {
                validate.Add(new
                {
                    fieldName = "Mobile",
                    errorMsg = "Số điện thoại đã tồn tại !",
                    isError = true
                });
            }
            if (!Regex.Match(user.Mobile, @"^[1-9][\.\d]*(,\d+)?$").Success)
            {
                validate.Add(new
                {
                    fieldName = "Mobile",
                    errorMsg = "Số điện thoại không đúng !",
                    isError = true
                });
            }

            var result = new
            {
                response_code = "01",
                response_data = validate
            };
            return result;
        }

        [HttpGet]
        [Route("list")]
        public IHttpActionResult list_account(string code, string searchKey)
        {
            searchKey = searchKey == null ? "" : searchKey;
            var UP = db.UserProfiles.ToList();
            var GrU = db.Group_User.ToList();
            var O = db.Organizations.ToList();
            var user = UP.Where(x => x.UserName == User.Identity.Name).FirstOrDefault();
            if (string.IsNullOrEmpty(code))
            {
                var collMenu = user.Type == "AD_PGD" ? UP.Where(x => x.District == user.District && (x.UserName.ToLower().Contains(searchKey.ToLower()) || x.FullName.ToLower().Contains(searchKey.ToLower()) || String.IsNullOrEmpty(searchKey))).OrderBy(x => x.UserName).ToList() : UP.Where(x => (x.UserName.ToLower().Contains(searchKey.ToLower()) || x.FullName.ToLower().Contains(searchKey.ToLower()) || String.IsNullOrEmpty(searchKey))).OrderBy(x => x.UserName).ToList();
                return Ok(collMenu);
            }
            else
            {
                var Org = O.Where(x => x.FCode == code).FirstOrDefault();

                if (Org.Type == "PHONGBAN")
                {
                    var collMenu = UP.Where(x => x.Department == code && (x.UserName.ToLower().Contains(searchKey.ToLower()) || x.FullName.ToLower().Contains(searchKey.ToLower()) || String.IsNullOrEmpty(searchKey))).OrderBy(x => x.UserName).ToList();
                    foreach (var c in collMenu)
                    {
                        var GU = GrU.Where(y => y.UserName == c.UserName && y.CodeGroup == code).FirstOrDefault();
                        if (GU != null)
                        {
                            c.FIndex = 2;
                        }
                    }
                    return Ok(collMenu);
                }
                if (Org.Type == "DVNHOM")
                {
                    var Nhom = O.Where(x => x.FParent == code).ToList();
                    var collMenu = new List<UserProfile>();
                    foreach (var dv in Nhom)
                    {
                        var coll = user.Type == "AD_PGD" ? UP.Where(x => x.DonVi == dv.FCode && x.District == user.District && (x.UserName.ToLower().Contains(searchKey.ToLower()) || x.FullName.ToLower().Contains(searchKey.ToLower()) || String.IsNullOrEmpty(searchKey))).OrderBy(x => x.UserName).ToList() : UP.Where(x => x.DonVi == dv.FCode && (x.UserName.ToLower().Contains(searchKey.ToLower()) || x.FullName.ToLower().Contains(searchKey.ToLower()) || String.IsNullOrEmpty(searchKey))).OrderBy(x => x.UserName).ToList();
                        foreach (var c in coll)
                        {
                            var GU = GrU.Where(y => y.UserName == c.UserName && y.CodeGroup == code).FirstOrDefault();
                            if (GU != null)
                            {
                                c.FIndex = 2;
                            }
                            collMenu.Add(c);
                        }
                    }
                    return Ok(collMenu);
                }
                else
                {
                    var collMenu = UP.Where(x => x.DonVi == code && (x.UserName.ToLower().Contains(searchKey.ToLower()) || x.FullName.ToLower().Contains(searchKey.ToLower()) || String.IsNullOrEmpty(searchKey))).OrderBy(x => x.UserName).ToList();
                    foreach (var c in collMenu)
                    {
                        var GU = GrU.Where(y => y.UserName == c.UserName && y.CodeGroup == code).FirstOrDefault();
                        if (GU != null)
                        {
                            c.FIndex = 2;
                        }
                    }
                    return Ok(collMenu);
                }
            }

        }

        [HttpGet]
        [Route("profile")]
        public IHttpActionResult profile()
        {
            var user = User.Identity.Name;
            var profile = db.UserProfiles.Where(x => x.UserName == user).FirstOrDefault();
            return Ok(profile);
        }

        [HttpGet]
        [Route("listbygroup")]
        public IHttpActionResult list_account_by_group(string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                var collMenu = db.UserProfiles.OrderBy(x => x.UserName).ToList();
                return Ok(collMenu);
            }
            else
            {
                var collMenu = db.UserProfiles.OrderBy(x => x.UserName).ToList();
                foreach (var c in collMenu)
                {
                    var GU = db.Group_User.Where(y => y.UserName == c.UserName && y.CodeGroup == code).FirstOrDefault();
                    if (GU != null)
                    {
                        c.FIndex = 2;
                    }
                }
                return Ok(collMenu);
            }

        }
    }

}
