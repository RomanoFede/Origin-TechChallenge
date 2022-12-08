using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ORIGIN_ATM.Data_Access_Layer
{
    public class DALHome : Controller
    {
        public List<Cuenta_Usuario> ListarCuentas()
        {
            using(ATM_DbEntities1 db = new ATM_DbEntities1())
            {
                List<Cuenta_Usuario> UserAccountsList = db.Cuenta_Usuario.ToList();
                return UserAccountsList;
            }
        }

        public List<Operacion> ListarOperaciones()
        {
            using (ATM_DbEntities1 db = new ATM_DbEntities1())
            {
                List<Operacion> OperationsList = db.Operacion.ToList();
                return OperationsList;
            }
        }

        public void SaveChangesInDb()
        {
            using(ATM_DbEntities1 db = new ATM_DbEntities1())
            {
                db.SaveChanges();
            }
        }

        public void AddAccountToDb(Cuenta_Usuario Acc)
        {
            using(ATM_DbEntities1 db = new ATM_DbEntities1())
            {
                db.Cuenta_Usuario.Add(Acc);
            }
        }

        public void AddOperationToDb(Operacion OP)
        {
            using (ATM_DbEntities1 db = new ATM_DbEntities1())
            {
                db.Operacion.Add(OP);
            }
        }

        public void RefreshEntryState(object Entity)
        {
            using(ATM_DbEntities1 db = new ATM_DbEntities1())
            {
                db.Entry(Entity).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
            }
        }

    }
}
