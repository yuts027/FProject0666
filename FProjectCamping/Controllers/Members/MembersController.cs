﻿
using FProjectCamping.Models.EFModels;
using FProjectCamping.Models.Infra;
using FProjectCamping.Models.Infra.FileValidators;
using FProjectCamping.Models.Respositories;
using FProjectCamping.Models.Services;
using FProjectCamping.Models.ViewModels.Members;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Security;
using System.Web.UI.WebControls;
using System.Xml;
using static System.Net.WebRequestMethods;
using Member = FProjectCamping.Models.EFModels.Member;

namespace FProjectCamping.Members.Controllers
{

    public class MembersController : Controller
    {
        private readonly MemberService _memberService = new MemberService();
        private readonly LoginService _loginService = new LoginService();
        private readonly ProfileService _profileService = new ProfileService();
        private readonly PasswordService _passwordService = new PasswordService();


        [Authorize]
        public ActionResult Index()
        {

            GetAndSetCurrentMember();

            return View();
        }
        // GET: Members
        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Register(RegisterVm vm)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                ViewBag.Errors = errors;
                return View(vm);
            }

            var urlTemeplate = Request.Url.Scheme + "://" + //生成http:.// 或https://
                              Request.Url.Authority +
                              "/Members/ActiveRegister?memberid={0}&confirmCode={1}";
            try
            {
                _memberService.RegisterMember(vm, urlTemeplate);
            }
            catch (Exception ex)
            {
                ViewBag.Errors = new List<string> { ex.Message }; //添加異常訊息
                return View(vm);
            }

            return View("RegisterConfirm");


        }

        public ActionResult ActiveRegister(int memberId, string confirmCode)
        {
            if (memberId <= 0 || string.IsNullOrEmpty(confirmCode))
            {
                return View();
            }

            var db = new AppDbContext();


            //根據memberId, confirmCode 取得Member
            var member = db.Members.FirstOrDefault(p => p.Id == memberId && p.ConfirmCode == confirmCode && p.IsConfirmed == false);
            if (member == null)
            {
                return View();
            }

            member.IsConfirmed = true;
            member.ConfirmCode = null;
            db.SaveChanges();

            return View();
        }

        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(LoginVm vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }
            try
            {
				_loginService.ValidLogin(vm);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(vm);
            }

            var processResult = _loginService.ProcessLogin(vm);

            Response.Cookies.Add(processResult.Cookie);
            return Redirect(processResult.ReturnUrl);

        }

        public ActionResult Logout()
        {
            Session.Abandon();
            FormsAuthentication.SignOut();
            return Redirect("/Home/Index/");

        }

        [Authorize]
        public ActionResult EditProfile()
        {
            var currentUserAccount = User.Identity.Name;
            var vm = _profileService.GetMemberProfile(currentUserAccount);

            GetAndSetCurrentMember();


            return View(vm);
        }

        [Authorize]
        [HttpPost]
        public ActionResult EditProfile(EditProfileVm vm)
        {
            var currentUserAccount = User.Identity.Name;
            if (!ModelState.IsValid)
            {
                return View(vm);
            }
            try
            {
                new ProfileService().UpdateProfile(vm, currentUserAccount);

            } catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
            return RedirectToAction("Index");

        }

        [Authorize]
        public ActionResult EditPassword()
        {

            GetAndSetCurrentMember();
            return View();
        }

        [Authorize]
        [HttpPost]
        public ActionResult EditPassword(EditPasswordVm vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }
            try
            {
                var currentUserAccount = User.Identity.Name;
                _passwordService.ChangePassword(vm, currentUserAccount);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(vm);
            }

            return RedirectToAction("Index");

        }

        public ActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ForgotPassword(ForgotPasswordVm vm)
        {

            if (!ModelState.IsValid) return View(vm);

            var urlTemeplate = Request.Url.Scheme + "://" + //生成http:.// 或https://
                              Request.Url.Authority +
                              "/Members/ResetPassword?memberid={0}&confirmCode={1}";
            try
            {
                _passwordService.ProcessResentPassword(vm.Account, vm.Email, urlTemeplate);
            } catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                 return View(vm);
            }
            return View("ForgotPasswordConfirm");
        }

        
        public ActionResult ResetPassword(int memberId,string confirmCode)
        {
            return View();
        }

        [HttpPost]
		public ActionResult ResetPassword(int memberId, string confirmCode, ResetPasswordVm vm)
		{
			//檢查是否有memberId, confirmCode
			//檢查 vm是否通過驗證

			if (!ModelState.IsValid) return View(vm);

			try
			{
                //檢查memberId, confirmCode是否正確
                //重設密碼
                //顯示重設密碼成功畫面
                _passwordService.ProcessResetPassword(memberId, confirmCode, vm);
			}
			catch (Exception ex)
			{
                ModelState.AddModelError("", ex.Message);
			}


			return View("ConfirmResetPassword");
		}

        [Authorize]
        public ActionResult MyOrders() // todo
        {
            GetAndSetCurrentMember();
            var orders = new OrderRepository().GetOrders("aliee");
            return View(orders);
        }
        [Authorize]
        public ActionResult EditPhoto()
        {
            var currentUserAccount = User.Identity.Name;
            var member = GetMember(currentUserAccount);
            return View(member);
        }


        [Authorize]
        [HttpPost]

        public ActionResult EditPhoto(string photo, HttpPostedFileBase myfile)
        {
            string fileName;
            string path = Server.MapPath("~/Files");
            IFileValidator[] validators =
            {
        new FileRequired(),
        new ImageValidator(),
        new FileSizeValidator(1024),
    };

            var currentUserAccount = User.Identity.Name;
            var member = GetMember(currentUserAccount); // 獲取會員資料

            try
            {
                fileName = UploadFileHelper.Save(myfile, path, validators);

                string sourceFullPath = Path.Combine(path, fileName);

                string dest = System.Configuration.ConfigurationManager.AppSettings["frontSiteRootPath"];
                string destFullPath = Path.Combine(dest, fileName);

                System.IO.File.Copy(sourceFullPath, destFullPath, true);

                // 將檔名存入 photo
                photo = fileName;

                //todo將紀錄存
                if (!ModelState.IsValid)
                {
                    return View();
                }

                EditPhoto(photo, currentUserAccount);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("myfile", ex.Message);
                // 在這裡處理文件上傳或處理錯誤，但不要影響 member 的值
                return View(member);
            }

            return RedirectToAction("Index");
        }




        private string EditPhoto(string photo, string account)
        {

            var db = new AppDbContext();
            var memberInDb = db.Members.FirstOrDefault(p => p.Account == account);
            if (memberInDb.Account != account)
            {
                throw new Exception("您沒有權限修改別人資料");
            }
           

            memberInDb.Photo = photo;
            db.SaveChanges();
            return photo;
        }



        //確認身分
        private void GetAndSetCurrentMember()
        {
            var currentUserAccount = User.Identity.Name;
            var member = GetMember(currentUserAccount);
            ViewBag.CurrentMember = member;
        }

        private Member GetMember(string account)
        {
            using (var db = new AppDbContext())
            {
                var memberInDb = db.Members.FirstOrDefault(p => p.Account == account);
                return memberInDb;
            }
        }


        //public ActionResult EditPhoto(string photo, HttpPostedFileBase myfile)
        //{

        //	string fileName;
        //	string path = Server.MapPath("~/Files");
        //	IFileValidator[] validators =
        //		new IFileValidator[]{
        //			new FileRequired(),
        //			new ImageValidator(),
        //			new FileSizeValidator(1024),
        //		};
        //	try
        //	{
        //		fileName = UploadFileHelper
        //			.Save(myfile, path, validators);

        //		string sourceFullPath = Path.Combine(path, fileName);


        //		string dest = System.Configuration
        //						.ConfigurationManager
        //						.AppSettings["frontSiteRootPath"];

        //		string destFullPath = Path.Combine(dest, fileName);

        //		System.IO.File.Copy(sourceFullPath, destFullPath, true);


        //	}
        //	catch (Exception ex)
        //	{
        //		ModelState.AddModelError("myfile", ex.Message);

        //		return View();
        //	}


        //	//將檔名存入photo
        //	photo = fileName;


        //	//todo將紀錄存
        //	if (!ModelState.IsValid)
        //	{
        //		return View();
        //	}
        //	try
        //	{
        //		var currentUserAccount = User.Identity.Name;
        //		EditPhoto(photo, currentUserAccount);
        //		return View(photo);

        //	}
        //	catch (Exception ex)
        //	{
        //		ModelState.AddModelError("", ex.Message);
        //	}


        //	return RedirectToAction("Index");



        //}

        //private void EditPhoto(int memberId, string photo, string account)
        //{



        //	var db = new AppDbContext();
        //	var memberInDb = db.Members.FirstOrDefault(p => p.Id == memberId);
        //	if (memberInDb.Account != account)
        //	{
        //		throw new Exception("您沒有權限修改別人資料");
        //	}

        //	memberInDb.Photo = photo;
        //	db.SaveChanges();
        //}


    }




}