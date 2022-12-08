using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ORIGIN_ATM.Controllers
{
    public class HomeController : Controller
    {
        ORIGIN_ATM.Data_Access_Layer.DALHome Handler = new ORIGIN_ATM.Data_Access_Layer.DALHome();
        public ActionResult Index()
        {
            return View();

        }

        public ActionResult VerificarPIN()
        {
            if (Session["IDCUENTA"] == null)
            {
                return RedirectToAction("Index", "Home");
            }
            ViewBag.Message = "Ingrese su PIN";

            return View();
        }
        public ActionResult Error1()
        {
            ViewBag.Message = "Error de Verificación";

            return View();
        }
        public ActionResult Error2()
        {
            ViewBag.Message = "Error de Extracción";

            return View();
        }
        public ActionResult Operaciones()
        {
            if (Session["IDCUENTA"] == null)
            {
                return RedirectToAction("Index", "Home");
            }
            ViewBag.Message = "Elija su operación";
            return View();
        }

        public ActionResult OperacionRealizada()
        {
            if (Session["IDCUENTA"] == null)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }
        public ActionResult Balance()
        {
            if (Session["IDCUENTA"] == null)
            {
                return RedirectToAction("Index", "Home");
            }
            ViewBag.Message = "El Balance de su cuenta";
            string ID = Session["IDCUENTA"].ToString();
            string ultimoID = ObtenerID();
            Operacion Op = new Operacion();
            string Stemp = Session["IDCUENTA"].ToString();
            Op.ID_Cuenta = int.Parse(Stemp);
            Op.Fecha = DateTime.Now;
            Op.Monto = 0;
            Handler.AddOperationToDb(Op);
            Handler.SaveChangesInDb();
            List<Cuenta_Usuario> Lista = Handler.ListarCuentas();
            foreach (Cuenta_Usuario item in Lista)
            {
                if (item.ID_Cuenta.ToString() == ID)
                {
                    item.ID_UltimaOperacion = int.Parse(ultimoID) + 1;
                    Handler.RefreshEntryState(item);
                    Handler.SaveChangesInDb();
                }
            }
            return View();
        }
        public ActionResult Extraccion()
        {
            if (Session["IDCUENTA"] == null)
            {
                return RedirectToAction("Index", "Home");
            }
            ViewBag.Message = "Hacer un retiro de su cuenta";
            return View();
        }
        public ActionResult Salir()
        {
            Session.Clear();
            Session.Abandon();
            return RedirectToAction("Index", "Home");
        }
        public string ObtenerID()
        {
            string IdValue = "";
            List<Operacion> Operator = Handler.ListarOperaciones();
            foreach (Operacion item in Operator)
            {
                IdValue = item.ID_Operacion.ToString();
            }
            return IdValue;
        }

        public ActionResult ExtraerMonto()
        {
            string ultimoID = ObtenerID();
            string Monto = Request.Form["MontoExtraido"].ToString();
            string ID = Session["IDCUENTA"].ToString();
            List<Cuenta_Usuario> Lista = Handler.ListarCuentas();
            foreach (Cuenta_Usuario item in Lista)
            {
                if (item.ID_Cuenta.ToString() == ID)
                {
                    if (int.Parse(Monto) < item.Balance)
                    {
                        Session["OldBalance"] = item.Balance.ToString();
                        item.Balance = item.Balance - int.Parse(Monto);
                        Cuenta_Usuario cuenta = item;
                        cuenta.ID_UltimaOperacion = int.Parse(ultimoID) + 1;
                        Handler.RefreshEntryState(cuenta);
                        Session["Balance"] = cuenta.Balance.ToString();
                        Operacion Operator = new Operacion();
                        Operator.ID_Cuenta = item.ID_Cuenta;
                        Operator.Fecha = DateTime.Now;
                        Operator.Monto = int.Parse(Monto);
                        Handler.AddOperationToDb(Operator);
                        Handler.SaveChangesInDb();
                        return RedirectToAction("OperacionRealizada", "Home");
                    }
                    else
                    {
                        return RedirectToAction("Error2", "Home");
                    }
                }
            }
            return View();
        }

        public ActionResult Autorizar()
        {
            string Numero = Request.Form["Numero"].ToString();
            System.Web.Helpers.Crypto.HashPassword(Numero);

            List<Cuenta_Usuario> Lista = Handler.ListarCuentas();
            foreach (Cuenta_Usuario item in Lista)
            {
                System.Web.Helpers.Crypto.HashPassword(item.Numero_Cuenta);
                if (item.Numero_Cuenta == Numero)
                {
                    if (item.Estado_Cuenta == 0)
                    {
                        Session["IDCUENTA"] = item.ID_Cuenta;
                        return RedirectToAction("VerificarPIN", "Home");
                    }
                    else
                    {
                        return RedirectToAction("Error1", "Home");
                    }

                }
            }
            return RedirectToAction("Error1", "Home");
        }
        public ActionResult ComprobarPIN()
        {
            string InvalidPINCounter;
            if (Session["InvalidPINCounter"] != null)
            {
                InvalidPINCounter = Session["InvalidPINCounter"].ToString();
            }
            else
            {
                InvalidPINCounter = 0.ToString();
            }
            string PIN = Request.Form["PIN"].ToString();
            System.Web.Helpers.Crypto.HashPassword(PIN);
            string CurrentID = Session["IDCUENTA"].ToString();
            List<Cuenta_Usuario> Lista = Handler.ListarCuentas();
            foreach (Cuenta_Usuario item in Lista)
            {
                if (CurrentID == item.ID_Cuenta.ToString())
                {
                    System.Web.Helpers.Crypto.HashPassword(item.PIN_Cuenta);
                    if (PIN == item.PIN_Cuenta)
                    {
                        Session["NumeroTarjeta"] = item.Numero_Cuenta;
                        Session["Nombre"] = item.Nombre;
                        Session["Apellido"] = item.Apellido;
                        Session["Vencimiento"] = item.Fecha_Vencimiento;
                        Session["Balance"] = item.Balance.ToString();
                        return RedirectToAction("Operaciones", "Home");
                    }
                }
            }
            int temp = int.Parse(InvalidPINCounter) + 1;
            Session["InvalidPINCounter"] = temp.ToString();
            if (temp >= 4)
            {
                foreach (Cuenta_Usuario item in Lista)
                {
                    if (CurrentID == item.ID_Cuenta.ToString())
                    {
                        Cuenta_Usuario cuenta = item;
                        cuenta.Estado_Cuenta = 1;
                        Handler.RefreshEntryState(cuenta);
                        Handler.SaveChangesInDb();
                        item.Estado_Cuenta = 1;
                        Session["InvalidPINCounter"] = 0.ToString();
                        return RedirectToAction("Error1", "Home");
                    }

                }
                InvalidPINCounter = temp.ToString();

                return RedirectToAction("VerificarPIN", "Home");
            }
            return RedirectToAction("VerificarPIN", "Home");
        }
    }
}