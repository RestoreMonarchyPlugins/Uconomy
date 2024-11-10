using System.Xml.Serialization;

namespace fr34kyn01535.Uconomy.Models
{
    public class SalaryGroup
    {
        public SalaryGroup(string groupId, decimal amount)
        {
            GroupId = groupId;
            Amount = amount;
        }

        public SalaryGroup()
        {
            
        }

        [XmlAttribute]
        public string GroupId { get; set; }
        [XmlAttribute]
        public decimal Amount { get; set; }
    }
}
