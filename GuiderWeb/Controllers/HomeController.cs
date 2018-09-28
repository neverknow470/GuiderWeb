using GuiderWeb.Models;
using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Web.Mvc;

namespace GuiderWeb.Controllers
{
    public class HomeController : Controller
    {
        string connStr = "server=localhost;uid=root;pwd=1234;database=guiderapidb";
        //login
        public ActionResult Index()
        {
            Session["USERID"] = string.Empty;
            Session["LEVEL"] = string.Empty;
            Session["AREAID"] = string.Empty;
            return View();
        }
        [HttpPost]
        public ActionResult Index(FormCollection post)
        {
            Session["USERID"] = string.Empty;
            Session["LEVEL"] = string.Empty;
            Session["AREAID"] = string.Empty;
            USERINFO USER = new USERINFO()
            {
                ID = post["ACCOUNT"],
                PASSWORD = post["PASSWORD"]
            };
            if (USER.ID != string.Empty && USER.PASSWORD != string.Empty)
            {
                string sql = $"select * from userinfo where ID='{USER.ID}' and PASSWORD='{USER.PASSWORD}' AND (STATUS=1 or STATUS is null)";
                DataTable DT = CommonTools.GetDataTable(sql, connStr);
                if (DT != null && DT.Rows.Count > 0)
                {
                    Session["USERID"] = DT.Rows[0]["USERID"].ToString();
                    Session["LEVEL"] = DT.Rows[0]["LEVEL"].ToString();
                    Session["AREAID"] = DT.Rows[0]["AREAID"].ToString();
                    return RedirectToAction("UserList", "Home", null);
                }
                else
                {
                    TempData["message"] = "帳號或密碼錯誤!";

                }
            }
            else { TempData["message"] = "請輸入帳號密碼!"; }
            return View();
        }
        [HttpGet]
        public ActionResult UserList(string CARDID)
        {
            if (Session["LEVEL"] == null) return RedirectToAction("Index");
            if (Session["LEVEL"] != null && Session["LEVEL"].ToString() == "1") CARDID = Session["USERID"].ToString();
            string AREAID = Session["LEVEL"].ToString() == "6" ? Session["AREAID"].ToString() : string.Empty;
            List<USERINFO> _userinfo = new List<USERINFO>();
            string sql = $@"select CARDID,USERNAME,ID,BIRTHDAY,GENDER from userinfo U 
            left join(select min(CARDID) CARDID, USERID from cardlist where USERID!= '' group by USERID) C
            on C.USERID = U.USERID where (STATUS = 1 or STATUS is null) and C.CARDID!='' { (string.IsNullOrEmpty(CARDID) ? "" : $"and (C.CARDID='{CARDID}' OR U.ID='{CARDID}')")} { (string.IsNullOrEmpty(AREAID) ? "" : $"and U.AREAID='{AREAID}'")}
            union all select CARDID,''USERNAME,''ID,''BIRTHDAY,''GENDER from cardlist where (USERID= '' or USERID is null) { (string.IsNullOrEmpty(CARDID) ? "" : $"and USERID='{CARDID}'")}";
            DataTable DT = CommonTools.GetDataTable(sql, connStr);
            foreach (DataRow row in DT.Rows)
            {
                USERINFO USER = new USERINFO();
                USER.USERID = row["CARDID"].ToString();
                //USER.USERID = $"<a href='UserInfo?CARDID={row["CARDID"].ToString()}&SDATE=&EDATE='>{row["CARDID"].ToString()}</a>";
                USER.USERNAME = row["USERNAME"].ToString() == "" ? "使用者" : row["USERNAME"].ToString();
                USER.ID = row["ID"].ToString();
                USER.BIRTHDAY = row["BIRTHDAY"].ToString();
                USER.GENDER = row["GENDER"].ToString() == "F" ? "女" : "男";
                _userinfo.Add(USER);
            }
            ViewBag.USERINFO = _userinfo;
            return View();
        }

        [HttpGet]
        public ActionResult UserInfo(string CARDID, DateTime? SDATE, DateTime? EDATE)
        {
            if (Session["LEVEL"] == null) return RedirectToAction("Index");
            GetData(CARDID, SDATE, EDATE);
            ViewBag.CARDID = CARDID;
            return View();
        }

        private void GetData(string CARDID, DateTime? SDATE, DateTime? EDATE)
        {
            if (Session["LEVEL"] != null && Session["LEVEL"].ToString() == "1") CARDID = Session["USERID"].ToString();
            USERINFO _userinfo = new USERINFO()
            {
                USERID = "",
                USERNAME = "使用者",
                ID = "",
                BIRTHDAY = "",
                GENDER = "",
                PHONE = ""
            };
            ViewBag.USERINFO = _userinfo;
            List<BLOODPRESSURE> _bloodperssure = new List<BLOODPRESSURE>();
            List<BLOODSUGAR> _bloodsugar = new List<BLOODSUGAR>();
            List<BODYTEMP> _bodytemp = new List<BODYTEMP>();
            List<BLOODOXYGEN> _bloodoxygen = new List<BLOODOXYGEN>();
            List<BASICINFO> _basicinfo = new List<BASICINFO>();
            string sql = $"select * from cardlist where cardid='{CARDID}'";
            DataTable DT = CommonTools.GetDataTable(sql, connStr);
            if (DT != null && DT.Rows.Count > 0)
            {
                sql = $"select * from userinfo where userid='{DT.Rows[0]["userid"].ToString()}' AND (STATUS=1 or STATUS is null)";
            }
            else
            {
                sql = $"select * from userinfo where id='{CARDID}' AND (STATUS=1 or STATUS is null)";
            }
            DT = CommonTools.GetDataTable(sql, connStr);
            if (DT != null && DT.Rows.Count > 0)
            {
                _userinfo.USERID = DT.Rows[0]["USERID"].ToString();
                _userinfo.USERNAME = DT.Rows[0]["USERNAME"].ToString();
                _userinfo.ID = DT.Rows[0]["ID"].ToString();
                _userinfo.BIRTHDAY = DT.Rows[0]["BIRTHDAY"].ToString();
                _userinfo.GENDER = DT.Rows[0]["GENDER"].ToString() == "F" ? "女" : "男";
                ViewBag.USERINFO = _userinfo;
                if (_userinfo.USERID == "") _userinfo.USERID = CARDID;
            }
            else { _userinfo.USERID = CARDID; }
            var DATE = "";
            if (SDATE != null && EDATE != null)
            {
                DATE = $"and cdate>= '{SDATE.Value.ToString("yyyy-MM-dd")}' and cdate<= '{EDATE.Value.ToString("yyyy-MM-dd")} 23:59:59'";
                ViewBag.SDATE = SDATE.Value.ToString("yyyy-MM-dd");
                ViewBag.EDATE = EDATE.Value.ToString("yyyy-MM-dd");
            }
            //BLOODPRESSURE
            sql = $"select DATE_FORMAT(cdate, '%Y/%m/%d %T')CDATE,DBP,SBP,HB,PU from BLOODPRESSURE where userid='{ _userinfo.USERID }' {DATE};";
            DataTable DTdetail = CommonTools.GetDataTable(sql, connStr);
            var strDATA = "";
            foreach (DataRow row in DTdetail.Rows)
            {
                BLOODPRESSURE BP = new BLOODPRESSURE();
                BP.CDATE = row["CDATE"].ToString();
                BP.DBP = row["DBP"].ToString();
                BP.SBP = row["SBP"].ToString();
                BP.HB = row["HB"].ToString();
                BP.PU = string.Empty;//row["PU"].ToString()
                strDATA += $"['{row["CDATE"].ToString()}',{row["DBP"].ToString()},{row["SBP"].ToString()},{(row["HB"].ToString() == "" ? "0" : row["HB"].ToString())}],";//,{(row["PU"].ToString() == "" ? "0" : row["PU"].ToString())}
                _bloodperssure.Add(BP);
            }
            if (strDATA.Length > 0) strDATA = strDATA.Remove(strDATA.Length - 1);
            ViewBag.strBP = strDATA;
            ViewData["summary"] = strDATA;
            ViewBag.BLOODPRESSURE = _bloodperssure;
            //BLOODSUGAR
            sql = $"select DATE_FORMAT(cdate, '%Y/%m/%d %T')CDATE,BS from BLOODSUGAR where userid='{ _userinfo.USERID }' {DATE};";
            DTdetail = CommonTools.GetDataTable(sql, connStr);
            strDATA = "";
            foreach (DataRow row in DTdetail.Rows)
            {
                BLOODSUGAR BS = new BLOODSUGAR();
                BS.CDATE = row["CDATE"].ToString();
                BS.BS = row["BS"].ToString();
                strDATA += $"['{row["CDATE"].ToString()}',{row["BS"].ToString()}],";
                _bloodsugar.Add(BS);
            }
            if (strDATA.Length > 0) strDATA = strDATA.Remove(strDATA.Length - 1);
            ViewBag.strBS = strDATA;
            ViewBag.BLOODSUGAR = _bloodsugar;
            //BODYTEMP
            sql = $"select DATE_FORMAT(cdate, '%Y/%m/%d %T')CDATE,BT from BODYTEMP where userid='{ _userinfo.USERID }' {DATE};";
            DTdetail = CommonTools.GetDataTable(sql, connStr);
            strDATA = "";
            foreach (DataRow row in DTdetail.Rows)
            {
                BODYTEMP BT = new BODYTEMP();
                BT.CDATE = row["CDATE"].ToString();
                BT.BT = row["BT"].ToString();
                strDATA += $"['{row["CDATE"].ToString()}',{row["BT"].ToString()}],";
                _bodytemp.Add(BT);
            }
            if (strDATA.Length > 0) strDATA = strDATA.Remove(strDATA.Length - 1);
            ViewBag.strBT = strDATA;
            ViewBag.BODYTEMP = _bodytemp;

            //BLOODOXYGEN
            sql = $"select DATE_FORMAT(cdate, '%Y/%m/%d %T')CDATE,BO from BLOODOXYGEN where userid='{ _userinfo.USERID }' {DATE};";
            DTdetail = CommonTools.GetDataTable(sql, connStr);
            strDATA = "";
            foreach (DataRow row in DTdetail.Rows)
            {
                BLOODOXYGEN BO = new BLOODOXYGEN();
                BO.CDATE = row["CDATE"].ToString();
                BO.BO = row["BO"].ToString();
                strDATA += $"['{row["CDATE"].ToString()}',{row["BO"].ToString()}],";
                _bloodoxygen.Add(BO);
            }
            if (strDATA.Length > 0) strDATA = strDATA.Remove(strDATA.Length - 1);
            ViewBag.strBO = strDATA;
            ViewBag.BLOODOXYGEN = _bloodoxygen;

            //BASICINFO
            sql = $"select DATE_FORMAT(cdate, '%Y/%m/%d %T')CDATE,HEIGHT,WEIGHT,BMI from BASICINFO where userid='{ _userinfo.USERID }' {DATE};";
            DTdetail = CommonTools.GetDataTable(sql, connStr);
            strDATA = "";
            foreach (DataRow row in DTdetail.Rows)
            {
                BASICINFO BI = new BASICINFO();
                BI.CDATE = row["CDATE"].ToString();
                BI.HEIGHT = row["HEIGHT"].ToString();
                BI.WEIGHT = row["WEIGHT"].ToString();
                BI.BMI = row["BMI"].ToString();
                strDATA += $"['{row["CDATE"].ToString()}',{row["HEIGHT"].ToString()},{row["WEIGHT"].ToString()},{row["BMI"].ToString()}],";
                _basicinfo.Add(BI);
            }
            if (strDATA.Length > 0) strDATA = strDATA.Remove(strDATA.Length - 1);
            ViewBag.strBI = strDATA;
            ViewBag.BASICINFO = _basicinfo;
        }

        public ActionResult CardBelong()
        {
            if (Session["LEVEL"] == null) return RedirectToAction("Index");
            return View();
        }

        [HttpPost]
        public ActionResult CardBelong(string CARDID)
        {
            if (Session["LEVEL"] == null) return RedirectToAction("Index");
            try
            {
                string sql = $"SELECT * FROM CARDLIST WHERE CARDID='{CARDID}'";
                DataTable DT = CommonTools.GetDataTable(sql, connStr);
                if (DT != null && DT.Rows.Count > 0)
                {
                    if (DT.Rows[0]["USERID"].ToString() == "")
                    {
                        sql = "UPDATE CARDLIST SET USERID='" + Session["USERID"].ToString() + "' WHERE CARDID='" + CARDID + "'";
                        CommonTools.doExecuteNonQuery(sql, connStr);
                        sql = "UPDATE BASICINFO SET USERID='" + Session["USERID"].ToString() + "' WHERE USERID='" + CARDID + "'";
                        CommonTools.doExecuteNonQuery(sql, connStr);
                        sql = "UPDATE BLOODPRESSURE SET USERID='" + Session["USERID"].ToString() + "' WHERE USERID='" + CARDID + "'";
                        CommonTools.doExecuteNonQuery(sql, connStr);
                        sql = "UPDATE BLOODOXYGEN SET USERID='" + Session["USERID"].ToString() + "' WHERE USERID='" + CARDID + "'";
                        CommonTools.doExecuteNonQuery(sql, connStr);
                        sql = "UPDATE BLOODSUGAR SET USERID='" + Session["USERID"].ToString() + "' WHERE USERID='" + CARDID + "'";
                        CommonTools.doExecuteNonQuery(sql, connStr);
                        sql = "UPDATE BODYTEMP SET USERID='" + Session["USERID"].ToString() + "' WHERE USERID='" + CARDID + "'";
                        CommonTools.doExecuteNonQuery(sql, connStr);
                        TempData["message"] = "歸戶完成";
                    }
                    else
                    {
                        TempData["message"] = "失敗!此卡片已歸戶過";
                    }
                }
                else
                {
                    TempData["message"] = "失敗!此卡片不存在";
                }
            }
            catch (Exception EX)
            {
                TempData["message"] = "失敗!未知錯誤";
            }
            return View();
        }

        public ActionResult UserSetting()
        {
            if (Session["LEVEL"] == null) return RedirectToAction("Index");
            if (Session["LEVEL"].ToString() != "9") return RedirectToAction("UserList");
            SearchUserSetting(string.Empty);
            return View();
        }
        private void SearchUserSetting(string CARDID)
        {
            string AREAID = Session["LEVEL"].ToString() == "6" ? Session["AREAID"].ToString() : string.Empty;
            List<USERINFO> _userinfo = new List<USERINFO>();
            string sql = $@"select CARDID,USERNAME,ID,BIRTHDAY,GENDER,LEVEL,AREAID,STATUS from userinfo U 
            left join(select min(CARDID) CARDID, USERID from cardlist where USERID!= '' group by USERID) C
            on C.USERID = U.USERID where 1=1 { (string.IsNullOrEmpty(CARDID) ? "" : $"and (C.CARDID='{CARDID}' OR U.ID='{CARDID}')")} { (string.IsNullOrEmpty(AREAID) ? "" : $"and U.AREAID='{AREAID}'")}";
            DataTable DT = CommonTools.GetDataTable(sql, connStr);
            var _LEVEL = new List<SelectListItem>()
            {
                new SelectListItem {Text="一般使用者", Value="1" },
                new SelectListItem {Text="區域管理者", Value="6" },
                new SelectListItem {Text="超級管理者", Value="9" }
            };

            var _STATUS = new List<SelectListItem>()
            {
                new SelectListItem {Text="啟用", Value="1" },
                new SelectListItem {Text="停用", Value="0" }
            };

            foreach (DataRow row in DT.Rows)
            {
                USERINFO USER = new USERINFO();
                USER.USERID = row["CARDID"].ToString();
                //USER.USERID = $"<a href='UserInfo?CARDID={row["CARDID"].ToString()}&SDATE=&EDATE='>{row["CARDID"].ToString()}</a>";
                USER.USERNAME = row["USERNAME"].ToString() == "" ? "使用者" : row["USERNAME"].ToString();
                USER.ID = row["ID"].ToString();
                USER.BIRTHDAY = row["BIRTHDAY"].ToString();
                USER.GENDER = row["GENDER"].ToString() == "F" ? "女" : "男";
                USER.LEVEL = new SelectList(_LEVEL, "Value", "Text", row["LEVEL"]);
                USER.AREAID = new SelectList(GetAREALIST(), "Value", "Text", row["AREAID"]);
                USER.STATUS = new SelectList(_STATUS, "Value", "Text", row["STATUS"]);
                _userinfo.Add(USER);
            }
            ViewBag.USERINFO = _userinfo;
        }
        private List<SelectListItem> GetAREALIST()
        {
            string sql = "select * from arealist";
            DataTable DTarea = CommonTools.GetDataTable(sql, connStr);
            var _AREAID = new List<SelectListItem>();
            foreach (DataRow row in DTarea.Rows)
            {
                _AREAID.Add(new SelectListItem { Text = row["AREANAME"].ToString(), Value = row["AREAID"].ToString() });
            }
            return _AREAID;
        }

        [HttpPost]
        public ActionResult UserSetting(string TYPE, string CARDID, string[] SID, string[] LEVEL, string[] AREAID, string[] STATUS)
        {
            if (Session["LEVEL"] == null) return RedirectToAction("Index");
            if (Session["LEVEL"].ToString() != "9") return RedirectToAction("UserList");
            if (TYPE == "Search")
            {
                if (CARDID != string.Empty)
                {
                    SearchUserSetting(CARDID);
                    return View();
                }
                else
                {
                    return RedirectToAction("UserSetting");
                }

            }
            else
            {
                for (int i = 0; i < SID.Length; i++)
                {
                    string sql = $"UPDATE USERINFO SET LEVEL={ LEVEL[i] },AREAID='{ AREAID[i] }',STATUS={ STATUS[i] } WHERE ID='{ SID[i] }'";
                    CommonTools.doExecuteNonQuery(sql, connStr);
                }
                //SearchUserSetting(string.Empty);
                return RedirectToAction("UserSetting");
            }
        }

        public ActionResult NewUser()
        {
            if (Session["LEVEL"] == null) return RedirectToAction("Index");
            if (Session["LEVEL"].ToString() != "9") return RedirectToAction("UserList");
            ViewBag.AREALIST = new SelectList(GetAREALIST(), "Value", "Text");
            return View();
        }

        [HttpPost]
        public ActionResult NewUser(string TYPE, string ID, string PASSWORD, string USERNAME, string GENDER, string LEVEL, string AREAID)
        {
            if (Session["LEVEL"] == null) return RedirectToAction("Index");
            if (Session["LEVEL"].ToString() != "9") return RedirectToAction("UserList");
            try
            {
                ViewBag.AREALIST = new SelectList(GetAREALIST(), "Value", "Text");
                if (TYPE == "SAVE")
                {
                    string sql = $"SELECT * FROM USERINFO WHERE ID='{ID}'";
                    DataTable DT = CommonTools.GetDataTable(sql, connStr);
                    if (DT != null && DT.Rows.Count > 0)
                    {
                        sql = $"UPDATE USERINFO SET PASSWORD='{ID}',GENDER='{GENDER}',LEVEL='{LEVEL}',AREAID='{AREAID}' WHERE ID='{ID}'";
                        TempData["message"] = "修改完成";
                    }
                    else
                    {
                        sql = $@"INSERT INTO USERINFO (USERID,USERNAME,ID,PASSWORD,GENDER,CDATE,MDATE,STATUS,LEVEL,AREAID)
                        VALUES('{GetUSERID(LEVEL)}','{USERNAME}','{ID}','{PASSWORD}','{GENDER}',NOW(),NOW(),1,{LEVEL},'{AREAID}')";
                        TempData["message"] = "新增完成";
                    }
                    CommonTools.doExecuteNonQuery(sql, connStr);
                }
            }
            catch (Exception EX)
            {
                TempData["message"] = "新增失敗";
            }
            return View();
        }
        private string GetUSERID(string LEVEL)
        {
            switch (LEVEL)
            {
                case "6": LEVEL = "A"; break;
                case "9": LEVEL = "S"; break;
                default: LEVEL = "U"; break;
            }
            string USERID = $"{LEVEL}00000000000001";
            string sql = $"SELECT MAX(USERID) FROM USERINFO WHERE USERID LIKE '{LEVEL}%'";
            DataTable DT = CommonTools.GetDataTable(sql, connStr);
            if (DT != null && DT.Rows.Count > 0 && DT.Rows[0][0].ToString() != "")
            {
                int ID = int.Parse(DT.Rows[0][0].ToString().Replace(LEVEL, ""));
                USERID = LEVEL + (ID + 1).ToString().PadLeft(14, '0');
            }
            return USERID;
        }
    }
}