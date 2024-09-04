using Azure;
using Azure.Data.Tables;
using System;

namespace EchoBot1.Modelos
{
    public class PersonalDataEntity : CommonEntity
    {
 
        public string Name { get; set; }
        public string Email { get; set; }
   
        public PersonalDataEntity()
        {
        }

        public PersonalDataEntity(string userId, string name, string email)
        {
            PartitionKey = userId; // Using userId as PartitionKey to logically group user-related data
            RowKey = Guid.NewGuid().ToString(); // Unique identifier for each record
            Name = name;
            Email = email;
        }
    }
}
