using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;

namespace AnseremTest.Models
{
    public class Domain
    {
        [Key]
        public string Name { get; set; }

        public List<Address> StopList { get; private set; }
        public List<Address> SwitchList { get; private set; }

        public Domain(string name)
        {
            Name = name;

            StopList = new();
            SwitchList = new();
        }

        public bool IsValid()
        {
            return Regex.IsMatch(Name, @"^[^@\s]+\.[^@\s]+$");
        }
    }
}
