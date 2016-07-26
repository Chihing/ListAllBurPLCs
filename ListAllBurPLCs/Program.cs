using System;
using System.Windows.Forms;
using BR.AN.PviServices;

namespace ListAllBurPLCs
{
    class Class1
    {
        // Definition of global communication objects
        static Service service;

        static int remainingEntries;

        /// <summary>
        /// Creating communication objects
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Console.WriteLine();

            service = new Service("ListAllBurPLCs");
            service.SNMP.ResponseTimeout = 500;
            service.Connected += new PviEventHandler(service_Connected);
            service.Error += Service_Error;
            service.Connect();

            // Message loop
            Application.Run();
        }

        private static void Service_Error(object sender, PviEventArgs e)
        {
            throw new NotImplementedException();
        }

        static void service_Connected(object sender, PviEventArgs e)
        {
            service.SNMP.SearchCompleted += SNMP_SearchCompleted;
            service.SNMP.Search();
        }

        private static void SNMP_SearchCompleted(object sender, ErrorEventArgs e)
        {
            service.SNMP.NetworkAdapters.SearchCompleted += NetworkAdapters_SearchCompleted;
            service.SNMP.NetworkAdapters.Search();
        }

        private static void NetworkAdapters_SearchCompleted(object sender, ErrorEventArgs e)
        {
            remainingEntries = service.SNMP.NetworkAdapters.Count;

            foreach (NetworkAdapter nwAdapt in service.SNMP.NetworkAdapters.Values)
            {
                nwAdapt.Variables.ValuesRead += Variables_ValuesRead1; ;
                nwAdapt.Variables.Read();
            }
        }

        private static void Variables_ValuesRead1(object sender, ErrorEventArgs e)
        {
            SNMPVariableCollection varCol = (SNMPVariableCollection)sender;

            string targetTypeDescription = "unknown PLC type";
            string ipAddress = "unknown IP address";

            foreach (Variable snmpVar in ((SNMPVariableCollection)sender).Values)
            {
                if (snmpVar.Name == "targetTypeDescription") targetTypeDescription = snmpVar.Value;
                if (snmpVar.Name == "ipAddress") ipAddress = snmpVar.Value;
            }

            Console.WriteLine(targetTypeDescription + "\t: " + ipAddress);

            if (--remainingEntries == 0) WaitForKeyStroke();
        }

        private static void WaitForKeyStroke()
        {
            Console.WriteLine();
            Console.WriteLine("press any key...");
            Console.ReadKey(true);
            Application.Exit();
        }
    }
}
