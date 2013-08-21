using System;
using MTGO.Common.Helpers;

namespace MTGO.BuildSchema
{
    class Program
    {
        static void Main(string[] args)
        {
            ConnectionHelper.BuildSessionFactory(true);
            Console.WriteLine("Finished! (For Styles)");
            Console.ReadKey();
        }
    }
}
