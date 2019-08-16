using System;
using System.Collections.Generic;
using System.Text;

namespace Namiko.Data
{
    public class ApiLogin
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string RefreshToken { get; set; }
    }
}
