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
            return View();
        }
        [HttpPost]
        public ActionResult Index(FormCollection post)
        {
            USERINFO USER = new USERINFO()
            {
                ID = post["ACCOUNT"],
                PASSWORD = post["PASSWORD"]
            };
            MySqlConnection conn = new MySqlConnection(connStr);
            MySqlCommand command = conn.CreateCommand();
            command.CommandText = $"select * from userinfo where ID='{USER.ID}' and PASSWORD='{USER.PASSWORD}' AND (STATUS=1 or STATUS is null)";
            DataTable DT = new DataTable();
            MySqlDataAdapter MDA = new MySqlDataAdapter(command.CommandText, conn);
            MDA = new MySqlDataAdapter(command.CommandText, conn);
            MDA.Fill(DT);
            if (DT != null && DT.Rows.Count > 0)
            {
                Session["USERID"] = DT.Rows[0]["USERID"].ToString();
                return RedirectToAction("UserList", "Home", null);
            }
            else
            {
                TempData["alert"] = "帳號或密碼錯誤!";
                return View();
            }
        }
        [HttpGet]
        public ActionResult UserList(string CARDID)
        {
            List<USERINFO> _userinfo = new List<USERINFO>();
            MySqlConnection conn = new MySqlConnection(connStr);
            MySqlCommand command = conn.CreateCommand();
            command.CommandText = $"select CARDID,USERNAME,ID,BIRTHDAY,GENDER from cardlist C left join userinfo U on C.USERID=U.USERID where (STATUS=1 or STATUS is null) {(string.IsNullOrEmpty(CARDID) ? "" : $"and CARDID='{CARDID}'")}";
            DataTable DT = new DataTable();
            MySqlDataAdapter MDA = new MySqlDataAdapter(command.CommandText, conn);
            MDA = new MySqlDataAdapter(command.CommandText, conn);
            MDA.Fill(DT);
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
            GetData(CARDID, SDATE, EDATE);
            ViewBag.CARDID = CARDID;
            return View();
        }

        private void GetData(string CARDID, DateTime? SDATE, DateTime? EDATE)
        {
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
            MySqlConnection conn = new MySqlConnection(connStr);
            MySqlCommand command = conn.CreateCommand();
            command.CommandText = $"select * from cardlist where cardid='{CARDID}'";
            DataTable DT = new DataTable();
            MySqlDataAdapter MDA = new MySqlDataAdapter(command.CommandText, conn);
            MDA.Fill(DT);
            if (DT != null && DT.Rows.Count > 0)
            {
                command.CommandText = $"select * from userinfo where userid='{DT.Rows[0]["userid"].ToString()}' AND (STATUS=1 or STATUS is null)";
            }
            else
            {
                command.CommandText = $"select * from userinfo where id='{CARDID}' AND (STATUS=1 or STATUS is null)";
            }
            DT = new DataTable();
            MDA = new MySqlDataAdapter(command.CommandText, conn);
            MDA.Fill(DT);
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
            DataTable DTdetail = new DataTable();
            //BLOODPRESSURE
            command.CommandText = $"select DATE_FORMAT(cdate, '%Y/%m/%d %H點')CDATE,DBP,SBP,HB,PU from BLOODPRESSURE where userid='{ _userinfo.USERID }' {DATE};";
            MDA = new MySqlDataAdapter(command.CommandText, conn);
            MDA.Fill(DTdetail);
            var strDATA = "";
            foreach (DataRow row in DTdetail.Rows)
            {
                BLOODPRESSURE BP = new BLOODPRESSURE();
                BP.CDATE = row["CDATE"].ToString();
                BP.DBP = row["DBP"].ToString();
                BP.SBP = row["SBP"].ToString();
                BP.HB = row["HB"].ToString();
                BP.PU = row["PU"].ToString();
                strDATA += $"['{row["CDATE"].ToString()}',{row["DBP"].ToString()},{row["SBP"].ToString()},{(row["HB"].ToString() == "" ? "0" : row["HB"].ToString())},{(row["PU"].ToString() == "" ? "0" : row["PU"].ToString())}],";
                _bloodperssure.Add(BP);
            }
            if (strDATA.Length > 0) strDATA = strDATA.Remove(strDATA.Length - 1);
            ViewBag.strBP = strDATA;
            ViewData["summary"] = strDATA;
            ViewBag.BLOODPRESSURE = _bloodperssure;
            //BLOODSUGAR
            DTdetail = new DataTable();
            command.CommandText = $"select DATE_FORMAT(cdate, '%Y/%m/%d %H點')CDATE,BS from BLOODSUGAR where userid='{ _userinfo.USERID }' {DATE};";
            MDA = new MySqlDataAdapter(command.CommandText, conn);
            MDA.Fill(DTdetail);
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
            DTdetail = new DataTable();
            command.CommandText = $"select DATE_FORMAT(cdate, '%Y/%m/%d %H點')CDATE,BT from BODYTEMP where userid='{ _userinfo.USERID }' {DATE};";
            MDA = new MySqlDataAdapter(command.CommandText, conn);
            MDA.Fill(DTdetail);
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
            DTdetail = new DataTable();
            command.CommandText = $"select DATE_FORMAT(cdate, '%Y/%m/%d %H點')CDATE,BO from BLOODOXYGEN where userid='{ _userinfo.USERID }' {DATE};";
            MDA = new MySqlDataAdapter(command.CommandText, conn);
            MDA.Fill(DTdetail);
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
        }
    }
}