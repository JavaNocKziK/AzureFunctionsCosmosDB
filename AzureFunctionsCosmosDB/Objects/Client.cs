using System;
using System.Collections.Generic;
using System.Text;

namespace AzureFunctionsCosmosDB.Objects
{
    public class Client
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Forename { get; set; }
        public string Surname { get; set; }
    }
}
