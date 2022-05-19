﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace VSDCompany.Models
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Core.Objects;
    using System.Linq;
    
    public partial class Entities : DbContext
    {
        public Entities()
            : base("name=Entities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<Menu> Menus { get; set; }
        public virtual DbSet<Area> Areas { get; set; }
        public virtual DbSet<Nation> Nations { get; set; }
        public virtual DbSet<Nationality> Nationalities { get; set; }
        public virtual DbSet<Processing_Status> Processing_Status { get; set; }
        public virtual DbSet<Role> Roles { get; set; }
        public virtual DbSet<AutoID> AutoIDs { get; set; }
        public virtual DbSet<Position> Positions { get; set; }
        public virtual DbSet<Group_Role> Group_Role { get; set; }
        public virtual DbSet<Group_User> Group_User { get; set; }
        public virtual DbSet<Role_Assignment> Role_Assignment { get; set; }
        public virtual DbSet<DINH_KEM_ONLINE> DINH_KEM_ONLINE { get; set; }
        public virtual DbSet<HO_SO_ONLINE_THUTUC> HO_SO_ONLINE_THUTUC { get; set; }
        public virtual DbSet<Group_Menu> Group_Menu { get; set; }
        public virtual DbSet<Group> Groups { get; set; }
        public virtual DbSet<Organization_Type> Organization_Type { get; set; }
        public virtual DbSet<UserProfile> UserProfiles { get; set; }
        public virtual DbSet<KET_QUA_HOC_TAP> KET_QUA_HOC_TAP { get; set; }
        public virtual DbSet<MON_HOC> MON_HOC { get; set; }
        public virtual DbSet<Organization> Organizations { get; set; }
        public virtual DbSet<NHOM_MON_HOC> NHOM_MON_HOC { get; set; }
        public virtual DbSet<HO_SO_ONLINE> HO_SO_ONLINE { get; set; }
        public virtual DbSet<NGUYEN_VONG> NGUYEN_VONG { get; set; }
        public virtual DbSet<THOI_GIAN_TS> THOI_GIAN_TS { get; set; }
        public virtual DbSet<HO_SO_TUYENSINH> HO_SO_TUYENSINH { get; set; }
        public virtual DbSet<CMS_News> CMS_News { get; set; }
        public virtual DbSet<TAG> TAGS { get; set; }
        public virtual DbSet<Group_Tags> Group_Tags { get; set; }
    
        public virtual ObjectResult<Get_List_Admissions_Result> Get_List_Admissions(string maDonVi, string status, string searchKey, string namHoc, Nullable<System.DateTime> tungay, Nullable<System.DateTime> denNgay, Nullable<bool> trungTuyen)
        {
            var maDonViParameter = maDonVi != null ?
                new ObjectParameter("MaDonVi", maDonVi) :
                new ObjectParameter("MaDonVi", typeof(string));
    
            var statusParameter = status != null ?
                new ObjectParameter("Status", status) :
                new ObjectParameter("Status", typeof(string));
    
            var searchKeyParameter = searchKey != null ?
                new ObjectParameter("SearchKey", searchKey) :
                new ObjectParameter("SearchKey", typeof(string));
    
            var namHocParameter = namHoc != null ?
                new ObjectParameter("NamHoc", namHoc) :
                new ObjectParameter("NamHoc", typeof(string));
    
            var tungayParameter = tungay.HasValue ?
                new ObjectParameter("Tungay", tungay) :
                new ObjectParameter("Tungay", typeof(System.DateTime));
    
            var denNgayParameter = denNgay.HasValue ?
                new ObjectParameter("DenNgay", denNgay) :
                new ObjectParameter("DenNgay", typeof(System.DateTime));
    
            var trungTuyenParameter = trungTuyen.HasValue ?
                new ObjectParameter("TrungTuyen", trungTuyen) :
                new ObjectParameter("TrungTuyen", typeof(bool));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<Get_List_Admissions_Result>("Get_List_Admissions", maDonViParameter, statusParameter, searchKeyParameter, namHocParameter, tungayParameter, denNgayParameter, trungTuyenParameter);
        }
    
        public virtual ObjectResult<Get_YearChart_Result> Get_YearChart(string maDonVi, string namHoc)
        {
            var maDonViParameter = maDonVi != null ?
                new ObjectParameter("MaDonVi", maDonVi) :
                new ObjectParameter("MaDonVi", typeof(string));
    
            var namHocParameter = namHoc != null ?
                new ObjectParameter("NamHoc", namHoc) :
                new ObjectParameter("NamHoc", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<Get_YearChart_Result>("Get_YearChart", maDonViParameter, namHocParameter);
        }
    
        public virtual ObjectResult<Count_HS_Result> Count_HS(string maDonVi, string namHoc)
        {
            var maDonViParameter = maDonVi != null ?
                new ObjectParameter("MaDonVi", maDonVi) :
                new ObjectParameter("MaDonVi", typeof(string));
    
            var namHocParameter = namHoc != null ?
                new ObjectParameter("NamHoc", namHoc) :
                new ObjectParameter("NamHoc", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<Count_HS_Result>("Count_HS", maDonViParameter, namHocParameter);
        }
    
        public virtual ObjectResult<Get_DS_TrungTuyen_Result> Get_DS_TrungTuyen(string maDonVi, string searchKey, string namHoc)
        {
            var maDonViParameter = maDonVi != null ?
                new ObjectParameter("MaDonVi", maDonVi) :
                new ObjectParameter("MaDonVi", typeof(string));
    
            var searchKeyParameter = searchKey != null ?
                new ObjectParameter("SearchKey", searchKey) :
                new ObjectParameter("SearchKey", typeof(string));
    
            var namHocParameter = namHoc != null ?
                new ObjectParameter("NamHoc", namHoc) :
                new ObjectParameter("NamHoc", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<Get_DS_TrungTuyen_Result>("Get_DS_TrungTuyen", maDonViParameter, searchKeyParameter, namHocParameter);
        }
    
        public virtual ObjectResult<Export_DS_TrungTuyen_Result> Export_DS_TrungTuyen(string maDonVi, string searchKey, string namHoc)
        {
            var maDonViParameter = maDonVi != null ?
                new ObjectParameter("MaDonVi", maDonVi) :
                new ObjectParameter("MaDonVi", typeof(string));
    
            var searchKeyParameter = searchKey != null ?
                new ObjectParameter("SearchKey", searchKey) :
                new ObjectParameter("SearchKey", typeof(string));
    
            var namHocParameter = namHoc != null ?
                new ObjectParameter("NamHoc", namHoc) :
                new ObjectParameter("NamHoc", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<Export_DS_TrungTuyen_Result>("Export_DS_TrungTuyen", maDonViParameter, searchKeyParameter, namHocParameter);
        }
    
        public virtual ObjectResult<Count_HS_2_Result> Count_HS_2(string maDonVi, string namHoc)
        {
            var maDonViParameter = maDonVi != null ?
                new ObjectParameter("MaDonVi", maDonVi) :
                new ObjectParameter("MaDonVi", typeof(string));
    
            var namHocParameter = namHoc != null ?
                new ObjectParameter("NamHoc", namHoc) :
                new ObjectParameter("NamHoc", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<Count_HS_2_Result>("Count_HS_2", maDonViParameter, namHocParameter);
        }
    
        public virtual ObjectResult<Get_List_HoSo_Result> Get_List_HoSo(string maDonVi, string status, string searchKey)
        {
            var maDonViParameter = maDonVi != null ?
                new ObjectParameter("MaDonVi", maDonVi) :
                new ObjectParameter("MaDonVi", typeof(string));
    
            var statusParameter = status != null ?
                new ObjectParameter("Status", status) :
                new ObjectParameter("Status", typeof(string));
    
            var searchKeyParameter = searchKey != null ?
                new ObjectParameter("SearchKey", searchKey) :
                new ObjectParameter("SearchKey", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<Get_List_HoSo_Result>("Get_List_HoSo", maDonViParameter, statusParameter, searchKeyParameter);
        }
    
        public virtual ObjectResult<Get_List_Result_Result> Get_List_Result(string maDonVi, string status, string searchKey)
        {
            var maDonViParameter = maDonVi != null ?
                new ObjectParameter("MaDonVi", maDonVi) :
                new ObjectParameter("MaDonVi", typeof(string));
    
            var statusParameter = status != null ?
                new ObjectParameter("Status", status) :
                new ObjectParameter("Status", typeof(string));
    
            var searchKeyParameter = searchKey != null ?
                new ObjectParameter("SearchKey", searchKey) :
                new ObjectParameter("SearchKey", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<Get_List_Result_Result>("Get_List_Result", maDonViParameter, statusParameter, searchKeyParameter);
        }
    }
}