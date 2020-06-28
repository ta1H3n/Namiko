using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Model
{
    public class Command
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Aliases { get; set; }
        public string Example { get; set; }
        public string Conditions { get; set; }

        public string ModuleName { get; set; }
        public Module Module { get; set; }
        
        [NotMapped]
        public string[] AliasesArray { get { return Aliases.Split(','); } }
        [NotMapped]
        public string[] ConditionsArray { get { return Conditions.Split(','); } }
    }
}
