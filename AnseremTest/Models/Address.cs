using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;

namespace AnseremTest.Models
{
    public class Address
    {
        [Key]
        public string Name { get; set; }

        public Address(string name)
        {
            Name = name;
        }

        public string GetDomain()
        {
            int index = Name.IndexOf('@');
            return Name[(index + 1)..];
        }

        public bool IsValid()
        {
            if (Name.Length > 254)
                throw new Exception("Адрес не может быть длиннее 254 символов");

            return Regex.IsMatch(Name, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        }
    }
}
