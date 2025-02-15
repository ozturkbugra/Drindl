using Microsoft.Ajax.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using Trindl.Models;

namespace Trindl.Controllers
{
    public class AdminController : Controller
    {
        DrindlEntities db = new DrindlEntities();

        public void sabit()
        {
            ViewBag.Mesajlar = db.Mesajlars.Where(x => x.OkunduMu == "H").OrderByDescending(x => x.Tarih).ToList();
            ViewBag.ToplamMesaj = db.Mesajlars.Where(x => x.OkunduMu == "H").Count();
        }

        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(string Parola, string KullaniciAdi, bool? remember)
        {
            string p = Crypto.Hash(Parola, "MD5");
            var a = db.Adminlers.Where(x => x.KullaniciAdi == KullaniciAdi && x.Parola == p).FirstOrDefault();
            if (a != null)
            {
                HttpCookie userCookie = new HttpCookie("Bilgi");
                userCookie.Values["AdminID"] = a.ID.ToString();
                userCookie.Values["KullaniciAdi"] = HttpUtility.UrlEncode(a.KullaniciAdi.ToString());
                userCookie.Values["Parola"] = HttpUtility.UrlEncode(a.Parola.ToString());


                // Cookie'nin geçerlilik süresini belirleyin (örneğin 1 gün)
                if (remember != null)
                {
                    userCookie.Expires = DateTime.Now.AddMonths(1); // 1 ay
                }
                else
                {
                    userCookie.Expires = DateTime.Now.AddDays(1); // 1 gün
                }


                // Cookie'yi ekle
                Response.Cookies.Add(userCookie);
                return RedirectToAction("Index", "Admin");
            }
            else
            {
                ViewBag.Uyari = "Kullanıcı Adı, Şifre veya bina yanlış";
                return View();
            }

        }

        public ActionResult Index()
        {
            sabit();
            ViewBag.UrunSayi = db.Urunlers.Count();
            ViewBag.YetkiliSayi = db.Adminlers.Count();
            ViewBag.OkunmamisMesajSayi = db.Mesajlars.Where(x => x.OkunduMu == "H").Count();
            ViewBag.OkunmusMesajSayi = db.Mesajlars.Where(x => x.OkunduMu == "E").Count();
            ViewBag.TumMesajSayi = db.Mesajlars.Count();
            return View();
        }

        public ActionResult Logout()
        {
            if (Request.Cookies["Bilgi"] == null)
            {

                return RedirectToAction("Login", "Admin");
            }

            if (Request.Cookies["KullaniciBilgileri"] != null)
            {
                // Cookie'nin süresini geçmiş bir zamana ayarla
                HttpCookie userCookie = new HttpCookie("Bilgi");
                userCookie.Expires = DateTime.Now.AddDays(-1);
                Response.Cookies.Add(userCookie);
            }
            return RedirectToAction("Login", "Admin");

        }

        public ActionResult Password()
        {

            if (Request.Cookies["Bilgi"] == null)
            {

                return RedirectToAction("Login", "AnaSayfa");
            }
            sabit();
            return View();
        }

        [HttpPost]
        public ActionResult Password(string eskiparola, string Parola, string Parola2)
        {
            HttpCookie userCookie = Request.Cookies["Bilgi"];
            int KullaniciID = Convert.ToInt32(userCookie.Values["AdminID"]);
            var a = db.Adminlers.Where(x => x.ID == KullaniciID).FirstOrDefault();
            var eskisifre = Crypto.Hash(eskiparola, "MD5");
            if (a.Parola == eskisifre)
            {
                if (Parola == Parola2)
                {

                    a.Parola = Crypto.Hash(Parola, "MD5");
                    db.SaveChanges();
                    TempData["Basarili"] = "Şifreniz Başarıyla Güncellendi.";

                }
                else
                {
                    TempData["Hata"] = "Şifreleriniz Birbiriyle Uyuşmuyor.";

                }
            }
            else
            {
                TempData["Hata"] = "Eski Şifreniz Hatalı.";
            }
            sabit();
            return View();

        }

        public ActionResult SabitBilgiler()
        {
            sabit();
            ViewBag.s = db.Sabitlers.OrderByDescending(x => x.SabitID).FirstOrDefault();
            return View();
        }

        [HttpPost]
        public ActionResult SabitBilgiler(Sabitler sabitler, int SabitID)
        {
            try
            {
                var y = db.Sabitlers.Where(x => x.SabitID == SabitID).FirstOrDefault();

                y.Baslik_de = sabitler.Baslik_de;
                y.Baslik_tr = sabitler.Baslik_tr;
                y.Baslik_en = sabitler.Baslik_en;
                y.DegisenYazi_de = sabitler.DegisenYazi_de;
                y.DegisenYazi_tr = sabitler.DegisenYazi_tr;
                y.DegisenYazi_en = sabitler.DegisenYazi_en;
                y.Instagram = sabitler.Instagram;
                y.Facebook = sabitler.Facebook;
                y.Youtube = sabitler.Youtube;
                y.VideoLink = sabitler.VideoLink;
                y.Adres = sabitler.Adres;
                y.Telefon_tr = sabitler.Telefon_tr;
                y.Telefon_de = sabitler.Telefon_de;
                y.Whatsapp = sabitler.Whatsapp;
                db.SaveChanges();
                sabit();
                ViewBag.s = db.Sabitlers.OrderByDescending(x => x.SabitID).FirstOrDefault();

                TempData["Basarili"] = "Güncelleme İşlemi Başarılı";
                return View();
            }
            catch (Exception)
            {

                sabit();
                ViewBag.s = db.Sabitlers.OrderByDescending(x => x.SabitID).FirstOrDefault();
                TempData["Hata"] = "Bir Hata Oluştu";
                return View();
            }

        }

        public ActionResult Hakkimizda()
        {
            sabit();
            ViewBag.Hakkimizda = db.Hakkimizdas.OrderByDescending(x => x.ID).ToList();
            return View();
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult HakkimizdaEkle(HttpPostedFileBase Resim, Hakkimizda hakkimizda)
        {
            WebImage img = new WebImage(Resim.InputStream); //bu ikisi resim ekleme
            FileInfo imginfo = new FileInfo(Resim.FileName);
            string ResimAdi = Guid.NewGuid().ToString() + imginfo.Extension; //resim adlandırma
            img.Save("~/uploads/" + ResimAdi);
            hakkimizda.Resim = "/uploads/" + ResimAdi;

            db.Hakkimizdas.Add(hakkimizda);
            db.SaveChanges();
            TempData["Basarili"] = "Ekleme İşlemi Başarılı";

            return RedirectToAction("Hakkimizda","Admin");
        }

        public ActionResult HakkimizdaSil(int id)
        {
            sabit();
            var a = db.Hakkimizdas.Where(x => x.ID == id).FirstOrDefault();

            string kapakResimYolu = Server.MapPath(a.Resim);
            if (System.IO.File.Exists(kapakResimYolu))
            {
                System.IO.File.Delete(kapakResimYolu); // Kapak resmini fiziksel olarak sil
            }

            db.Hakkimizdas.Remove(a);
            db.SaveChanges();
            TempData["Basarili"] = "Silme İşlemi Başarılı";

            return RedirectToAction("Hakkimizda","Admin");
        }

        public ActionResult HakkimizdaGuncelle(int id)
        {
            sabit();
            ViewBag.h = db.Hakkimizdas.Where(x => x.ID == id).FirstOrDefault();
                       
            return View();
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult HakkimizdaGuncelle(int ID, HttpPostedFileBase Resim, Hakkimizda hakkimizda)
        {
            var sorgu = db.Hakkimizdas.Where(x => x.ID == ID).FirstOrDefault();

            if (Resim != null)
            {
                string kapakResimYolu = Server.MapPath(sorgu.Resim);
                if (System.IO.File.Exists(kapakResimYolu))
                {
                    System.IO.File.Delete(kapakResimYolu); // Kapak resmini fiziksel olarak sil
                }

                WebImage img = new WebImage(Resim.InputStream); //bu ikisi resim ekleme
                FileInfo imginfo = new FileInfo(Resim.FileName);
                string ResimAdi = Guid.NewGuid().ToString() + imginfo.Extension; //resim adlandırma
                img.Save("~/uploads/" + ResimAdi);
                sorgu.Resim = "/uploads/" + ResimAdi;

            }

            sorgu.Aciklama_en = hakkimizda.Aciklama_en;
            sorgu.Aciklama_de = hakkimizda.Aciklama_de;
            sorgu.Aciklama_tr = hakkimizda.Aciklama_tr;
            db.SaveChanges();

            TempData["Basarili"] = "Güncelleme İşlemi Başarılı";

            return RedirectToAction("Hakkimizda","Admin");
        }

        public ActionResult Kategoriler()
        {
            sabit();
            ViewBag.Kategoriler = db.Kategorilers.OrderByDescending(x => x.KategoriID).ToList();
            return View();
        }

        [HttpPost]
        public ActionResult KategoriEkle(Kategoriler kategoriler)
        {
            db.Kategorilers.Add(kategoriler);
            db.SaveChanges();
            TempData["Basarili"] = "Ekleme İşlemi Başarılı";
            return RedirectToAction("Kategoriler", "Admin");
        }

        public ActionResult KategoriSil(int id)
        {
            var a = db.Kategorilers.Where(x => x.KategoriID == id).FirstOrDefault();

            var b = db.UrunlerViews.Where(x => x.KategoriID == id).FirstOrDefault();

            if(b != null)
            {
                TempData["Hata"] = "Bu Kategoriye Ait Ürün Girdiğiniz İçin Silinemedi. İlk Ürünü Silmelisiniz !";
                return RedirectToAction("Kategoriler", "Admin");
            }
            db.Kategorilers.Remove(a);
            db.SaveChanges();
            TempData["Basarili"] = "Silme İşlemi Başarılı";
            return RedirectToAction("Kategoriler", "Admin");
        }

        public ActionResult KategoriGuncelle(int id)
        {
            sabit();
            ViewBag.k = db.Kategorilers.Where(x => x.KategoriID == id).FirstOrDefault();

            return View();
        }

        [HttpPost]
        public ActionResult KategoriGuncelle(int KategoriID, Kategoriler kategoriler)
        {
            var bul = db.Kategorilers.Where(x => x.KategoriID == KategoriID).FirstOrDefault();

            var varmi = db.Kategorilers.Where(x => (x.KategoriAdi_en == kategoriler.KategoriAdi_en ||
            x.KategoriAdi_de == kategoriler.KategoriAdi_de || x.KategoriAdi_tr == kategoriler.KategoriAdi_tr) && x.KategoriID != KategoriID)
                .FirstOrDefault();

            if(varmi != null)
            {
                TempData["Hata"] = "Bu Kategori Adı Zaten Mevcut Farklı Bir Ad Deneyiniz !";
                return RedirectToAction("Kategoriler", "Admin");
            }


            bul.KategoriAdi_de = kategoriler.KategoriAdi_de;
            bul.KategoriAdi_tr = kategoriler.KategoriAdi_tr;
            bul.KategoriAdi_en = kategoriler.KategoriAdi_en;
            db.SaveChanges();
            TempData["Basarili"] = "Güncelleme İşlemi Başarılı";

            return RedirectToAction("Kategoriler", "Admin");
        }

        public ActionResult Urunler()
        {
            sabit();
            ViewBag.Kategoriler = db.Kategorilers.OrderBy(x => x.KategoriAdi_tr).ToList();
            ViewBag.Urunler = db.UrunlerViews.OrderByDescending(x => x.UrunID).ToList();
            return View();
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult UrunEkle(HttpPostedFileBase UrunResim, Urunler urunler)
        {
            WebImage img = new WebImage(UrunResim.InputStream); //bu ikisi resim ekleme
            FileInfo imginfo = new FileInfo(UrunResim.FileName);
            string ResimAdi = Guid.NewGuid().ToString() + imginfo.Extension; //resim adlandırma
            img.Save("~/uploads/" + ResimAdi);
            urunler.UrunResim = "/uploads/" + ResimAdi;

            db.Urunlers.Add(urunler);
            db.SaveChanges();
            TempData["Basarili"] = "Ekleme İşlemi Başarılı";

            return RedirectToAction("Urunler", "Admin");
        }

        public ActionResult Urunsil(int id)
        {
            sabit();
            var a = db.Urunlers.Where(x => x.UrunID == id).FirstOrDefault();

            string kapakResimYolu = Server.MapPath(a.UrunResim);
            if (System.IO.File.Exists(kapakResimYolu))
            {
                System.IO.File.Delete(kapakResimYolu); // Kapak resmini fiziksel olarak sil
            }

            db.Urunlers.Remove(a);
            db.SaveChanges();
            TempData["Basarili"] = "Silme İşlemi Başarılı";

            return RedirectToAction("Urunler", "Admin");
        }

        public ActionResult UrunDuzenle(int id)
        {
            sabit();
            var a = db.Urunlers.Where(x => x.UrunID == id).FirstOrDefault();
            ViewBag.u = a;
            ViewBag.Kategoriler = db.Kategorilers.OrderBy(x => x.KategoriAdi_tr).ToList();

            return View();
        }

        [HttpPost]
        public ActionResult UrunDuzenle(int UrunID, Urunler urunler)
        {

            var sorgu = db.Urunlers.Where(x => x.UrunID == UrunID).FirstOrDefault();

            sorgu.KategoriID = urunler.KategoriID;
            sorgu.UrunAdi_tr = urunler.UrunAdi_tr;
            sorgu.UrunAdi_de = urunler.UrunAdi_de;
            sorgu.UrunAdi_en = urunler.UrunAdi_en;
            sorgu.UrunAciklama_tr = urunler.UrunAciklama_tr;
            sorgu.UrunAciklama_en = urunler.UrunAciklama_en;
            sorgu.UrunAciklama_de = urunler.UrunAciklama_de;
            sorgu.OneCikarilsinMi = urunler.OneCikarilsinMi;
           
            db.SaveChanges();

            TempData["Basarili"] = "Güncelleme İşlemi Başarılı";

            return RedirectToAction("Urunler", "Admin");
        }

        public ActionResult Mesajlar()
        {
            sabit();
            ViewBag.M = db.Mesajlars.OrderByDescending(x => x.Tarih).ToList();
          
            return View();
        }

        public ActionResult MesajOkundu(int id)
        {
            sabit();
            var a = db.Mesajlars.Where(x => x.ID == id).FirstOrDefault();
            a.OkunduMu = "E";
            db.SaveChanges();
            return RedirectToAction("Mesajlar","Admin");
        }

        public ActionResult MesajOkunmadi(int id)
        {
            sabit();
            var a = db.Mesajlars.Where(x => x.ID == id).FirstOrDefault();
            a.OkunduMu = "H";
            db.SaveChanges();
            return RedirectToAction("Mesajlar", "Admin");
        }

        public ActionResult MesajOku(int id)
        {
            sabit();
            var a = db.Mesajlars.Where(x => x.ID == id).FirstOrDefault();
            a.OkunduMu = "E";
            db.SaveChanges();

            ViewBag.m = a;

            return View();
        }

        public ActionResult MesajSil(int id)
        {
            sabit();
            var a = db.Mesajlars.Where(x => x.ID == id).FirstOrDefault();

            db.Mesajlars.Remove(a);
            db.SaveChanges();
            TempData["Basarili"] = "Silme İşlemi Başarılı";
            return RedirectToAction("Mesajlar","Admin");
        }

        public ActionResult Yetkililer()
        {
            sabit();
            ViewBag.Yetkililer = db.Adminlers.ToList();
            return View();
        }

        [HttpPost]
        public ActionResult YetkiliEkle(string Parola, string KullaniciAdi, Adminler adminler)
        {

            var varmi = db.Adminlers.Where(x => x.KullaniciAdi == KullaniciAdi).FirstOrDefault();

            if(varmi != null)
            {
                TempData["Hata"] = "Aynı isimde zaten yetkili var !";
                return RedirectToAction("Yetkililer", "Admin");

            }

            adminler.Parola = Crypto.Hash(Parola, "MD5");
            adminler.KullaniciAdi = KullaniciAdi;
            db.Adminlers.Add(adminler);
            db.SaveChanges();

            TempData["Başarılı"] = "Yetkili başarıyla eklendi";

            return RedirectToAction("Yetkililer","Admin");
        }

        public ActionResult YetkiliSil(int id)
        {
            sabit();
            var sayi = db.Adminlers.Count();
            if(sayi == 1)
            {
                TempData["Hata"] = "Sistemde 1 yetkiliden düşük yetkili olamaz !";
                return RedirectToAction("Yetkililer", "Admin");
            }

            int adminid = Convert.ToInt32(Request.Cookies["Bilgi"]["AdminID"]);

            

            if(adminid == id)
            {
                TempData["Hata"] = "Kendi kullanıcınızı silemezsiniz !";
                return RedirectToAction("Yetkililer", "Admin");

            }

            var a = db.Adminlers.Where(x => x.ID == id).FirstOrDefault();

            db.Adminlers.Remove(a);
            db.SaveChanges();
            TempData["Basarili"] = "Silme işlemi başarılı";
            return RedirectToAction("Yetkililer", "Admin");
                     
        }
    }
}