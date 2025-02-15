using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Web;
using System.Web.Mvc;
using Trindl.Models;

namespace Trindl.Controllers
{
    public class HomeController : Controller
    {
        DrindlEntities db = new DrindlEntities();
        public ActionResult Index()
        {
            string dil = "de"; // Varsayılan dil


            var sorgu = db.Sabitlers.FirstOrDefault();

            ViewBag.Facebook = sorgu.Facebook;
            ViewBag.Youtube = sorgu.Youtube;
            ViewBag.Instagram = sorgu.Instagram;
            ViewBag.Link = sorgu.VideoLink;
            ViewBag.Hakkimizda = db.Hakkimizdas.ToList();
            ViewBag.Urunler = db.UrunlerViews.Where(x => x.OneCikarilsinMi == "E").OrderByDescending(x => x.UrunID).ToList();
            ViewBag.Kategoriler = db.Kategorilers.ToList();
            ViewBag.Adres = sorgu.Adres;
            ViewBag.TelefonTR = sorgu.Telefon_tr;
            ViewBag.TelefonDE = sorgu.Telefon_de;
            ViewBag.WhatsApp = sorgu.Whatsapp;

            // Çerezden dil bilgisini al ve kontrol et
            if (Request.Cookies["D"] != null && !string.IsNullOrEmpty(Request.Cookies["D"]["Dil"]))
            {
                dil = Request.Cookies["D"]["Dil"];
            }

            // Dil bilgisine göre başlık belirle
            if (dil == "tr")
            {
                ViewBag.Baslik = sorgu.Baslik_tr;
                ViewBag.DegisenYazi = sorgu.DegisenYazi_tr;
            }
            else if (dil == "en")
            {
                ViewBag.Baslik = sorgu.Baslik_en;
                ViewBag.DegisenYazi = sorgu.DegisenYazi_en;
            }
            else
            {
                ViewBag.Baslik = sorgu.Baslik_de;
                ViewBag.DegisenYazi = sorgu.DegisenYazi_de;
            }
            return View();
        }

        public ActionResult Dil(int id)
        {

            if (id == 1)
            {
                HttpCookie userCookie = new HttpCookie("D");
                userCookie.Values["Dil"] = "en";
                userCookie.Expires = DateTime.Now.AddDays(30);
                Response.Cookies.Add(userCookie);
            }
            if (id == 2)
            {
                HttpCookie userCookie = new HttpCookie("D");
                userCookie.Values["Dil"] = "de";
                userCookie.Expires = DateTime.Now.AddDays(30);
                Response.Cookies.Add(userCookie);
            }
            if (id == 3)
            {
                HttpCookie userCookie = new HttpCookie("D");
                userCookie.Values["Dil"] = "tr";
                userCookie.Expires = DateTime.Now.AddDays(30);
                Response.Cookies.Add(userCookie);
            }
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public ActionResult Message(Mesajlar mesajlar, string Cevap)
        {
            string dil = "de"; // Varsayılan dil
            if (Request.Cookies["D"] != null && !string.IsNullOrEmpty(Request.Cookies["D"]["Dil"]))
            {
                dil = Request.Cookies["D"]["Dil"];
            }

            try
            {


                if (Cevap != "40")
                {

                    // Dil bilgisine göre başlık belirle
                    if (dil == "tr")
                    {
                        TempData["Hata"] = "Güvenlik Sorusunu Yanlış Cevapladınız !";
                    }
                    else if (dil == "en")
                    {
                        TempData["Hata"] = "You answered the security question incorrectly !";

                    }
                    else
                    {
                        TempData["Hata"] = "Sie haben die Sicherheitsfrage falsch beantwortet !";

                    }
                    return RedirectToAction("Index", "Home");

                }


                mesajlar.Tarih = DateTime.Now;
                mesajlar.OkunduMu = "H";
                db.Mesajlars.Add(mesajlar);
                db.SaveChanges();
                if (dil == "tr")
                {
                    TempData["Basarili"] = "Mesajınız başarıyla iletildi !";
                }
                else if (dil == "en")
                {
                    TempData["Basarili"] = " Your message has been delivered successfully !";

                }
                else
                {
                    TempData["Basarili"] = " Ihre Nachricht wurde erfolgreich zugestellt !";
                }
                return RedirectToAction("Index", "Home");

            }
            catch (Exception)
            {
                if (dil == "tr")
                {
                    TempData["Hata"] = "Bir hata oluştu !";
                }
                else if (dil == "en")
                {
                    TempData["Hata"] = "An error occurred !";

                }
                else
                {
                    TempData["Hata"] = "Es ist ein Fehler aufgetreten !";

                }
                return RedirectToAction("Index", "Home");

            }




        }


        public ActionResult Product(int page = 1, int pageSize = 6)
        {
            // Cookie'den dil bilgisini al
            string dil = "de"; // Varsayılan dil
            if (Request.Cookies["D"] != null && !string.IsNullOrEmpty(Request.Cookies["D"]["Dil"]))
            {
                dil = Request.Cookies["D"]["Dil"];
            }

            // Kategorileri al
            var kategoriler = db.Kategorilers.ToList();

            // Ürünleri al
            var urunler = db.UrunlerViews.OrderByDescending(x => x.UrunID);

            // Sayfalandırılmış ürünleri almak
            var currentPageItems = urunler.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            var totalItems = urunler.Count();

            // ViewBag üzerinden verileri gönder
            ViewBag.Kategoriler = kategoriler;
            ViewBag.Urunler = currentPageItems;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            ViewBag.CurrentPage = page;
            ViewBag.Dil = dil; // Cookie'den alınan dil bilgisi

            return View();
        }
    }
}