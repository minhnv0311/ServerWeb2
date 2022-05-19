using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Web;
using VSDCompany.Models;

namespace VSDCompany.Controllers
{
    //https://dev.azure.com/tvdung/VSDCompany

    public class tree_org
    {
        public string FName { get; set; }
        public string FCode { get; set; }
        public long Id { get; set; }
        public int FIndex { get; set; }
        public string Icon { get; set; }
        public string orgTypeFName { get; set; }
        public string FParent { get; set; }
       
        
    }
    public class Commons
    {
        public static AutoID GenerateID(Entities db, string Code)
        {
            Code = Code.ToUpper();
            AutoID AutoId = db.AutoIDs.Where(x => x.FCode == Code).SingleOrDefault();
            if (AutoId == null)
            {
                AutoId = new AutoID();
                AutoId.FCode = Code;
                AutoId.Counter = 1;
                db.AutoIDs.Add(AutoId);
            }
            AutoId.FName = Code;
            for (int i = 0; i < 8 - AutoId.Counter.ToString().Length; i++)
                AutoId.FName += 0;
            AutoId.FName += AutoId.Counter.ToString();
            AutoId.Counter += 1;
            db.SaveChanges();
            return AutoId;
        }

        public static Regex CreateValidEmailRegex()
        {
            string validEmailPattern = @"^(?!\.)(""([^""\r\\]|\\[""\r\\])*""|"
                + @"([-a-z0-9!#$%&'*+/=?^_`{|}~]|(?<!\.)\.)*)(?<!\.)"
                + @"@[a-z0-9][\w\.-]*[a-z0-9]\.[a-z][a-z\.]*[a-z]$";

            return new Regex(validEmailPattern, RegexOptions.IgnoreCase);
        }

        public static string ConvertMobile(string mobile)
        {
            mobile = mobile.Replace(" ", "");
            mobile = mobile.Replace(".", "");
            mobile = mobile.Replace("-", "");
            if (mobile.StartsWith("+84"))
                mobile = mobile.Replace("+", "");
            if (mobile.StartsWith("0"))
                mobile = "84" + mobile.Substring(1, mobile.Length - 1);
            return mobile;
        }
    }
}