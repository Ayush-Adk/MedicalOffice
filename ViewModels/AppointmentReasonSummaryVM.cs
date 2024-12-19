using System.ComponentModel.DataAnnotations;

namespace MedicalOffice.ViewModels
{
    public class AppointmentReasonSummaryVM
    {
        public int? ID { get; set; }

        [Display(Name = "Appointment Reason")]
        [DisplayFormat(NullDisplayText = "No Reason Given")]
        public string? Appointment_Reason { get; set; }

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
