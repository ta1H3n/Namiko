using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace Namiko
{
    public class WelcomeMessage
    {
        [Key]
        public int Id { get; set; }
        public String Message { get; set; }
    }
}
