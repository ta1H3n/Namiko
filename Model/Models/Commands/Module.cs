using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Model
{
    public class Module
    {
        [Key]
        public string Name { get; set; }
        public string Description { get; set; }

        public List<Command> Commands { get; set; }
    }
}
