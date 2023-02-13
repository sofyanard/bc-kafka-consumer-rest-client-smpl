using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace test_kafka_consumer
{
    public class Employee
    {
        public int? Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmailId { get; set; }

        public Employee(int id, string firstName, string lastName, string emailId)
        {
            this.Id = id;
            this.FirstName = firstName;
            this.LastName = lastName;
            this.EmailId = emailId;
        }

        public Employee(string firstName, string lastName, string emailId)
        {
            this.FirstName = firstName;
            this.LastName = lastName;
            this.EmailId = emailId;
        }
    }
}
