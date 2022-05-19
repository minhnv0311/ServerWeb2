using Microsoft.AspNetCore.StaticFiles;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using VSDCompany.Models;


namespace VSDCompany.Controllers
{
    [Authorize]
    public class StatisticController : ApiController
    {
        private Entities db = new Entities();

        [HttpGet]
        [Route("api/statistic/admissions")]
        public IHttpActionResult admissions(int Id)
        {
            try
            {
                var HoSo = db.HO_SO_ONLINE.Where(x => x.Id == Id).FirstOrDefault();
                if (HoSo.TrangThaiHS == "DA_TRAKQ") HoSo.TrangThaiHS = "DANG_XL";
                else HoSo.TrangThaiHS = "TN_OL";
                var DinhKem = db.DINH_KEM_ONLINE.Where(x => x.MAHOSO == HoSo.FCode && x.TepKetQua == true).ToList();
                foreach (var dk in DinhKem)
                {
                    db.DINH_KEM_ONLINE.Remove(dk);
                }
                db.SaveChanges();
                var result = new
                {
                    response_code = "00",
                    response_data = 1
                };
                return Ok(result);
            }
            catch (System.Exception e)
            {
                var result = new
                {
                    response_code = "-1",
                    response_data = e.ToString()
                };
                return Ok(result);
            }
        }
    }
}
