using System;

namespace EternalSolutions.Samples.B2C.Common.Contracts
{
    public class Note
    {
        public int Id { get; set; }

        public string Text { get; set; }

        public DateTime CreatedAt { get; set; }

        public string CreatedBy { get; set; }
    }
}
