using Azure;
using Azure.Data.Tables;
using System;

namespace EchoBot1.Modelos
{
    public class PersonalDataEntity : CommonEntity
    {    
        public string Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }

   
        public PersonalDataEntity()
        {
        }

        public PersonalDataEntity( string userId, string name, string email,string conversationId)
        {
            PartitionKey = userId; // Using userId as PartitionKey to logically group user-related data
            RowKey = conversationId; 
            Name = name;
            Email = email;
            Id = userId;
        }
    }
}
