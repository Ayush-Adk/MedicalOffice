using System.ComponentModel.DataAnnotations;

namespace MedicalOffice.ViewModels
{
    public class AppointmentSummaryVM
    {
        public int ID { get; set; }

        [Display(Name = "Patient")]
        public string Summary
        {
            get
            {
                return First_Name
                    + (string.IsNullOrEmpty(Middle_Name) ? " " :
                        (" " + (char?)Middle_Name[0] + ". ").ToUpper())
                    + Last_Name;
            }
        }

        public string First_Name { get; set; }

        public string? Middle_Name { get; set; }

        public string Last_Name { get; set; }

        [Display(Name = "Number of Appointments")]
        public int? Number_Of_Appointments { get; set; }

        [Display(Name = "Total Extra Fees")]
        [DataType(DataType.Currency)]
        public double? Total_Extra_Fees { get; set; }

        [Display(Name = "Maximum Fee Charged")]
        [DataType(DataType.Currency)]
        public double? Maximum_Fee_Charged { get; set; }
    }

}
