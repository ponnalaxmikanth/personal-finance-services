using System;
using System.Collections.Generic;

namespace Entities.Models.DTO.Funds
{
    public class Charges
    {
        public decimal STT { get; set; }
        public decimal StampDuty { get; set; }
        public decimal TDS { get; set; }
    }

    public class SwitchInTransaction {
        public string FolioNumber { get; set; }
        public int FolioID { get; set; }
        public int PortfolioID { get; set; }
        public int SchemaCode { get; set; }
        public DateTime TransactionDate { get; set; }
        public string TransactionType { get; set; }
        public decimal Amount { get; set; }
        public decimal Units { get; set; }
        public decimal Price { get; set; }
        public Charges Charges { get; set; }
    }

    public class FundTransaction
    {
        public int TransactionID { get; set; }
        public int PortfolioID { get; set; }
        public string FolioNumber { get; set; }
        public int FolioID { get; set; }
        public string ProductCode { get; set; }
        public int SchemaCode { get; set; }
        public DateTime TransactionDate { get; set; }
        public string FundOption { get; set; }
        public decimal Amount { get; set; }
        public decimal Units { get; set; }
        public decimal InvestNAV { get; set; }
        public decimal ActualNAV { get; set; }
        public decimal RedeemNAV { get; set; }
        public decimal CurrentNAV { get; set; }

        public char IsActive { get; set; }
        public string Comments { get; set; }
        public decimal DividendPerNAV { get; set; }
        public decimal Dividend { get; set; }
        public decimal STT { get; set; }
        public decimal StampDuty { get; set; }
        public decimal TDS { get; set; }

        public string PanCard { get; set; }
        public string TransactionType { get; set; }
        
        public uint RowIndex { get; set; }
        public int FinancialYear { get; set; }

        public List<FundTransaction> History { get; set; }
        public List<FundDividend> DividendHistory { get; set; }

        public SwitchInTransaction SwitchInTransaction { get; set; }

    }

    public class FundInvestment : FundTransaction
    {
        public string Portfolio { get; set; }
        public string SchemaName { get; set; }
        public decimal NAV { get; set; }
        public decimal CurrentValue { get; set; }
        public string FundHouse { get; set; }
        public string Category { get; set; }
        public decimal XIRR { get; set; }
        public int WithholdDays { get; set; }
        public decimal Profit => CurrentValue - Amount;
        public decimal ProfitPer => (CurrentValue - Amount) / Amount * 100;
        public int FinancialYear { get; set; }

        public string Goal { get; set; }
        public int? GoalID { get; set; }
    }

    //public class Portfolio
    //{
    //    public int PortfolioID { get; set; }
    //    public string PortfolioName { get; set; }
    //    public List<Investment> Category { get; set; }
    //    public List<Investment> FinancialYear { get; set; }
    //    public List<FundInvestment> Transactions { get; set; }

    //    public Consolidated Consolidated { get; set; }
    //}

    public class Investment
    {
        public decimal Amount { get; set; }
        public decimal Profit { get; set; }
        public decimal AbsoulteReturn { get; set; }
        public decimal XIRR { get; set; }
        public int WithholdDays { get; set; }
        public string GroupKey { get; set; }
    }









    public class Consolidated
    {
        public int PortfolioID { get; set; }
        public int NoOfFundHouses { get; set; }
        public int NoOfFolios { get; set; }
        public int NoOfFunds { get; set; }
        public decimal Investment { get; set; }
        public decimal CurrentValue { get; set; }
        
        
        public decimal Xirr { get; set; }
        public int WithHoldDays { get; set; }
        public decimal CurrentMonth { get; set; }

        public decimal Profit => CurrentValue - Investment;
        public decimal AbsoulteReturn => Investment > 0 ? (Profit * 100) / Investment : 0;
    }

    public class CategoryValuation
    {
        public string Name { get; set; }
        public string TransactionType { get; set; }
        public decimal Amount { get; set; }
        public decimal CurrentValue { get; set; }
        public decimal Profit => CurrentValue - Amount;
        public decimal ProfitPer => (CurrentValue - Amount) / Amount * 100;
        public decimal XIRR { get; set; }
    }

    public class FinancialYearValuation
    {
        public int FinancialYear { get; set; }
        public string TransactionType { get; set; }

        public decimal Amount { get; set; }
        public decimal CurrentValue { get; set; }
        public decimal Profit => CurrentValue - Amount;
        public decimal XIRR { get; set; }
        public decimal ProfitPer => (CurrentValue - Amount) / Amount * 100;
    }

    public class FundSwitchInValuation
    {
        public string SchemaName { get; set; }
        public decimal CurrentInValue { get; set; }
        public decimal CurrentOutValue { get; set; }
        public decimal Profit => CurrentInValue - CurrentOutValue;
        public decimal ProfitPer => (CurrentInValue - CurrentOutValue) / CurrentOutValue * 100;
        public decimal XIRR { get; set; }
        public int WithHoldDays { get; set; }
    }

    //public class InvestmentTracker
    //{
    //    public int PortfolioID { get; set; }
    //    public DateTime TransactionDate { get; set; }
    //    public string TransaactionType { get; set; }
    //    public decimal Amount { get; set; }
    //    public decimal Charges { get; set; }
    //}

    public class FundRedeems {
        public int TransactionID { get; set; }
        public int PortfolioID { get; set; }

        public string FolioNumber { get; set; }
        public int FolioID { get; set; }
        
        public int SchemaCode { get; set; }
        public DateTime RedeemDate { get; set; }
        public decimal Profit { get; set; }
        public decimal RedeemNAV { get; set; }
        public DateTime InvestDate { get; set; }
        public decimal Amount { get; set; }
        public decimal InvestNAV { get; set; }
        public decimal Units { get; set; }
        public decimal DividendPerNAV { get; set; }
        public decimal Dividend { get; set; }
        public Charges Charges { get; set; }
        public int FinancialYear { get; set; }
        public string FundOption { get; set; }
        public string PANCard { get; set; }
        public string ProductCode { get; set; }
    }

    public class FundSwitchIn
    {
        public int TransactionID { get; set; }
        public int PortfolioID { get; set; }
        public string FolioNumber { get; set; }

        public DateTime SwitchInDate { get; set; }
        public int InSchemaCode { get; set; }
        public string SchemaName { get; set; }
        public decimal InUnits { get; set; }
        public decimal InNAV { get; set; }
        public decimal InCharges { get; set; }
        public decimal CurrentInNAV { get; set; }

        public DateTime SwitchOutDate { get; set; }
        public int OutSchemaCode { get; set; }
        public decimal OutUnits { get; set; }
        public decimal OutNAV { get; set; }
        public decimal OutCharges { get; set; }
        public decimal CurrentOutNAV { get; set; }
    }
}
