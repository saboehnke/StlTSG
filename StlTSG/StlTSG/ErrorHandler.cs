using System;
using StlTSG.Models;

namespace StlTSG
{
    public static class ErrorHandler
    {
        public static void PostError(CDARModel db, string err)
        {
            Error error = new Error()
            {
                Value = err,
                Date = DateTime.Now
            };
            db.Errors.Add(error);
            db.SaveChanges();
        }
    }
}