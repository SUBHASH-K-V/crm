namespace AcxiomCRM.DTOs
{
    public class OpportunityDto
    {
        public int OpportunityId { get; set; }
        public string OpportunityName { get; set; } = string.Empty;
        public string? CustomerName { get; set; }
        public decimal Amount { get; set; }
        public string Stage { get; set; } = string.Empty;
        public int Probability { get; set; }
        public DateTime ExpectedCloseDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal WeightedValue { get; set; }
        public string? AssignedToName { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class CreateOpportunityDto
    {
        public string OpportunityName { get; set; } = string.Empty;
        public int? CustomerId { get; set; }
        public int? LeadId { get; set; }
        public decimal Amount { get; set; }
        public string Stage { get; set; } = "Qualification";
        public int Probability { get; set; }
        public DateTime ExpectedCloseDate { get; set; }
        public string? Notes { get; set; }
    }
}

