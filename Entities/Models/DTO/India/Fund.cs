using System;
using System.Collections;
using System.Collections.Generic;
//using MySqlX.XDevAPI.Relational;

namespace Entities.Models.DTO.India
{

    public class FundCagetoryPerformance
    {
        public string Classification { get; set; }
        public string Category { get; set; }
        public int NoOfFunds { get; set; }

        public decimal Current { get; set; }

        public decimal One { get; set; }
        public decimal OneGrowth => One == 0 ? 0 : (Current - One) * 100 / One;

        public decimal Three { get; set; }
        public decimal ThreeGrowth => Three == 0 ? 0 : (Current - Three) * 100 / Three;

        public decimal Six { get; set; }
        public decimal SixGrowth => Six == 0 ? 0 : (Current - Six) * 100 / Six;

        public decimal Nine { get; set; }
        public decimal NineGrowth => Nine == 0 ? 0 : (Current - Nine) * 100 / Nine;

        public decimal Year { get; set; }
        public decimal YearGrowth => Year == 0 ? 0 : (Current - Year) * 100 / Year;
    }

    public class CategoryFundsPerformance : FundCagetoryPerformance
    {
        public int SchemaCode { get; set; }
        public string SchemaName { get; set; }
    }

    public class Portfolio
    {
        public int ID { get; set; }
        public string PortfolioName { get; set; }
        public bool IsActive { get; set; }
        public string InvestorPAN { get; set; }
        public string Description { get; set; }
    }

    public class FundFolio
    {
        public Portfolio Portfolio { get; set; }
        public List<Folio> Folios { get; set; }
        //public string FundHouse { get; set; }
        //public int PortfolioID { get; set; }
        //public string RegisteredOwner { get; set; }
        //public string Owner { get; set; }
        //public string Description { get; set; }
    }

    public class Folio
    {
        public int ID { get; set; }
        public string FolioNumber { get; set; }
        public string FundHouse { get; set; }
        public DateTime? DateOpened { get; set; }
        public string DefaultBank { get; set; }
        public string DefaultAccountNumber { get; set; }
        public string Email { get; set; }
        public long MobileNumber { get; set; }
        public bool IsActive { get; set; }
        public string Description { get; set; }

        public string Goal { get; set; }
    }

    public class FundNAV
    {
        public int SchemaCode { get; set; }
        public DateTime date { get; set; }
        public string Name { get; set; }
        public decimal NAV { get; set; }

        public string FundHouse { get; set; }
        public string FundOption { get; set; }
        public string Category { get; set; }
    }

    public class GoalReview
    {
        public int PortfolioID { get; set; }
        public string Portfolio { get; set; }

        public int GoalID { get; set; }
        public string Goal { get; set; }
        public decimal CurrentExpense { get; set; }
        public decimal TargetExpense { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public DateTime Date { get; set; }
        public decimal Investment { get; set; }
        public decimal CurrentValue { get; set; }
        public decimal Profit => CurrentValue - Investment;
        public decimal AbsoluteReturn => Profit / Investment * 100;
        public decimal XIRR { get; set; }
        public decimal SIP { get; set; }
        public int CompletedMonths => EndDate < Date ? (EndDate.Month + EndDate.Year * 12) - (StartDate.Month + StartDate.Year * 12) : (Date.Month + Date.Year * 12) - (StartDate.Month + StartDate.Year * 12) + 1;
        public decimal EstimatedInvestment => SIP * CompletedMonths;
        public decimal SIPAchieved => EstimatedInvestment == 0 ? 0 : Investment / EstimatedInvestment * 100;
        public decimal GoalCurrentAchieved => CurrentExpense == 0 ? 0 : CurrentValue / CurrentExpense * 100;
        public decimal GoalTargetAchieved => TargetExpense == 0 ? 0 : CurrentValue / TargetExpense * 100;
    }

    //public class Charges
    //{
    //    public decimal STT { get; set; }
    //    public decimal StampDuty { get; set; }
    //    public decimal TDS { get; set; }
    //}

    //public class Dividend
    //{
    //    public int PortfolioID { get; set; }
    //    public decimal DividendRate { get; set; }
    //    public string DividendType { get; set; }
    //    public string FolioNumber { get; set; }
    //    public int SchemaCode { get; set; }
    //    public DateTime TransactionDate { get; set; }
    //    public string TransactionType { get; set; }
    //    public decimal Amount { get; set; }
    //    public decimal Units { get; set; }
    //    public decimal Price { get; set; }
    //    public Charges Charges { get; set; }
    //}

    //public class Purchase
    //{
    //    public int PortfolioID { get; set; }
    //    public string FolioNumber { get; set; }
    //    public int SchemaCode { get; set; }
    //    public DateTime TransactionDate { get; set; }
    //    public string TransactionType { get; set; }
    //    public decimal Amount { get; set; }
    //    public decimal Units { get; set; }
    //    public decimal Price { get; set; }
    //    public Dividend Dividend { get; set; }
    //}

    //public class Fund_Investments
    //{
    //    public int TransactionID { get; set; }
    //    public int PortfolioID { get; set; }
    //    public string PANCard { get; set; }
    //    public int FinancialYear { get; set; }
    //    public string FolioNumber { get; set; }
    //    public int FolioID { get; set; }
    //    public string ProductCode { get; set; }
    //    public int SchemaCode { get; set; }
    //    public DateTime TransactionDate { get; set; }
    //    public string TransactionType { get; set; }
    //    public decimal Amount { get; set; }
    //    public decimal NAV { get; set; }
    //    public decimal Units { get; set; }
    //    public decimal DividendPerNAV { get; set; }
    //    public decimal Dividend { get; set; }
    //    public decimal ActualNAV { get; set; }
    //    public decimal STT { get; set; }
    //    public decimal StampDuty { get; set; }
    //    public decimal TDS { get; set; }
    //    public List<Dividend> DividendHistory { get; set; }
    //    public List<Purchase> PurchaseHistory { get; set; }
    //}
}
