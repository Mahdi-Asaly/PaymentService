namespace PaymentService.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class transactions
    {
        [Key]
        [Column(Order = 0)]
        [StringLength(50)]
        public string card_number { get; set; }

        [Key]
        [Column(Order = 1)]
        //[Range(1, 12, ErrorMessage = "Exp month must be between 01 and 12")]
        [StringLength(50)]
        public string expdate_month { get; set; }

        [Key]
        [Column(Order = 2)]
        //[Range(2021, 2030, ErrorMessage = "Exp year must be between 2021 and 2030")]
        [StringLength(50)]
        public string expdate_year { get; set; }

        [Key]
        [Column(Order = 3)]
        [StringLength(50)]
        public string email { get; set; }

        [Key]
        [Column(Order = 4)]
        [StringLength(50)]
        public string city { get; set; }

        [Key]
        [Column(Order = 5)]
        [StringLength(50)]
        public string phone { get; set; }

        [Key]
        [Column(Order = 6)]
        [StringLength(50)]
        public string status { get; set; }

        [Key]
        [Column(Order = 7)]
        //[Range(1, 10000, ErrorMessage = "Amount must be between 1 and 10000 Shekels")]
        public double amount { get; set; }

        [Key]
        [Column(Order = 8)]
        [StringLength(50)]
        public string transaction_cod { get; set; }

        [Key]
        [Column(Order = 9)]
        [StringLength(50)]
        public string response_cod { get; set; }

        [Key]
        [Column(Order = 10)]
        [StringLength(50)]
        public string country { get; set; }


        [Key]
        [Column(Order = 11)]
        [StringLength(50)]
        public string zip { get; set; }

        [Key]
        [Column(Order = 12)]
        [StringLength(50)]
        public string address { get; set; }

        [Key]
        [Column(Order = 13)]
        [StringLength(50)]
        //[Range(100, 999, ErrorMessage = "CVV must be between 100 and 999")]
        public string cvv { get; set; }
        [Key]
        [Column(Order = 14)]
        [StringLength(50)]
        public string card_holder_name { get; set; }

        

    }
}
