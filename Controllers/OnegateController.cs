using Aspose.Words;
using Microsoft.AspNetCore.StaticFiles;
using Newtonsoft.Json;
using NPOI.OpenXmlFormats.Wordprocessing;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.XWPF.UserModel;
using RestSharp;
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
    /// <summary>
    /// Quản lý tiếp nhận và trả kết quả
    /// </summary>
    [Authorize]
    public class OnegateController : ApiController
    {
        private Entities db = new Entities();

        #region View Ho so

        public class Result
        {
            public List<Get_List_HoSo_Result> collResult { get; set; }
            public int ResultPerPage { get; set; }
            public int lastPage { get; set; }
            public int TotalCount { get; set; }
        }
        public class Result_KQ
        {
            public List<Get_List_Result_Result> collResult { get; set; }
            public int ResultPerPage { get; set; }
            public int lastPage { get; set; }
            public int TotalCount { get; set; }
        }
        [HttpGet]
        [Route("api/onegate/list_files")]
        public IHttpActionResult list_files(string Status, string SearchKey, int currPage, int pageSize)
        {
            try
            {
                var namhoc = db.HO_SO_TUYENSINH.OrderByDescending(x => x.Id).Select(y => y.NamHoc).FirstOrDefault();
                var start = currPage * pageSize - pageSize + 1;
                var end = currPage * pageSize;
                var user = User.Identity.Name;
                var DV_User = db.UserProfiles.Where(x => x.UserName == user).Select(y => y.DonVi).FirstOrDefault();
                if (Status != "DA_TRAKQ" && Status != "LOAI")
                {
                    var result = db.Get_List_HoSo(DV_User, Status, SearchKey).Where(x => x.NamHoc == namhoc).ToList();
                    var collResult = result.Where(x => x.FIndex >= start && x.FIndex <= end && x.NamHoc == namhoc).ToList();
                    var RS = new Result()
                    {
                        collResult = collResult,
                        ResultPerPage = collResult.Count(),
                        lastPage = (result.Count() % pageSize == 0) ? (result.Count() / pageSize) : (result.Count() / pageSize + 1),
                        TotalCount = result.Count()
                    };
                    var response = new
                    {
                        response_code = "0",
                        response_desc = "Thành công",
                        response_data = RS
                    };
                    return Ok(response);
                }
                else
                {
                    var result = db.Get_List_Result(DV_User, Status, SearchKey).Where(x => x.NamHoc == namhoc).ToList();
                    var collResult = result.Where(x => x.FIndex >= start && x.FIndex <= end && x.NamHoc == namhoc).ToList();
                    var RS = new Result_KQ()
                    {
                        collResult = collResult,
                        ResultPerPage = collResult.Count(),
                        lastPage = (result.Count() % pageSize == 0) ? (result.Count() / pageSize) : (result.Count() / pageSize + 1),
                        TotalCount = result.Count()
                    };
                    var response = new
                    {
                        response_code = "0",
                        response_desc = "Thành công",
                        response_data = RS
                    };
                    return Ok(response);
                }
            }
            catch (System.Exception ex)
            {
                var response = new
                {
                    response_code = "-1",
                    response_desc = "Lỗi",
                    response_data = ex.ToString()
                };
                return Ok(response);
            }
        }

        //Lấy thông tin hồ sơ
        [HttpGet]
        [Route("api/onegate/lay_ho_so")]
        public IHttpActionResult lay_ho_so(string Code)
        {
            try
            {
                string NoiSinh = "";
                string HKTT = "";
                string NguoiGiamHo_CuTru = "";
                string DanToc = "";
                var HS_OL = db.HO_SO_ONLINE.Where(x => x.FCode == Code).FirstOrDefault();
                var HS_TT = db.HO_SO_ONLINE_THUTUC.Where(x => x.MaHoSo == Code).ToList();
                var HS_DK = db.DINH_KEM_ONLINE.Where(x => x.MAHOSO == Code).ToList();
                var HS_TS = db.HO_SO_TUYENSINH.Where(x => x.MaHoSo == Code).FirstOrDefault();
                var KQHT = db.KET_QUA_HOC_TAP.Where(x => x.MaHoSo == Code).FirstOrDefault();
                List<NguyenVong> nguyenVong = new List<NguyenVong>();
                var NGV = db.NGUYEN_VONG.Where(x => x.MaHoSo == Code).ToList();
                //var NGV2 = db.NGUYEN_VONG.Where(x => x.MaHoSoNV1 == Code).FirstOrDefault();
                foreach(var ngv in NGV)
                {
                    if(ngv.MonHoc != null) nguyenVong.Add(JsonConvert.DeserializeObject<NguyenVong>(ngv.MonHoc));
                }
                //if (NGV2 != null)
                //{
                //    NguyenVong nguyenVong2 = NGV2.MonHoc != null ? JsonConvert.DeserializeObject<NguyenVong>(NGV2.MonHoc) : null;
                //    if (nguyenVong2 != null) listNV.Add(nguyenVong2);
                //}
                if (HS_TS != null)
                {
                    NoiSinh = ((HS_TS.NoiSinh != "" && HS_TS.NoiSinh != null) ? HS_TS.NoiSinh + ", " : null)
                            + ((HS_TS.XaNoiSinh != "" && HS_TS.XaNoiSinh != null) ? db.Areas.Where(x => x.FCode == HS_TS.XaNoiSinh).Select(y => y.FName).FirstOrDefault() + ", " : null)
                            + ((HS_TS.HuyenNoiSinh != "" && HS_TS.HuyenNoiSinh != null) ? db.Areas.Where(x => x.FCode == HS_TS.HuyenNoiSinh).Select(y => y.FName).FirstOrDefault() + ", " : null)
                            + ((HS_TS.TinhNoiSinh != "" && HS_TS.TinhNoiSinh != null) ? db.Areas.Where(x => x.FCode == HS_TS.TinhNoiSinh).Select(y => y.FName).FirstOrDefault() : null);
                    HKTT = ((HS_TS.CuTru != "" && HS_TS.CuTru != null) ? HS_TS.CuTru + ", " : null)
                         + ((HS_TS.XaCuTru != "" && HS_TS.XaCuTru != null) ? db.Areas.Where(x => x.FCode == HS_TS.XaCuTru).Select(y => y.FName).FirstOrDefault() + ", " : null)
                         + ((HS_TS.HuyenCuTru != "" && HS_TS.HuyenCuTru != null) ? db.Areas.Where(x => x.FCode == HS_TS.HuyenCuTru).Select(y => y.FName).FirstOrDefault() + ", " : null)
                         + ((HS_TS.TinhCuTru != "" && HS_TS.TinhCuTru != null) ? db.Areas.Where(x => x.FCode == HS_TS.TinhCuTru).Select(y => y.FName).FirstOrDefault() : null);
                    NguoiGiamHo_CuTru = ((HS_TS.NguoiGiamHo_CuTru != "" && HS_TS.NguoiGiamHo_CuTru != null) ? HS_TS.NguoiGiamHo_CuTru + ", " : null)
                                      + ((HS_TS.NguoiGiamHo_XaCuTru != "" && HS_TS.NguoiGiamHo_XaCuTru != null) ? db.Areas.Where(x => x.FCode == HS_TS.NguoiGiamHo_XaCuTru).Select(y => y.FName).FirstOrDefault() + ", " : null)
                                      + ((HS_TS.NguoiGiamHo_HuyenCuTru != "" && HS_TS.NguoiGiamHo_HuyenCuTru != null) ? db.Areas.Where(x => x.FCode == HS_TS.NguoiGiamHo_HuyenCuTru).Select(y => y.FName).FirstOrDefault() + ", " : null)
                                      + ((HS_TS.NguoiGiamHo_TinhCuTru != "" && HS_TS.NguoiGiamHo_TinhCuTru != null) ? db.Areas.Where(x => x.FCode == HS_TS.NguoiGiamHo_TinhCuTru).Select(y => y.FName).FirstOrDefault() : null);
                    DanToc = HS_TS.DanToc != null ? db.Nations.Where(x => x.FCode == HS_TS.DanToc).Select(y => y.FName).FirstOrDefault() : null;
                }
                var HS = new HOSO_OL_RS()
                {
                    HoSoOnline = HS_OL,
                    HoSoTuyenSinh = HS_TS,
                    HoSoOnlineThuTuc = HS_TT,
                    DinhKemOnline = HS_DK,
                    KetQuaHocTap = KQHT,
                    NguyenVong = nguyenVong,
                    NoiSinh = NoiSinh,
                    HKTT = HKTT,
                    DanToc = DanToc,
                    NguoiGiamHo_CuTru = NguoiGiamHo_CuTru
                };
                var response = new
                {
                    response_code = "0",
                    response_desc = "Thành công",
                    response_data = HS
                };
                return Ok(response);

            }
            catch (System.Exception ex)
            {
                var response = new
                {
                    response_code = "-1",
                    response_desc = "Lỗi",
                    response_data = ex.ToString()
                };
                return Ok(response);
            }
        }
        //Lấy thông tin hồ sơ
        [HttpGet]
        [Route("api/onegate/lay_ho_so_save")]
        public IHttpActionResult lay_ho_so_save(string Code)
        {
            try
            {
                var HS_OL = db.HO_SO_ONLINE.Where(x => x.FCode == Code).FirstOrDefault();
                var HS_TT = db.HO_SO_ONLINE_THUTUC.Where(x => x.MaHoSo == Code).ToList();
                var HS_DK = db.DINH_KEM_ONLINE.Where(x => x.MAHOSO == Code).ToList();
                var HS_TS = db.HO_SO_TUYENSINH.Where(x => x.MaHoSo == Code).FirstOrDefault();
                var KQHT = db.KET_QUA_HOC_TAP.Where(x => x.MaHoSo == Code).FirstOrDefault();
                List<NguyenVong> listNV = new List<NguyenVong>();
                NguyenVong nguyenVong = new NguyenVong();
                var NGV = db.NGUYEN_VONG.Where(x => x.MaHoSo == Code).FirstOrDefault();
                var NGV2 = db.NGUYEN_VONG.Where(x => x.MaHoSoNV1 == Code).FirstOrDefault();
                if (NGV != null)
                {
                    nguyenVong = NGV.MonHoc != null ? JsonConvert.DeserializeObject<NguyenVong>(NGV.MonHoc) : null;
                    if (nguyenVong != null) listNV.Add(nguyenVong);
                }
                if (NGV2 != null)
                {
                    NguyenVong nguyenVong2 = NGV2.MonHoc != null ? JsonConvert.DeserializeObject<NguyenVong>(NGV2.MonHoc) : null;
                    if (nguyenVong2 != null) listNV.Add(nguyenVong2);
                }
                var HS = new HOSO_OL_SAVE()
                {
                    HoSoOnline = HS_OL,
                    HoSoTuyenSinh = HS_TS,
                    HoSoOnlineThuTuc = HS_TT,
                    DinhKemOnline = HS_DK,
                    KetQuaHocTap = KQHT,
                    NguyenVong = listNV,
                };

                var response = new
                {
                    response_code = "0",
                    response_desc = "Thành công",
                    response_data = HS
                };
                return Ok(response);
            }
            catch (System.Exception ex)
            {
                var response = new
                {
                    response_code = "-1",
                    response_desc = "Lỗi",
                    response_data = ex.ToString()
                };
                return Ok(response);
            }
        }

        [HttpGet]
        [Route("api/onegate/downloadFile")]
        public HttpResponseMessage downloadFile(string Code)
        {
            var file = db.DINH_KEM_ONLINE.Where(x => x.MATHUTUC == Code).FirstOrDefault();
            string filepath = HttpContext.Current.Server.MapPath("/Uploads/Documents/" + file.FCode);
            var stream = new FileStream(filepath, FileMode.Open);
            HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);
            result.Content = new StreamContent(stream);
            result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
            result.Content.Headers.ContentDisposition.FileName = Path.GetFileName(filepath);
            result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            result.Content.Headers.ContentLength = stream.Length;
            return result;
        }

        #endregion

        #region Thao tac ho so

        [HttpGet]
        [Route("api/onegate/deny")]
        public IHttpActionResult deny(int Id, string Lydo)
        {
            using (var dbContextTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    List<HsDongBo> lhs = new List<HsDongBo>();
                    var HoSo = db.HO_SO_ONLINE.Where(x => x.Id == Id).FirstOrDefault();
                    HoSo.TrangThaiHS = "LOAI";
                    HoSo.NoiDungYKien = Lydo;
                    try
                    {
                        HsDongBo hs = new HsDongBo();
                        hs.MaHoSo = HoSo.FCode;
                        hs.SoBienNhan = "";
                        if (HoSo.MaDichVu == "3.000181.000.00.00.H22")
                        {
                            hs.TenTTHC = "Tuyển sinh trung học phổ thông";
                            hs.MaLinhVuc = "G03-GD10";
                            hs.TenLinhVuc = "Giáo dục Trung học";
                            hs.MaTTHC = "3.000181.000.00.00.H22";
                        }
                        if (HoSo.MaDichVu == "3.000182.000.00.00.H22")
                        {
                            hs.TenTTHC = "Tuyển sinh trung học cơ sở";
                            hs.MaLinhVuc = "G03-GD10";
                            hs.TenLinhVuc = "Giáo dục Trung học";
                            hs.MaTTHC = "3.000182.000.00.00.H22";
                        }
                        if (HoSo.MaDichVu == "TS_lop1.000.00.00.H22")
                        {
                            hs.TenTTHC = "Tuyển sinh tiểu học";
                            hs.MaLinhVuc = "G03-GD09";
                            hs.TenLinhVuc = "Giáo dục Tiểu học";
                            hs.MaTTHC = "2.002492";
                        }
                        if (HoSo.MaDichVu == "TS_MN.000.00.00.H22")
                        {
                            hs.TenTTHC = "Tuyển sinh mầm non";
                            hs.MaLinhVuc = "G03-GD06";
                            hs.TenLinhVuc = "Giáo dục Mầm non";
                            hs.MaTTHC = "2.002493";
                        }
                        hs.ChuHoSo = HoSo.HoTenNguoiNop;
                        hs.LoaiDoiTuong = "1";
                        hs.MaDoiTuong = HoSo.SoGiayTo;
                        hs.NgayTiepNhan = (HoSo.FCreateTime ?? DateTime.Now).ToString("yyyyMMddHHmmss");
                        hs.NgayHenTra = "";
                        hs.TrangThaiHoSo = "3";
                        hs.HinhThuc = "0";
                        hs.DonViXuLy = db.Organizations.Where(x => x.FCode == HoSo.MaDonVi).Select(y => y.FName).FirstOrDefault();
                        var ListFileDK = db.DINH_KEM_ONLINE.Where(x => x.MAHOSO == HoSo.FCode && (x.TepKetQua == null || x.TepKetQua == false)).ToList();
                        var ListFileKQ = db.DINH_KEM_ONLINE.Where(x => x.MAHOSO == HoSo.FCode && x.TepKetQua == true).ToList();
                        hs.TaiLieuNop = new List<DinhKem>();
                        foreach (var item in ListFileDK)
                        {
                            var dk = new DinhKem()
                            {
                                TepDinhKemId = item.Id.ToString(),
                                TenTepDinhKem = item.FName,
                                IsDeleted = "False",
                                MaThanhPhanHoSo = item.MATHUTUC,
                                DuongDanTaiTepTin = "http://10.0.80.189:8084/hgtest/dowload.aspx?dv=" + "tuyensinh.hagiang.gov.vn" + "&filename=" + item.FCode.Replace(" ", "+")
                            };
                            hs.TaiLieuNop.Add(dk);
                        }
                        hs.DanhSachGiayToKetQua = new List<DinhKemKetQua>();
                        foreach (var item in ListFileKQ)
                        {
                            var kq = new DinhKemKetQua()
                            {
                                GiayToId = item.Id.ToString(),
                                TenGiayTo = item.FName,
                                MaThanhPhanHoSo = item.MATHUTUC,
                                DuongDanTepTinKetQua = "http://10.0.80.189:8084/hgtest/dowload.aspx?dv=" + "tuyensinh.hagiang.gov.vn" + "&filename=" + item.FCode.Replace(" ", "+")
                            };
                            hs.DanhSachGiayToKetQua.Add(kq);
                        }
                        lhs.Add(hs);
                        db.SaveChanges();
                        DongBoTrangThaiHoSo(lhs, true);
                    }
                    catch (Exception ex)
                    {
                        var res = new
                        {
                            response_code = "-1",
                            response_data = ex.ToString()
                        };
                        return Ok(res);
                    }
                    db.SaveChanges();
                    dbContextTransaction.Commit();
                    var result = new
                    {
                        response_code = "00",
                        response_data = 1
                    };
                    return Ok(result);
                }
                catch (System.Exception e)
                {
                    dbContextTransaction.Rollback();
                    var result = new
                    {
                        response_code = "-1",
                        response_data = e.ToString()
                    };
                    return Ok(result);
                }
            }
        }

        [HttpGet]
        [Route("api/onegate/accept")]
        public IHttpActionResult accept(int Id)
        {
            using (var dbContextTransaction = db.Database.BeginTransaction())
            {
                try
                {
                List<HsDongBo> lhs = new List<HsDongBo>();
                var HoSo = db.HO_SO_ONLINE.Where(x => x.Id == Id).FirstOrDefault();
                HoSo.TrangThaiHS = "DANG_XL";
                try
                    {
                        HsDongBo hs = new HsDongBo();
                        hs.MaHoSo = HoSo.FCode;
                        hs.SoBienNhan = "";
                        if (HoSo.MaDichVu == "3.000181.000.00.00.H22")
                        {
                            hs.TenTTHC = "Tuyển sinh trung học phổ thông";
                            hs.MaLinhVuc = "G03-GD10";
                            hs.TenLinhVuc = "Giáo dục Trung học";
                            hs.MaTTHC = "3.000181.000.00.00.H22";
                        }
                        if (HoSo.MaDichVu == "3.000182.000.00.00.H22")
                        {
                            hs.TenTTHC = "Tuyển sinh trung học cơ sở";
                            hs.MaLinhVuc = "G03-GD10";
                            hs.TenLinhVuc = "Giáo dục Trung học";
                            hs.MaTTHC = "3.000182.000.00.00.H22";
                        }
                        if (HoSo.MaDichVu == "TS_lop1.000.00.00.H22")
                        {
                            hs.TenTTHC = "Tuyển sinh tiểu học";
                            hs.MaLinhVuc = "G03-GD09";
                            hs.TenLinhVuc = "Giáo dục Tiểu học";
                            hs.MaTTHC = "2.002492";
                        }
                        if (HoSo.MaDichVu == "TS_MN.000.00.00.H22")
                        {
                            hs.TenTTHC = "Tuyển sinh mầm non";
                            hs.MaLinhVuc = "G03-GD06";
                            hs.TenLinhVuc = "Giáo dục Mầm non";
                            hs.MaTTHC = "2.002493";
                        }
                        hs.ChuHoSo = HoSo.HoTenNguoiNop;
                        hs.LoaiDoiTuong = "1";
                        hs.MaDoiTuong = HoSo.SoGiayTo;
                        hs.NgayTiepNhan = (HoSo.FCreateTime ?? DateTime.Now).ToString("yyyyMMddHHmmss");
                        hs.NgayHenTra = "";
                        hs.TrangThaiHoSo = "1";
                        hs.HinhThuc = "0";
                        hs.DonViXuLy = db.Organizations.Where(x => x.FCode == HoSo.MaDonVi).Select(y => y.FName).FirstOrDefault();
                        var ListFileDK = db.DINH_KEM_ONLINE.Where(x => x.MAHOSO == HoSo.FCode && (x.TepKetQua == null || x.TepKetQua == false)).ToList();
                        var ListFileKQ = db.DINH_KEM_ONLINE.Where(x => x.MAHOSO == HoSo.FCode && x.TepKetQua == true).ToList();
                        hs.TaiLieuNop = new List<DinhKem>();
                        foreach (var item in ListFileDK)
                        {
                            var dk = new DinhKem()
                            {
                                TepDinhKemId = item.Id.ToString(),
                                TenTepDinhKem = item.FName,
                                IsDeleted = "False",
                                MaThanhPhanHoSo = item.MATHUTUC,
                                DuongDanTaiTepTin = "http://10.0.80.189:8084/hgtest/dowload.aspx?dv=" + "tuyensinh.hagiang.gov.vn" + "&filename=" + item.FCode.Replace(" ", "+")
                            };
                            hs.TaiLieuNop.Add(dk);
                        }
                        hs.DanhSachGiayToKetQua = new List<DinhKemKetQua>();
                        foreach (var item in ListFileKQ)
                        {
                            var kq = new DinhKemKetQua()
                            {
                                GiayToId = item.Id.ToString(),
                                TenGiayTo = item.FName,
                                MaThanhPhanHoSo = item.MATHUTUC,
                                DuongDanTepTinKetQua = "http://10.0.80.189:8084/hgtest/dowload.aspx?dv=" + "tuyensinh.hagiang.gov.vn" + "&filename=" + item.FCode.Replace(" ", "+")
                            };
                            hs.DanhSachGiayToKetQua.Add(kq);
                        }
                        lhs.Add(hs);
                        var resp = new
                        {
                            response_code = "00",
                            response_data = 1
                        };
                        DongBoTrangThaiHoSo(lhs, false);
                        WriteLogToTextFile(JsonConvert.SerializeObject(lhs));
                        hs.TrangThaiHoSo = "2";
                        DongBoTrangThaiHoSo(lhs, true);
                    }
                    catch (Exception ex)
                    {
                        var res = new
                        {
                            response_code = "-1",
                            response_data = ex.ToString()
                        };
                        return Ok(res);
                    }
                    db.SaveChanges();
                    dbContextTransaction.Commit();
                    var result = new
                    {
                        response_code = "00",
                        response_data = 1
                    };
                    return Ok(result);
                }
                catch (System.Exception e)
                {
                    dbContextTransaction.Rollback();
                    var result = new
                {
                    response_code = "-1",
                    response_data = e.ToString()
                };
                return Ok(result);
                }
            }
        }

        [HttpPost]
        [Route("api/onegate/acceptmulti")]
        public IHttpActionResult acceptmulti(List<string> FCode)
        {
            using (var dbContextTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    var HO_SO_ONLINE = db.HO_SO_ONLINE.ToList();
                    var Organizations = db.Organizations.ToList();
                    var DINH_KEM_ONLINE = db.DINH_KEM_ONLINE.ToList();
                    foreach (var code in FCode)
                    {
                        try
                        {
                            List<HsDongBo> lhs = new List<HsDongBo>();
                            var HoSo = HO_SO_ONLINE.Where(x => x.FCode == code).FirstOrDefault();
                            HoSo.TrangThaiHS = "DANG_XL";
                            HsDongBo hs = new HsDongBo();
                            hs.MaHoSo = HoSo.FCode;
                            hs.SoBienNhan = "";
                            if (HoSo.MaDichVu == "3.000181.000.00.00.H22")
                            {
                                hs.TenTTHC = "Tuyển sinh trung học phổ thông";
                                hs.MaLinhVuc = "G03-GD10";
                                hs.TenLinhVuc = "Giáo dục Trung học";
                                hs.MaTTHC = "3.000181.000.00.00.H22";
                            }
                            if (HoSo.MaDichVu == "3.000182.000.00.00.H22")
                            {
                                hs.TenTTHC = "Tuyển sinh trung học cơ sở";
                                hs.MaLinhVuc = "G03-GD10";
                                hs.TenLinhVuc = "Giáo dục Trung học";
                                hs.MaTTHC = "3.000182.000.00.00.H22";
                            }
                            if (HoSo.MaDichVu == "TS_lop1.000.00.00.H22")
                            {
                                hs.TenTTHC = "Tuyển sinh tiểu học";
                                hs.MaLinhVuc = "G03-GD09";
                                hs.TenLinhVuc = "Giáo dục Tiểu học";
                                hs.MaTTHC = "2.002492";
                            }
                            if (HoSo.MaDichVu == "TS_MN.000.00.00.H22")
                            {
                                hs.TenTTHC = "Tuyển sinh mầm non";
                                hs.MaLinhVuc = "G03-GD06";
                                hs.TenLinhVuc = "Giáo dục Mầm non";
                                hs.MaTTHC = "2.002493";
                            }
                            hs.ChuHoSo = HoSo.HoTenNguoiNop;
                            hs.LoaiDoiTuong = "1";
                            hs.MaDoiTuong = HoSo.SoGiayTo;
                            hs.NgayTiepNhan = (HoSo.FCreateTime ?? DateTime.Now).ToString("yyyyMMddHHmmss");
                            hs.NgayHenTra = "";
                            hs.TrangThaiHoSo = "1";
                            hs.HinhThuc = "0";
                            hs.DonViXuLy = Organizations.Where(x => x.FCode == HoSo.MaDonVi).Select(y => y.FName).FirstOrDefault();
                            var ListFileDK = DINH_KEM_ONLINE.Where(x => x.MAHOSO == HoSo.FCode && (x.TepKetQua == null || x.TepKetQua == false)).ToList();
                            var ListFileKQ = DINH_KEM_ONLINE.Where(x => x.MAHOSO == HoSo.FCode && x.TepKetQua == true).ToList();
                            hs.TaiLieuNop = new List<DinhKem>();
                            foreach (var item in ListFileDK)
                            {
                                var dk = new DinhKem()
                                {
                                    TepDinhKemId = item.Id.ToString(),
                                    TenTepDinhKem = item.FName,
                                    IsDeleted = "False",
                                    MaThanhPhanHoSo = item.MATHUTUC,
                                    DuongDanTaiTepTin = "http://10.0.80.189:8084/hgtest/dowload.aspx?dv=" + "tuyensinh.hagiang.gov.vn" + "&filename=" + item.FCode.Replace(" ", "+")
                                };
                                hs.TaiLieuNop.Add(dk);
                            }
                            hs.DanhSachGiayToKetQua = new List<DinhKemKetQua>();
                            foreach (var item in ListFileKQ)
                            {
                                var kq = new DinhKemKetQua()
                                {
                                    GiayToId = item.Id.ToString(),
                                    TenGiayTo = item.FName,
                                    MaThanhPhanHoSo = item.MATHUTUC,
                                    DuongDanTepTinKetQua = "http://10.0.80.189:8084/hgtest/dowload.aspx?dv=" + "tuyensinh.hagiang.gov.vn" + "&filename=" + item.FCode.Replace(" ", "+")
                                };
                                hs.DanhSachGiayToKetQua.Add(kq);
                            }
                            lhs.Add(hs);
                            var resp = new
                            {
                                response_code = "00",
                                response_data = 1
                            };
                            DongBoTrangThaiHoSo(lhs, false);
                            WriteLogToTextFile(JsonConvert.SerializeObject(lhs));
                            hs.TrangThaiHoSo = "2";
                            DongBoTrangThaiHoSo(lhs, true);
                        }
                        catch (Exception ex)
                        {
                            ex.ToString();
                        }
                    }
                    db.SaveChanges();
                    dbContextTransaction.Commit();
                    var result = new
                    {
                        response_code = "00",
                        response_data = 1
                    };
                    return Ok(result);
                }
                catch (System.Exception e)
                {
                    dbContextTransaction.Rollback();
                    var result = new
                    {
                        response_code = "-1",
                        response_data = e.ToString()
                    };
                    return Ok(result);
                }
            }
        }

        [HttpGet]
        [Route("api/onegate/returnresult")]
        public IHttpActionResult returnresult(int Id, string Lydo, bool TrungTuyen)
        {
            using (var dbContextTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    List<HsDongBo> lhs = new List<HsDongBo>();
                    var HoSo = db.HO_SO_ONLINE.Where(x => x.Id == Id).FirstOrDefault();
                    HoSo.TrangThaiHS = "DA_TRAKQ";
                    HoSo.NoiDungYKien = Lydo;
                    HoSo.TrungTuyen = TrungTuyen;
                    try
                    {
                        HsDongBo hs = new HsDongBo();
                        hs.MaHoSo = HoSo.FCode;
                        hs.SoBienNhan = "";
                        if (HoSo.MaDichVu == "3.000181.000.00.00.H22")
                        {
                            hs.TenTTHC = "Tuyển sinh trung học phổ thông";
                            hs.MaLinhVuc = "G03-GD10";
                            hs.TenLinhVuc = "Giáo dục Trung học";
                            hs.MaTTHC = "3.000181.000.00.00.H22";
                        }
                        if (HoSo.MaDichVu == "3.000182.000.00.00.H22")
                        {
                            hs.TenTTHC = "Tuyển sinh trung học cơ sở";
                            hs.MaLinhVuc = "G03-GD10";
                            hs.TenLinhVuc = "Giáo dục Trung học";
                            hs.MaTTHC = "3.000182.000.00.00.H22";
                        }
                        if (HoSo.MaDichVu == "TS_lop1.000.00.00.H22")
                        {
                            hs.TenTTHC = "Tuyển sinh tiểu học";
                            hs.MaLinhVuc = "G03-GD09";
                            hs.TenLinhVuc = "Giáo dục Tiểu học";
                            hs.MaTTHC = "2.002492";
                        }
                        if (HoSo.MaDichVu == "TS_MN.000.00.00.H22")
                        {
                            hs.TenTTHC = "Tuyển sinh mầm non";
                            hs.MaLinhVuc = "G03-GD06";
                            hs.TenLinhVuc = "Giáo dục Mầm non";
                            hs.MaTTHC = "2.002493";
                        }
                        hs.ChuHoSo = HoSo.HoTenNguoiNop;
                        hs.LoaiDoiTuong = "1";
                        hs.MaDoiTuong = HoSo.SoGiayTo;
                        hs.NgayTiepNhan = (HoSo.FCreateTime ?? DateTime.Now).ToString("yyyyMMddHHmmss");
                        hs.NgayHenTra = "";
                        hs.TrangThaiHoSo = "10";
                        hs.HinhThuc = "0";
                        hs.DonViXuLy = db.Organizations.Where(x => x.FCode == HoSo.MaDonVi).Select(y => y.FName).FirstOrDefault();
                        var ListFileDK = db.DINH_KEM_ONLINE.Where(x => x.MAHOSO == HoSo.FCode && (x.TepKetQua == null || x.TepKetQua == false)).ToList();
                        var ListFileKQ = db.DINH_KEM_ONLINE.Where(x => x.MAHOSO == HoSo.FCode && x.TepKetQua == true).ToList();
                        hs.TaiLieuNop = new List<DinhKem>();
                        foreach (var item in ListFileDK) {
                            var dk = new DinhKem()
                            {
                                TepDinhKemId = item.Id.ToString(),
                                TenTepDinhKem = item.FName,
                                IsDeleted = "False",
                                MaThanhPhanHoSo = item.MATHUTUC,
                                DuongDanTaiTepTin = "http://10.0.80.189:8084/hgtest/dowload.aspx?dv=" + "tuyensinh.hagiang.gov.vn" + "&filename=" + item.FCode.Replace(" ", "+")
                            };
                            hs.TaiLieuNop.Add(dk);
                        }
                        hs.DanhSachGiayToKetQua = new List<DinhKemKetQua>();
                        foreach (var item in ListFileKQ)
                        {
                            var kq = new DinhKemKetQua()
                            {
                                GiayToId = item.Id.ToString(),
                                TenGiayTo = item.FName,
                                MaThanhPhanHoSo = item.MATHUTUC,
                                DuongDanTepTinKetQua = "http://10.0.80.189:8084/hgtest/dowload.aspx?dv=" + "tuyensinh.hagiang.gov.vn" + "&filename=" + item.FCode.Replace(" ", "+")
                            };
                            hs.DanhSachGiayToKetQua.Add(kq);
                        }
                        lhs.Add(hs);
                        DongBoTrangThaiHoSo(lhs, true);
                    }
                    catch (Exception ex)
                    {
                        var res = new
                        {
                            response_code = "-1",
                            response_data = ex.ToString()
                        };
                        return Ok(res);
                    }
                    db.SaveChanges();
                    dbContextTransaction.Commit();
                    var result = new
                    {
                        response_code = "00",
                        response_data = 1
                    };
                    return Ok(result);
                }
                catch (System.Exception e)
                {
                    dbContextTransaction.Rollback();
                    var result = new
                {
                    response_code = "-1",
                    response_data = e.ToString()
                };
                return Ok(result);
                }
            }
        }


        [HttpPost]
        [Route("api/onegate/returnresultmulti")]
        public IHttpActionResult returnresultmulti(List<string> FCode, string Lydo, bool TrungTuyen)
        {
            using (var dbContextTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    var HO_SO_ONLINE = db.HO_SO_ONLINE.ToList();
                    var Org = db.Organizations.ToList();
                    var LIST_DINHKEM = db.DINH_KEM_ONLINE.ToList();
                    foreach (var code in FCode)
                    {
                        try
                        {
                            List<HsDongBo> lhs = new List<HsDongBo>();
                            var HoSo = HO_SO_ONLINE.Where(x => x.FCode == code).FirstOrDefault();
                            HoSo.TrangThaiHS = "DA_TRAKQ";
                            HoSo.NoiDungYKien = Lydo;
                            HoSo.TrungTuyen = TrungTuyen;
                            HsDongBo hs = new HsDongBo();
                            hs.MaHoSo = code;
                            hs.SoBienNhan = "";
                            if (HoSo.MaDichVu == "3.000181.000.00.00.H22")
                            {
                                hs.TenTTHC = "Tuyển sinh trung học phổ thông";
                                hs.MaLinhVuc = "G03-GD10";
                                hs.TenLinhVuc = "Giáo dục Trung học";
                                hs.MaTTHC = "3.000181.000.00.00.H22";
                            }
                            if (HoSo.MaDichVu == "3.000182.000.00.00.H22")
                            {
                                hs.TenTTHC = "Tuyển sinh trung học cơ sở";
                                hs.MaLinhVuc = "G03-GD10";
                                hs.TenLinhVuc = "Giáo dục Trung học";
                                hs.MaTTHC = "3.000182.000.00.00.H22";
                            }
                            if (HoSo.MaDichVu == "TS_lop1.000.00.00.H22")
                            {
                                hs.TenTTHC = "Tuyển sinh tiểu học";
                                hs.MaLinhVuc = "G03-GD09";
                                hs.TenLinhVuc = "Giáo dục Tiểu học";
                                hs.MaTTHC = "2.002492";
                            }
                            if (HoSo.MaDichVu == "TS_MN.000.00.00.H22")
                            {
                                hs.TenTTHC = "Tuyển sinh mầm non";
                                hs.MaLinhVuc = "G03-GD06";
                                hs.TenLinhVuc = "Giáo dục Mầm non";
                                hs.MaTTHC = "2.002493";
                            }
                            hs.ChuHoSo = HoSo.HoTenNguoiNop;
                            hs.LoaiDoiTuong = "1";
                            hs.MaDoiTuong = HoSo.SoGiayTo;
                            hs.NgayTiepNhan = (HoSo.FCreateTime ?? DateTime.Now).ToString("yyyyMMddHHmmss");
                            hs.NgayHenTra = "";
                            hs.TrangThaiHoSo = "10";
                            hs.HinhThuc = "0";
                            hs.DonViXuLy = Org.Where(x => x.FCode == HoSo.MaDonVi).Select(y => y.FName).FirstOrDefault();
                            var ListFileDK = LIST_DINHKEM.Where(x => x.MAHOSO == HoSo.FCode && (x.TepKetQua == null || x.TepKetQua == false)).ToList();
                            var ListFileKQ = LIST_DINHKEM.Where(x => x.MAHOSO == HoSo.FCode && x.TepKetQua == true).ToList();
                            hs.TaiLieuNop = new List<DinhKem>();
                            foreach (var item in ListFileDK)
                            {
                                var dk = new DinhKem()
                                {
                                    TepDinhKemId = item.Id.ToString(),
                                    TenTepDinhKem = item.FName,
                                    IsDeleted = "False",
                                    MaThanhPhanHoSo = item.MATHUTUC,
                                    DuongDanTaiTepTin = "http://10.0.80.189:8084/hgtest/dowload.aspx?dv=" + "tuyensinh.hagiang.gov.vn" + "&filename=" + item.FCode.Replace(" ", "+")
                                };
                                hs.TaiLieuNop.Add(dk);
                            }
                            hs.DanhSachGiayToKetQua = new List<DinhKemKetQua>();
                            foreach (var item in ListFileKQ)
                            {
                                var kq = new DinhKemKetQua()
                                {
                                    GiayToId = item.Id.ToString(),
                                    TenGiayTo = item.FName,
                                    MaThanhPhanHoSo = item.MATHUTUC,
                                    DuongDanTepTinKetQua = "http://10.0.80.189:8084/hgtest/dowload.aspx?dv=" + "tuyensinh.hagiang.gov.vn" + "&filename=" + item.FCode.Replace(" ", "+")
                                };
                                hs.DanhSachGiayToKetQua.Add(kq);
                            }
                            lhs.Add(hs);
                            DongBoTrangThaiHoSo(lhs, true);
                        }
                        catch (Exception ex)
                        {
                            ex.ToString();
                        }
                    }
                    db.SaveChanges();
                    dbContextTransaction.Commit();
                    var result = new
                    {
                        response_code = "00",
                        response_data = 1
                    };
                    return Ok(result);
                }
                catch (System.Exception e)
                {
                    dbContextTransaction.Rollback();
                    var result = new
                    {
                        response_code = "-1",
                        response_data = e.ToString()
                    };
                    return Ok(result);
                }
            }
        }

        public void DongBoTrangThaiHoSo(List<HsDongBo> dshs_dongbo, bool isUpdating)
        {
            try
            {
                string isUpdate = isUpdating ? "True" : "False";
                RestClient clientDB = new RestClient("http://10.0.80.189:8084/test/api/SynsTTHCC/DongBoHoSoMC?MaDinhDanhDV=000.00.01.H22&isUpdating=" + isUpdate);

                var requestDB = new RestRequest("", Method.POST);
                requestDB.AddHeader("Content-Type", "application/json");
                requestDB.RequestFormat = DataFormat.Json;
                requestDB.AddBody(dshs_dongbo);
                var res = clientDB.Execute(requestDB);
                WriteLogToTextFile("DongBoTrangThaiHoSo_" + res.Content);
            }
            catch (System.Exception ex)
            {
                WriteLogToTextFile("DongBoTrangThaiHoSo_Lỗi không lấy được danh sách thủ tục hành chính" + ex.ToString());
            }
        }

        public void DongBoNguoiDung(string MaHoSo, string SoDienThoai)
        {
            try
            {
                string Url = "http://10.0.80.189:8084/hgtest/DongBoNguoiDungTS.aspx?sdt=" + SoDienThoai + "&mahoso=" + MaHoSo;
                RestClient clientDB = new RestClient(Url);
                var requestDB = new RestRequest("", Method.POST);
                requestDB.AddHeader("Content-Type", "application/json");
                requestDB.RequestFormat = DataFormat.Json;
                requestDB.AddBody(null);
                var res = clientDB.Execute(requestDB);
                WriteLogToTextFile("DongBoNguoiDung_" + Url + "_" + res.Content);
            }
            catch (System.Exception ex)
            {
                WriteLogToTextFile("DongBoNguoiDung_Lỗi không thể đồng bộ_" + ex.ToString());
            }
        }

        public void WriteLogToTextFile(string Error)
        {
            string path = HttpContext.Current.Server.MapPath("Logs_Error") + string.Format("\\error_{0}.txt", System.DateTime.Now.ToString("yyyyMMdd"));
            if (!File.Exists(path))
            {
                StreamWriter sw = File.CreateText(path);
                sw.Close();
            }
            if (File.Exists(path))
            {
                try
                {
                    using (StreamWriter sw = File.AppendText(path))
                    {
                        sw.WriteLine(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss.fff") + ": " + Error);
                        sw.Close();
                    }
                }
                catch { }

            }
        }

        public class returnSession
        {
            public string error_code { get; set; }
            public string message { get; set; }
            public string session { get; set; }

        }

        public class returnAPI
        {
            public returnSession Content { get; set; } 
        }

        public class HsDongBo
        {
            public string MaHoSo;
            public string MaTTHC;
            public string TenTTHC;
            public string MaLinhVuc; // = null
            public string TenLinhVuc; // = null
            public string SoBienNhan; // = null
            public string ChuHoSo;
            public string LoaiDoiTuong;
            public string MaDoiTuong;
            public string NgayTiepNhan;
            public string NgayHenTra;
            public string TrangThaiHoSo;
            public string HinhThuc; // = "0"
            public string DonViXuLy;
            public List<DinhKem> TaiLieuNop;
            public List<DinhKemKetQua> DanhSachGiayToKetQua;
        }

        public class DinhKemKetQua
        {
            public string TenGiayTo;
            public string MaThanhPhanHoSo;
            public string GiayToId;
            public string DuongDanTepTinKetQua;

        }

        public class DinhKem
        {
            public string TepDinhKemId;
            public string TenTepDinhKem;
            public string IsDeleted;
            public string MaThanhPhanHoSo;
            public string DuongDanTaiTepTin;
        }

        [HttpGet]
        [Route("api/onegate/cancel")]
        public IHttpActionResult cancel(int Id)
        {
            using (var dbContextTransaction = db.Database.BeginTransaction())
            {
                try
                {
                List<HsDongBo> lhs = new List<HsDongBo>();
                var HoSo = db.HO_SO_ONLINE.Where(x => x.Id == Id).FirstOrDefault();
                if (HoSo.TrangThaiHS == "DA_TRAKQ") HoSo.TrangThaiHS = "DANG_XL";
                else HoSo.TrangThaiHS = "TN_OL";
                try
                    {
                        HsDongBo hs = new HsDongBo();
                        hs.MaHoSo = HoSo.FCode;
                        hs.SoBienNhan = "";
                        if (HoSo.MaDichVu == "3.000181.000.00.00.H22")
                        {
                            hs.TenTTHC = "Tuyển sinh trung học phổ thông";
                            hs.MaLinhVuc = "G03-GD10";
                            hs.TenLinhVuc = "Giáo dục Trung học";
                            hs.MaTTHC = "3.000181.000.00.00.H22";
                        }
                        if (HoSo.MaDichVu == "3.000182.000.00.00.H22")
                        {
                            hs.TenTTHC = "Tuyển sinh trung học cơ sở";
                            hs.MaLinhVuc = "G03-GD10";
                            hs.TenLinhVuc = "Giáo dục Trung học";
                            hs.MaTTHC = "3.000182.000.00.00.H22";
                        }
                        if (HoSo.MaDichVu == "TS_lop1.000.00.00.H22")
                        {
                            hs.TenTTHC = "Tuyển sinh tiểu học";
                            hs.MaLinhVuc = "G03-GD09";
                            hs.TenLinhVuc = "Giáo dục Tiểu học";
                            hs.MaTTHC = "2.002492";
                        }
                        if (HoSo.MaDichVu == "TS_MN.000.00.00.H22")
                        {
                            hs.TenTTHC = "Tuyển sinh mầm non";
                            hs.MaLinhVuc = "G03-GD06";
                            hs.TenLinhVuc = "Giáo dục Mầm non";
                            hs.MaTTHC = "2.002493";
                        }
                        hs.ChuHoSo = HoSo.HoTenNguoiNop;
                        hs.LoaiDoiTuong = "1";
                        hs.MaDoiTuong = HoSo.SoGiayTo;
                        hs.NgayTiepNhan = (HoSo.FCreateTime ?? DateTime.Now).ToString("yyyyMMddHHmmss");
                        hs.NgayHenTra = "";
                        hs.TrangThaiHoSo = HoSo.TrangThaiHS == "DA_TRAKQ" ? "2" : "1";
                        hs.HinhThuc = "0";
                        hs.DonViXuLy = db.Organizations.Where(x => x.FCode == HoSo.MaDonVi).Select(y => y.FName).FirstOrDefault();
                        var ListFileDK = db.DINH_KEM_ONLINE.Where(x => x.MAHOSO == HoSo.FCode && (x.TepKetQua == null || x.TepKetQua == false)).ToList();
                        var ListFileKQ = db.DINH_KEM_ONLINE.Where(x => x.MAHOSO == HoSo.FCode && x.TepKetQua == true).ToList();
                        hs.TaiLieuNop = new List<DinhKem>();
                        foreach (var item in ListFileDK)
                        {
                            var dk = new DinhKem()
                            {
                                TepDinhKemId = item.Id.ToString(),
                                TenTepDinhKem = item.FName,
                                IsDeleted = "False",
                                MaThanhPhanHoSo = item.MATHUTUC,
                                DuongDanTaiTepTin = "http://10.0.80.189:8084/hgtest/dowload.aspx?dv=" + "tuyensinh.hagiang.gov.vn" + "&filename=" + item.FCode.Replace(" ", "+")
                            };
                            hs.TaiLieuNop.Add(dk);
                        }
                        hs.DanhSachGiayToKetQua = new List<DinhKemKetQua>();
                        lhs.Add(hs);
                        DongBoTrangThaiHoSo(lhs, true);
                        db.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        ex.ToString();
                    }
                    var DinhKem = db.DINH_KEM_ONLINE.Where(x => x.MAHOSO == HoSo.FCode && x.TepKetQua == true).ToList();
                    foreach (var dk in DinhKem)
                    {
                        db.DINH_KEM_ONLINE.Remove(dk);
                    }
                    db.SaveChanges();
                    dbContextTransaction.Commit();
                    var result = new
                    {
                        response_code = "00",
                        response_data = 1
                    };
                    return Ok(result);
                }
                catch (System.Exception e)
                {
                    dbContextTransaction.Rollback();
                    var result = new
                    {
                        response_code = "-1",
                        response_data = e.ToString()
                    };
                    return Ok(result);
                }
            }
        }

        [HttpGet]
        [Route("api/onegate/TraBoSung")]
        public IHttpActionResult TraBoSung(int Id, string LyDo)
        {
            using (var dbContextTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    List<HsDongBo> lhs = new List<HsDongBo>();
                    var HoSo = db.HO_SO_ONLINE.Where(x => x.Id == Id).FirstOrDefault();
                    HoSo.TrangThaiHS = "CBS";
                    var HoSoTS = db.HO_SO_TUYENSINH.Where(x => x.MaHoSo == HoSo.FCode).FirstOrDefault();
                    HoSoTS.FDescription = LyDo;
                    try
                    {
                        HsDongBo hs = new HsDongBo();
                        hs.MaHoSo = HoSo.FCode;
                        hs.SoBienNhan = "";
                        if (HoSo.MaDichVu == "3.000181.000.00.00.H22")
                        {
                            hs.TenTTHC = "Tuyển sinh trung học phổ thông";
                            hs.MaLinhVuc = "G03-GD10";
                            hs.TenLinhVuc = "Giáo dục Trung học";
                            hs.MaTTHC = "3.000181.000.00.00.H22";
                        }
                        if (HoSo.MaDichVu == "3.000182.000.00.00.H22")
                        {
                            hs.TenTTHC = "Tuyển sinh trung học cơ sở";
                            hs.MaLinhVuc = "G03-GD10";
                            hs.TenLinhVuc = "Giáo dục Trung học";
                            hs.MaTTHC = "3.000182.000.00.00.H22";
                        }
                        if (HoSo.MaDichVu == "TS_lop1.000.00.00.H22")
                        {
                            hs.TenTTHC = "Tuyển sinh tiểu học";
                            hs.MaLinhVuc = "G03-GD09";
                            hs.TenLinhVuc = "Giáo dục Tiểu học";
                            hs.MaTTHC = "2.002492";
                        }
                        if (HoSo.MaDichVu == "TS_MN.000.00.00.H22")
                        {
                            hs.TenTTHC = "Tuyển sinh mầm non";
                            hs.MaLinhVuc = "G03-GD06";
                            hs.TenLinhVuc = "Giáo dục Mầm non";
                            hs.MaTTHC = "2.002493";
                        }
                        hs.ChuHoSo = HoSo.HoTenNguoiNop;
                        hs.LoaiDoiTuong = "1";
                        hs.MaDoiTuong = HoSo.SoGiayTo;
                        hs.NgayTiepNhan = (HoSo.FCreateTime ?? DateTime.Now).ToString("yyyyMMddHHmmss");
                        hs.NgayHenTra = "";
                        hs.TrangThaiHoSo = "1";
                        hs.HinhThuc = "0";
                        hs.DonViXuLy = db.Organizations.Where(x => x.FCode == HoSo.MaDonVi).Select(y => y.FName).FirstOrDefault();
                        var ListFileDK = db.DINH_KEM_ONLINE.Where(x => x.MAHOSO == HoSo.FCode && (x.TepKetQua == null || x.TepKetQua == false)).ToList();
                        var ListFileKQ = db.DINH_KEM_ONLINE.Where(x => x.MAHOSO == HoSo.FCode && x.TepKetQua == true).ToList();
                        hs.TaiLieuNop = new List<DinhKem>();
                        foreach (var item in ListFileDK)
                        {
                            var dk = new DinhKem()
                            {
                                TepDinhKemId = item.Id.ToString(),
                                TenTepDinhKem = item.FName,
                                IsDeleted = "False",
                                MaThanhPhanHoSo = item.MATHUTUC,
                                DuongDanTaiTepTin = "http://10.0.80.189:8084/hgtest/dowload.aspx?dv=" + "tuyensinh.hagiang.gov.vn" + "&filename=" + item.FCode.Replace(" ", "+")
                            };
                            hs.TaiLieuNop.Add(dk);
                        }
                        hs.DanhSachGiayToKetQua = new List<DinhKemKetQua>();
                        lhs.Add(hs);
                        DongBoTrangThaiHoSo(lhs, true);
                        db.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        ex.ToString();
                    }
                    var DinhKem = db.DINH_KEM_ONLINE.Where(x => x.MAHOSO == HoSo.FCode && x.TepKetQua == true).ToList();
                    foreach (var dk in DinhKem)
                    {
                        db.DINH_KEM_ONLINE.Remove(dk);
                    }
                    db.SaveChanges();
                    dbContextTransaction.Commit();
                    var result = new
                    {
                        response_code = "00",
                        response_data = 1
                    };
                    return Ok(result);
                }
                catch (System.Exception e)
                {
                    dbContextTransaction.Rollback();
                    var result = new
                    {
                        response_code = "-1",
                        response_data = e.ToString()
                    };
                    return Ok(result);
                }
            }
        }

        #endregion

        #region Ham lien quan

        [HttpPost]
        [Route("api/onegate/upload")]
        public IHttpActionResult UploadMultipleFile(int Id)
        {
            string TenFile = "";
            string ex = "";
            var HoSo = db.HO_SO_ONLINE.Where(x => x.Id == Id).FirstOrDefault();
            bool exists = System.IO.Directory.Exists(HttpContext.Current.Server.MapPath("~/Uploads/Documents"));
            if (!exists)
                Directory.CreateDirectory(HttpContext.Current.Server.MapPath("~/Uploads/Documents"));
            HttpFileCollection httpRequest = System.Web.HttpContext.Current.Request.Files;
            using (DbContextTransaction transaction = db.Database.BeginTransaction())
            {
                try
                {
                    HttpPostedFile postedfile = httpRequest[0];
                    if (postedfile.ContentLength > 0)
                    {
                        TenFile = DateTime.Now.Ticks.ToString() + "_" + postedfile.FileName.Replace(" ", "");
                        var fileSavePath = Path.Combine(HttpContext.Current.Server.MapPath("~/Uploads/Documents"), TenFile);
                        postedfile.SaveAs(fileSavePath);
                        ex = Path.GetExtension(postedfile.FileName);
                    }
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    var result = new
                    {
                        response_code = "-1",
                        response_data = e.ToString()
                    };
                    return Ok(result);
                }
            }
            try
            {
                var DinhKem = new VSDCompany.Models.DINH_KEM_ONLINE()
                {
                    FCode = TenFile,
                    FName = TenFile,
                    MAHOSO = HoSo.FCode,
                    LOAI_FILE = ex,
                    TepKetQua = true,
                    FInUse = true
                };
                db.DINH_KEM_ONLINE.AddOrUpdate(DinhKem);
                db.SaveChanges();
                var result = new
                {
                    response_code = "00",
                    response_data = 1
                };
                return Ok(result);
            }
            catch (Exception e)
            {
                var result = new
                {
                    response_code = "-1",
                    response_data = e.ToString()
                };
                return Ok(result);
            }
        }

        [HttpPost]
        [Route("api/onegate/uploadmulti")]
        public IHttpActionResult UploadMultipleFile()
        {
            string TenFile = "";
            string ex = "";
            bool exists = System.IO.Directory.Exists(HttpContext.Current.Server.MapPath("~/Uploads/Documents"));
            if (!exists)
                Directory.CreateDirectory(HttpContext.Current.Server.MapPath("~/Uploads/Documents"));
            HttpFileCollection httpRequest = System.Web.HttpContext.Current.Request.Files;
            var ListFCode = System.Web.HttpContext.Current.Request.Params.GetValues(0);
            using (DbContextTransaction transaction = db.Database.BeginTransaction())
            {
                try
                {
                    HttpPostedFile postedfile = httpRequest[0];
                    if (postedfile.ContentLength > 0)
                    {
                        TenFile = DateTime.Now.Ticks.ToString() + "_" + postedfile.FileName.Replace(" ", "");
                        var fileSavePath = Path.Combine(HttpContext.Current.Server.MapPath("~/Uploads/Documents"), TenFile);
                        postedfile.SaveAs(fileSavePath);
                        ex = Path.GetExtension(postedfile.FileName);
                    }
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    var result = new
                    {
                        response_code = "-1",
                        response_data = e.ToString()
                    };
                    return Ok(result);
                }
            }
            try
            {
                foreach (var FCode in ListFCode)
                {
                    var DinhKem = new VSDCompany.Models.DINH_KEM_ONLINE()
                    {
                        FCode = TenFile,
                        FName = TenFile,
                        MAHOSO = FCode,
                        LOAI_FILE = ex,
                        TepKetQua = true,
                        FInUse = true
                    };
                    db.DINH_KEM_ONLINE.AddOrUpdate(DinhKem);
                    db.SaveChanges();
                }
                var result = new
                {
                    response_code = "00",
                    response_data = 1
                };
                return Ok(result);
            }
            catch (Exception e)
            {
                var result = new
                {
                    response_code = "-1",
                    response_data = e.ToString()
                };
                return Ok(result);
            }
        }

        [HttpGet]
        [Route("api/onegate/CountFiles")]
        public IHttpActionResult CountFiles(string NamHoc)
        {
            var Organizations = db.Organizations.ToList();
            var user = User.Identity.Name;
            var DV = db.UserProfiles.Where(x => x.UserName == user).Select(y => y.DonVi).FirstOrDefault();
            //var DV_C = Organizations.Where(x => x.FParent == user).Select(y => y.FCode).ToList();
            var HSTN = 0;
            var HSXL = 0;
            var TraKQ = 0;
            var yc = db.Count_HS_2(DV, NamHoc).ToList();
            HSTN += yc.Where(x => x.FInUse == true && x.TrangThaiHS == "TN_OL").Count();
            HSXL += yc.Where(x => x.FInUse == true && x.TrangThaiHS == "DANG_XL").Count();
            TraKQ += yc.Where(x => x.FInUse == true && x.TrangThaiHS == "DA_TRAKQ").Count();
            var RS = new
            {
                HSTN = HSTN,
                HSXL = HSXL,
                TraKQ = TraKQ
            };
            var response = new
            {
                response_code = "0",
                response_desc = "Thành công",
                response_data = RS
            };
            return Ok(response);
        }
        
        //[HttpGet]
        //[Route("api/onegate/CountFiles")]
        //public IHttpActionResult CountFiles(string user, string NamHoc)
        //{
        //    var Organizations = db.Organizations.ToList();
        //    var DV = db.UserProfiles.Where(x => x.Id == user).Select(y => y.DonVi).FirstOrDefault();
        //    var DV_C = Organizations.Where(x => x.FParent == user).Select(y => y.FCode).ToList();
        //    var HSTN = 0;
        //    var HSXL = 0;
        //    var TraKQ = 0;
        //    if (DV_C.Count == 0)
        //    {
        //        var yc = db.Count_HS(DV, NamHoc).ToList();
        //        HSTN += yc.Where(x => x.FInUse == true && x.TrangThaiHS == "TN_OL").Count();
        //        HSXL += yc.Where(x => x.FInUse == true && x.TrangThaiHS == "DANG_XL").Count();
        //        TraKQ += yc.Where(x => x.FInUse == true && x.TrangThaiHS == "DA_TRAKQ").Count();
        //    }
        //    else
        //    {
        //        foreach (var dvc in DV_C)
        //        {
        //            var DV_CC = Organizations.Where(x => x.FParent == dvc).Select(y => y.FCode).ToList();
        //            if (DV_C.Count == 0)
        //            {
        //                var yc = db.Count_HS(dvc, NamHoc).ToList();
        //                HSTN += yc.Where(x => x.FInUse == true && x.TrangThaiHS == "TN_OL").Count();
        //                HSXL += yc.Where(x => x.FInUse == true && x.TrangThaiHS == "DANG_XL").Count();
        //                TraKQ += yc.Where(x => x.FInUse == true && x.TrangThaiHS == "DA_TRAKQ").Count();
        //            }
        //            else
        //            {
        //                foreach (var dvcc in DV_CC)
        //                {
        //                    var yc = db.Count_HS(dvcc, NamHoc).ToList();
        //                    HSTN += yc.Where(x => x.FInUse == true && x.TrangThaiHS == "TN_OL").Count();
        //                    HSXL += yc.Where(x => x.FInUse == true && x.TrangThaiHS == "DANG_XL").Count();
        //                    TraKQ += yc.Where(x => x.FInUse == true && x.TrangThaiHS == "DA_TRAKQ").Count();
        //                }
        //            }
        //        }
        //    }
        //    var RS = new
        //    {
        //        HSTN = HSTN,
        //        HSXL = HSXL,
        //        TraKQ = TraKQ
        //    };
        //    var response = new
        //    {
        //        response_code = "0",
        //        response_desc = "Thành công",
        //        response_data = RS
        //    };
        //    return Ok(response);
        //}

        [HttpGet]
        [Route("api/onegate/YearChart")]
        public IHttpActionResult YearChart(string NamHoc)
        {
            var user = User.Identity.Name;
            var DV = db.UserProfiles.Where(x => x.UserName == user).Select(y => y.DonVi).FirstOrDefault();
            var DV_C = db.Organizations.Where(x => x.FParent == user).Select(y => y.FCode).ToList();
            int HST = 0;
            int HSKT = 0;
            if (DV_C.Count == 0)
            {
                var yc = db.Get_YearChart(DV, NamHoc).ToList();
                HST += yc[0].HocSinhThuong ?? 0;
                HSKT += yc[0].HocSinhKT ?? 0;
            }
            else
            {
                foreach (var dvc in DV_C)
                {
                    var DV_CC = db.Organizations.Where(x => x.FParent == dvc).Select(y => y.FCode).ToList();
                    if (DV_C.Count == 0)
                    {
                        var yc = db.Get_YearChart(dvc, NamHoc).ToList();
                        HST += yc[0].HocSinhThuong ?? 0;
                        HSKT += yc[0].HocSinhKT ?? 0;
                    }
                    else
                    {
                        foreach (var dvcc in DV_CC)
                        {
                            var yc = db.Get_YearChart(dvcc, NamHoc).ToList();
                            HST += yc[0].HocSinhThuong ?? 0;
                            HSKT += yc[0].HocSinhKT ?? 0;
                        }
                    }
                }
            }
            var yearchart = new YearChart()
            {
                NamHoc = NamHoc,
                HST = HST,
                HSKT = HSKT
            };
            var response = new
            {
                response_code = "0",
                response_desc = "Thành công",
                response_data = yearchart
            };
            return Ok(response);
        }

        public class DS_TrungTuyen
        {
            public List<Get_DS_TrungTuyen_Result> collResult { get; set; }
            public int ResultPerPage { get; set; }
            public int lastPage { get; set; }
            public int TotalCount { get; set; }
        }
        [HttpGet]
        [Route("api/onegate/DSTrungTuyen")]
        public IHttpActionResult DSTrungTuyen(string NamHoc, string SearchKey, int currPage, int pageSize)
        {
            var start = currPage * pageSize - pageSize + 1;
            var end = currPage * pageSize;
            var user = User.Identity.Name;
            var u = db.UserProfiles.Where(x => x.UserName == user).FirstOrDefault();
            var Org = db.Organizations.ToList();
            var DV = Org.Where(x => x.FCode == u.DonVi).FirstOrDefault();
            var DV_C = u.Type == "AD_PGD" ? Org.Where(x => x.Disctrict == DV.Disctrict).ToList() : Org.Where(x => x.FParent == u.DonVi).ToList();
            var DSTT = new List<Get_DS_TrungTuyen_Result>();
            int i = 1;
            if (DV_C.Count == 0)
            {
                var ds = db.Get_DS_TrungTuyen(DV.FCode, SearchKey, NamHoc).ToList();
                foreach (var d in ds)
                {
                    d.FIndex = i;
                    DSTT.Add(d);
                    i++;
                }
            }
            else
            {
                foreach (var dvc in DV_C)
                {
                    var DV_CC = u.Type == "AD_PGD" ? Org.Where(x => x.Disctrict == dvc.Disctrict).ToList() : Org.Where(x => x.FParent == dvc.FCode).ToList();
                    if (DV_C.Count == 0 || u.Type == "AD_PGD")
                    {
                        var ds = db.Get_DS_TrungTuyen(dvc.FCode, SearchKey, NamHoc).ToList();
                        foreach (var d in ds)
                        {
                            d.FIndex = i;
                            DSTT.Add(d);
                            i++;
                        }
                    }
                    else
                    {
                        foreach (var dvcc in DV_CC)
                        {
                            var ds = db.Get_DS_TrungTuyen(dvcc.FCode, SearchKey, NamHoc).ToList();
                            foreach (var d in ds)
                            {
                                d.FIndex = i;
                                DSTT.Add(d);
                                i++;
                            }
                        }
                    }
                }
            }
            var collResult = DSTT.Where(x => x.FIndex >= start && x.FIndex <= end).ToList();
            var RS = new DS_TrungTuyen()
            {
                collResult = collResult,
                ResultPerPage = collResult.Count(),
                lastPage = (DSTT.Count() % pageSize == 0) ? (DSTT.Count() / pageSize) : (DSTT.Count() / pageSize + 1),
                TotalCount = DSTT.Count()
            };
            var response = new
            {
                response_code = "0",
                response_desc = "Thành công",
                response_data = RS
            };
            return Ok(response);
        }

        public class NamHoc
        {
            public int index { get; set; }
            public string year { get; set; }
        }
        [HttpGet]
        [Route("api/onegate/GetYear")]
        public IHttpActionResult GetYear()
        {
            var user = User.Identity.Name;
            var u = db.UserProfiles.Where(x => x.UserName == user).FirstOrDefault();
            var NamHoc = (u.Type == "AD_PGD" || u.Type == "AD_SGD") ? db.HO_SO_TUYENSINH.Where(x => x.FInUse == true).OrderByDescending(y => y.Id).Select(z => z.NamHoc).Distinct().ToList() : db.HO_SO_TUYENSINH.Where(x => x.FInUse == true && x.MaDonVi == u.DonVi).OrderByDescending(y => y.Id).Select(z => z.NamHoc).Distinct().ToList();
            int i = 0;
            var rs = new List<NamHoc>();
            foreach (var d in NamHoc)
            {
                var nh = new NamHoc()
                {
                    index = i,
                    year = d
                };
                rs.Add(nh);
                i++;
            }
            var response = new
            {
                response_code = "0",
                response_desc = "Thành công",
                response_data = rs
            };
            return Ok(response);
        }

        #endregion

        #region Export

        [HttpGet]
        [Route("api/onegate/ExportExcel")]
        [Obsolete]
        public HttpResponseMessage ExportExcel(string NamHoc, string SearchKey)
        {
            var user = User.Identity.Name;
            var UserP = db.UserProfiles.Where(x => x.UserName == user).FirstOrDefault();
            var Org = db.Organizations.ToList();
            var DV = Org.Where(x => x.FCode == UserP.DonVi).FirstOrDefault();
            var DV_C = UserP.Type == "AD_PGD" ? Org.Where(x => x.Disctrict == DV.Disctrict).ToList() : Org.Where(x => x.FParent == UserP.DonVi).ToList();
            var DSTT = new List<Export_DS_TrungTuyen_Result>();
            int i = 1;
            if (DV_C.Count == 0)
            {
                var ds = db.Export_DS_TrungTuyen(DV.FCode, SearchKey, NamHoc).ToList();
                foreach (var d in ds)
                {
                    d.TT = i;
                    DSTT.Add(d);
                    i++;
                }
            }
            else
            {
                foreach (var dvc in DV_C)
                {
                    var DV_CC = UserP.Type == "AD_PGD" ? Org.Where(x => x.Disctrict == dvc.Disctrict).ToList() : Org.Where(x => x.FParent == dvc.FCode).ToList();
                    if (DV_C.Count == 0 || UserP.Type == "AD_PGD")
                    {
                        var ds = db.Export_DS_TrungTuyen(dvc.FCode, SearchKey, NamHoc).ToList();
                        foreach (var d in ds)
                        {
                            d.TT = i;
                            DSTT.Add(d);
                            i++;
                        }
                    }
                    else
                    {
                        foreach (var dvcc in DV_CC)
                        {
                            var ds = db.Export_DS_TrungTuyen(dvcc.FCode, SearchKey, NamHoc).ToList();
                            foreach (var d in ds)
                            {
                                d.TT = i;
                                DSTT.Add(d);
                                i++;
                            }
                        }
                    }
                }
            }
            string filepath = HttpContext.Current.Server.MapPath("~/Uploads/Template/danh_sach_hoc_sinh_trung_tuyen.xlsx");
            XSSFWorkbook hssfwb;
            using (FileStream file = new FileStream(filepath, FileMode.Open, FileAccess.Read))
            {
                hssfwb = new XSSFWorkbook(file);
                file.Close();
            }
            ISheet sheet1 = hssfwb.GetSheetAt(0);
            XSSFFont iFont = (XSSFFont)hssfwb.CreateFont();
            iFont.FontHeightInPoints = 10;
            iFont.FontName = "Times New Roman";
            XSSFCellStyle borderedCellStyleCenter = (XSSFCellStyle)hssfwb.CreateCellStyle();
            borderedCellStyleCenter.BorderLeft = BorderStyle.Thin;
            borderedCellStyleCenter.BorderTop = BorderStyle.Thin;
            borderedCellStyleCenter.BorderRight = BorderStyle.Thin;
            borderedCellStyleCenter.BorderBottom = BorderStyle.Thin;
            borderedCellStyleCenter.VerticalAlignment = VerticalAlignment.Center;
            borderedCellStyleCenter.Alignment = HorizontalAlignment.Center;
            borderedCellStyleCenter.SetFont(iFont);
            borderedCellStyleCenter.WrapText = true;
            XSSFCellStyle borderedCellStyleLeft = (XSSFCellStyle)hssfwb.CreateCellStyle();
            borderedCellStyleLeft.BorderLeft = BorderStyle.Thin;
            borderedCellStyleLeft.BorderTop = BorderStyle.Thin;
            borderedCellStyleLeft.BorderRight = BorderStyle.Thin;
            borderedCellStyleLeft.BorderBottom = BorderStyle.Thin;
            borderedCellStyleLeft.VerticalAlignment = VerticalAlignment.Center;
            borderedCellStyleLeft.Alignment = HorizontalAlignment.Left;
            borderedCellStyleLeft.SetFont(iFont);
            borderedCellStyleLeft.WrapText = true;

            NPOI.SS.UserModel.ICell r;

            var font = hssfwb.CreateFont();
            font.FontHeightInPoints = 10;
            font.FontName = "Times New Roman";
            font.Boldweight = (short)FontBoldWeight.Bold;

            var font1 = hssfwb.CreateFont();
            font1.FontHeightInPoints = 10;
            font1.FontName = "Times New Roman";
            font1.IsItalic = true;

            var font2 = hssfwb.CreateFont();
            font2.FontHeightInPoints = 10;
            font2.FontName = "Times New Roman";
            font2.Boldweight = (short)FontBoldWeight.Bold;

            XSSFCellStyle borderedCellStyleLeftBold = (XSSFCellStyle)hssfwb.CreateCellStyle();
            borderedCellStyleLeftBold.BorderLeft = BorderStyle.Thin;
            borderedCellStyleLeftBold.BorderTop = BorderStyle.Thin;
            borderedCellStyleLeftBold.BorderRight = BorderStyle.Thin;
            borderedCellStyleLeftBold.BorderBottom = BorderStyle.Thin;
            borderedCellStyleLeftBold.VerticalAlignment = VerticalAlignment.Center;
            borderedCellStyleLeftBold.Alignment = HorizontalAlignment.Left;
            borderedCellStyleLeftBold.SetFont(font);
            borderedCellStyleLeftBold.WrapText = true;

            XSSFCellStyle CellStyleCenterBold = (XSSFCellStyle)hssfwb.CreateCellStyle();
            CellStyleCenterBold.VerticalAlignment = VerticalAlignment.Center;
            CellStyleCenterBold.Alignment = HorizontalAlignment.Center;
            CellStyleCenterBold.SetFont(font2);
            CellStyleCenterBold.WrapText = true;
            
            XSSFCellStyle CellStyleCenterItalic = (XSSFCellStyle)hssfwb.CreateCellStyle();
            CellStyleCenterItalic.VerticalAlignment = VerticalAlignment.Center;
            CellStyleCenterItalic.Alignment = HorizontalAlignment.Center;
            CellStyleCenterItalic.SetFont(font1);
            CellStyleCenterItalic.WrapText = true;

            IRow Row = sheet1.GetRow(2);
            Row.Height = 325;
            r = Row.GetCell(0);
            r.SetCellValue(DV.FName.ToUpper());
            r.CellStyle = CellStyleCenterBold;

            IRow Row1 = sheet1.GetRow(3);
            Row1.Height = 325;
            r = Row1.CreateCell(0);
            r.SetCellValue("(Năm học " + NamHoc + ")");
            r.CellStyle = CellStyleCenterItalic;

            i = 6;
            foreach (var pb in DSTT)
            {
                IRow row = sheet1.CreateRow(i);
                row.Height = 315;
                int j = 0;
                r = row.CreateCell(j);
                r.SetCellValue(pb.TT.ToString());
                r.CellStyle.WrapText = true;
                r.CellStyle = borderedCellStyleCenter;
                j++;
                r = row.CreateCell(j);
                r.SetCellValue(pb.DonVi);
                r.CellStyle.WrapText = true;
                r.CellStyle = borderedCellStyleLeft;
                j++;
                r = row.CreateCell(j);
                r.SetCellValue(pb.HoVaTen);
                r.CellStyle.WrapText = true;
                r.CellStyle = borderedCellStyleLeft;
                j++;
                r = row.CreateCell(j);
                r.SetCellValue(pb.NgaySinh);
                r.CellStyle.WrapText = true;
                r.CellStyle = borderedCellStyleCenter;
                j++;
                r = row.CreateCell(j);
                r.SetCellValue(pb.Tuoi.ToString());
                r.CellStyle.WrapText = true;
                r.CellStyle = borderedCellStyleCenter;
                j++;
                r = row.CreateCell(j);
                r.SetCellValue(pb.GioiTinh == "nam" ? "Nam" : (pb.GioiTinh == "nu" ? "Nữ" : "Khác"));
                r.CellStyle.WrapText = true;
                r.CellStyle = borderedCellStyleCenter;
                j++;
                r = row.CreateCell(j);
                r.SetCellValue(((pb.NoiSinh != null && pb.NoiSinh != "") ? (pb.NoiSinh + ", ") : "") + ((pb.XaNoiSinh != null && pb.XaNoiSinh != "") ? (pb.XaNoiSinh + ", ") : "") + ((pb.HuyenNoiSinh != null && pb.HuyenNoiSinh != "") ? (pb.HuyenNoiSinh + ", ") : "") + ((pb.TinhNoiSinh != null && pb.TinhNoiSinh != "") ? pb.TinhNoiSinh : ""));
                r.CellStyle.WrapText = true;
                r.CellStyle = borderedCellStyleLeft;
                j++;
                r = row.CreateCell(j);
                r.SetCellValue(pb.SoDienThoai);
                r.CellStyle.WrapText = true;
                r.CellStyle = borderedCellStyleCenter;
                j++;
                r = row.CreateCell(j);
                r.SetCellValue(pb.HoTen_Cha);
                r.CellStyle.WrapText = true;
                r.CellStyle = borderedCellStyleLeft;
                j++;
                r = row.CreateCell(j);
                r.SetCellValue(pb.DienThoai_Cha);
                r.CellStyle.WrapText = true;
                r.CellStyle = borderedCellStyleCenter;
                j++;
                r = row.CreateCell(j);
                r.SetCellValue(pb.HoTen_Me);
                r.CellStyle.WrapText = true;
                r.CellStyle = borderedCellStyleLeft;
                j++;
                r = row.CreateCell(j);
                r.SetCellValue(pb.DienThoai_Me);
                r.CellStyle.WrapText = true;
                r.CellStyle = borderedCellStyleCenter;
                i++;
            }

            using (var memoryStream = new MemoryStream())
            {
                //ExcelToPdf x = new ExcelToPdf(); 
                hssfwb.Write(memoryStream);

                byte[] xlsBytes = memoryStream.ToArray();
                //byte[] pdfBytes = x.ConvertBytes(xlsBytes);

                //MemoryStream ms = new MemoryStream();
                //ms.Write(xlsBytes, 0, xlsBytes.Length);
                //Workbook workbook = new Workbook(memoryStream);
                //workbook.Save(ms, SaveFormat.Xlsx);
                //byte[] pdfBytes = ms.ToArray();

                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new ByteArrayContent(xlsBytes) // pdfBytes     xlsBytes
                };
                response.Content.Headers.ContentType = new MediaTypeHeaderValue
                       ("application/vnd.ms-excel");  //  pdf  vnd.ms-excel
                response.Content.Headers.ContentDisposition =
                       new ContentDispositionHeaderValue("attachment")
                       {
                           FileName = "BaoCaoDieuDongBoNhiem.xlsx"  // pdf  xlsx
                       };

                return response;
            }
        }

        [HttpPost]
        [Route("api/onegate/ExportDSTKQ")]
        [Obsolete]
        public HttpResponseMessage ExportDSTKQ(string TT, List<string> DS)
        {
            var mahs = DS[0];
            var namhoc = db.HO_SO_TUYENSINH.Where(x => x.MaHoSo == mahs).Select(y => y.NamHoc).FirstOrDefault();
            var DonVi = db.Organizations.Where(x => x.FCode == (db.UserProfiles.Where(y => y.UserName == User.Identity.Name).Select(z => z.DonVi).FirstOrDefault())).Select(t => t.FName).FirstOrDefault();
            var DSHS = db.HO_SO_TUYENSINH.OrderBy(x => x.NamHoc == namhoc).ToList();
            var DiaBan = db.Areas.ToList();
            var DSTT = new List<HO_SO_TUYENSINH>();
            foreach(var hs in DS)
            {
                DSTT.Add(DSHS.Where(x => x.MaHoSo == hs).FirstOrDefault());
            }
            string filepath = TT == "TT" ? HttpContext.Current.Server.MapPath("~/Uploads/Template/ds_tra_ket_qua_trung_tuyen.xlsx") : HttpContext.Current.Server.MapPath("~/Uploads/Template/ds_tra_ket_qua_khong_trung_tuyen.xlsx");
            XSSFWorkbook hssfwb;
            using (FileStream file = new FileStream(filepath, FileMode.Open, FileAccess.Read))
            {
                hssfwb = new XSSFWorkbook(file);
                file.Close();
            }
            ISheet sheet1 = hssfwb.GetSheetAt(0);
            XSSFFont iFont = (XSSFFont)hssfwb.CreateFont();
            iFont.FontHeightInPoints = 10;
            iFont.FontName = "Times New Roman";
            XSSFCellStyle borderedCellStyleCenter = (XSSFCellStyle)hssfwb.CreateCellStyle();
            borderedCellStyleCenter.BorderLeft = BorderStyle.Thin;
            borderedCellStyleCenter.BorderTop = BorderStyle.Thin;
            borderedCellStyleCenter.BorderRight = BorderStyle.Thin;
            borderedCellStyleCenter.BorderBottom = BorderStyle.Thin;
            borderedCellStyleCenter.VerticalAlignment = VerticalAlignment.Center;
            borderedCellStyleCenter.Alignment = HorizontalAlignment.Center;
            borderedCellStyleCenter.SetFont(iFont);
            borderedCellStyleCenter.WrapText = true;
            XSSFCellStyle borderedCellStyleLeft = (XSSFCellStyle)hssfwb.CreateCellStyle();
            borderedCellStyleLeft.BorderLeft = BorderStyle.Thin;
            borderedCellStyleLeft.BorderTop = BorderStyle.Thin;
            borderedCellStyleLeft.BorderRight = BorderStyle.Thin;
            borderedCellStyleLeft.BorderBottom = BorderStyle.Thin;
            borderedCellStyleLeft.VerticalAlignment = VerticalAlignment.Center;
            borderedCellStyleLeft.Alignment = HorizontalAlignment.Left;
            borderedCellStyleLeft.SetFont(iFont);
            borderedCellStyleLeft.WrapText = true;

            NPOI.SS.UserModel.ICell r;

            var font = hssfwb.CreateFont();
            font.FontHeightInPoints = 10;
            font.FontName = "Times New Roman";
            font.Boldweight = (short)FontBoldWeight.Bold;

            var font1 = hssfwb.CreateFont();
            font1.FontHeightInPoints = 10;
            font1.FontName = "Times New Roman";
            font1.IsItalic = true;

            var font2 = hssfwb.CreateFont();
            font2.FontHeightInPoints = 10;
            font2.FontName = "Times New Roman";
            font2.Boldweight = (short)FontBoldWeight.Bold;

            XSSFCellStyle borderedCellStyleLeftBold = (XSSFCellStyle)hssfwb.CreateCellStyle();
            borderedCellStyleLeftBold.BorderLeft = BorderStyle.Thin;
            borderedCellStyleLeftBold.BorderTop = BorderStyle.Thin;
            borderedCellStyleLeftBold.BorderRight = BorderStyle.Thin;
            borderedCellStyleLeftBold.BorderBottom = BorderStyle.Thin;
            borderedCellStyleLeftBold.VerticalAlignment = VerticalAlignment.Center;
            borderedCellStyleLeftBold.Alignment = HorizontalAlignment.Left;
            borderedCellStyleLeftBold.SetFont(font);
            borderedCellStyleLeftBold.WrapText = true;

            XSSFCellStyle CellStyleCenterBold = (XSSFCellStyle)hssfwb.CreateCellStyle();
            CellStyleCenterBold.VerticalAlignment = VerticalAlignment.Center;
            CellStyleCenterBold.Alignment = HorizontalAlignment.Center;
            CellStyleCenterBold.SetFont(font2);
            CellStyleCenterBold.WrapText = true;

            XSSFCellStyle CellStyleCenterItalic = (XSSFCellStyle)hssfwb.CreateCellStyle();
            CellStyleCenterItalic.VerticalAlignment = VerticalAlignment.Center;
            CellStyleCenterItalic.Alignment = HorizontalAlignment.Center;
            CellStyleCenterItalic.SetFont(font1);
            CellStyleCenterItalic.WrapText = true;

            IRow Row = sheet1.GetRow(2);
            Row.Height = 325;
            r = Row.GetCell(0);
            r.SetCellValue(DonVi.ToUpper());
            r.CellStyle = CellStyleCenterBold;

            IRow Row1 = sheet1.GetRow(3);
            Row1.Height = 325;
            r = Row1.CreateCell(0);
            r.SetCellValue("(Năm học " + namhoc + ")");
            r.CellStyle = CellStyleCenterItalic;

            int i = 6;
            foreach (var pb in DSTT)
            {
                IRow row = sheet1.CreateRow(i);
                row.Height = 315;
                int j = 0;
                r = row.CreateCell(j);
                r.SetCellValue((i-5).ToString());
                r.CellStyle.WrapText = true;
                r.CellStyle = borderedCellStyleCenter;
                j++;
                r = row.CreateCell(j);
                r.SetCellValue(pb.MaHoSo);
                r.CellStyle.WrapText = true;
                r.CellStyle = borderedCellStyleLeft;
                j++;
                r = row.CreateCell(j);
                r.SetCellValue(pb.HoVaTen);
                r.CellStyle.WrapText = true;
                r.CellStyle = borderedCellStyleLeft;
                j++;
                r = row.CreateCell(j);
                r.SetCellValue((pb.NgaySinh ?? DateTime.Now).ToString("dd/MM/yyyy"));
                r.CellStyle.WrapText = true;
                r.CellStyle = borderedCellStyleCenter;
                j++;
                r = row.CreateCell(j);
                r.SetCellValue(pb.GioiTinh == "nam" ? "Nam" : (pb.GioiTinh == "nu" ? "Nữ" : "Khác"));
                r.CellStyle.WrapText = true;
                r.CellStyle = borderedCellStyleCenter;
                j++;
                r = row.CreateCell(j);
                r.SetCellValue(((pb.NoiSinh != null && pb.NoiSinh != "") ? (DiaBan.Where(x => x.FCode == pb.NoiSinh).Select(y => y.FName).FirstOrDefault() + ", ") : "") + ((pb.XaNoiSinh != null && pb.XaNoiSinh != "") ? (DiaBan.Where(x => x.FCode == pb.XaNoiSinh).Select(y => y.FName).FirstOrDefault() + ", ") : "") + ((pb.HuyenNoiSinh != null && pb.HuyenNoiSinh != "") ? (DiaBan.Where(x => x.FCode == pb.HuyenNoiSinh).Select(y => y.FName).FirstOrDefault() + ", ") : "") + ((pb.TinhNoiSinh != null && pb.TinhNoiSinh != "") ? DiaBan.Where(x => x.FCode == pb.TinhNoiSinh).Select(y => y.FName).FirstOrDefault() : ""));
                r.CellStyle.WrapText = true;
                r.CellStyle = borderedCellStyleLeft;
                j++;
                r = row.CreateCell(j);
                r.SetCellValue(pb.SoDienThoai);
                r.CellStyle.WrapText = true;
                r.CellStyle = borderedCellStyleCenter;
                j++;
                r = row.CreateCell(j);
                r.SetCellValue(pb.HoTen_Cha);
                r.CellStyle.WrapText = true;
                r.CellStyle = borderedCellStyleLeft;
                j++;
                r = row.CreateCell(j);
                r.SetCellValue(pb.DienThoai_Cha);
                r.CellStyle.WrapText = true;
                r.CellStyle = borderedCellStyleCenter;
                j++;
                r = row.CreateCell(j);
                r.SetCellValue(pb.HoTen_Me);
                r.CellStyle.WrapText = true;
                r.CellStyle = borderedCellStyleLeft;
                j++;
                r = row.CreateCell(j);
                r.SetCellValue(pb.DienThoai_Me);
                r.CellStyle.WrapText = true;
                r.CellStyle = borderedCellStyleCenter;
                i++;
            }

            using (var memoryStream = new MemoryStream())
            {
                //ExcelToPdf x = new ExcelToPdf(); 
                hssfwb.Write(memoryStream);

                byte[] xlsBytes = memoryStream.ToArray();
                //byte[] pdfBytes = x.ConvertBytes(xlsBytes);

                //MemoryStream ms = new MemoryStream();
                //ms.Write(xlsBytes, 0, xlsBytes.Length);
                //Workbook workbook = new Workbook(memoryStream);
                //workbook.Save(ms, SaveFormat.Xlsx);
                //byte[] pdfBytes = ms.ToArray();

                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new ByteArrayContent(xlsBytes) // pdfBytes     xlsBytes
                };
                response.Content.Headers.ContentType = new MediaTypeHeaderValue
                       ("application/vnd.ms-excel");  //  pdf  vnd.ms-excel
                response.Content.Headers.ContentDisposition =
                       new ContentDispositionHeaderValue("attachment")
                       {
                           FileName = "BaoCaoDieuDongBoNhiem.xlsx"  // pdf  xlsx
                       };

                return response;
            }
        }

        #endregion

        #region Nop ho so he thong

        [HttpGet]
        [Route("api/onegate/GetNamHoc")]
        public IHttpActionResult GetNamHoc()
        {
            string NamHoc = DateTime.Now.Year + " - " + (DateTime.Now.Year + 1);
            var response = new
            {
                response_code = "0",
                response_desc = "Thành công",
                response_data = NamHoc
            };
            return Ok(response);
        }

        [HttpPost]
        [Route("api/onegate/nhan_ho_so")]
        public IHttpActionResult nhan_ho_so(HOSO hoso)
        {
            using (var dbContextTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    string FCodeNV2 = "";
                    string MaHoSo = hoso.HoSoTuyenSinh.MaHoSo;
                    string MaDonVi = hoso.HoSoTuyenSinh.MaDonVi;
                    if (!String.IsNullOrEmpty(hoso.HoSoTuyenSinh.SoCCCDHocSinh))
                    {
                        var CCCD = db.HO_SO_TUYENSINH.Where(x => x.SoCCCDHocSinh == hoso.HoSoTuyenSinh.SoCCCDHocSinh && x.NamHoc == hoso.HoSoTuyenSinh.NamHoc).FirstOrDefault();
                        if(CCCD != null)
                        {
                            var res = new
                            {
                                response_code = "03",
                                response_desc = "Already",
                                response_data = "Đã tồn tại Hồ sơ cho số CCCD/Mã định danh điện tử này"
                            };
                            return Ok(res);
                        }
                    }
                    if (String.IsNullOrEmpty(hoso.HoSoTuyenSinh.MaHoSo)){
                        MaHoSo = getAutoId(MaDonVi);
                        if (MaHoSo == null)
                        {
                            var res = new
                            {
                                response_code = "02",
                                response_desc = "NotFound",
                                response_data = "Đơn vị chưa có mã định danh"
                            };
                            return Ok(res);
                        }
                        hoso.HoSoOnline.FCode = MaHoSo;
                    }
                    foreach (var tt in hoso.HoSoOnlineThuTuc)
                    {
                        tt.MaHoSo = MaHoSo;
                        tt.MaDonVi = MaDonVi;
                        db.HO_SO_ONLINE_THUTUC.AddOrUpdate(tt);
                    }
                    hoso.HoSoTuyenSinh.MaHoSo = MaHoSo;
                    hoso.HoSoOnline.MaDonVi = MaDonVi;
                    string Truong = db.Organizations.Where(x => x.FCode == MaDonVi).Select(y => y.FName).FirstOrDefault();
                    hoso.KetQuaHocTap.MaHoSo = MaHoSo;
                    hoso.HoSoOnline.TenDuAn = "đăng ký tuyển sinh vào trường " + Truong + " " + hoso.HoSoTuyenSinh.NamHoc;
                    //Lưu thông tin hồ sơ
                    db.HO_SO_ONLINE.AddOrUpdate(hoso.HoSoOnline);
                    //db.DINH_KEM_ONLINE.AddRange(hoso.DinhKemOnline);
                    db.HO_SO_TUYENSINH.AddOrUpdate(hoso.HoSoTuyenSinh);
                    db.KET_QUA_HOC_TAP.AddOrUpdate(hoso.KetQuaHocTap);
                    List<NGUYEN_VONG> listNV = new List<NGUYEN_VONG>();
                    NGUYEN_VONG ngv = new NGUYEN_VONG();
                    if (hoso.NguyenVong.Count() == 2)
                    {

                        string LoaiTruong = db.Organizations.Where(x => x.FCode == hoso.HoSoOnline.MaDonVi).Select(y => y.Loai).FirstOrDefault();

                        if (!String.IsNullOrEmpty(hoso.NguyenVong[1].MaTruong) && LoaiTruong != "CHUYEN")
                        {
                            if (hoso.NguyenVong[1].MaTruong == hoso.NguyenVong[0].MaTruong)
                            {
                                if (hoso.HoSoTuyenSinh.NhomMonBatBuoc == hoso.HoSoTuyenSinh.NhomMonBatBuocNV2)
                                {
                                    if (JsonConvert.SerializeObject(hoso.NguyenVong[1].NhomMonHoc) == JsonConvert.SerializeObject(hoso.NguyenVong[0].NhomMonHoc))
                                    {
                                        var res = new
                                        {
                                            response_code = "04",
                                            response_desc = "Duplicate",
                                            response_data = "2 nguyện vọng không được phép giống nhau!"
                                        };
                                        return Ok(res);
                                    }
                                }
                                List<NGUYEN_VONG> listNV2 = new List<NGUYEN_VONG>();
                                NGUYEN_VONG ngv2 = new NGUYEN_VONG();
                                ngv2.MaHoSo = hoso.HoSoOnline.FCode;
                                ngv2.MonHoc = JsonConvert.SerializeObject(hoso.NguyenVong[1]);
                                ngv2.MaHoSoNV1 = hoso.HoSoOnline.FCode;
                                db.NGUYEN_VONG.Add(ngv2);
                            }
                            else
                            {
                                HOSO hoso2 = hoso.Clone();
                                string MaDonViNV2 = hoso.NguyenVong[1].MaTruong;
                                string TenTruong = db.Organizations.Where(x => x.FCode == MaDonViNV2).Select(y => y.FName).FirstOrDefault();
                                FCodeNV2 = getAutoId(MaDonViNV2);
                                hoso2.HoSoOnline.FCode = FCodeNV2;
                                hoso2.HoSoOnline.TenDuAn = "đăng ký tuyển sinh vào trường " + TenTruong + " " + hoso2.HoSoTuyenSinh.NamHoc;
                                hoso2.HoSoOnline.NguyenVong2 = true;
                                hoso2.HoSoOnline.MaHoSoNguyenVong1 = hoso.HoSoOnline.FCode;
                                hoso2.HoSoOnline.MaDonVi = MaDonViNV2;
                                foreach (var hstt in hoso2.HoSoOnlineThuTuc)
                                {
                                    hstt.MaHoSo = FCodeNV2;
                                    hstt.MaDonVi = MaDonViNV2;
                                }
                                hoso2.HoSoTuyenSinh.MaHoSo = FCodeNV2;
                                hoso2.HoSoTuyenSinh.MaDonVi = MaDonViNV2;
                                hoso2.KetQuaHocTap.MaHoSo = FCodeNV2;

                                List<NGUYEN_VONG> listNV2 = new List<NGUYEN_VONG>();
                                NGUYEN_VONG ngv2 = new NGUYEN_VONG();
                                ngv2.MaHoSo = hoso2.HoSoOnline.FCode;
                                ngv2.MonHoc = JsonConvert.SerializeObject(hoso.NguyenVong[1]);
                                ngv2.MaHoSoNV1 = hoso.HoSoOnline.FCode;

                                db.HO_SO_ONLINE.Add(hoso2.HoSoOnline);
                                db.HO_SO_ONLINE_THUTUC.AddRange(hoso2.HoSoOnlineThuTuc);
                                //db.DINH_KEM_ONLINE.AddRange(hoso.DinhKemOnline);
                                db.HO_SO_TUYENSINH.Add(hoso2.HoSoTuyenSinh);
                                db.KET_QUA_HOC_TAP.Add(hoso2.KetQuaHocTap);
                                db.NGUYEN_VONG.Add(ngv2);
                            }
                        }
                        if (LoaiTruong == "CHUYEN")
                        {
                            List<NGUYEN_VONG> listNV2 = new List<NGUYEN_VONG>();
                            NGUYEN_VONG ngv2 = new NGUYEN_VONG();
                            ngv2.MaHoSo = hoso.HoSoOnline.FCode;
                            ngv2.MonHoc = JsonConvert.SerializeObject(hoso.NguyenVong[1]);

                            db.NGUYEN_VONG.Add(ngv2);
                        }
                        else
                        {
                            ngv.MaHoSo = hoso.HoSoOnline.FCode;
                            ngv.MonHoc = JsonConvert.SerializeObject(hoso.NguyenVong[0]);
                            db.NGUYEN_VONG.Add(ngv);
                        }
                    }
                    else if (hoso.NguyenVong.Count() == 1)
                    {
                        ngv.MaHoSo = hoso.HoSoOnline.FCode;
                        ngv.MonHoc = JsonConvert.SerializeObject(hoso.NguyenVong[0]);
                        db.NGUYEN_VONG.Add(ngv);
                    }
                    db.SaveChanges();

                    dbContextTransaction.Commit();
                    DongBoNguoiDung(hoso.HoSoOnline.FCode, hoso.HoSoOnline.SoDienThoai);
                    if(!String.IsNullOrEmpty(FCodeNV2)) DongBoNguoiDung(FCodeNV2, hoso.HoSoOnline.SoDienThoai);

                    var response = new
                    {
                        response_code = "0",
                        response_desc = "Thành công",
                    };
                    return Ok(response);

                }
                catch (System.Exception ex)
                {
                    dbContextTransaction.Rollback();
                    var response = new
                    {
                        response_code = "-1",
                        response_desc = "Lỗi",
                        response_data = ex.ToString()
                    };
                    return Ok(response);
                }
            }
        }

        [HttpPost]
        [Route("api/onegate/sua_ho_so")]
        public IHttpActionResult sua_ho_so(HOSO hoso)
        {
            using (var dbContextTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    string MaHoSo = hoso.HoSoTuyenSinh.MaHoSo;
                    string MaDonVi = hoso.HoSoTuyenSinh.MaDonVi;
                    if (!String.IsNullOrEmpty(hoso.HoSoTuyenSinh.SoCCCDHocSinh))
                    {
                        var CCCD = db.HO_SO_TUYENSINH.Where(x => x.SoCCCDHocSinh == hoso.HoSoTuyenSinh.SoCCCDHocSinh && x.NamHoc == hoso.HoSoTuyenSinh.NamHoc).FirstOrDefault();
                        if (CCCD != null)
                        {
                            var res = new
                            {
                                response_code = "03",
                                response_desc = "Already",
                                response_data = "Đã tồn tại Hồ sơ cho số CCCD/Mã định danh điện tử này"
                            };
                            return Ok(res);
                        }
                    }
                    foreach (var tt in hoso.HoSoOnlineThuTuc)
                    {
                        tt.MaHoSo = MaHoSo;
                        tt.MaDonVi = MaDonVi;
                        db.HO_SO_ONLINE_THUTUC.AddOrUpdate(tt);
                    }
                    hoso.HoSoTuyenSinh.MaHoSo = MaHoSo;
                    hoso.HoSoOnline.MaDonVi = MaDonVi;
                    string Truong = db.Organizations.Where(x => x.FCode == MaDonVi).Select(y => y.FName).FirstOrDefault();
                    hoso.KetQuaHocTap.MaHoSo = MaHoSo;
                    hoso.HoSoOnline.TenDuAn = "đăng ký tuyển sinh vào trường " + Truong + " " + hoso.HoSoTuyenSinh.NamHoc;
                    //Lưu thông tin hồ sơ
                    db.HO_SO_ONLINE.AddOrUpdate(hoso.HoSoOnline);
                    //db.DINH_KEM_ONLINE.AddRange(hoso.DinhKemOnline);
                    db.HO_SO_TUYENSINH.AddOrUpdate(hoso.HoSoTuyenSinh);
                    db.KET_QUA_HOC_TAP.AddOrUpdate(hoso.KetQuaHocTap);
                    List<NGUYEN_VONG> listNV = new List<NGUYEN_VONG>();
                    NGUYEN_VONG ngv = new NGUYEN_VONG();
                    var NVO = db.NGUYEN_VONG.Where(x => x.MaHoSo == hoso.HoSoOnline.FCode || x.MaHoSoNV1 == hoso.HoSoOnline.FCode).ToList();
                    foreach (var NV in NVO)
                    {
                        db.NGUYEN_VONG.Remove(NV);
                    }
                    if (hoso.NguyenVong.Count() == 2)
                    {

                        string LoaiTruong = db.Organizations.Where(x => x.FCode == hoso.HoSoOnline.MaDonVi).Select(y => y.Loai).FirstOrDefault();

                        if (!String.IsNullOrEmpty(hoso.NguyenVong[1].MaTruong) && LoaiTruong != "CHUYEN")
                        {
                            if(hoso.NguyenVong[1].MaTruong == hoso.NguyenVong[0].MaTruong)
                            {
                                if (hoso.HoSoTuyenSinh.NhomMonBatBuoc == hoso.HoSoTuyenSinh.NhomMonBatBuocNV2)
                                {
                                    if (JsonConvert.SerializeObject(hoso.NguyenVong[1].NhomMonHoc) == JsonConvert.SerializeObject(hoso.NguyenVong[0].NhomMonHoc))
                                    {
                                        var res = new
                                        {
                                            response_code = "04",
                                            response_desc = "Duplicate",
                                            response_data = "2 nguyện vọng không được phép giống nhau!"
                                        };
                                        return Ok(res);
                                    }
                                }
                                List<NGUYEN_VONG> listNV2 = new List<NGUYEN_VONG>();
                                NGUYEN_VONG ngv2 = new NGUYEN_VONG();
                                ngv2.MaHoSo = hoso.HoSoOnline.FCode;
                                ngv2.MonHoc = JsonConvert.SerializeObject(hoso.NguyenVong[1]);
                                ngv2.MaHoSoNV1 = hoso.HoSoOnline.FCode;
                                db.NGUYEN_VONG.Add(ngv2);
                            }
                            else
                            {
                                HOSO hoso2 = hoso.Clone();
                                string MaDonViNV2 = hoso.NguyenVong[1].MaTruong;
                                string TenTruong = db.Organizations.Where(x => x.FCode == MaDonViNV2).Select(y => y.FName).FirstOrDefault();
                                var HoSoNV2 = db.HO_SO_ONLINE.Where(x => x.MaHoSoNguyenVong1 == hoso.HoSoOnline.FCode).FirstOrDefault();
                                var HoSoTSNV2 = db.HO_SO_TUYENSINH.Where(x => x.MaHoSo == HoSoNV2.FCode).FirstOrDefault();
                                var KQNV2 = db.KET_QUA_HOC_TAP.Where(x => x.MaHoSo == HoSoNV2.FCode).FirstOrDefault();
                                hoso2.HoSoOnline.Id = HoSoNV2.Id;
                                hoso2.HoSoOnline.FCode = HoSoNV2.FCode;
                                hoso2.HoSoOnline.TenDuAn = "đăng ký tuyển sinh vào trường " + TenTruong + " " + hoso2.HoSoTuyenSinh.NamHoc;
                                hoso2.HoSoOnline.NguyenVong2 = true;
                                hoso2.HoSoOnline.MaHoSoNguyenVong1 = hoso.HoSoOnline.FCode;
                                hoso2.HoSoOnline.MaDonVi = MaDonViNV2;
                                var HSTTO = db.HO_SO_ONLINE_THUTUC.Where(x => x.MaHoSo == HoSoNV2.FCode).ToList();
                                foreach (var NV in HSTTO)
                                {
                                    db.HO_SO_ONLINE_THUTUC.Remove(NV);
                                }
                                foreach (var hstt in hoso2.HoSoOnlineThuTuc)
                                {
                                    hstt.MaHoSo = HoSoNV2.FCode;
                                    hstt.MaDonVi = MaDonViNV2;
                                }
                                hoso2.HoSoTuyenSinh.Id = HoSoTSNV2.Id;
                                hoso2.HoSoTuyenSinh.MaHoSo = HoSoNV2.FCode;
                                hoso2.HoSoTuyenSinh.MaDonVi = MaDonViNV2;
                                hoso2.KetQuaHocTap.Id = KQNV2.Id;
                                hoso2.KetQuaHocTap.MaHoSo = HoSoNV2.FCode;

                                List<NGUYEN_VONG> listNV2 = new List<NGUYEN_VONG>();
                                NGUYEN_VONG ngv2 = new NGUYEN_VONG();
                                ngv2.MaHoSo = hoso2.HoSoOnline.FCode;
                                ngv2.MonHoc = JsonConvert.SerializeObject(hoso.NguyenVong[1]);
                                ngv2.MaHoSoNV1 = hoso.HoSoOnline.FCode;

                                db.HO_SO_ONLINE.AddOrUpdate(hoso2.HoSoOnline);
                                db.HO_SO_ONLINE_THUTUC.AddRange(hoso2.HoSoOnlineThuTuc);
                                //db.DINH_KEM_ONLINE.AddRange(hoso.DinhKemOnline);
                                db.HO_SO_TUYENSINH.AddOrUpdate(hoso2.HoSoTuyenSinh);
                                db.KET_QUA_HOC_TAP.AddOrUpdate(hoso2.KetQuaHocTap);
                                db.NGUYEN_VONG.Add(ngv2);
                            }
                        }
                        if (LoaiTruong == "CHUYEN")
                        {
                            List<NGUYEN_VONG> listNV2 = new List<NGUYEN_VONG>();
                            NGUYEN_VONG ngv2 = new NGUYEN_VONG();
                            ngv2.MaHoSo = hoso.HoSoOnline.FCode;
                            ngv2.MonHoc = JsonConvert.SerializeObject(hoso.NguyenVong[1]);

                            db.NGUYEN_VONG.Add(ngv2);
                        }
                        else
                        {
                            ngv.MaHoSo = hoso.HoSoOnline.FCode;
                            ngv.MonHoc = JsonConvert.SerializeObject(hoso.NguyenVong[0]);
                            db.NGUYEN_VONG.Add(ngv);
                        }
                    }
                    else if (hoso.NguyenVong.Count() == 1)
                    {
                        ngv.MaHoSo = hoso.HoSoOnline.FCode;
                        ngv.MonHoc = JsonConvert.SerializeObject(hoso.NguyenVong[0]);
                        db.NGUYEN_VONG.Add(ngv);
                    }
                    db.SaveChanges();

                    dbContextTransaction.Commit();

                    var response = new
                    {
                        response_code = "0",
                        response_desc = "Thành công",
                    };
                    return Ok(response);

                }
                catch (System.Exception ex)
                {
                    dbContextTransaction.Rollback();
                    var response = new
                    {
                        response_code = "-1",
                        response_desc = "Lỗi",
                        response_data = ex.ToString()
                    };
                    return Ok(response);
                }
            }
        }

        public string getAutoId(string MaDonVi)
        {
            var DonVi = db.Organizations.Where(x => x.FCode == MaDonVi).Select(y => y.FBranchCode).FirstOrDefault();
            if (String.IsNullOrEmpty(DonVi))
            {
                return null;
            }
            else
            {
                AutoID AutoId = db.AutoIDs.Where(x => x.FCode == DonVi).SingleOrDefault();
                if (AutoId == null)
                {
                    AutoId = new AutoID();
                    AutoId.FCode = DonVi;
                    AutoId.Counter = 1;
                    AutoId.FCreateTime = DateTime.Now;
                    db.AutoIDs.Add(AutoId);
                }
                var Time = AutoId.FCreateTime ?? DateTime.Now;
                if (Time.Date != DateTime.Now.Date)
                {
                    AutoId.Counter = 1;
                }
                AutoId.FCreateTime = DateTime.Now;
                AutoId.FName = DonVi + "-" + DateTime.Now.ToString("yyMMdd") + "-";
                for (int i = 0; i < 4 - AutoId.Counter.ToString().Length; i++)
                    AutoId.FName += 0;
                AutoId.FName += AutoId.Counter.ToString();
                AutoId.Counter += 1;
                db.SaveChanges();
                //var response = new
                //{
                //    response_code = "00",
                //    response_desc = "success",
                //    response_data = AutoId.FName
                //};
                return AutoId.FName;
            }
        }
        [HttpGet]
        [Route("api/onegate/CreateNguyenVong")]
        public IHttpActionResult CreateNguyenVong(string MaDonVi, string NhomMon)
        {
            try
            {
                NguyenVong nguyenVong = new NguyenVong();
                nguyenVong.MaTruong = MaDonVi;
                List<NhomMonHoc> dsNhomMonHoc = new List<NhomMonHoc>();
                var item = db.NHOM_MON_HOC.Where(x => x.MaDonVi == MaDonVi || x.MaDonVi == null || x.MaDonVi == "").ToList();
                var itemMonHoc = db.MON_HOC.ToList();
                foreach (var n in item)
                {
                    List<string> ListMonHoc = JsonConvert.DeserializeObject<List<string>>(n.MonHoc);
                    List<MonHoc> dsMonHoc = new List<MonHoc>();
                    foreach (var mh in ListMonHoc)
                    {
                        dsMonHoc.Add(new MonHoc()
                        {
                            FCode = mh,
                            FName = itemMonHoc.Where(x => x.FCode == mh).Select(y => y.FName).FirstOrDefault(),
                            Check = false,
                            NhomMonHoc = n.FCode
                        });
                    };
                    if (n.FCode != NhomMon)
                    {
                        dsNhomMonHoc.Add(new NhomMonHoc()
                        {
                            FCode = n.FCode,
                            FName = n.FName,
                            MonHoc = dsMonHoc
                        });
                    }
                }
                nguyenVong.NhomMonHoc = dsNhomMonHoc;
                return Ok(nguyenVong);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ToString());
            }
        }

        [HttpGet]
        [Route("api/onegate/GetDonVi")]
        public IHttpActionResult GetDonVi()
        {
            try
            {
                string Username = User.Identity.Name;
                string MaDonVi = db.UserProfiles.Where(x => x.UserName == Username).Select(x => x.DonVi).FirstOrDefault();
                var DonVi = db.Organizations.Where(x => x.FCode == MaDonVi).FirstOrDefault();
                return Ok(DonVi);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ToString());
            }
        }

        [HttpGet]
        [Route("api/onegate/GetThoiGianTS")]
        public IHttpActionResult GetThoiGianTS(string Cap)
        {
            try
            {
                bool ChoPhep = false;
                var thoigian = db.THOI_GIAN_TS.Where(x => x.FCode == Cap).FirstOrDefault();
                if (thoigian.BatDau <= DateTime.Now)
                {
                    if (thoigian.KetThuc == null) ChoPhep = true;
                    else if (thoigian.KetThuc != null)
                    {
                        if (thoigian.KetThuc >= DateTime.Now) ChoPhep = true;
                    }
                }
                return Ok(ChoPhep);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ToString());
            }
        }

        #endregion

        #region In phieu dang ky xet tuyen

        public class HS_MN
        {
            public string MaDonVi { get; set; }
            public string LopMN { get; set; }
            public string HoVaTen { get; set; }
            public DateTime NgaySinh { get; set; }
            public string GioiTinh { get; set; }
            public string DanToc { get; set; }
            public string TinhNoiSinh { get; set; }
            public string HuyenNoiSinh { get; set; }
            public string MaDinhDanh { get; set; }
            public string TinhCuTru { get; set; }
            public string HuyenCuTru { get; set; }
            public string XaCuTru { get; set; }
            public string NoiCuTru { get; set; }
            public string NguoiLienHe { get; set; }
            public string SoGiayToNLH { get; set; }
            public string DiaChiNLH { get; set; }
            public string DienThoaiNLH { get; set; }
        }

        [HttpPost]
        [Route("api/onegate/in_ho_so_mn")]
        public HttpResponseMessage in_ho_so_mn(HS_MN HoSo)
        {
            string DonVi = db.Organizations.Where(x => x.FCode == HoSo.MaDonVi).Select(y => y.FName).FirstOrDefault().Replace("Trường ", "").Replace("Trường", "");
            string NgaySinh = HoSo.NgaySinh.ToString("dd/MM/yyyy");
            string GioiTinh = HoSo.GioiTinh == "nam" ? "Nam" : "Nữ";
            string DanToc = db.Nations.Where(x => x.FCode == HoSo.DanToc).Select(y => y.FName).FirstOrDefault();
            string NoiSinh = db.Areas.Where(x => x.FCode == HoSo.HuyenNoiSinh).Select(y => y.FName).FirstOrDefault() + ", " + db.Areas.Where(x => x.FCode == HoSo.TinhNoiSinh).Select(y => y.FName).FirstOrDefault();
            string NoiCuTru = HoSo.NoiCuTru + ", " + db.Areas.Where(x => x.FCode == HoSo.XaCuTru).Select(y => y.FName).FirstOrDefault() + ", " + db.Areas.Where(x => x.FCode == HoSo.HuyenCuTru).Select(y => y.FName).FirstOrDefault() + ", " + db.Areas.Where(x => x.FCode == HoSo.TinhCuTru).Select(y => y.FName).FirstOrDefault();
            string Ngay = DateTime.Now.Day < 10 ? "0" + DateTime.Now.Day.ToString() : DateTime.Now.Day.ToString();
            string Thang = DateTime.Now.Month < 10 ? "0" + DateTime.Now.Month.ToString() : DateTime.Now.Month.ToString();
            string dataDir = HttpContext.Current.Server.MapPath("~/Templates/DON_TUYEN_SINH_MN_2022.docx");
            // Open an existing document.
            Aspose.Words.Document doc = new Aspose.Words.Document(dataDir);
            using (FileStream file = new FileStream(dataDir, FileMode.Open, FileAccess.Read))
            {
                doc = new Aspose.Words.Document(file);
                file.Close();
            }
            // Trim trailing and leading whitespaces mail merge values
            doc.MailMerge.TrimWhitespaces = false;

            // Fill the fields in the document with user data.
            doc.MailMerge.Execute(
                new string[] { "TenDonVi", "HoVaTen", "NgaySinh", "GioiTinh", "DanToc", "NoiSinh", "MaDinhDanh", "NoiCuTru", "Lop", "NguoiLienHe", "DiaChiNLH", "SoGiayToNLH", "DienThoaiNLH", "Ngay", "Thang", "Nam" },
                new object[] { DonVi, HoSo.HoVaTen, NgaySinh, GioiTinh, DanToc, NoiSinh, HoSo.MaDinhDanh, NoiCuTru, HoSo.LopMN, HoSo.NguoiLienHe, HoSo.DiaChiNLH, HoSo.SoGiayToNLH, HoSo.DienThoaiNLH, Ngay, Thang, DateTime.Now.Year.ToString() });
            using (var memoryStream = new MemoryStream())
            {
                doc.Save(memoryStream, SaveFormat.Docx);
                memoryStream.Position = 0;
                XWPFDocument doc2 = new XWPFDocument(memoryStream);
                doc2.RemoveBodyElement(0);
                using (var memoryStream2 = new MemoryStream())
                {
                    doc2.Write(memoryStream2);
                    byte[] docBytes = memoryStream2.ToArray();
                    var response = new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new ByteArrayContent(docBytes) // pdfBytes     xlsBytes
                    };
                    response.Content.Headers.ContentType = new MediaTypeHeaderValue
                           ("application/vnd.ms-word");  //  pdf  vnd.ms-excel
                    response.Content.Headers.ContentDisposition =
                           new ContentDispositionHeaderValue("attachment")
                           {
                               FileName = "DON_TUYEN_SINH_MN_2022.docx"  // pdf  xlsx
                       };

                    return response;
                }
            }
        }
        public class HS_TH
        {
            public string MaDonVi { get; set; }
            public string HoVaTen { get; set; }
            public DateTime NgaySinh { get; set; }
            public string GioiTinh { get; set; }
            public string DanToc { get; set; }
            public string TinhNoiSinh { get; set; }
            public string HuyenNoiSinh { get; set; }
            public string MaDinhDanh { get; set; }
            public string TinhCuTru { get; set; }
            public string HuyenCuTru { get; set; }
            public string XaCuTru { get; set; }
            public string NoiCuTru { get; set; }
            public string NguoiLienHe { get; set; }
            public string SoGiayToNLH { get; set; }
            public string DiaChiNLH { get; set; }
            public string DienThoaiNLH { get; set; }
        }
        [HttpPost]
        [Route("api/onegate/in_ho_so_th")]
        public HttpResponseMessage in_ho_so_th(HS_TH HoSo)
        {
            string DonVi = db.Organizations.Where(x => x.FCode == HoSo.MaDonVi).Select(y => y.FName).FirstOrDefault().Replace("Trường ", "").Replace("Trường", "");
            string NgaySinh = HoSo.NgaySinh.ToString("dd/MM/yyyy");
            string GioiTinh = HoSo.GioiTinh == "nam" ? "Nam" : "Nữ";
            string DanToc = db.Nations.Where(x => x.FCode == HoSo.DanToc).Select(y => y.FName).FirstOrDefault();
            string NoiSinh = db.Areas.Where(x => x.FCode == HoSo.HuyenNoiSinh).Select(y => y.FName).FirstOrDefault() + ", " + db.Areas.Where(x => x.FCode == HoSo.TinhNoiSinh).Select(y => y.FName).FirstOrDefault();
            string NoiCuTru = HoSo.NoiCuTru + ", " + db.Areas.Where(x => x.FCode == HoSo.XaCuTru).Select(y => y.FName).FirstOrDefault() + ", " + db.Areas.Where(x => x.FCode == HoSo.HuyenCuTru).Select(y => y.FName).FirstOrDefault() + ", " + db.Areas.Where(x => x.FCode == HoSo.TinhCuTru).Select(y => y.FName).FirstOrDefault();
            string Ngay = DateTime.Now.Day < 10 ? "0" + DateTime.Now.Day.ToString() : DateTime.Now.Day.ToString();
            string Thang = DateTime.Now.Month < 10 ? "0" + DateTime.Now.Month.ToString() : DateTime.Now.Month.ToString();
            string dataDir = HttpContext.Current.Server.MapPath("~/Templates/DON_TUYEN_SINH_TH_2022.docx");
            // Open an existing document.
            Aspose.Words.Document doc = new Aspose.Words.Document(dataDir);
            using (FileStream file = new FileStream(dataDir, FileMode.Open, FileAccess.Read))
            {
                doc = new Aspose.Words.Document(file);
                file.Close();
            }
            // Trim trailing and leading whitespaces mail merge values
            doc.MailMerge.TrimWhitespaces = false;

            // Fill the fields in the document with user data.
            doc.MailMerge.Execute(
                new string[] { "TenDonVi", "HoVaTen", "NgaySinh", "GioiTinh", "DanToc", "NoiSinh", "MaDinhDanh", "NoiCuTru", "NguoiLienHe", "DiaChiNLH", "SoGiayToNLH", "DienThoaiNLH", "Ngay", "Thang", "Nam" },
                new object[] { DonVi, HoSo.HoVaTen, NgaySinh, GioiTinh, DanToc, NoiSinh, HoSo.MaDinhDanh, NoiCuTru, HoSo.NguoiLienHe, HoSo.DiaChiNLH, HoSo.SoGiayToNLH, HoSo.DienThoaiNLH, Ngay, Thang, DateTime.Now.Year.ToString() });
            using (var memoryStream = new MemoryStream())
            {
                doc.Save(memoryStream, SaveFormat.Docx);
                memoryStream.Position = 0;
                XWPFDocument doc2 = new XWPFDocument(memoryStream);
                doc2.RemoveBodyElement(0);
                using (var memoryStream2 = new MemoryStream())
                {
                    doc2.Write(memoryStream2);
                    byte[] docBytes = memoryStream2.ToArray();
                    var response = new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new ByteArrayContent(docBytes) // pdfBytes     xlsBytes
                    };
                    response.Content.Headers.ContentType = new MediaTypeHeaderValue
                           ("application/vnd.ms-word");  //  pdf  vnd.ms-excel
                    response.Content.Headers.ContentDisposition =
                           new ContentDispositionHeaderValue("attachment")
                           {
                               FileName = "DON_TUYEN_SINH_TH_2022.docx"  // pdf  xlsx
                           };

                    return response;
                }
            }
        }

        public class HS_THCS
        {
            public string MaDonVi { get; set; }
            public string HoVaTen { get; set; }
            public DateTime NgaySinh { get; set; }
            public string GioiTinh { get; set; }
            public string DanToc { get; set; }
            public string TinhNoiSinh { get; set; }
            public string HuyenNoiSinh { get; set; }
            public string MaDinhDanh { get; set; }
            public string HocSinhTruong { get; set; }
            public string DoiTuongUuTien { get; set; }
            public string TinhCuTru { get; set; }
            public string HuyenCuTru { get; set; }
            public string XaCuTru { get; set; }
            public string NoiCuTru { get; set; }
            public string NguoiLienHe { get; set; }
            public string SoGiayToNLH { get; set; }
            public string DiaChiNLH { get; set; }
            public string DienThoaiNLH { get; set; }
            public string DatGiai { get; set; }
            public string DatGiaiKhac { get; set; }
            public string L1 { get; set; }
            public string L2 { get; set; }
            public string L3 { get; set; }
            public string L4 { get; set; }
            public string L5 { get; set; }
        }

        [HttpPost]
        [Route("api/onegate/in_ho_so_thcs")]
        public HttpResponseMessage in_ho_so_thcs(HS_THCS HoSo)
        {
            string DonVi = db.Organizations.Where(x => x.FCode == HoSo.MaDonVi).Select(y => y.FName).FirstOrDefault().Replace("Trường ", "").Replace("Trường", "");
            string NgaySinh = HoSo.NgaySinh.ToString("dd/MM/yyyy");
            string GioiTinh = HoSo.GioiTinh == "nam" ? "Nam" : "Nữ";
            string DanToc = db.Nations.Where(x => x.FCode == HoSo.DanToc).Select(y => y.FName).FirstOrDefault();
            string NoiSinh = db.Areas.Where(x => x.FCode == HoSo.HuyenNoiSinh).Select(y => y.FName).FirstOrDefault() + ", " + db.Areas.Where(x => x.FCode == HoSo.TinhNoiSinh).Select(y => y.FName).FirstOrDefault();
            string NoiCuTru = HoSo.NoiCuTru + ", " + db.Areas.Where(x => x.FCode == HoSo.XaCuTru).Select(y => y.FName).FirstOrDefault() + ", " + db.Areas.Where(x => x.FCode == HoSo.HuyenCuTru).Select(y => y.FName).FirstOrDefault() + ", " + db.Areas.Where(x => x.FCode == HoSo.TinhCuTru).Select(y => y.FName).FirstOrDefault();
            string Ngay = DateTime.Now.Day < 10 ? "0" + DateTime.Now.Day.ToString() : DateTime.Now.Day.ToString();
            string Thang = DateTime.Now.Month < 10 ? "0" + DateTime.Now.Month.ToString() : DateTime.Now.Month.ToString();
            string dataDir = HttpContext.Current.Server.MapPath("~/Templates/PHIEU_DANG_KY_TUYEN_SINH_THCS_2022.docx");
            // Open an existing document.
            Aspose.Words.Document doc = new Aspose.Words.Document(dataDir);
            using (FileStream file = new FileStream(dataDir, FileMode.Open, FileAccess.Read))
            {
                doc = new Aspose.Words.Document(file);
                file.Close();
            }
            // Trim trailing and leading whitespaces mail merge values
            doc.MailMerge.TrimWhitespaces = false;

            // Fill the fields in the document with user data.
            doc.MailMerge.Execute(
                new string[] { "TenDonVi", "HoVaTen", "NgaySinh", "GioiTinh", "DanToc", "NoiSinh", "MaDinhDanh", "HocSinhTruong","DoiTuongUuTien", "NoiCuTru", "DatGiai", "DatGiaiKhac", "L1", "L2", "L3", "L4", "L5", "NguoiLienHe", "DiaChiNLH", "SoGiayToNLH", "DienThoaiNLH", "Ngay", "Thang", "Nam" },
                new object[] { DonVi, HoSo.HoVaTen, NgaySinh, GioiTinh, DanToc, NoiSinh, HoSo.MaDinhDanh, HoSo.HocSinhTruong, HoSo.DoiTuongUuTien, NoiCuTru, HoSo.DatGiai ?? "..............................", HoSo.DatGiaiKhac ?? "..............................", HoSo.L1, HoSo.L2, HoSo.L3, HoSo.L4, HoSo.L5, HoSo.NguoiLienHe, HoSo.DiaChiNLH, HoSo.SoGiayToNLH, HoSo.DienThoaiNLH, Ngay, Thang, DateTime.Now.Year.ToString() });
            using (var memoryStream = new MemoryStream())
            {
                doc.Save(memoryStream, SaveFormat.Docx);
                memoryStream.Position = 0;
                XWPFDocument doc2 = new XWPFDocument(memoryStream);
                doc2.RemoveBodyElement(0);
                using (var memoryStream2 = new MemoryStream())
                {
                    doc2.Write(memoryStream2);
                    byte[] docBytes = memoryStream2.ToArray();
                    var response = new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new ByteArrayContent(docBytes) // pdfBytes     xlsBytes
                    };
                    response.Content.Headers.ContentType = new MediaTypeHeaderValue
                           ("application/vnd.ms-word");  //  pdf  vnd.ms-excel
                    response.Content.Headers.ContentDisposition =
                           new ContentDispositionHeaderValue("attachment")
                           {
                               FileName = "PHIEU_DANG_KY_TUYEN_SINH_THCS_2022.docx"  // pdf  xlsx
                           };

                    return response;
                }
            }
        }
        public class HS_THPT
        {
            public string MaDonVi { get; set; }
            public string HoVaTen { get; set; }
            public DateTime NgaySinh { get; set; }
            public string GioiTinh { get; set; }
            public string DanToc { get; set; }
            public string TinhNoiSinh { get; set; }
            public string HuyenNoiSinh { get; set; }
            public string MaDinhDanh { get; set; }
            public string HocSinhTruong { get; set; }
            public string DoiTuongUuTien { get; set; }
            public string TinhCuTru { get; set; }
            public string HuyenCuTru { get; set; }
            public string XaCuTru { get; set; }
            public string NoiCuTru { get; set; }
            public string NguoiLienHe { get; set; }
            public string SoGiayToNLH { get; set; }
            public string DiaChiNLH { get; set; }
            public string DienThoaiNLH { get; set; }
            public string DatGiai { get; set; }
            public string DatGiaiKhac { get; set; }
            public string HL6 { get; set; }
            public string HK6 { get; set; }
            public string HL7 { get; set; }
            public string HK7 { get; set; }
            public string HL8 { get; set; }
            public string HK8 { get; set; }
            public string HL9 { get; set; }
            public string HK9 { get; set; }
            public string NhomMonNV1 { get; set; }
            public string NhomMonNV2 { get; set; }
            public List<NguyenVong> NguyenVong { get; set; }
        }

        [HttpPost]
        [Route("api/onegate/in_ho_so_thpt")]
        public HttpResponseMessage in_ho_so_thpt(HS_THPT HoSo)
        {
            string DonVi = db.Organizations.Where(x => x.FCode == HoSo.MaDonVi).Select(y => y.FName).FirstOrDefault().Replace("Trường ", "").Replace("Trường", "");
            string NgaySinh = HoSo.NgaySinh.ToString("dd/MM/yyyy");
            string GioiTinh = HoSo.GioiTinh == "nam" ? "Nam" : "Nữ";
            string DanToc = db.Nations.Where(x => x.FCode == HoSo.DanToc).Select(y => y.FName).FirstOrDefault();
            string NoiSinh = db.Areas.Where(x => x.FCode == HoSo.HuyenNoiSinh).Select(y => y.FName).FirstOrDefault() + ", " + db.Areas.Where(x => x.FCode == HoSo.TinhNoiSinh).Select(y => y.FName).FirstOrDefault();
            string NoiCuTru = HoSo.NoiCuTru + ", " + db.Areas.Where(x => x.FCode == HoSo.XaCuTru).Select(y => y.FName).FirstOrDefault() + ", " + db.Areas.Where(x => x.FCode == HoSo.HuyenCuTru).Select(y => y.FName).FirstOrDefault() + ", " + db.Areas.Where(x => x.FCode == HoSo.TinhCuTru).Select(y => y.FName).FirstOrDefault();
            string Ngay = DateTime.Now.Day < 10 ? "0" + DateTime.Now.Day.ToString() : DateTime.Now.Day.ToString();
            string Thang = DateTime.Now.Month < 10 ? "0" + DateTime.Now.Month.ToString() : DateTime.Now.Month.ToString();
            string dataDir = HttpContext.Current.Server.MapPath("~/Templates/PHIEU_DANG_KY_TUYEN_SINH_LOP_10_THPT.docx");
            // Open an existing document.
            Aspose.Words.Document doc = new Aspose.Words.Document(dataDir);
            using (FileStream file = new FileStream(dataDir, FileMode.Open, FileAccess.Read))
            {
                doc = new Aspose.Words.Document(file);
                file.Close();
            }
            // Trim trailing and leading whitespaces mail merge values
            doc.MailMerge.TrimWhitespaces = false;

            // Fill the fields in the document with user data.
            doc.MailMerge.Execute(
                new string[] { "TenDonVi", "HoVaTen", "NgaySinh", "GioiTinh", "DanToc", "NoiSinh", "MaDinhDanh", "HocSinhTruong","DoiTuongUuTien", "NoiCuTru", "DatGiai", "DatGiaiKhac", "HL6", "HK6", "HL7", "HK7", "HL8", "HK8", "HL9", "HK9", "NguoiLienHe", "DiaChiNLH", "SoGiayToNLH", "DienThoaiNLH", "Ngay", "Thang", "Nam" },
                new object[] { DonVi, HoSo.HoVaTen, NgaySinh, GioiTinh, DanToc, NoiSinh, HoSo.MaDinhDanh, HoSo.HocSinhTruong, HoSo.DoiTuongUuTien, NoiCuTru, HoSo.DatGiai ?? "..............................", HoSo.DatGiaiKhac ?? "..............................", HoSo.HL6, HoSo.HK6, HoSo.HL7, HoSo.HK7, HoSo.HL8, HoSo.HK8, HoSo.HL9, HoSo.HK9, HoSo.NguoiLienHe, HoSo.DiaChiNLH, HoSo.SoGiayToNLH, HoSo.DienThoaiNLH, Ngay, Thang, DateTime.Now.Year.ToString() });
            using (var memoryStream = new MemoryStream())
            {
                doc.Save(memoryStream, SaveFormat.Docx);
                memoryStream.Position = 0;
                XWPFDocument doc2 = new XWPFDocument(memoryStream);
                doc2.RemoveBodyElement(0);

                XWPFTable table = doc2.GetTableArray(2);
                if (HoSo.NhomMonNV1 == "KHTN")
                {
                    XWPFParagraph paragraph11 = table.GetRow(2).GetCell(1).GetParagraphArray(0);
                    paragraph11.Alignment = NPOI.XWPF.UserModel.ParagraphAlignment.CENTER;
                    XWPFRun Run = paragraph11.CreateRun();
                    Run.SetText("x");

                    XWPFParagraph paragraph12 = table.GetRow(2).GetCell(2).GetParagraphArray(0);
                    paragraph12.Alignment = NPOI.XWPF.UserModel.ParagraphAlignment.CENTER;
                    Run = paragraph12.CreateRun();
                    Run.SetText("x");

                    XWPFParagraph paragraph13 = table.GetRow(2).GetCell(3).GetParagraphArray(0);
                    paragraph13.Alignment = NPOI.XWPF.UserModel.ParagraphAlignment.CENTER;
                    Run = paragraph13.CreateRun();
                    Run.SetText("x");

                    int i = 4;
                    foreach (var nmh in HoSo.NguyenVong[0].NhomMonHoc)
                    {
                        foreach (var mh in nmh.MonHoc)
                        {
                            if (mh.Check == true)
                            {
                                XWPFParagraph paragraph = table.GetRow(2).GetCell(i).GetParagraphArray(0);
                                paragraph.Alignment = NPOI.XWPF.UserModel.ParagraphAlignment.CENTER;
                                Run = paragraph.CreateRun();
                                Run.SetText("x");
                            }
                            i++;
                        }
                    }
                }
                if (HoSo.NhomMonNV1 == "KHXH")
                {
                    XWPFParagraph paragraph11 = table.GetRow(2).GetCell(4).GetParagraphArray(0);
                    paragraph11.Alignment = NPOI.XWPF.UserModel.ParagraphAlignment.CENTER;
                    XWPFRun Run = paragraph11.CreateRun();
                    Run.SetText("x");

                    XWPFParagraph paragraph12 = table.GetRow(2).GetCell(5).GetParagraphArray(0);
                    paragraph12.Alignment = NPOI.XWPF.UserModel.ParagraphAlignment.CENTER;
                    Run = paragraph12.CreateRun();
                    Run.SetText("x");

                    XWPFParagraph paragraph13 = table.GetRow(2).GetCell(6).GetParagraphArray(0);
                    paragraph13.Alignment = NPOI.XWPF.UserModel.ParagraphAlignment.CENTER;
                    Run = paragraph13.CreateRun();
                    Run.SetText("x");

                    int i = 1;
                    foreach (var nmh in HoSo.NguyenVong[0].NhomMonHoc)
                    {
                        foreach (var mh in nmh.MonHoc)
                        {
                            if (mh.Check == true)
                            {
                                XWPFParagraph paragraph = table.GetRow(2).GetCell(i).GetParagraphArray(0);
                                paragraph.Alignment = NPOI.XWPF.UserModel.ParagraphAlignment.CENTER;
                                Run = paragraph.CreateRun();
                                Run.SetText("x");
                            }
                            i++;
                        }
                        i += 3;
                    }
                }
                if (!String.IsNullOrEmpty(HoSo.NhomMonNV2))
                {
                    if (HoSo.NguyenVong[0].MaTruong != HoSo.NguyenVong[1].MaTruong)
                    {
                        string MaDV2 = HoSo.NguyenVong[1].MaTruong;
                        string DonViNV2 = db.Organizations.Where(x => x.FCode == MaDV2).Select(y => y.FName).FirstOrDefault().Replace("Trường ", "").Replace("Trường", "");
                        XWPFParagraph p = doc2.GetParagraphArray(17);
                        XWPFRun Run = p.CreateRun();
                        Run.SetText("- Nguyện vọng 2: Trường " + DonViNV2);
                        Run.FontFamily = "Times New Roman";
                        Run.FontSize = 13;
                        CT_Tbl a = new CT_Tbl();
                        a.Set(table.GetCTTbl());
                        XWPFTable table2 = new XWPFTable(a, doc2);
                        doc2.SetTable(3, table2);
                        if (HoSo.NhomMonNV2 == "KHTN")
                        {
                            XWPFParagraph paragraph11 = table2.GetRow(3).GetCell(1).GetParagraphArray(0);
                            paragraph11.Alignment = NPOI.XWPF.UserModel.ParagraphAlignment.CENTER;
                            Run = paragraph11.CreateRun();
                            Run.SetText("x");

                            XWPFParagraph paragraph12 = table2.GetRow(3).GetCell(2).GetParagraphArray(0);
                            paragraph12.Alignment = NPOI.XWPF.UserModel.ParagraphAlignment.CENTER;
                            Run = paragraph12.CreateRun();
                            Run.SetText("x");

                            XWPFParagraph paragraph13 = table2.GetRow(3).GetCell(3).GetParagraphArray(0);
                            paragraph13.Alignment = NPOI.XWPF.UserModel.ParagraphAlignment.CENTER;
                            Run = paragraph13.CreateRun();
                            Run.SetText("x");

                            int i = 4;
                            foreach (var nmh in HoSo.NguyenVong[1].NhomMonHoc)
                            {
                                foreach (var mh in nmh.MonHoc)
                                {
                                    if (mh.Check == true)
                                    {
                                        XWPFParagraph paragraph = table2.GetRow(3).GetCell(i).GetParagraphArray(0);
                                        paragraph.Alignment = NPOI.XWPF.UserModel.ParagraphAlignment.CENTER;
                                        Run = paragraph.CreateRun();
                                        Run.SetText("x");
                                    }
                                    i++;
                                }
                            }
                        }
                        if (HoSo.NhomMonNV2 == "KHXH")
                        {
                            XWPFParagraph paragraph11 = table2.GetRow(3).GetCell(4).GetParagraphArray(0);
                            paragraph11.Alignment = NPOI.XWPF.UserModel.ParagraphAlignment.CENTER;
                            Run = paragraph11.CreateRun();
                            Run.SetText("x");

                            XWPFParagraph paragraph12 = table2.GetRow(3).GetCell(5).GetParagraphArray(0);
                            paragraph12.Alignment = NPOI.XWPF.UserModel.ParagraphAlignment.CENTER;
                            Run = paragraph12.CreateRun();
                            Run.SetText("x");

                            XWPFParagraph paragraph13 = table2.GetRow(3).GetCell(6).GetParagraphArray(0);
                            paragraph13.Alignment = NPOI.XWPF.UserModel.ParagraphAlignment.CENTER;
                            Run = paragraph13.CreateRun();
                            Run.SetText("x");

                            int i = 1;
                            foreach (var nmh in HoSo.NguyenVong[1].NhomMonHoc)
                            {
                                foreach (var mh in nmh.MonHoc)
                                {
                                    if (mh.Check == true)
                                    {
                                        XWPFParagraph paragraph = table2.GetRow(3).GetCell(i).GetParagraphArray(0);
                                        paragraph.Alignment = NPOI.XWPF.UserModel.ParagraphAlignment.CENTER;
                                        Run = paragraph.CreateRun();
                                        Run.SetText("x");
                                    }
                                    i++;
                                }
                                i += 3;
                            }
                        }

                        XWPFTable table3 = doc2.GetTableArray(2);
                        table3.RemoveRow(3);
                        XWPFTable table4 = doc2.GetTableArray(3);
                        table4.RemoveRow(2);
                    }
                    else
                    {
                        if (HoSo.NhomMonNV2 == "KHTN")
                        {
                            XWPFParagraph paragraph11 = table.GetRow(3).GetCell(1).GetParagraphArray(0);
                            paragraph11.Alignment = NPOI.XWPF.UserModel.ParagraphAlignment.CENTER;
                            XWPFRun Run = paragraph11.CreateRun();
                            Run.SetText("x");

                            XWPFParagraph paragraph12 = table.GetRow(3).GetCell(2).GetParagraphArray(0);
                            paragraph12.Alignment = NPOI.XWPF.UserModel.ParagraphAlignment.CENTER;
                            Run = paragraph12.CreateRun();
                            Run.SetText("x");

                            XWPFParagraph paragraph13 = table.GetRow(3).GetCell(3).GetParagraphArray(0);
                            paragraph13.Alignment = NPOI.XWPF.UserModel.ParagraphAlignment.CENTER;
                            Run = paragraph13.CreateRun();
                            Run.SetText("x");

                            int i = 4;
                            foreach (var nmh in HoSo.NguyenVong[1].NhomMonHoc)
                            {
                                foreach (var mh in nmh.MonHoc)
                                {
                                    if (mh.Check == true)
                                    {
                                        XWPFParagraph paragraph = table.GetRow(3).GetCell(i).GetParagraphArray(0);
                                        paragraph.Alignment = NPOI.XWPF.UserModel.ParagraphAlignment.CENTER;
                                        Run = paragraph.CreateRun();
                                        Run.SetText("x");
                                    }
                                    i++;
                                }
                            }
                        }
                        if (HoSo.NhomMonNV2 == "KHXH")
                        {
                            XWPFParagraph paragraph11 = table.GetRow(3).GetCell(4).GetParagraphArray(0);
                            paragraph11.Alignment = NPOI.XWPF.UserModel.ParagraphAlignment.CENTER;
                            XWPFRun Run = paragraph11.CreateRun();
                            Run.SetText("x");

                            XWPFParagraph paragraph12 = table.GetRow(3).GetCell(5).GetParagraphArray(0);
                            paragraph12.Alignment = NPOI.XWPF.UserModel.ParagraphAlignment.CENTER;
                            Run = paragraph12.CreateRun();
                            Run.SetText("x");

                            XWPFParagraph paragraph13 = table.GetRow(3).GetCell(6).GetParagraphArray(0);
                            paragraph13.Alignment = NPOI.XWPF.UserModel.ParagraphAlignment.CENTER;
                            Run = paragraph13.CreateRun();
                            Run.SetText("x");

                            int i = 1;
                            foreach (var nmh in HoSo.NguyenVong[1].NhomMonHoc)
                            {
                                foreach (var mh in nmh.MonHoc)
                                {
                                    if (mh.Check == true)
                                    {
                                        XWPFParagraph paragraph = table.GetRow(3).GetCell(i).GetParagraphArray(0);
                                        paragraph.Alignment = NPOI.XWPF.UserModel.ParagraphAlignment.CENTER;
                                        Run = paragraph.CreateRun();
                                        Run.SetText("x");
                                    }
                                    i++;
                                }
                                i += 3;
                            }
                        }
                    }
                }
                using (var memoryStream2 = new MemoryStream())
                {
                    doc2.Write(memoryStream2);
                    byte[] docBytes = memoryStream2.ToArray();
                    var response = new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new ByteArrayContent(docBytes) // pdfBytes     xlsBytes
                    };
                    response.Content.Headers.ContentType = new MediaTypeHeaderValue
                           ("application/vnd.ms-word");  //  pdf  vnd.ms-excel
                    response.Content.Headers.ContentDisposition =
                           new ContentDispositionHeaderValue("attachment")
                           {
                               FileName = "PHIEU_DANG_KY_TUYEN_SINH_LOP_10_THPT.docx"  // pdf  xlsx
                           };

                    return response;
                }
            }
        }
        
        public class HS_THPT_CHUYEN
        {
            public string MaDonVi { get; set; }
            public string HoVaTen { get; set; }
            public DateTime NgaySinh { get; set; }
            public string GioiTinh { get; set; }
            public string DanToc { get; set; }
            public string TinhNoiSinh { get; set; }
            public string HuyenNoiSinh { get; set; }
            public string MaDinhDanh { get; set; }
            public string HocSinhTruong { get; set; }
            public string DoiTuongUuTien { get; set; }
            public string TinhCuTru { get; set; }
            public string HuyenCuTru { get; set; }
            public string XaCuTru { get; set; }
            public string NoiCuTru { get; set; }
            public string NguoiLienHe { get; set; }
            public string SoGiayToNLH { get; set; }
            public string DiaChiNLH { get; set; }
            public string DienThoaiNLH { get; set; }
            public string DatGiai { get; set; }
            public string DatGiaiKhac { get; set; }
            public string HL6 { get; set; }
            public string HK6 { get; set; }
            public string HL7 { get; set; }
            public string HK7 { get; set; }
            public string HL8 { get; set; }
            public string HK8 { get; set; }
            public string HL9 { get; set; }
            public string HK9 { get; set; }
            public string MonHocChuyen { get; set; }
            public string NhomMonNV2 { get; set; }
            public List<NguyenVong> NguyenVong { get; set; }
        }

        [HttpPost]
        [Route("api/onegate/in_ho_so_thpt_chuyen")]
        public HttpResponseMessage in_ho_so_thpt_chuyen(HS_THPT_CHUYEN HoSo)
        {
            string DonVi = db.Organizations.Where(x => x.FCode == HoSo.MaDonVi).Select(y => y.FName).FirstOrDefault().Replace("Trường ", "").Replace("Trường", "");
            string NgaySinh = HoSo.NgaySinh.ToString("dd/MM/yyyy");
            string GioiTinh = HoSo.GioiTinh == "nam" ? "Nam" : "Nữ";
            string DanToc = db.Nations.Where(x => x.FCode == HoSo.DanToc).Select(y => y.FName).FirstOrDefault();
            string NoiSinh = db.Areas.Where(x => x.FCode == HoSo.HuyenNoiSinh).Select(y => y.FName).FirstOrDefault() + ", " + db.Areas.Where(x => x.FCode == HoSo.TinhNoiSinh).Select(y => y.FName).FirstOrDefault();
            string NoiCuTru = HoSo.NoiCuTru + ", " + db.Areas.Where(x => x.FCode == HoSo.XaCuTru).Select(y => y.FName).FirstOrDefault() + ", " + db.Areas.Where(x => x.FCode == HoSo.HuyenCuTru).Select(y => y.FName).FirstOrDefault() + ", " + db.Areas.Where(x => x.FCode == HoSo.TinhCuTru).Select(y => y.FName).FirstOrDefault();
            string Ngay = DateTime.Now.Day < 10 ? "0" + DateTime.Now.Day.ToString() : DateTime.Now.Day.ToString();
            string Thang = DateTime.Now.Month < 10 ? "0" + DateTime.Now.Month.ToString() : DateTime.Now.Month.ToString();
            string dataDir = HttpContext.Current.Server.MapPath("~/Templates/PHIEU_DANG_KY_TUYEN_SINH_LOP_10_CHUYEN.docx");
            // Open an existing document.
            Aspose.Words.Document doc = new Aspose.Words.Document(dataDir);
            using (FileStream file = new FileStream(dataDir, FileMode.Open, FileAccess.Read))
            {
                doc = new Aspose.Words.Document(file);
                file.Close();
            }
            // Trim trailing and leading whitespaces mail merge values
            doc.MailMerge.TrimWhitespaces = false;

            // Fill the fields in the document with user data.
            doc.MailMerge.Execute(
                new string[] { "TenDonVi", "HoVaTen", "NgaySinh", "GioiTinh", "DanToc", "NoiSinh", "MaDinhDanh", "HocSinhTruong","DoiTuongUuTien", "NoiCuTru", "DatGiai", "DatGiaiKhac", "HL6", "HK6", "HL7", "HK7", "HL8", "HK8", "HL9", "HK9", "MonChuyen", "NguoiLienHe", "DiaChiNLH", "SoGiayToNLH", "DienThoaiNLH", "Ngay", "Thang", "Nam" },
                new object[] { DonVi, HoSo.HoVaTen, NgaySinh, GioiTinh, DanToc, NoiSinh, HoSo.MaDinhDanh, HoSo.HocSinhTruong, HoSo.DoiTuongUuTien, NoiCuTru, HoSo.DatGiai ?? "..............................", HoSo.DatGiaiKhac ?? "..............................", HoSo.HL6, HoSo.HK6, HoSo.HL7, HoSo.HK7, HoSo.HL8, HoSo.HK8, HoSo.HL9, HoSo.HK9, HoSo.MonHocChuyen, HoSo.NguoiLienHe, HoSo.DiaChiNLH, HoSo.SoGiayToNLH, HoSo.DienThoaiNLH, Ngay, Thang, DateTime.Now.Year.ToString() });
            using (var memoryStream = new MemoryStream())
            {
                doc.Save(memoryStream, SaveFormat.Docx);
                memoryStream.Position = 0;
                XWPFDocument doc2 = new XWPFDocument(memoryStream);
                doc2.RemoveBodyElement(0);

                XWPFTable table = doc2.GetTableArray(2);
                
                if (!String.IsNullOrEmpty(HoSo.NhomMonNV2))
                {
                    if (HoSo.NhomMonNV2 == "KHTN")
                    {
                        XWPFParagraph paragraph11 = table.GetRow(2).GetCell(1).GetParagraphArray(0);
                        paragraph11.Alignment = NPOI.XWPF.UserModel.ParagraphAlignment.CENTER;
                        XWPFRun Run = paragraph11.CreateRun();
                        Run.SetText("x");

                        XWPFParagraph paragraph12 = table.GetRow(2).GetCell(2).GetParagraphArray(0);
                        paragraph12.Alignment = NPOI.XWPF.UserModel.ParagraphAlignment.CENTER;
                        Run = paragraph12.CreateRun();
                        Run.SetText("x");

                        XWPFParagraph paragraph13 = table.GetRow(2).GetCell(3).GetParagraphArray(0);
                        paragraph13.Alignment = NPOI.XWPF.UserModel.ParagraphAlignment.CENTER;
                        Run = paragraph13.CreateRun();
                        Run.SetText("x");

                        int i = 4;
                        foreach (var nmh in HoSo.NguyenVong[0].NhomMonHoc)
                        {
                            foreach (var mh in nmh.MonHoc)
                            {
                                if (mh.Check == true)
                                {
                                    XWPFParagraph paragraph = table.GetRow(2).GetCell(i).GetParagraphArray(0);
                                    paragraph.Alignment = NPOI.XWPF.UserModel.ParagraphAlignment.CENTER;
                                    Run = paragraph.CreateRun();
                                    Run.SetText("x");
                                }
                                i++;
                            }
                        }
                    }
                    if (HoSo.NhomMonNV2 == "KHXH")
                    {
                        XWPFParagraph paragraph11 = table.GetRow(3).GetCell(4).GetParagraphArray(0);
                        paragraph11.Alignment = NPOI.XWPF.UserModel.ParagraphAlignment.CENTER;
                        XWPFRun Run = paragraph11.CreateRun();
                        Run.SetText("x");

                        XWPFParagraph paragraph12 = table.GetRow(3).GetCell(5).GetParagraphArray(0);
                        paragraph12.Alignment = NPOI.XWPF.UserModel.ParagraphAlignment.CENTER;
                        Run = paragraph12.CreateRun();
                        Run.SetText("x");

                        XWPFParagraph paragraph13 = table.GetRow(3).GetCell(6).GetParagraphArray(0);
                        paragraph13.Alignment = NPOI.XWPF.UserModel.ParagraphAlignment.CENTER;
                        Run = paragraph13.CreateRun();
                        Run.SetText("x");

                        int i = 1;
                        foreach (var nmh in HoSo.NguyenVong[0].NhomMonHoc)
                        {
                            foreach (var mh in nmh.MonHoc)
                            {
                                if (mh.Check == true)
                                {
                                    XWPFParagraph paragraph = table.GetRow(3).GetCell(i).GetParagraphArray(0);
                                    paragraph.Alignment = NPOI.XWPF.UserModel.ParagraphAlignment.CENTER;
                                    Run = paragraph.CreateRun();
                                    Run.SetText("x");
                                }
                                i++;
                            }
                            i += 3;
                        }
                    }   
                }
                using (var memoryStream2 = new MemoryStream())
                {
                    doc2.Write(memoryStream2);
                    byte[] docBytes = memoryStream2.ToArray();
                    var response = new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new ByteArrayContent(docBytes) // pdfBytes     xlsBytes
                    };
                    response.Content.Headers.ContentType = new MediaTypeHeaderValue
                           ("application/vnd.ms-word");  //  pdf  vnd.ms-excel
                    response.Content.Headers.ContentDisposition =
                           new ContentDispositionHeaderValue("attachment")
                           {
                               FileName = "PHIEU_DANG_KY_TUYEN_SINH_LOP_10_CHUYEN.docx"  // pdf  xlsx
                           };

                    return response;
                }
            }
        }

        public class HS_PTDTNT
        {
            public string MaDonVi { get; set; }
            public string MaDonViNV2 { get; set; }
            public string HoVaTen { get; set; }
            public DateTime NgaySinh { get; set; }
            public string GioiTinh { get; set; }
            public string DanToc { get; set; }
            public string TinhNoiSinh { get; set; }
            public string HuyenNoiSinh { get; set; }
            public string MaDinhDanh { get; set; }
            public string HocSinhTruong { get; set; }
            public string DoiTuongUuTien { get; set; }
            public string DiemUuTien { get; set; }
            public string TinhCuTru { get; set; }
            public string HuyenCuTru { get; set; }
            public string XaCuTru { get; set; }
            public string NoiCuTru { get; set; }
            public string NguoiLienHe { get; set; }
            public string SoGiayToNLH { get; set; }
            public string DienThoaiNLH { get; set; }
            public string DatGiai { get; set; }
            public string DatGiaiKhac { get; set; }
            public string HL6 { get; set; }
            public string HK6 { get; set; }
            public string HL7 { get; set; }
            public string HK7 { get; set; }
            public string HL8 { get; set; }
            public string HK8 { get; set; }
            public string HL9 { get; set; }
            public string HK9 { get; set; }
            public string NhomMonNV1 { get; set; }
            public string NhomMonNV2 { get; set; }
            public List<NguyenVong> NguyenVong { get; set; }
        }

        [HttpPost]
        [Route("api/onegate/in_ho_so_ptdtnt")]
        public HttpResponseMessage in_ho_so_ptdtnt(HS_PTDTNT HoSo)
        {
            string DonVi = db.Organizations.Where(x => x.FCode == HoSo.MaDonVi).Select(y => y.FName).FirstOrDefault().Replace("Trường ", "").Replace("Trường", "");
            string DonViNV2 = db.Organizations.Where(x => x.FCode == HoSo.MaDonViNV2).Select(y => y.FName).FirstOrDefault().Replace("Trường ", "").Replace("Trường", "");
            string NgaySinh = HoSo.NgaySinh.ToString("dd/MM/yyyy");
            string GioiTinh = HoSo.GioiTinh == "nam" ? "Nam" : "Nữ";
            string DanToc = db.Nations.Where(x => x.FCode == HoSo.DanToc).Select(y => y.FName).FirstOrDefault();
            string NoiSinh = db.Areas.Where(x => x.FCode == HoSo.HuyenNoiSinh).Select(y => y.FName).FirstOrDefault() + ", " + db.Areas.Where(x => x.FCode == HoSo.TinhNoiSinh).Select(y => y.FName).FirstOrDefault();
            string NoiCuTru = HoSo.NoiCuTru + ", " + db.Areas.Where(x => x.FCode == HoSo.XaCuTru).Select(y => y.FName).FirstOrDefault() + ", " + db.Areas.Where(x => x.FCode == HoSo.HuyenCuTru).Select(y => y.FName).FirstOrDefault() + ", " + db.Areas.Where(x => x.FCode == HoSo.TinhCuTru).Select(y => y.FName).FirstOrDefault();
            string Ngay = DateTime.Now.Day < 10 ? "0" + DateTime.Now.Day.ToString() : DateTime.Now.Day.ToString();
            string Thang = DateTime.Now.Month < 10 ? "0" + DateTime.Now.Month.ToString() : DateTime.Now.Month.ToString();
            string dataDir = HttpContext.Current.Server.MapPath("~/Templates/PHIEU_DANG_KY_TUYEN_SINH_LOP_10_NOI_TRU.docx");
            // Open an existing document.
            Aspose.Words.Document doc = new Aspose.Words.Document(dataDir);
            using (FileStream file = new FileStream(dataDir, FileMode.Open, FileAccess.Read))
            {
                doc = new Aspose.Words.Document(file);
                file.Close();
            }
            // Trim trailing and leading whitespaces mail merge values
            doc.MailMerge.TrimWhitespaces = false;

            // Fill the fields in the document with user data.
            doc.MailMerge.Execute(
                new string[] { "TenDonVi", "TenDonViNV2", "HoVaTen", "NgaySinh", "GioiTinh", "DanToc", "NoiSinh", "MaDinhDanh", "HocSinhTruong", "DoiTuongUuTien", "DiemUuTien", "NoiCuTru", "DatGiai", "DatGiaiKhac", "HL6", "HK6", "HL7", "HK7", "HL8", "HK8", "HL9", "HK9", "NguoiLienHe", "SoGiayToNLH", "DienThoaiNLH", "Ngay", "Thang", "Nam" },
                new object[] { DonVi, DonViNV2, HoSo.HoVaTen, NgaySinh, GioiTinh, DanToc, NoiSinh, HoSo.MaDinhDanh, HoSo.HocSinhTruong, HoSo.DoiTuongUuTien, HoSo.DiemUuTien, NoiCuTru, HoSo.DatGiai ?? "..............................", HoSo.DatGiaiKhac ?? "..............................", HoSo.HL6, HoSo.HK6, HoSo.HL7, HoSo.HK7, HoSo.HL8, HoSo.HK8, HoSo.HL9, HoSo.HK9, HoSo.NguoiLienHe, HoSo.SoGiayToNLH, HoSo.DienThoaiNLH, Ngay, Thang, DateTime.Now.Year.ToString() });
            using (var memoryStream = new MemoryStream())
            {
                doc.Save(memoryStream, SaveFormat.Docx);
                memoryStream.Position = 0;
                XWPFDocument doc2 = new XWPFDocument(memoryStream);
                doc2.RemoveBodyElement(0);

                XWPFTable table = doc2.GetTableArray(2);
                XWPFTable table2 = doc2.GetTableArray(3);
                if (HoSo.NhomMonNV1 == "KHTN")
                {
                    XWPFParagraph paragraph11 = table.GetRow(2).GetCell(1).GetParagraphArray(0);
                    paragraph11.Alignment = NPOI.XWPF.UserModel.ParagraphAlignment.CENTER;
                    XWPFRun Run = paragraph11.CreateRun();
                    Run.SetText("x");

                    XWPFParagraph paragraph12 = table.GetRow(2).GetCell(2).GetParagraphArray(0);
                    paragraph12.Alignment = NPOI.XWPF.UserModel.ParagraphAlignment.CENTER;
                    Run = paragraph12.CreateRun();
                    Run.SetText("x");

                    XWPFParagraph paragraph13 = table.GetRow(2).GetCell(3).GetParagraphArray(0);
                    paragraph13.Alignment = NPOI.XWPF.UserModel.ParagraphAlignment.CENTER;
                    Run = paragraph13.CreateRun();
                    Run.SetText("x");

                    if ((HoSo.MaDonVi != HoSo.MaDonViNV2) && !String.IsNullOrEmpty(HoSo.MaDonViNV2))
                    {
                        XWPFParagraph paragraph21 = table2.GetRow(2).GetCell(1).GetParagraphArray(0);
                        paragraph21.Alignment = NPOI.XWPF.UserModel.ParagraphAlignment.CENTER;
                        Run = paragraph21.CreateRun();
                        Run.SetText("x");

                        XWPFParagraph paragraph22 = table2.GetRow(2).GetCell(2).GetParagraphArray(0);
                        paragraph22.Alignment = NPOI.XWPF.UserModel.ParagraphAlignment.CENTER;
                        Run = paragraph22.CreateRun();
                        Run.SetText("x");

                        XWPFParagraph paragraph23 = table2.GetRow(2).GetCell(3).GetParagraphArray(0);
                        paragraph23.Alignment = NPOI.XWPF.UserModel.ParagraphAlignment.CENTER;
                        Run = paragraph23.CreateRun();
                        Run.SetText("x");
                    }
                    int i = 4;
                    foreach (var nmh in HoSo.NguyenVong[0].NhomMonHoc)
                    {
                        foreach (var mh in nmh.MonHoc)
                        {
                            if (mh.Check == true)
                            {
                                XWPFParagraph paragraph = table.GetRow(2).GetCell(i).GetParagraphArray(0);
                                paragraph.Alignment = NPOI.XWPF.UserModel.ParagraphAlignment.CENTER;
                                Run = paragraph.CreateRun();
                                Run.SetText("x");

                                if ((HoSo.MaDonVi != HoSo.MaDonViNV2) && !String.IsNullOrEmpty(HoSo.MaDonViNV2))
                                {
                                    XWPFParagraph paragraph2 = table2.GetRow(2).GetCell(i).GetParagraphArray(0);
                                    paragraph2.Alignment = NPOI.XWPF.UserModel.ParagraphAlignment.CENTER;
                                    Run = paragraph2.CreateRun();
                                    Run.SetText("x");
                                }
                            }
                            i++;
                        }
                    }
                }
                if (HoSo.NhomMonNV1 == "KHXH")
                {
                    XWPFParagraph paragraph11 = table.GetRow(2).GetCell(4).GetParagraphArray(0);
                    paragraph11.Alignment = NPOI.XWPF.UserModel.ParagraphAlignment.CENTER;
                    XWPFRun Run = paragraph11.CreateRun();
                    Run.SetText("x");

                    XWPFParagraph paragraph12 = table.GetRow(2).GetCell(5).GetParagraphArray(0);
                    paragraph12.Alignment = NPOI.XWPF.UserModel.ParagraphAlignment.CENTER;
                    Run = paragraph12.CreateRun();
                    Run.SetText("x");

                    XWPFParagraph paragraph13 = table.GetRow(2).GetCell(6).GetParagraphArray(0);
                    paragraph13.Alignment = NPOI.XWPF.UserModel.ParagraphAlignment.CENTER;
                    Run = paragraph13.CreateRun();
                    Run.SetText("x");

                    if ((HoSo.MaDonVi != HoSo.MaDonViNV2) && !String.IsNullOrEmpty(HoSo.MaDonViNV2))
                    {
                        XWPFParagraph paragraph21 = table2.GetRow(2).GetCell(4).GetParagraphArray(0);
                        paragraph21.Alignment = NPOI.XWPF.UserModel.ParagraphAlignment.CENTER;
                        Run = paragraph21.CreateRun();
                        Run.SetText("x");

                        XWPFParagraph paragraph22 = table2.GetRow(2).GetCell(5).GetParagraphArray(0);
                        paragraph22.Alignment = NPOI.XWPF.UserModel.ParagraphAlignment.CENTER;
                        Run = paragraph22.CreateRun();
                        Run.SetText("x");

                        XWPFParagraph paragraph23 = table2.GetRow(2).GetCell(6).GetParagraphArray(0);
                        paragraph23.Alignment = NPOI.XWPF.UserModel.ParagraphAlignment.CENTER;
                        Run = paragraph23.CreateRun();
                        Run.SetText("x");
                    }
                    int i = 1;
                    foreach (var nmh in HoSo.NguyenVong[0].NhomMonHoc)
                    {
                        foreach (var mh in nmh.MonHoc)
                        {
                            if (mh.Check == true)
                            {
                                XWPFParagraph paragraph = table.GetRow(2).GetCell(i).GetParagraphArray(0);
                                paragraph.Alignment = NPOI.XWPF.UserModel.ParagraphAlignment.CENTER;
                                Run = paragraph.CreateRun();
                                Run.SetText("x");

                                if ((HoSo.MaDonVi != HoSo.MaDonViNV2) && !String.IsNullOrEmpty(HoSo.MaDonViNV2))
                                {
                                    XWPFParagraph paragraph2 = table2.GetRow(2).GetCell(i).GetParagraphArray(0);
                                    paragraph2.Alignment = NPOI.XWPF.UserModel.ParagraphAlignment.CENTER;
                                    Run = paragraph2.CreateRun();
                                    Run.SetText("x");
                                }
                            }
                            i++;
                        }
                        i += 3;
                    }
                }
                if (!String.IsNullOrEmpty(HoSo.NhomMonNV2))
                {
                    if (HoSo.NhomMonNV2 == "KHTN")
                    {
                        XWPFParagraph paragraph11 = table.GetRow(3).GetCell(1).GetParagraphArray(0);
                        paragraph11.Alignment = NPOI.XWPF.UserModel.ParagraphAlignment.CENTER;
                        XWPFRun Run = paragraph11.CreateRun();
                        Run.SetText("x");

                        XWPFParagraph paragraph12 = table.GetRow(3).GetCell(2).GetParagraphArray(0);
                        paragraph12.Alignment = NPOI.XWPF.UserModel.ParagraphAlignment.CENTER;
                        Run = paragraph12.CreateRun();
                        Run.SetText("x");

                        XWPFParagraph paragraph13 = table.GetRow(3).GetCell(3).GetParagraphArray(0);
                        paragraph13.Alignment = NPOI.XWPF.UserModel.ParagraphAlignment.CENTER;
                        Run = paragraph13.CreateRun();
                        Run.SetText("x");

                        if ((HoSo.MaDonVi != HoSo.MaDonViNV2) && !String.IsNullOrEmpty(HoSo.MaDonViNV2))
                        {
                            XWPFParagraph paragraph21 = table2.GetRow(3).GetCell(1).GetParagraphArray(0);
                            paragraph21.Alignment = NPOI.XWPF.UserModel.ParagraphAlignment.CENTER;
                            Run = paragraph21.CreateRun();
                            Run.SetText("x");

                            XWPFParagraph paragraph22 = table2.GetRow(3).GetCell(2).GetParagraphArray(0);
                            paragraph22.Alignment = NPOI.XWPF.UserModel.ParagraphAlignment.CENTER;
                            Run = paragraph22.CreateRun();
                            Run.SetText("x");

                            XWPFParagraph paragraph23 = table2.GetRow(3).GetCell(3).GetParagraphArray(0);
                            paragraph23.Alignment = NPOI.XWPF.UserModel.ParagraphAlignment.CENTER;
                            Run = paragraph23.CreateRun();
                            Run.SetText("x");
                        }
                        int i = 4;
                        foreach (var nmh in HoSo.NguyenVong[1].NhomMonHoc)
                        {
                            foreach (var mh in nmh.MonHoc)
                            {
                                if (mh.Check == true)
                                {
                                    XWPFParagraph paragraph = table.GetRow(3).GetCell(i).GetParagraphArray(0);
                                    paragraph.Alignment = NPOI.XWPF.UserModel.ParagraphAlignment.CENTER;
                                    Run = paragraph.CreateRun();
                                    Run.SetText("x");

                                    if ((HoSo.MaDonVi != HoSo.MaDonViNV2) && !String.IsNullOrEmpty(HoSo.MaDonViNV2))
                                    {
                                        XWPFParagraph paragraph2 = table2.GetRow(3).GetCell(i).GetParagraphArray(0);
                                        paragraph2.Alignment = NPOI.XWPF.UserModel.ParagraphAlignment.CENTER;
                                        Run = paragraph2.CreateRun();
                                        Run.SetText("x");
                                    }
                                }
                                i++;
                            }
                        }
                    }
                    if (HoSo.NhomMonNV2 == "KHXH")
                    {
                        XWPFParagraph paragraph11 = table.GetRow(3).GetCell(4).GetParagraphArray(0);
                        paragraph11.Alignment = NPOI.XWPF.UserModel.ParagraphAlignment.CENTER;
                        XWPFRun Run = paragraph11.CreateRun();
                        Run.SetText("x");

                        XWPFParagraph paragraph12 = table.GetRow(3).GetCell(5).GetParagraphArray(0);
                        paragraph12.Alignment = NPOI.XWPF.UserModel.ParagraphAlignment.CENTER;
                        Run = paragraph12.CreateRun();
                        Run.SetText("x");

                        XWPFParagraph paragraph13 = table.GetRow(3).GetCell(6).GetParagraphArray(0);
                        paragraph13.Alignment = NPOI.XWPF.UserModel.ParagraphAlignment.CENTER;
                        Run = paragraph13.CreateRun();
                        Run.SetText("x");

                        if ((HoSo.MaDonVi != HoSo.MaDonViNV2) && !String.IsNullOrEmpty(HoSo.MaDonViNV2))
                        {
                            XWPFParagraph paragraph21 = table2.GetRow(3).GetCell(4).GetParagraphArray(0);
                            paragraph21.Alignment = NPOI.XWPF.UserModel.ParagraphAlignment.CENTER;
                            Run = paragraph21.CreateRun();
                            Run.SetText("x");

                            XWPFParagraph paragraph22 = table2.GetRow(3).GetCell(5).GetParagraphArray(0);
                            paragraph22.Alignment = NPOI.XWPF.UserModel.ParagraphAlignment.CENTER;
                            Run = paragraph22.CreateRun();
                            Run.SetText("x");

                            XWPFParagraph paragraph23 = table2.GetRow(3).GetCell(6).GetParagraphArray(0);
                            paragraph23.Alignment = NPOI.XWPF.UserModel.ParagraphAlignment.CENTER;
                            Run = paragraph23.CreateRun();
                            Run.SetText("x");
                        }
                        int i = 1;
                        foreach (var nmh in HoSo.NguyenVong[1].NhomMonHoc)
                        {
                            foreach (var mh in nmh.MonHoc)
                            {
                                if (mh.Check == true)
                                {
                                    XWPFParagraph paragraph = table.GetRow(3).GetCell(i).GetParagraphArray(0);
                                    paragraph.Alignment = NPOI.XWPF.UserModel.ParagraphAlignment.CENTER;
                                    Run = paragraph.CreateRun();
                                    Run.SetText("x");

                                    if ((HoSo.MaDonVi != HoSo.MaDonViNV2) && !String.IsNullOrEmpty(HoSo.MaDonViNV2))
                                    {
                                        XWPFParagraph paragraph2 = table2.GetRow(3).GetCell(i).GetParagraphArray(0);
                                        paragraph2.Alignment = NPOI.XWPF.UserModel.ParagraphAlignment.CENTER;
                                        Run = paragraph2.CreateRun();
                                        Run.SetText("x");
                                    }
                                }
                                i++;
                            }
                            i += 3;
                        }
                    }   
                }
                using (var memoryStream2 = new MemoryStream())
                {
                    doc2.Write(memoryStream2);
                    byte[] docBytes = memoryStream2.ToArray();
                    var response = new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new ByteArrayContent(docBytes) // pdfBytes     xlsBytes
                    };
                    response.Content.Headers.ContentType = new MediaTypeHeaderValue
                           ("application/vnd.ms-word");  //  pdf  vnd.ms-excel
                    response.Content.Headers.ContentDisposition =
                           new ContentDispositionHeaderValue("attachment")
                           {
                               FileName = "PHIEU_DANG_KY_TUYEN_SINH_LOP_10_NOI_TRU.docx"  // pdf  xlsx
                           };

                    return response;
                }
            }
        }

        #endregion
    }
    public class HOSO_OL
    {
        public HO_SO_ONLINE HoSoOnline { get; set; }
        public HO_SO_TUYENSINH HoSoTuyenSinh { get; set; }
        public List<HO_SO_ONLINE_THUTUC> HoSoOnlineThuTuc { get; set; }
        public List<DINH_KEM_ONLINE> DinhKemOnline { get; set; }
        public KET_QUA_HOC_TAP KetQuaHocTap { get; set; }
        public NguyenVong NguyenVong { get; set; }
        public string NoiSinh { get; set; }
        public string HKTT { get; set; }
        public string DanToc { get; set; }
        public string NguoiGiamHo_CuTru { get; set; }
    }
    public class HOSO_OL_RS
    {
        public HO_SO_ONLINE HoSoOnline { get; set; }
        public HO_SO_TUYENSINH HoSoTuyenSinh { get; set; }
        public List<HO_SO_ONLINE_THUTUC> HoSoOnlineThuTuc { get; set; }
        public List<DINH_KEM_ONLINE> DinhKemOnline { get; set; }
        public KET_QUA_HOC_TAP KetQuaHocTap { get; set; }
        public List<NguyenVong> NguyenVong { get; set; }
        public string NoiSinh { get; set; }
        public string HKTT { get; set; }
        public string DanToc { get; set; }
        public string NguoiGiamHo_CuTru { get; set; }
    }
    public class HOSO_OL_SAVE
    {
        public HO_SO_ONLINE HoSoOnline { get; set; }
        public HO_SO_TUYENSINH HoSoTuyenSinh { get; set; }
        public List<HO_SO_ONLINE_THUTUC> HoSoOnlineThuTuc { get; set; }
        public List<DINH_KEM_ONLINE> DinhKemOnline { get; set; }
        public KET_QUA_HOC_TAP KetQuaHocTap { get; set; }
        public List<NguyenVong> NguyenVong { get; set; }
    }
    public class YearChart
    {
        public string NamHoc { get; set; }
        public int HST { get; set; }
        public int HSKT { get; set; }
    }

}
