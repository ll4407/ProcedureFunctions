
namespace ProcedureFunction
{
	public class Procedure
	{
        public string? id { get; set; }  // The unique identifier for the document
        public string? procedureId { get; set; }  // The identifier for the procedure
        public string? description { get; set; }  // Description of the procedure
        public decimal price { get; set; }  // Price of the procedure
        public int duration { get; set; }  // Duration of the procedure in minutes
        public bool requireHygienist { get; set; }  // Whether a hygienist is required
        public bool requireDentist { get; set; }  // Whether a dentist is required
        public bool requireTechnician { get; set; }  // Whether a technician is required
    }
}

