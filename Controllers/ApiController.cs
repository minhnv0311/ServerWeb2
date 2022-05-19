using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity.Migrations;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Web;
using System.Web.Http;
using VSDCompany.Models;

namespace VSDCompany.Controllers
{

    [AllowAnonymous]
    public class DvcApiController : ApiController
    {
        private Entities db = new Entities();
        public DvcApiController()
        {
        }

        [HttpGet]
        [Route("api/dvc/list_nation")]
        public IHttpActionResult list_nation()
        {
            try
            {
                var collResult = db.Nations.OrderBy(x => x.FCode).ToList();

                var response = new
                {
                    response_code = "0",
                    response_desc = "Thành công",
                    response_data = collResult
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
        [Route("api/dvc/org")]
        public IHttpActionResult DVC_ORG(string type)
        {
            //test
            try
            {
                var Organizations = db.Organizations.ToList();
                var Area = db.Areas.ToList();
                if (type == "MN")
                {
                    var oColl = Organizations.Where(x => x.IsMN == true).ToList();
                    foreach (var item in oColl)
                    {
                        var Xa = Area.Where(X => X.FCode == item.Ward).Select(y => y.FName).FirstOrDefault();
                        var Huyen = Area.Where(X => X.FCode == item.Disctrict).Select(y => y.FName).FirstOrDefault();
                        item.FName = item.FName + ", " + (Xa != null ? Xa + ", " : "") + Huyen;
                    }
                    var data = new
                    {
                        oColl = oColl,
                        oCollDTNT = oColl.Where(x => x.Loai == "PTDTNT")
                    };
                    var response = new
                    {
                        response_code = "0",
                        response_desc = "Thành công",
                        response_data = data
                    };
                    return Ok(response);
                }
                if (type == "TH")
                {
                    var oColl = Organizations.Where(x => x.IsTH == true).ToList();
                    foreach (var item in oColl)
                    {
                        var Xa = Area.Where(X => X.FCode == item.Ward).Select(y => y.FName).FirstOrDefault();
                        var Huyen = Area.Where(X => X.FCode == item.Disctrict).Select(y => y.FName).FirstOrDefault();
                        item.FName = item.FName + ", " + (Xa != null ? Xa + ", " : "") + Huyen;
                    }
                    var data = new
                    {
                        oColl = oColl,
                        oCollDTNT = oColl.Where(x => x.Loai == "PTDTNT")
                    };
                    var response = new
                    {
                        response_code = "0",
                        response_desc = "Thành công",
                        response_data = data
                    };
                    return Ok(response);
                }
                if (type == "THCS")
                {
                    var oColl = Organizations.Where(x => x.IsTHCS == true).ToList();
                    foreach (var item in oColl)
                    {
                        var Xa = Area.Where(X => X.FCode == item.Ward).Select(y => y.FName).FirstOrDefault();
                        var Huyen = Area.Where(X => X.FCode == item.Disctrict).Select(y => y.FName).FirstOrDefault();
                        item.FName = item.FName + ", " + (Xa != null ? Xa + ", " : "") + Huyen;
                    }
                    var data = new
                    {
                        oColl = oColl,
                        oCollDTNT = oColl.Where(x => x.Loai == "PTDTNT")
                    };
                    var response = new
                    {
                        response_code = "0",
                        response_desc = "Thành công",
                        response_data = data
                    };
                    return Ok(response);
                }
                if (type == "THPT")
                {
                    var oColl = Organizations.Where(x => x.IsTHPT == true).ToList();
                    foreach (var item in oColl)
                    {
                        var Xa = Area.Where(X => X.FCode == item.Ward).Select(y => y.FName).FirstOrDefault();
                        var Huyen = Area.Where(X => X.FCode == item.Disctrict).Select(y => y.FName).FirstOrDefault();
                        item.FName = item.FName + ", " + (Xa != null ? Xa + ", " : "") + Huyen;
                    }
                    var data = new
                    {
                        oColl = oColl,
                        oCollDTNT = oColl.Where(x => x.Loai == "PTDTNT")
                    };
                    var response = new
                    {
                        response_code = "0",
                        response_desc = "Thành công",
                        response_data = data
                    };
                    return Ok(response);
                }
                var rs = new
                {
                    response_code = "-1",
                    response_desc = "Không có dữ liệu",
                    response_data = ""
                };
                return Ok(rs);
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
        [Route("api/dvc/Cap")]
        public IHttpActionResult CAP_ORG()
        {
            //test
            try
            {
                var oColl = db.Organizations.Where(x => x.Type == "DVNHOM").ToList();
                var response = new
                {
                    response_code = "0",
                    response_desc = "Thành công",
                    response_data = oColl
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


        [HttpPost]
        [Route("api/dvc/nhan_ho_so_dinhkem")]
        public IHttpActionResult nhan_ho_so_dinhkem()
        {
            try
            {
                //Xử lsy lưu file đính kèm
                bool exists = System.IO.Directory.Exists(HttpContext.Current.Server.MapPath("~/Uploads/Documents"));
                if (!exists)
                    System.IO.Directory.CreateDirectory(HttpContext.Current.Server.MapPath("~/Uploads/Documents"));
                System.Web.HttpFileCollection httpRequest = System.Web.HttpContext.Current.Request.Files;
                string filename = "";
                List<dynamic> listFile = new List<dynamic>();
                for (int i = 0; i <= httpRequest.Count - 1; i++)
                {
                    System.Web.HttpPostedFile postedfile = httpRequest[i];
                    if (postedfile.ContentLength > 0)
                    {
                        filename = postedfile.FileName;
                        var fileSavePath = Path.Combine(HttpContext.Current.Server.MapPath("~/Uploads/Documents"), filename);
                        postedfile.SaveAs(fileSavePath);
                        var file = new
                        {
                            Name = postedfile.FileName,
                            FileSize = postedfile.ContentLength,
                            ContentType = postedfile.ContentType,
                            Path = "/Uploads/Documents/" + filename
                        };
                        listFile.Add(file);
                    }
                }


                var response = new
                {
                    response_code = "0",
                    response_desc = "Thành công",
                    response_file = listFile

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

        [HttpPost]
        [Route("api/dvc/file_dinhkem")]
        public IHttpActionResult file_dinhkem()
        {
            try
            {
                //Xử lsy lưu file đính kèm
                bool exists = System.IO.Directory.Exists(HttpContext.Current.Server.MapPath("~/Uploads/Documents"));
                if (!exists)
                    System.IO.Directory.CreateDirectory(HttpContext.Current.Server.MapPath("~/Uploads/Documents"));
                System.Web.HttpFileCollection httpRequest = System.Web.HttpContext.Current.Request.Files;
                string filename = "";
                List<dynamic> listFile = new List<dynamic>();
                for (int i = 0; i <= httpRequest.Count - 1; i++)
                {
                    System.Web.HttpPostedFile postedfile = httpRequest[i];
                    if (postedfile.ContentLength > 0)
                    {
                        filename = DateTime.Now.Ticks.ToString() + "_" + postedfile.FileName.Replace(" ", "");
                        var fileSavePath = Path.Combine(HttpContext.Current.Server.MapPath("~/Uploads/Documents"), filename);
                        postedfile.SaveAs(fileSavePath);
                        var file = new
                        {
                            Name = filename,
                            FileSize = postedfile.ContentLength,
                            ContentType = postedfile.ContentType,
                            Path = "/Uploads/Documents/" + filename
                        };
                        listFile.Add(file);
                    }
                }


                var response = new
                {
                    response_code = "0",
                    response_desc = "Thành công",
                    response_file = listFile

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



        [HttpPost]
        [Route("api/dvc/nhan_ho_so")]
        public IHttpActionResult nhan_ho_so(HOSO hoso)
        {
            try
            {
                //Lưu thông tin hồ sơ
                db.HO_SO_ONLINE.Add(hoso.HoSoOnline);
                db.HO_SO_ONLINE_THUTUC.AddRange(hoso.HoSoOnlineThuTuc);
                db.DINH_KEM_ONLINE.AddRange(hoso.DinhKemOnline);
                db.HO_SO_TUYENSINH.Add(hoso.HoSoTuyenSinh);
                db.KET_QUA_HOC_TAP.Add(hoso.KetQuaHocTap);
                List<NGUYEN_VONG> listNV = new List<NGUYEN_VONG>();
                NGUYEN_VONG ngv = new NGUYEN_VONG();
                if (hoso.NguyenVong.Count() == 2)
                {
                    string LoaiTruong = db.Organizations.Where(x => x.FCode == hoso.HoSoOnline.MaDonVi).Select(y => y.Loai).FirstOrDefault();
                    if (!String.IsNullOrEmpty(hoso.NguyenVong[1].MaTruong) && LoaiTruong != "CHUYEN")
                    {
                        HOSO hoso2 = hoso.Clone();
                        string MaDonViNV2 = hoso.NguyenVong[1].MaTruong;
                        string TenTruong = db.Organizations.Where(x => x.FCode == MaDonViNV2).Select(y => y.FName).FirstOrDefault();
                        string FCodeNV2 = get_auto_id(MaDonViNV2);
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

                var response = new
                {
                    response_code = "0",
                    response_desc = "Thành công",
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

        //Lấy danh sách hồ sơ theo mã khách hàng
        [HttpGet]
        [Route("api/dvc/lay_ds_ho_so")]
        public IHttpActionResult lay_ds_ho_so(string MaKhachHang, string Status, DateTime? TuNgay, DateTime? DenNgay)
        {
            try
            {
                var HS = db.HO_SO_ONLINE.Where(x => x.MaKhachHang == MaKhachHang && (x.TrangThaiHS == Status || Status == null || Status == "")
                                                    && (x.FCreateTime >= TuNgay || TuNgay == null) && (x.FCreateTime <= DenNgay || DenNgay == null)).ToList();
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
        [Route("api/dvc/lay_ho_so")]
        public IHttpActionResult lay_ho_so(string Code)
        {
            try
            {
                var HS_OL = db.HO_SO_ONLINE.Where(x => x.FCode == Code).FirstOrDefault();
                var HS_TT = db.HO_SO_ONLINE_THUTUC.Where(x => x.MaHoSo == Code).ToList();
                var HS_DK = db.DINH_KEM_ONLINE.Where(x => x.MAHOSO == Code).ToList();
                var HS_TS = db.HO_SO_TUYENSINH.Where(x => x.MaHoSo == Code).FirstOrDefault();
                var KQHT = db.KET_QUA_HOC_TAP.Where(x => x.MaHoSo == Code).FirstOrDefault();
                List<NguyenVong> listNV = new List<NguyenVong>();
                var NGV = db.NGUYEN_VONG.Where(x => x.MaHoSo == Code).FirstOrDefault();
                var NGV2 = db.NGUYEN_VONG.Where(x => x.MaHoSoNV1 == Code).FirstOrDefault();
                if(NGV != null)
                {
                    NguyenVong nguyenVong = NGV.MonHoc != null ? JsonConvert.DeserializeObject<NguyenVong>(NGV.MonHoc) : null;
                    if (nguyenVong != null) listNV.Add(nguyenVong);
                }
                if(NGV2 != null)
                {
                    NguyenVong nguyenVong2 = NGV2.MonHoc != null ? JsonConvert.DeserializeObject<NguyenVong>(NGV2.MonHoc) : null;
                    if (nguyenVong2 != null) listNV.Add(nguyenVong2);
                }

                var HS = new HOSO()
                {
                    HoSoOnline = HS_OL,
                    HoSoTuyenSinh = HS_TS,
                    HoSoOnlineThuTuc = HS_TT,
                    DinhKemOnline = HS_DK,
                    KetQuaHocTap = KQHT,
                    NguyenVong = listNV
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

        //Đếm số lượng hồ sơ theo mã khách hàng
        [HttpGet]
        [Route("api/dvc/dem_ho_so")]
        public IHttpActionResult dem_ho_so(string MaKhachHang)
        {
            try
            {
                var HS = db.HO_SO_ONLINE.Where(x => x.MaKhachHang == MaKhachHang).Count();
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
                    response_data = 0
                };
                return Ok(response);
            }
        }

        [HttpGet]
        [Route("api/dvc/get_auto_id")]
        public IHttpActionResult api_get_auto_id(string MaDonVi)
        {
            try
            {
                var DonVi = db.Organizations.Where(x => x.FCode == MaDonVi).Select(y => y.FBranchCode).FirstOrDefault();
                if (String.IsNullOrEmpty(DonVi))
                {
                    var response = new
                    {
                        response_code = "02",
                        response_desc = "NotFound",
                        response_data = "Đơn vị chưa có mã định danh"
                    };
                    return Ok(response);
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
                    var response = new
                    {
                        response_code = "00",
                        response_desc = "success",
                        response_data = AutoId.FName
                    };
                    return Ok(response);
                }

            }
            catch (System.Exception ex)
            {
                var response = new
                {
                    response_code = "01",
                    response_desc = "failed",
                    response_data = ex.ToString()
                };
                return Ok(response);
            }
        }
        public string get_auto_id(string MaDonVi)
        {
            try
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
                    return AutoId.FName;
                }

            }
            catch (System.Exception ex)
            {
                ex.ToString();
                return null;
            }
        }
        [HttpPost]
        [Route("api/org/import")]
        public IHttpActionResult ImportOrg(List<ImportOrganization> list)
        {
            using (var dbContextTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    var listorg = db.Organizations.ToList();
                    var ListError = new List<string>();
                    foreach (var obj in list)
                    {
                        try
                        {
                            var org = listorg.Where(x => x.FCode == obj.FCode).FirstOrDefault();
                            if (org != null)
                            {
                                org.FBranchCode = obj.FBranchCode;
                                db.Organizations.AddOrUpdate(org);
                            }
                        }
                        catch
                        {
                            ListError.Add(obj.FCode);
                        }
                    }
                    //db.ChangeTracker.DetectChanges();
                    db.SaveChanges();

                    dbContextTransaction.Commit();

                    var data = new
                    {
                        ThanhCong = list.Count - ListError.Count,
                        Loi = ListError.Count,
                        List = ListError
                    };

                    var result = new
                    {
                        response_code = "00",
                        response_data = data
                    };
                    return Ok(result);
                }

                catch (Exception ex)
                {
                    dbContextTransaction.Rollback();
                    var result = new
                    {
                        response_code = "-01",
                        response_data = ex.ToString()
                    };
                    return Ok(result);
                }
            }
        }

        [HttpGet]
        [Route("api/settings/organization")]
        public IHttpActionResult OrgByFCode(string FCode)
        {
            try
            {
                var collMenu = db.Organizations.Where(x => x.FCode == FCode).FirstOrDefault();
                return Ok(collMenu);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ToString());
            }
        }

        [HttpGet]
        [Route("api/dvc/CreateNguyenVong")]
        public IHttpActionResult CreateNguyenVong(string MaDonVi)
        {
            try
            {
                List<NhomMonHoc> dsNhomMonHoc = new List<NhomMonHoc>();
                var item = db.NHOM_MON_HOC.Where(x => x.MaDonVi == MaDonVi || x.MaDonVi == null || x.MaDonVi == "").ToList();
                var itemMonHoc = db.MON_HOC.ToList();
                foreach(var n in item)
                {
                    List<string> ListMonHoc = JsonConvert.DeserializeObject<List<string>>(n.MonHoc);
                    List<MonHoc> dsMonHoc = new List<MonHoc>();
                    foreach(var mh in ListMonHoc)
                    {
                        dsMonHoc.Add(new MonHoc()
                        {
                            FCode = mh,
                            FName = itemMonHoc.Where(x => x.FCode == mh).Select(y => y.FName).FirstOrDefault(),
                            Check = false,
                            NhomMonHoc = n.FCode
                        });
                    };
                    dsNhomMonHoc.Add(new NhomMonHoc()
                    {
                        FCode = n.FCode,
                        FName = n.FName,
                        MonHoc = dsMonHoc
                    });
                }
                return Ok(dsNhomMonHoc);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ToString());
            }
        }

        [HttpGet]
        [Route("api/dvc/CheckSoDinhDanh")]
        public IHttpActionResult CheckSoDinhDanh(string SoDinhDanh, string NamHoc)
        {
            try
            {
                var item = db.HO_SO_TUYENSINH.Where(x => x.SoCCCDHocSinh == SoDinhDanh && x.NamHoc == NamHoc && x.LoaiXetTuyen == "THPT").ToList();
                int TonTai = 0;
                if(item.Count() > 0)
                {
                    TonTai = 1;
                }
                var response = new
                {
                    response_code = "0",
                    response_desc = "Thành công",
                    response_data = TonTai
                };
                return Ok(response);
            }
            catch (Exception ex)
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

        [HttpPost]
        [Route("api/dvc/UpdateMaKhachHang")]
        public IHttpActionResult UpdateMaKhachHang(string MaHoSo, string MaKhachHang)
        {
            try
            {
                var HSOL = db.HO_SO_ONLINE.Where(x => x.FCode == MaHoSo).FirstOrDefault();
                HSOL.MaKhachHang = MaKhachHang;
                db.SaveChanges();
                var response = new
                {
                    response_code = "0",
                    response_desc = "Thành công",
                    response_data = ""
                };
                return Ok(response);
            }
            catch (Exception ex)
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
        
        public class ImportOrganization
        {
            public string FCode { get; set; }
            public string FBranchCode { get; set; }
        }

    }

    public class NguyenVong
    {
        public string NV { get; set; }
        public string MaTruong { get; set; }
        public List<NhomMonHoc> NhomMonHoc { get; set; }
    }
    public class NhomMonHoc
    {
        public string FCode { get; set; }
        public string FName { get; set; }
        public List<MonHoc> MonHoc { get; set; }
    }
    public class MonHoc
    {
        public string FCode { get; set; }
        public string FName { get; set; }
        public bool Check { get; set; }
        public string NhomMonHoc { get; set; }
    }

    public class HOSO
    {
        public HO_SO_ONLINE HoSoOnline { get; set; }
        public HO_SO_TUYENSINH HoSoTuyenSinh { get; set; }
        public List<HO_SO_ONLINE_THUTUC> HoSoOnlineThuTuc { get; set; }
        public List<DINH_KEM_ONLINE> DinhKemOnline { get; set; }
        public KET_QUA_HOC_TAP KetQuaHocTap { get; set; }
        public List<NguyenVong> NguyenVong { get; set; }
    }
    //public class HOSOreturn
    //{
    //    public HO_SO_ONLINE HoSoOnline { get; set; }
    //    public HO_SO_TUYENSINH HoSoTuyenSinh { get; set; }
    //    public List<HO_SO_ONLINE_THUTUC> HoSoOnlineThuTuc { get; set; }
    //    public List<DINH_KEM_ONLINE> DinhKemOnline { get; set; }
    //    public KET_QUA_HOC_TAP KetQuaHocTap { get; set; }
    //    public List<NhomMonHoc> NguyenVong { get; set; }
    //}
    public static class CloningService
    {
        public static T Clone<T>(this T source)
        {
            // Don't serialize a null object, simply return the default for that object
            if (Object.ReferenceEquals(source, null))
            {
                return default(T);
            }
            var deserializeSettings = new JsonSerializerSettings { ObjectCreationHandling = ObjectCreationHandling.Replace };
            var serializeSettings = new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore };
            return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(source, serializeSettings), deserializeSettings);
        }
    }
}