using System.ComponentModel.DataAnnotations;

namespace MedicalOffice.Models
{
    public class MedicalTrial : IValidatableObject
    {
        public int ID { get; set; }

        [Display(Name = "Trial Name")]
        [Required(ErrorMessage = "You cannot leave the name of the trial blank.")]
        [StringLength(200, ErrorMessage = "Trial name cannot be more than 200 characters long.")]
        [DataType(DataType.MultilineText)]
        [DisplayFormat(NullDisplayText = "None")]
        public string TrialName { get; set; } = "";

        public virtual ICollection<Patient> Patients { get; set; } = new HashSet<Patient>();

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (TrialName.Contains("Macbeth"))//I know this is silly, but it is just for fun
            {
                yield return new ValidationResult("You should never enter the name of The Scotish Play!", new[] { "TrialName" });
            }
            if (TrialName.Length == 7)
            {
                yield return new ValidationResult("You cannot have a Trial Name with length 7.", new[] { "TrialName" });
            }
        }
    }
}
