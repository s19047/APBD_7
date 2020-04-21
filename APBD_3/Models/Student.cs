using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace APBD_3.Models
{
    public class Student
    {
       
        public String FirstName { get; set; }
        public String LastName { get; set; }
        public String Studies { get; set; }
        public int Semester { get; set; }

        //Question , how can i change DoB to show as Date_of_Birth without changing the name of the variable... annotations?
        public DateTime DoB { get; set; }
    }
}
