
using Microsoft.Ajax.Utilities;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Newtonsoft.Json;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.UI.WebControls;
using VSDCompany.Models;

namespace VSDCompany.Controllers
{



    public class Navigation
    {
        public List<dynamic> compact { get; set; }
        [JsonProperty("default")]
        public List<dynamic> _default { get; set; }
        public List<dynamic> futuristic { get; set; }
        public List<dynamic> horizontal { get; set; }

    }
    public class IsActiveMatchOptions
    {

        public string matrixParams { get; set; } // 'exact' | 'subset' | 'ignored';
        public string queryParams { get; set; } //'exact' | 'subset' | 'ignored';
        public string paths { get; set; } // 'exact' | 'subset';
        public string fragment { get; set; } //exact' | 'ignored';
    }
    public class Badge
    {
        public string title { get; set; }
        public string classes { get; set; }
    }


    public class FuseNavigationItem
    {
        public string Id { get; set; }
        public string title { get; set; }
        public string subtitle { get; set; }

        public string type { get; set; } //| 'aside' | 'basic'| 'collapsable' | 'divider'| 'group'| 'spacer'
        public bool hidden { get; set; }
        public bool active { get; set; }
        public bool disabled { get; set; }
        public string link { get; set; }
        public bool externalLink { get; set; }
        public string target { get; set; } // | '_blank' | '_self' | '_parent' | '_top' | string
        public bool exactMatch { get; set; }
        public IsActiveMatchOptions isActiveMatchOptions { get; set; }

        public string icon { get; set; }
        public Badge badge { get; set; }
        public List<FuseNavigationItem> children { get; set; }

        public object meta { get; set; }

    }


    public class Message
    {
        public string id { get; set; }
        public string icon { get; set; }
        public string image { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public DateTime time { get; set; }
        public string link { get; set; }
        public bool useRouter { get; set; }
        public bool read { get; set; }
    }

    public class Notification
    {
        public string id { get; set; }
        public string icon { get; set; }
        public string image { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public DateTime time { get; set; }
        public string link { get; set; }
        public bool useRouter { get; set; }
        public bool read { get; set; }
    }
    public class Shortcut
    {
        public string id { get; set; }
        public string label { get; set; }
        public string description { get; set; }
        public string icon { get; set; }
        public string link { get; set; }
        public bool useRouter { get; set; }

    }
    public class User
    {
        public string id { get; set; }
        public string name { get; set; }
        public string email { get; set; }
        public string avatar { get; set; }
        public string status { get; set; }
    }


    [Authorize]
    //[EnableCors(origins: "http://localhost:4200", headers: "*", methods: "*")]
    public class CommonsController : ApiController
    {
        private Entities db = new Entities();

        [HttpGet]
        [AllowAnonymous]
        [Route("api/common/checkurl")]
        public IHttpActionResult checkurl(string url)
        {
            var Menu = db.Menus.Where(x => x.link == url).FirstOrDefault();
            if(Menu == null)
            {
                var result = new
                {
                    response_code = "00",
                    response_data = "Access"
                };
                return Ok(result);
            }
            var codeAccess = Menu.FCode;

            IdentityUser user = UserManager.FindById(User.Identity.GetUserId());
            var oUser = db.UserProfiles.Where(x => x.Id == user.Id).FirstOrDefault();
            // Lấy danh sách nhóm
            var collGropus = db.Group_User.Where(x => x.UserName == oUser.UserName).Select(x => x.CodeGroup).Distinct().ToList();

            //Kiểm tra quyền truy cập

            var MenuAccess = db.Group_Menu.Where(x => collGropus.Contains(x.CodeGroup) && x.CodeMenu == codeAccess).ToList();
            if(MenuAccess.Count>0)
            {
                var result = new
                {
                    response_code = "00",
                    response_data = "Access"
                };
                return Ok(result);
            }
            else
            {
                var result = new
                {
                    response_code = "01",
                    response_data = "No Access"
                };
                return Ok(result);
            }

        }

        [HttpGet]
        [Route("api/common/navigation")]
        public IHttpActionResult navigation()
        {

            // Lấy danh sách chức năng được phép truy cập
            IdentityUser user = UserManager.FindById(User.Identity.GetUserId());
            var oUser = db.UserProfiles.Where(x => x.Id == user.Id).FirstOrDefault();

            // Lấy danh sách nhóm
            var collGropus = db.Group_User.Where(x => x.UserName == oUser.UserName).Select(x => x.CodeGroup).Distinct().ToList();
            //Lấy danh sách menu
            var collMenus = db.Group_Menu.Where(x => collGropus.Contains(x.CodeGroup)).Select(x => x.CodeMenu).Distinct().ToList();

            Navigation MenuMain = new Navigation();
            var allMenu = db.Menus.Where(x => collMenus.Contains(x.FCode)).ToList();

            var collMenu = allMenu.Where(x => x.parent == "" || x.parent == null).OrderBy(x => x.FIndex).ToList();
            List<dynamic> tree = new List<dynamic>();
            List<dynamic> tree_compact = new List<dynamic>();
            foreach (var m in collMenu)
            {
                var oMenu = new
                {
                    name = m.title,
                    m.subtitle,
                    m.FCode,
                    m.icon,
                    id = m.FCode,
                    m.title,
                    m.hidden,
                    m.active,
                    m.disabled,
                    m.link,
                    m.externalLink,
                    m.target,
                    m.type,
                    m.FIndex,
                   
                    children = getChildMenu(m.FCode, oUser.DonVi, allMenu),

                };
                tree.Add(oMenu);
                var oMenuCompact = new
                {
                    m.icon,
                    id = m.FCode,
                    m.title,
                    type = "aside",
                    m.FIndex,
                    children = getChildMenu(m.FCode, oUser.DonVi, allMenu),
                };
                tree_compact.Add(oMenuCompact);

            }
            MenuMain._default = new List<dynamic>();
            MenuMain.compact = new List<dynamic>();
            
            MenuMain._default = tree;
            MenuMain.compact = tree_compact;
            MenuMain.futuristic = tree;
            MenuMain.horizontal = tree;
            return Ok(MenuMain);

        }
        public class Badge
        {
            public string title { get; set; }
            public string classes { get; set; }
        }
        private List<dynamic> getChildMenu(string parent,string DonVi,List<Models.Menu> allMenu)
        {
            var DV_User = db.UserProfiles.Where(x => x.UserName == User.Identity.Name).Select(y => y.DonVi).FirstOrDefault();
            var HSTS = db.HO_SO_TUYENSINH.Where(x => x.FInUse == true && x.MaDonVi == DV_User).OrderByDescending(y => y.Id).ToList();
            var NamHoc = HSTS.Select(z => z.NamHoc).FirstOrDefault();
            var collMenu = allMenu.Where(x => x.parent == parent).OrderBy(x => x.FIndex).ToList();
            List<dynamic> tree = new List<dynamic>();
            if (collMenu.Count > 0)
            {
                foreach (var m in collMenu)
                {
                    var Count = new Badge()
                    {
                        title = "",
                        classes = ""
                    };
                    if (parent == "HOSO")
                    {
                        var c = db.Count_HS(DonVi, NamHoc).Where(x => x.TrangThaiHS == m.FDescription).Count();
                        if (c > 0)
                        {
                            Count.title = c.ToString();
                            Count.classes = "px-2 bg-pink-600 text-white rounded-full";
                        }
                    }
                    var oMenu = new
                    {
                        name = m.title,
                        //m.subtitle,
                        m.FCode,
                        m.icon,
                        id = m.FCode,
                        m.title,
                        m.hidden,
                        m.active,
                        m.disabled,
                        m.link,
                        m.externalLink,
                        m.target,
                        m.type,
                        m.FIndex,
                        badge = Count,
                        children = getChildMenu(m.FCode, DonVi, allMenu),

                    };
                    tree.Add(oMenu);
                }
            }
            return tree;
        }

        [HttpGet]
        [Route("api/common/messages")]
        public IHttpActionResult messages()
        {
            List<Message> messages = new List<Message>();
            Message msg = new Message();
            msg.id = "832276cc-c5e9-4fcc-8e23-d38e2e267bc9";
            msg.image = "assets/images/avatars/male-01.jpg";
            msg.title = "Gary Peters";
            msg.description = "We should talk about that at lunch!";
            msg.time = DateTime.Now;
            msg.read = false;
            messages.Add(msg);

            return Ok(messages);

        }

        [HttpGet]
        [Route("api/common/notifications")]
        public IHttpActionResult notifications()
        {
            List<Notification> notification = new List<Notification>();
            Notification notifi = new Notification();
            notifi.id = "832276cc-c5e9-4fcc-8e23-d38e2e267bc9";
            notifi.image = "assets/images/avatars/male-01.jpg";
            notifi.title = "Gary Peters";
            notifi.description = "We should talk about that at lunch!";
            notifi.time = DateTime.Now;
            notifi.read = false;
            notification.Add(notifi);

            return Ok(notification);

        }

        private const string LocalLoginProvider = "Local";
        private ApplicationUserManager _userManager;
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

        [HttpGet]
        [Route("api/common/shortcuts")]
        public IHttpActionResult shortcuts()
        {
            List<Shortcut> Shortcuts = new List<Shortcut>();
            Shortcut shortcut = new Shortcut();
            shortcut.id = "832276cc-c5e9-4fcc-8e23-d38e2e267bc9";
            shortcut.label = "Changelog";
            shortcut.icon = "heroicons_outline:clipboard-list";
            shortcut.description = "We should talk about that at lunch!";
            shortcut.link = "/docs/changelog";
            shortcut.useRouter = true;
            Shortcuts.Add(shortcut);

            return Ok(Shortcuts);

        }

        [HttpGet]
        [Route("api/common/user")]
        public IHttpActionResult user()
        {

            IdentityUser user = UserManager.FindById(User.Identity.GetUserId());
            var oUser = db.UserProfiles.Where(x => x.Id == user.Id).FirstOrDefault();
            User u = new User();
            u.id = oUser.Id;
            u.name = oUser.FullName;
            u.email = oUser.Email;
            u.avatar = string.IsNullOrEmpty(oUser.Avatar) ? "/Uploads/Images/user_no_image.png" : oUser.Avatar;
            u.status = "online";
            return Ok(u);
        }

        [HttpGet]
        [Route("api/common/ping")]
        public IHttpActionResult ping()
        {
            return Ok("Ping Api success");
        }

        [HttpPost]
        [Route("api/auth/refresh-access-token")]
        public IHttpActionResult refresh_access_token([FromBody]string accessToken)
        {
            User u = new User();
            u.id = "cfaad35d-07a3-4447-a6c3-d8c3d54fd5df";
            u.name = "Brian Hughes";
            u.email = "hughes.brian@company.com";
            u.avatar = "assets/images/avatars/brian-hughes.jpg";
            u.status = "online";

            var obj = new 
            {
                accessToken = accessToken,
                user = u
            };

            return Ok(obj);
        }

        [HttpPost]
        [Route("api/common/upload")]
        public IHttpActionResult UploadFile()
        {
            bool exists = System.IO.Directory.Exists(HttpContext.Current.Server.MapPath("~/Uploads/Images"));
            if (!exists)
                System.IO.Directory.CreateDirectory(HttpContext.Current.Server.MapPath("~/Uploads/Images"));
            System.Web.HttpFileCollection httpRequest = System.Web.HttpContext.Current.Request.Files;
            string filename = "";
            List<dynamic> listFile = new List<dynamic>();
            for (int i = 0; i <= httpRequest.Count - 1; i++)
            {
                System.Web.HttpPostedFile postedfile = httpRequest[i];
                if (postedfile.ContentLength > 0)
                {

                    System.Guid guid = System.Guid.NewGuid();
                    filename = guid.ToString() + "_" + postedfile.FileName;
                    var fileSavePath = Path.Combine(HttpContext.Current.Server.MapPath("~/Uploads/Images"), filename);
                    postedfile.SaveAs(fileSavePath);
                    var file = new
                    {
                        Name = postedfile.FileName,
                        FileSize = postedfile.ContentLength,
                        ContentType = postedfile.ContentType,
                        Path = "/Uploads/Images/" + filename
                    };
                    listFile.Add(file);
                }
            }
            return Ok(listFile);
        }
    }
}
