﻿using MedicalOffice.Models;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace MedicalOffice.Data
{
    public static class MedicalOfficeInitializer
    {
        /// <summary>
        /// Prepares the Database and seeds data as required
        /// </summary>
        /// <param name="serviceProvider">DI Container</param>
        /// <param name="DeleteDatabase">Delete the database and start from scratch</param>
        /// <param name="UseMigrations">Use Migrations or EnsureCreated</param>
        /// <param name="SeedSampleData">Add optional sample data</param>
        public static void Initialize(IServiceProvider serviceProvider,
            bool DeleteDatabase = false, bool UseMigrations = true, bool SeedSampleData = true)
        {
            using (var context = new MedicalOfficeContext(
                serviceProvider.GetRequiredService<DbContextOptions<MedicalOfficeContext>>()))
            {
                //Refresh the database as per the parameter options
                #region Prepare the Database
                try
                {
                    //Note: .CanConnect() will return false if the database is not there!
                    if (DeleteDatabase || !context.Database.CanConnect())
                    {
                        context.Database.EnsureDeleted(); //Delete the existing version 
                        if (UseMigrations)
                        {
                            context.Database.Migrate(); //Create the Database and apply all migrations
                        }
                        else
                        {
                            context.Database.EnsureCreated(); //Create and update the database as per the Model
                        }
                        //Now create any additional database objects such as Triggers or Views
                        //--------------------------------------------------------------------
                        //Create the Triggers
                        string sqlCmd = @"
                            CREATE TRIGGER SetPatientTimestampOnUpdate
                            AFTER UPDATE ON Patients
                            BEGIN
                                UPDATE Patients
                                SET RowVersion = randomblob(8)
                                WHERE rowid = NEW.rowid;
                            END;
                        ";
                        context.Database.ExecuteSqlRaw(sqlCmd);

                        sqlCmd = @"
                            CREATE TRIGGER SetPatientTimestampOnInsert
                            AFTER INSERT ON Patients
                            BEGIN
                                UPDATE Patients
                                SET RowVersion = randomblob(8)
                                WHERE rowid = NEW.rowid;
                            END
                        ";
                        context.Database.ExecuteSqlRaw(sqlCmd);

                        sqlCmd = @"
                            Drop View IF EXISTS [AppointmentSummaries];
		                    Create View AppointmentSummaries as
                            Select p.ID, p.FirstName as First_Name, p.MiddleName as Middle_Name, 
                                p.LastName as Last_Name, Count(*) as Number_Of_Appointments, 
	                            Sum(a.extraFee) as Total_Extra_Fees, Max(a.extraFee) as Maximum_Fee_Charged
                            From Patients p Join Appointments a
                            on p.ID = a.PatientID
                            Group By p.ID, p.FirstName, p.MiddleName, p.LastName;
                        ";
                        context.Database.ExecuteSqlRaw(sqlCmd);
                    }
                    else //The database is already created
                    {
                        if (UseMigrations)
                        {
                            context.Database.Migrate(); //Apply all migrations
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.GetBaseException().Message);
                }
                #endregion

                //Seed data needed for production and during development
                #region Seed Required Data
                try
                {
                    //Add some Medical Trials
                    if (!context.MedicalTrials.Any())
                    {
                        context.MedicalTrials.AddRange(
                         new MedicalTrial
                         {
                             TrialName = "UOT - Lukemia Treatment"
                         }, new MedicalTrial
                         {
                             TrialName = "HyGIeaCare Center -  Microbiome Analysis of Constipated Versus Non-constipation Patients"
                         }, new MedicalTrial
                         {
                             TrialName = "Sunnybrook -  Trial of BNT162b2 versus mRNA-1273 COVID-19 Vaccine Boosters in Chronic Kidney Disease and Dialysis Patients With Poor Humoral Response following COVID-19 Vaccination"
                         }, new MedicalTrial
                         {
                             TrialName = "Altimmune -  Evaluate the Effect of Position and Duration on the Safety and Immunogenicity of Intranasal AdCOVID Administration"
                         }, new MedicalTrial
                         {
                             TrialName = "TUK - Hair Loss Treatment"
                         });
                        context.SaveChanges();
                    }
                    //Conditions - Needed for production and during development
                    if (!context.Conditions.Any())
                    {
                        string[] conditions = new string[] { "Asthma", "Cancer", "Cardiac disease", "Diabetes", "Hypertension", "Seizure disorder", "Circulation problems", "Bleeding disorder", "Thyroid condition", "Liver Disease", "Measles", "Mumps" };

                        foreach (string condition in conditions)
                        {
                            Condition c = new Condition
                            {
                                ConditionName = condition
                            };
                            context.Conditions.Add(c);
                        }
                        context.SaveChanges();
                    }
                    //Add to the region 'Seed Required Data'
                    //Seed Specialties for Doctors
                    string[] specialties = new string[] { "Abdominal Radiology", "Addiction Psychiatry", "Adolescent Medicine Pediatrics", "Cardiothoracic Anesthesiology", "Adult Reconstructive Orthopaedics", "Advanced Heart Failure ", "Allergy & Immunology ", "Anesthesiology ", "Biochemical Genetics", "Blood Banking ", "Cardiothoracic Radiology", "Cardiovascular Disease Internal Medicine", "Chemical Pathology", "Child & Adolescent Psychiatry", "Child Abuse Pediatrics", "Child Neurology", "Clinical & Laboratory Immunology", "Clinical Cardiac Electrophysiology", "Clinical Neurophysiology Neurology", "Colon & Rectal Surgery ", "Congenital Cardiac Surgery", "Craniofacial Surgery", "Critical Care Medicine", "Cytopathology ", "Dermatology ", "Dermatopathology ", "Family Medicine ", "Family Practice", "Female Pelvic Medicine", "Foot & Ankle Orthopaedics", "Forensic Pathology", "Forensic Psychiatry ", "Hand Surgery", "Hematology Pathology", "Oncology ", "Infectious Disease", "Internal Medicine ", "Interventional Cardiology", "Neonatal-Perinatal Medicine", "Nephrology Internal Medicine", "Neurological Surgery ", "Neurology ", "Neuromuscular Medicine", "Neuropathology Pathology", "Nuclear Medicine ", "Nuclear Radiology", "Obstetric Anesthesiology", "Obstetrics & Gynecology ", "Ophthalmic Plastic", "Ophthalmology ", "Orthopaedic Sports Medicine", "Orthopaedic Surgery ", "Otolaryngology ", "Otology", "Pediatrics ", "Plastic Surgery ", "Preventive Medicine ", "Radiation Oncology ", "Rheumatology", "Vascular & Interventional Radiology", "Vascular Surgery", "Integrated Thoracic Surgery", "Transplant Hepatology", "Urology" };
                    if (!context.Specialties.Any())
                    {
                        foreach (string s in specialties)
                        {
                            Specialty sp = new Specialty
                            {
                                SpecialtyName = s
                            };
                            context.Specialties.Add(sp);
                        }
                        context.SaveChanges();
                    }
                    //Put in the "Seed Required Data" region
                    //Add Appointment Reasons
                    string[] AppointmentReasons = new string[] { "Illness", "Accident", "Mental State", "Annual Checkup", "COVID-19", "Work Injury" };
                    if (!context.AppointmentReasons.Any())
                    {
                        foreach (string s in AppointmentReasons)
                        {
                            AppointmentReason ar = new AppointmentReason
                            {
                                ReasonName = s
                            };
                            context.AppointmentReasons.Add(ar);
                        }
                        context.SaveChanges();
                    }


                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.GetBaseException().Message);
                }
                #endregion

                //Seed meaningless data as sample data during development
                #region Seed Sample Data 
                if (SeedSampleData)
                {
                    //To randomly generate data
                    Random random = new Random();

                    //Seed a few specific Doctors and Patients. We will add more with random values later,
                    //but it can be useful to know we will have some specific records in the sample data.
                    try
                    {
                        // Seed Doctors first since we can't have Patients without Doctors.
                        if (!context.Doctors.Any())
                        {
                            context.Doctors.AddRange(
                            new Doctor
                            {
                                FirstName = "Gregory",
                                MiddleName = "A",
                                LastName = "House"
                            },
                            new Doctor
                            {
                                FirstName = "Doogie",
                                MiddleName = "R",
                                LastName = "Houser"
                            },
                            new Doctor
                            {
                                FirstName = "Charles",
                                LastName = "Xavier"
                            });
                            context.SaveChanges();
                        }
                        // Seed Patients if there aren't any.
                        if (!context.Patients.Any())
                        {
                            context.Patients.AddRange(
                            new Patient
                            {
                                FirstName = "Fred",
                                MiddleName = "Reginald",
                                LastName = "Flintstone",
                                OHIP = "1231231234",
                                DOB = DateTime.Parse("1955-09-01"),
                                ExpYrVisits = 6,
                                Phone = "9055551212",
                                EMail = "fflintstone@outlook.com",
                                DoctorID = context.Doctors.FirstOrDefault(static d => d.FirstName == "Gregory" && d.LastName == "House").ID
                            },
                            new Patient
                            {
                                FirstName = "Wilma",
                                MiddleName = "Jane",
                                LastName = "Flintstone",
                                OHIP = "1321321324",
                                DOB = DateTime.Parse("1964-04-23"),
                                ExpYrVisits = 2,
                                Phone = "9055551212",
                                EMail = "wflintstone@outlook.com",
                                DoctorID = context.Doctors.FirstOrDefault(d => d.FirstName == "Gregory" && d.LastName == "House").ID
                            },
                            new Patient
                            {
                                FirstName = "Barney",
                                LastName = "Rubble",
                                OHIP = "3213213214",
                                DOB = DateTime.Parse("1964-02-22"),
                                ExpYrVisits = 2,
                                Phone = "9055551213",
                                EMail = "brubble@outlook.com",
                                Coverage = Coverage.OHIP,
                                DoctorID = context.Doctors.FirstOrDefault(d => d.FirstName == "Doogie" && d.LastName == "Houser").ID
                            },
                            new Patient //Note that I removed the assignment of an OHIP since we are setting Coverage.OutOfProvince
                            {
                                FirstName = "Jane",
                                MiddleName = "Samantha",
                                LastName = "Doe",
                                ExpYrVisits = 2,
                                Phone = "9055551234",
                                EMail = "jdoe@outlook.com",
                                Coverage = Coverage.OutOfProvince,
                                DoctorID = context.Doctors.FirstOrDefault(d => d.FirstName == "Charles" && d.LastName == "Xavier").ID
                            });
                            context.SaveChanges();
                        }
                        //Now we can seed a few PatientConditions becuase we have already have a 
                        //few Conditions and Patients.  Be careful that you choose ones that exist!
                        if (!context.PatientConditions.Any())
                        {
                            context.PatientConditions.AddRange(
                                new PatientCondition
                                {
                                    ConditionID = context.Conditions.FirstOrDefault(c => c.ConditionName == "Cancer").ID,
                                    PatientID = context.Patients.FirstOrDefault(p => p.LastName == "Flintstone" && p.FirstName == "Fred").ID
                                },
                                new PatientCondition
                                {
                                    ConditionID = context.Conditions.FirstOrDefault(c => c.ConditionName == "Cardiac disease").ID,
                                    PatientID = context.Patients.FirstOrDefault(p => p.LastName == "Flintstone" && p.FirstName == "Fred").ID
                                },
                                new PatientCondition
                                {
                                    ConditionID = context.Conditions.FirstOrDefault(c => c.ConditionName == "Diabetes").ID,
                                    PatientID = context.Patients.FirstOrDefault(p => p.LastName == "Flintstone" && p.FirstName == "Wilma").ID
                                });
                            context.SaveChanges();
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.GetBaseException().Message);
                    }

                    //Leave those in place but add more using random values
                    try
                    {
                        //Add more Doctors
                        if (context.Doctors.Count() < 4)//Don't add a second time
                        {
                            string[] firstNames = new string[] { "Woodstock", "Violet", "Charlie", "Lucy", "Linus", "Franklin", "Marcie", "Schroeder" };
                            string[] lastNames = new string[] { "Hightower", "Broomspun", "Jones" };

                            //Loop through names and add more
                            foreach (string lastName in lastNames)
                            {
                                foreach (string firstname in firstNames)
                                {
                                    //Construct some details
                                    Doctor a = new Doctor()
                                    {
                                        FirstName = firstname,
                                        LastName = lastName,
                                        //Take second character of the last name and make it the middle name
                                        MiddleName = lastName[1].ToString().ToUpper(),
                                    };
                                    context.Doctors.Add(a);
                                }
                            }
                            context.SaveChanges();
                        }

                        //So we can gererate random data, create collections of the primary keys
                        int[] doctorIDs = context.Doctors.Select(a => a.ID).ToArray();
                        int doctorIDCount = doctorIDs.Length;// Why does this help efficiency?
                        int[] medicalTrialIDs = context.MedicalTrials.Select(a => a.ID).ToArray();
                        int medicalTrialIDCount = medicalTrialIDs.Length;

                        //Add more Patients.  Now it gets more interesting because we
                        //have Foreign Keys to worry about
                        //and more complicated property values to generate
                        if (context.Patients.Count() < 5)
                        {
                            string[] firstNames = new string[] { "Lyric", "Antoinette", "Kendal", "Vivian", "Ruth", "Jamison", "Emilia", "Natalee", "Yadiel", "Jakayla", "Lukas", "Moses", "Kyler", "Karla", "Chanel", "Tyler", "Camilla", "Quintin", "Braden", "Clarence" };
                            string[] lastNames = new string[] { "Watts", "Randall", "Arias", "Weber", "Stone", "Carlson", "Robles", "Frederick", "Parker", "Morris", "Soto", "Bruce", "Orozco", "Boyer", "Burns", "Cobb", "Blankenship", "Houston", "Estes", "Atkins", "Miranda", "Zuniga", "Ward", "Mayo", "Costa", "Reeves", "Anthony", "Cook", "Krueger", "Crane", "Watts", "Little", "Henderson", "Bishop" };
                            int firstNameCount = firstNames.Length;

                            // Birthdate for randomly produced Patients 
                            // We will subtract a random number of days from today
                            DateTime startDOB = DateTime.Today;// More efficiency?
                            int counter = 1; //Used to help add some Patients to Medical Trials

                            foreach (string lastName in lastNames)
                            {
                                //Choose a random HashSet of 5 (Unique) first names
                                HashSet<string> selectedFirstNames = new HashSet<string>();
                                while (selectedFirstNames.Count() < 5)
                                {
                                    selectedFirstNames.Add(firstNames[random.Next(firstNameCount)]);
                                }

                                foreach (string firstName in selectedFirstNames)
                                {
                                    //Construct some Patient details
                                    Patient patient = new Patient()
                                    {
                                        FirstName = firstName,
                                        LastName = lastName,
                                        MiddleName = lastName[1].ToString().ToUpper(),
                                        OHIP = random.Next(2, 9).ToString() + random.Next(213214131, 989898989).ToString(),
                                        EMail = (firstName.Substring(0, 2) + lastName + random.Next(11, 111).ToString() + "@outlook.com").ToLower(),
                                        Phone = random.Next(2, 10).ToString() + random.Next(213214131, 989898989).ToString(),
                                        ExpYrVisits = (byte)random.Next(2, 12),
                                        DOB = startDOB.AddDays(-random.Next(60, 34675)),
                                        DoctorID = doctorIDs[random.Next(doctorIDCount)]
                                    };
                                    if (counter % 3 == 0)//Every third Patient gets assigned to a Medical Trial
                                    {
                                        patient.MedicalTrialID = medicalTrialIDs[random.Next(medicalTrialIDCount)];
                                    }
                                    counter++;
                                    try
                                    {
                                        //Could be a duplicate OHIP or combination of DOD, First and Last Name
                                        context.Patients.Add(patient);
                                        context.SaveChanges();
                                    }
                                    catch (Exception)
                                    {
                                        //Failed so remove it and go on to the next.
                                        //If you don't remove it from the context it
                                        //will keep trying to save it each time you 
                                        //call .SaveChanges() the the save process will stop
                                        //and prevent any other records in the que from getting saved.
                                        context.Patients.Remove(patient);
                                    }
                                }
                            }
                            //Since we didn't guarantee that evey Doctor had
                            //at least one Patient assigned, let's remove Doctors
                            //without any Patients.  We could do this other ways, but it
                            //gives a chance to show how to execute 
                            //raw SQL through our DbContext.
                            string cmd = "DELETE FROM Doctors WHERE NOT EXISTS(SELECT 1 FROM Patients WHERE Doctors.Id = Patients.DoctorID)";
                            context.Database.ExecuteSqlRaw(cmd);
                        }
                        //Add to the region 'Seed Sample Data'
                        //Put this after you have added Doctors.
                        //Make sure we have the array of Doctor primary keys.
                        doctorIDs = context.Doctors.Select(a => a.ID).ToArray();
                        doctorIDCount = doctorIDs.Length;

                        //Create collection of the primary keys of the Specialties
                        int[] specialtyIDs = context.Specialties.Select(s => s.ID).ToArray();
                        int specialtyIDCount = specialtyIDs.Length;

                        //DoctorSpecialties - the Intersection
                        //Add a few specialties to each Doctor
                        if (!context.DoctorSpecialties.Any())
                        {
                            //i loops through the primary keys of the Doctors
                            //j is just a counter so we add some Specialities to a Doctor
                            //k lets us step through all Specialties so we can make sure each gets used
                            int k = 0;//Start with the first Specialty
                            foreach (int i in doctorIDs)
                            {
                                int howMany = random.Next(1, 10);//Add up to 10 specialties
                                for (int j = 1; j <= howMany; j++)
                                {
                                    k = (k >= specialtyIDCount) ? 0 : k;//Resets counter k to 0 if we have run out of Specialties
                                    DoctorSpecialty ds = new DoctorSpecialty()
                                    {
                                        DoctorID = i,
                                        SpecialtyID = specialtyIDs[k]
                                    };
                                    k++;
                                    context.DoctorSpecialties.Add(ds);
                                }
                                context.SaveChanges();
                            }
                        }
                        //Add Appointments
                        //Create 5 notes from Bacon ipsum
                        string[] baconNotes = new string[] { "Bacon ipsum dolor amet meatball corned beef kevin, alcatra kielbasa biltong drumstick strip steak spare ribs swine. Pastrami shank swine leberkas bresaola, prosciutto frankfurter porchetta ham hock short ribs short loin andouille alcatra. Andouille shank meatball pig venison shankle ground round sausage kielbasa. Chicken pig meatloaf fatback leberkas venison tri-tip burgdoggen tail chuck sausage kevin shank biltong brisket.", "Sirloin shank t-bone capicola strip steak salami, hamburger kielbasa burgdoggen jerky swine andouille rump picanha. Sirloin porchetta ribeye fatback, meatball leberkas swine pancetta beef shoulder pastrami capicola salami chicken. Bacon cow corned beef pastrami venison biltong frankfurter short ribs chicken beef. Burgdoggen shank pig, ground round brisket tail beef ribs turkey spare ribs tenderloin shankle ham rump. Doner alcatra pork chop leberkas spare ribs hamburger t-bone. Boudin filet mignon bacon andouille, shankle pork t-bone landjaeger. Rump pork loin bresaola prosciutto pancetta venison, cow flank sirloin sausage.", "Porchetta pork belly swine filet mignon jowl turducken salami boudin pastrami jerky spare ribs short ribs sausage andouille. Turducken flank ribeye boudin corned beef burgdoggen. Prosciutto pancetta sirloin rump shankle ball tip filet mignon corned beef frankfurter biltong drumstick chicken swine bacon shank. Buffalo kevin andouille porchetta short ribs cow, ham hock pork belly drumstick pastrami capicola picanha venison.", "Picanha andouille salami, porchetta beef ribs t-bone drumstick. Frankfurter tail landjaeger, shank kevin pig drumstick beef bresaola cow. Corned beef pork belly tri-tip, ham drumstick hamburger swine spare ribs short loin cupim flank tongue beef filet mignon cow. Ham hock chicken turducken doner brisket. Strip steak cow beef, kielbasa leberkas swine tongue bacon burgdoggen beef ribs pork chop tenderloin.", "Kielbasa porchetta shoulder boudin, pork strip steak brisket prosciutto t-bone tail. Doner pork loin pork ribeye, drumstick brisket biltong boudin burgdoggen t-bone frankfurter. Flank burgdoggen doner, boudin porchetta andouille landjaeger ham hock capicola pork chop bacon. Landjaeger turducken ribeye leberkas pork loin corned beef. Corned beef turducken landjaeger pig bresaola t-bone bacon andouille meatball beef ribs doner. T-bone fatback cupim chuck beef ribs shank tail strip steak bacon." };

                        //Create collections of the primary keys of the three Parents
                        int[] AppointmentReasonIDs = context.AppointmentReasons.Select(s => s.ID).ToArray();
                        int AppointmentReasonIDCount = AppointmentReasonIDs.Length;

                        int[] patientIDs = context.Patients.Select(d => d.ID).ToArray();
                        int patientIDCount = patientIDs.Length;

                        //Appointments - the Intersection
                        //Add a few appointments to each patient
                        if (!context.Appointments.Any())
                        {
                            foreach (int i in patientIDs)
                            {
                                //i loops through the primary keys of the Patients
                                //j is just a counter so we add some Appointments to a Patient
                                //k lets us step through all AppointmentReasons so we can make sure each gets used
                                int k = 0;//Start with the first AppointmentReason
                                int howMany = random.Next(1, AppointmentReasonIDCount);
                                for (int j = 1; j <= howMany; j++)
                                {
                                    k = (k >= AppointmentReasonIDCount) ? 0 : k;
                                    Appointment a = new Appointment()
                                    {
                                        PatientID = i,
                                        AppointmentReasonID = AppointmentReasonIDs[k],
                                        StartTime = DateTime.Today.AddDays(-1 * random.Next(120)),
                                        Notes = baconNotes[random.Next(5)],
                                        DoctorID = doctorIDs[random.Next(doctorIDCount)]
                                    };
                                    //StartTime will be at midnight so add hours to it so it is more reasonable
                                    a.StartTime = a.StartTime.AddHours(random.Next(8, 16));
                                    //Set a random EndTime between 10 and 120 minutes later
                                    a.EndTime = a.StartTime + new TimeSpan(0, random.Next(1, 12) * 10, 0);
                                    //Zero out some ExtraFees
                                    a.ExtraFee = a.ExtraFee * (random.Next(3));
                                    k++;
                                    try
                                    {
                                        context.Appointments.Add(a);
                                        context.SaveChanges();
                                    }
                                    catch (Exception)
                                    {
                                        context.Remove(a);
                                    }
                                }
                                
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.GetBaseException().Message);
                    }
                }

                #endregion

            }

        }
    }
}
