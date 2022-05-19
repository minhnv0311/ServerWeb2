using Microsoft.Ajax.Utilities;
using Newtonsoft.Json;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.UI.WebControls;
using VSDCompany.Models;

namespace VSDCompany.Controllers
{
    /// <summary>
    /// Quản lý danh mục chung
    /// </summary>
    [Authorize]
    public class CategoryController : ApiController
    {
        private Entities db = new Entities();

        #region API Menus
        /// <summary>
        /// Chỉnh sửa dữ liệu danh mục chức năng
        /// </summary>
        /// <param name="m">Đối tượng cần thêm hoặc sửa</param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/category/menu")]
        public IHttpActionResult ActionMenu(VSDCompany.Models.Menu m)
        {
            dynamic objValidate = Validate(m);
            if (objValidate.response_data.Count > 0)
                return Ok(objValidate);

            //var item = db.Menus.Where(x => x.Id == m.Id).FirstOrDefault();
            //if (item == null)
            //    db.Menus.Add(m);
            //else
            m.FCode = m.FCode.ToUpper();
            db.Menus.AddOrUpdate(m);
            db.SaveChanges();
            var result = new
            {
                response_code = "00",
                response_data = m
            };
            return Ok(result);
        }

        [HttpDelete]
        [Route("api/category/menu")]
        public IHttpActionResult Menu(int Id)
        {
            try
            {
                var item = db.Menus.Where(x => x.Id == Id).FirstOrDefault();
                if (item != null)
                {
                    db.Menus.Remove(item);
                    db.SaveChanges();
                }
                return Ok();
            }
            catch
            {
                return BadRequest();
            }

        }

        [HttpPost]
        [Route("api/category/list_menu")]
        public IHttpActionResult list_menu(string code)
        {

            if (string.IsNullOrEmpty(code))
            {
                var collMenu = db.Menus.Where(x => x.parent == "" || x.parent == null).OrderBy(x => x.FIndex).ToList();
                return Ok(collMenu);
            }
            else
            {
                var collMenu = db.Menus.Where(x => x.parent == code).OrderBy(x => x.FIndex).ToList();
                return Ok(collMenu);
            }

        }

        [HttpPost]
        [Route("api/category/tree_menu")]
        public IHttpActionResult tree_menu()
        {
            var collMenu = db.Menus.Where(x => x.parent == "" || x.parent == null).OrderBy(x => x.FIndex).ToList();
            List<dynamic> tree = new List<dynamic>();
            foreach (var m in collMenu)
            {
                var oMenu = new
                {
                    name = m.title,
                    m.FCode,
                    m.icon,
                    m.Id,
                    m.title,
                    m.hidden,
                    m.active,
                    m.disabled,
                    m.externalLink,
                    m.target,
                    m.type,
                    m.FIndex,
                    children = getChildMenu(m.FCode),

                };
                tree.Add(oMenu);
            }
            var Root = new
            {
                name = "Chức năng chính",
                FCode = "ROOT",
                icon = "",
                Id = 0,
                title = "Chức năng chính",
                children = tree
            };
            List<dynamic> result = new List<dynamic>();
            result.Add(Root);
            return Ok(result);
        }

        private List<dynamic> getChildMenu(string parent)
        {
            var collMenu = db.Menus.Where(x => x.parent == parent).OrderBy(x => x.FIndex).ToList();
            List<dynamic> tree = new List<dynamic>();
            if (collMenu.Count > 0)
            {
                foreach (var m in collMenu)
                {
                    var oMenu = new
                    {
                        name = m.title,
                        m.FCode,
                        m.icon,
                        m.Id,
                        m.title,
                        m.hidden,
                        m.active,
                        m.disabled,
                        m.externalLink,
                        m.target,
                        m.type,
                        m.FIndex,
                        children = getChildMenu(m.FCode),

                    };
                    tree.Add(oMenu);
                }
            }
            return tree;
        }

        private dynamic Validate(VSDCompany.Models.Menu menu)
        {

            List<dynamic> validate = new List<dynamic>();
            var item = db.Menus.Where(x => x.FCode == menu.FCode).FirstOrDefault();
            if (item != null && menu.Id <= 0)
            {

                validate.Add(new
                {
                    fieldName = "FCode",
                    errorMsg = "Mã đã tồn tại vui lòng chọn mã khác !",
                    isError = true
                });

            }

            //if (string.IsNullOrEmpty(menu.link))
            //{
            //    validate.Add(new
            //    {
            //        fieldName = "link",
            //        errorMsg = "Yêu cầu nhập link chức năng !",
            //        isError = true
            //    });

            //}
            var result = new
            {
                response_code = "01",
                response_data = validate
            };
            return result;
        }

        /// <summary>
        /// Kiểm tra mã chức năng đã được sử dụng chưa
        /// </summary>
        /// <param name="FCode">Mã chức năng cần kiểm tra </param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/category/menu_check")]
        public IHttpActionResult menu_check(string FCode)
        {
            var item = db.Menus.Where(x => x.FCode == FCode).FirstOrDefault();
            if (item == null)
            {
                return Ok();
            }
            else
            {
                return BadRequest();
            }

        }
        #endregion

        #region API Area

        [HttpPost]
        [Route("api/settings/tree_area")]
        public IHttpActionResult tree_area()
        {
            List<Area> ListArea = db.Areas.Where(x => x.Type == "TINH" || x.Type == "HUYEN").OrderBy(x => x.FCode).ToList();
            var collArea = db.Areas.Where(x => x.Parent == "").OrderBy(x => x.FCode).ToList();
            List<dynamic> tree = new List<dynamic>();
            foreach (var m in collArea)
            {
                var oArea = new
                {
                    name = m.FName,
                    m.FCode,
                    m.Id,
                    title = m.FName,
                    m.FIndex,

                    children = getChildArea(m.FCode, ListArea),

                };
                tree.Add(oArea);
            }
            var Root = new
            {
                name = "Danh sách địa bàn",
                FCode = "ROOT",
                icon = "",
                Id = 0,
                title = "Danh sách địa bàn",

                children = tree
            };
            List<dynamic> result = new List<dynamic>();
            result.Add(Root);
            return Ok(result);
        }

        private List<dynamic> getChildArea(string parent, List<Area> ListArea)
        {
            var collArea = ListArea.Where(x => x.Parent == parent && x.Type == "HUYEN").OrderBy(x => x.FCode).ToList();
            List<dynamic> tree = new List<dynamic>();
            if (collArea.Count > 0)
            {
                foreach (var m in collArea)
                {
                    var oArea = new
                    {
                        name = m.FName,
                        m.FCode,
                        m.Id,
                        title = m.FName,
                        m.FIndex,
                        children = getChildArea(m.FCode, ListArea),

                    };
                    tree.Add(oArea);
                }
            }
            return tree;
        }

        [HttpPost]
        [Route("api/category/list_area")]
        public IHttpActionResult list_area(string code)
        {

            if (string.IsNullOrEmpty(code))
            {
                var collMenu = db.Areas.Where(x => x.Parent == "" || x.Parent == null).OrderBy(x => x.FCode).ToList();
                return Ok(collMenu);
            }
            else
            {
                var collMenu = db.Areas.Where(x => x.Parent == code).OrderBy(x => x.FCode).ToList();
                return Ok(collMenu);
            }

        }

        [HttpPost]
        [Route("api/settings/area")]
        public IHttpActionResult ActionArea(VSDCompany.Models.Area m)
        {
            dynamic objValidate = Validate(m);
            if (objValidate.response_data.Count > 0)
                return Ok(objValidate);

            m.FCode = m.FCode.ToUpper();
            db.Areas.AddOrUpdate(m);
            db.SaveChanges();
            var result = new
            {
                response_code = "00",
                response_data = m
            };
            return Ok(result);
        }
        private dynamic Validate(VSDCompany.Models.Area menu)
        {

            List<dynamic> validate = new List<dynamic>();
            var item = db.Menus.Where(x => x.FCode == menu.FCode).FirstOrDefault();
            if (item != null && menu.Id <= 0)
            {

                validate.Add(new
                {
                    fieldName = "FCode",
                    errorMsg = "Mã đã tồn tại vui lòng chọn mã khác !",
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

        [HttpDelete]
        [Route("api/settings/area")]
        public IHttpActionResult Area(int Id)
        {
            try
            {
                var item = db.Areas.Where(x => x.Id == Id).FirstOrDefault();
                if (item != null)
                {
                    db.Areas.Remove(item);
                    db.SaveChanges();
                }
                return Ok();
            }
            catch
            {
                return BadRequest();
            }

        }

        [HttpGet]
        [Route("api/settings/areabycode")]
        public IHttpActionResult AreaByCode(string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                var collMenu = db.Areas.Where(x => x.Parent == "" || x.Parent == null).OrderBy(x => x.FCode).ToList();
                return Ok(collMenu);
            }
            else
            {
                var collMenu = db.Areas.Where(x => x.Parent == code).OrderBy(x => x.FCode).ToList();
                return Ok(collMenu);
            }

        }
        #endregion

        #region API Nation
        [HttpPost]
        [Route("api/settings/nation")]
        public IHttpActionResult ActionNation(VSDCompany.Models.Nation m)
        {

            dynamic objValidate = Validate(m);
            if (objValidate.response_data.Count > 0)
                return Ok(objValidate);

            m.FCode = m.FCode.ToUpper();
            db.Nations.AddOrUpdate(m);
            db.SaveChanges();
            var result = new
            {
                response_code = "00",
                response_data = m
            };
            return Ok(result);
        }
        private dynamic Validate(VSDCompany.Models.Nation menu)
        {

            List<dynamic> validate = new List<dynamic>();
            var item = db.Nations.Where(x => x.FCode == menu.FCode).SingleOrDefault();
            if (item != null && menu.Id <= 0)
            {
                validate.Add(new
                {
                    fieldName = "FCode",
                    errorMsg = "Mã đã tồn tại vui lòng chọn mã khác !",
                    isError = true
                });
            }

            if (string.IsNullOrEmpty(menu.FName))
            {
                validate.Add(new
                {
                    fieldName = "FName",
                    errorMsg = "Tên không được bỏ trống !",
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

        [HttpDelete]
        [Route("api/settings/nation")]
        public IHttpActionResult Nation(int Id)
        {
            try
            {
                var item = db.Nations.Where(x => x.Id == Id).FirstOrDefault();
                if (item != null)
                {
                    db.Nations.Remove(item);
                    db.SaveChanges();
                }
                return Ok();
            }
            catch
            {
                return BadRequest();
            }

        }

        [HttpPost]
        [Route("api/category/list_nation")]
        public IHttpActionResult list_nation()
        {
            var collResult = db.Nations.OrderBy(x => x.FCode).ToList();
            return Ok(collResult);
        }

        #endregion 

        #region API Nationality
        [HttpPost]
        [Route("api/settings/nationality")]
        public IHttpActionResult ActionNationality(VSDCompany.Models.Nationality m)
        {

            dynamic objValidate = Validate(m);
            if (objValidate.response_data.Count > 0)
                return Ok(objValidate);

            m.FCode = m.FCode.ToUpper();
            db.Nationalities.AddOrUpdate(m);
            db.SaveChanges();
            var result = new
            {
                response_code = "00",
                response_data = m
            };
            return Ok(result);
        }
        private dynamic Validate(VSDCompany.Models.Nationality menu)
        {

            List<dynamic> validate = new List<dynamic>();
            var item = db.Nationalities.Where(x => x.FCode == menu.FCode).SingleOrDefault();
            if (item != null && menu.Id <= 0)
            {
                validate.Add(new
                {
                    fieldName = "FCode",
                    errorMsg = "Mã đã tồn tại vui lòng chọn mã khác !",
                    isError = true
                });
            }

            if (string.IsNullOrEmpty(menu.FName))
            {
                validate.Add(new
                {
                    fieldName = "FName",
                    errorMsg = "Tên không được bỏ trống !",
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

        [HttpDelete]
        [Route("api/settings/nationality")]
        public IHttpActionResult Nationality(int Id)
        {
            try
            {
                var item = db.Nationalities.Where(x => x.Id == Id).FirstOrDefault();
                if (item != null)
                {
                    db.Nationalities.Remove(item);
                    db.SaveChanges();
                }
                return Ok();
            }
            catch
            {
                return BadRequest();
            }

        }

        [HttpPost]
        [Route("api/category/list_nationality")]
        public IHttpActionResult list_nationality()
        {
            var collResult = db.Nationalities.OrderBy(x => x.FCode).ToList();
            return Ok(collResult);
        }
        #endregion

        #region API Groups

        [HttpPost]
        [Route("api/settings/group")]
        public IHttpActionResult Actiongroup(VSDCompany.Models.Group m)
        {

            dynamic objValidate = Validate(m);
            if (objValidate.response_data.Count > 0)
                return Ok(objValidate);

            m.FCode = m.FCode.ToUpper();
            db.Groups.AddOrUpdate(m);
            db.SaveChanges();
            var result = new
            {
                response_code = "00",
                response_data = m
            };
            return Ok(result);
        }
        private dynamic Validate(VSDCompany.Models.Group menu)
        {

            List<dynamic> validate = new List<dynamic>();
            var item = db.Groups.Where(x => x.FCode == menu.FCode).SingleOrDefault();
            if (item != null && menu.Id <= 0)
            {
                validate.Add(new
                {
                    fieldName = "FCode",
                    errorMsg = "Mã đã tồn tại vui lòng chọn mã khác !",
                    isError = true
                });
            }

            if (string.IsNullOrEmpty(menu.FName))
            {
                validate.Add(new
                {
                    fieldName = "FName",
                    errorMsg = "Tên không được bỏ trống !",
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

        [HttpDelete]
        [Route("api/settings/group")]
        public IHttpActionResult group(int Id)
        {
            try
            {
                var item = db.Groups.Where(x => x.Id == Id).FirstOrDefault();
                if (item != null)
                {
                    db.Groups.Remove(item);
                    db.SaveChanges();
                }
                return Ok();
            }
            catch
            {
                return BadRequest();
            }

        }

        [HttpPost]
        [Route("api/category/list_group")]
        public IHttpActionResult list_group()
        {
            var collResult = User.Identity.Name == "admin" ? db.Groups.OrderBy(x => x.FCode).ToList() : db.Groups.Where(a => a.FCode != "SYSTEM").OrderBy(x => x.FIndex).ToList();
            return Ok(collResult);
        }

        [HttpPost]
        [Route("api/category/list_group_by_account")]
        public IHttpActionResult list_group_by_account(string username)
        {
            var listCode = db.Group_User.Where(x => x.UserName == username).Select(y => y.CodeGroup).ToList();
            var collResult = User.Identity.Name == "admin" ? db.Groups.ToList() : db.Groups.Where(a => a.FCode != "SYSTEM").ToList();
            foreach (var code in listCode)
            {
                foreach(var result in collResult)
                {
                    result.Check = false;
                    if (result.FCode == code)
                    {
                        result.Check = true;
                    }
                }
            }
            return Ok(collResult);
        }

        [HttpPost]
        [Route("api/settings/group_by_account")]
        public IHttpActionResult group_by_account(string UserName, List<VSDCompany.Models.Group> groups)
        {
            var item = db.Group_User.Where(x => x.UserName == UserName).ToList();
            foreach (var i in item)
            {
                db.Group_User.Remove(i);
                db.SaveChanges();
            }
            foreach (var g in groups)
            {
                if (g.Check == true)
                {
                    var gu = new Group_User();
                    gu.FCode = Commons.GenerateID(db, "GU").FName;
                    gu.UserName = UserName;
                    gu.CodeGroup = g.FCode;
                    db.Group_User.AddOrUpdate(gu);
                    db.SaveChanges();
                }
            }
            var result = new
            {
                response_code = "00",
                response_data = UserName
            };
            return Ok(result);
        }

        #endregion

        #region API Group_Users

        [HttpPost]
        [Route("api/settings/groupuser")]
        public IHttpActionResult ActiongGroupuser(string Code, List<VSDCompany.Models.UserProfile> users)
        {
            var item = db.Group_User.Where(x => x.CodeGroup == Code).ToList();
            foreach(var i in item)
            {
                db.Group_User.Remove(i);
                db.SaveChanges();
            }
            foreach (var u in users) {
                if(u.FIndex == 2)
                {
                    var gu = new Group_User();
                    gu.FCode = Commons.GenerateID(db, "GU").FName;
                    gu.UserName = u.UserName;
                    gu.CodeGroup = Code;
                    db.Group_User.AddOrUpdate(gu);
                    db.SaveChanges();
                }
            }
            var result = new
            {
                response_code = "00",
                response_data = Code
            };
            return Ok(result);
        }

        #endregion

        #region API Group_Roles

        [HttpPost]
        [Route("api/settings/grouprole")]
        public IHttpActionResult ActiongGrouprole(string Code, List<string> menu)
        {
            var item = db.Group_Menu.Where(x => x.CodeGroup == Code).ToList();
            foreach (var i in item)
            {
                db.Group_Menu.Remove(i);
                db.SaveChanges();
            }
            foreach (var m in menu)
            {
                var gr = new Group_Menu();
                gr.FCode = Commons.GenerateID(db, "GM").FName;
                gr.CodeMenu = m;
                gr.CodeGroup = Code;
                db.Group_Menu.AddOrUpdate(gr);
                db.SaveChanges();
            }
            var result = new
            {
                response_code = "00",
                response_data = Code
            };
            return Ok(result);
        }

        [HttpGet]
        [Route("api/settings/group_role")]
        public IHttpActionResult ActiongGrouproles(string Code)
        {
            var gr = db.Group_Menu.Where(x => x.CodeGroup == Code).Select(y => y.CodeMenu).ToList();
            var result = new
            {
                response_code = "00",
                response_data = gr
            };
            return Ok(result);
        }

        [HttpPost]
        [Route("api/category/tree_menu_group")]
        public IHttpActionResult tree_menu_group(string Code)
        {
            var collMenu = db.Menus.Where(x => x.parent == "" || x.parent == null).OrderBy(x => x.FIndex).ToList();
            List<dynamic> tree = new List<dynamic>();
            foreach (var m in collMenu)
            {
                var g = db.Group_Menu.Where(x => x.CodeGroup == Code && x.CodeMenu == m.FCode).FirstOrDefault();
                var oMenu = new
                {
                    name = m.title,
                    m.FCode,
                    m.icon,
                    m.Id,
                    m.title,
                    m.hidden,
                    m.active,
                    m.disabled,
                    m.externalLink,
                    m.target,
                    m.type,
                    m.FIndex,
                    isSelected = g != null ? true : false,
                    children = getChildGroupMenu(m.FCode, Code),

                };
                tree.Add(oMenu);
            }
            var gr = db.Group_Menu.Where(x => x.CodeGroup == Code && x.CodeMenu == "ROOT").FirstOrDefault();
            var Root = new
            {
                name = "Chức năng chính",
                FCode = "ROOT",
                icon = "",
                Id = 0,
                title = "Chức năng chính",
                isSelected = gr != null ? true : false,
                children = tree
            };
            List<dynamic> result = new List<dynamic>();
            result.Add(Root);
            return Ok(result);
        }

        private List<dynamic> getChildGroupMenu(string parent, string Code)
        {
            var collMenu = db.Menus.Where(x => x.parent == parent).OrderBy(x => x.FIndex).ToList();
            List<dynamic> tree = new List<dynamic>();
            if (collMenu.Count > 0)
            {
                foreach (var m in collMenu)
                {
                    var g = db.Group_Menu.Where(x => x.CodeGroup == Code && x.CodeMenu == m.FCode).FirstOrDefault();
                    var oMenu = new
                    {
                        name = m.title,
                        m.FCode,
                        m.icon,
                        m.Id,
                        m.title,
                        m.hidden,
                        m.active,
                        m.disabled,
                        m.externalLink,
                        m.target,
                        m.type,
                        m.FIndex,
                        isSelected = g != null ? true : false,
                        children = getChildGroupMenu(m.FCode, Code),

                    };
                    tree.Add(oMenu);
                }
            }
            return tree;
        }

        #endregion

        #region API Roles

        [HttpPost]
        [Route("api/settings/role")]
        public IHttpActionResult ActionRole(VSDCompany.Models.Role m)
        {

            dynamic objValidate = Validate(m);
            if (objValidate.response_data.Count > 0)
                return Ok(objValidate);

            m.FCode = m.FCode.ToUpper();
            db.Roles.AddOrUpdate(m);
            db.SaveChanges();
            var result = new
            {
                response_code = "00",
                response_data = m
            };
            return Ok(result);
        }
        private dynamic Validate(VSDCompany.Models.Role menu)
        {

            List<dynamic> validate = new List<dynamic>();
            var item = db.Roles.Where(x => x.FCode == menu.FCode).SingleOrDefault();
            if (item != null && menu.Id <= 0)
            {
                validate.Add(new
                {
                    fieldName = "FCode",
                    errorMsg = "Mã đã tồn tại vui lòng chọn mã khác !",
                    isError = true
                });
            }

            if (string.IsNullOrEmpty(menu.FName))
            {
                validate.Add(new
                {
                    fieldName = "FName",
                    errorMsg = "Tên không được bỏ trống !",
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

        [HttpDelete]
        [Route("api/settings/role")]
        public IHttpActionResult role(int Id)
        {
            try
            {
                var item = db.Roles.Where(x => x.Id == Id).FirstOrDefault();
                if (item != null)
                {
                    db.Roles.Remove(item);
                    db.SaveChanges();
                }
                return Ok();
            }
            catch
            {
                return BadRequest();
            }

        }

        [HttpPost]
        [Route("api/category/list_role")]
        public IHttpActionResult list_role()
        {
            var collResult = db.Roles.OrderBy(x => x.FCode).ToList();
            return Ok(collResult);
        }

        #endregion

        #region API Processing Status

        [HttpPost]
        [Route("api/settings/status")]
        public IHttpActionResult Actionstatus(VSDCompany.Models.Processing_Status m)
        {

            dynamic objValidate = Validate(m);
            if (objValidate.response_data.Count > 0)
                return Ok(objValidate);

            m.FCode = m.FCode.ToUpper();
            db.Processing_Status.AddOrUpdate(m);
            db.SaveChanges();
            var result = new
            {
                response_code = "00",
                response_data = m
            };
            return Ok(result);
        }
        private dynamic Validate(VSDCompany.Models.Processing_Status menu)
        {

            List<dynamic> validate = new List<dynamic>();
            var item = db.Processing_Status.Where(x => x.FCode == menu.FCode).SingleOrDefault();
            if (item != null && menu.Id <= 0)
            {
                validate.Add(new
                {
                    fieldName = "FCode",
                    errorMsg = "Mã đã tồn tại vui lòng chọn mã khác !",
                    isError = true
                });
            }

            if (string.IsNullOrEmpty(menu.FName))
            {
                validate.Add(new
                {
                    fieldName = "FName",
                    errorMsg = "Tên không được bỏ trống !",
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

        [HttpDelete]
        [Route("api/settings/status")]
        public IHttpActionResult status(int Id)
        {
            try
            {
                var item = db.Processing_Status.Where(x => x.Id == Id).FirstOrDefault();
                if (item != null)
                {
                    db.Processing_Status.Remove(item);
                    db.SaveChanges();
                }
                return Ok();
            }
            catch
            {
                return BadRequest();
            }

        }

        [HttpPost]
        [Route("api/category/list_status")]
        public IHttpActionResult list_status()
        {
            var collResult = db.Processing_Status.OrderBy(x => x.FCode).ToList();
            return Ok(collResult);
        }

        #endregion

        #region API Organization_Type

        [HttpPost]
        [Route("api/settings/orgtype")]
        public IHttpActionResult ActionUnit(VSDCompany.Models.Organization_Type m)
        {

            dynamic objValidate = Validate(m);
            if (objValidate.response_data.Count > 0)
                return Ok(objValidate);

            m.FCode = m.FCode.ToUpper();
            db.Organization_Type.AddOrUpdate(m);
            db.SaveChanges();
            var result = new
            {
                response_code = "00",
                response_data = m
            };
            return Ok(result);
        }
        private dynamic Validate(VSDCompany.Models.Organization_Type menu)
        {

            List<dynamic> validate = new List<dynamic>();
            var item = db.Roles.Where(x => x.FCode == menu.FCode).SingleOrDefault();
            if (item != null && menu.Id <= 0)
            {
                validate.Add(new
                {
                    fieldName = "FCode",
                    errorMsg = "Mã đã tồn tại vui lòng chọn mã khác !",
                    isError = true
                });
            }

            if (string.IsNullOrEmpty(menu.FName))
            {
                validate.Add(new
                {
                    fieldName = "FName",
                    errorMsg = "Tên không được bỏ trống !",
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

        [HttpDelete]
        [Route("api/settings/orgtype")]
        public IHttpActionResult unit(int Id)
        {
            try
            {
                var item = db.Organization_Type.Where(x => x.Id == Id).FirstOrDefault();
                if (item != null)
                {
                    db.Organization_Type.Remove(item);
                    db.SaveChanges();
                }
                return Ok();
            }
            catch
            {
                return BadRequest();
            }

        }

        [HttpPost]
        [Route("api/category/list_orgtype")]
        public IHttpActionResult list_unit()
        {
            var collResult = db.Organization_Type.OrderBy(x => x.FCode).ToList();
            return Ok(collResult);
        }

        #endregion

        #region API Organization
        [HttpPost]
        [Route("api/settings/org")]
        public IHttpActionResult ActionOrg(VSDCompany.Models.Organization m)
        {
            if (m.Id <= 0 && string.IsNullOrEmpty(m.FCode))
            {
                m.FCode = Commons.GenerateID(db, "ORG").FName;
            }
            dynamic objValidate = Validate(m);
            if (objValidate.response_data.Count > 0)
                return Ok(objValidate);

            m.FCode = m.FCode.ToUpper();
            db.Organizations.AddOrUpdate(m);
            db.SaveChanges();
            var result = new
            {
                response_code = "00",
                response_data = m
            };
            return Ok(result);
        }

        private dynamic Validate(VSDCompany.Models.Organization menu)
        {

            List<dynamic> validate = new List<dynamic>();
            var item = db.Organizations.Where(x => x.FCode == menu.FCode).SingleOrDefault();
            if (item != null && menu.Id <= 0)
            {
                validate.Add(new
                {
                    fieldName = "FCode",
                    errorMsg = "Mã đã tồn tại vui lòng chọn mã khác !",
                    isError = true
                });
            }

            if (string.IsNullOrEmpty(menu.FName))
            {
                validate.Add(new
                {
                    fieldName = "FName",
                    errorMsg = "Tên không được bỏ trống !",
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

        [HttpPost]
        [Route("api/settings/tree_org")]
        public IHttpActionResult tree_org()
        {
            var username = User.Identity.Name;
            List<Organization> ListOrg = db.Organizations.OrderBy(x => x.FCode).ToList();
            var user = db.UserProfiles.Where(x => x.UserName == username).FirstOrDefault();
            List<dynamic> tree = new List<dynamic>();
            if (user.Type == "AD_PGD")
            {
                var collOrg = ListOrg.Where(x => (x.Disctrict == user.District || x.Type == "DVNHOM") && (x.FParent == "" || x.FParent == null) && (x.IsTHPT != true || (x.IsTHCS == true && x.IsTHPT == true))).ToList();
                var collOrgType = db.Organization_Type.ToList();

                foreach (var m in collOrg)
                {
                    var orgType = collOrgType.Where(x => x.FCode == m.Type).FirstOrDefault();
                    var oArea = new
                    {
                        name = m.FName,
                        m.FCode,
                        m.Id,
                        title = m.FName,
                        m.FIndex,
                        Icon = orgType == null ? "" : orgType.Icon,
                        children = getChildOrg(m.FCode, ListOrg, collOrgType, user.Type, user.District),
                    };
                    tree.Add(oArea);
                }
            }
            if(user.Type == "AD_SGD" || user.UserName == "admin")
            {
                var collOrg = ListOrg.Where(x => x.FParent == "" || x.FParent == null).ToList();
                var collOrgType = db.Organization_Type.ToList();

                foreach (var m in collOrg)
                {
                    var orgType = collOrgType.Where(x => x.FCode == m.Type).FirstOrDefault();
                    var oArea = new
                    {
                        name = m.FName,
                        m.FCode,
                        m.Id,
                        title = m.FName,
                        m.FIndex,
                        Icon = orgType == null ? "" : orgType.Icon,
                        children = getChildOrg(m.FCode, ListOrg, collOrgType, user.Type, user.District),
                    };
                    tree.Add(oArea);
                }
            }
            if(user.Type == "AD_TRUONG")
            {
                var collOrg = ListOrg.Where(x => x.FCode == user.DonVi || x.Type == "DVNHOM").ToList();
                var collOrgType = db.Organization_Type.ToList();

                foreach (var m in ListOrg)
                {
                    var orgType = collOrgType.Where(x => x.FCode == m.Type).FirstOrDefault();
                    var oArea = new
                    {
                        name = m.FName,
                        m.FCode,
                        m.Id,
                        title = m.FName,
                        m.FIndex,
                        Icon = orgType == null ? "" : orgType.Icon,
                        children = getChildOrg(m.FCode, ListOrg, collOrgType, user.Type, user.District),
                    };
                    tree.Add(oArea);
                }
            }
            var Root = new
            {
                name = "Danh sách Đơn vị",
                FCode = "ROOT",
                Icon = "mat_outline:home_work",
                Id = 0,
                title = "Danh sách Đơn vị",

                children = tree
            };
            List<dynamic> result = new List<dynamic>();
            result.Add(Root);
            return Ok(result);
        }


        private List<dynamic> getChildOrg(string parent, List<Organization> ListOrg, List<Organization_Type> collOrgType, string Type, string District)
        {
            List<dynamic> tree = new List<dynamic>();
            if (Type == "AD_PGD")
            {
                var collOrg = ListOrg.Where(x => x.FParent == parent && (x.Disctrict == District || x.Type == "DVNHOM") && (x.IsTHPT != true || (x.IsTHCS == true && x.IsTHPT == true))).OrderBy(x => x.FCode).ToList();
                if (collOrg.Count > 0)
                {
                    foreach (var m in collOrg)
                    {
                        var orgType = collOrgType.Where(x => x.FCode == m.Type).FirstOrDefault();
                        var oOrg = new
                        {
                            name = m.FName,
                            m.FCode,
                            m.Id,
                            title = m.FName,
                            m.FIndex,
                            Icon = orgType == null ? "" : orgType.Icon,
                            children = getChildOrg(m.FCode, ListOrg, collOrgType, Type, District),

                        };
                        tree.Add(oOrg);
                    }
                }
            }
            if (Type == "AD_SGD" || User.Identity.Name == "admin")
            {
                var collOrg = ListOrg.Where(x => x.FParent == parent).OrderBy(x => x.FCode).ToList();
                if (collOrg.Count > 0)
                {
                    foreach (var m in collOrg)
                    {
                        var orgType = collOrgType.Where(x => x.FCode == m.Type).FirstOrDefault();
                        var oOrg = new
                        {
                            name = m.FName,
                            m.FCode,
                            m.Id,
                            title = m.FName,
                            m.FIndex,
                            Icon = orgType == null ? "" : orgType.Icon,
                            children = getChildOrg(m.FCode, ListOrg, collOrgType, Type, District),

                        };
                        tree.Add(oOrg);
                    }
                }
            }
            if (Type == "AD_TRUONG")
            {
                var username = User.Identity.Name;
                var user = db.UserProfiles.Where(x => x.UserName == username).FirstOrDefault();
                var collOrg = ListOrg.Where(x => x.FParent == parent && (x.FCode == user.DonVi || x.Type == "DVNHOM")).OrderBy(x => x.FCode).ToList();
                if (collOrg.Count > 0)
                {
                    foreach (var m in collOrg)
                    {
                        var orgType = collOrgType.Where(x => x.FCode == m.Type).FirstOrDefault();
                        var oOrg = new
                        {
                            name = m.FName,
                            m.FCode,
                            m.Id,
                            title = m.FName,
                            m.FIndex,
                            Icon = orgType == null ? "" : orgType.Icon,
                            children = getChildOrg(m.FCode, ListOrg, collOrgType, Type, District),

                        };
                        tree.Add(oOrg);
                    }
                }
            }
            return tree;
        }

        [HttpDelete]
        [Route("api/settings/org")]
        public IHttpActionResult Org(int Id)
        {
            try
            {
                var item = db.Organizations.Where(x => x.Id == Id).FirstOrDefault();
                if (item != null)
                {
                    db.Organizations.Remove(item);
                    db.SaveChanges();
                }
                return Ok();
            }
            catch
            {
                return BadRequest();
            }

        }

        [HttpGet]
        [Route("api/settings/orgbycode")]
        public IHttpActionResult OrgByCode(string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                var collMenu = db.Organizations.Where(x => x.FParent == "" || x.FParent == null).OrderBy(x => x.FCode).ToList();
                return Ok(collMenu);
            }
            else
            {
                var collMenu = db.Organizations.Where(x => x.FParent == code).OrderBy(x => x.FCode).ToList();
                return Ok(collMenu);
            }

        }

        [HttpPost]
        [Route("api/category/list_org")]
        public IHttpActionResult list_org(string code, string searchKey)
        {
            searchKey = searchKey == null ? "" : searchKey;
            var username = User.Identity.Name;
            var user = db.UserProfiles.Where(x => x.UserName == username).FirstOrDefault();
            Organization curentOrg = db.Organizations.Where(x => x.FCode == code && (x.FName.ToLower().Contains(searchKey.ToLower()) || String.IsNullOrEmpty(searchKey))).FirstOrDefault();
            if(user.Type == "AD_PGD")
            {
                if (string.IsNullOrEmpty(code))
                {
                    var collMenu = db.Organizations.Where(x => x.Disctrict == user.District && (x.FName.ToLower().Contains(searchKey.ToLower()) || String.IsNullOrEmpty(searchKey))).OrderBy(x => x.FCode).ToList();
                    return Ok(collMenu);
                }
                else
                {
                    var collMenu = db.Organizations.Where(x => x.FParent == code && x.Disctrict == user.District && (x.FName.ToLower().Contains(searchKey.ToLower()) || String.IsNullOrEmpty(searchKey))).OrderBy(x => x.FCode).ToList();
                    //collMenu.Insert(0, curentOrg);
                    return Ok(collMenu);
                }
            }
            else if(user.Type == "AD_SGD")
            {
                if (string.IsNullOrEmpty(code))
                {
                    var collMenu = db.Organizations.Where(x => x.Provin == user.Provin && (x.FName.ToLower().Contains(searchKey.ToLower()) || String.IsNullOrEmpty(searchKey))).OrderBy(x => x.FCode).ToList();
                    return Ok(collMenu);
                }
                else
                {
                    var collMenu = db.Organizations.Where(x => x.FParent == code && (x.FName.ToLower().Contains(searchKey.ToLower()) || String.IsNullOrEmpty(searchKey))).OrderBy(x => x.FCode).ToList();
                    //collMenu.Insert(0, curentOrg);
                    return Ok(collMenu);
                }
            }
            else
            {
                if (string.IsNullOrEmpty(code))
                {
                    if (!string.IsNullOrEmpty(searchKey))
                    {
                        var collMenu = db.Organizations.Where(x => (x.FName.ToLower().Contains(searchKey.ToLower()) || String.IsNullOrEmpty(searchKey))).OrderBy(x => x.FCode).ToList();
                        return Ok(collMenu);
                    }
                    else
                    {
                        var collMenu = db.Organizations.Where(x => x.FParent == "" || x.FParent == null).OrderBy(x => x.FCode).ToList();
                        return Ok(collMenu);
                    }
                }
                else
                {
                    var collMenu = db.Organizations.Where(x => x.FParent == code && (x.FName.ToLower().Contains(searchKey.ToLower()) || String.IsNullOrEmpty(searchKey))).OrderBy(x => x.FCode).ToList();
                    collMenu.Insert(0, curentOrg);
                    return Ok(collMenu);
                }
            }
        }

        [HttpPost]
        [Route("api/category/list_department")]
        public IHttpActionResult list_department(string code)
        {

            var collMenu = db.Organizations.Where(x => x.FParent == code && x.Type == "PHONGBAN").OrderBy(x => x.FCode).ToList();
            return Ok(collMenu);

        }
        #endregion

        #region API Positions

        [HttpPost]
        [Route("api/settings/position")]
        public IHttpActionResult ActionPosition(VSDCompany.Models.Position m)
        {

            dynamic objValidate = Validate(m);
            if (objValidate.response_data.Count > 0)
                return Ok(objValidate);

            m.FCode = m.FCode.ToUpper();
            db.Positions.AddOrUpdate(m);
            db.SaveChanges();
            var result = new
            {
                response_code = "00",
                response_data = m
            };
            return Ok(result);
        }
        private dynamic Validate(VSDCompany.Models.Position menu)
        {

            List<dynamic> validate = new List<dynamic>();
            var item = db.Positions.Where(x => x.FCode == menu.FCode).SingleOrDefault();
            if (item != null && menu.Id <= 0)
            {
                validate.Add(new
                {
                    fieldName = "FCode",
                    errorMsg = "Mã đã tồn tại vui lòng chọn mã khác !",
                    isError = true
                });
            }

            if (string.IsNullOrEmpty(menu.FName))
            {
                validate.Add(new
                {
                    fieldName = "FName",
                    errorMsg = "Tên không được bỏ trống !",
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

        [HttpDelete]
        [Route("api/settings/postion")]
        public IHttpActionResult postion(int Id)
        {
            try
            {
                var item = db.Positions.Where(x => x.Id == Id).FirstOrDefault();
                if (item != null)
                {
                    db.Positions.Remove(item);
                    db.SaveChanges();
                }
                return Ok();
            }
            catch
            {
                return BadRequest();
            }

        }

        [HttpPost]
        [Route("api/category/list_position")]
        public IHttpActionResult list_position()
        {
            var collResult = db.Positions.OrderBy(x => x.FCode).ToList();
            return Ok(collResult);
        }

        #endregion

        #region API MON_HOC

        [HttpPost]
        [Route("api/settings/monhoc")]
        public IHttpActionResult ActionMonHoc(VSDCompany.Models.MON_HOC m)
        {

            dynamic objValidate = Validate(m);
            if (objValidate.response_data.Count > 0)
                return Ok(objValidate);

            m.FCode = m.FCode.ToUpper();
            db.MON_HOC.AddOrUpdate(m);
            db.SaveChanges();
            var result = new
            {
                response_code = "00",
                response_data = m
            };
            return Ok(result);
        }
        private dynamic Validate(VSDCompany.Models.MON_HOC menu)
        {

            List<dynamic> validate = new List<dynamic>();
            var item = db.MON_HOC.Where(x => x.FCode == menu.FCode).SingleOrDefault();
            if (item != null && menu.Id <= 0)
            {
                validate.Add(new
                {
                    fieldName = "FCode",
                    errorMsg = "Mã đã tồn tại vui lòng chọn mã khác !",
                    isError = true
                });
            }

            if (string.IsNullOrEmpty(menu.FName))
            {
                validate.Add(new
                {
                    fieldName = "FName",
                    errorMsg = "Tên không được bỏ trống !",
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

        [HttpDelete]
        [Route("api/settings/monhoc")]
        public IHttpActionResult monhoc(int Id)
        {
            try
            {
                var item = db.MON_HOC.Where(x => x.Id == Id).FirstOrDefault();
                if (item != null)
                {
                    db.MON_HOC.Remove(item);
                    db.SaveChanges();
                }
                return Ok();
            }
            catch
            {
                return BadRequest();
            }

        }

        [HttpPost]
        [Route("api/category/list_monhoc")]
        public IHttpActionResult list_monhoc()
        {
            var collResult = db.MON_HOC.OrderBy(x => x.NhomMonHoc).ToList();
            return Ok(collResult);
        }

        #endregion

        #region API NHOM_MON_HOC

        [HttpPost]
        [Route("api/settings/nhommonhoc")]
        public IHttpActionResult ActionNhomMonHoc(VSDCompany.Models.NHOM_MON_HOC m)
        {

            string username = User.Identity.Name;
            string Org = db.UserProfiles.Where(x => x.UserName == username).Select(y => y.DonVi).FirstOrDefault();
            dynamic objValidate = Validate(m);
            if (objValidate.response_data.Count > 0)
                return Ok(objValidate);

            m.FCode = m.FCode.ToUpper();
            m.MaDonVi = Org;
            db.NHOM_MON_HOC.AddOrUpdate(m);
            db.SaveChanges();
            var result = new
            {
                response_code = "00",
                response_data = m
            };
            return Ok(result);
        }
        private dynamic Validate(VSDCompany.Models.NHOM_MON_HOC menu)
        {
            string username = User.Identity.Name;
            string Org = db.UserProfiles.Where(x => x.UserName == username).Select(y => y.DonVi).FirstOrDefault();
            List<dynamic> validate = new List<dynamic>();
            var item = db.NHOM_MON_HOC.Where(x => x.FCode == menu.FCode && x.MaDonVi == Org).SingleOrDefault();
            if (item != null && menu.Id <= 0)
            {
                validate.Add(new
                {
                    fieldName = "FCode",
                    errorMsg = "Mã đã tồn tại vui lòng chọn mã khác !",
                    isError = true
                });
            }

            if (string.IsNullOrEmpty(menu.FName))
            {
                validate.Add(new
                {
                    fieldName = "FName",
                    errorMsg = "Tên không được bỏ trống !",
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

        [HttpDelete]
        [Route("api/settings/nhommonhoc")]
        public IHttpActionResult nhommonhoc(int Id)
        {
            try
            {
                var item = db.NHOM_MON_HOC.Where(x => x.Id == Id).FirstOrDefault();
                if (item != null)
                {
                    db.NHOM_MON_HOC.Remove(item);
                    db.SaveChanges();
                }
                return Ok();
            }
            catch
            {
                return BadRequest();
            }

        }

        [HttpPost]
        [Route("api/category/list_nhommonhoc")]
        public IHttpActionResult list_nhommonhoc()
        {
            string username = User.Identity.Name;
            string Org = db.UserProfiles.Where(x => x.UserName == username).Select(y => y.DonVi).FirstOrDefault();
            var collResult = db.NHOM_MON_HOC.Where(a =>  a.MaDonVi == Org || a.MaDonVi == null || a.MaDonVi == "").OrderBy(x => x.FCode).ToList();
            return Ok(collResult);
        }

        [HttpGet]
        [Route("api/category/nhommonhoc")]
        public IHttpActionResult get_nhommonhoc(int Id)
        {
            var collResult = db.NHOM_MON_HOC.Where(a => a.Id == Id).FirstOrDefault();
            return Ok(collResult);
        }

        #endregion

        #region API MON_HOC

        [HttpPost]
        [Route("api/settings/thoigiantuyensinh")]
        public IHttpActionResult ActionTGTS(VSDCompany.Models.THOI_GIAN_TS m)
        {

            dynamic objValidate = Validate(m);
            if (objValidate.response_data.Count > 0)
                return Ok(objValidate);

            m.FCode = m.FCode.ToUpper();
            db.THOI_GIAN_TS.AddOrUpdate(m);
            db.SaveChanges();
            var result = new
            {
                response_code = "00",
                response_data = m
            };
            return Ok(result);
        }
        private dynamic Validate(VSDCompany.Models.THOI_GIAN_TS menu)
        {

            List<dynamic> validate = new List<dynamic>();
            var item = db.THOI_GIAN_TS.Where(x => x.FCode == menu.FCode).SingleOrDefault();
            if (item != null && menu.Id <= 0)
            {
                validate.Add(new
                {
                    fieldName = "FCode",
                    errorMsg = "Mã đã tồn tại vui lòng chọn mã khác !",
                    isError = true
                });
            }

            if (string.IsNullOrEmpty(menu.FName))
            {
                validate.Add(new
                {
                    fieldName = "FName",
                    errorMsg = "Tên không được bỏ trống !",
                    isError = true
                });

            }

            if (menu.BatDau == null)
            {
                validate.Add(new
                {
                    fieldName = "BatDau",
                    errorMsg = "Ngày bắt đầu không được trống !",
                    isError = true
                });

            }

            if (menu.FCode != "MN")
            {
                if (menu.KetThuc == null)
                {
                    validate.Add(new
                    {
                        fieldName = "KetThuc",
                        errorMsg = "Ngày kết thúc không được trống !",
                        isError = true
                    });

                }

            }

            var result = new
            {
                response_code = "01",
                response_data = validate
            };
            return result;
        }

        //[HttpDelete]
        //[Route("api/settings/thoigiantuyensinh")]
        //public IHttpActionResult thoigiantuyensinh(int Id)
        //{
        //    try
        //    {
        //        var item = db.MON_HOC.Where(x => x.Id == Id).FirstOrDefault();
        //        if (item != null)
        //        {
        //            db.MON_HOC.Remove(item);
        //            db.SaveChanges();
        //        }
        //        return Ok();
        //    }
        //    catch
        //    {
        //        return BadRequest();
        //    }

        //}

        [HttpPost]
        [Route("api/category/list_thoigiantuyensinh")]
        public IHttpActionResult list_thoigiantuyensinh()
        {
            var collResult = db.THOI_GIAN_TS.OrderBy(x => x.FCode).ToList();
            return Ok(collResult);
        }

        #endregion
    }
}