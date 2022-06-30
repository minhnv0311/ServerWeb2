using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using VSDCompany.Models;

namespace VSDCompany.Controllers
{

    [RoutePrefix("api/CCM_Home")]
    public class CCM_HomeController : ApiController
    {
        private Entities db = new Entities();

        public ListNews GetFromTuoiTre()
        {
            RestClient clientDB = new RestClient("https://news-nghiepradeon-app.herokuapp.com/api/v1/news/all");

            RestRequest requestDB = new RestRequest(Method.GET);
            IRestResponse res = clientDB.Execute(requestDB);
            string con = res.Content;
            var dataVNE = JsonConvert.DeserializeObject<ListNewsVNE>(con);
            var ListNewsTTR = new List<CMS_News>();
            if (dataVNE != null)
            {
                foreach (var news in dataVNE.data)
                {
                    CMS_News n = new CMS_News()
                    {
                        Id = 0,
                        FName = news.title,
                        FDescription = news.link,
                        FCreateTime = DateTime.Parse(news.pubDate),
                        SortContent = news.description,
                        FLevel = 2,
                        Image = news.img,
                        FBranchCode = "TTO"
                    };
                    ListNewsTTR.Add(n);
                }
            }
            var data = new ListNews()
            {
                list = ListNewsTTR,
                total = ListNewsTTR.Count()
            };
            return (data);
        }
        public ListNews GetFromVNExpress()
        {
            RestClient clientDB = new RestClient("https://soa2022.herokuapp.com/api/getListFeedMessage");

            RestRequest requestDB = new RestRequest(Method.POST);
            IRestResponse res = clientDB.Execute(requestDB);
            string con = res.Content;
            var dataVNE = JsonConvert.DeserializeObject<ListNewsVNE>(con);
            var ListNewsVNE = new List<CMS_News>();
            if (dataVNE != null)
            {
                foreach (var news in dataVNE.data)
                {
                    CMS_News n = new CMS_News()
                    {
                        Id = 0,
                        FName = news.title,
                        FDescription = news.link,
                        FCreateTime = DateTime.Parse(news.pubDate),
                        SortContent = news.description,
                        FLevel = 2,
                        Image = news.img,
                        FBranchCode = "VNE"
                    };
                    ListNewsVNE.Add(n);
                }
            }
            var data = new ListNews()
            {
                list = ListNewsVNE,
                total = ListNewsVNE.Count()
            };
            return (data);
        }
        [HttpGet]
        [Route("GetList10BienBan")]
        public IHttpActionResult GetListBienBan(int pageNumber, int pageSize, string searchKey, string TAG)
        {
            try
            {
                var ListNews = new ListNews();
                if (TAG == "tin-tuc-su-kien") {
                    ListNews = GetFromVNExpress();
                    var ListNews2 = GetFromTuoiTre();
                    if (ListNews2 != null)
                    {
                        ListNews.list.AddRange(ListNews2.list);
                        ListNews.total += ListNews2.total;
                    }
                }
                else GetListNewsFromLocal(pageNumber, pageSize, searchKey, TAG);
                var respon = new
                {
                    list = ListNews.list.OrderByDescending(x => x.FCreateTime).Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList(),
                    total = ListNews.total
                };
                return Ok(respon);
            }
            catch (Exception ex)
            {
                string branchCode = HttpContext.Current.Request.Headers["x-company"];
                string language = HttpContext.Current.Request.Headers["x-language"];
                var data = db.CMS_News.Where(x => x.FInUse == true && (x.Menu == TAG || x.Tags.Contains(TAG)) && x.FLanguage == language
                     && (x.FName.Contains(searchKey) || x.FDescription.Contains(searchKey) || x.Content.Contains(searchKey) || x.SortContent.Contains(searchKey) || string.IsNullOrEmpty(searchKey)))
                        .OrderByDescending(x => x.FCreateTime)
                        .Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
                var count = db.CMS_News.Where(x => x.FInUse == true && (x.Menu == TAG || x.Tags.Contains(TAG)) && x.FLanguage == language
                     && (x.FName.Contains(searchKey) || x.FDescription.Contains(searchKey) || x.Content.Contains(searchKey) || x.SortContent.Contains(searchKey) || string.IsNullOrEmpty(searchKey))).Count();
                var total = 0;
                if (count > 0) total = (count % pageSize == 0) ? count / pageSize : (count / pageSize + 1);
                var respon = new
                {
                    list = data,
                    total = total
                };
                return Ok(respon);
            }
        }
        public ListNews GetListNewsFromLocal(int pageNumber, int pageSize, string searchKey, string TAG)
        {
            try
            {
                string branchCode = HttpContext.Current.Request.Headers["x-company"];
                string language = HttpContext.Current.Request.Headers["x-language"];
                var data = db.CMS_News.Where(x => x.FInUse == true && (x.Menu == TAG || x.Tags.Contains(TAG)) && x.FLanguage == language
                     && (x.FName.Contains(searchKey) || x.FDescription.Contains(searchKey) || x.Content.Contains(searchKey) || x.SortContent.Contains(searchKey) || string.IsNullOrEmpty(searchKey)))
                        .OrderByDescending(x => x.FCreateTime)
                        .Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
                var count = db.CMS_News.Where(x => x.FInUse == true && (x.Menu == TAG || x.Tags.Contains(TAG)) && x.FLanguage == language
                     && (x.FName.Contains(searchKey) || x.FDescription.Contains(searchKey) || x.Content.Contains(searchKey) || x.SortContent.Contains(searchKey) || string.IsNullOrEmpty(searchKey))).Count();
                var total = 0;
                if (count > 0) total = (count % pageSize == 0) ? count / pageSize : (count / pageSize + 1);
                var respon = new ListNews()
                {
                    list = data,
                    total = total
                };
                return respon;
            }
            catch (System.Exception ex)
            {
                ex.ToString();
                return null;
            }
        }

        // GET: api/Groups
        [HttpGet]
        [Route("GetTopNews")]
        public IHttpActionResult GetTopNews(string searchKey)
        {
            string branchCode = HttpContext.Current.Request.Headers["x-company"];
            string language = HttpContext.Current.Request.Headers["x-language"];
            var data = db.CMS_News.Where(x => x.FInUse == true && x.IsTopNews == true
                 && (x.FName.Contains(searchKey) || x.FDescription.Contains(searchKey) || x.Content.Contains(searchKey) || x.SortContent.Contains(searchKey) || string.IsNullOrEmpty(searchKey)))
                    .OrderByDescending(x => x.FCreateTime)
                    .Skip((1 - 1) * 5).Take(5).ToList();
            var data2 = db.CMS_News.Where(x => x.FInUse == true && x.IsTopNews == true
                && (x.FName.Contains(searchKey) || x.FDescription.Contains(searchKey) || x.Content.Contains(searchKey) || x.SortContent.Contains(searchKey) || string.IsNullOrEmpty(searchKey)))
                   .OrderByDescending(x => x.FCreateTime)
                   .Skip((2 - 1) * 5).Take(5).ToList();
            var respon = new
            {
                list = data,
                list2 = data2
            };
            return Ok(respon);
        }
        [HttpGet]
        [Route("GetTags")]
        public IHttpActionResult GetTags(string code)
        {
            string branchCode = HttpContext.Current.Request.Headers["x-company"];
            string language = HttpContext.Current.Request.Headers["x-language"];
            var gt = db.Group_Tags.Where(x => x.CodeNews == code && x.FInUse == true).Select(y => y.CodeTag).ToList();
            var data = new List<TAG>();
            foreach(var d in gt)
            {
                var tag = db.TAGS.Where(x => x.FLanguage == language && x.FCode == d).FirstOrDefault();
                data.Add(tag);
            }
            
            var respon = new
            {
                list = data
            };
            return Ok(respon);
        }
        [HttpGet]
        [Route("GetListTags")]
        public IHttpActionResult GetListTags()
        {
            string branchCode = HttpContext.Current.Request.Headers["x-company"];
            string language = HttpContext.Current.Request.Headers["x-language"];
            var data = db.TAGS.Where(x => x.FLanguage == language).ToList();
            var respon = new
            {
                list = data
            };
            return Ok(respon);
        }
        [HttpGet]
        [Route("GetAllNews")]
        public IHttpActionResult GetAllNews(string searchKey)
        {
            string branchCode = HttpContext.Current.Request.Headers["x-company"];
            string language = HttpContext.Current.Request.Headers["x-language"];
            string news = "";
            if (language == "vi-VN") news = "tin-tuc-su-kien";
            else news = "news";
            var data = db.CMS_News.Where(x => x.FInUse == true && x.IsLiveNews != true && x.Menu == news
                 && (x.FName.Contains(searchKey) || x.FDescription.Contains(searchKey) || x.Content.Contains(searchKey) || x.SortContent.Contains(searchKey) || string.IsNullOrEmpty(searchKey)))
                    .OrderByDescending(x => x.FCreateTime)
                    .Skip((1 - 1) * 10).Take(10).ToList();
            var respon = new
            {
                list = data
            };
            return Ok(respon);
        }
        [HttpGet]
        [Route("GetCDTD")]
        public IHttpActionResult GetCDTD()
        {
            var data = db.CMS_News.Where(x => x.FInUse == true && (x.Tags.Contains("chinh-sach-doi-ngoai") || x.Tags.Contains("foreign-policy")))
                    .OrderByDescending(x => x.FCreateTime)
                    .Skip((1 - 1) * 3).Take(3).ToList();
            return Ok(data);
        }
        [HttpGet]
        [Route("GetNewsNCQG")]
        public IHttpActionResult GetNewsNCQG()
        {
            var data = db.CMS_News.Where(x => x.FInUse == true && (x.Tags.Contains("chu-quyen-lanh-tho") || x.Tags.Contains("territorial-sovereignty")))
                    .OrderByDescending(x => x.FCreateTime)
                    .Skip((1 - 1) * 5).Take(5).ToList();
            return Ok(data);
        }
        [HttpGet]
        [Route("GetNewsBienBan")]
        public IHttpActionResult GetNewsBienBan()
        {
            var data = db.CMS_News.Where(x => x.FInUse == true && (x.Tags.Contains("van-ban-phap-quy") || x.Tags.Contains("legal-documents")))
                    .OrderByDescending(x => x.FCreateTime)
                    .Skip((1 - 1) * 5).Take(5).ToList();
            return Ok(data);
        }
        [HttpGet]
        [Route("GetNewsTieuBan")]
        public IHttpActionResult GetNewsTieuBan()
        {
            var data = db.CMS_News.Where(x => x.FInUse == true && (x.Tags.Contains("thong-tin-kt-xh") || x.Tags.Contains("economy-society")))
                    .OrderByDescending(x => x.FCreateTime)
                    .Skip((1 - 1) * 5).Take(5).ToList();
            return Ok(data);
        }
        [HttpGet]
        [Route("GetNewsThongBao")]
        public IHttpActionResult GetNewsThongBao()
        {
            var data = db.CMS_News.Where(x => x.FInUse == true && x.IsImportantNews == true)
                    .OrderByDescending(x => x.FCreateTime)
                    .Skip((1 - 1) * 5).Take(5).ToList();
            return Ok(data);
        }
        [HttpGet]
        [Route("GetHotNews")]
        public IHttpActionResult GetHotNews()
        {
            string branchCode = HttpContext.Current.Request.Headers["x-company"];
            string language = HttpContext.Current.Request.Headers["x-language"];
            var data = db.CMS_News.Where(x => x.FInUse == true && x.IsHotNews == true)
                    .OrderByDescending(x => x.FCreateTime)
                    .Skip((1 - 1) * 10).Take(10).ToList();
            var respon = new
            {
                list = data
            };
            return Ok(respon);
        }
        [HttpGet]
        [Route("ViewNews")]
        public IHttpActionResult ViewNews(int Id)
        {
            string branchCode = HttpContext.Current.Request.Headers["x-company"];
            string language = HttpContext.Current.Request.Headers["x-language"];
            var data = db.CMS_News.Where(x => x.Id == Id).FirstOrDefault();
            return Ok(data);
        }

        public class ListNews
        {
            public List<CMS_News> list { get; set; }
            public int total { get; set; }
        }
        public class News_VNE
        {
            public string title { get; set; }
            public string description { get; set; }
            public string pubDate { get; set; }
            public string link { get; set; }
            public string img { get; set; }
            public int FLevel { get; set; }
        }
        public class ListNewsVNE
        {
            public bool success { get; set; }
            public List<News_VNE> data { get; set; }
        }
    }
    public class Comments
    {
        public string Name;
        public string Email;
        public string WebSite;
        public string Comment;
        public long NewsId;
    }
    public class Contact
    {
        public string Name;
        public string Email;
        public string PhoneNumber;
        public string Comment;
        public string Address;
    }
}