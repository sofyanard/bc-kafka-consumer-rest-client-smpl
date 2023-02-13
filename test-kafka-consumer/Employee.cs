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
        public int? id { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string emailId { get; set; }

        public Employee(int id, string firstName, string lastName, string emailId)
        {
            this.id = id;
            this.firstName = firstName;
            this.lastName = lastName;
            this.emailId = emailId;
        }

        public Employee(string firstName, string lastName, string emailId)
        {
            this.firstName = firstName;
            this.lastName = lastName;
            this.emailId = emailId;
        }
    }
}
