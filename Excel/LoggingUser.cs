using ServiceReference1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wiring
{
    public class LoggingUser
    {
        public static void getUsers()
        {
            using (ServiceReference1.FCSoapClient fc = new ServiceReference1.FCSoapClient(FCSoapClient.EndpointConfiguration.FCSoap))
            {
                var res = fc.GetFacilityCommander_Data("kwi_acpbackup");
                Data.ListOfUsers = res.Nodes.Last().Descendants("Table1");

            }
        }

        public static void findUser(string encodedNumber)
        {
            string cleanedEncodedNumber = encodedNumber.TrimStart('0');

            if (Data.ListOfUsers == null)
                getUsers();

            var user = Data.ListOfUsers
                        .Where(x => ((string)x.Element("EncodedNumber") ?? "").TrimStart('0') == cleanedEncodedNumber)
                        .Select(x => new
                        {
                            LastName = (string)x.Element("LastName"),
                            FirstName = (string)x.Element("FirstName"),
                            EncodedNumber = ((string)x.Element("EncodedNumber") ?? "").TrimStart('0'), // Usuwanie początkowych zer
                            EmployeeNumber = (string)x.Element("EmployeeNumber")
                        })
                        .FirstOrDefault();


            if (user != null)
            {
                Data.LoggedPerson = $"{user.FirstName} {user.LastName} {user.EmployeeNumber}";
                Data.LoggedPersonBT = $"{user.EmployeeNumber}";

            }
            else
            {
                Data.LoggedPerson = null;
                Data.LoggedPersonBT = null;
            }
        }

    }
}
